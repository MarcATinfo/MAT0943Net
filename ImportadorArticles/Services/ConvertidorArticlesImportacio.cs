using A3ErpImportadorArticles.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Text;

namespace A3ErpImportadorArticles.Services
{
    /// <summary>
    /// Converteix les files genèriques llegides d'un fitxer Excel o CSV
    /// al model intern ArticleImportacio.
    ///
    /// Aquesta classe interpreta exclusivament els camps
    /// de la plantilla actual del client.
    ///
    /// Els camps antics del model es conserven com a reserva futura,
    /// però no es llegeixen ni participen en el procés d'importació.
    ///
    /// Aquesta classe encara no consulta a3ERP ni determina
    /// si l'article s'ha de crear o actualitzar.
    /// </summary>
    public sealed class ConvertidorArticlesImportacio
    {
        private readonly CultureInfo culturaEspanyola =
            new CultureInfo("es-ES");

        /// <summary>
        /// Converteix totes les files d'una taula
        /// en articles importables.
        /// </summary>
        /// <param name="taula">
        /// Taula obtinguda després de llegir el fitxer Excel o CSV.
        /// </param>
        /// <returns>
        /// Llista d'articles interpretats i validats tècnicament.
        /// </returns>
        public List<ArticleImportacio> Convertir(
            DataTable taula)
        {
            if (taula == null)
            {
                throw new ArgumentNullException(
                    nameof(taula));
            }

            Dictionary<string, DataColumn> columnes =
                ConstruirIndexColumnes(taula);

            DataColumn columnaCodi =
                BuscarColumna(
                    columnes,
                    "CODART",
                    "CODI",
                    "CODI_ARTICLE",
                    "CODIGO",
                    "CODIGO_ARTICULO",
                    "CODI ARTICLE",
                    "CODIGO ARTICULO");

            if (columnaCodi == null)
            {
                throw new InvalidOperationException(
                    "No s'ha trobat la columna obligatòria CODART.");
            }

            List<ArticleImportacio> resultat =
                new List<ArticleImportacio>();

            for (
                int indexFila = 0;
                indexFila < taula.Rows.Count;
                indexFila++)
            {
                /*
                 * La fila 1 correspon a les capçaleres.
                 * Per tant, la primera fila de dades és la fila 2.
                 */
                int numeroFilaOrigen =
                    indexFila + 2;

                ArticleImportacio article =
                    ConvertirFila(
                        taula.Rows[indexFila],
                        columnes,
                        numeroFilaOrigen);

                resultat.Add(article);
            }

            return resultat;
        }

