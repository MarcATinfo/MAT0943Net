namespace A3ErpImportadorArticles.Models
{
    /// <summary>
    /// Representa les dades actuals d'un article
    /// recuperades de la taula ARTICULO d'a3ERP.
    ///
    /// Aquest model només s'utilitza per comparar
    /// les dades procedents del fitxer amb les dades
    /// que ja existeixen a l'empresa activa.
    /// </summary>
    public sealed class ArticleA3ErpActual
    {
        /// <summary>
        /// Codi de l'article.
        /// Correspon a ARTICULO.CODART.
        /// </summary>
        public string CodiArticle { get; set; }

        /// <summary>
        /// Descripció de l'article.
        /// Correspon a ARTICULO.DESCART.
        /// </summary>
        public string Descripcio { get; set; }

        /// <summary>
        /// Família de l'article.
        /// Correspon a ARTICULO.CAR1.
        /// </summary>
        public string Familia { get; set; }

        /// <summary>
        /// Classificació textual d'unitats.
        /// Correspon a ARTICULO.CAR2.
        /// </summary>
        public string Unitats { get; set; }

        /// <summary>
        /// Tipus d'unitat indicat pel client.
        /// Correspon a ARTICULO.PARAM1.
        /// </summary>
        public string TipusUnitat { get; set; }

        /// <summary>
        /// Codi del proveïdor habitual.
        /// Correspon a ARTICULO.CODPRO.
        /// </summary>
        public string CodiProveidor { get; set; }

        /// <summary>
        /// Referència de l'article segons el proveïdor.
        /// Correspon a ARTICULO.ARTPRO.
        /// </summary>
        public string ReferenciaProveidor { get; set; }

        /// <summary>
        /// Preu de compra de l'article.
        /// Correspon a ARTICULO.PRCCOMPRA.
        ///
        /// En la documentació del client també
        /// apareix identificat com a PVP.
        /// </summary>
        public decimal? PreuCompra { get; set; }

        /// <summary>
        /// Primer descompte de compra.
        /// Correspon a ARTICULO.DESC1.
        /// </summary>
        public decimal? Descompte1 { get; set; }

        /// <summary>
        /// Segon descompte de compra.
        /// Correspon a ARTICULO.DESC2.
        /// </summary>
        public decimal? Descompte2 { get; set; }

        /// <summary>
        /// Tercer descompte de compra.
        /// Correspon a ARTICULO.DESC3.
        /// </summary>
        public decimal? Descompte3 { get; set; }

        /// <summary>
        /// Preu de cost de l'article.
        /// Correspon a ARTICULO.PRCCOSTE.
        ///
        /// En les fórmules del client
        /// representa la variable B.
        /// </summary>
        public decimal? PreuCost { get; set; }

        /// <summary>
        /// Import de transport o preu estàndard.
        /// Correspon a ARTICULO.PRCSTANDARD.
        /// </summary>
        public decimal? PreuTransport { get; set; }

        /// <summary>
        /// Fórmula de tarifa assignada actualment.
        /// Correspon a ARTICULO.AT_FORMULA_TARIFA_ID.
        /// </summary>
        public int? IdFormula { get; set; }

        #region Compatibilitat temporal amb la plantilla anterior

        /*
         * Aquestes propietats es mantenen temporalment perquè
         * el repositori, el servei de validació i altres components
         * encara utilitzen l'estructura de la plantilla anterior.
         *
         * Les eliminarem quan tot el flux ja treballi exclusivament
         * amb els camps definitius del client.
         */

        public decimal? PreuVenda { get; set; }

        public string TipusIva { get; set; }

        public decimal? Descompte { get; set; }

        public bool? AfectaEstoc { get; set; }

        public bool? EsVenda { get; set; }

        public bool? EsCompra { get; set; }

        public bool? UsaLots { get; set; }

        public bool? UsaCaducitat { get; set; }

        public bool? UsaNumeroSerie { get; set; }

        #endregion

        public ArticleA3ErpActual()
        {
            CodiArticle = string.Empty;
            Descripcio = string.Empty;
            Familia = string.Empty;
            Unitats = string.Empty;
            TipusUnitat = string.Empty;
            CodiProveidor = string.Empty;
            ReferenciaProveidor = string.Empty;

            /*
             * Inicialitzacions temporals corresponents
             * als camps de la plantilla anterior.
             */
            TipusIva = string.Empty;
        }
    }
}
