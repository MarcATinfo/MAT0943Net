using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;

namespace A3ErpCalculadorTarifes.Infrastructure.Logging
{
    /// <summary>
    /// Log propi i independent del calculador de tarifes.
    /// </summary>
    public static class CalculadorTarifesLogger
    {
        private const string PrefixFitxer =
            "CalculadorTarifes_";

        private const string ExtensioFitxer =
            ".log";

        private static readonly object SyncRoot =
            new object();

        private static bool logActiu;

        private static bool errorsFatalsOberturaActius;

        private static string rutaBase =
            string.Empty;

        public static bool LogActiu
        {
            get
            {
                lock (SyncRoot)
                {
                    return logActiu;
                }
            }
        }

        public static void InicialitzarLogReserva()
        {
            string rutaLocal =
                ObtenirRutaReservaLocal();

            if (string.IsNullOrWhiteSpace(rutaLocal))
            {
                return;
            }

            lock (SyncRoot)
            {
                errorsFatalsOberturaActius =
                    true;

                rutaBase =
                    rutaLocal;

                logActiu =
                    true;
            }

            PrepararRutaActual();
            NetejarLogsAnticsEnRuta(
                rutaLocal,
                7);
        }

        public static void AplicarConfiguracioLog(
            ConfiguracioLogCalculadorTarifes configuracio)
        {
            InicialitzarLogReserva();

            if (configuracio == null ||
                !configuracio.ConfiguracioDisponible)
            {
                Advertencia(
                    "No s'ha trobat configuració de log del calculador. Es continua amb el log local de reserva.");

                return;
            }

            if (!configuracio.LogActiu)
            {
                Advertencia(
                    "La configuració ha desactivat el log operatiu del calculador.");

                lock (SyncRoot)
                {
                    logActiu =
                        false;

                    errorsFatalsOberturaActius =
                        true;
                }

                return;
            }

            string rutaConfigurada =
                (configuracio.RutaLog ?? string.Empty)
                .Trim();

            if (string.IsNullOrWhiteSpace(rutaConfigurada))
            {
                Advertencia(
                    "La configuració del log del calculador no informa cap ruta. Es continua amb el log local de reserva.");

                return;
            }

            Exception errorPreparantRuta;

            if (!IntentarPrepararRuta(
                rutaConfigurada,
                out errorPreparantRuta))
            {
                Error(
                    "No s'ha pogut utilitzar la ruta configurada del log. Es continua amb el log local de reserva.",
                    errorPreparantRuta);

                return;
            }

            Informacio(
                "El log operatiu continua a la ruta configurada.");

            lock (SyncRoot)
            {
                rutaBase =
                    rutaConfigurada;

                logActiu =
                    true;

                errorsFatalsOberturaActius =
                    true;
            }

            NetejarLogsAnticsEnRuta(
                rutaConfigurada,
                7);

            Informacio(
                "El logger s'ha inicialitzat des del log local de reserva.");
        }

        public static void Configurar(
            ConfiguracioLogCalculadorTarifes configuracio)
        {
            AplicarConfiguracioLog(
                configuracio);
        }

        public static void Debug(
            string missatge,
            IDictionary<string, object> camps = null)
        {
            Escriure(
                "DBG",
                missatge,
                camps,
                null,
                false);
        }

        public static void Informacio(
            string missatge,
            IDictionary<string, object> camps = null)
        {
            Escriure(
                "INF",
                missatge,
                camps,
                null,
                false);
        }

        public static void InformacioArrencada(
            string missatge,
            IDictionary<string, object> camps = null)
        {
            Escriure(
                "INF",
                missatge,
                camps,
                null,
                true);
        }

        public static void Advertencia(
            string missatge,
            IDictionary<string, object> camps = null)
        {
            Escriure(
                "WRN",
                missatge,
                camps,
                null,
                false);
        }

        public static void AdvertenciaArrencada(
            string missatge,
            IDictionary<string, object> camps = null)
        {
            Escriure(
                "WRN",
                missatge,
                camps,
                null,
                true);
        }

        public static void Error(
            string missatge,
            Exception excepcio,
            IDictionary<string, object> camps = null)
        {
            Escriure(
                "ERR",
                missatge,
                camps,
                excepcio,
                false);
        }

        public static void ErrorFatalObertura(
            string missatge,
            Exception excepcio,
            IDictionary<string, object> camps = null)
        {
            Escriure(
                "ERR",
                missatge,
                camps,
                excepcio,
                true);
        }

        public static void NetejarLogsAntics(
            int diesRetencio)
        {
            string carpeta;

            lock (SyncRoot)
            {
                carpeta =
                    rutaBase;
            }

            NetejarLogsAnticsEnRuta(
                carpeta,
                diesRetencio);
        }

        private static void PrepararRutaActual()
        {
            string ruta;

            lock (SyncRoot)
            {
                ruta =
                    rutaBase;
            }

            Exception error;

            if (!IntentarPrepararRuta(
                ruta,
                out error))
            {
                lock (SyncRoot)
                {
                    logActiu =
                        false;
                }
            }
        }

        private static bool IntentarPrepararRuta(
            string ruta,
            out Exception error)
        {
            error =
                null;

            try
            {
                if (string.IsNullOrWhiteSpace(ruta))
                {
                    return false;
                }

                Directory.CreateDirectory(
                    ruta);

                return true;
            }
            catch (Exception ex)
            {
                error =
                    ex;

                return false;
            }
        }