        /// <summary>
        /// Converteix una fila concreta del fitxer
        /// en un article.
        /// </summary>
        private ArticleImportacio ConvertirFila(
            DataRow fila,
            Dictionary<string, DataColumn> columnes,
            int numeroFila)
        {
            List<string> errors =
                new List<string>();

            List<string> avisos =
                new List<string>();

            /*
             * CODART sempre es tracta com a text.
             *
             * Exemples:
             * 15    → "15"
             * 00015 → "00015"
             * TLF   → "TLF"
             *
             * Només eliminem espais inicials i finals.
             */
            object valorCodi =
                ObtenirValor(
                    fila,
                    columnes,
                    "CODART",
                    "CODI",
                    "CODI_ARTICLE",
                    "CODIGO",
                    "CODIGO_ARTICULO",
                    "CODI ARTICLE",
                    "CODIGO ARTICULO");

            string codiArticle =
                ConvertirCodiArticle(valorCodi);

            ValidarCodiArticle(
                codiArticle,
                errors);

            string descripcio =
                ConvertirText(
                    ObtenirValor(
                        fila,
                        columnes,
                        "DESCART",
                        "DESCRIPCIO",
                        "DESCRIPCION",
                        "DESCRIPCIÓ"));

            ValidarLongitudText(
                descripcio,
                100,
                "La descripció",
                errors);

            if (string.IsNullOrWhiteSpace(descripcio))
            {
                avisos.Add(
                    "La descripció no està informada.");
            }

            string familia =
                ConvertirText(
                    ObtenirValor(
                        fila,
                        columnes,
                        "CAR1",
                        "FAMILIA",
                        "FAMÍLIA"));

            ValidarLongitudText(
                familia,
                8,
                "La família",
                errors);

            string unitats =
                ConvertirText(
                    ObtenirValor(
                        fila,
                        columnes,
                        "CAR2",
                        "UNITATS",
                        "UNIDADES"));

            ValidarLongitudText(
                unitats,
                8,
                "El camp d'unitats",
                errors);

            string tipusUnitat =
                ConvertirText(
                    ObtenirValor(
                        fila,
                        columnes,
                        "PARAM1",
                        "TIPUS_UNITAT",
                        "TIPO_UNIDAD",
                        "TIPUS UNITAT",
                        "TIPO UNIDAD"));

            ValidarLongitudText(
                tipusUnitat,
                30,
                "El tipus d'unitat",
                errors);

            string codiProveidor =
                ConvertirText(
                    ObtenirValor(
                        fila,
                        columnes,
                        "CODPRO",
                        "CODI_PROVEIDOR",
                        "CODIGO_PROVEEDOR",
                        "CODI PROVEIDOR",
                        "CODIGO PROVEEDOR"));

            ValidarLongitudText(
                codiProveidor,
                8,
                "El codi de proveïdor",
                errors);

            string referenciaProveidor =
                ConvertirText(
                    ObtenirValor(
                        fila,
                        columnes,
                        "ARTPRO",
                        "REFERENCIA_PROVEIDOR",
                        "REFERENCIA_PROVEEDOR",
                        "REFERÈNCIA PROVEÏDOR",
                        "REFERENCIA PROVEEDOR"));

            ValidarLongitudText(
                referenciaProveidor,
                35,
                "La referència del proveïdor",
                errors);

            decimal? preuCompra =
                ConvertirDecimal(
                    ObtenirValor(
                        fila,
                        columnes,
                        "PRCCOMPRA",
                        "PREU_COMPRA",
                        "PRECIO_COMPRA",
                        "PREU COMPRA",
                        "PRECIO COMPRA",
                        "PVP"),
                    "preu de compra",
                    errors);

            decimal? descompte1 =
                ConvertirDecimal(
                    ObtenirValor(
                        fila,
                        columnes,
                        "DESC1",
                        "DTO1",
                        "DTO_1",
                        "DESCOMPTE1",
                        "DESCOMPTE_1",
                        "DESCUENTO1",
                        "DESCUENTO_1"),
                    "descompte 1",
                    errors);

            decimal? descompte2 =
                ConvertirDecimal(
                    ObtenirValor(
                        fila,
                        columnes,
                        "DESC2",
                        "DTO2",
                        "DTO_2",
                        "DESCOMPTE2",
                        "DESCOMPTE_2",
                        "DESCUENTO2",
                        "DESCUENTO_2"),
                    "descompte 2",
                    errors);

            decimal? descompte3 =
                ConvertirDecimal(
                    ObtenirValor(
                        fila,
                        columnes,
                        "DESC3",
                        "DTO3",
                        "DTO_3",
                        "DESCOMPTE3",
                        "DESCOMPTE_3",
                        "DESCUENTO3",
                        "DESCUENTO_3"),
                    "descompte 3",
                    errors);

            decimal? preuCost =
                ConvertirDecimal(
                    ObtenirValor(
                        fila,
                        columnes,
                        "PRCCOSTE",
                        "PREU_COST",
                        "PRECIO_COSTE",
                        "PREU COST",
                        "PRECIO COSTE"),
                    "preu de cost",
                    errors);

            decimal? preuTransport =
                ConvertirDecimal(
                    ObtenirValor(
                        fila,
                        columnes,
                        "PRCSTANDARD",
                        "PREU_TRANSPORT",
                        "PRECIO_TRANSPORTE",
                        "TRANSPORT",
                        "TRANSPORTE",
                        "PREU TRANSPORT",
                        "PRECIO TRANSPORTE"),
                    "transport",
                    errors);

            object valorIdFormula =
                ObtenirValor(
                    fila,
                    columnes,
                    "IDFORMULA");

            string idFormulaOriginal =
                ConvertirText(
                    valorIdFormula);

            int? idFormula =
                ConvertirEnterNullable(
                    valorIdFormula,
                    "IDFORMULA",
                    errors);

            EstatImportacio estat =
                errors.Count > 0
                    ? EstatImportacio.Error
                    : EstatImportacio.Pendent;

            string missatge =
                ConstruirMissatge(
                    errors,
                    avisos);

            return new ArticleImportacio
            {
                Seleccionat =
                    errors.Count == 0,

                NumeroFila =
                    numeroFila,

                Estat =
                    estat,

                CodiArticle =
                    codiArticle,

                Descripcio =
                    descripcio,

                Familia =
                    familia,

                Unitats =
                    unitats,

                TipusUnitat =
                    tipusUnitat,

                CodiProveidor =
                    codiProveidor,

                ReferenciaProveidor =
                    referenciaProveidor,

                PreuCompra =
                    preuCompra,

                Descompte1 =
                    descompte1,

                Descompte2 =
                    descompte2,

                Descompte3 =
                    descompte3,

                PreuCost =
                    preuCost,

                PreuTransport =
                    preuTransport,

                IdFormula =
                    idFormula,

                IdFormulaOriginal =
                    idFormulaOriginal,

                Missatge =
                    missatge

                /*
                 * Els camps antics no s'assignen.
                 *
                 * Per tant, es mantenen amb null o cadena buida
                 * i no participen en el procés actual.
                 */
            };
        }

