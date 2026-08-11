using A3ErpCalculadorTarifes.Models;
using A3ErpCalculadorTarifes.Infrastructure.Logging;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb;
using System.Globalization;
using System.Linq;

namespace A3ErpCalculadorTarifes.Infrastructure.Data
{
    /// <summary>
    /// Executa l'aplicació de tarifes mitjançant
    /// el procediment oficial dbo.ActualizarTarifa.
    ///
    /// En aquesta primera fase només ofereix
    /// una simulació transaccional:
    /// - executa les operacions;
    /// - verifica els resultats;
    /// - sempre fa ROLLBACK.
    ///
    /// DESCUENT es modifica o es neteja
    /// dins de la mateixa transacció.
    /// </summary>
    public sealed class RepositoriAplicacioTarifes
    {
        private static readonly DateTime DataMinima =
            new DateTime(
                1900,
                1,
                1);

        private static readonly DateTime DataMaxima =
            new DateTime(
                9999,
                12,
                31);

        private const double UnitatsGenerals =
            0d;

        private const decimal ToleranciaPreu =
            0.0001m;

        /// <summary>
        /// Simula l'aplicació de les tarifes 1–6
        /// i, quan correspon, dels quatre descomptes
        /// per família de client.
        ///
        /// Els procediments oficials d'a3ERP poden
        /// actualitzar registres existents o crear-ne
        /// de nous.
        ///
        /// La transacció sempre acaba amb ROLLBACK.
        /// No queda cap canvi guardat a TARIFAVE
        /// ni a DESCUENT.
        /// </summary>
        public ResultatSimulacioAplicacioTarifes Simular(
            string cadenaConnexio,
            IEnumerable<ResultatCalculTarifes> resultats,
            IProgress<ProgresAplicacioTarifes> progres = null)
        {
            if (string.IsNullOrWhiteSpace(
                cadenaConnexio))
            {
                throw new ArgumentException(
                    "La connexió amb a3ERP és buida.",
                    nameof(cadenaConnexio));
            }

            List<ResultatCalculTarifes> lot =
                resultats == null
                    ? new List<ResultatCalculTarifes>()
                    : resultats.ToList();

            CalculadorTarifesLogger.Informacio(
                "Repositori: s'inicia Simular().",
                CrearCampsLot(
                    lot));

            ValidarLot(
                lot);

            OleDbTransaction transaccio =
                null;

            try
            {
                using (var connexio =
                    new OleDbConnection(
                        cadenaConnexio))
                {
                    connexio.Open();

                    transaccio =
                        connexio.BeginTransaction(
                            IsolationLevel.ReadCommitted);

                    string monedaBaseDades =
                        ObtenirMonedaBaseDades(
                            connexio,
                            transaccio,
                            "EURO");

                    int decimalsPreu =
                        ObtenirDecimalsPreu(
                            connexio,
                            transaccio);

                    Dictionary<int, string>
                        tarifesBaseDades =
                            ObtenirTarifesBaseDades(
                                connexio,
                                transaccio);

                    /*
                     * Les famílies de descompte només
                     * són necessàries quan almenys
                     * un resultat genera descomptes.
                     */
                    bool hiHaDescomptes =
                        lot.Any(
                            resultat =>
                                resultat.GeneraDescomptes);

                    Dictionary<int, string>
                        familiesDescompteBaseDades =
                            new Dictionary<int, string>();

                    if (hiHaDescomptes)
                    {
                        familiesDescompteBaseDades =
                            ObtenirFamiliesDescompteBaseDades(
                                connexio,
                                transaccio);
                    }

                    int tarifesProcessades =
                        0;

                    int descomptesProcessats =
                        0;

                    int descomptesEliminats =
                        0;

                    int articlesProcessats =
                        0;

                    int ultimPercentatgeNotificat =
                        0;

                    foreach (
                        ResultatCalculTarifes resultat
                        in lot)
                    {
                        /*
                         * Simulem sempre les sis tarifes
                         * de cada article.
                         */
                        tarifesProcessades +=
                            ProcessarArticle(
                                connexio,
                                transaccio,
                                monedaBaseDades,
                                tarifesBaseDades,
                                decimalsPreu,
                                resultat);

                        /*
                         * DESCUENT es crea o actualitza
                         * quan la fórmula genera descomptes;
                         * en cas contrari es netegen els
                         * descomptes gestionats.
                         */
                        if (resultat.GeneraDescomptes)
                        {
                            descomptesProcessats +=
                                ProcessarDescomptesArticle(
                                    connexio,
                                    transaccio,
                                    familiesDescompteBaseDades,
                                    resultat);
                        }
                        else
                        {
                            int filesDelete =
                                EliminarDescomptesGestionatsArticle(
                                    connexio,
                                    transaccio,
                                    resultat.CodiArticleBaseDades);

                            descomptesEliminats +=
                                filesDelete;
                        }

                        articlesProcessats++;

                        NotificarProgres(
                            progres,
                            articlesProcessats,
                            lot.Count,
                            ref ultimPercentatgeNotificat);
                    }

                    /*
                     * Aquesta fase és estrictament
                     * una simulació.
                     *
                     * TARIFAVE i DESCUENT es troben
                     * dins de la mateixa transacció
                     * i es desfan conjuntament.
                     */
                    transaccio.Rollback();

                    CalculadorTarifesLogger.Informacio(
                        "Repositori: ROLLBACK correcte de Simular().",
                        CrearCampsProces(
                            lot.Count,
                            tarifesProcessades,
                            descomptesProcessats,
                            descomptesEliminats));

                    transaccio =
                        null;

                    CalculadorTarifesLogger.Informacio(
                        "Repositori: finalitza Simular() correctament.",
                        CrearCampsProces(
                            lot.Count,
                            tarifesProcessades,
                            descomptesProcessats,
                            descomptesEliminats));

                    return
                        ResultatSimulacioAplicacioTarifes
                            .CrearCorrecte(
                                lot.Count,
                                tarifesProcessades,
                                descomptesProcessats,
                                descomptesEliminats);
                }
            }
            catch (Exception ex)
            {
                /*
                 * Davant de qualsevol error intentem
                 * desfer tota la transacció.
                 */
                IntentarRollback(
                    transaccio,
                    "Simular");

                CalculadorTarifesLogger.Error(
                    "Repositori: error a Simular().",
                    ex,
                    CrearCampsLot(
                        lot));

                return
                    ResultatSimulacioAplicacioTarifes
                        .CrearError(
                            "No s'ha pogut completar "
                            + "la simulació de tarifes "
                            + "i descomptes."
                            + Environment.NewLine
                            + Environment.NewLine
                            + ex.Message);
            }
        }

