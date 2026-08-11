namespace A3ErpCalculadorTarifes.Models
{
    /// <summary>
    /// Informació descriptiva d'una plantilla
    /// disponible al calculador de tarifes.
    /// </summary>
    public sealed class DefinicioPlantillaTarifes
    {
        public TipusPlantillaTarifes Tipus { get; set; }

        public string Nom { get; set; }

        public string Descripcio { get; set; }

        /// <summary>
        /// Text que explica què representen els quatre
        /// valors editables de les tarifes 1–4.
        /// </summary>
        public string TipusValors { get; set; }

        /// <summary>
        /// Indica si la plantilla utilitza els quatre
        /// valors editables per calcular les tarifes.
        ///
        /// Les plantilles 3 i 4 no els necessiten,
        /// perquè utilitzen directament PRCCOMPRA.
        /// </summary>
        public bool UtilitzaValorsTarifes { get; set; }

        public bool GeneraDescomptes { get; set; }

        public DefinicioPlantillaTarifes()
        {
            Nom = string.Empty;
            Descripcio = string.Empty;
            TipusValors = string.Empty;
        }

        /// <summary>
        /// El ComboBox mostrarà directament
        /// el nom descriptiu de la plantilla.
        /// </summary>
        public override string ToString()
        {
            return Nom;
        }
    }
}