using ADODB;
using MAT0943Net.TraspasFacturas.Infrastructure.Connections;
using MAT0943Net.TraspasFacturas.Infrastructure.Logging;
using MAT0943Net.TraspasFacturas.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.OleDb;
using System.Globalization;
using System.Runtime.InteropServices;

namespace MAT0943Net.TraspasFacturas.Services
{
    internal sealed class ConsultaFacturasOrigenService
    {
        private const string ClauUsuariActiveX =
            "TraspasFacturas_ActiveXUsuario";

        private const string ClauPasswordActiveX =
            "TraspasFacturas_ActiveXPassword";

        public const int LimiteFacturas =
            200;

        public List<FacturaOrigenDto> Consultar(
            string baseDatosOrigen,
            DateTime? fechaDesde,
            DateTime? fechaHasta,
            string texto,
            string tipo,
            string estado)
        {
            string filtroTexto =
                NormalizarFiltroTexto(
                    texto);

            string filtroTipo =
                NormalizarFiltroTipo(
                    tipo);

            string filtroEstado =
                NormalizarFiltroEstado(
                    estado);

            string sql =
                CrearSql(
                    fechaDesde.HasValue,
                    fechaHasta.HasValue,
                    !string.IsNullOrWhiteSpace(
                        filtroTexto),
                    filtroTipo,
                    filtroEstado);

            List<FacturaOrigenDto> facturas;

            if (IntentarConsultarViaActiveX(
                    baseDatosOrigen,
                    fechaDesde,
                    fechaHasta,
                    filtroTexto,
                    sql,
                    out facturas))
            {
                return facturas;
            }

            string conexion =
                TraspasFacturasConnectionStringHelper
                    .CrearConnexioOrigen(
                        baseDatosOrigen);

            facturas =
                new List<FacturaOrigenDto>();

            using (var connection =
                   new OleDbConnection(conexion))
            using (var command =
                   new OleDbCommand(
                       sql,
                       connection))
            {
                command.CommandType =
                    CommandType.Text;

                command.CommandTimeout =
                    60;

                /*
                 * IMPORTANT:
                 * OleDb associa els paràmetres
                 * estrictament per posició.
                 *
                 * L'ordre d'aquests Add ha de coincidir
                 * amb l'ordre dels ? de CrearSql().
                 */

                if (fechaDesde.HasValue)
                {
                    command.Parameters
                        .Add(
                            "FECHA_DESDE",
                            OleDbType.Date)
                        .Value =
                            fechaDesde.Value.Date;
                }

                if (fechaHasta.HasValue)
                {
                    command.Parameters
                        .Add(
                            "FECHA_HASTA",
                            OleDbType.Date)
                        .Value =
                            fechaHasta.Value.Date
                                .AddDays(1);
                }

                if (!string.IsNullOrWhiteSpace(
                        filtroTexto))
                {
                    string textoLike =
                        "%" +
                        filtroTexto +
                        "%";

                    /*
                     * 1. Codi client
                     */
                    command.Parameters
                        .Add(
                            "CODCLI",
                            OleDbType.VarWChar)
                        .Value =
                            textoLike;

                    /*
                     * 2. Nom client
                     */
                    command.Parameters
                        .Add(
                            "NOMCLI",
                            OleDbType.VarWChar)
                        .Value =
                            textoLike;

                    /*
                     * 3. Número factura
                     */
                    command.Parameters
                        .Add(
                            "NUMDOC",
                            OleDbType.VarWChar)
                        .Value =
                            textoLike;

                    /*
                     * 4. Sèrie
                     */
                    command.Parameters
                        .Add(
                            "SERIE",
                            OleDbType.VarWChar)
                        .Value =
                            textoLike;

                    /*
                     * 5. Factura completa.
                     * Exemple: 2026-836
                     */
                    command.Parameters
                        .Add(
                            "FACTURA",
                            OleDbType.VarWChar)
                        .Value =
                            textoLike;
                }

                connection.Open();

                using (OleDbDataReader reader =
                       command.ExecuteReader())
                {
                    while (reader != null &&
                           reader.Read())
                    {
                        facturas.Add(
                            MapearFactura(
                                reader));
                    }
                }
            }

            TraspasFacturasLogger.Informacio(
                "Connexió utilitzada=OleDb/.config.",
                new Dictionary<string, object>
                {
                    { "BaseDadesOrigen", baseDatosOrigen ?? string.Empty },
                    { "FacturesRetornades", facturas.Count }
                });

            return facturas;
        }