        /// <summary>
        /// Valida CODART segons l'estructura
        /// real d'ARTICULO.CODART.
        ///
        /// El camp és obligatori, admet un màxim
        /// de 15 caràcters i pot contenir números,
        /// lletres o una combinació d'ambdós.
        /// </summary>
        private static void ValidarCodiArticle(
            string codiArticle,
            List<string> errors)
        {
            if (string.IsNullOrWhiteSpace(codiArticle))
            {
                errors.Add(
                    "El codi d'article és obligatori.");

                return;
            }

            if (codiArticle.Length > 15)
            {
                errors.Add(
                    "El codi d'article supera els 15 caràcters permesos.");
            }
        }

        /// <summary>
        /// Valida la longitud màxima d'un camp textual.
        ///
        /// Els valors buits són vàlids perquè els camps
        /// de la plantilla, excepte CODART, són opcionals.
        /// </summary>
        private static void ValidarLongitudText(
            string valor,
            int longitudMaxima,
            string nomCamp,
            List<string> errors)
        {
            if (string.IsNullOrEmpty(valor))
            {
                return;
            }

            if (valor.Length > longitudMaxima)
            {
                errors.Add(
                    string.Format(
                        "{0} supera els {1} caràcters permesos.",
                        nomCamp,
                        longitudMaxima));
            }
        }

        /// <summary>
        /// Converteix CODART a text.
        ///
        /// No afegeix zeros, no elimina zeros inicials
        /// i no interpreta el codi com una dada numèrica.
        ///
        /// Només elimina espais inicials i finals.
        /// </summary>
        private static string ConvertirCodiArticle(
            object valor)
        {
            if (valor == null ||
                valor == DBNull.Value)
            {
                return string.Empty;
            }

            string valorText =
                valor as string;

            if (valorText != null)
            {
                return valorText.Trim();
            }

            /*
             * Quan Excel retorna una cel·la numèrica:
             *
             * 15 continuarà sent "15".
             *
             * No completarem automàticament el valor
             * amb zeros inicials.
             */
            return Convert
                .ToString(
                    valor,
                    CultureInfo.InvariantCulture)
                .Trim();
        }

        /// <summary>
        /// Converteix una cel·la genèrica a text.
        /// </summary>
        private static string ConvertirText(
            object valor)
        {
            if (valor == null ||
                valor == DBNull.Value)
            {
                return string.Empty;
            }