        /// <summary>
        /// Aplica i verifica les sis tarifes
        /// d'un únic article.
        /// </summary>
        private static int ProcessarArticle(
            OleDbConnection connexio,
            OleDbTransaction transaccio,
            string monedaBaseDades,
            IDictionary<int, string> tarifesBaseDades,
            int decimalsPreu,
            ResultatCalculTarifes resultat)
        {
            AplicarIVerificarTarifaAmbLog(
                connexio,
                transaccio,
                tarifesBaseDades[1],
                resultat.CodiArticleBaseDades,
                monedaBaseDades,
                decimalsPreu,
                resultat.Tarifa1);

            AplicarIVerificarTarifaAmbLog(
                connexio,
                transaccio,
                tarifesBaseDades[2],
                resultat.CodiArticleBaseDades,
                monedaBaseDades,
                decimalsPreu,
                resultat.Tarifa2);

            AplicarIVerificarTarifaAmbLog(
                connexio,
                transaccio,
                tarifesBaseDades[3],
                resultat.CodiArticleBaseDades,
                monedaBaseDades,
                decimalsPreu,
                resultat.Tarifa3);

            AplicarIVerificarTarifaAmbLog(
                connexio,
                transaccio,
                tarifesBaseDades[4],
                resultat.CodiArticleBaseDades,
                monedaBaseDades,
                decimalsPreu,
                resultat.Tarifa4);

            AplicarIVerificarTarifaAmbLog(
                connexio,
                transaccio,
                tarifesBaseDades[5],
                resultat.CodiArticleBaseDades,
                monedaBaseDades,
                decimalsPreu,
                resultat.Tarifa5);

            AplicarIVerificarTarifaAmbLog(
                connexio,
                transaccio,
                tarifesBaseDades[6],
                resultat.CodiArticleBaseDades,
                monedaBaseDades,
                decimalsPreu,
                resultat.Tarifa6);

            return 6;
        }

