using A3ErpGestorFormulesTarifes.Models;
using System;
using System.Collections.Generic;
using System.Windows.Forms;
using A3ErpGestorFormulesTarifes.Infrastructure.Logging;

namespace A3ErpGestorFormulesTarifes.Integracio
{
    /// <summary>
    /// Punt d'entrada públic per obrir
    /// el gestor de fórmules de tarifes.
    ///
    /// Aquesta classe centralitza la creació
    /// del context i l'obertura del formulari.
    /// El projecte que inicia el gestor no necessita
    /// conèixer els detalls interns del formulari.
    /// </summary>
    public static class PuntEntradaGestorFormules
    {
        /// <summary>
        /// Obre el gestor amb el context
        /// de l'empresa activa.
        /// </summary>
        /// <param name="empresaActiva">
        /// Nom o identificador de l'empresa activa.
        /// </param>
        /// <param name="baseDadesEmpresa">
        /// Base de dades sobre la qual es gestionaran
        /// les fórmules.
        /// </param>
        /// <param name="cadenaConnexio">
        /// Connexió ja resolta pel procés d'origen.
        /// No es mostra ni es registra.
        /// </param>
        /// <param name="utilitzaConnexioAlternativa">
        /// Indica si la connexió prové
        /// del mecanisme alternatiu.
        /// </param>
        public static void Obrir(
            string empresaActiva,
            string baseDadesEmpresa,
            string cadenaConnexio,
            bool utilitzaConnexioAlternativa)
        {
            GestorFormulesTarifesLogger.InicialitzarLogReserva();

            Dictionary<string, object> campsObertura =
                CrearCampsObertura(
                    empresaActiva,
                    baseDadesEmpresa,
                    cadenaConnexio);

            GestorFormulesTarifesLogger.InformacioArrencada(
                "Petició d'obertura del gestor de fórmules rebuda.",
                campsObertura);

            try
            {
                if (string.IsNullOrWhiteSpace(
                    baseDadesEmpresa)
                    ||
                    string.IsNullOrWhiteSpace(
                        cadenaConnexio))
                {
                    GestorFormulesTarifesLogger.AdvertenciaArrencada(
                        "No s'ha rebut un context vàlid per obrir el gestor de fórmules.",
                        campsObertura);
                }

                ValidarContext(
                    baseDadesEmpresa,
                    cadenaConnexio);

                ContextGestorFormulesA3Erp context =
                    new ContextGestorFormulesA3Erp(
                        empresaActiva:
                            empresaActiva,

                        baseDadesEmpresa:
                            baseDadesEmpresa,

                        cadenaConnexio:
                            cadenaConnexio,

                        utilitzaConnexioAlternativa:
                            utilitzaConnexioAlternativa);

                ObrirFormulari(
                    context);
            }
            catch (Exception ex)
            {
                GestorFormulesTarifesLogger.ErrorFatalObertura(
                    "No s'ha pogut obrir el gestor de fórmules.",
                    ex,
                    campsObertura);

                throw;
            }
        }

        /// <summary>
        /// Comprova que s'han rebut les dades mínimes
        /// necessàries per treballar amb l'empresa activa.
        /// </summary>
        private static void ValidarContext(
            string baseDadesEmpresa,
            string cadenaConnexio)
        {
            if (string.IsNullOrWhiteSpace(
                baseDadesEmpresa))
            {
                throw new InvalidOperationException(
                    "No s'ha rebut la base de dades "
                    + "de l'empresa activa.");
            }

            if (string.IsNullOrWhiteSpace(
                cadenaConnexio))
            {
                throw new InvalidOperationException(
                    "No s'ha rebut la connexió "
                    + "de l'empresa activa.");
            }
        }

        /// <summary>
        /// Crea i mostra el formulari del gestor
        /// dins del bucle de finestres existent.
        /// </summary>
        private static void ObrirFormulari(
            ContextGestorFormulesA3Erp context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(
                    nameof(context));
            }

            using (FrmGestorFormulesTarifes formulari =
                new FrmGestorFormulesTarifes(
                    context))
            {
                GestorFormulesTarifesLogger.InformacioArrencada(
                    "Formulari del gestor de fórmules creat correctament.",
                    CrearCampsContext(
                        context));

                formulari.Shown +=
                    delegate
                    {
                        GestorFormulesTarifesLogger.InformacioArrencada(
                            "Formulari del gestor de fórmules mostrat correctament.",
                            CrearCampsContext(
                                context));
                    };

                formulari.ShowDialog();

                GestorFormulesTarifesLogger.InformacioArrencada(
                    "ShowDialog del gestor de fórmules finalitzat correctament.",
                    CrearCampsContext(
                        context));
            }
        }

        private static Dictionary<string, object> CrearCampsObertura(
            string empresaActiva,
            string baseDadesEmpresa,
            string cadenaConnexio)
        {
            return new Dictionary<string, object>
            {
                {
                    "Origen",
                    "A3ERP"
                },
                {
                    "Empresa",
                    empresaActiva ?? string.Empty
                },
                {
                    "BaseDades",
                    baseDadesEmpresa ?? string.Empty
                },
                {
                    "ConnexioRebuda",
                    string.IsNullOrWhiteSpace(
                        cadenaConnexio)
                        ? "No"
                        : "Sí"
                }
            };
        }

        private static Dictionary<string, object> CrearCampsContext(
            ContextGestorFormulesA3Erp context)
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
    }
}