            return Convert
                .ToString(
                    valor,
                    CultureInfo.InvariantCulture)
                .Trim();
        }

        /// <summary>
        /// Converteix imports i percentatges.
        ///
        /// Admet decimals amb coma o amb punt.
        /// Una cel·la buida es manté com a null
        /// i no modificarà el valor existent a a3ERP.
        /// </summary>
        private decimal? ConvertirDecimal(
            object valor,
            string nomCamp,
            List<string> errors)
        {
            if (valor == null ||
                valor == DBNull.Value)
            {
                return null;
            }

            if (EsValorNumeric(valor))
            {
                try
                {
                    return Convert.ToDecimal(
                        valor,
                        CultureInfo.InvariantCulture);
                }
                catch
                {
                    errors.Add(
                        "El valor del camp " +
                        nomCamp +
                        " no és vàlid.");

                    return null;
                }
            }

            string text =
                ConvertirText(valor);

            if (string.IsNullOrWhiteSpace(text))
            {
                return null;
            }

            decimal resultat;

            /*
             * Format habitual espanyol: 12,50
             */
            if (text.Contains(",") &&
                !text.Contains("."))
            {
                if (decimal.TryParse(
                    text,
                    NumberStyles.Number,
                    culturaEspanyola,
                    out resultat))
                {
                    return resultat;
                }
            }
            /*
             * Format invariant: 12.50
             */
            else if (text.Contains(".") &&
                     !text.Contains(","))
            {
                if (decimal.TryParse(
                    text,
                    NumberStyles.Number,
                    CultureInfo.InvariantCulture,
                    out resultat))
                {
                    return resultat;
                }
            }
            else
            {
                if (decimal.TryParse(
                    text,
                    NumberStyles.Number,
                    culturaEspanyola,
                    out resultat))
                {
                    return resultat;
                }

                if (decimal.TryParse(
                    text,
                    NumberStyles.Number,
                    CultureInfo.InvariantCulture,
                    out resultat))
                {
                    return resultat;
                }
            }

            errors.Add(
                "El valor del camp " +
                nomCamp +
                " no és numèric.");

            return null;
        }

        /// <summary>
        /// Converteix un camp enter opcional.
        ///
        /// Una cel·la buida es manté com a null
        /// i no modificarà el valor existent a a3ERP.
        /// </summary>
        private static int? ConvertirEnterNullable(
            object valor,
            string nomCamp,
            List<string> errors)
        {
            if (valor == null ||
                valor == DBNull.Value)
            {
                return null;
            }

            if (EsValorNumeric(valor))
            {
                decimal valorDecimal;

                try
                {
                    valorDecimal =
                        Convert.ToDecimal(
                            valor,
                            CultureInfo.InvariantCulture);
                }
                catch
                {
                    errors.Add(
                        "El camp " +
                        nomCamp +
                        " ha de ser un enter vàlid.");

                    return null;
                }

                if (valorDecimal != decimal.Truncate(valorDecimal) ||
                    valorDecimal < int.MinValue ||
                    valorDecimal > int.MaxValue)
                {
                    errors.Add(
                        "El camp " +
                        nomCamp +
                        " ha de ser un enter vàlid.");

                    return null;
                }

                return (int)valorDecimal;
            }

            string text =
                ConvertirText(valor);

            if (string.IsNullOrWhiteSpace(text))
            {
                return null;
            }

            int resultat;

            if (int.TryParse(
                text,
                NumberStyles.Integer,
                CultureInfo.InvariantCulture,
                out resultat))
            {
                return resultat;
            }

            errors.Add(
                "El camp " +
                nomCamp +
                " ha de ser un enter vàlid.");

            return null;
        }

        /// <summary>
        /// Retorna el valor d'una columna admetent
        /// diferents noms equivalents.
        /// </summary>
        private static object ObtenirValor(
            DataRow fila,
            Dictionary<string, DataColumn> columnes,
            params string[] nomsPossibles)
        {
            DataColumn columna =
                BuscarColumna(
                    columnes,
                    nomsPossibles);

            if (columna == null)
            {
                return null;
            }

            return fila[columna];
        }