        private static bool IntentarConsultarViaActiveX(
            string baseDatosOrigen,
            DateTime? fechaDesde,
            DateTime? fechaHasta,
            string filtroTexto,
            string sql,
            out List<FacturaOrigenDto> facturas)
        {
            a3ERPActiveX.Enlace enlace = null;
            Command command = null;
            Recordset recordset = null;
            facturas = null;

            try
            {
                TraspasFacturasLogger.Informacio(
                    "Intentant carregar CABEFACV via ActiveX/ADO.",
                    new Dictionary<string, object>
                    {
                        { "BaseDadesOrigen", baseDatosOrigen ?? string.Empty }
                    });

                string usuari =
                    ConfigurationManager.AppSettings[
                        ClauUsuariActiveX];

                if (string.IsNullOrWhiteSpace(usuari))
                {
                    throw new InvalidOperationException(
                        "No s'ha configurat l'usuari ActiveX.");
                }

                enlace =
                    new a3ERPActiveX.Enlace();

                enlace.RaiseOnException = true;

                bool loginCorrecte =
                    enlace.LoginUsuario(
                        usuari,
                        ConfigurationManager.AppSettings[
                            ClauPasswordActiveX] ?? string.Empty);

                if (!loginCorrecte)
                {
                    throw new InvalidOperationException(
                        "No s'ha pogut iniciar sessió a a3ERP ActiveX.");
                }

                string empresaActiveX =
                    ResoldreEmpresaActiveX(
                        enlace,
                        baseDatosOrigen);

                TraspasFacturasLogger.Informacio(
                    "Empresa origen resolta per a la connexió ActiveX/ADO.",
                    new Dictionary<string, object>
                    {
                        { "BaseDadesOrigen", baseDatosOrigen ?? string.Empty },
                        { "EmpresaOrigen", empresaActiveX }
                    });

                enlace.Iniciar(
                    empresaActiveX,
                    string.Empty);

                if (enlace.Estado !=
                    a3ERPActiveX.EstadoEnlace.estACTIVO)
                {
                    throw new InvalidOperationException(
                        "No s'ha pogut iniciar l'empresa origen via a3ERP ActiveX.");
                }

                Connection connexio =
                    enlace.GetConexionDB("EMPRESA");

                if (connexio == null)
                {
                    throw new InvalidOperationException(
                        "a3ERP ActiveX no ha retornat la connexió de l'empresa origen.");
                }

                command =
                    new Command
                    {
                        ActiveConnection = connexio,
                        CommandText = sql,
                        CommandType = CommandTypeEnum.adCmdText,
                        CommandTimeout = 60
                    };

                AfegirParametresAdo(
                    command,
                    fechaDesde,
                    fechaHasta,
                    filtroTexto);

                object filesAfectades =
                    Type.Missing;

                recordset =
                    command.Execute(
                        out filesAfectades);

                facturas =
                    new List<FacturaOrigenDto>();

                while (recordset != null &&
                       !recordset.EOF)
                {
                    facturas.Add(
                        MapearFactura(
                            recordset));

                    recordset.MoveNext();
                }

                TraspasFacturasLogger.Informacio(
                    "Connexió utilitzada=ActiveX/ADO.",
                    new Dictionary<string, object>
                    {
                        { "BaseDadesOrigen", baseDatosOrigen ?? string.Empty },
                        { "FacturesRetornades", facturas.Count }
                    });

                return true;
            }
            catch (Exception ex)
            {
                TraspasFacturasLogger.Advertencia(
                    "No s'ha pogut carregar CABEFACV via ActiveX/ADO. S'activa el fallback OleDb/.config.",
                    new Dictionary<string, object>
                    {
                        { "BaseDatosOrigen", baseDatosOrigen ?? string.Empty },
                        { "Error", ex.Message }
                    });

                facturas = null;
                return false;
            }
            finally
            {
                TancarRecordset(recordset);
                AlliberarComObject(recordset);
                AlliberarComObject(command);

                if (enlace != null)
                {
                    try
                    {
                        if (enlace.Estado ==
                            a3ERPActiveX.EstadoEnlace.estACTIVO)
                        {
                            enlace.Acabar();
                        }
                    }
                    catch
                    {
                        // No bloquegem el fallback si no es pot tancar l'Enlace.
                    }

                    AlliberarComObject(enlace);
                }
            }
        }

