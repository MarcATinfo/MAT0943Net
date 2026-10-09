using MAT0943Net.TraspasFacturas.Infrastructure.Connections;
using MAT0943Net.TraspasFacturas.Models;
using System;
using System.Data;
using System.Data.OleDb;

namespace MAT0943Net.TraspasFacturas.Services
{
    internal sealed class ServeiVincleTraspasFactures
    {
        private const string EstadoPendiente = "PENDIENTE";
        private const string EstadoCreada = "CREADA";
        private const string EstadoError = "ERROR";

        public void PrepararTraspaso(
            string baseDatosOrigen,
            decimal idFacvOrigen,
            string empresaDestino,
            string baseDatosDestino)
        {
            ValidarParametros(
                baseDatosOrigen,
                idFacvOrigen,
                empresaDestino);

            if (ServeiVincleTraspasFacturesActiveX.IntentarPrepararTraspaso(
                    baseDatosOrigen,
                    idFacvOrigen,
                    empresaDestino,
                    baseDatosDestino))
            {
                return;
            }

            string conexion =
                TraspasFacturasConnectionStringHelper
                    .CrearConnexioBaseDades(
                        baseDatosOrigen);

            using (var connection =
                   new OleDbConnection(conexion))
            {
                connection.Open();

                string estadoActual =
                    ObtenerEstado(
                        connection,
                        baseDatosOrigen,
                        idFacvOrigen);

                if (string.Equals(
                        estadoActual,
                        EstadoCreada,
                        StringComparison.OrdinalIgnoreCase))
                {
                    throw new InvalidOperationException(
                        "La factura origen ja ha estat traspassada " +
                        "anteriorment i no es pot tornar a traspassar.");
                }

                if (string.Equals(
                        estadoActual,
                        EstadoPendiente,
                        StringComparison.OrdinalIgnoreCase))
                {
                    throw new InvalidOperationException(
                        "La factura origen ja té un traspàs pendent.");
                }

                if (string.Equals(
                        estadoActual,
                        EstadoError,
                        StringComparison.OrdinalIgnoreCase))
                {
                    ReutilizarRegistroError(
                        connection,
                        baseDatosOrigen,
                        idFacvOrigen,
                        empresaDestino,
                        baseDatosDestino);

                    return;
                }

                InsertarPendiente(
                    connection,
                    baseDatosOrigen,
                    idFacvOrigen,
                    empresaDestino,
                    baseDatosDestino);
            }

            ServeiVincleTraspasFacturesActiveX.RegistrarConnexioOleDb(
                nameof(PrepararTraspaso),
                baseDatosOrigen);
        }

        public bool EstaTraspassada(
            string baseDatosOrigen,
            decimal idFacvOrigen)
        {
            if (string.IsNullOrWhiteSpace(baseDatosOrigen))
            {
                throw new InvalidOperationException(
                    "No s'ha informat la base de dades origen.");
            }

            if (idFacvOrigen <= 0)
            {
                throw new InvalidOperationException(
                    "L'ID de la factura origen no és vàlid.");
            }

            bool estaTraspassada;
            if (ServeiVincleTraspasFacturesActiveX.IntentarEstaTraspassada(
                    baseDatosOrigen,
                    idFacvOrigen,
                    out estaTraspassada))
            {
                return estaTraspassada;
            }

            string conexion =
                TraspasFacturasConnectionStringHelper
                    .CrearConnexioBaseDades(
                        baseDatosOrigen);

            using (var connection =
                   new OleDbConnection(conexion))
            {
                connection.Open();

                string estado =
                    ObtenerEstado(
                        connection,
                        baseDatosOrigen,
                        idFacvOrigen);

                bool resultat = string.Equals(
                    estado,
                    EstadoCreada,
                    StringComparison.OrdinalIgnoreCase);

                ServeiVincleTraspasFacturesActiveX.RegistrarConnexioOleDb(
                    nameof(EstaTraspassada),
                    baseDatosOrigen);

                return resultat;
            }
        }