        /// <summary>
        /// Aplica definitivament les tarifes 1–6
        /// i, quan correspon, els quatre descomptes
        /// per família de client.
        ///
        /// Tot el lot s'executa dins d'una única
        /// transacció:
        /// - si tot és correcte, fa COMMIT;
        /// - si falla qualsevol operació, fa ROLLBACK.
        ///
        /// TARIFAVE i DESCUENT es confirmen
        /// o es desfan conjuntament.
        /// </summary>
        public ResultatAplicacioTarifes Aplicar(
            string cadenaConnexio,
            IEnumerable<ResultatCalculTarifes> resultats,
            IProgress<ProgresAplicacioTarifes> progres = null)
        {
            if (string.IsNullOrWhiteSpace(
                cadenaConnexio))
            {
                throw new ArgumentException(
                    "La connexió amb a3ERP és buida.",
                    nameof(cadenaConnexio));
            }

            List<ResultatCalculTarifes> lot =
                resultats == null
                    ? new List<ResultatCalculTarifes>()
                    : resultats.ToList();

            CalculadorTarifesLogger.Informacio(
                "Repositori: s'inicia Aplicar().",
                CrearCampsLot(
                    lot));

            ValidarLot(
                lot);

            OleDbTransaction transaccio =
                null;

            try
            {
                using (var connexio =
                    new OleDbConnection(
                        cadenaConnexio))
                {
                    connexio.Open();

                    transaccio =
                        connexio.BeginTransaction(
                            IsolationLevel.ReadCommitted);

                    string monedaBaseDades =
                        ObtenirMonedaBaseDades(
                            connexio,
                            transaccio,
                            "EURO");

                    int decimalsPreu =
                        ObtenirDecimalsPreu(
                            connexio,
                            transaccio);

                    Dictionary<int, string>
                        tarifesBaseDades =
                            ObtenirTarifesBaseDades(
                                connexio,
                                transaccio);

                    /*
                     * Només carreguem les famílies
                     * de descompte quan alguna plantilla
                     * necessita crear o actualitzar DESCUENT.
                     */
                    bool hiHaDescomptes =
                        lot.Any(
                            resultat =>
                                resultat.GeneraDescomptes);

                    Dictionary<int, string>
                        familiesDescompteBaseDades =
                            new Dictionary<int, string>();

                    if (hiHaDescomptes)
                    {
                        familiesDescompteBaseDades =
                            ObtenirFamiliesDescompteBaseDades(
                                connexio,
                                transaccio);
                    }

                    int tarifesProcessades =
                        0;

                    int descomptesProcessats =
                        0;

                    int descomptesEliminats =
                        0;

                    int articlesProcessats =
                        0;

                    int ultimPercentatgeNotificat =
                        0;

                    foreach (
                        ResultatCalculTarifes resultat
                        in lot)
                    {
                        /*
                         * Apliquem i verifiquem sempre
                         * les sis tarifes de l'article.
                         */
                        tarifesProcessades +=
                            ProcessarArticle(
                                connexio,
                                transaccio,
                                monedaBaseDades,
                                tarifesBaseDades,
                                decimalsPreu,
                                resultat);

                        /*
                         * Les fórmules amb descomptes creen
                         * o actualitzen els quatre registres.
                         * Les altres eliminen només els
                         * registres gestionats.
                         */
                        if (resultat.GeneraDescomptes)
                        {
                            descomptesProcessats +=
                                ProcessarDescomptesArticle(
                                    connexio,
                                    transaccio,
                                    familiesDescompteBaseDades,
                                    resultat);
                        }
                        else
                        {
                            int filesDelete =
                                EliminarDescomptesGestionatsArticle(
                                    connexio,
                                    transaccio,
                                    resultat.CodiArticleBaseDades);

                            descomptesEliminats +=
                                filesDelete;
                        }

                        articlesProcessats++;

                        NotificarProgres(
                            progres,
                            articlesProcessats,
                            lot.Count,
                            ref ultimPercentatgeNotificat);
                    }

                    /*
                     * Només arribem al COMMIT quan totes
                     * les tarifes i tots els descomptes
                     * han estat aplicats i verificats.
                     */
                    transaccio.Commit();

                    CalculadorTarifesLogger.Informacio(
                        "Repositori: COMMIT correcte d'Aplicar().",
                        CrearCampsProces(
                            lot.Count,
                            tarifesProcessades,
                            descomptesProcessats,
                            descomptesEliminats));

                    transaccio =
                        null;

                    CalculadorTarifesLogger.Informacio(
                        "Repositori: finalitza Aplicar() correctament.",
                        CrearCampsProces(
                            lot.Count,
                            tarifesProcessades,
                            descomptesProcessats,
                            descomptesEliminats));

                    return
                        ResultatAplicacioTarifes
                            .CrearCorrecte(
                                lot.Count,
                                tarifesProcessades,
                                descomptesProcessats,
                                descomptesEliminats);
                }
            }
            catch (Exception ex)
            {
                /*
                 * Qualsevol error desfà conjuntament
                 * TARIFAVE i DESCUENT.
                 *
                 * No poden quedar dades parcials.
                 */
                IntentarRollback(
                    transaccio,
                    "Aplicar");

                CalculadorTarifesLogger.Error(
                    "Repositori: error a Aplicar().",
                    ex,
                    CrearCampsLot(
                        lot));

                return
                    ResultatAplicacioTarifes
                        .CrearError(
                            "No s'han pogut aplicar les tarifes "
                            + "i els descomptes."
                            + Environment.NewLine
                            + Environment.NewLine
                            + ex.Message);
            }
        }

        /// <summary>
        /// Arrodoneix el preu segons DATOSCONFIG,
        /// crida dbo.ActualizarTarifa i comprova
        /// immediatament el valor persistent.
        /// </summary>
        private static void AplicarIVerificarTarifaAmbLog(
            OleDbConnection connexio,
            OleDbTransaction transaccio,
            string tarifaBaseDades,
            string articleBaseDades,
            string monedaBaseDades,
            int decimalsPreu,
            decimal preu)
        {
            try
            {
                AplicarIVerificarTarifa(
                    connexio,
                    transaccio,
                    tarifaBaseDades,
                    articleBaseDades,
                    monedaBaseDades,
                    decimalsPreu,
                    preu);
            }
            catch (Exception ex)
            {
                CalculadorTarifesLogger.Error(
                    "Repositori: error processant TARIFAVE.",
                    ex,
                    new Dictionary<string, object>
                    {
                        {
                            "Article",
                            (articleBaseDades ?? string.Empty).Trim()
                        },
                        {
                            "Tarifa",
                            (tarifaBaseDades ?? string.Empty).Trim()
                        }
                    });

                throw;
            }
        }

