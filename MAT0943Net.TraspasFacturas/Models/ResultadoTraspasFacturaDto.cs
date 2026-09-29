namespace MAT0943Net.TraspasFacturas.Models
{
    internal sealed class ResultadoTraspasFacturaDto
    {
        public bool Correcto { get; set; }

        public decimal IdDocumento { get; set; }

        public string Serie { get; set; } =
            string.Empty;

        public string NumDoc { get; set; } =
            string.Empty;

        public string Error { get; set; } =
            string.Empty;
    }
}