        public void GuardarIdFacvDestinoPendiente(
            string baseDatosOrigen,
            decimal idFacvOrigen,
            decimal idFacvDestino)
        {
            if (string.IsNullOrWhiteSpace(baseDatosOrigen))
            {
                throw new InvalidOperationException(
                    "No s'ha informat la base de dades origen.");
            }

            if (idFacvOrigen <= 0)
            {
                throw new InvalidOperationException(
                    "L'ID de la factura origen no és vàlid.");
            }

            if (idFacvDestino <= 0)
            {
                throw new InvalidOperationException(
                    "L'ID de la factura destí no és vàlid.");
            }

            if (ServeiVincleTraspasFacturesActiveX.IntentarGuardarIdFacvDestinoPendiente(
                    baseDatosOrigen,
                    idFacvOrigen,
                    idFacvDestino))
            {
                return;
            }

            string conexion =
                TraspasFacturasConnectionStringHelper
                    .CrearConnexioBaseDades(
                        baseDatosOrigen);

            const string sql = @"
                UPDATE dbo.AT_TRASPAS_FACTURES
                SET
                    IDFACV_DESTI = ?,
                    FECHA_TRASPAS = GETDATE()
                WHERE
                    BD_ORIGEN = ?
                    AND IDFACV_ORIGEN = ?
                    AND ESTADO = ?;";

            using (var connection =
                   new OleDbConnection(conexion))
            using (var command =
                   new OleDbCommand(sql, connection))
            {
                command.CommandType =
                    CommandType.Text;

                /*
                 * OleDb associa els paràmetres
                 * estrictament per posició.
                 */
                command.Parameters
                    .Add("IDFACV_DESTI", OleDbType.Decimal)
                    .Value =
                        idFacvDestino;

                command.Parameters
                    .Add("BD_ORIGEN", OleDbType.VarChar, 100)
                    .Value =
                        baseDatosOrigen.Trim();

                command.Parameters
                    .Add("IDFACV_ORIGEN", OleDbType.Decimal)
                    .Value =
                        idFacvOrigen;

                command.Parameters
                    .Add("ESTADO", OleDbType.VarChar, 20)
                    .Value =
                        EstadoPendiente;

                connection.Open();

                int afectadas =
                    command.ExecuteNonQuery();

                if (afectadas != 1)
                {
                    throw new InvalidOperationException(
                        "No s'ha pogut guardar l'ID de la factura destí " +
                        "al traspàs pendent.");
                }
            }

            ServeiVincleTraspasFacturesActiveX.RegistrarConnexioOleDb(
                nameof(GuardarIdFacvDestinoPendiente),
                baseDatosOrigen);
        }

        public VincleTraspasPendienteDto ObtenerPendiente(string baseDatosOrigen, decimal idFacvOrigen)
        {
            if (string.IsNullOrWhiteSpace(baseDatosOrigen))
            {
                throw new InvalidOperationException(
                    "No s'ha informat la base de dades origen.");
            }

            if (idFacvOrigen <= 0)
            {
                throw new InvalidOperationException(
                    "L'ID de la factura origen no és vàlid.");
            }

            VincleTraspasPendienteDto pendiente;
            if (ServeiVincleTraspasFacturesActiveX.IntentarObtenerPendiente(
                    baseDatosOrigen,
                    idFacvOrigen,
                    out pendiente))
            {
                return pendiente;
            }

            string conexion =
                TraspasFacturasConnectionStringHelper
                    .CrearConnexioBaseDades(
                        baseDatosOrigen);

            const string sql = @"
                SELECT
                    EMPRESA_DESTI,
                    BD_DESTI,
                    IDFACV_DESTI
                FROM dbo.AT_TRASPAS_FACTURES
                WHERE
                    BD_ORIGEN = ?
                    AND IDFACV_ORIGEN = ?
                    AND ESTADO = ?;";

            using (var connection =
                   new OleDbConnection(conexion))
            using (var command =
                   new OleDbCommand(sql, connection))
            {
                command.CommandType =
                    CommandType.Text;

                command.Parameters
                    .Add(
                        "BD_ORIGEN",
                        OleDbType.VarChar,
                        100)
                    .Value =
                        baseDatosOrigen.Trim();

                command.Parameters
                    .Add(
                        "IDFACV_ORIGEN",
                        OleDbType.Decimal)
                    .Value =
                        idFacvOrigen;

                command.Parameters
                    .Add(
                        "ESTADO",
                        OleDbType.VarChar,
                        20)
                    .Value =
                        EstadoPendiente;

                connection.Open();

                using (OleDbDataReader reader =
                       command.ExecuteReader())
                {
                    if (reader == null ||
                        !reader.Read())
                    {
                        ServeiVincleTraspasFacturesActiveX.RegistrarConnexioOleDb(
                            nameof(ObtenerPendiente),
                            baseDatosOrigen);

                        return null;
                    }

                    decimal? idFacvDestino =
                        null;

                    if (reader["IDFACV_DESTI"] != DBNull.Value)
                    {
                        idFacvDestino =
                            Convert.ToDecimal(
                                reader["IDFACV_DESTI"]);
                    }

                    pendiente = new VincleTraspasPendienteDto
                    {
                        EmpresaDestino =
                            reader["EMPRESA_DESTI"] == DBNull.Value
                                ? string.Empty
                                : Convert.ToString(
                                    reader["EMPRESA_DESTI"])?.Trim()
                                  ?? string.Empty,

                        BaseDatosDestino =
                            reader["BD_DESTI"] == DBNull.Value
                                ? string.Empty
                                : Convert.ToString(
                                    reader["BD_DESTI"])?.Trim()
                                  ?? string.Empty,

                        IdFacvDestino =
                            idFacvDestino
                    };

                    ServeiVincleTraspasFacturesActiveX.RegistrarConnexioOleDb(
                        nameof(ObtenerPendiente),
                        baseDatosOrigen);

                    return pendiente;
                }
            }
        }