        private static void AplicarIVerificarTarifa(
            OleDbConnection connexio,
            OleDbTransaction transaccio,
            string tarifaBaseDades,
            string articleBaseDades,
            string monedaBaseDades,
            int decimalsPreu,
            decimal preu)
        {
            decimal preuArrodonit =
                Math.Round(
                    preu,
                    decimalsPreu,
                    MidpointRounding.AwayFromZero);

            ExecutarActualizarTarifa(
                connexio,
                transaccio,
                tarifaBaseDades,
                articleBaseDades,
                monedaBaseDades,
                preuArrodonit);

            decimal preuLlegit =
                LlegirPreuTarifa(
                    connexio,
                    transaccio,
                    tarifaBaseDades,
                    articleBaseDades,
                    monedaBaseDades);

            decimal preuObtingut =
                Math.Round(
                    preuLlegit,
                    decimalsPreu,
                    MidpointRounding.AwayFromZero);

            if (preuArrodonit !=
                preuObtingut)
            {
                throw new InvalidOperationException(
                    "La verificació de TARIFAVE no coincideix."
                    + Environment.NewLine
                    + "Article: "
                    + articleBaseDades.Trim()
                    + Environment.NewLine
                    + "Tarifa: "
                    + tarifaBaseDades.Trim()
                    + Environment.NewLine
                    + "Preu esperat: "
                    + preuArrodonit.ToString(
                        "N" + decimalsPreu,
                        CultureInfo.CurrentCulture)
                    + Environment.NewLine
                    + "Preu obtingut: "
                    + preuObtingut.ToString(
                        "N" + decimalsPreu,
                        CultureInfo.CurrentCulture));
            }
        }

        /// <summary>
        /// Utilitza el procediment oficial d'a3ERP.
        ///
        /// OleDb treballa per posició, no pel nom
        /// textual dels paràmetres. Cal conservar
        /// exactament aquest ordre.
        /// </summary>
        private static void ExecutarActualizarTarifa(
            OleDbConnection connexio,
            OleDbTransaction transaccio,
            string tarifaBaseDades,
            string articleBaseDades,
            string monedaBaseDades,
            decimal preu)
        {
            const string sql = @"
EXEC dbo.ActualizarTarifa
    ?, ?, ?, ?, ?, ?, ?;";

            using (var comanda =
                new OleDbCommand(
                    sql,
                    connexio,
                    transaccio))
            {
                comanda.Parameters.Add(
                    "@Tarifa",
                    OleDbType.VarChar,
                    8)
                    .Value =
                        tarifaBaseDades;

                comanda.Parameters.Add(
                    "@CodArt",
                    OleDbType.VarChar,
                    15)
                    .Value =
                        articleBaseDades;

                comanda.Parameters.Add(
                    "@CodMon",
                    OleDbType.VarChar,
                    8)
                    .Value =
                        monedaBaseDades;

                comanda.Parameters.Add(
                    "@FecMin",
                    OleDbType.DBTimeStamp)
                    .Value =
                        DataMinima;

                comanda.Parameters.Add(
                    "@FecMax",
                    OleDbType.DBTimeStamp)
                    .Value =
                        DataMaxima;

                comanda.Parameters.Add(
                    "@Unidades",
                    OleDbType.Double)
                    .Value =
                        UnitatsGenerals;

                comanda.Parameters.Add(
                    "@Precio",
                    OleDbType.Double)
                    .Value =
                        Convert.ToDouble(
                            preu,
                            CultureInfo.InvariantCulture);

                comanda.ExecuteNonQuery();
            }
        }

        /// <summary>
        /// Llegeix el preu que acaba d'aplicar-se
        /// dins de la mateixa transacció.
        /// </summary>
        private static decimal LlegirPreuTarifa(
            OleDbConnection connexio,
            OleDbTransaction transaccio,
            string tarifaBaseDades,
            string articleBaseDades,
            string monedaBaseDades)
        {
            const string sql = @"
SELECT PRECIO
FROM dbo.TARIFAVE
WHERE TARIFA = ?
  AND CODART = ?
  AND CODMON = ?
  AND FECMIN = ?
  AND FECMAX = ?
  AND UNIDADES = ?;";

            using (var comanda =
                new OleDbCommand(
                    sql,
                    connexio,
                    transaccio))
            {
                comanda.Parameters.Add(
                    "@Tarifa",
                    OleDbType.VarChar,
                    8)
                    .Value =
                        tarifaBaseDades;

                comanda.Parameters.Add(
                    "@CodArt",
                    OleDbType.VarChar,
                    15)
                    .Value =
                        articleBaseDades;

                comanda.Parameters.Add(
                    "@CodMon",
                    OleDbType.VarChar,
                    8)
                    .Value =
                        monedaBaseDades;

                comanda.Parameters.Add(
                    "@FecMin",
                    OleDbType.DBTimeStamp)
                    .Value =
                        DataMinima;

                comanda.Parameters.Add(
                    "@FecMax",
                    OleDbType.DBTimeStamp)
                    .Value =
                        DataMaxima;

                comanda.Parameters.Add(
                    "@Unidades",
                    OleDbType.Double)
                    .Value =
                        UnitatsGenerals;

                object valor =
                    comanda.ExecuteScalar();

                if (valor == null ||
                    valor == DBNull.Value)
                {
                    throw new InvalidOperationException(
                        "No s'ha trobat la tarifa després "
                        + "d'executar dbo.ActualizarTarifa."
                        + Environment.NewLine
                        + "Article: "
                        + articleBaseDades.Trim()
                        + Environment.NewLine
                        + "Tarifa: "
                        + tarifaBaseDades.Trim());
                }

                return Convert.ToDecimal(
                    valor,
                    CultureInfo.InvariantCulture);
            }
        }

