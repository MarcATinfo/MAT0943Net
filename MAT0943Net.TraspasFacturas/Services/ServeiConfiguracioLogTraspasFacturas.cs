using MAT0943Net.TraspasFacturas.Models;
using System;
using System.Data.OleDb;

namespace MAT0943Net.TraspasFacturas.Services
{
    internal sealed class ServeiConfiguracioLogTraspasFacturas
    {
        private const string ClauLogActiu =
            "TraspasFacturas_LogActivo";

        private const string ClauRutaLog =
            "TraspasFacturas_LogRuta";

        public ConfiguracioLogTraspasFacturas Carregar(
            string cadenaConnexio)
        {
            if (string.IsNullOrWhiteSpace(cadenaConnexio))
            {
                throw new ArgumentException(
                    "La cadena de connexió és buida.",
                    nameof(cadenaConnexio));
            }

            ConfiguracioLogTraspasFacturas configuracio =
                new ConfiguracioLogTraspasFacturas();

            const string sql =
                "SELECT " +
                "CLAVE, " +
                "VALOR " +
                "FROM dbo.AT_MAT0943NET_CONFIG " +
                "WHERE ACTIVO = 1 " +
                "AND CLAVE IN (?, ?)";

            using (OleDbConnection connexio =
                   new OleDbConnection(cadenaConnexio))
            {
                connexio.Open();

                using (OleDbCommand comanda =
                       new OleDbCommand(
                           sql,
                           connexio))
                {
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
                                        valor,
                                        false);
                            }
                            else if (string.Equals(
                                         clau,
                                         ClauRutaLog,
                                         StringComparison.OrdinalIgnoreCase))
                            {
                                if (!string.IsNullOrWhiteSpace(valor))
                                {
                                    configuracio.RutaLog =
                                        valor;
                                }
                            }
                        }
                    }
                }
            }

            return configuracio;
        }

        private static bool InterpretarBoolea(
            string valor,
            bool valorPerDefecte)
        {
            string valorNormalitzat =
                (valor ?? string.Empty)
                .Trim()
                .ToUpperInvariant();

            switch (valorNormalitzat)
            {
                case "TRUE":
                case "T":
                case "1":
                case "SI":
                case "SÍ":
                case "S":
                    return true;

                case "FALSE":
                case "F":
                case "0":
                case "NO":
                case "N":
                    return false;

                default:
                    return valorPerDefecte;
            }
        }
    }
}