        public void ReiniciarPendienteParaReintento(
            string baseDatosOrigen,
            decimal idFacvOrigen,
            string empresaDestino,
            string baseDatosDestino)
        {
            if (ServeiVincleTraspasFacturesActiveX.IntentarReiniciarPendienteParaReintento(
                    baseDatosOrigen,
                    idFacvOrigen,
                    empresaDestino,
                    baseDatosDestino))
            {
                return;
            }

            string conexion =
                TraspasFacturasConnectionStringHelper
                    .CrearConnexioBaseDades(
                        baseDatosOrigen);

            const string sql = @"
                UPDATE dbo.AT_TRASPAS_FACTURES
                SET
                    EMPRESA_DESTI = ?,
                    BD_DESTI = ?,
                    IDFACV_DESTI = NULL,
                    SERIE_DESTI = NULL,
                    NUMDOC_DESTI = NULL,
                    FECHA_TRASPAS = GETDATE()
                WHERE
                    BD_ORIGEN = ?
                    AND IDFACV_ORIGEN = ?
                    AND ESTADO = ?;";

            using (var connection =
                   new OleDbConnection(conexion))
            using (var command =
                   new OleDbCommand(sql, connection))
            {
                command.CommandType =
                    CommandType.Text;

                command.Parameters
                    .Add(
                        "EMPRESA_DESTI",
                        OleDbType.VarChar,
                        100)
                    .Value =
                        empresaDestino.Trim();

                command.Parameters
                    .Add(
                        "BD_DESTI",
                        OleDbType.VarChar,
                        100)
                    .Value =
                        baseDatosDestino ?? string.Empty;

                command.Parameters
                    .Add(
                        "BD_ORIGEN",
                        OleDbType.VarChar,
                        100)
                    .Value =
                        baseDatosOrigen.Trim();

                command.Parameters
                    .Add(
                        "IDFACV_ORIGEN",
                        OleDbType.Decimal)
                    .Value =
                        idFacvOrigen;

                command.Parameters
                    .Add(
                        "ESTADO",
                        OleDbType.VarChar,
                        20)
                    .Value =
                        EstadoPendiente;

                connection.Open();

                int afectadas =
                    command.ExecuteNonQuery();

                if (afectadas != 1)
                {
                    throw new InvalidOperationException(
                        "No s'ha pogut preparar el traspàs pendent per al reintent.");
                }
            }

            ServeiVincleTraspasFacturesActiveX.RegistrarConnexioOleDb(
                nameof(ReiniciarPendienteParaReintento),
                baseDatosOrigen);
        }