        /// <summary>
        /// Recupera els codis literals de les
        /// tarifes 1–6 des de dbo.TARIFAS.
        /// </summary>
        private static Dictionary<int, string>
            ObtenirTarifesBaseDades(
                OleDbConnection connexio,
                OleDbTransaction transaccio)
        {
            var resultat =
                new Dictionary<int, string>();

            for (int tarifa = 1;
                 tarifa <= 6;
                 tarifa++)
            {
                resultat.Add(
                    tarifa,
                    ObtenirTarifaBaseDades(
                        connexio,
                        transaccio,
                        tarifa));
            }

            return resultat;
        }

        private static string ObtenirTarifaBaseDades(
            OleDbConnection connexio,
            OleDbTransaction transaccio,
            int tarifaVisible)
        {
            const string sql = @"
SELECT TOP 1 TARIFA
FROM dbo.TARIFAS
WHERE LTRIM(RTRIM(TARIFA)) = ?;";

            using (var comanda =
                new OleDbCommand(
                    sql,
                    connexio,
                    transaccio))
            {
                comanda.Parameters.Add(
                    "@TarifaVisible",
                    OleDbType.VarChar,
                    8)
                    .Value =
                        tarifaVisible.ToString(
                            CultureInfo.InvariantCulture);

                object valor =
                    comanda.ExecuteScalar();

                if (valor == null ||
                    valor == DBNull.Value)
                {
                    throw new InvalidOperationException(
                        "No existeix la tarifa "
                        + tarifaVisible
                        + " a dbo.TARIFAS.");
                }

                return Convert.ToString(
                           valor,
                           CultureInfo.InvariantCulture)
                       ?? string.Empty;
            }
        }

        /// <summary>
        /// Recupera el codi literal de la moneda
        /// des de dbo.MONEDAS.
        /// </summary>
        private static string ObtenirMonedaBaseDades(
            OleDbConnection connexio,
            OleDbTransaction transaccio,
            string monedaVisible)
        {
            const string sql = @"
SELECT TOP 1 CODMON
FROM dbo.MONEDAS
WHERE LTRIM(RTRIM(CODMON)) = ?;";

            using (var comanda =
                new OleDbCommand(
                    sql,
                    connexio,
                    transaccio))
            {
                comanda.Parameters.Add(
                    "@MonedaVisible",
                    OleDbType.VarChar,
                    8)
                    .Value =
                        monedaVisible;

                object valor =
                    comanda.ExecuteScalar();

                if (valor == null ||
                    valor == DBNull.Value)
                {
                    throw new InvalidOperationException(
                        "No existeix la moneda "
                        + monedaVisible
                        + " a dbo.MONEDAS.");
                }

                return Convert.ToString(
                           valor,
                           CultureInfo.InvariantCulture)
                       ?? string.Empty;
            }
        }

        /// <summary>
        /// Simula els quatre descomptes per família
        /// de client d'un article.
        /// </summary>
        private static int ProcessarDescomptesArticle(
            OleDbConnection connexio,
            OleDbTransaction transaccio,
            IDictionary<int, string> familiesBaseDades,
            ResultatCalculTarifes resultat)
        {
            AplicarIVerificarDescompteAmbLog(
                connexio,
                transaccio,
                resultat.CodiArticleBaseDades,
                familiesBaseDades[1],
                resultat.DescompteGrup1);

            AplicarIVerificarDescompteAmbLog(
                connexio,
                transaccio,
                resultat.CodiArticleBaseDades,
                familiesBaseDades[2],
                resultat.DescompteGrup2);

            AplicarIVerificarDescompteAmbLog(
                connexio,
                transaccio,
                resultat.CodiArticleBaseDades,
                familiesBaseDades[3],
                resultat.DescompteGrup3);

            AplicarIVerificarDescompteAmbLog(
                connexio,
                transaccio,
                resultat.CodiArticleBaseDades,
                familiesBaseDades[4],
                resultat.DescompteGrup4);

            return 4;
        }

        private static int EliminarDescomptesGestionatsArticle(
            OleDbConnection connexio,
            OleDbTransaction transaccio,
            string articleBaseDades)
        {
            try
            {
                int filesEliminades =
                    ExecutarDeleteDescomptesGestionatsArticle(
                        connexio,
                        transaccio,
                        articleBaseDades);

                // Només registrem el detall si realment s'ha eliminat algun registre.
                if (filesEliminades > 0)
                {
                    CalculadorTarifesLogger.Informacio(
                        "Repositori: descomptes gestionats eliminats.",
                        new Dictionary<string, object>
                        {
                            {
                                "CODART",
                                articleBaseDades ?? string.Empty
                            },
                            {
                                "GeneraDescomptes",
                                false
                            },
                            {
                                "DescomptesEliminats",
                                filesEliminades
                            }
                        });
                }

                return filesEliminades;
            }
            catch (Exception ex)
            {
                CalculadorTarifesLogger.Error(
                    "Repositori: error eliminant descomptes gestionats.",
                    ex,
                    new Dictionary<string, object>
                    {
                        {
                            "CODART",
                            articleBaseDades ?? string.Empty
                        },
                        {
                            "GeneraDescomptes",
                            false
                        }
                    });

                throw;
            }
        }

