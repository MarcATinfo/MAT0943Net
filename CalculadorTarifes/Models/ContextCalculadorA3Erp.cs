namespace A3ErpCalculadorTarifes.Models
{
    /// <summary>
    /// Conté el context d'a3ERP necessari per carregar
    /// els articles de l'empresa activa al calculador.
    /// </summary>
    public sealed class ContextCalculadorA3Erp
    {
        public string EmpresaActiva { get; private set; }

        public string BaseDadesEmpresa { get; private set; }

        public string CadenaConnexio { get; private set; }

        public bool UtilitzaConnexioAlternativa { get; private set; }

        public bool ConnexioDisponible
        {
            get
            {
                return !string.IsNullOrWhiteSpace(
                    CadenaConnexio);
            }
        }

        public string OrigenConnexio
        {
            get
            {
                if (!ConnexioDisponible)
                {
                    return "Sense connexió";
                }

                return UtilitzaConnexioAlternativa
                    ? "Alternativa"
                    : "a3ERP";
            }
        }

        public ContextCalculadorA3Erp(
            string empresaActiva,
            string baseDadesEmpresa,
            string cadenaConnexio,
            bool utilitzaConnexioAlternativa)
        {
            EmpresaActiva =
                empresaActiva ?? string.Empty;

            BaseDadesEmpresa =
                baseDadesEmpresa ?? string.Empty;

            CadenaConnexio =
                cadenaConnexio ?? string.Empty;

            UtilitzaConnexioAlternativa =
                utilitzaConnexioAlternativa;
        }

        /// <summary>
        /// Crea el context utilitzat quan l'executable
        /// s'obre directament, fora d'a3ERP.
        /// </summary>
        public static ContextCalculadorA3Erp
            CrearModeDesenvolupament()
        {
            return new ContextCalculadorA3Erp(
                empresaActiva: string.Empty,
                baseDadesEmpresa: string.Empty,
                cadenaConnexio: string.Empty,
                utilitzaConnexioAlternativa: false);
        }
    }
}
