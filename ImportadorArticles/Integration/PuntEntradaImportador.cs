using A3ErpImportadorArticles.Infrastructure.Connections;
using A3ErpImportadorArticles.Infrastructure.Logging;
using A3ErpImportadorArticles.Models;
using A3ErpImportadorArticles.Services;
using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace A3ErpImportadorArticles.Integration
{
    /// <summary>
    /// Punt d'entrada únic per obrir l'importador des d'a3ERP.
    ///
    /// Aquesta classe rep l'empresa activa i la connexió
    /// proporcionada per a3ERP, resol una connexió operativa,
    /// inicialitza el log i obre el formulari.
    /// </summary>
    public static class PuntEntradaImportador
    {
        private const int DiesRetencioLog =
            7;

        /// <summary>
        /// Obre l'importador amb el context
        /// de l'empresa activa.
        /// </summary>
        public static void Obrir(
            string empresaActiva,
            string cadenaConnexioRebuda)
        {
            ImportadorArticlesLogger.InicialitzarLogReserva();

            Dictionary<string, object> campsObertura =
                CrearCampsObertura(
                    empresaActiva,
                    cadenaConnexioRebuda);

            ImportadorArticlesLogger.InformacioArrencada(
                "S'inicia PuntEntradaImportador.Obrir().",
                campsObertura);

            try
            {
                ServeiResolucioConnexioA3Erp serveiConnexio =
                    new ServeiResolucioConnexioA3Erp();

                ResultatResolucioConnexio resultat =
                    serveiConnexio.Resoldre(
                        empresaActiva,
                        cadenaConnexioRebuda);

                if (!resultat.Correcte)
                {
                    ImportadorArticlesLogger.ErrorFatalObertura(
                        "No s'ha pogut resoldre la connexió de l'importador.",
                        new InvalidOperationException(
                            resultat.Missatge),
                        campsObertura);

                    MostrarErrorConnexio(
                        resultat.Missatge);

                    return;
                }

                /*
                 * El log s'inicialitza quan ja disposem
                 * d'una connexió operativa amb l'empresa.
                 */
                InicialitzarLog(
                    resultat.Context);

                ImportadorArticlesLogger.InformacioArrencada(
                    "S'obre l'importador d'articles.",
                    CrearCampsContext(
                        resultat.Context));

                ObrirFormulari(
                    resultat.Context);

                ImportadorArticlesLogger.Informacio(
                    "Es tanca l'importador d'articles.",
                    CrearCampsContext(
                        resultat.Context));
            }
            catch (Exception ex)
            {
                ImportadorArticlesLogger.ErrorFatalObertura(
                    "No s'ha pogut iniciar l'importador d'articles.",
                    ex,
                    campsObertura);

                MessageBox.Show(
                    "No s'ha pogut iniciar l'importador d'articles."
                    + Environment.NewLine
                    + Environment.NewLine
                    + ex.Message,
                    "Importador d'articles",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Llegeix la configuració, configura el logger
        /// i executa sempre la neteja per data.
        ///
        /// Un error del sistema de log no ha d'impedir
        /// l'obertura de l'importador.
        /// </summary>
        private static void InicialitzarLog(
            ContextImportadorA3Erp context)
        {
            if (context == null)
            {
                return;
            }

            ConfiguracioLogImportador configuracio =
                new ConfiguracioLogImportador();

            try
            {
                ServeiConfiguracioLogImportador serveiConfiguracio =
                    new ServeiConfiguracioLogImportador();

                configuracio =
                    serveiConfiguracio.Carregar(
                        context.CadenaConnexio);
            }
            catch (Exception ex)
            {
                ImportadorArticlesLogger.Error(
                    "No s'ha pogut llegir la configuració del log de l'importador. Es continua amb el log local de reserva.",
                    ex,
                    CrearCampsContext(
                        context));
            }

            ImportadorArticlesLogger.AplicarConfiguracioLog(
                configuracio);

            ImportadorArticlesLogger.Informacio(
                "S'ha inicialitzat el sistema de log.",
                new Dictionary<string, object>
                {
                    {
                        "Empresa",
                        context.EmpresaActiva
                    },
                    {
                        "BaseDades",
                        context.BaseDadesEmpresa
                    },
                    {
                        "Ruta",
                        configuracio.RutaLog
                    },
                    {
                        "RetencioDies",
                        DiesRetencioLog
                    }
                });
        }

        /// <summary>
        /// Obre el formulari amb el context ja validat.
        ///
        /// S'utilitza ShowDialog perquè, quan s'executa
        /// des d'a3ERP, no s'ha de crear un segon bucle
        /// principal amb Application.Run.
        /// </summary>
        private static void ObrirFormulari(
            ContextImportadorA3Erp context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(
                    nameof(context));
            }

            using (FrmImportadorArticles formulari =
                new FrmImportadorArticles(context))
            {
                ImportadorArticlesLogger.InformacioArrencada(
                    "Formulari de l'importador creat correctament.",
                    CrearCampsContext(
                        context));

                formulari.Shown +=
                    delegate
                    {
                        ImportadorArticlesLogger.InformacioArrencada(
                            "Formulari de l'importador mostrat correctament.",
                            CrearCampsContext(
                                context));
                    };

                formulari.ShowDialog();
            }
        }

        private static Dictionary<string, object> CrearCampsObertura(
            string empresaActiva,
            string cadenaConnexioRebuda)
        {
            return new Dictionary<string, object>
            {
                {
                    "Origen",
                    "A3ERP"
                },
                {
                    "EmpresaRebuda",
                    empresaActiva ?? string.Empty
                },
                {
                    "BaseDadesRebuda",
                    empresaActiva ?? string.Empty
                },
                {
                    "ConnexioRebuda",
                    string.IsNullOrWhiteSpace(
                        cadenaConnexioRebuda)
                        ? "No"
                        : "Sí"
                }
            };
        }

        /// <summary>
        /// Construeix els camps comuns del context.
        ///
        /// No inclou cap contrasenya ni cadena
        /// de connexió.
        /// </summary>
        private static Dictionary<string, object>
            CrearCampsContext(
                ContextImportadorA3Erp context)
        {
            return new Dictionary<string, object>
            {
                {
                    "Empresa",
                    context?.EmpresaActiva
                    ?? string.Empty
                },
                {
                    "BaseDades",
                    context?.BaseDadesEmpresa
                    ?? string.Empty
                }
            };
        }

        /// <summary>
        /// Mostra un error comprensible quan no s'ha pogut
        /// establir cap connexió amb l'empresa activa.
        /// </summary>
        private static void MostrarErrorConnexio(
            string missatge)
        {
            string detall =
                string.IsNullOrWhiteSpace(missatge)
                    ? "No s'ha pogut establir la connexió amb a3ERP."
                    : missatge;

            MessageBox.Show(
                detall,
                "Connexió amb a3ERP",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
        }
    }
}
