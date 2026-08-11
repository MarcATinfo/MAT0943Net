namespace A3ErpCalculadorTarifes.Models
{
    /// <summary>
    /// Dades bàsiques d'un article necessàries
    /// per calcular i aplicar les tarifes.
    /// </summary>
    public sealed class ArticleCalculTarifes
    {
        /// <summary>
        /// Indica si l'article està seleccionat
        /// per al càlcul.
        /// </summary>
        public bool Seleccionat { get; set; }

        /// <summary>
        /// Codi visible de l'article.
        ///
        /// Exemple:
        /// "16"
        ///
        /// Aquest és el valor que es mostra
        /// a la graella i a la previsualització.
        /// </summary>
        public string CodiArticle { get; set; }

        /// <summary>
        /// Codi literal de l'article tal com està
        /// emmagatzemat a dbo.ARTICULO.
        ///
        /// Exemple:
        /// "             16"
        ///
        /// Aquest valor s'utilitzarà exclusivament
        /// per consultar o persistir dades a a3ERP.
        /// No s'ha de reconstruir amb PadLeft ni
        /// modificar amb Trim abans de gravar.
        /// </summary>
        public string CodiArticleBaseDades { get; set; }

        public string Descripcio { get; set; }

        public string Familia { get; set; }

        /// <summary>
        /// Valor de PRCCOMPRA.
        ///
        /// En algunes plantilles s'utilitza
        /// com a preu base de venda.
        /// </summary>
        public decimal PreuCompra { get; set; }

        /// <summary>
        /// Valor de PRCCOSTE.
        /// </summary>
        public decimal PreuCost { get; set; }

        /// <summary>
        /// Valor de PRCSTANDARD.
        ///
        /// En aquest projecte representa
        /// el cost de transport.
        /// </summary>
        public decimal PreuTransport { get; set; }

        public ArticleCalculTarifes()
        {
            Seleccionat = false;
            CodiArticle = string.Empty;
            CodiArticleBaseDades = string.Empty;
            Descripcio = string.Empty;
            Familia = string.Empty;
        }
    }
}