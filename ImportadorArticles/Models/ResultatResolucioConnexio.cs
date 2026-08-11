using System;

namespace A3ErpImportadorArticles.Models
{
    /// <summary>
    /// Representa el resultat de localitzar i validar
    /// una connexió amb la base de dades activa d'a3ERP.
    /// </summary>
    public sealed class ResultatResolucioConnexio
    {
        /// <summary>
        /// Indica si s'ha obtingut una connexió vàlida.
        /// </summary>
        public bool Correcte { get; private set; }

        /// <summary>
        /// Context preparat per obrir el formulari.
        /// Només estarà informat quan el resultat sigui correcte.
        /// </summary>
        public ContextImportadorA3Erp Context { get; private set; }

        /// <summary>
        /// Missatge informatiu o descripció de l'error.
        /// </summary>
        public string Missatge { get; private set; }

        /// <summary>
        /// Error retornat en provar la connexió rebuda d'a3ERP.
        /// Pot estar informat encara que el fallback hagi funcionat.
        /// </summary>
        public string ErrorConnexioOriginal { get; private set; }

        private ResultatResolucioConnexio()
        {
            Missatge = string.Empty;
            ErrorConnexioOriginal = string.Empty;
        }

        /// <summary>
        /// Crea un resultat correcte amb el context resolt.
        /// </summary>
        public static ResultatResolucioConnexio CrearCorrecte(
            ContextImportadorA3Erp context,
            string missatge,
            string errorConnexioOriginal)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            return new ResultatResolucioConnexio
            {
                Correcte = true,
                Context = context,
                Missatge = missatge ?? string.Empty,
                ErrorConnexioOriginal =
                    errorConnexioOriginal ?? string.Empty
            };
        }

        /// <summary>
        /// Crea un resultat incorrecte quan no s'ha pogut
        /// obrir ni la connexió original ni l'alternativa.
        /// </summary>
        public static ResultatResolucioConnexio CrearError(
            string missatge,
            string errorConnexioOriginal)
        {
            return new ResultatResolucioConnexio
            {
                Correcte = false,
                Context = null,
                Missatge = missatge ?? string.Empty,
                ErrorConnexioOriginal =
                    errorConnexioOriginal ?? string.Empty
            };
        }
    }
}