        public void MarcarCreada(
            string baseDatosOrigen,
            decimal idFacvOrigen,
            string empresaDestino,
            string baseDatosDestino,
            decimal idFacvDestino,
            string serieDestino,
            decimal numDocDestino)
        {
            if (ServeiVincleTraspasFacturesActiveX.IntentarMarcarCreada(
                    baseDatosOrigen,
                    idFacvOrigen,
                    empresaDestino,
                    baseDatosDestino,
                    idFacvDestino,
                    serieDestino,
                    numDocDestino))
            {
                return;
            }

            string conexion =
                TraspasFacturasConnectionStringHelper
                    .CrearConnexioBaseDades(
                        baseDatosOrigen);

            const string sql = @"
                UPDATE dbo.AT_TRASPAS_FACTURES
                SET
                    EMPRESA_DESTI = ?,
                    BD_DESTI = ?,
                    IDFACV_DESTI = ?,
                    SERIE_DESTI = ?,
                    NUMDOC_DESTI = ?,
                    FECHA_TRASPAS = GETDATE(),
                    ESTADO = ?
                WHERE
                    BD_ORIGEN = ?
                    AND IDFACV_ORIGEN = ?;";

            using (var connection =
                   new OleDbConnection(conexion))
            using (var command =
                   new OleDbCommand(sql, connection))
            {
                command.CommandType =
                    CommandType.Text;

                command.Parameters
                    .Add("EMPRESA_DESTI", OleDbType.VarChar, 100)
                    .Value =
                        empresaDestino.Trim();

                command.Parameters
                    .Add("BD_DESTI", OleDbType.VarChar, 100)
                    .Value =
                        baseDatosDestino ?? string.Empty;

                command.Parameters
                    .Add("IDFACV_DESTI", OleDbType.Decimal)
                    .Value =
                        idFacvDestino;

                command.Parameters
                    .Add("SERIE_DESTI", OleDbType.VarChar, 20)
                    .Value =
                        serieDestino ?? string.Empty;

                command.Parameters
                    .Add("NUMDOC_DESTI", OleDbType.Decimal)
                    .Value =
                        numDocDestino;

                command.Parameters
                    .Add("ESTADO", OleDbType.VarChar, 20)
                    .Value =
                        EstadoCreada;

                command.Parameters
                    .Add("BD_ORIGEN", OleDbType.VarChar, 100)
                    .Value =
                        baseDatosOrigen.Trim();

                command.Parameters
                    .Add("IDFACV_ORIGEN", OleDbType.Decimal)
                    .Value =
                        idFacvOrigen;

                connection.Open();

                int afectadas =
                    command.ExecuteNonQuery();

                if (afectadas != 1)
                {
                    throw new InvalidOperationException(
                        "No s'ha pogut marcar el traspàs com a CREADA.");
                }
            }

            ServeiVincleTraspasFacturesActiveX.RegistrarConnexioOleDb(
                nameof(MarcarCreada),
                baseDatosOrigen);
        }

        public void MarcarError(
            string baseDatosOrigen,
            decimal idFacvOrigen,
            string empresaDestino)
        {
            if (ServeiVincleTraspasFacturesActiveX.IntentarMarcarError(
                    baseDatosOrigen,
                    idFacvOrigen))
            {
                return;
            }

            string conexion =
                TraspasFacturasConnectionStringHelper
                    .CrearConnexioBaseDades(
                        baseDatosOrigen);

            const string sql = @"
                UPDATE dbo.AT_TRASPAS_FACTURES
                SET
                    ESTADO = ?,
                    FECHA_TRASPAS = GETDATE()
                WHERE
                    BD_ORIGEN = ?
                    AND IDFACV_ORIGEN = ?;";

            using (var connection =
                   new OleDbConnection(conexion))
            using (var command =
                   new OleDbCommand(sql, connection))
            {
                command.CommandType =
                    CommandType.Text;

                command.Parameters
                    .Add("ESTADO", OleDbType.VarChar, 20)
                    .Value =
                        EstadoError;

                command.Parameters
                    .Add("BD_ORIGEN", OleDbType.VarChar, 100)
                    .Value =
                        baseDatosOrigen.Trim();

                command.Parameters
                    .Add("IDFACV_ORIGEN", OleDbType.Decimal)
                    .Value =
                        idFacvOrigen;

                connection.Open();

                command.ExecuteNonQuery();
            }

            ServeiVincleTraspasFacturesActiveX.RegistrarConnexioOleDb(
                nameof(MarcarError),
                baseDatosOrigen);
        }

