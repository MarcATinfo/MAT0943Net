using A3ErpGestorFormulesTarifes.Dades;
using A3ErpGestorFormulesTarifes.Models;
using System;
using System.Collections.Generic;
using System.Data.OleDb;
using System.Linq;
using System.Windows.Forms;
using A3ErpGestorFormulesTarifes.Infrastructure.Logging;
using MAT0943Net.Infrastructure;

namespace A3ErpGestorFormulesTarifes
{
    /// <summary>
    /// Formulari principal del gestor de fórmules
    /// configurables del Calculador de tarifes.
    ///
    /// Carrega les fórmules de la base de dades
    /// i permet crear, editar, duplicar,
    /// activar, desactivar i eliminar-les
    /// de manera lògica.
    ///
    /// Les fórmules eliminades no es mostren
    /// al llistat principal del gestor.
    /// </summary>
    public partial class FrmGestorFormulesTarifes : Form
    {
        /// <summary>
        /// Context d'a3ERP rebut pel formulari.
        ///
        /// Conté l'empresa activa, la base de dades
        /// i la connexió ja resolta pel procés d'origen.
        /// </summary>
        private readonly ContextGestorFormulesA3Erp
            _contextA3Erp;

        /// <summary>
        /// Repositori encarregat de llegir i, més endavant,
        /// persistir les fórmules configurables.
        /// </summary>
        private readonly RepositoriFormulesTarifes
            _repositoriFormules;

        /// <summary>
        /// Fórmules carregades actualment al gestor.
        ///
        /// En aquesta fase la col·lecció s'inicialitza buida.
        /// La lectura des de SQL s'afegirà al pas següent.
        /// </summary>
        private List<FormulaTarifa>
            _formules;

        /// <summary>
        /// Evita que l'esdeveniment Shown carregui
        /// les fórmules més d'una vegada durant
        /// la mateixa obertura del formulari.
        /// </summary>
        private bool _carregaInicialExecutada;

        /// <summary>
        /// Constructor utilitzat quan l'aplicació
        /// s'executa directament des de Visual Studio.
        ///
        /// En aquest mode no hi ha empresa activa
        /// ni connexió disponible.
        /// </summary>
        public FrmGestorFormulesTarifes()
            : this(
                ContextGestorFormulesA3Erp
                    .CrearModeDesenvolupament())
        {
        }

        /// <summary>
        /// Constructor utilitzat quan el gestor
        /// rep el context real de l'empresa activa.
        /// </summary>
        /// <param name="context">
        /// Context d'a3ERP necessari per treballar
        /// amb les fórmules de l'empresa activa.
        /// </param>
        public FrmGestorFormulesTarifes(
            ContextGestorFormulesA3Erp context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(
                    nameof(context));
            }

            _contextA3Erp =
                context;

            InicialitzarLogGestor();

            InitializeComponent();

            IconaAplicacio.Aplicar(
                this);

            /*
             * Evitem que la tipografia depengui del procés
             * que allotja el formulari: executable propi o a3ERP.
             */
            AplicarRenderitzatText(
                this);

            /*
            * Configurem les columnes de la graella
            * abans de carregar cap fórmula.
            */
            ConfigurarGraellaFormules();

            _repositoriFormules =
                new RepositoriFormulesTarifes();

            _formules =
                new List<FormulaTarifa>();

            _carregaInicialExecutada =
                false;

            GestorFormulesTarifesLogger.Informacio(
                "S'inicia el gestor de fórmules.",
                CrearCampsContextLog(
                    new Dictionary<string, object>
                    {
                        {
                            "LogActiu",
                            GestorFormulesTarifesLogger.LogActiu
                        }
                    }));

            /*
             * La primera lectura es farà quan el formulari
             * ja s'hagi mostrat.
             *
             * Això evita executar accés a dades abans
             * que la inicialització visual hagi finalitzat.
             */
            Shown +=
                FrmGestorFormulesTarifes_Shown;

            FormClosed +=
                FrmGestorFormulesTarifes_FormClosed;

            /*
             * Actualitzem les accions disponibles
             * cada vegada que canvia la fórmula seleccionada.
             */
            dgvFormules.SelectionChanged +=
                dgvFormules_SelectionChanged;

            btnCanviarEstat.Click +=
                btnCanviarEstat_Click;

            /*
             * Obre el formulari per crear
             * una fórmula nova.
             */
            btnNova.Click +=
                btnNova_Click;

            /*
            * Obre l'edició de la fórmula seleccionada.
            */
            btnEditar.Click +=
                btnEditar_Click;

            /*
             * Crea una fórmula nova a partir
             * de la fórmula seleccionada.
             */
            btnDuplicar.Click +=
                btnDuplicar_Click;

            /*
             * Elimina lògicament
             * la fórmula seleccionada.
             */
            btnEliminar.Click +=
                btnEliminar_Click;

            /*
             * Inicialment encara no hi ha cap fórmula seleccionada.
             */
            ActualitzarEstatAccions();
        }

