namespace A3ErpGestorFormulesTarifes.Models
{
    /// <summary>
    /// Conté el context d'a3ERP necessari
    /// per treballar amb les fórmules de tarifes
    /// de l'empresa activa.
    ///
    /// El gestor rep aquest context des del procés
    /// que l'obre. No inicia una nova instància
    /// d'a3ERP ni resol credencials pel seu compte.
    /// </summary>
    public sealed class ContextGestorFormulesA3Erp
    {
        /// <summary>
        /// Nom o identificador de l'empresa activa
        /// rebut des d'a3ERP.
        /// </summary>
        public string EmpresaActiva { get; private set; }

        /// <summary>
        /// Nom de la base de dades de l'empresa
        /// sobre la qual es gestionaran les fórmules.
        /// </summary>
        public string BaseDadesEmpresa { get; private set; }

        /// <summary>
        /// Cadena de connexió ja resolta
        /// per l'entorn que obre el gestor.
        ///
        /// No s'ha de mostrar ni registrar als logs.
        /// </summary>
        public string CadenaConnexio { get; private set; }

        /// <summary>
        /// Indica si la connexió prové
        /// del mecanisme alternatiu previst
        /// per l'entorn d'integració.
        /// </summary>
        public bool UtilitzaConnexioAlternativa { get; private set; }

        /// <summary>
        /// Indica si el gestor disposa
        /// d'una connexió utilitzable.
        /// </summary>
        public bool ConnexioDisponible
        {
            get
            {
                return !string.IsNullOrWhiteSpace(
                    CadenaConnexio);
            }
        }

        /// <summary>
        /// Retorna una descripció segura
        /// de l'origen de la connexió.
        ///
        /// No retorna la cadena de connexió
        /// ni cap credencial.
        /// </summary>
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

        /// <summary>
        /// Crea el context del gestor.
        /// </summary>
        public ContextGestorFormulesA3Erp(
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
        /// Crea el context utilitzat quan
        /// l'executable s'obre directament
        /// des de Visual Studio, fora d'a3ERP.
        ///
        /// En aquest mode no hi ha connexió
        /// ni empresa activa.
        /// </summary>
        public static ContextGestorFormulesA3Erp
            CrearModeDesenvolupament()
        {
            return new ContextGestorFormulesA3Erp(
                empresaActiva: string.Empty,
                baseDadesEmpresa: string.Empty,
                cadenaConnexio: string.Empty,
                utilitzaConnexioAlternativa: false);
        }
    }
}