        private static string ObtenerEstado(
            OleDbConnection connection,
            string baseDatosOrigen,
            decimal idFacvOrigen)
        {
            const string sql = @"
                SELECT ESTADO
                FROM dbo.AT_TRASPAS_FACTURES
                WHERE
                    BD_ORIGEN = ?
                    AND IDFACV_ORIGEN = ?;";

            using (var command =
                   new OleDbCommand(sql, connection))
            {
                command.CommandType =
                    CommandType.Text;

                command.Parameters
                    .Add("BD_ORIGEN", OleDbType.VarChar, 100)
                    .Value =
                        baseDatosOrigen.Trim();

                command.Parameters
                    .Add("IDFACV_ORIGEN", OleDbType.Decimal)
                    .Value =
                        idFacvOrigen;

                object valor =
                    command.ExecuteScalar();

                if (valor == null ||
                    valor == DBNull.Value)
                {
                    return string.Empty;
                }

                return Convert.ToString(valor)?.Trim()
                       ?? string.Empty;
            }
        }

        private static void InsertarPendiente(
            OleDbConnection connection,
            string baseDatosOrigen,
            decimal idFacvOrigen,
            string empresaDestino,
            string baseDatosDestino)
        {
            const string sql = @"
                INSERT INTO dbo.AT_TRASPAS_FACTURES
                (
                    BD_ORIGEN,
                    IDFACV_ORIGEN,
                    EMPRESA_DESTI,
                    BD_DESTI,
                    IDFACV_DESTI,
                    SERIE_DESTI,
                    NUMDOC_DESTI,
                    FECHA_TRASPAS,
                    ESTADO
                )
                VALUES
                (
                    ?,
                    ?,
                    ?,
                    ?,
                    NULL,
                    NULL,
                    NULL,
                    GETDATE(),
                    ?
                );";

            using (var command =
                   new OleDbCommand(sql, connection))
            {
                command.CommandType =
                    CommandType.Text;

                command.Parameters
                    .Add("BD_ORIGEN", OleDbType.VarChar, 100)
                    .Value =
                        baseDatosOrigen.Trim();

                command.Parameters
                    .Add("IDFACV_ORIGEN", OleDbType.Decimal)
                    .Value =
                        idFacvOrigen;

                command.Parameters
                    .Add("EMPRESA_DESTI", OleDbType.VarChar, 100)
                    .Value =
                        empresaDestino.Trim();

                command.Parameters
                    .Add("BD_DESTI", OleDbType.VarChar, 100)
                    .Value =
                        baseDatosDestino ?? string.Empty;

                command.Parameters
                    .Add("ESTADO", OleDbType.VarChar, 20)
                    .Value =
                        EstadoPendiente;

                command.ExecuteNonQuery();
            }
        }

        private static void ReutilizarRegistroError(
            OleDbConnection connection,
            string baseDatosOrigen,
            decimal idFacvOrigen,
            string empresaDestino,
            string baseDatosDestino)
        {
            const string sql = @"
                UPDATE dbo.AT_TRASPAS_FACTURES
                SET
                    EMPRESA_DESTI = ?,
                    BD_DESTI = ?,
                    IDFACV_DESTI = NULL,
                    SERIE_DESTI = NULL,
                    NUMDOC_DESTI = NULL,
                    FECHA_TRASPAS = GETDATE(),
                    ESTADO = ?
                WHERE
                    BD_ORIGEN = ?
                    AND IDFACV_ORIGEN = ?;";

            using (var command =
                   new OleDbCommand(sql, connection))
            {
                command.CommandType =
                    CommandType.Text;

                command.Parameters
                    .Add("EMPRESA_DESTI", OleDbType.VarChar, 100)
                    .Value =
                        empresaDestino.Trim();

                command.Parameters
                    .Add("BD_DESTI", OleDbType.VarChar, 100)
                    .Value =
                        baseDatosDestino ?? string.Empty;

                command.Parameters
                    .Add("ESTADO", OleDbType.VarChar, 20)
                    .Value =
                        EstadoPendiente;

                command.Parameters
                    .Add("BD_ORIGEN", OleDbType.VarChar, 100)
                    .Value =
                        baseDatosOrigen.Trim();

                command.Parameters
                    .Add("IDFACV_ORIGEN", OleDbType.Decimal)
                    .Value =
                        idFacvOrigen;

                int afectadas =
                    command.ExecuteNonQuery();

                if (afectadas != 1)
                {
                    throw new InvalidOperationException(
                        "No s'ha pogut reactivar el registre ERROR.");
                }
            }
        }

