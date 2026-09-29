namespace MAT0943Net.TraspasFacturas.Models
{
    internal sealed class ConfiguracioLogTraspasFacturas
    {
        public bool ConfiguracioDisponible { get; set; }

        public bool LogActiu { get; set; }

        public string RutaLog { get; set; }

        public ConfiguracioLogTraspasFacturas()
        {
            ConfiguracioDisponible = false;
            LogActiu = false;
            RutaLog = string.Empty;
        }
    }
}