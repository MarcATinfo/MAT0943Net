using A3ErpCalculadorTarifes.Integration;
using A3ErpImportadorArticles.Infrastructure.Connections;
using A3ErpImportadorArticles.Integration;
using A3ErpImportadorArticles.Infrastructure.Logging;
using A3ErpImportadorArticles.Models;
using MAT0943Net.Infrastructure.Articles;
using MAT0943Net.Infrastructure.Events;
using MAT0943Net.Infrastructure.Runtime;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace MAT0943Net
{
    /// <summary>
    /// Classe COM principal que a3ERP instanciarà
    /// per obrir les opcions .NET de MAT0943.
    ///
    /// Responsabilitats:
    /// - rebre les connexions d'empresa i sistema proporcionades per a3ERP;
    /// - identificar la connexió corresponent a l'empresa activa;
    /// - conservar el context mentre l'empresa continuï oberta;
    /// - respondre a les opcions de menú de l'importador i del calculador;
    /// - netejar el context quan a3ERP tanqui l'empresa.
    /// </summary>
    [ComVisible(true)]
    [Guid("7D85F867-1458-4AAA-9587-56FB1829BCA4")]
    [ProgId("MAT0943Net.Principal")]
    [ClassInterface(ClassInterfaceType.AutoDual)]
    public class Principal
    {
        private const string IdOpcioImportador =
            "IMPORTARARTICULOS";

        private const string IdOpcioCalculador =
            "CALCULADORTARIFAS";

        private const string AliasOpcioImportador =
            "MuestraFrmImportadorArticulos";

        private const string AliasOpcioCalculador =
            "MuestraFrmCalculadorTarifas";

        private const string EventAntesDeGuardarMaestroV2 =
            "AntesDeGuardarMaestroV2";

        private const string TablaArticulo =
            "ARTICULO";

        private const int EstadoMaestroAlta =
            0;

        private const int EstadoMaestroEdicion =
            1;

        private static readonly string[] CampsFormulaPrcCoste =
        {
            "PRCCOMPRA",
            "DESC1",
            "DESC2",
            "DESC3",
            "DESC4",
            "PRCSTANDARD",
            "PRCCOSTE"
        };

        /// <summary>
        /// Context mantingut durant la sessió de l'empresa activa.
        /// </summary>
        private readonly ContextRuntimeA3Erp _runtimeContext =
            new ContextRuntimeA3Erp();

        /// <summary>
        /// Conserva un possible error produït durant Iniciar.
        ///
        /// No mostrem cap missatge en aquell moment per evitar
        /// interrompre l'entrada de l'usuari a l'empresa d'a3ERP.
        /// L'error es mostrarà només si intenta obrir una opció.
        /// </summary>
        private string _errorInicialitzacio = string.Empty;

        /// <summary>
        /// Retorna a a3ERP els procediments disponibles en aquesta DLL.
        /// </summary>
        public object[] ListaProcedimientos()
        {
            return new object[]
            {
                "Iniciar",
                "Finalizar",
                "Opcion",
                EventAntesDeGuardarMaestroV2
            };
        }

        /// <summary>
        /// Event de mestre invocat per a3ERP abans de confirmar el guardat.
        ///
        /// Només recalcula PRCCOSTE per ARTICULO i modifica el mateix payload
        /// rebut, de manera que a3ERP persisteixi el valor en el seu guardat
        /// normal. No fa SQL directe, no crida Guarda() i no interfereix amb
        /// el flux ja validat de l'importador.
        /// </summary>
        public bool AntesDeGuardarMaestroV2(
            string tabla,
            ref object datos,
            int estado)
        {
            try
            {
                if (!string.Equals(
                    (tabla ?? string.Empty).Trim(),
                    TablaArticulo,
                    StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }

                if (estado != EstadoMaestroAlta &&
                    estado != EstadoMaestroEdicion)
                {
                    ImportadorArticlesLogger.Debug(
                        "Event de mestre ignorat perquè l'estat no correspon a alta/modificació.",
                        new Dictionary<string, object>
                        {
                            { "Event", EventAntesDeGuardarMaestroV2 },
                            { "Tabla", tabla },
                            { "Estado", estado }
                        });

                    return true;
                }

                A3ErpMaestroEventData eventData;
                string motiuPayload;

                if (!A3ErpMaestroEventData.TryCreate(
                    datos,
                    out eventData,
                    out motiuPayload))
                {
                    ImportadorArticlesLogger.Advertencia(
                        "No s'ha pogut interpretar el payload del guardat d'ARTICULO. Es deixa continuar el guardat.",
                        new Dictionary<string, object>
                        {
                            { "Event", EventAntesDeGuardarMaestroV2 },
                            { "Tabla", tabla },
                            { "Estado", estado },
                            { "Motiu", motiuPayload }
                        });

                    return true;
                }

                string campAbsent =
                    ObtenirPrimerCampFormulaAbsent(
                        eventData);

                if (!string.IsNullOrWhiteSpace(
                    campAbsent))
                {
                    ImportadorArticlesLogger.Advertencia(
                        "No es recalcula PRCCOSTE perquè el payload del mestre no conté tots els camps necessaris. Es deixa continuar el guardat.",
                        new Dictionary<string, object>
                        {
                            { "Event", EventAntesDeGuardarMaestroV2 },
                            { "Tabla", tabla },
                            { "Estado", estado },
                            { "CampFaltant", campAbsent }
                        });

                    return true;
                }

                double prcCompra;
                double desc1;
                double desc2;
                double desc3;
                double desc4;
                double prcStandard;

                if (!TryLlegirCampNumericFormula(
                    eventData,
                    "PRCCOMPRA",
                    out prcCompra,
                    tabla,
                    estado)
                    ||
                    !TryLlegirCampNumericFormula(
                        eventData,
                        "DESC1",
                        out desc1,
                        tabla,
                        estado)
                    ||
                    !TryLlegirCampNumericFormula(
                        eventData,
                        "DESC2",
                        out desc2,
                        tabla,
                        estado)
                    ||
                    !TryLlegirCampNumericFormula(
                        eventData,
                        "DESC3",
                        out desc3,
                        tabla,
                        estado)
                    ||
                    !TryLlegirCampNumericFormula(
                        eventData,
                        "DESC4",
                        out desc4,
                        tabla,
                        estado)
                    ||
                    !TryLlegirCampNumericFormula(
                        eventData,
                        "PRCSTANDARD",
                        out prcStandard,
                        tabla,
                        estado))
                {
                    return true;
                }

                double prcCosteCalculat =
                    CalculadoraPrcCosteArticle.Calcular(
                        prcCompra,
                        desc1,
                        desc2,
                        desc3,
                        desc4,
                        prcStandard);

                if (!eventData.TryAssignarValor(
                    "PRCCOSTE",
                    prcCosteCalculat))
                {
                    ImportadorArticlesLogger.Advertencia(
                        "No s'ha pogut assignar PRCCOSTE al payload del mestre. Es deixa continuar el guardat.",
                        new Dictionary<string, object>
                        {
                            { "Event", EventAntesDeGuardarMaestroV2 },
                            { "Tabla", tabla },
                            { "Estado", estado },
                            { "PRCCOSTECalculat", prcCosteCalculat }
                        });

                    return true;
                }

                ImportadorArticlesLogger.Debug(
                    "S'ha recalculat PRCCOSTE a AntesDeGuardarMaestroV2 d'ARTICULO.",
                    CrearCampsLogEventMaestro(
                        tabla,
                        estado,
                        prcCompra,
                        desc1,
                        desc2,
                        desc3,
                        desc4,
                        prcStandard,
                        prcCosteCalculat));

                return true;
            }
            catch (Exception ex)
            {
                ImportadorArticlesLogger.Error(
                    "Error no bloquejant recalculant PRCCOSTE en el guardat d'ARTICULO.",
                    ex,
                    new Dictionary<string, object>
                    {
                        { "Event", EventAntesDeGuardarMaestroV2 },
                        { "Tabla", tabla },
                        { "Estado", estado }
                    });

                return true;
            }
        }

        /// <summary>
        /// Rep les connexions quan a3ERP entra en una empresa.
        /// </summary>
        public void Iniciar(
            string conexionSistema,
            string conexionEmpresa)
        {
            try
            {
                _errorInicialitzacio = string.Empty;

                ImportadorArticlesLogger.InicialitzarLogReserva();

                _runtimeContext.InicialitzarDesA3Erp(
                    conexionSistema,
                    conexionEmpresa);

                InicialitzarLogOperatiuImportador();
            }
            catch (Exception ex)
            {
                /*
                 * No propaguem l'error perquè Iniciar s'executa
                 * mentre a3ERP està entrant a l'empresa.
                 *
                 * Un error de MAT0943Net no ha de bloquejar
                 * l'accés normal de l'usuari a a3ERP.
                 */
                _runtimeContext.Netejar();
                _errorInicialitzacio = ex.Message;
            }
        }

        /// <summary>
        /// Neteja les connexions i dades de l'empresa anterior.
        /// </summary>
        public void Finalizar()
        {
            try
            {
                _runtimeContext.Netejar();
                _errorInicialitzacio = string.Empty;
            }
            catch
            {
                /*
                 * No propaguem errors durant el tancament.
                 * Finalizar no ha d'impedir que a3ERP tanqui
                 * correctament l'empresa o l'aplicació.
                 */
            }
        }

        /// <summary>
        /// Punt d'entrada utilitzat per les opcions de menú ActiveX.
        ///
        /// El text situat després del segon símbol @ del fitxer XML
        /// arriba a aquest mètode mitjançant IdOpcion.
        /// </summary>
        public void Opcion(
            string IdOpcion,
            string parametro)
        {
            try
            {
                string idNormalitzat =
                    NormalitzarIdOpcion(
                        IdOpcion);

                if (EsOpcioImportador(
                    idNormalitzat))
                {
                    ObrirImportador(
                        origenCrida: "Opcion",
                        parametro: parametro);

                    return;
                }

                if (EsOpcioCalculador(
                    idNormalitzat))
                {
                    ObrirCalculador(
                        origenCrida: "Opcion",
                        parametro: parametro);

                    return;
                }

                MessageBox.Show(
                    "L'opció de menú rebuda no és reconeguda."
                    + Environment.NewLine
                    + Environment.NewLine
                    + "Opció: "
                    + Convert.ToString(IdOpcion),
                    "MAT0943Net",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
            }
            catch (Exception ex)
            {
                MostrarErrorObertura(ex);
            }
        }

        /// <summary>
        /// Punt d'entrada públic alternatiu per obrir
        /// l'importador d'articles.
        /// </summary>
        public void MuestraFrmImportadorArticulos(
            [Optional] object parametro)
        {
            ObrirImportador(
                origenCrida: AliasOpcioImportador,
                parametro: parametro);
        }

        /// <summary>
        /// Punt d'entrada públic alternatiu per obrir
        /// el calculador de tarifes.
        /// </summary>
        public void MuestraFrmCalculadorTarifas(
            [Optional] object parametro)
        {
            ObrirCalculador(
                origenCrida: AliasOpcioCalculador,
                parametro: parametro);
        }

        /// <summary>
        /// Punt d'entrada alternatiu per a versions o configuracions
        /// d'a3ERP que invoquin Opcion amb un únic paràmetre.
        /// </summary>
        public void Opcion(
            [Optional] object parametro)
        {
            ObrirImportador(
                origenCrida: "Opcion(object)",
                parametro: parametro);
        }

        /// <summary>
        /// Comprova que existeixi una empresa inicialitzada
        /// i obre l'importador amb la connexió capturada per a3ERP.
        /// </summary>
        private void ObrirImportador(
            string origenCrida,
            object parametro)
        {
            try
            {
                if (!_runtimeContext.TeEmpresaInicialitzada)
                {
                    MostrarContextNoDisponible(
                        origenCrida,
                        parametro);

                    return;
                }

                PuntEntradaImportador.Obrir(
                    _runtimeContext.BaseDadesEmpresa,
                    _runtimeContext.ConnexioEmpresa);
            }
            catch (Exception ex)
            {
                MostrarErrorObertura(ex);
            }
        }

        /// <summary>
        /// Comprova que existeixi una empresa inicialitzada
        /// i obre el calculador amb la connexió capturada per a3ERP.
        /// </summary>
        private void ObrirCalculador(
            string origenCrida,
            object parametro)
        {
            try
            {
                if (!_runtimeContext.TeEmpresaInicialitzada)
                {
                    MostrarContextNoDisponibleCalculador(
                        origenCrida,
                        parametro);

                    return;
                }

                PuntEntradaCalculador.Obrir(
                    _runtimeContext.BaseDadesEmpresa,
                    _runtimeContext.ConnexioEmpresa);
            }
            catch (Exception ex)
            {
                MostrarErrorOberturaCalculador(ex);
            }
        }

        private static string NormalitzarIdOpcion(
            string idOpcion)
        {
            return string.IsNullOrWhiteSpace(idOpcion)
                ? string.Empty
                : idOpcion.Trim();
        }

        private static bool EsOpcioImportador(
            string idOpcion)
        {
            return
                string.Equals(
                    idOpcion,
                    IdOpcioImportador,
                    StringComparison.OrdinalIgnoreCase)
                ||
                string.Equals(
                    idOpcion,
                    AliasOpcioImportador,
                    StringComparison.OrdinalIgnoreCase);
        }

        private static bool EsOpcioCalculador(
            string idOpcion)
        {
            return
                string.Equals(
                    idOpcion,
                    IdOpcioCalculador,
                    StringComparison.OrdinalIgnoreCase)
                ||
                string.Equals(
                    idOpcion,
                    AliasOpcioCalculador,
                    StringComparison.OrdinalIgnoreCase);
        }

        private static string ObtenirPrimerCampFormulaAbsent(
            A3ErpMaestroEventData eventData)
        {
            foreach (string camp in CampsFormulaPrcCoste)
            {
                if (!eventData.ConteCamp(
                    camp))
                {
                    return camp;
                }
            }

            return string.Empty;
        }

        private static bool TryLlegirCampNumericFormula(
            A3ErpMaestroEventData eventData,
            string camp,
            out double valor,
            string tabla,
            int estado)
        {
            string motiu;

            if (eventData.TryObtenirDouble(
                camp,
                out valor,
                out motiu))
            {
                return true;
            }

            ImportadorArticlesLogger.Advertencia(
                "No es recalcula PRCCOSTE perquè un camp de la fórmula no és numèric. Es deixa continuar el guardat.",
                new Dictionary<string, object>
                {
                    { "Event", EventAntesDeGuardarMaestroV2 },
                    { "Tabla", tabla },
                    { "Estado", estado },
                    { "Camp", camp },
                    { "Motiu", motiu }
                });

            return false;
        }

        private static Dictionary<string, object> CrearCampsLogEventMaestro(
            string tabla,
            int estado,
            double prcCompra,
            double desc1,
            double desc2,
            double desc3,
            double desc4,
            double prcStandard,
            double prcCosteCalculat)
        {
            return new Dictionary<string, object>
            {
                { "Event", EventAntesDeGuardarMaestroV2 },
                { "Tabla", tabla },
                { "Estado", estado },
                { "PRCCOMPRA", prcCompra.ToString(CultureInfo.InvariantCulture) },
                { "DESC1", desc1.ToString(CultureInfo.InvariantCulture) },
                { "DESC2", desc2.ToString(CultureInfo.InvariantCulture) },
                { "DESC3", desc3.ToString(CultureInfo.InvariantCulture) },
                { "DESC4", desc4.ToString(CultureInfo.InvariantCulture) },
                { "PRCSTANDARD", prcStandard.ToString(CultureInfo.InvariantCulture) },
                { "PRCCOSTECalculat", prcCosteCalculat.ToString(CultureInfo.InvariantCulture) }
            };
        }

        private void InicialitzarLogOperatiuImportador()
        {
            try
            {
                if (!_runtimeContext.TeEmpresaInicialitzada)
                {
                    return;
                }

                ServeiResolucioConnexioA3Erp serveiConnexio =
                    new ServeiResolucioConnexioA3Erp();

                ResultatResolucioConnexio resultat =
                    serveiConnexio.Resoldre(
                        _runtimeContext.BaseDadesEmpresa,
                        _runtimeContext.ConnexioEmpresa);

                if (!resultat.Correcte)
                {
                    ImportadorArticlesLogger.AdvertenciaArrencada(
                        "No s'ha pogut resoldre la connexió per inicialitzar el log operatiu des de Principal.Iniciar. Es manté el log local de reserva.",
                        new Dictionary<string, object>
                        {
                            {
                                "Error",
                                resultat.Missatge
                            },
                            {
                                "ErrorConnexioOriginal",
                                resultat.ErrorConnexioOriginal
                            }
                        });

                    return;
                }

                InicialitzadorLogImportador.Inicialitzar(
                    resultat.Context);

                ImportadorArticlesLogger.InformacioArrencada(
                    "S'ha resolt la connexió per inicialitzar el log operatiu des de Principal.Iniciar.",
                    new Dictionary<string, object>
                    {
                        {
                            "OrigenConnexio",
                            resultat.Context.OrigenConnexio
                        },
                        {
                            "UtilitzaConnexioAlternativa",
                            resultat.Context.UtilitzaConnexioAlternativa
                        },
                        {
                            "BaseDades",
                            resultat.Context.BaseDadesEmpresa
                        }
                    });
            }
            catch (Exception ex)
            {
                ImportadorArticlesLogger.AdvertenciaArrencada(
                    "No s'ha pogut inicialitzar el log operatiu des de Principal.Iniciar. Es manté el log local de reserva.",
                    new Dictionary<string, object>
                    {
                        {
                            "Error",
                            ex.Message
                        }
                    });
            }
        }

        /// <summary>
        /// Informa que a3ERP no ha pogut proporcionar
        /// un context d'empresa vàlid.
        /// </summary>
        private void MostrarContextNoDisponible(
            string origenCrida,
            object parametro)
        {
            string detallError =
                string.IsNullOrWhiteSpace(_errorInicialitzacio)
                    ? "No s'ha rebut una connexió vàlida de l'empresa activa."
                    : _errorInicialitzacio;

            MessageBox.Show(
                "No es pot obrir l'importador perquè no hi ha "
                + "cap empresa inicialitzada correctament."
                + Environment.NewLine
                + Environment.NewLine
                + detallError
                + Environment.NewLine
                + Environment.NewLine
                + "Origen de la crida: "
                + Convert.ToString(origenCrida)
                + Environment.NewLine
                + "Paràmetre: "
                + Convert.ToString(parametro),
                "Importador d'articles",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning);
        }

        /// <summary>
        /// Informa que a3ERP no ha pogut proporcionar
        /// un context d'empresa vàlid al calculador.
        /// </summary>
        private void MostrarContextNoDisponibleCalculador(
            string origenCrida,
            object parametro)
        {
            string detallError =
                string.IsNullOrWhiteSpace(_errorInicialitzacio)
                    ? "No s'ha rebut una connexió vàlida de l'empresa activa."
                    : _errorInicialitzacio;

            MessageBox.Show(
                "No es pot obrir el calculador de tarifes perquè no hi ha "
                + "cap empresa inicialitzada correctament."
                + Environment.NewLine
                + Environment.NewLine
                + detallError
                + Environment.NewLine
                + Environment.NewLine
                + "Origen de la crida: "
                + Convert.ToString(origenCrida)
                + Environment.NewLine
                + "Paràmetre: "
                + Convert.ToString(parametro),
                "Calculador de tarifes",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning);
        }

        /// <summary>
        /// Mostra un error controlat si l'importador
        /// no s'ha pogut obrir.
        /// </summary>
        private static void MostrarErrorObertura(
            Exception excepcio)
        {
            string detall = excepcio == null
                ? "S'ha produït un error desconegut."
                : excepcio.Message;

            MessageBox.Show(
                "No s'ha pogut obrir l'importador d'articles."
                + Environment.NewLine
                + Environment.NewLine
                + detall,
                "Importador d'articles",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
        }

        /// <summary>
        /// Mostra un error controlat si el calculador
        /// no s'ha pogut obrir.
        /// </summary>
        private static void MostrarErrorOberturaCalculador(
            Exception excepcio)
        {
            string detall = excepcio == null
                ? "S'ha produït un error desconegut."
                : excepcio.Message;

            MessageBox.Show(
                "No s'ha pogut obrir el calculador de tarifes."
                + Environment.NewLine
                + Environment.NewLine
                + detall,
                "Calculador de tarifes",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
        }
    }
}
