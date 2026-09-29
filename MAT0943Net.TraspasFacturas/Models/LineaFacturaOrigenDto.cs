namespace MAT0943Net.TraspasFacturas.Models
{
    internal sealed class LineaFacturaOrigenDto
    {
        public string CodArt { get; set; } =
            string.Empty;

        public decimal Unidades { get; set; }

        public string DescLin { get; set; } =
            string.Empty;

        public decimal PrcMoneda { get; set; }

        public decimal Desc1 { get; set; }

        public decimal Desc2 { get; set; }

        public decimal Desc3 { get; set; }

        public decimal Desc4 { get; set; }

        public string TipIva { get; set; } =
            string.Empty;
    }
}