        private static void AfegirParametresAdo(
            Command command,
            DateTime? fechaDesde,
            DateTime? fechaHasta,
            string filtroTexto)
        {
            if (fechaDesde.HasValue)
            {
                AfegirParametreAdo(
                    command,
                    "FECHA_DESDE",
                    DataTypeEnum.adDate,
                    0,
                    fechaDesde.Value.Date);
            }

            if (fechaHasta.HasValue)
            {
                AfegirParametreAdo(
                    command,
                    "FECHA_HASTA",
                    DataTypeEnum.adDate,
                    0,
                    fechaHasta.Value.Date.AddDays(1));
            }

            if (string.IsNullOrWhiteSpace(filtroTexto))
            {
                return;
            }

            string textoLike =
                "%" +
                filtroTexto +
                "%";

            AfegirParametreTextAdo(command, "CODCLI", textoLike);
            AfegirParametreTextAdo(command, "NOMCLI", textoLike);
            AfegirParametreTextAdo(command, "NUMDOC", textoLike);
            AfegirParametreTextAdo(command, "SERIE", textoLike);
            AfegirParametreTextAdo(command, "FACTURA", textoLike);
        }

        private static void AfegirParametreTextAdo(
            Command command,
            string nom,
            string valor)
        {
            AfegirParametreAdo(
                command,
                nom,
                DataTypeEnum.adVarWChar,
                Math.Max(valor.Length, 1),
                valor);
        }

        private static void AfegirParametreAdo(
            Command command,
            string nom,
            DataTypeEnum tipus,
            int mida,
            object valor)
        {
            command.Parameters.Append(
                command.CreateParameter(
                    nom,
                    tipus,
                    ParameterDirectionEnum.adParamInput,
                    mida,
                    valor));
        }

        private static string ResoldreEmpresaActiveX(
            a3ERPActiveX.Enlace enlace,
            string baseDatosOrigen)
        {
            if (string.IsNullOrWhiteSpace(baseDatosOrigen))
            {
                throw new InvalidOperationException(
                    "No s'ha informat la base de dades origen.");
            }

            object resultat =
                enlace.Empresas();

            Array empreses =
                resultat as Array;

            if (empreses == null ||
                empreses.GetLowerBound(0) > 0 ||
                empreses.GetUpperBound(0) < 0)
            {
                throw new InvalidOperationException(
                    "Empresas() no ha retornat el format esperat.");
            }

            int total;

            if (!int.TryParse(
                    Convert.ToString(
                        empreses.GetValue(0),
                        CultureInfo.InvariantCulture),
                    out total) ||
                total < 0)
            {
                throw new InvalidOperationException(
                    "Empresas() no ha retornat un total d'empreses vàlid.");
            }

            int maxim =
                empreses.GetUpperBound(0);

            for (int index = 1;
                 index <= total && index <= maxim;
                 index++)
            {
                string empresa =
                    Convert.ToString(
                        empreses.GetValue(index),
                        CultureInfo.InvariantCulture)
                    ?.Trim();

                if (string.IsNullOrWhiteSpace(empresa))
                {
                    continue;
                }

                try
                {
                    string baseDadesEmpresa =
                        ObtenirBaseDadesEmpresa(
                            enlace,
                            empresa);

                    if (string.Equals(
                            baseDadesEmpresa,
                            baseDatosOrigen.Trim(),
                            StringComparison.OrdinalIgnoreCase))
                    {
                        return empresa;
                    }
                }
                catch
                {
                    // Una empresa invalida no ha de bloquejar la resolucio de l'origen.
                }
            }

            throw new InvalidOperationException(
                "No s'ha trobat cap empresa ActiveX per a la base de dades origen '" +
                baseDatosOrigen.Trim() +
                "'.");
        }

