namespace Ja.Models
{
    /// <summary>
    /// Pojedynczy punkt danych mocy dla wykresu
    /// </summary>
    public class PowerDataPoint
    {
        public int TimeSeconds { get; set; }
        public double Power { get; set; }
        public int Zone { get; set; }
        public bool IsInterval { get; set; }
    }
}
