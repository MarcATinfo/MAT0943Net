using A3ErpImportadorArticles.Models;
using System;
using System.Collections.Generic;
using System.Data.OleDb;
using System.Globalization;
using System.Linq;

namespace A3ErpImportadorArticles.Infrastructure.Data
{
    /// <summary>
    /// Consulta els articles existents a la base de dades
    /// activa d'a3ERP.
    ///
    /// Aquesta classe només executa operacions de lectura.
    /// No crea, actualitza ni elimina articles.
    ///
    /// Només recupera els camps de la plantilla actual
    /// del client. Els camps antics conservats als models
    /// no es consulten ni participen en el procés.
    /// </summary>
    public sealed class RepositoriArticlesA3Erp
    {
        /*
         * Evitem construir consultes excessivament grans
         * quan el fitxer contingui molts articles.
         */
        private const int MidaLotConsulta = 500;

        /// <summary>
        /// Recupera les dades actuals dels codis sol·licitats.
        ///
        /// El diccionari utilitza una comparació que no diferencia
        /// majúscules i minúscules, però conserva el codi original.
        /// </summary>
        public Dictionary<string, ArticleA3ErpActual> ObtenirPerCodis(
            string cadenaConnexio,
            IEnumerable<string> codisArticle)
        {
            if (string.IsNullOrWhiteSpace(cadenaConnexio))
            {
                throw new ArgumentException(
                    "La cadena de connexió amb a3ERP és buida.",
                    nameof(cadenaConnexio));
            }

            List<string> codisNormalitzats =
                PrepararCodis(codisArticle);

            Dictionary<string, ArticleA3ErpActual> resultat =
                new Dictionary<string, ArticleA3ErpActual>(
                    StringComparer.OrdinalIgnoreCase);

            if (codisNormalitzats.Count == 0)
            {
                return resultat;
            }

            using (OleDbConnection connexio =
                new OleDbConnection(cadenaConnexio))
            {
                connexio.Open();

                for (
                    int inici = 0;
                    inici < codisNormalitzats.Count;
                    inici += MidaLotConsulta)
                {
                    List<string> lot =
                        codisNormalitzats
                            .Skip(inici)
                            .Take(MidaLotConsulta)
                            .ToList();

                    LlegirLot(
                        connexio,
                        lot,
                        resultat);
                }
            }

            return resultat;
        }

        public HashSet<string> ObtenirCodisCaracteristiques(
            string cadenaConnexio)
        {
            if (string.IsNullOrWhiteSpace(cadenaConnexio))
            {
                throw new ArgumentException(
                    "La cadena de connexió amb a3ERP és buida.",
                    nameof(cadenaConnexio));
            }

            HashSet<string> codis =
                new HashSet<string>(
                    StringComparer.OrdinalIgnoreCase);

            const string sql =
                "SELECT LTRIM(RTRIM(CODCAR)) AS CODCAR " +
                "FROM dbo.CARACTERISTICAS " +
                "WHERE CODCAR IS NOT NULL";

            using (OleDbConnection connexio =
                new OleDbConnection(cadenaConnexio))
            using (OleDbCommand comanda =
                new OleDbCommand(
                    sql,
                    connexio))
            {
                connexio.Open();

                using (OleDbDataReader lector =
                    comanda.ExecuteReader())
                {
                    if (lector == null)
                    {
                        return codis;
                    }

                    while (lector.Read())
                    {
                        string codi =
                            LlegirText(
                                lector,
                                "CODCAR")
                            .Trim();

                        if (!string.IsNullOrWhiteSpace(
                            codi))
                        {
                            codis.Add(
                                codi);
                        }
                    }
                }
            }

            return codis;
        }

        /// <summary>
        /// Elimina codis buits i duplicats sense modificar
        /// els zeros inicials ni els codis alfanumèrics.
        /// </summary>
        private static List<string> PrepararCodis(
            IEnumerable<string> codisArticle)
        {
            if (codisArticle == null)
            {
                return new List<string>();
            }

            return codisArticle
                .Where(codi =>
                    !string.IsNullOrWhiteSpace(codi))
                .Select(codi =>
                    codi.Trim())
                .Distinct(
                    StringComparer.OrdinalIgnoreCase)
                .ToList();
        }

        /// <summary>
        /// Executa una consulta parametritzada
        /// per a un lot de codis.
        ///
        /// Alguns CODART numèrics d'a3ERP estan desats
        /// amb espais inicials fins a completar la longitud
        /// del camp.
        ///
        /// Per aquest motiu normalitzem CODART únicament
        /// amb LTRIM i RTRIM. Aquesta operació elimina
        /// els espais, però conserva els zeros inicials
        /// de codis com 00099.
        /// </summary>
        private static void LlegirLot(
            OleDbConnection connexio,
            IReadOnlyCollection<string> codis,
            IDictionary<string, ArticleA3ErpActual> resultat)
        {
            if (codis == null ||
                codis.Count == 0)
            {
                return;
            }

            string marcadors =
                string.Join(
                    ", ",
                    Enumerable.Repeat(
                        "?",
                        codis.Count));

            /*
             * Només recuperem els camps inclosos
             * a la plantilla actual del client.
             */
            string sql =
                "SELECT " +
                "LTRIM(RTRIM(CODART)) AS CODART, " +
                "DESCART, " +
                "CAR1, " +
                "CAR2, " +
                "PARAM1, " +
                "CODPRO, " +
                "ARTPRO, " +
                "PRCCOMPRA, " +
                "DESC1, " +
                "DESC2, " +
                "DESC3, " +
                "PRCCOSTE, " +
                "PRCSTANDARD, " +
                "AT_FORMULA_TARIFA_ID " +
                "FROM dbo.ARTICULO " +
                "WHERE LTRIM(RTRIM(CODART)) IN (" +
                marcadors +
                ")";

            using (OleDbCommand comanda =
                new OleDbCommand(
                    sql,
                    connexio))
            {
                /*
                 * OleDb associa els paràmetres per posició,
                 * no pel nom assignat.
                 *
                 * Per això els afegim en el mateix ordre
                 * que els interrogants de la consulta.
                 */
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

                using (OleDbDataReader lector =
                    comanda.ExecuteReader())
                {
                    if (lector == null)
                    {
                        return;
                    }

                    while (lector.Read())
                    {
                        ArticleA3ErpActual article =
                            CrearArticle(lector);

                        if (string.IsNullOrWhiteSpace(
                            article.CodiArticle))
                        {
                            continue;
                        }

                        resultat[article.CodiArticle] =
                            article;
                    }
                }
            }
        }