        private static int ExecutarDeleteDescomptesGestionatsArticle(
            OleDbConnection connexio,
            OleDbTransaction transaccio,
            string articleBaseDades)
        {
            const string sql = @"
                DELETE FROM dbo.DESCUENT
                WHERE CODART = ?
                  AND TIPREG = 'AF'
                  AND LTRIM(RTRIM(FAMCLI)) IN ('1', '2', '3', '4')
                  AND UNIDADES = 0
                  AND FECMIN = ?
                  AND FECMAX = ?;";

            using (var comanda =
                new OleDbCommand(
                    sql,
                    connexio,
                    transaccio))
            {
                comanda.Parameters.Add(
                    "@CodArt",
                    OleDbType.VarChar,
                    15)
                    .Value =
                        articleBaseDades;

                comanda.Parameters.Add(
                    "@FecMin",
                    OleDbType.DBTimeStamp)
                    .Value =
                        DataMinima;

                comanda.Parameters.Add(
                    "@FecMax",
                    OleDbType.DBTimeStamp)
                    .Value =
                        DataMaxima;

                return comanda.ExecuteNonQuery();
            }
        }

        /// <summary>
        /// Aplica temporalment un descompte mitjançant
        /// dbo.ActualizarDescArtFam i comprova el registre.
        /// </summary>
        private static void AplicarIVerificarDescompteAmbLog(
            OleDbConnection connexio,
            OleDbTransaction transaccio,
            string articleBaseDades,
            string familiaBaseDades,
            decimal descompte)
        {
            try
            {
                AplicarIVerificarDescompte(
                    connexio,
                    transaccio,
                    articleBaseDades,
                    familiaBaseDades,
                    descompte);
            }
            catch (Exception ex)
            {
                CalculadorTarifesLogger.Error(
                    "Repositori: error processant DESCUENT.",
                    ex,
                    new Dictionary<string, object>
                    {
                        {
                            "Article",
                            (articleBaseDades ?? string.Empty).Trim()
                        },
                        {
                            "FamiliaClient",
                            (familiaBaseDades ?? string.Empty).Trim()
                        }
                    });

                throw;
            }
        }

        private static void AplicarIVerificarDescompte(
            OleDbConnection connexio,
            OleDbTransaction transaccio,
            string articleBaseDades,
            string familiaBaseDades,
            decimal descompte)
        {
            decimal descompteArrodonit =
                Math.Round(
                    descompte,
                    4,
                    MidpointRounding.AwayFromZero);

            if (descompteArrodonit < 0 ||
                descompteArrodonit > 100)
            {
                throw new InvalidOperationException(
                    "El descompte ha d'estar entre 0 i 100."
                    + Environment.NewLine
                    + "Article: "
                    + articleBaseDades.Trim()
                    + Environment.NewLine
                    + "Grup de client: "
                    + familiaBaseDades.Trim()
                    + Environment.NewLine
                    + "Descompte: "
                    + descompteArrodonit.ToString(
                        "N4",
                        CultureInfo.CurrentCulture));
            }

            ExecutarActualizarDescArtFam(
                connexio,
                transaccio,
                articleBaseDades,
                familiaBaseDades,
                descompteArrodonit);

            decimal[] descomptesLlegits =
                LlegirDescompteArticleFamilia(
                    connexio,
                    transaccio,
                    articleBaseDades,
                    familiaBaseDades);

            decimal descompte1Llegit =
                Math.Round(
                    descomptesLlegits[0],
                    4,
                    MidpointRounding.AwayFromZero);

            decimal descompte2Llegit =
                Math.Round(
                    descomptesLlegits[1],
                    4,
                    MidpointRounding.AwayFromZero);

            decimal descompte3Llegit =
                Math.Round(
                    descomptesLlegits[2],
                    4,
                    MidpointRounding.AwayFromZero);

            decimal descompte4Llegit =
                Math.Round(
                    descomptesLlegits[3],
                    4,
                    MidpointRounding.AwayFromZero);

            if (descompte1Llegit !=
                    descompteArrodonit
                ||
                descompte2Llegit != 0
                ||
                descompte3Llegit != 0
                ||
                descompte4Llegit != 0)
            {
                throw new InvalidOperationException(
                    "La verificació de DESCUENT no coincideix."
                    + Environment.NewLine
                    + "Article: "
                    + articleBaseDades.Trim()
                    + Environment.NewLine
                    + "Grup de client: "
                    + familiaBaseDades.Trim()
                    + Environment.NewLine
                    + "DESC1 esperat: "
                    + descompteArrodonit.ToString(
                        "N4",
                        CultureInfo.CurrentCulture)
                    + Environment.NewLine
                    + "DESC1 obtingut: "
                    + descompte1Llegit.ToString(
                        "N4",
                        CultureInfo.CurrentCulture));
            }
        }

