using A3ErpCalculadorTarifes.Models;
using System;
using System.Collections.Generic;
using System.Data.OleDb;
using System.Globalization;

namespace A3ErpCalculadorTarifes.Infrastructure.Data
{
    /// <summary>
    /// Consulta els articles de l'empresa activa.
    ///
    /// Aquesta classe només executa operacions
    /// de lectura.
    /// </summary>
    public sealed class RepositoriArticlesCalculTarifes
    {
        /// <summary>
        /// Obté els articles assignats a una fórmula
        /// concreta per al calculador de tarifes.
        ///
        /// Es conserven dos codis:
        /// - el codi visible, sense espais;
        /// - el codi literal, tal com està guardat
        ///   a dbo.ARTICULO.
        /// </summary>
        public List<ArticleCalculTarifes> ObtenirPerFormula(
            string cadenaConnexio,
            int idFormula)
        {
            if (string.IsNullOrWhiteSpace(
                cadenaConnexio))
            {
                throw new ArgumentException(
                    "La connexió amb a3ERP és buida.",
                    nameof(cadenaConnexio));
            }

            if (idFormula <= 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(idFormula),
                    "L'identificador de la fórmula no és vàlid.");
            }

            const string sql = @"
                SELECT
                    A.CODART AS CODART_BASE_DADES,
                    LTRIM(RTRIM(A.CODART)) AS CODART_VISIBLE,
                    A.DESCART,
                    LTRIM(RTRIM(A.CAR1)) AS CAR1,
                    A.PRCCOMPRA,
                    A.PRCCOSTE,
                    A.PRCSTANDARD
                FROM dbo.ARTICULO AS A
                WHERE A.AT_FORMULA_TARIFA_ID = ?
                ORDER BY
                    CASE
                        WHEN LTRIM(RTRIM(A.CODART)) <> ''
                             AND LTRIM(RTRIM(A.CODART)) NOT LIKE '%[^0-9]%'
                            THEN 0
                        ELSE 1
                    END,
                    CONVERT(
                        decimal(38, 0),
                        CASE
                            WHEN LTRIM(RTRIM(A.CODART)) <> ''
                                 AND LTRIM(RTRIM(A.CODART)) NOT LIKE '%[^0-9]%'
                                THEN LTRIM(RTRIM(A.CODART))
                            ELSE NULL
                        END),
                    LTRIM(RTRIM(A.CODART));";

            var articles =
                new List<ArticleCalculTarifes>();

            using (var connexio =
                new OleDbConnection(
                    cadenaConnexio))
            using (var comanda =
                new OleDbCommand(
                    sql,
                    connexio))
            {
                comanda.Parameters.Add(
                    "@IdFormula",
                    OleDbType.Integer)
                    .Value =
                    idFormula;

                connexio.Open();

                using (OleDbDataReader lector =
                    comanda.ExecuteReader())
                {
                    while (lector != null &&
                           lector.Read())
                    {
                        ArticleCalculTarifes article =
                            CrearArticle(
                                lector);

                        /*
                         * Validem el codi visible perquè una fila
                         * sense codi no s'ha de mostrar.
                         *
                         * El codi literal es conserva intacte.
                         */
                        if (!string.IsNullOrWhiteSpace(
                            article.CodiArticle))
                        {
                            articles.Add(
                                article);
                        }
                    }
                }
            }

            return articles;
        }

        /// <summary>
        /// Construeix el model a partir de la fila
        /// llegida de dbo.ARTICULO.
        /// </summary>
        private static ArticleCalculTarifes
            CrearArticle(
                OleDbDataReader lector)
        {
            string codiArticleBaseDades =
                LlegirText(
                    lector,
                    "CODART_BASE_DADES");

            string codiArticleVisible =
                LlegirText(
                    lector,
                    "CODART_VISIBLE")
                .Trim();

            return new ArticleCalculTarifes
            {
                Seleccionat = false,

                /*
                 * Valor que veu l'usuari:
                 * "16"
                 */
                CodiArticle =
                    codiArticleVisible,

                /*
                 * Valor literal de dbo.ARTICULO:
                 * "             16"
                 *
                 * No s'aplica Trim.
                 */
                CodiArticleBaseDades =
                    codiArticleBaseDades,

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

                PreuCompra =
                    LlegirDecimal(
                        lector,
                        "PRCCOMPRA"),

                PreuCost =
                    LlegirDecimal(
                        lector,
                        "PRCCOSTE"),

                PreuTransport =
                    LlegirDecimal(
                        lector,
                        "PRCSTANDARD")
            };
        }

        /// <summary>
        /// Llegeix un valor textual.
        ///
        /// El mètode no aplica Trim perquè alguns
        /// codis d'a3ERP necessiten conservar
        /// els espais originals.
        /// </summary>
        private static string LlegirText(
            OleDbDataReader lector,
            string columna)
        {
            int ordinal =
                lector.GetOrdinal(
                    columna);

            if (lector.IsDBNull(
                ordinal))
            {
                return string.Empty;
            }

            return Convert.ToString(
                       lector.GetValue(
                           ordinal),
                       CultureInfo.InvariantCulture)
                   ?? string.Empty;
        }

        /// <summary>
        /// Llegeix un valor decimal i converteix
        /// els nuls de base de dades en zero.
        /// </summary>
        private static decimal LlegirDecimal(
            OleDbDataReader lector,
            string columna)
        {
            int ordinal =
                lector.GetOrdinal(
                    columna);

            if (lector.IsDBNull(
                ordinal))
            {
                return 0m;
            }

            return Convert.ToDecimal(
                lector.GetValue(
                    ordinal),
                CultureInfo.InvariantCulture);
        }
    }
}
