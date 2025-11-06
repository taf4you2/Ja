namespace Ja.Models
{
    /// <summary>
    /// Informacje o strefach treningowych według modelu Coggan
    /// </summary>
    public static class ZoneInfo
    {
        public static string GetZoneName(int zone)
        {
            return zone switch
            {
                1 => "Z1: Recovery",
                2 => "Z2: Endurance",
                3 => "Z3: Tempo",
                4 => "Z4: Threshold",
                5 => "Z5: VO2max",
                6 => "Z6: Anaerobic",
                7 => "Z7: Neuromuscular",
                _ => "Unknown"
            };
        }

        public static string GetZoneDescription(int zone)
        {
            return zone switch
            {
                1 => "Regeneracja - < 55% FTP",
                2 => "Wytrzymałość tlenowa - 55-75% FTP",
                3 => "Tempo maratońskie - 75-90% FTP",
                4 => "Próg mleczanowy - 90-105% FTP",
                5 => "Moc tlenowa - 105-120% FTP",
                6 => "Moc beztlenowa - 120-150% FTP",
                7 => "Sprinty maksymalne - > 150% FTP",
                _ => "Nieznana strefa"
            };
        }

        public static (double min, double max) GetZoneRange(int zone)
        {
            return zone switch
            {
                1 => (0, 55),
                2 => (55, 75),
                3 => (75, 90),
                4 => (90, 105),
                5 => (105, 120),
                6 => (120, 150),
                7 => (150, double.MaxValue),
                _ => (0, 0)
            };
        }

        public static int GetMinimumDuration(int zone)
        {
            return zone switch
            {
                7 => 5,   // Neuromuscular - min 5s
                6 => 10,  // Anaerobic - min 10s
                5 => 30,  // VO2max - min 30s
                4 => 60,  // Threshold - min 60s
                3 => 120, // Tempo - min 120s
                2 => 0,   // Endurance - bez limitu
                1 => 0,   // Recovery - bez limitu
                _ => 0
            };
        }
    }
}