        private void InicialitzarLogGestor()
        {
            GestorFormulesTarifesLogger.InicialitzarLogReserva();

            if (!_contextA3Erp.ConnexioDisponible)
            {
                return;
            }

            try
            {
                RepositoriConfiguracioLogGestorFormulesTarifes
                    repositoriConfiguracio =
                        new RepositoriConfiguracioLogGestorFormulesTarifes();

                ConfiguracioLogGestorFormulesTarifes configuracio =
                    repositoriConfiguracio.Carregar(
                        _contextA3Erp.CadenaConnexio);

                GestorFormulesTarifesLogger.AplicarConfiguracioLog(
                    configuracio);
            }
            catch (Exception ex)
            {
                GestorFormulesTarifesLogger.Error(
                    "No s'ha pogut llegir la configuració del log del gestor de fórmules. Es continua amb el log local de reserva.",
                    ex,
                    CrearCampsContextLog());
            }
        }

        private Dictionary<string, object> CrearCampsContextLog(
            Dictionary<string, object> camps = null)
        {
            Dictionary<string, object> resultat =
                camps == null
                    ? new Dictionary<string, object>()
                    : new Dictionary<string, object>(
                        camps);

            resultat["Empresa"] =
                _contextA3Erp.EmpresaActiva
                ?? string.Empty;

            resultat["BaseDades"] =
                _contextA3Erp.BaseDadesEmpresa
                ?? string.Empty;

            return resultat;
        }

        private Dictionary<string, object> CrearCampsFormulaLog(
            FormulaTarifa formula,
            Dictionary<string, object> camps = null)
        {
            Dictionary<string, object> resultat =
                CrearCampsContextLog(
                    camps);

            if (formula == null)
            {
                return resultat;
            }

            resultat["FormulaId"] =
                formula.Id;

            resultat["FormulaCodi"] =
                formula.Codi
                ?? string.Empty;

            resultat["FormulaNom"] =
                formula.Nom
                ?? string.Empty;

            resultat["FormulaOrdre"] =
                formula.Ordre;

            resultat["FormulaActiva"] =
                formula.Activa;

            return resultat;
        }

        /// <summary>
        /// Executa la primera càrrega de fórmules
        /// quan el formulari ja està visible.
        /// </summary>
        private void FrmGestorFormulesTarifes_Shown(
            object sender,
            EventArgs e)
        {
            if (_carregaInicialExecutada)
            {
                return;
            }

            _carregaInicialExecutada =
                true;

            /*
             * En execució directa des de Visual Studio,
             * el context de desenvolupament no disposa
             * encara d'una connexió real.
             */
            if (!_contextA3Erp.ConnexioDisponible)
            {
                GestorFormulesTarifesLogger.Advertencia(
                    "No s'han pogut carregar les fórmules perquè no hi ha connexió disponible.",
                    CrearCampsContextLog());

                MessageBox.Show(
                    this,
                    "El gestor s'ha obert sense una connexió "
                    + "d'empresa disponible.\r\n\r\n"
                    + "No s'ha executat cap consulta.",
                    "Gestor de fórmules",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);

                return;
            }

            CarregarFormulesInicials();

        }

