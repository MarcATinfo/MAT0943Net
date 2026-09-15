namespace MAT0943Net.Infrastructure.Articles
{
    /// <summary>
    /// Càlcul matemàtic pur del preu de cost MAT0943.
    /// </summary>
    internal static class CalculadoraPrcCosteArticle
    {
        public static double Calcular(
            double prcCompra,
            double desc1,
            double desc2,
            double desc3,
            double desc4,
            double prcStandard)
        {
            return prcCompra
                * (1d - (desc1 / 100d))
                * (1d - (desc2 / 100d))
                * (1d - (desc3 / 100d))
                * (1d - (desc4 / 100d))
                + prcStandard;
        }
    }
}
