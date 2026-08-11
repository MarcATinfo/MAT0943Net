namespace A3ErpImportadorArticles.Models
{
    /// <summary>
    /// Configuració utilitzada pel sistema de log
    /// de l'importador d'articles.
    ///
    /// Temporalment, aquests valors es recuperen
    /// de la taula AT_MAT0943NET_CONFIG.
    /// </summary>
    public sealed class ConfiguracioLogImportador
    {
        public bool ConfiguracioDisponible { get; set; }


        /// <summary>
        /// Indica si s'han d'escriure noves entrades de log.
        /// </summary>
        public bool LogActiu { get; set; }

        /// <summary>
        /// Carpeta on es guarden els fitxers de log.
        /// </summary>
        public string RutaLog { get; set; }

        public ConfiguracioLogImportador()
        {
            /*
             * Si la configuració no existeix o no es pot llegir,
             * no activem el log automàticament.
             */
            ConfiguracioDisponible = false;

            LogActiu = false;

            RutaLog =
                string.Empty;
        }
    }
}
