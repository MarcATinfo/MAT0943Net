using A3ErpImportadorArticles.Infrastructure.Data;
using A3ErpImportadorArticles.Models;
using System;
using System.Collections.Generic;
using System.Data.OleDb;
using System.Linq;

namespace A3ErpImportadorArticles.Services
{
    /// <summary>
    /// Valida els articles llegits del fitxer contra
    /// la taula ARTICULO de l'empresa activa d'a3ERP.
    ///
    /// Aquesta classe només consulta i classifica les files.
    /// No crea ni modifica cap article.
    ///
    /// Només es comparen els camps de la plantilla actual
    /// del client. Els camps antics conservats als models
    /// no participen en aquesta validació.
    /// </summary>
    public sealed class ServeiValidacioArticlesA3Erp
    {
        /*
         * Evita detectar falsos canvis provocats
         * per petites diferències en camps float.
         */
        private const decimal ToleranciaDecimal = 0.0001m;

        private readonly RepositoriArticlesA3Erp repositoriArticles;

        public ServeiValidacioArticlesA3Erp()
        {
            repositoriArticles =
                new RepositoriArticlesA3Erp();
        }

        /// <summary>
        /// Consulta els articles existents i actualitza
        /// l'estat de cada fila del fitxer.
        ///
        /// Regla funcional:
        /// - camp informat: es compara amb a3ERP;
        /// - camp buit: s'ignora i no provoca actualització.
        /// </summary>
        public void Validar(
            string cadenaConnexio,
            IList<ArticleImportacio> articles)
        {
            if (string.IsNullOrWhiteSpace(cadenaConnexio))
            {
                throw new ArgumentException(
                    "No hi ha cap connexió disponible amb a3ERP.",
                    nameof(cadenaConnexio));
            }

            if (articles == null)
            {
                throw new ArgumentNullException(
                    nameof(articles));
            }

            if (articles.Count == 0)
            {
                return;
            }

            /*
             * Abans de consultar a3ERP, impedim que el mateix
             * codi aparegui diverses vegades dins del fitxer.
             */
            MarcarCodisDuplicats(articles);

            ValidarCaracteristica1(
                cadenaConnexio,
                articles);

            ValidarFormulesTarifa(
                cadenaConnexio,
                articles);

            List<string> codisValids =
                articles
                    .Where(article =>
                        article != null &&
                        article.Estat != EstatImportacio.Error &&
                        !string.IsNullOrWhiteSpace(
                            article.CodiArticle))
                    .Select(article =>
                        article.CodiArticle.Trim())
                    .Distinct(
                        StringComparer.OrdinalIgnoreCase)
                    .ToList();

            Dictionary<string, ArticleA3ErpActual> articlesExistents =
                repositoriArticles.ObtenirPerCodis(
                    cadenaConnexio,
                    codisValids);

            Dictionary<string, string> codisLiteralsA3Erp =
                ObtenirCodisLiteralsA3Erp(
                    cadenaConnexio,
                    codisValids);

            foreach (ArticleImportacio article in articles)
            {
                if (article == null)
                {
                    continue;
                }

                /*
                 * Respectem els errors detectats durant
                 * la lectura o conversió del fitxer.
                 */
                if (article.Estat == EstatImportacio.Error)
                {
                    article.Seleccionat = false;
                    continue;
                }

                string codiArticle =
                    article.CodiArticle?.Trim()
                    ?? string.Empty;

                article.CodiArticleLiteralA3Erp =
                    string.Empty;

                ArticleA3ErpActual articleActual;

                if (!articlesExistents.TryGetValue(
                    codiArticle,
                    out articleActual))
                {
                    article.Estat =
                        EstatImportacio.Nou;

                    article.Seleccionat =
                        true;

                    article.Missatge =
                        "Article nou. No existeix a a3ERP.";

                    continue;
                }

                string codiLiteralA3Erp;

                if (codisLiteralsA3Erp.TryGetValue(
                    codiArticle,
                    out codiLiteralA3Erp))
                {
                    article.CodiArticleLiteralA3Erp =
                        codiLiteralA3Erp;
                }

                List<string> campsModificats =
                    ObtenirCampsModificats(
                        article,
                        articleActual);

                if (campsModificats.Count == 0)
                {
                    article.Estat =
                        EstatImportacio.SenseCanvis;

                    /*
                     * Els articles sense canvis no s'han
                     * de seleccionar automàticament.
                     */
                    article.Seleccionat =
                        false;

                    article.Missatge =
                        "L'article ja existeix i no presenta canvis.";
                }
                else
                {
                    article.Estat =
                        EstatImportacio.Actualitzacio;

                    article.Seleccionat =
                        true;

                    article.Missatge =
                        "Actualització necessària: "
                        + string.Join(
                            ", ",
                            campsModificats)
                        + ".";
                }
            }
        }

