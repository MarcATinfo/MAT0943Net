namespace A3ErpCalculadorTarifes.Models
{
    /// <summary>
    /// Resultat calculat per a un article.
    ///
    /// Conté els imports previsualitzats i les dades
    /// necessàries per poder-los aplicar posteriorment.
    ///
    /// La creació d'aquest objecte no implica que
    /// les dades s'hagin guardat a a3ERP.
    /// </summary>
    public sealed class ResultatCalculTarifes
    {
        /// <summary>
        /// Codi visible de l'article.
        ///
        /// Exemple:
        /// "16"
        /// </summary>
        public string CodiArticle { get; set; }

        /// <summary>
        /// Codi literal de l'article tal com està
        /// emmagatzemat a dbo.ARTICULO.
        ///
        /// Exemple:
        /// "             16"
        ///
        /// Aquest valor no es mostra a la graella.
        /// S'utilitzarà posteriorment per aplicar
        /// les tarifes mitjançant a3ERP.
        /// </summary>
        public string CodiArticleBaseDades { get; set; }

        public string Descripcio { get; set; }

        public decimal Tarifa1 { get; set; }

        public decimal Tarifa2 { get; set; }

        public decimal Tarifa3 { get; set; }

        public decimal Tarifa4 { get; set; }

        /// <summary>
        /// Tarifa 5 = PRCCOSTE.
        /// </summary>
        public decimal Tarifa5 { get; set; }

        /// <summary>
        /// Tarifa 6 = PRCCOSTE + PRCSTANDARD.
        /// </summary>
        public decimal Tarifa6 { get; set; }

        public decimal DescompteGrup1 { get; set; }

        public decimal DescompteGrup2 { get; set; }

        public decimal DescompteGrup3 { get; set; }

        public decimal DescompteGrup4 { get; set; }

        /// <summary>
        /// Indica si la plantilla utilitzada
        /// genera descomptes per als grups 1–4.
        /// </summary>
        public bool GeneraDescomptes { get; set; }

        /// <summary>
        /// Indica si el càlcul de l'article
        /// ha finalitzat correctament.
        /// </summary>
        public bool Correcte { get; set; }

        public string Missatge { get; set; }

        public ResultatCalculTarifes()
        {
            CodiArticle = string.Empty;
            CodiArticleBaseDades = string.Empty;
            Descripcio = string.Empty;
            Missatge = string.Empty;
        }

        /// <summary>
        /// Crea un resultat d'error conservant
        /// la identificació de l'article.
        /// </summary>
        public static ResultatCalculTarifes CrearError(
            ArticleCalculTarifes article,
            string missatge)
        {
            return new ResultatCalculTarifes
            {
                CodiArticle =
                    article?.CodiArticle
                    ?? string.Empty,

                CodiArticleBaseDades =
                    article?.CodiArticleBaseDades
                    ?? string.Empty,

                Descripcio =
                    article?.Descripcio
                    ?? string.Empty,

                Correcte = false,

                Missatge =
                    missatge
                    ?? string.Empty
            };
        }
    }
}