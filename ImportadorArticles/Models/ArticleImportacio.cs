namespace A3ErpImportadorArticles.Models
{
    /// <summary>
    /// Representa una fila del fitxer d'importació d'articles.
    ///
    /// Els camps opcionals es mantenen a null quan la cel·la
    /// corresponent no està informada. En articles existents,
    /// un valor no informat no ha de modificar el valor d'a3ERP.
    /// </summary>
    public sealed class ArticleImportacio
    {
        /// <summary>
        /// Indica si l'article està seleccionat per importar.
        /// </summary>
        public bool Seleccionat { get; set; }

        /// <summary>
        /// Número de fila original dins del fitxer.
        /// </summary>
        public int NumeroFila { get; set; }

        /// <summary>
        /// Estat actual de validació o importació.
        /// </summary>
        public EstatImportacio Estat { get; set; }

        /// <summary>
        /// Codi de l'article.
        /// Correspon a ARTICULO.CODART.
        /// </summary>
        public string CodiArticle { get; set; }

        /// <summary>
        /// Valor físic de CODART recuperat de dbo.ARTICULO.
        /// Es conserva per localitzar articles existents amb ActiveX.
        /// </summary>
        public string CodiArticleLiteralA3Erp { get; set; }

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
        /// Tipus d'unitat informat pel client.
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
        /// En la documentació del client també apareix
        /// identificat amb la sigla PVP.
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
        /// En les fórmules del client és la variable B.
        /// </summary>
        public decimal? PreuCost { get; set; }

        /// <summary>
        /// Import de transport o cost estàndard.
        /// Correspon a ARTICULO.PRCSTANDARD.
        /// </summary>
        public decimal? PreuTransport { get; set; }

        /// <summary>
        /// Fórmula de tarifa assignada a l'article.
        /// Correspon a ARTICULO.AT_FORMULA_TARIFA_ID.
        ///
        /// Es llegeix des de la columna opcional IDFORMULA.
        /// Quan és null, no modifica el valor existent.
        /// </summary>
        public int? IdFormula { get; set; }

        /// <summary>
        /// Valor original llegit de la columna IDFORMULA.
        /// S'utilitza només per mostrar o registrar
        /// incidències de conversió.
        /// </summary>
        public string IdFormulaOriginal { get; set; }

        /// <summary>
        /// Missatge de validació o resultat de la importació.
        /// </summary>
        public string Missatge { get; set; }

        /// <summary>
        /// Text que es mostra a la graella segons l'estat actual.
        /// </summary>
        public string EstatText
        {
            get
            {
                switch (Estat)
                {
                    case EstatImportacio.Nou:
                        return "Nou";

                    case EstatImportacio.Actualitzacio:
                        return "Actualitzar";

                    case EstatImportacio.SenseCanvis:
                        return "Sense canvis";

                    case EstatImportacio.Error:
                        return "Error";

                    case EstatImportacio.Importat:
                        return "Importat";

                    default:
                        return "Pendent";
                }
            }
        }

        #region Compatibilitat temporal amb la plantilla anterior

        /*
         * Aquestes propietats es mantenen temporalment perquè
         * el convertidor, la validació, la graella i el servei
         * d'importació actual encara les utilitzen.
         *
         * Les eliminarem quan tots aquests components hagin
         * estat adaptats a la nova plantilla del client.
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
    }
}