        private static Dictionary<string, string> ObtenirCodisLiteralsA3Erp(
            string cadenaConnexio,
            IEnumerable<string> codisArticle)
        {
            List<string> codis =
                codisArticle
                    .Where(codi =>
                        !string.IsNullOrWhiteSpace(codi))
                    .Select(codi =>
                        codi.Trim())
                    .Distinct(
                        StringComparer.OrdinalIgnoreCase)
                    .ToList();

            Dictionary<string, string> resultat =
                new Dictionary<string, string>(
                    StringComparer.OrdinalIgnoreCase);

            if (codis.Count == 0)
            {
                return resultat;
            }

            string marcadors =
                string.Join(
                    ", ",
                    Enumerable.Repeat(
                        "?",
                        codis.Count));

            string sql =
                "SELECT " +
                "CODART AS CODART_LITERAL, " +
                "LTRIM(RTRIM(CODART)) AS CODART_NORMALITZAT " +
                "FROM dbo.ARTICULO " +
                "WHERE LTRIM(RTRIM(CODART)) IN (" +
                marcadors +
                ")";

            using (OleDbConnection connexio =
                new OleDbConnection(cadenaConnexio))
            using (OleDbCommand comanda =
                new OleDbCommand(
                    sql,
                    connexio))
            {
                foreach (string codi in codis)
                {
                    OleDbParameter parametre =
                        comanda.Parameters.Add(
                            "CODART",
                            OleDbType.VarChar,
                            15);

                    parametre.Value =
                        codi.Trim();
                }

                connexio.Open();

                using (OleDbDataReader lector =
                    comanda.ExecuteReader())
                {
                    if (lector == null)
                    {
                        return resultat;
                    }

                    while (lector.Read())
                    {
                        string codiNormalitzat =
                            LlegirText(
                                lector,
                                "CODART_NORMALITZAT")
                            .Trim();

                        if (string.IsNullOrWhiteSpace(
                            codiNormalitzat))
                        {
                            continue;
                        }

                        resultat[codiNormalitzat] =
                            LlegirText(
                                lector,
                                "CODART_LITERAL");
                    }
                }
            }

            return resultat;
        }

        private static string LlegirText(
            OleDbDataReader lector,
            string columna)
        {
            int ordinal =
                lector.GetOrdinal(columna);

            if (lector.IsDBNull(ordinal))
            {
                return string.Empty;
            }

            return Convert.ToString(
                lector.GetValue(ordinal))
                ?? string.Empty;
        }

        private void ValidarCaracteristica1(
            string cadenaConnexio,
            IEnumerable<ArticleImportacio> articles)
        {
            List<ArticleImportacio> articlesAmbCar1 =
                articles
                    .Where(article =>
                        article != null &&
                        article.Estat != EstatImportacio.Error &&
                        !string.IsNullOrWhiteSpace(
                            article.Familia))
                    .ToList();

            if (articlesAmbCar1.Count == 0)
            {
                return;
            }

            HashSet<string> codisCaracteristiques =
                repositoriArticles.ObtenirCodisCaracteristiques(
                    cadenaConnexio);

            foreach (ArticleImportacio article in articlesAmbCar1)
            {
                string car1 =
                    article.Familia.Trim();

                if (codisCaracteristiques.Contains(
                    car1))
                {
                    continue;
                }

                article.Estat =
                    EstatImportacio.Error;

                article.Seleccionat =
                    false;

                article.Missatge =
                    "La característica indicada a CAR1 no existeix a a3ERP: "
                    + car1
                    + ".";
            }
        }