        private static string ObtenirBaseDadesEmpresa(
            a3ERPActiveX.Enlace enlace,
            string empresa)
        {
            Array valors =
                enlace.ParamConexion(empresa) as Array;

            if (valors == null)
            {
                throw new InvalidOperationException(
                    "ParamConexion no ha retornat el format esperat.");
            }

            int indexBaseDades =
                valors.GetLowerBound(0) + 5;

            if (indexBaseDades > valors.GetUpperBound(0))
            {
                throw new InvalidOperationException(
                    "ParamConexion no conté la base de dades de l'empresa.");
            }

            string baseDades =
                Convert.ToString(
                    valors.GetValue(indexBaseDades),
                    CultureInfo.InvariantCulture)
                ?.Trim();

            if (string.IsNullOrWhiteSpace(baseDades))
            {
                throw new InvalidOperationException(
                    "ParamConexion ha retornat una base de dades buida.");
            }

            return baseDades;
        }

        private static void TancarRecordset(
            Recordset recordset)
        {
            if (recordset == null)
            {
                return;
            }

            try
            {
                if (recordset.State ==
                    (int)ObjectStateEnum.adStateOpen)
                {
                    recordset.Close();
                }
            }
            catch
            {
                // El tancament del Recordset no ha de bloquejar el fallback.
            }
        }

        private static void AlliberarComObject(
            object objecteCom)
        {
            try
            {
                if (objecteCom != null &&
                    Marshal.IsComObject(objecteCom))
                {
                    Marshal.FinalReleaseComObject(objecteCom);
                }
            }
            catch
            {
                // L'alliberament COM no ha de bloquejar el fallback.
            }
        }

