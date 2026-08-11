namespace A3ErpGestorFormulesTarifes.MotorExpressions
{
    /// <summary>
    /// Tipus de peces que poden aparèixer
    /// dins d'una expressió de tarifes.
    /// </summary>
    public enum TipusTokenExpressio
    {
        Numero = 1,
        Variable = 2,
        Suma = 3,
        Resta = 4,
        Multiplicacio = 5,
        Divisio = 6,
        ParentesiObert = 7,
        ParentesiTancat = 8,
        Final = 9
    }
}