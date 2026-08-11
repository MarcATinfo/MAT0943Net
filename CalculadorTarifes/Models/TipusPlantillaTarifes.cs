namespace A3ErpCalculadorTarifes.Models
{
    /// <summary>
    /// Plantilles disponibles per calcular les tarifes
    /// de venda d'un article.
    /// </summary>
    public enum TipusPlantillaTarifes
    {
        CostDividitCoeficients = 1,
        CostMesPercentatges = 2,
        PreuCompraAmbDescomptes = 3,
        PreuCompraDobleAmbDescomptes = 4,
        CostDividitCoeficientsAlternatius = 5,
        CostMesImportsFixos = 6,
        CostMesPercentatgesAlternatius = 7
    }
}