using A3ErpCalculadorTarifes.Infrastructure.Connections;
using A3ErpCalculadorTarifes.Models;
using A3ErpCalculadorTarifes.Infrastructure.Logging;
using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace A3ErpCalculadorTarifes.Integration
{
    /// <summary>
    /// Punt d'entrada unic per obrir el calculador
    /// de tarifes des d'a3ERP.
    ///
    /// Rep el context capturat per la DLL COM existent
    /// i obre el formulari sense fer cap escriptura.
    /// </summary>
    public static class PuntEntradaCalculador
    {
        /// <summary>
        /// Obre el calculador amb la base de dades
        /// i la connexio de l'empresa activa.
        /// </summary>
        public static void Obrir(
            string baseDadesEmpresa,
            string connexioEmpresa)
        {
            CalculadorTarifesLogger.InicialitzarLogReserva();

            Dictionary<string, object> campsObertura =
                CrearCampsObertura(
                    baseDadesEmpresa,
                    connexioEmpresa);

            CalculadorTarifesLogger.InformacioArrencada(
                "S'inicia PuntEntradaCalculador.Obrir().",
                campsObertura);

            ServeiResolucioConnexioA3Erp serveiConnexio =
                new ServeiResolucioConnexioA3Erp();

            ResultatResolucioConnexio resultat =
                serveiConnexio.Resoldre(
                    baseDadesEmpresa,
                    connexioEmpresa);

            if (!resultat.Correcte)
            {
                CalculadorTarifesLogger.ErrorFatalObertura(
                    "No s'ha pogut resoldre la connexió del calculador.",
                    new InvalidOperationException(
                        resultat.Missatge),
                    campsObertura);

                MostrarErrorConnexio(
                    resultat.Missatge);

                return;
            }

            ObrirFormulari(
                resultat.Context);
        }

        /// <summary>
        /// Obre el formulari dins del bucle de finestres
        /// que ja proporciona a3ERP.
        /// </summary>
        private static void ObrirFormulari(
            ContextCalculadorA3Erp context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(
                    nameof(context));
            }

            using (FrmCalculadorTarifes formulari =
                new FrmCalculadorTarifes(context))
            {
                CalculadorTarifesLogger.InformacioArrencada(
                    "Formulari del calculador creat correctament.",
                    CrearCampsContext(
                        context));

                formulari.Shown +=
                    delegate
                    {
                        CalculadorTarifesLogger.InformacioArrencada(
                            "Formulari del calculador mostrat correctament.",
                            CrearCampsContext(
                                context));
                    };

                formulari.ShowDialog();
            }
        }

        private static Dictionary<string, object> CrearCampsObertura(
            string baseDadesEmpresa,
            string connexioEmpresa)
        {
            return new Dictionary<string, object>
            {
                {
                    "Origen",
                    "A3ERP"
                },
                {
                    "EmpresaRebuda",
                    baseDadesEmpresa ?? string.Empty
                },
                {
                    "BaseDadesRebuda",
                    baseDadesEmpresa ?? string.Empty
                },
                {
                    "ConnexioRebuda",
                    string.IsNullOrWhiteSpace(connexioEmpresa)
                        ? "No"
                        : "Sí"
                }
            };
        }

        private static Dictionary<string, object> CrearCampsContext(
            ContextCalculadorA3Erp context)
        {
            return new Dictionary<string, object>
            {
                {
                    "Origen",
                    "A3ERP"
                },
                {
                    "Empresa",
                    context == null
                        ? string.Empty
                        : context.EmpresaActiva
                },
                {
                    "BaseDades",
                    context == null
                        ? string.Empty
                        : context.BaseDadesEmpresa
                },
                {
                    "ConnexioRebuda",
                    context != null &&
                    context.ConnexioDisponible
                        ? "Sí"
                        : "No"
                }
            };
        }

        /// <summary>
        /// Mostra un error controlat quan no s'ha pogut
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