        private void ValidarFormulesTarifa(
            string cadenaConnexio,
            IEnumerable<ArticleImportacio> articles)
        {
            List<ArticleImportacio> articlesAmbFormula =
                articles
                    .Where(article =>
                        article != null &&
                        article.Estat != EstatImportacio.Error &&
                        article.IdFormula.HasValue)
                    .ToList();

            if (articlesAmbFormula.Count == 0)
            {
                return;
            }

            HashSet<int> idsFormules =
                repositoriArticles.ObtenirIdsFormulesTarifa(
                    cadenaConnexio);

            foreach (ArticleImportacio article in articlesAmbFormula)
            {
                int idFormula =
                    article.IdFormula.Value;

                if (idsFormules.Contains(
                    idFormula))
                {
                    continue;
                }

                article.Estat =
                    EstatImportacio.Error;

                article.Seleccionat =
                    false;

                article.Missatge =
                    "La fórmula de tarifa indicada no existeix a a3ERP: "
                    + idFormula
                    + ".";
            }
        }

        /// <summary>
        /// Marca com a error totes les files que comparteixen
        /// el mateix CODART dins del fitxer.
        ///
        /// No escollim arbitràriament una de les files perquè
        /// podria provocar una importació inesperada.
        /// </summary>
        private static void MarcarCodisDuplicats(
            IEnumerable<ArticleImportacio> articles)
        {
            List<IGrouping<string, ArticleImportacio>> grupsDuplicats =
                articles
                    .Where(article =>
                        article != null &&
                        article.Estat != EstatImportacio.Error &&
                        !string.IsNullOrWhiteSpace(
                            article.CodiArticle))
                    .GroupBy(
                        article =>
                            article.CodiArticle.Trim(),
                        StringComparer.OrdinalIgnoreCase)
                    .Where(grup =>
                        grup.Count() > 1)
                    .ToList();

            foreach (
                IGrouping<string, ArticleImportacio> grup
                in grupsDuplicats)
            {
                foreach (ArticleImportacio article in grup)
                {
                    article.Estat =
                        EstatImportacio.Error;

                    article.Seleccionat =
                        false;

                    article.Missatge =
                        "El codi d'article està duplicat dins del fitxer.";
                }
            }
        }

