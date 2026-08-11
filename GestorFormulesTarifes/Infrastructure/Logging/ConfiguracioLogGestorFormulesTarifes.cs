namespace A3ErpGestorFormulesTarifes.Infrastructure.Logging
{
    /// <summary>
    /// Configuració del log propi del gestor
    /// de fórmules de tarifes.
    /// </summary>
    public sealed class ConfiguracioLogGestorFormulesTarifes
    {
        public bool ConfiguracioDisponible { get; set; }

        public bool LogActiu { get; set; }

        public string RutaLog { get; set; }

        public ConfiguracioLogGestorFormulesTarifes()
        {
            ConfiguracioDisponible =
                false;

            LogActiu =
                false;

            RutaLog =
                string.Empty;
        }
    }
}
