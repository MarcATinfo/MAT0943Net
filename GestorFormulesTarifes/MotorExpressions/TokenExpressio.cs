namespace A3ErpGestorFormulesTarifes.MotorExpressions
{
    /// <summary>
    /// Representa una peça individual
    /// detectada dins d'una expressió.
    /// </summary>
    public sealed class TokenExpressio
    {
        /// <summary>
        /// Tipus de token detectat.
        /// </summary>
        public TipusTokenExpressio Tipus
        {
            get;
        }

        /// <summary>
        /// Text original del token.
        /// </summary>
        public string Text
        {
            get;
        }

        /// <summary>
        /// Posició inicial dins
        /// de l'expressió original.
        /// </summary>
        public int Posicio
        {
            get;
        }

        /// <summary>
        /// Valor decimal quan el token
        /// representa un número.
        /// </summary>
        public decimal? ValorNumeric
        {
            get;
        }

        /// <summary>
        /// Inicialitza un token no numèric.
        /// </summary>
        public TokenExpressio(
            TipusTokenExpressio tipus,
            string text,
            int posicio)
            : this(
                tipus,
                text,
                posicio,
                null)
        {
        }

        /// <summary>
        /// Inicialitza un token amb
        /// un possible valor numèric.
        /// </summary>
        public TokenExpressio(
            TipusTokenExpressio tipus,
            string text,
            int posicio,
            decimal? valorNumeric)
        {
            Tipus =
                tipus;

            Text =
                text ?? string.Empty;

            Posicio =
                posicio;

            ValorNumeric =
                valorNumeric;
        }

        /// <summary>
        /// Retorna una representació informativa
        /// útil durant les proves del motor.
        /// </summary>
        public override string ToString()
        {
            return Tipus
                + ": "
                + Text
                + " ["
                + Posicio
                + "]";
        }
    }
}