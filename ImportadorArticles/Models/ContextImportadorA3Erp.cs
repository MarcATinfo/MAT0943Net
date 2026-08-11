using System;

namespace A3ErpImportadorArticles.Models
{
    /// <summary>
    /// Conté el context d'a3ERP necessari per executar l'importador.
    ///
    /// Aquest objecte es prepararà abans d'obrir el formulari,
    /// de manera que la interfície no hagi de localitzar l'empresa
    /// ni resoldre directament la connexió.
    /// </summary>
    public sealed class ContextImportadorA3Erp
    {
        /// <summary>
        /// Nom de l'empresa activa dins d'a3ERP.
        /// </summary>
        public string EmpresaActiva { get; private set; }

        /// <summary>
        /// Nom de la base de dades de l'empresa activa.
        /// </summary>
        public string BaseDadesEmpresa { get; private set; }

        /// <summary>
        /// Cadena de connexió SQL que utilitzarà l'importador.
        /// Pot provenir directament d'a3ERP o del fallback AtInfo.
        /// </summary>
        public string CadenaConnexio { get; private set; }

        /// <summary>
        /// Indica si s'ha hagut d'utilitzar la connexió alternativa AtInfo.
        /// </summary>
        public bool UtilitzaConnexioAlternativa { get; private set; }

        /// <summary>
        /// Indica si el context disposa d'una connexió preparada.
        /// </summary>
        public bool ConnexioDisponible
        {
            get
            {
                return !string.IsNullOrWhiteSpace(CadenaConnexio);
            }
        }

        /// <summary>
        /// Indica l'origen de la connexió utilitzada.
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
                    ? "AtInfo"
                    : "a3ERP";
            }
        }

        public ContextImportadorA3Erp(
            string empresaActiva,
            string baseDadesEmpresa,
            string cadenaConnexio,
            bool utilitzaConnexioAlternativa)
        {
            EmpresaActiva = empresaActiva ?? string.Empty;
            BaseDadesEmpresa = baseDadesEmpresa ?? string.Empty;
            CadenaConnexio = cadenaConnexio ?? string.Empty;
            UtilitzaConnexioAlternativa = utilitzaConnexioAlternativa;
        }

        /// <summary>
        /// Crea un context provisional per executar el projecte
        /// directament des de Visual Studio.
        ///
        /// Aquest context desapareixerà del flux principal quan
        /// l'importador s'obri des del menú d'a3ERP.
        /// </summary>
        public static ContextImportadorA3Erp CrearModeDesenvolupament()
        {
            return new ContextImportadorA3Erp(
                empresaActiva: string.Empty,
                baseDadesEmpresa: string.Empty,
                cadenaConnexio: string.Empty,
                utilitzaConnexioAlternativa: false);
        }
    }
}