        private static void Escriure(
            string nivell,
            string missatge,
            IDictionary<string, object> camps,
            Exception excepcio,
            bool permetreAmbLogDesactivat)
        {
            bool actiu;
            bool potRegistrarFatal;
            string rutaFitxer;

            lock (SyncRoot)
            {
                actiu =
                    logActiu;

                potRegistrarFatal =
                    permetreAmbLogDesactivat
                    &&
                    errorsFatalsOberturaActius;

                if ((!actiu && !potRegistrarFatal) ||
                    string.IsNullOrWhiteSpace(rutaBase))
                {
                    return;
                }

                rutaFitxer =
                    Path.Combine(
                        rutaBase,
                        ObtenirNomFitxerActual());
            }

            try
            {
                StringBuilder registre =
                    new StringBuilder();

                registre.Append(
                    DateTime.Now.ToString(
                        "yyyy-MM-dd HH:mm:ss.fff",
                        CultureInfo.InvariantCulture));

                registre.Append(" [");
                registre.Append(
                    NormalitzarText(
                        nivell));
                registre.Append("] ");

                registre.Append(
                    NormalitzarText(
                        missatge));

                if (camps != null)
                {
                    foreach (
                        KeyValuePair<string, object> camp
                        in camps)
                    {
                        registre.Append(" | ");
                        registre.Append(
                            NormalitzarText(
                                camp.Key));
                        registre.Append("=");
                        registre.Append(
                            NormalitzarValor(
                                camp.Value));
                    }
                }

                if (excepcio != null)
                {
                    registre.Append(" | ");
                    registre.Append(
                        NormalitzarText(
                            excepcio.GetType().Name));
                    registre.Append(": ");
                    registre.Append(
                        NormalitzarText(
                            excepcio.Message));

                    registre.AppendLine();
                    registre.Append(
                        excepcio.ToString());
                }

                lock (SyncRoot)
                {
                    string directori =
                        Path.GetDirectoryName(
                            rutaFitxer);

                    if (!string.IsNullOrWhiteSpace(
                        directori))
                    {
                        Directory.CreateDirectory(
                            directori);
                    }

                    File.AppendAllText(
                        rutaFitxer,
                        registre.ToString()
                        + Environment.NewLine,
                        Encoding.UTF8);
                }
            }
            catch
            {
            }
        }

        private static void NetejarLogsAnticsEnRuta(
            string carpeta,
            int diesRetencio)
        {
            if (diesRetencio < 0 ||
                string.IsNullOrWhiteSpace(carpeta) ||
                !Directory.Exists(carpeta))
            {
                return;
            }

            DateTime dataLimit =
                DateTime.Today.AddDays(
                    -diesRetencio);

            string patro =
                PrefixFitxer
                + "*"
                + ExtensioFitxer;

            string[] fitxers;

            try
            {
                fitxers =
                    Directory.GetFiles(
                        carpeta,
                        patro,
                        SearchOption.TopDirectoryOnly);
            }
            catch
            {
                return;
            }

            foreach (string rutaFitxer in fitxers)
            {
                try
                {
                    DateTime dataFitxer;

                    if (!IntentarObtenirDataFitxer(
                        rutaFitxer,
                        out dataFitxer))
                    {
                        continue;
                    }

                    if (dataFitxer.Date < dataLimit.Date)
                    {
                        File.Delete(
                            rutaFitxer);
                    }
                }
                catch
                {
                }
            }
        }

        private static string ObtenirRutaReservaLocal()
        {
            try
            {
                string localAppData =
                    Environment.GetFolderPath(
                        Environment.SpecialFolder.LocalApplicationData);

                if (string.IsNullOrWhiteSpace(
                    localAppData))
                {
                    return string.Empty;
                }

                return Path.Combine(
                    localAppData,
                    "AT_Infoserveis",
                    "A3ErpCalculadorTarifes",
                    "Logs");
            }
            catch
            {
                return string.Empty;
            }
        }

        private static string ObtenirNomFitxerActual()
        {
            return
                PrefixFitxer
                + DateTime.Today.ToString(
                    "yyyy-MM-dd",
                    CultureInfo.InvariantCulture)
                + ExtensioFitxer;
        }

        private static bool IntentarObtenirDataFitxer(
            string rutaFitxer,
            out DateTime dataFitxer)
        {
            dataFitxer =
                DateTime.MinValue;

            string nomSenseExtensio =
                Path.GetFileNameWithoutExtension(
                    rutaFitxer);

            if (string.IsNullOrWhiteSpace(
                nomSenseExtensio) ||
                !nomSenseExtensio.StartsWith(
                    PrefixFitxer,
                    StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            string textData =
                nomSenseExtensio.Substring(
                    PrefixFitxer.Length);

            return DateTime.TryParseExact(
                textData,
                "yyyy-MM-dd",
                CultureInfo.InvariantCulture,
                DateTimeStyles.None,
                out dataFitxer);
        }

        private static string NormalitzarText(
            string valor)
        {
            return (valor ?? string.Empty)
                .Replace(
                    "\r",
                    " ")
                .Replace(
                    "\n",
                    " ")
                .Trim();
        }

        private static string NormalitzarValor(
            object valor)
        {
            if (valor == null ||
                valor == DBNull.Value)
            {
                return string.Empty;
            }

            string text =
                Convert.ToString(
                    valor,
                    CultureInfo.CurrentCulture)
                ?? string.Empty;

            return NormalitzarText(
                text);
        }
    }
}
