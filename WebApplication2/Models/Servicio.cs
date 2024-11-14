namespace WebApplication2.Models
{
    public class Servicio
    {
        public int Id { get; set; }
        public string CFE { get; set; }
        public decimal CFE_Mensualidad { get; set; } // Mensualidad para CFE
        public string JAPAY { get; set; }
        public decimal JAPAY_Mensualidad { get; set; } // Mensualidad para JAPAY
        public string Telmex { get; set; }
        public decimal Telmex_Mensualidad { get; set; } // Mensualidad para Telmex
        public string Totalplay { get; set; }
        public decimal Totalplay_Mensualidad { get; set; } // Mensualidad para Totalplay
        public string Izzi { get; set; }
        public decimal Izzi_Mensualidad { get; set; } // Mensualidad para Izzi
    }
}

