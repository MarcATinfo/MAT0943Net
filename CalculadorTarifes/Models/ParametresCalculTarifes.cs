namespace A3ErpCalculadorTarifes.Models
{
    /// <summary>
    /// Valors editables utilitzats per calcular
    /// les tarifes 1, 2, 3 i 4.
    ///
    /// Segons la plantilla, aquests valors poden
    /// representar coeficients, percentatges
    /// o imports fixos.
    /// </summary>
    public sealed class ParametresCalculTarifes
    {
        public decimal ValorTarifa1 { get; set; }

        public decimal ValorTarifa2 { get; set; }

        public decimal ValorTarifa3 { get; set; }

        public decimal ValorTarifa4 { get; set; }

        /// <summary>
        /// Descompte aplicable al grup de client 1.
        /// </summary>
        public decimal DescompteGrup1 { get; set; }

        /// <summary>
        /// Descompte aplicable al grup de client 2.
        /// </summary>
        public decimal DescompteGrup2 { get; set; }

        /// <summary>
        /// Descompte aplicable al grup de client 3.
        /// </summary>
        public decimal DescompteGrup3 { get; set; }

        /// <summary>
        /// Descompte aplicable al grup de client 4.
        /// </summary>
        public decimal DescompteGrup4 { get; set; }

        /// <summary>
        /// Indica si la plantilla genera registres
        /// de descompte a DESCUENT.
        /// </summary>
        public bool GeneraDescomptes { get; set; }
    }
}