        /// <summary>
        /// Executa el procediment oficial d'a3ERP
        /// per al descompte article-família de client.
        ///
        /// OleDb interpreta els paràmetres per posició.
        /// </summary>
        private static void ExecutarActualizarDescArtFam(
            OleDbConnection connexio,
            OleDbTransaction transaccio,
            string articleBaseDades,
            string familiaBaseDades,
            decimal descompte)
        {
            const string sql = @"
EXEC dbo.ActualizarDescArtFam
    ?, ?, ?, ?, ?, ?, ?, ?, ?;";

            using (var comanda =
                new OleDbCommand(
                    sql,
                    connexio,
                    transaccio))
            {
                comanda.Parameters.Add(
                    "@CodArt",
                    OleDbType.VarChar,
                    15)
                    .Value =
                        articleBaseDades;

                comanda.Parameters.Add(
                    "@FamCli",
                    OleDbType.VarChar,
                    8)
                    .Value =
                        familiaBaseDades;

                comanda.Parameters.Add(
                    "@FecMin",
                    OleDbType.DBTimeStamp)
                    .Value =
                        DataMinima;

                comanda.Parameters.Add(
                    "@FecMax",
                    OleDbType.DBTimeStamp)
                    .Value =
                        DataMaxima;

                comanda.Parameters.Add(
                    "@Unidades",
                    OleDbType.Double)
                    .Value =
                        UnitatsGenerals;

                comanda.Parameters.Add(
                    "@Desc1",
                    OleDbType.Double)
                    .Value =
                        Convert.ToDouble(
                            descompte,
                            CultureInfo.InvariantCulture);

                comanda.Parameters.Add(
                    "@Desc2",
                    OleDbType.Double)
                    .Value =
                        0d;

                comanda.Parameters.Add(
                    "@Desc3",
                    OleDbType.Double)
                    .Value =
                        0d;

                comanda.Parameters.Add(
                    "@Desc4",
                    OleDbType.Double)
                    .Value =
                        0d;

                comanda.ExecuteNonQuery();
            }
        }

        /// <summary>
        /// Llegeix els quatre camps de descompte
        /// dins de la mateixa transacció.
        /// </summary>
        private static decimal[]
            LlegirDescompteArticleFamilia(
                OleDbConnection connexio,
                OleDbTransaction transaccio,
                string articleBaseDades,
                string familiaBaseDades)
        {
            const string sql = @"
SELECT
    DESC1,
    DESC2,
    DESC3,
    DESC4
FROM dbo.DESCUENT
WHERE TIPREG = 'AF'
  AND CODART = ?
  AND FAMCLI = ?
  AND FECMIN = ?
  AND FECMAX = ?
  AND UNIDADES = ?;";

            using (var comanda =
                new OleDbCommand(
                    sql,
                    connexio,
                    transaccio))
            {
                comanda.Parameters.Add(
                    "@CodArt",
                    OleDbType.VarChar,
                    15)
                    .Value =
                        articleBaseDades;

                comanda.Parameters.Add(
                    "@FamCli",
                    OleDbType.VarChar,
                    8)
                    .Value =
                        familiaBaseDades;

                comanda.Parameters.Add(
                    "@FecMin",
                    OleDbType.DBTimeStamp)
                    .Value =
                        DataMinima;

                comanda.Parameters.Add(
                    "@FecMax",
                    OleDbType.DBTimeStamp)
                    .Value =
                        DataMaxima;

                comanda.Parameters.Add(
                    "@Unidades",
                    OleDbType.Double)
                    .Value =
                        UnitatsGenerals;

                using (OleDbDataReader lector =
                    comanda.ExecuteReader())
                {
                    if (lector == null ||
                        !lector.Read())
                    {
                        throw new InvalidOperationException(
                            "No s'ha trobat el descompte després "
                            + "d'executar dbo.ActualizarDescArtFam."
                            + Environment.NewLine
                            + "Article: "
                            + articleBaseDades.Trim()
                            + Environment.NewLine
                            + "Grup de client: "
                            + familiaBaseDades.Trim());
                    }

                    return new[]
                    {
                lector.IsDBNull(0)
                    ? 0m
                    : Convert.ToDecimal(
                        lector.GetValue(0),
                        CultureInfo.InvariantCulture),

                lector.IsDBNull(1)
                    ? 0m
                    : Convert.ToDecimal(
                        lector.GetValue(1),
                        CultureInfo.InvariantCulture),

                lector.IsDBNull(2)
                    ? 0m
                    : Convert.ToDecimal(
                        lector.GetValue(2),
                        CultureInfo.InvariantCulture),

                lector.IsDBNull(3)
                    ? 0m
                    : Convert.ToDecimal(
                        lector.GetValue(3),
                        CultureInfo.InvariantCulture)
            };
                }
            }
        }

        /// <summary>
        /// Recupera els codis literals de les famílies
        /// de descompte de client 1–4.
        /// </summary>
        private static Dictionary<int, string>
            ObtenirFamiliesDescompteBaseDades(
                OleDbConnection connexio,
                OleDbTransaction transaccio)
        {
            var resultat =
                new Dictionary<int, string>();

            for (int grup = 1;
                 grup <= 4;
                 grup++)
            {
                resultat.Add(
                    grup,
                    ObtenirFamiliaDescompteBaseDades(
                        connexio,
                        transaccio,
                        grup));
            }

            return resultat;
        }

        private static string
            ObtenirFamiliaDescompteBaseDades(
                OleDbConnection connexio,
                OleDbTransaction transaccio,
                int grupVisible)
        {
            const string sql = @"
                SELECT TOP 1
                    CODFAM
                FROM dbo.FAMILIAS
                WHERE LTRIM(RTRIM(CODFAM)) = ?
                  AND UPPER(LTRIM(RTRIM(FICHERO))) = 'DESCCLI';";

            using (var comanda =
                new OleDbCommand(
                    sql,
                    connexio,
                    transaccio))
            {
                comanda.Parameters.Add(
                    "@GrupVisible",
                    OleDbType.VarChar,
                    8)
                    .Value =
                        grupVisible.ToString(
                            CultureInfo.InvariantCulture);

                object valor =
                    comanda.ExecuteScalar();

                if (valor == null ||
                    valor == DBNull.Value)
                {
                    throw new InvalidOperationException(
                        "No existeix la família de descompte "
                        + "de client "
                        + grupVisible
                        + " a dbo.FAMILIAS.");
                }

                return Convert.ToString(
                           valor,
                           CultureInfo.InvariantCulture)
                       ?? string.Empty;
            }
        }

