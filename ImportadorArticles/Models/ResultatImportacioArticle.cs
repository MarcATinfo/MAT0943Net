using System;

namespace A3ErpImportadorArticles.Models
{
    /// <summary>
    /// Representa el resultat d'importar un únic article
    /// mitjançant a3ERP ActiveX.
    /// </summary>
    public sealed class ResultatImportacioArticle
    {
        /// <summary>
        /// Indica si l'operació ha finalitzat correctament.
        /// </summary>
        public bool Correcte { get; private set; }

        /// <summary>
        /// Codi de l'article processat.
        /// </summary>
        public string CodiArticle { get; private set; }

        /// <summary>
        /// Indica si l'article s'ha creat.
        /// </summary>
        public bool Creat { get; private set; }

        /// <summary>
        /// Indica si l'article existent s'ha actualitzat.
        /// </summary>
        public bool Actualitzat { get; private set; }

        /// <summary>
        /// Missatge informatiu o descripció de l'error.
        /// </summary>
        public string Missatge { get; private set; }

        private ResultatImportacioArticle()
        {
            CodiArticle = string.Empty;
            Missatge = string.Empty;
        }

        /// <summary>
        /// Crea un resultat correcte corresponent
        /// a un article nou.
        /// </summary>
        public static ResultatImportacioArticle CrearNou(
            string codiArticle)
        {
            return new ResultatImportacioArticle
            {
                Correcte = true,
                CodiArticle = codiArticle ?? string.Empty,
                Creat = true,
                Actualitzat = false,
                Missatge = "Article creat correctament."
            };
        }

        /// <summary>
        /// Crea un resultat correcte corresponent
        /// a l'actualització d'un article existent.
        /// </summary>
        public static ResultatImportacioArticle CrearActualitzat(
            string codiArticle)
        {
            return new ResultatImportacioArticle
            {
                Correcte = true,
                CodiArticle = codiArticle ?? string.Empty,
                Creat = false,
                Actualitzat = true,
                Missatge = "Article actualitzat correctament."
            };
        }

        /// <summary>
        /// Crea un resultat incorrecte per a un article
        /// que no s'ha pogut importar.
        /// </summary>
        public static ResultatImportacioArticle CrearError(
            string codiArticle,
            string missatge)
        {
            return new ResultatImportacioArticle
            {
                Correcte = false,
                CodiArticle = codiArticle ?? string.Empty,
                Creat = false,
                Actualitzat = false,
                Missatge = string.IsNullOrWhiteSpace(missatge)
                    ? "No s'ha pogut importar l'article."
                    : missatge
            };
        }
    }
}