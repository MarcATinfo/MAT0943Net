using A3ErpCalculadorTarifes.Integration;
using A3ErpImportadorArticles.Integration;
using MAT0943Net.Infrastructure.Runtime;
using System;
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
        public string[] ListaProcedimientos()
        {
            return new string[]
            {
                "Iniciar",
                "Finalizar",
                "Opcion"
            };
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

                _runtimeContext.InicialitzarDesA3Erp(
                    conexionSistema,
                    conexionEmpresa);
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