        /// <summary>
        /// Busca una columna a partir
        /// dels seus possibles noms.
        /// </summary>
        private static DataColumn BuscarColumna(
            Dictionary<string, DataColumn> columnes,
            params string[] nomsPossibles)
        {
            foreach (string nom in nomsPossibles)
            {
                string nomNormalitzat =
                    NormalitzarNomColumna(nom);

                DataColumn columna;

                if (columnes.TryGetValue(
                    nomNormalitzat,
                    out columna))
                {
                    return columna;
                }
            }

            return null;
        }

        /// <summary>
        /// Construeix un índex de les columnes del fitxer.
        ///
        /// Això permet localitzar-les sense dependre
        /// de majúscules, accents o espais.
        /// </summary>
        private static Dictionary<string, DataColumn>
            ConstruirIndexColumnes(
                DataTable taula)
        {
            Dictionary<string, DataColumn> resultat =
                new Dictionary<string, DataColumn>();

            foreach (DataColumn columna in taula.Columns)
            {
                string nomNormalitzat =
                    NormalitzarNomColumna(
                        columna.ColumnName);

                if (!resultat.ContainsKey(
                    nomNormalitzat))
                {
                    resultat.Add(
                        nomNormalitzat,
                        columna);
                }
            }

            return resultat;
        }

        /// <summary>
        /// Normalitza els noms de les columnes.
        ///
        /// Exemples:
        /// "Código artículo" → "CODIGO_ARTICULO"
        /// "Preu compra"     → "PREU_COMPRA"
        /// </summary>
        private static string NormalitzarNomColumna(
            string text)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                return string.Empty;
            }

            string textDescompost =
                text
                    .Trim()
                    .Normalize(
                        NormalizationForm.FormD);

            StringBuilder resultat =
                new StringBuilder();

            foreach (char caracter in textDescompost)
            {
                UnicodeCategory categoria =
                    CharUnicodeInfo.GetUnicodeCategory(
                        caracter);

                /*
                 * Eliminem els signes dels accents.
                 */
                if (categoria ==
                    UnicodeCategory.NonSpacingMark)
                {
                    continue;
                }

                if (char.IsLetterOrDigit(caracter))
                {
                    resultat.Append(
                        char.ToUpperInvariant(
                            caracter));
                }
                else
                {
                    resultat.Append('_');
                }
            }

            return resultat
                .ToString()
                .Trim('_');
        }

        /// <summary>
        /// Indica si una cel·la conté
        /// un valor numèric.
        ///
        /// S'utilitza per als imports i percentatges,
        /// però mai per modificar CODART.
        /// </summary>
        private static bool EsValorNumeric(
            object valor)
        {
            if (valor == null ||
                valor == DBNull.Value)
            {
                return false;
            }

            TypeCode tipus =
                Type.GetTypeCode(
                    valor.GetType());

            switch (tipus)
            {
                case TypeCode.Byte:
                case TypeCode.SByte:
                case TypeCode.Int16:
                case TypeCode.UInt16:
                case TypeCode.Int32:
                case TypeCode.UInt32:
                case TypeCode.Int64:
                case TypeCode.UInt64:
                case TypeCode.Single:
                case TypeCode.Double:
                case TypeCode.Decimal:
                    return true;

                default:
                    return false;
            }
        }

        /// <summary>
        /// Construeix el missatge que es mostrarà
        /// a la graella per a cada fila.
        /// </summary>
        private static string ConstruirMissatge(
            List<string> errors,
            List<string> avisos)
        {
            if (errors.Count > 0)
            {
                return string.Join(
                    " · ",
                    errors);
            }

            if (avisos.Count > 0)
            {
                return string.Join(
                    " · ",
                    avisos);
            }

            return "Pendent de validació amb a3ERP.";
        }
    }
}