        /// <summary>
        /// Retorna els camps informats al fitxer que tenen
        /// un valor diferent del valor actual d'a3ERP.
        ///
        /// Els camps buits del fitxer no es comparen.
        /// </summary>
        private static List<string> ObtenirCampsModificats(
            ArticleImportacio articleFitxer,
            ArticleA3ErpActual articleActual)
        {
            List<string> camps =
                new List<string>();

            if (HaCanviText(
                    articleFitxer.Descripcio,
                    articleActual.Descripcio,
                    distingirMajuscules: true))
            {
                camps.Add(
                    "descripció");
            }

            if (HaCanviText(
                    articleFitxer.Familia,
                    articleActual.Familia,
                    distingirMajuscules: false))
            {
                camps.Add(
                    "família");
            }

            if (HaCanviText(
                    articleFitxer.Unitats,
                    articleActual.Unitats,
                    distingirMajuscules: false))
            {
                camps.Add(
                    "unitats");
            }

            if (HaCanviText(
                    articleFitxer.TipusUnitat,
                    articleActual.TipusUnitat,
                    distingirMajuscules: false))
            {
                camps.Add(
                    "tipus d'unitat");
            }

            if (HaCanviText(
                    articleFitxer.CodiProveidor,
                    articleActual.CodiProveidor,
                    distingirMajuscules: false))
            {
                camps.Add(
                    "codi de proveïdor");
            }

            if (HaCanviText(
                    articleFitxer.ReferenciaProveidor,
                    articleActual.ReferenciaProveidor,
                    distingirMajuscules: false))
            {
                camps.Add(
                    "referència del proveïdor");
            }

            if (HaCanviDecimal(
                    articleFitxer.PreuCompra,
                    articleActual.PreuCompra))
            {
                camps.Add(
                    "preu de compra");
            }

            if (HaCanviDecimal(
                    articleFitxer.Descompte1,
                    articleActual.Descompte1))
            {
                camps.Add(
                    "descompte 1");
            }

            if (HaCanviDecimal(
                    articleFitxer.Descompte2,
                    articleActual.Descompte2))
            {
                camps.Add(
                    "descompte 2");
            }

            if (HaCanviDecimal(
                    articleFitxer.Descompte3,
                    articleActual.Descompte3))
            {
                camps.Add(
                    "descompte 3");
            }

            if (HaCanviDecimal(
                    articleFitxer.PreuCost,
                    articleActual.PreuCost))
            {
                camps.Add(
                    "preu de cost");
            }

            if (HaCanviDecimal(
                    articleFitxer.PreuTransport,
                    articleActual.PreuTransport))
            {
                camps.Add(
                    "transport");
            }

            if (HaCanviEnter(
                    articleFitxer.IdFormula,
                    articleActual.IdFormula))
            {
                camps.Add(
                    "fórmula de tarifa");
            }

            return camps;
        }

        /// <summary>
        /// Determina si un camp textual informat al fitxer
        /// és diferent del valor actual d'a3ERP.
        ///
        /// Una cadena buida significa que el camp no està informat
        /// i, per tant, no s'ha de modificar ni comparar.
        /// </summary>
        private static bool HaCanviText(
            string valorFitxer,
            string valorA3Erp,
            bool distingirMajuscules)
        {
            /*
             * Un camp buit al fitxer significa
             * "no modificar el valor actual".
             */
            if (string.IsNullOrWhiteSpace(valorFitxer))
            {
                return false;
            }

            string esquerra =
                valorFitxer.Trim();

            string dreta =
                (valorA3Erp ?? string.Empty).Trim();

            StringComparison comparacio =
                distingirMajuscules
                    ? StringComparison.Ordinal
                    : StringComparison.OrdinalIgnoreCase;

            return !string.Equals(
                esquerra,
                dreta,
                comparacio);
        }

        /// <summary>
        /// Determina si un camp decimal informat al fitxer
        /// és diferent del valor actual d'a3ERP.
        ///
        /// Un valor null al fitxer significa que el camp
        /// no està informat i no s'ha de modificar.
        /// </summary>
        private static bool HaCanviDecimal(
            decimal? valorFitxer,
            decimal? valorA3Erp)
        {
            /*
             * Un valor buit al fitxer significa
             * "no modificar el valor actual".
             */
            if (!valorFitxer.HasValue)
            {
                return false;
            }

            /*
             * Si el fitxer informa un valor però a3ERP
             * encara no en té cap, existeix una modificació.
             */
            if (!valorA3Erp.HasValue)
            {
                return true;
            }

            return Math.Abs(
                       valorFitxer.Value -
                       valorA3Erp.Value)
                   > ToleranciaDecimal;
        }

        private static bool HaCanviEnter(
            int? valorFitxer,
            int? valorA3Erp)
        {
            /*
             * Un valor buit al fitxer significa
             * "no modificar el valor actual".
             */
            if (!valorFitxer.HasValue)
            {
                return false;
            }

            if (!valorA3Erp.HasValue)
            {
                return true;
            }

            return valorFitxer.Value !=
                   valorA3Erp.Value;
        }
    }
}