        /// <summary>
        /// Recupera les fórmules no eliminades
        /// de l'empresa activa.
        ///
        /// En aquesta primera prova, el resultat
        /// només es conserva en memòria i se'n mostra
        /// el nombre de registres carregats.
        /// </summary>
        private void CarregarFormulesInicials()
        {
            try
            {
                _formules =
                    _repositoriFormules.ObtenirPerGestor(
                        _contextA3Erp.CadenaConnexio);

                MostrarFormulesCarregades();

                GestorFormulesTarifesLogger.Informacio(
                    "S'han carregat les fórmules del gestor.",
                    CrearCampsContextLog(
                        new Dictionary<string, object>
                        {
                            {
                                "TotalFormules",
                                _formules.Count
                            }
                        }));

                GestorFormulesTarifesLogger.Informacio(
                    "Gestor de fórmules inicialitzat correctament.",
                    CrearCampsContextLog());
            }
            catch (Exception ex)
            {
                GestorFormulesTarifesLogger.Error(
                    "Error en carregar les fórmules del gestor.",
                    ex,
                    CrearCampsContextLog());

                /*
                 * Evitem conservar dades parcials
                 * si la lectura no finalitza correctament.
                 */
                _formules.Clear();

                MessageBox.Show(
                    this,
                    "No s'han pogut carregar les fórmules "
                    + "de tarifes.\r\n\r\n"
                    + ex.Message,
                    "Error de lectura",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        private void FrmGestorFormulesTarifes_FormClosed(
            object sender,
            FormClosedEventArgs e)
        {
            GestorFormulesTarifesLogger.Informacio(
                "Es tanca el gestor de fórmules.",
                CrearCampsContextLog());
        }

        /// <summary>
        /// Força el mateix sistema de renderització de text
        /// tant quan el formulari s'executa directament com
        /// quan s'obre dins del procés d'a3ERP.
        ///
        /// Aquesta configuració evita diferències visuals
        /// en etiquetes i botons segons el procés que allotja
        /// el formulari.
        /// </summary>
        private static void AplicarRenderitzatText(
            Control control)
        {
            if (control == null)
            {
                return;
            }

            /*
             * GDI és el mateix sistema que utilitza
             * Application.SetCompatibleTextRenderingDefault(false).
             */
            if (control is Label etiqueta)
            {
                etiqueta.UseCompatibleTextRendering =
                    false;
            }
            else if (control is ButtonBase boto)
            {
                boto.UseCompatibleTextRendering =
                    false;
            }

            /*
             * Recorrem recursivament tots els controls fills
             * perquè la configuració també s'apliqui als controls
             * continguts dins de panells i disposicions.
             */
            foreach (
                Control controlFill
                in control.Controls)
            {
                AplicarRenderitzatText(
                    controlFill);
            }
        }

        /// <summary>
        /// Configura les columnes visibles
        /// del llistat de fórmules.
        ///
        /// La graella és exclusivament de consulta.
        /// L'edició es farà posteriorment mitjançant
        /// un formulari específic.
        /// </summary>
        private void ConfigurarGraellaFormules()
        {
            dgvFormules.Columns.Clear();

            dgvFormules.AutoGenerateColumns =
                false;

            dgvFormules.EnableHeadersVisualStyles =
                false;

            dgvFormules.ColumnHeadersDefaultCellStyle.BackColor =
                System.Drawing.Color.FromArgb(
                    241,
                    245,
                    249);

            dgvFormules.ColumnHeadersDefaultCellStyle.ForeColor =
                System.Drawing.Color.FromArgb(
                    51,
                    65,
                    85);

            dgvFormules.ColumnHeadersDefaultCellStyle.Font =
                new System.Drawing.Font(
                    "Segoe UI Semibold",
                    9F);

            dgvFormules.ColumnHeadersHeight =
                38;

            dgvFormules.RowTemplate.Height =
                34;

            dgvFormules.DefaultCellStyle.SelectionBackColor =
                System.Drawing.Color.FromArgb(
                    224,
                    242,
                    254);

            dgvFormules.DefaultCellStyle.SelectionForeColor =
                System.Drawing.Color.FromArgb(
                    15,
                    23,
                    42);

            dgvFormules.AlternatingRowsDefaultCellStyle.BackColor =
                System.Drawing.Color.FromArgb(
                    248,
                    250,
                    252);

            /*
             * Ordre visual de la fórmula.
             */
            dgvFormules.Columns.Add(
                CrearColumnaText(
                    propietat: nameof(FormulaTarifa.Ordre),
                    titol: "Ordre",
                    amplada: 70));

            /*
             * Codi intern estable.
             */
            dgvFormules.Columns.Add(
                CrearColumnaText(
                    propietat: nameof(FormulaTarifa.Codi),
                    titol: "Codi",
                    amplada: 245));

            /*
             * Nom visible de la fórmula.
             */
            DataGridViewTextBoxColumn columnaNom =
                CrearColumnaText(
                    propietat: nameof(FormulaTarifa.Nom),
                    titol: "Nom",
                    amplada: 300);

            columnaNom.AutoSizeMode =
                DataGridViewAutoSizeColumnMode.Fill;

            columnaNom.MinimumWidth =
                240;

            dgvFormules.Columns.Add(
                columnaNom);

            /*
             * Tipus dels paràmetres P1-P4.
             */
            dgvFormules.Columns.Add(
                CrearColumnaText(
                    propietat: nameof(FormulaTarifa.TipusValors),
                    titol: "Tipus de valors",
                    amplada: 190));

            /*
             * Estat actiu de la fórmula.
             */
            dgvFormules.Columns.Add(
                CrearColumnaBoolea(
                    propietat: nameof(FormulaTarifa.Activa),
                    titol: "Activa",
                    amplada: 75));

            /*
             * Indica si la fórmula genera descomptes.
             */
            dgvFormules.Columns.Add(
                CrearColumnaBoolea(
                    propietat: nameof(FormulaTarifa.GeneraDescomptes),
                    titol: "Descomptes",
                    amplada: 100));
        }

        /// <summary>
        /// Crea una columna textual vinculada
        /// a una propietat de FormulaTarifa.
        /// </summary>
        private static DataGridViewTextBoxColumn
            CrearColumnaText(
                string propietat,
                string titol,
                int amplada)
        {
            return new DataGridViewTextBoxColumn
            {
                DataPropertyName =
                    propietat,

                HeaderText =
                    titol,

                Name =
                    "col" + propietat,

                ReadOnly =
                    true,

                Width =
                    amplada,

                SortMode =
                    DataGridViewColumnSortMode.Automatic
            };
        }

        /// <summary>
        /// Crea una columna de selecció vinculada
        /// a una propietat booleana de FormulaTarifa.
        /// </summary>
        private static DataGridViewCheckBoxColumn
            CrearColumnaBoolea(
                string propietat,
                string titol,
                int amplada)
        {
            return new DataGridViewCheckBoxColumn
            {
                DataPropertyName =
                    propietat,

                HeaderText =
                    titol,

                Name =
                    "col" + propietat,

                ReadOnly =
                    true,

                Width =
                    amplada,

                SortMode =
                    DataGridViewColumnSortMode.Automatic
            };
        }

        /// <summary>
        /// Mostra a la graella les fórmules recuperades
        /// de la base de dades i actualitza l'estat
        /// general del formulari.
        ///
        /// Quan es rep un identificador, intenta mantenir
        /// seleccionada aquella mateixa fórmula després
        /// de recarregar les dades.
        /// </summary>
        private void MostrarFormulesCarregades(
            int? idFormulaSeleccionar = null)
        {
            dgvFormules.DataSource =
                null;

            dgvFormules.DataSource =
                _formules;

            lblEstat.Text =
                _formules.Count
                + " fórmules carregades de l'empresa "
                + _contextA3Erp.BaseDadesEmpresa
                + ".";

            if (dgvFormules.Rows.Count == 0)
            {
                ActualitzarEstatAccions();

                return;
            }

            dgvFormules.ClearSelection();

            DataGridViewRow filaSeleccionar =
                null;

            /*
             * Intentem recuperar la mateixa fórmula
             * que estava seleccionada abans de recarregar.
             */
            if (idFormulaSeleccionar.HasValue)
            {
                foreach (
                    DataGridViewRow fila
                    in dgvFormules.Rows)
                {
                    FormulaTarifa formulaFila =
                        fila.DataBoundItem
                        as FormulaTarifa;

                    if (formulaFila != null
                        && formulaFila.Id
                            == idFormulaSeleccionar.Value)
                    {
                        filaSeleccionar =
                            fila;

                        break;
                    }
                }
            }

            /*
             * Si no s'ha indicat cap fórmula o ja no existeix,
             * seleccionem la primera fila disponible.
             */
            if (filaSeleccionar == null)
            {
                filaSeleccionar =
                    dgvFormules.Rows[0];
            }

            filaSeleccionar.Selected =
                true;

            dgvFormules.CurrentCell =
                filaSeleccionar.Cells[0];

            ActualitzarEstatAccions();
        }

        /// <summary>
        /// Actualitza els botons d'acció quan
        /// canvia la fórmula seleccionada.
        /// </summary>
        private void dgvFormules_SelectionChanged(
            object sender,
            EventArgs e)
        {
            ActualitzarEstatAccions();
        }

        /// <summary>
        /// Retorna la fórmula vinculada a la fila
        /// seleccionada de la graella.
        ///
        /// Retorna null quan no hi ha cap selecció vàlida.
        /// </summary>
        private FormulaTarifa ObtenirFormulaSeleccionada()
        {
            if (dgvFormules.CurrentRow == null)
            {
                return null;
            }

            return dgvFormules.CurrentRow.DataBoundItem
                as FormulaTarifa;
        }

        /// <summary>
        /// Habilita o deshabilita les accions
        /// segons si hi ha una fórmula seleccionada.
        ///
        /// El text del botó d'estat s'adapta
        /// a la situació actual de la fórmula.
        /// </summary>
        private void ActualitzarEstatAccions()
        {
            FormulaTarifa formula =
                ObtenirFormulaSeleccionada();

            bool hiHaSeleccio =
                formula != null;

            /*
             * Crear una fórmula nova no depèn
             * de la selecció actual.
             */
            btnNova.Enabled =
                true;

            btnEditar.Enabled =
                hiHaSeleccio;

            btnDuplicar.Enabled =
                hiHaSeleccio;

            btnCanviarEstat.Enabled =
                hiHaSeleccio;

            btnEliminar.Enabled =
                hiHaSeleccio;

            if (!hiHaSeleccio)
            {
                btnCanviarEstat.Text =
                    "Activar / Desactivar";

                return;
            }

            btnCanviarEstat.Text =
                formula.Activa
                    ? "Desactivar"
                    : "Activar";
        }

        /// <summary>
        /// Activa o desactiva la fórmula seleccionada
        /// després de demanar confirmació a l'usuari.
        ///
        /// Després de l'actualització, torna a carregar
        /// les dades de SQL i conserva la mateixa selecció.
        /// </summary>
        private void btnCanviarEstat_Click(
            object sender,
            EventArgs e)
        {
            FormulaTarifa formula =
                ObtenirFormulaSeleccionada();

            if (formula == null)
            {
                MessageBox.Show(
                    this,
                    "Selecciona una fórmula abans "
                    + "de canviar-ne l'estat.",
                    "Gestor de fórmules",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);

                return;
            }

            bool activar =
                !formula.Activa;

            bool activaAnterior =
                formula.Activa;

            bool activaNova =
                activar;

            string accio =
                activar
                    ? "activar"
                    : "desactivar";

            DialogResult resposta =
                MessageBox.Show(
                    this,
                    "Vols "
                    + accio
                    + " la fórmula següent?\r\n\r\n"
                    + formula.ToString(),
                    activar
                        ? "Activar fórmula"
                        : "Desactivar fórmula",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question,
                    MessageBoxDefaultButton.Button2);

            if (resposta != DialogResult.Yes)
            {
                return;
            }

            try
            {
                _repositoriFormules.CanviarEstatActiu(
                    _contextA3Erp.CadenaConnexio,
                    formula.Id,
                    activar);

                /*
                 * Tornem a consultar SQL perquè la graella
                 * sempre reflecteixi l'estat real de la BD.
                 */
                _formules =
                    _repositoriFormules.ObtenirPerGestor(
                        _contextA3Erp.CadenaConnexio);

                MostrarFormulesCarregades(
                    formula.Id);

                FormulaTarifa formulaActualitzada =
                    _formules
                        .FirstOrDefault(
                            formulaCarregada =>
                                formulaCarregada.Id ==
                                formula.Id);

                Dictionary<string, object> campsCanviEstat =
                    CrearCampsFormulaLog(
                        formulaActualitzada
                        ?? formula,
                        new Dictionary<string, object>
                        {
                            {
                                "ActivaAnterior",
                                activaAnterior
                            },
                            {
                                "ActivaNova",
                                activaNova
                            },
                            {
                                "TotalFormules",
                                _formules.Count
                            }
                        });

                if (formulaActualitzada == null)
                {
                    campsCanviEstat["FormulaActiva"] =
                        activaNova;
                }

                GestorFormulesTarifesLogger.Informacio(
                    "S'ha canviat l'estat de la fórmula.",
                    campsCanviEstat);

                lblEstat.Text =
                    "La fórmula \""
                    + formula.Nom
                    + "\" s'ha "
                    + (
                        activar
                            ? "activat"
                            : "desactivat"
                    )
                    + " correctament.";
            }
            catch (Exception ex)
            {
                GestorFormulesTarifesLogger.Error(
                    "Error en canviar l'estat de la fórmula.",
                    ex,
                    CrearCampsFormulaLog(
                        formula,
                        new Dictionary<string, object>
                        {
                            {
                                "ActivaAnterior",
                                activaAnterior
                            },
                            {
                                "ActivaNova",
                                activaNova
                            }
                        }));

                MessageBox.Show(
                    this,
                    "No s'ha pogut "
                    + accio
                    + " la fórmula.\r\n\r\n"
                    + ex.Message,
                    "Error en canviar l'estat",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Obre el formulari per crear una fórmula nova.
        ///
        /// Quan les dades són correctes:
        /// - insereix la fórmula a SQL;
        /// - torna a carregar el catàleg;
        /// - selecciona el registre creat.
        /// </summary>
        private void btnNova_Click(
            object sender,
            EventArgs e)
        {
            var formulaNova =
                new FormulaTarifa
                {
                    Ordre =
                        ObtenirSeguentOrdre(),

                    Activa =
                        true,

                    Eliminada =
                        false,

                    TipusValors =
                        "No aplica",

                    ExpressioTarifa5 =
                        "PRCCOSTE",

                    ExpressioTarifa6 =
                        "PRCCOSTE + PRCSTANDARD"
                };

            using (var formulari =
                new FrmEdicioFormulaTarifa(
                    ModeEdicioFormulaTarifa.Nova,
                    formulaNova))
            {
                DialogResult resultat =
                    formulari.ShowDialog(
                        this);

                /*
                 * Cancel·lar o tancar el formulari
                 * no provoca cap modificació a SQL.
                 */
                if (resultat != DialogResult.OK)
                {
                    return;
                }

                FormulaTarifa formulaDesar =
                    formulari.FormulaResultat;

                try
                {
                    int idFormulaNova =
                        _repositoriFormules.InserirFormula(
                            _contextA3Erp.CadenaConnexio,
                            formulaDesar);

                    formulaDesar.Id =
                        idFormulaNova;

                    /*
                     * Tornem a consultar SQL perquè
                     * la graella reflecteixi exactament
                     * el contingut real de la base de dades.
                     */
                    _formules =
                        _repositoriFormules.ObtenirPerGestor(
                            _contextA3Erp.CadenaConnexio);

                    MostrarFormulesCarregades(
                        idFormulaNova);

                    GestorFormulesTarifesLogger.Informacio(
                        "S'ha creat una fórmula.",
                        CrearCampsFormulaLog(
                            formulaDesar,
                            new Dictionary<string, object>
                            {
                                {
                                    "TotalFormules",
                                    _formules.Count
                                }
                            }));

                    lblEstat.Text =
                        "La fórmula \""
                        + formulaDesar.Nom
                        + "\" s'ha creat correctament.";

                    MessageBox.Show(
                        this,
                        "La fórmula s'ha creat correctament.",
                        "Nova fórmula",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);
                }
                catch (OleDbException ex)
                    when (EsErrorCodiDuplicat(ex))
                {
                    GestorFormulesTarifesLogger.Advertencia(
                        "No s'ha pogut crear la fórmula perquè el codi ja existeix.",
                        CrearCampsFormulaLog(
                            formulaDesar));

                    MessageBox.Show(
                        this,
                        "No s'ha pogut crear la fórmula perquè "
                        + "ja existeix una fórmula amb el codi \""
                        + formulaDesar.Codi
                        + "\".\r\n\r\n"
                        + "Informa un codi diferent.",
                        "Codi de fórmula duplicat",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);
                }
                catch (Exception ex)
                {
                    GestorFormulesTarifesLogger.Error(
                        "Error en crear la fórmula.",
                        ex,
                        CrearCampsFormulaLog(
                            formulaDesar));

                    MessageBox.Show(
                        this,
                        "No s'ha pogut crear la fórmula."
                        + "\r\n\r\n"
                        + ex.Message,
                        "Error en crear la fórmula",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error);
                }
            }
        }

        /// <summary>
        /// Obre la fórmula seleccionada en mode edició.
        ///
        /// Quan l'usuari desa:
        /// - actualitza el registre existent;
        /// - torna a consultar SQL;
        /// - conserva seleccionada la mateixa fórmula.
        /// </summary>
        private void btnEditar_Click(
            object sender,
            EventArgs e)
        {
            FormulaTarifa formulaSeleccionada =
                ObtenirFormulaSeleccionada();

            if (formulaSeleccionada == null)
            {
                MessageBox.Show(
                    this,
                    "Selecciona una fórmula abans d'editar-la.",
                    "Editar fórmula",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);

                return;
            }

            using (var formulari =
                new FrmEdicioFormulaTarifa(
                    ModeEdicioFormulaTarifa.Editar,
                    formulaSeleccionada))
            {
                DialogResult resultat =
                    formulari.ShowDialog(
                        this);

                /*
                 * Cancel·lar o tancar el formulari
                 * no modifica la base de dades.
                 */
                if (resultat != DialogResult.OK)
                {
                    return;
                }

                FormulaTarifa formulaDesar =
                    formulari.FormulaResultat;

                try
                {
                    _repositoriFormules.ActualitzarFormula(
                        _contextA3Erp.CadenaConnexio,
                        formulaDesar);

                    /*
                     * Recarreguem des de SQL per mostrar
                     * els valors realment persistits.
                     */
                    _formules =
                        _repositoriFormules.ObtenirPerGestor(
                            _contextA3Erp.CadenaConnexio);

                    MostrarFormulesCarregades(
                        formulaDesar.Id);

                    GestorFormulesTarifesLogger.Informacio(
                        "S'ha editat una fórmula.",
                        CrearCampsFormulaLog(
                            formulaDesar,
                            new Dictionary<string, object>
                            {
                                {
                                    "TotalFormules",
                                    _formules.Count
                                }
                            }));

                    lblEstat.Text =
                        "La fórmula \""
                        + formulaDesar.Nom
                        + "\" s'ha actualitzat correctament.";

                    MessageBox.Show(
                        this,
                        "La fórmula s'ha actualitzat correctament.",
                        "Editar fórmula",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);
                }
                catch (OleDbException ex)
                    when (EsErrorCodiDuplicat(ex))
                {
                    GestorFormulesTarifesLogger.Advertencia(
                        "No s'ha pogut editar la fórmula perquè el codi ja existeix.",
                        CrearCampsFormulaLog(
                            formulaDesar));

                    MessageBox.Show(
                        this,
                        "No s'ha pogut actualitzar la fórmula perquè "
                        + "ja existeix una altra fórmula amb el codi \""
                        + formulaDesar.Codi
                        + "\".\r\n\r\n"
                        + "Informa un codi diferent.",
                        "Codi de fórmula duplicat",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);
                }
                catch (Exception ex)
                {
                    GestorFormulesTarifesLogger.Error(
                        "Error en editar la fórmula.",
                        ex,
                        CrearCampsFormulaLog(
                            formulaDesar));

                    MessageBox.Show(
                        this,
                        "No s'ha pogut actualitzar la fórmula."
                        + "\r\n\r\n"
                        + ex.Message,
                        "Error en editar la fórmula",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error);
                }
            }
        }

        /// <summary>
        /// Obre una còpia de la fórmula seleccionada
        /// perquè l'usuari pugui crear una fórmula nova.
        ///
        /// El formulari de duplicació:
        /// - elimina l'identificador original;
        /// - deixa el codi buit;
        /// - conserva valors, expressions i descomptes;
        /// - proposa el nom amb el sufix "(còpia)".
        /// </summary>
        private void btnDuplicar_Click(
            object sender,
            EventArgs e)
        {
            FormulaTarifa formulaSeleccionada =
                ObtenirFormulaSeleccionada();

            if (formulaSeleccionada == null)
            {
                MessageBox.Show(
                    this,
                    "Selecciona una fórmula abans de duplicar-la.",
                    "Duplicar fórmula",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);

                return;
            }

            using (var formulari =
                new FrmEdicioFormulaTarifa(
                    ModeEdicioFormulaTarifa.Duplicar,
                    formulaSeleccionada))
            {
                DialogResult resultat =
                    formulari.ShowDialog(
                        this);

                /*
                 * Cancel·lar o tancar el formulari
                 * no crea cap registre.
                 */
                if (resultat != DialogResult.OK)
                {
                    return;
                }

                FormulaTarifa formulaDesar =
                    formulari.FormulaResultat;

                try
                {
                    int idFormulaNova =
                        _repositoriFormules.InserirFormula(
                            _contextA3Erp.CadenaConnexio,
                            formulaDesar);

                    formulaDesar.Id =
                        idFormulaNova;

                    /*
                     * Recarreguem la graella des de SQL
                     * i seleccionem la fórmula duplicada.
                     */
                    _formules =
                        _repositoriFormules.ObtenirPerGestor(
                            _contextA3Erp.CadenaConnexio);

                    MostrarFormulesCarregades(
                        idFormulaNova);

                    GestorFormulesTarifesLogger.Informacio(
                        "S'ha duplicat una fórmula.",
                        CrearCampsContextLog(
                            new Dictionary<string, object>
                            {
                                {
                                    "FormulaOrigenId",
                                    formulaSeleccionada.Id
                                },
                                {
                                    "FormulaOrigenCodi",
                                    formulaSeleccionada.Codi
                                    ?? string.Empty
                                },
                                {
                                    "FormulaNovaId",
                                    formulaDesar.Id
                                },
                                {
                                    "FormulaNovaCodi",
                                    formulaDesar.Codi
                                    ?? string.Empty
                                },
                                {
                                    "FormulaNovaNom",
                                    formulaDesar.Nom
                                    ?? string.Empty
                                },
                                {
                                    "TotalFormules",
                                    _formules.Count
                                }
                            }));

                    lblEstat.Text =
                        "La fórmula \""
                        + formulaDesar.Nom
                        + "\" s'ha duplicat correctament.";

                    MessageBox.Show(
                        this,
                        "La fórmula s'ha duplicat correctament.",
                        "Duplicar fórmula",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);
                }
                catch (OleDbException ex)
                    when (EsErrorCodiDuplicat(ex))
                {
                    GestorFormulesTarifesLogger.Advertencia(
                        "No s'ha pogut duplicar la fórmula perquè el codi ja existeix.",
                        CrearCampsContextLog(
                            new Dictionary<string, object>
                            {
                                {
                                    "FormulaOrigenId",
                                    formulaSeleccionada.Id
                                },
                                {
                                    "FormulaOrigenCodi",
                                    formulaSeleccionada.Codi
                                    ?? string.Empty
                                },
                                {
                                    "FormulaNovaCodi",
                                    formulaDesar.Codi
                                    ?? string.Empty
                                },
                                {
                                    "FormulaNovaNom",
                                    formulaDesar.Nom
                                    ?? string.Empty
                                }
                            }));

                    MessageBox.Show(
                        this,
                        "No s'ha pogut duplicar la fórmula perquè "
                        + "ja existeix una fórmula amb el codi \""
                        + formulaDesar.Codi
                        + "\".\r\n\r\n"
                        + "Informa un codi diferent.",
                        "Codi de fórmula duplicat",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);
                }
                catch (Exception ex)
                {
                    GestorFormulesTarifesLogger.Error(
                        "Error en duplicar la fórmula.",
                        ex,
                        CrearCampsContextLog(
                            new Dictionary<string, object>
                            {
                                {
                                    "FormulaOrigenId",
                                    formulaSeleccionada.Id
                                },
                                {
                                    "FormulaOrigenCodi",
                                    formulaSeleccionada.Codi
                                    ?? string.Empty
                                },
                                {
                                    "FormulaNovaId",
                                    formulaDesar == null
                                        ? 0
                                        : formulaDesar.Id
                                },
                                {
                                    "FormulaNovaCodi",
                                    formulaDesar == null
                                        ? string.Empty
                                        : (
                                            formulaDesar.Codi
                                            ?? string.Empty
                                        )
                                },
                                {
                                    "FormulaNovaNom",
                                    formulaDesar == null
                                        ? string.Empty
                                        : (
                                            formulaDesar.Nom
                                            ?? string.Empty
                                        )
                                }
                            }));

                    MessageBox.Show(
                        this,
                        "No s'ha pogut duplicar la fórmula."
                        + "\r\n\r\n"
                        + ex.Message,
                        "Error en duplicar la fórmula",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error);
                }
            }
        }

        /// <summary>
        /// Elimina lògicament la fórmula seleccionada
        /// després de demanar confirmació.
        ///
        /// El registre continua existint a SQL,
        /// però deixa d'aparèixer al gestor perquè
        /// queda marcat com a eliminat.
        /// </summary>
        private void btnEliminar_Click(
            object sender,
            EventArgs e)
        {
            FormulaTarifa formula =
                ObtenirFormulaSeleccionada();

            if (formula == null)
            {
                MessageBox.Show(
                    this,
                    "Selecciona una fórmula abans d'eliminar-la.",
                    "Eliminar fórmula",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);

                return;
            }

            int indexFilaActual =
                dgvFormules.CurrentRow == null
                    ? 0
                    : dgvFormules.CurrentRow.Index;

            int articlesAssignats;

            try
            {
                articlesAssignats =
                    _repositoriFormules.ComptarArticlesAssignats(
                        _contextA3Erp.CadenaConnexio,
                        formula.Id);
            }
            catch (Exception ex)
            {
                GestorFormulesTarifesLogger.Error(
                    "Error en comprovar els articles assignats a la fórmula.",
                    ex,
                    CrearCampsFormulaLog(
                        formula));

                MessageBox.Show(
                    this,
                    "No s'ha pogut comprovar si la fórmula "
                    + "té articles assignats."
                    + "\r\n\r\n"
                    + ex.Message,
                    "Eliminar fórmula",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);

                return;
            }

            if (articlesAssignats > 0)
            {
                string missatgeBloqueig =
                    articlesAssignats == 1
                        ? "No es pot eliminar la fórmula perquè està assignada a 1 article."
                          + "\r\n"
                          + "Reassigna aquest article a una altra fórmula abans d'eliminar-la."
                        : "No es pot eliminar la fórmula perquè està assignada a "
                          + articlesAssignats
                          + " articles."
                          + "\r\n"
                          + "Reassigna aquests articles a una altra fórmula abans d'eliminar-la.";

                GestorFormulesTarifesLogger.Advertencia(
                    "Eliminació de fórmula bloquejada perquè té articles assignats.",
                    CrearCampsFormulaLog(
                        formula,
                        new Dictionary<string, object>
                        {
                            {
                                "IdFormula",
                                formula.Id
                            },
                            {
                                "CodiFormula",
                                formula.Codi
                                ?? string.Empty
                            },
                            {
                                "NomFormula",
                                formula.Nom
                                ?? string.Empty
                            },
                            {
                                "ArticlesAssignats",
                                articlesAssignats
                            }
                        }));

                lblEstat.Text =
                    "La fórmula \""
                    + formula.Nom
                    + "\" no es pot eliminar perquè té articles assignats.";

                MessageBox.Show(
                    this,
                    missatgeBloqueig,
                    "Eliminar fórmula",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);

                return;
            }

            DialogResult resposta =
                MessageBox.Show(
                    this,
                    "Vols eliminar la fórmula següent?"
                    + "\r\n\r\n"
                    + "Codi: "
                    + formula.Codi
                    + "\r\n"
                    + "Nom: "
                    + formula.Nom
                    + "\r\n\r\n"
                    + "La fórmula deixarà d'aparèixer al gestor, "
                    + "però es conservarà a la base de dades "
                    + "com a registre eliminat.",
                    "Eliminar fórmula",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Warning,
                    MessageBoxDefaultButton.Button2);

            if (resposta != DialogResult.Yes)
            {
                return;
            }

            try
            {
                _repositoriFormules.EliminarFormula(
                    _contextA3Erp.CadenaConnexio,
                    formula.Id);

                /*
                 * Tornem a consultar SQL.
                 * La fórmula eliminada ja no serà retornada
                 * per ObtenirPerGestor().
                 */
                _formules =
                    _repositoriFormules.ObtenirPerGestor(
                        _contextA3Erp.CadenaConnexio);

                MostrarFormulesCarregades();

                /*
                 * Intentem conservar una posició pròxima
                 * a la fórmula que s'acaba d'eliminar.
                 */
                SeleccionarFilaPerIndex(
                    indexFilaActual);

                GestorFormulesTarifesLogger.Informacio(
                    "S'ha eliminat lògicament una fórmula.",
                    CrearCampsFormulaLog(
                        formula,
                        new Dictionary<string, object>
                        {
                            {
                                "TotalFormules",
                                _formules.Count
                            }
                        }));

                lblEstat.Text =
                    "La fórmula \""
                    + formula.Nom
                    + "\" s'ha eliminat correctament.";

                MessageBox.Show(
                    this,
                    "La fórmula s'ha eliminat correctament.",
                    "Eliminar fórmula",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                GestorFormulesTarifesLogger.Error(
                    "Error en eliminar lògicament la fórmula.",
                    ex,
                    CrearCampsFormulaLog(
                        formula));

                MessageBox.Show(
                    this,
                    "No s'ha pogut eliminar la fórmula."
                    + "\r\n\r\n"
                    + ex.Message,
                    "Error en eliminar la fórmula",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Selecciona una fila de la graella
        /// segons la seva posició.
        ///
        /// Si l'índex ja no existeix, selecciona
        /// l'última fila disponible.
        /// </summary>
        private void SeleccionarFilaPerIndex(
            int indexFila)
        {
            if (dgvFormules.Rows.Count == 0)
            {
                ActualitzarEstatAccions();

                return;
            }

            int indexValid =
                Math.Max(
                    0,
                    Math.Min(
                        indexFila,
                        dgvFormules.Rows.Count - 1));

            dgvFormules.ClearSelection();

            DataGridViewRow fila =
                dgvFormules.Rows[indexValid];

            fila.Selected =
                true;

            dgvFormules.CurrentCell =
                fila.Cells[0];

            ActualitzarEstatAccions();
        }

        /// <summary>
        /// Determina si SQL ha rebutjat l'operació
        /// perquè el codi de fórmula ja existeix.
        ///
        /// Els errors 2601 i 2627 corresponen
        /// a índexs o restriccions úniques duplicades.
        /// </summary>
        private static bool EsErrorCodiDuplicat(
            OleDbException excepcio)
        {
            if (excepcio == null)
            {
                return false;
            }

            foreach (
                OleDbError error
                in excepcio.Errors)
            {
                if (error.NativeError == 2601
                    || error.NativeError == 2627)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Calcula l'ordre inicial proposat
        /// per a una fórmula nova.
        ///
        /// Utilitza l'ordre més alt del catàleg
        /// actual i hi suma una unitat.
        /// </summary>
        private int ObtenirSeguentOrdre()
        {
            if (_formules == null
                || _formules.Count == 0)
            {
                return 1;
            }

            int ordreMaxim =
                0;

            foreach (
                FormulaTarifa formula
                in _formules)
            {
                if (formula != null
                    && formula.Ordre > ordreMaxim)
                {
                    ordreMaxim =
                        formula.Ordre;
                }
            }

            return ordreMaxim + 1;
        }
    }
}