        private static void ValidarParametros(
            string baseDatosOrigen,
            decimal idFacvOrigen,
            string empresaDestino)
        {
            if (string.IsNullOrWhiteSpace(baseDatosOrigen))
            {
                throw new InvalidOperationException(
                    "No s'ha informat la base de dades origen.");
            }

            if (idFacvOrigen <= 0)
            {
                throw new InvalidOperationException(
                    "L'ID de la factura origen no és vàlid.");
            }

            if (string.IsNullOrWhiteSpace(empresaDestino))
            {
                throw new InvalidOperationException(
                    "No s'ha informat l'empresa de destinació.");
            }
        }

        public void ValidarDisponibleParaTraspaso(
            string baseDatosOrigen,
            decimal idFacvOrigen,
            string empresaDestino)
        {
            if (string.IsNullOrWhiteSpace(baseDatosOrigen))
            {
                throw new InvalidOperationException(
                    "No s'ha informat la base de dades origen.");
            }

            if (idFacvOrigen <= 0)
            {
                throw new InvalidOperationException(
                    "L'ID de la factura origen no és vàlid.");
            }

            if (ServeiVincleTraspasFacturesActiveX.IntentarValidarDisponibleParaTraspaso(
                    baseDatosOrigen,
                    idFacvOrigen,
                    empresaDestino))
            {
                return;
            }

            string conexion =
                TraspasFacturasConnectionStringHelper
                    .CrearConnexioBaseDades(
                        baseDatosOrigen);

            using (var connection =
                   new OleDbConnection(conexion))
            {
                connection.Open();

                string estado =
                    ObtenerEstado(
                        connection,
                        baseDatosOrigen,
                        idFacvOrigen);

                if (string.Equals(
                        estado,
                        EstadoCreada,
                        StringComparison.OrdinalIgnoreCase))
                {
                    throw new InvalidOperationException(
                        "La factura ja ha estat traspassada anteriorment.");
                }

                if (string.Equals(
                    estado,
                    EstadoPendiente,
                    StringComparison.OrdinalIgnoreCase))
                {
                    VincleTraspasPendienteDto pendiente =
                        ObtenerPendiente(
                            baseDatosOrigen,
                            idFacvOrigen);

                    if (pendiente != null &&
                        !string.IsNullOrWhiteSpace(
                            pendiente.EmpresaDestino))
                    {
                        string empresaPendiente =
                            pendiente.EmpresaDestino.Trim();

                        string empresaSeleccionada =
                            (empresaDestino ?? string.Empty).Trim();

                        if (!string.Equals(
                                empresaPendiente,
                                empresaSeleccionada,
                                StringComparison.OrdinalIgnoreCase))
                        {
                            throw new InvalidOperationException(
                                "La factura té un traspàs pendent cap a l'empresa '" +
                                empresaPendiente +
                                "'." +
                                Environment.NewLine +
                                "Empresa seleccionada actualment: '" +
                                empresaSeleccionada +
                                "'." +
                                Environment.NewLine +
                                "Selecciona la mateixa empresa per recuperar-lo.");
                        }
                        /*
                         * Mateixa empresa:
                         * permetem continuar perquè el servei
                         * intentarà recuperar el PENDIENTE.
                         */
                        ServeiVincleTraspasFacturesActiveX.RegistrarConnexioOleDb(
                            nameof(ValidarDisponibleParaTraspaso),
                            baseDatosOrigen);

                        return;
                    }
                }
            }

            ServeiVincleTraspasFacturesActiveX.RegistrarConnexioOleDb(
                nameof(ValidarDisponibleParaTraspaso),
                baseDatosOrigen);
        }
    }
}
