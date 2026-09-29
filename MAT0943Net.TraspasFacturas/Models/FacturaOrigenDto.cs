using System;

namespace MAT0943Net.TraspasFacturas.Models
{
    internal sealed class FacturaOrigenDto
    {
        public bool Seleccionada { get; set; }

        public decimal IdFacv { get; set; }

        public string Serie { get; set; } = string.Empty;

        public decimal NumDoc { get; set; }

        public DateTime? Fecha { get; set; }

        public string CodCli { get; set; } = string.Empty;

        public string NomCli { get; set; } = string.Empty;

        public decimal Base { get; set; }

        public decimal Iva { get; set; }

        public decimal Total { get; set; }

        public string Rectificativa { get; set; } = string.Empty;

        public decimal Borrador { get; set; }

        public string Factura
        {
            get
            {
                string numero =
                    NumDoc.ToString("0");

                return string.IsNullOrWhiteSpace(Serie)
                    ? numero
                    : Serie.Trim() + "-" + numero;
            }
        }

        public string FechaTexto
        {
            get
            {
                return Fecha.HasValue
                    ? Fecha.Value.ToString("dd/MM/yyyy")
                    : string.Empty;
            }
        }

        public string Tipo
        {
            get
            {
                return string.Equals(
                    Rectificativa?.Trim(),
                    "T",
                    StringComparison.OrdinalIgnoreCase)
                    ? "Rectificativa"
                    : "Normal";
            }
        }

        public bool Traspassada { get; set; }

        public string Estat
        {
            get
            {
                return Traspassada
                    ? "Traspassada"
                    : string.Empty;
            }
        }
    }
}
