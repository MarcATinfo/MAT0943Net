namespace A3ErpCalculadorTarifes.Infrastructure.Logging
{
    /// <summary>
    /// Configuracio del log propi del calculador
    /// de tarifes.
    /// </summary>
    public sealed class ConfiguracioLogCalculadorTarifes
    {
        public bool ConfiguracioDisponible { get; set; }

        public bool LogActiu { get; set; }

        public string RutaLog { get; set; }

        public ConfiguracioLogCalculadorTarifes()
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