        /// <summary>
        /// Valida el lot abans d'obrir
        /// cap transacció d'aplicació.
        /// </summary>
        private static Dictionary<string, object> CrearCampsLot(
            IEnumerable<ResultatCalculTarifes> resultats)
        {
            List<ResultatCalculTarifes> lot =
                resultats == null
                    ? new List<ResultatCalculTarifes>()
                    : resultats
                        .Where(
                            resultat =>
                                resultat != null)
                        .ToList();

            int descomptes =
                lot.Count(
                    resultat =>
                        resultat.GeneraDescomptes)
                * 4;

            int descomptesEliminats =
                lot.Count(
                    resultat =>
                        !resultat.GeneraDescomptes)
                * 4;

            return CrearCampsProces(
                lot.Count,
                lot.Count * 6,
                descomptes,
                descomptesEliminats);
        }

        private static Dictionary<string, object> CrearCampsProces(
            int articles,
            int tarifes,
            int descomptes,
            int descomptesEliminats)
        {
            return new Dictionary<string, object>
            {
                {
                    "Articles",
                    articles
                },
                {
                    "Tarifes",
                    tarifes
                },
                {
                    "Descomptes",
                    descomptes
                },
                {
                    "DescomptesEliminats",
                    descomptesEliminats
                }
            };
        }

        private static void NotificarProgres(
            IProgress<ProgresAplicacioTarifes> progres,
            int processats,
            int total,
            ref int ultimPercentatgeNotificat)
        {
            if (progres == null ||
                total <= 0)
            {
                return;
            }

            int percentatge =
                processats * 100 / total;

            percentatge =
                Math.Max(
                    0,
                    Math.Min(
                        100,
                        percentatge));

            if (percentatge ==
                ultimPercentatgeNotificat)
            {
                return;
            }

            ultimPercentatgeNotificat =
                percentatge;

            progres.Report(
                new ProgresAplicacioTarifes
                {
                    Processats =
                        processats,

                    Total =
                        total,

                    Percentatge =
                        percentatge
                });
        }

        private static void ValidarLot(
            IEnumerable<ResultatCalculTarifes> resultats)
        {
            if (resultats == null)
            {
                throw new ArgumentNullException(
                    nameof(resultats));
            }

            List<ResultatCalculTarifes> lot =
                resultats.ToList();

            if (lot.Count == 0)
            {
                throw new InvalidOperationException(
                    "No hi ha cap resultat per simular.");
            }

            foreach (
                ResultatCalculTarifes resultat
                in lot)
            {
                if (resultat == null)
                {
                    throw new InvalidOperationException(
                        "El lot conté un resultat nul.");
                }

                if (!resultat.Correcte)
                {
                    throw new InvalidOperationException(
                        "No es poden aplicar resultats "
                        + "que contenen errors.");
                }

                if (string.IsNullOrWhiteSpace(
                    resultat.CodiArticleBaseDades))
                {
                    throw new InvalidOperationException(
                        "L'article "
                        + resultat.CodiArticle
                        + " no conserva el seu CODART literal.");
                }
            }
        }

        private static void IntentarRollback(
            OleDbTransaction transaccio,
            string origen)
        {
            if (transaccio == null)
            {
                return;
            }

            try
            {
                transaccio.Rollback();

                CalculadorTarifesLogger.Informacio(
                    "Repositori: ROLLBACK executat.",
                    new Dictionary<string, object>
                    {
                        {
                            "Origen",
                            origen ?? string.Empty
                        }
                    });
            }
            catch (Exception ex)
            {
                CalculadorTarifesLogger.Error(
                    "Repositori: error executant ROLLBACK.",
                    ex,
                    new Dictionary<string, object>
                    {
                        {
                            "Origen",
                            origen ?? string.Empty
                        }
                    });

                /*
                 * No ocultem l'error original.
                 * El procés retornarà l'error principal.
                 */
            }
        }

        /// <summary>
        /// Obté el nombre de decimals configurat
        /// per als preus de l'empresa activa.
        /// </summary>
        private static int ObtenirDecimalsPreu(
            OleDbConnection connexio,
            OleDbTransaction transaccio)
        {
            const string sql = @"
SELECT TOP 1
    NUMDECPRC
FROM dbo.DATOSCONFIG;";

            using (var comanda =
                new OleDbCommand(
                    sql,
                    connexio,
                    transaccio))
            {
                object valor =
                    comanda.ExecuteScalar();

                if (valor == null ||
                    valor == DBNull.Value)
                {
                    throw new InvalidOperationException(
                        "No s'ha pogut obtenir NUMDECPRC "
                        + "de dbo.DATOSCONFIG.");
                }

                int decimals =
                    Convert.ToInt32(
                        valor,
                        CultureInfo.InvariantCulture);

                if (decimals < 0 ||
                    decimals > 8)
                {
                    throw new InvalidOperationException(
                        "El valor NUMDECPRC no és vàlid: "
                        + decimals);
                }

                return decimals;
            }
        }
    }
}