        /// <summary>
        /// Converteix una fila SQL al model utilitzat
        /// durant la validació del fitxer.
        /// </summary>
        private static ArticleA3ErpActual CrearArticle(
            OleDbDataReader lector)
        {
            return new ArticleA3ErpActual
            {
                CodiArticle =
                    LlegirText(
                        lector,
                        "CODART")
                    .Trim(),

                Descripcio =
                    LlegirText(
                        lector,
                        "DESCART")
                    .Trim(),

                Familia =
                    LlegirText(
                        lector,
                        "CAR1")
                    .Trim(),

                Unitats =
                    LlegirText(
                        lector,
                        "CAR2")
                    .Trim(),

                TipusUnitat =
                    LlegirText(
                        lector,
                        "PARAM1")
                    .Trim(),

                CodiProveidor =
                    LlegirText(
                        lector,
                        "CODPRO")
                    .Trim(),

                ReferenciaProveidor =
                    LlegirText(
                        lector,
                        "ARTPRO")
                    .Trim(),

                PreuCompra =
                    LlegirDecimalNullable(
                        lector,
                        "PRCCOMPRA"),

                Descompte1 =
                    LlegirDecimalNullable(
                        lector,
                        "DESC1"),

                Descompte2 =
                    LlegirDecimalNullable(
                        lector,
                        "DESC2"),

                Descompte3 =
                    LlegirDecimalNullable(
                        lector,
                        "DESC3"),

                PreuCost =
                    LlegirDecimalNullable(
                        lector,
                        "PRCCOSTE"),

                PreuTransport =
                    LlegirDecimalNullable(
                        lector,
                        "PRCSTANDARD"),

                IdFormula =
                    LlegirEnterNullable(
                        lector,
                        "AT_FORMULA_TARIFA_ID")
            };
        }

        public HashSet<int> ObtenirIdsFormulesTarifa(
            string cadenaConnexio)
        {
            if (string.IsNullOrWhiteSpace(cadenaConnexio))
            {
                throw new ArgumentException(
                    "La cadena de connexió amb a3ERP és buida.",
                    nameof(cadenaConnexio));
            }

            HashSet<int> ids =
                new HashSet<int>();

            const string sql =
                "SELECT IDFORMULA " +
                "FROM dbo.AT_ARTICULO_FORMULAS";

            using (OleDbConnection connexio =
                new OleDbConnection(cadenaConnexio))
            using (OleDbCommand comanda =
                new OleDbCommand(
                    sql,
                    connexio))
            {
                connexio.Open();

                using (OleDbDataReader lector =
                    comanda.ExecuteReader())
                {
                    if (lector == null)
                    {
                        return ids;
                    }

                    while (lector.Read())
                    {
                        int? idFormula =
                            LlegirEnterNullable(
                                lector,
                                "IDFORMULA");

                        if (idFormula.HasValue)
                        {
                            ids.Add(
                                idFormula.Value);
                        }
                    }
                }
            }

            return ids;
        }

        /// <summary>
        /// Llegeix una columna textual.
        ///
        /// Quan el valor és NULL retorna una cadena buida.
        /// </summary>
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
                       lector.GetValue(ordinal),
                       CultureInfo.InvariantCulture)
                   ?? string.Empty;
        }

        /// <summary>
        /// Llegeix un valor numèric nullable.
        ///
        /// També permet convertir els camps float d'a3ERP
        /// al tipus decimal utilitzat pel model intern.
        /// </summary>
        private static decimal? LlegirDecimalNullable(
            OleDbDataReader lector,
            string columna)
        {
            int ordinal =
                lector.GetOrdinal(columna);

            if (lector.IsDBNull(ordinal))
            {
                return null;
            }

            object valor =
                lector.GetValue(ordinal);

            try
            {
                return Convert.ToDecimal(
                    valor,
                    CultureInfo.InvariantCulture);
            }
            catch
            {
                return null;
            }
        }

        private static int? LlegirEnterNullable(
            OleDbDataReader lector,
            string columna)
        {
            int ordinal =
                lector.GetOrdinal(columna);

            if (lector.IsDBNull(ordinal))
            {
                return null;
            }

            try
            {
                return Convert.ToInt32(
                    lector.GetValue(ordinal),
                    CultureInfo.InvariantCulture);
            }
            catch
            {
                return null;
            }
        }
    }
}
