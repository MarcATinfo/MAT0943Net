using System;
using System.Data.OleDb;

namespace A3ErpCalculadorTarifes.Infrastructure.Logging
{
    /// <summary>
    /// Llegeix la configuracio del log del calculador
    /// des de la taula AT_MAT0943NET_CONFIG.
    /// </summary>
    public sealed class RepositoriConfiguracioLogCalculadorTarifes
    {
        private const string ClauLogActiu =
            "CalculadorTarifes_LogActivo";

        private const string ClauRutaLog =
            "CalculadorTarifes_LogRuta";

        public ConfiguracioLogCalculadorTarifes Carregar(
            string cadenaConnexio)
        {
            if (string.IsNullOrWhiteSpace(
                cadenaConnexio))
            {
                return new ConfiguracioLogCalculadorTarifes();
            }

            ConfiguracioLogCalculadorTarifes configuracio =
                new ConfiguracioLogCalculadorTarifes();

            const string sql =
                "SELECT " +
                "CLAVE, " +
                "VALOR " +
                "FROM dbo.AT_MAT0943NET_CONFIG " +
                "WHERE ACTIVO = 1 " +
                "AND CLAVE IN (?, ?)";

            using (OleDbConnection connexio =
                new OleDbConnection(
                    cadenaConnexio))
            using (OleDbCommand comanda =
                new OleDbCommand(
                    sql,
                    connexio))
            {
                connexio.Open();

                comanda.Parameters.Add(
                        "CLAVE_1",
                        OleDbType.VarChar,
                        100)
                    .Value =
                        ClauLogActiu;

                comanda.Parameters.Add(
                        "CLAVE_2",
                        OleDbType.VarChar,
                        100)
                    .Value =
                        ClauRutaLog;

                using (OleDbDataReader lector =
                    comanda.ExecuteReader())
                {
                    if (lector == null)
                    {
                        return configuracio;
                    }

                    while (lector.Read())
                    {
                        configuracio.ConfiguracioDisponible =
                            true;

                        string clau =
                            Convert.ToString(
                                lector["CLAVE"])
                            ?.Trim()
                            ?? string.Empty;

                        string valor =
                            Convert.ToString(
                                lector["VALOR"])
                            ?.Trim()
                            ?? string.Empty;

                        if (string.Equals(
                            clau,
                            ClauLogActiu,
                            StringComparison.OrdinalIgnoreCase))
                        {
                            configuracio.LogActiu =
                                InterpretarBoolea(
                                    valor);
                        }
                        else if (string.Equals(
                            clau,
                            ClauRutaLog,
                            StringComparison.OrdinalIgnoreCase))
                        {
                            configuracio.RutaLog =
                                valor ?? string.Empty;
                        }
                    }
                }
            }

            if (configuracio.ConfiguracioDisponible &&
                string.IsNullOrWhiteSpace(
                    configuracio.RutaLog))
            {
                configuracio.LogActiu =
                    false;
            }

            return configuracio;
        }

        private static bool InterpretarBoolea(
            string valor)
        {
            string normalitzat =
                (valor ?? string.Empty)
                .Trim()
                .ToUpperInvariant();

            switch (normalitzat)
            {
                case "1":
                case "T":
                case "TRUE":
                case "S":
                case "SI":
                    return true;

                default:
                    return false;
            }
        }
    }
}
