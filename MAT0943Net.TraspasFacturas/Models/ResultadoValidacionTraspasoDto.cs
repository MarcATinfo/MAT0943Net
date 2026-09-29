using System.Collections.Generic;

namespace MAT0943Net.TraspasFacturas.Models
{
    internal sealed class ResultadoValidacionTraspasoDto
    {
        public string EmpresaDestino { get; set; } =
            string.Empty;

        public string BaseDatosDestino { get; set; } =
            string.Empty;

        public List<IncidenciaValidacionDto> Incidencias { get; } =
            new List<IncidenciaValidacionDto>();

        public bool EsCorrecto
        {
            get
            {
                return Incidencias.Count == 0;
            }
        }
    }

    internal sealed class IncidenciaValidacionDto
    {
        public string Factura { get; set; } =
            string.Empty;

        public string Tipo { get; set; } =
            string.Empty;

        public string Codigo { get; set; } =
            string.Empty;

        public string Mensaje { get; set; } =
            string.Empty;
    }
}