        private static string CrearSql(
            bool aplicaFechaDesde,
            bool aplicaFechaHasta,
            bool aplicaTexto,
            string tipo,
            string estado)
        {
            string sql = @"
                SELECT TOP 200
                    F.IDFACV,
                    LTRIM(RTRIM(COALESCE(F.SERIE, ''))) AS SERIE,
                    F.NUMDOC,
                    F.FECHA,
                    LTRIM(RTRIM(F.CODCLI)) AS CODCLI,
                    F.NOMCLI,
                    F.BASE,
                    F.TOTIVA,
                    F.TOTDOC,
                    F.RECTIFICATIVA,
                    F.BORRADOR,
                    CASE
                        WHEN EXISTS
                        (
                            SELECT 1
                            FROM dbo.AT_TRASPAS_FACTURES T
                            WHERE
                                T.IDFACV_ORIGEN = F.IDFACV
                                AND T.ESTADO = 'CREADA'
                        )
                        THEN 1
                        ELSE 0
                    END AS TRASPASSADA
                FROM dbo.CABEFACV F
                WHERE 1 = 1";

                /*
                 * FILTRES DE DATA
                 */

                        if (aplicaFechaDesde)
                        {
                            sql += @"
                AND F.FECHA >= ?";
                        }

                        if (aplicaFechaHasta)
                        {
                            sql += @"
                AND F.FECHA < ?";
                        }

            /*
             * CERCA GENERAL
             *
             * Busca simultàniament per:
             * - codi client
             * - nom client
             * - número factura
             * - sèrie
             * - factura completa, ex. 2026-836
             */

            if (aplicaTexto)
            {
                sql += @"
                  AND
                  (
                      LTRIM(RTRIM(COALESCE(F.CODCLI, ''))) LIKE ?
                      OR COALESCE(F.NOMCLI, '') LIKE ?
                      OR CONVERT(varchar(30), F.NUMDOC) LIKE ?
                      OR LTRIM(RTRIM(COALESCE(F.SERIE, ''))) LIKE ?
                      OR
                      (
                          LTRIM(RTRIM(COALESCE(F.SERIE, '')))
                          + '-'
                          + CONVERT(
                                varchar(30),
                                CONVERT(bigint, F.NUMDOC)
                            )
                      ) LIKE ?
                  )";
            }

            /*
             * TIPUS FACTURA
             */

            if (string.Equals(
                    tipo,
                    "Normal",
                    StringComparison.OrdinalIgnoreCase))
            {
                sql += @"
                  AND
                  (
                      F.RECTIFICATIVA IS NULL
                      OR LTRIM(RTRIM(F.RECTIFICATIVA)) <> 'T'
                  )";
            }
            else if (string.Equals(
                         tipo,
                         "Rectificativa",
                         StringComparison.OrdinalIgnoreCase))
            {
                sql += @"
                AND LTRIM(RTRIM(COALESCE(F.RECTIFICATIVA, ''))) = 'T'";
            }

            /*
             * ESTAT DEL TRASPÀS
             */

            if (string.Equals(
                    estado,
                    "Pendents",
                    StringComparison.OrdinalIgnoreCase))
            {
                sql += @"
                  AND NOT EXISTS
                  (
                      SELECT 1
                      FROM dbo.AT_TRASPAS_FACTURES T
                      WHERE
                          T.IDFACV_ORIGEN = F.IDFACV
                          AND T.ESTADO = 'CREADA'
                  )";
            }
            else if (string.Equals(
                         estado,
                         "Traspassades",
                         StringComparison.OrdinalIgnoreCase))
            {
                sql += @"
                  AND EXISTS
                  (
                      SELECT 1
                      FROM dbo.AT_TRASPAS_FACTURES T
                      WHERE
                          T.IDFACV_ORIGEN = F.IDFACV
                          AND T.ESTADO = 'CREADA'
                  )";
            }

            sql += @"
                ORDER BY
                    F.FECHA DESC,
                    F.IDFACV DESC;";

            return sql;
        }

        private static FacturaOrigenDto MapearFactura(
            OleDbDataReader reader)
        {
            return new FacturaOrigenDto
            {
                IdFacv =
                    LeerDecimal(
                        reader,
                        "IDFACV"),

                Serie =
                    LeerTexto(
                        reader,
                        "SERIE"),

                NumDoc =
                    LeerDecimal(
                        reader,
                        "NUMDOC"),

                Fecha =
                    LeerFechaNullable(
                        reader,
                        "FECHA"),

                CodCli =
                    LeerTexto(
                        reader,
                        "CODCLI"),

                NomCli =
                    LeerTexto(
                        reader,
                        "NOMCLI"),

                Base =
                    LeerDecimal(
                        reader,
                        "BASE"),

                Iva =
                    LeerDecimal(
                        reader,
                        "TOTIVA"),

                Total =
                    LeerDecimal(
                        reader,
                        "TOTDOC"),

                Rectificativa =
                    LeerTexto(
                        reader,
                        "RECTIFICATIVA"),

                Borrador =
                    LeerDecimal(
                        reader,
                        "BORRADOR"),

                Traspassada =
                    Convert.ToInt32(
                        reader["TRASPASSADA"]) == 1
            };
        }

