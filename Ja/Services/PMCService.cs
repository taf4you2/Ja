using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Ja.Database;
using Ja.Database.Entities;

namespace Ja.Services
{
    /// <summary>
    /// Serwis do obliczania Performance Management Chart (CTL/ATL/TSB)
    /// Implementacja zgodna ze specyfikacją JA Training
    /// </summary>
    public class PMCService
    {
        private readonly JaDbContext _context;
        private readonly int _ctlDays; // Domyślnie 42 dni
        private readonly int _atlDays; // Domyślnie 7 dni

        public PMCService(JaDbContext context, int ctlDays = 42, int atlDays = 7)
        {
            _context = context;
            _ctlDays = ctlDays;
            _atlDays = atlDays;
        }

        /// <summary>
        /// Oblicza CTL (Chronic Training Load) - Fitness
        /// Formuła: CTL_today = CTL_yesterday + (TSS_today - CTL_yesterday) / 42
        /// </summary>
        private double CalculateCTL(double previousCTL, double todayTSS)
        {
            return previousCTL + (todayTSS - previousCTL) / _ctlDays;
        }

        /// <summary>
        /// Oblicza ATL (Acute Training Load) - Fatigue
        /// Formuła: ATL_today = ATL_yesterday + (TSS_today - ATL_yesterday) / 7
        /// </summary>
        private double CalculateATL(double previousATL, double todayTSS)
        {
            return previousATL + (todayTSS - previousATL) / _atlDays;
        }

        /// <summary>
        /// Oblicza TSB (Training Stress Balance) - Form
        /// Formuła: TSB = CTL - ATL
        /// </summary>
        private double CalculateTSB(double ctl, double atl)
        {
            return ctl - atl;
        }

        /// <summary>
        /// Aktualizuje PMC dla użytkownika na podstawie wszystkich treningów
        /// </summary>
        public async Task UpdatePMCForUserAsync(int userId)
        {
            // Pobierz wszystkie treningi użytkownika posortowane chronologicznie
            var trainings = await _context.Trainings
                .Where(t => t.UserId == userId && t.TSS.HasValue)
                .OrderBy(t => t.TrainingDate)
                .Select(t => new { t.TrainingDate, t.TSS })
                .ToListAsync();

            if (!trainings.Any())
                return;

            // Usuń stare dane PMC
            var oldPmcData = await _context.PMCData
                .Where(p => p.UserId == userId)
                .ToListAsync();
            _context.PMCData.RemoveRange(oldPmcData);

            // Grupuj treningi po dacie (może być więcej niż jeden trening dziennie)
            var dailyTSS = trainings
                .GroupBy(t => t.TrainingDate.Date)
                .Select(g => new
                {
                    Date = g.Key,
                    TotalTSS = g.Sum(t => t.TSS ?? 0)
                })
                .OrderBy(d => d.Date)
                .ToList();

            // Inicjalizacja
            double ctl = 0;
            double atl = 0;
            double tsb = 0;

            var startDate = dailyTSS.First().Date;
            var endDate = DateTime.Today;

            // Oblicz PMC dla każdego dnia
            var pmcDataList = new List<PMCData>();

            for (var date = startDate; date <= endDate; date = date.AddDays(1))
            {
                // Znajdź TSS dla tego dnia (0 jeśli brak treningu)
                var dayTSS = dailyTSS.FirstOrDefault(d => d.Date == date)?.TotalTSS ?? 0;

                // Oblicz CTL, ATL, TSB
                ctl = CalculateCTL(ctl, dayTSS);
                atl = CalculateATL(atl, dayTSS);
                tsb = CalculateTSB(ctl, atl);

                // Zapisz dane
                pmcDataList.Add(new PMCData
                {
                    UserId = userId,
                    Date = date,
                    CTL = Math.Round(ctl, 2),
                    ATL = Math.Round(atl, 2),
                    TSB = Math.Round(tsb, 2),
                    CreatedAt = DateTime.UtcNow
                });
            }

            // Zapisz wszystkie dane na raz (bulk insert)
            await _context.PMCData.AddRangeAsync(pmcDataList);
            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// Pobiera dane PMC dla użytkownika w określonym zakresie dat
        /// </summary>
        public async Task<List<PMCData>> GetPMCDataAsync(int userId, DateTime startDate, DateTime endDate)
        {
            return await _context.PMCData
                .Where(p => p.UserId == userId && p.Date >= startDate && p.Date <= endDate)
                .OrderBy(p => p.Date)
                .ToListAsync();
        }

        /// <summary>
        /// Pobiera najnowsze dane PMC dla użytkownika
        /// </summary>
        public async Task<PMCData?> GetLatestPMCDataAsync(int userId)
        {
            return await _context.PMCData
                .Where(p => p.UserId == userId)
                .OrderByDescending(p => p.Date)
                .FirstOrDefaultAsync();
        }

        /// <summary>
        /// Określa stan formy na podstawie TSB
        /// </summary>
        public string GetFormStatus(double tsb)
        {
            if (tsb > 25)
                return "Świeży"; // Fresh
            else if (tsb >= -10)
                return "Optymalny"; // Optimal
            else if (tsb >= -30)
                return "Przemęczony"; // Fatigued
            else
                return "Przeciążony"; // Overreached

            // Zgodnie ze specyfikacją:
            // Świeży (Fresh): > 25
            // Optymalny (Optimal): -10 do 25
            // Przeciążony (Overreached): < -30
        }

        /// <summary>
        /// Pobiera zmianę CTL/ATL/TSB w stosunku do poprzedniego dnia
        /// </summary>
        public async Task<(double ctlChange, double atlChange, double tsbChange)?> GetPMCChangesAsync(int userId)
        {
            var latest = await _context.PMCData
                .Where(p => p.UserId == userId)
                .OrderByDescending(p => p.Date)
                .Take(2)
                .ToListAsync();

            if (latest.Count < 2)
                return null;

            var today = latest[0];
            var yesterday = latest[1];

            return (
                today.CTL - yesterday.CTL,
                today.ATL - yesterday.ATL,
                today.TSB - yesterday.TSB
            );
        }
    }
}