        private static FacturaOrigenDto MapearFactura(
            Recordset reader)
        {
            return new FacturaOrigenDto
            {
                IdFacv =
                    LeerDecimal(
                        reader,
                        "IDFACV"),

                Serie =
                    LeerTexto(
                        reader,
                        "SERIE"),

                NumDoc =
                    LeerDecimal(
                        reader,
                        "NUMDOC"),

                Fecha =
                    LeerFechaNullable(
                        reader,
                        "FECHA"),

                CodCli =
                    LeerTexto(
                        reader,
                        "CODCLI"),

                NomCli =
                    LeerTexto(
                        reader,
                        "NOMCLI"),

                Base =
                    LeerDecimal(
                        reader,
                        "BASE"),

                Iva =
                    LeerDecimal(
                        reader,
                        "TOTIVA"),

                Total =
                    LeerDecimal(
                        reader,
                        "TOTDOC"),

                Rectificativa =
                    LeerTexto(
                        reader,
                        "RECTIFICATIVA"),

                Borrador =
                    LeerDecimal(
                        reader,
                        "BORRADOR"),

                Traspassada =
                    Convert.ToInt32(
                        LeerValor(
                            reader,
                            "TRASPASSADA")) == 1
            };
        }

        private static string NormalizarFiltroTexto(
            string texto)
        {
            return texto == null
                ? string.Empty
                : texto.Trim();
        }

        private static string NormalizarFiltroTipo(
            string tipo)
        {
            string valor =
                (tipo ?? string.Empty)
                .Trim();

            if (string.Equals(
                    valor,
                    "Normal",
                    StringComparison.OrdinalIgnoreCase))
            {
                return "Normal";
            }

            if (string.Equals(
                    valor,
                    "Rectificativa",
                    StringComparison.OrdinalIgnoreCase))
            {
                return "Rectificativa";
            }

            return "Tots";
        }

        private static string NormalizarFiltroEstado(
            string estado)
        {
            string valor =
                (estado ?? string.Empty)
                .Trim();

            if (string.Equals(
                    valor,
                    "Pendents",
                    StringComparison.OrdinalIgnoreCase))
            {
                return "Pendents";
            }

            if (string.Equals(
                    valor,
                    "Traspassades",
                    StringComparison.OrdinalIgnoreCase))
            {
                return "Traspassades";
            }

            return "Totes";
        }

        private static string LeerTexto(
            OleDbDataReader reader,
            string campo)
        {
            object valor =
                reader[campo];

            return valor == DBNull.Value
                ? string.Empty
                : Convert.ToString(valor)?.Trim()
                  ?? string.Empty;
        }

        private static string LeerTexto(
            Recordset reader,
            string campo)
        {
            object valor =
                LeerValor(
                    reader,
                    campo);

            return valor == DBNull.Value ||
                   valor == null
                ? string.Empty
                : Convert.ToString(valor)?.Trim()
                  ?? string.Empty;
        }

        private static decimal LeerDecimal(
            OleDbDataReader reader,
            string campo)
        {
            object valor =
                reader[campo];

            return valor == DBNull.Value
                ? 0m
                : Convert.ToDecimal(valor);
        }

        private static decimal LeerDecimal(
            Recordset reader,
            string campo)
        {
            object valor =
                LeerValor(
                    reader,
                    campo);

            return valor == DBNull.Value ||
                   valor == null
                ? 0m
                : Convert.ToDecimal(valor);
        }

        private static DateTime? LeerFechaNullable(
            OleDbDataReader reader,
            string campo)
        {
            object valor =
                reader[campo];

            if (valor == DBNull.Value)
            {
                return null;
            }

            return Convert.ToDateTime(
                valor);
        }

        private static DateTime? LeerFechaNullable(
            Recordset reader,
            string campo)
        {
            object valor =
                LeerValor(
                    reader,
                    campo);

            if (valor == DBNull.Value ||
                valor == null)
            {
                return null;
            }

            return Convert.ToDateTime(
                valor);
        }

        private static object LeerValor(
            Recordset reader,
            string campo)
        {
            object valor =
                reader.Fields[campo].Value;

            return valor ??
                   DBNull.Value;
        }
    }
}
