using A3ErpCalculadorTarifes.Infrastructure.Data;
using A3ErpCalculadorTarifes.Infrastructure.Logging;
using A3ErpCalculadorTarifes.Models;
using A3ErpCalculadorTarifes.Services;
using A3ErpGestorFormulesTarifes.Models;
using A3ErpGestorFormulesTarifes.Integracio;
using MAT0943Net.Infrastructure;
using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace A3ErpCalculadorTarifes
{
    /// <summary>
    /// Formulari principal del calculador de tarifes.
    ///
    /// Carrega els articles de l'empresa activa i
    /// només genera una previsualització.
    /// No escriu cap dada a a3ERP.
    /// </summary>
    public partial class FrmCalculadorTarifes : Form
    {
        /*
         * Motor que executa les expressions
         * configurades a la base de dades.
         */
        private readonly MotorCalculTarifesConfigurable
            _motorCalculConfigurable;

        private readonly List<ArticleCalculTarifes>
            _articles;
        /// <summary>
        /// Resultats corresponents a l'última
        /// previsualització generada.
        ///
        /// Aquesta llista serà la font que utilitzarà
        /// posteriorment el procés d'aplicació.
        /// </summary>
        private readonly List<ResultatCalculTarifes>
            _resultatsPrevisualitzacio;

        /// <summary>
        /// Indica que la previsualització actual
        /// continua sent vàlida i no ha estat invalidada
        /// per canvis de selecció, fórmula o paràmetres.
        /// </summary>
        private bool _previsualitzacioValida;

        private readonly ContextCalculadorA3Erp
            _contextA3Erp;

        private readonly RepositoriArticlesCalculTarifes
            _repositoriArticles;
        private readonly RepositoriAplicacioTarifes
        _repositoriAplicacioTarifes;
        /*
         * Servei responsable de recuperar i validar
         * les fórmules actives configurades a SQL.
         */
        private readonly ServeiCatalegFormulesConfigurables
            _serveiCatalegFormulesConfigurables;

        /*
         * Catàleg configurable carregat
         * des de la base de dades activa.
         *
         * S'utilitza per alimentar el selector
         * de fórmules del Calculador.
         */
        private List<FormulaTarifa>
            _formulesConfigurablesActives;

        private bool _inicialitzant;

        private bool _actualitzantSeleccio;

        private bool _carregaArticlesIniciada;

        private int _versioCarregaArticles;

        private bool _normalitzantControlNumericEditable;

        private readonly Dictionary<NumericUpDown, FiltreEnganxatNumericEditable>
            _filtresEnganxatNumericEditable;

        private string _filtreArticles;

        /// <summary>
        /// Indica si el catàleg configurable
        /// ja s'ha carregat correctament.
        /// </summary>
        private bool HiHaFormulesConfigurablesCarregades
        {
            get
            {
                return _formulesConfigurablesActives != null
                    && _formulesConfigurablesActives.Count > 0;
            }
        }

        public FrmCalculadorTarifes()
            : this(
                ContextCalculadorA3Erp
                    .CrearModeDesenvolupament())
        {
        }

        public FrmCalculadorTarifes(
            ContextCalculadorA3Erp context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(
                    nameof(context));
            }

            _contextA3Erp =
                context;

            _serveiCatalegFormulesConfigurables =
                new ServeiCatalegFormulesConfigurables();

            _formulesConfigurablesActives =
                new List<FormulaTarifa>();

            _filtresEnganxatNumericEditable =
                new Dictionary<NumericUpDown, FiltreEnganxatNumericEditable>();

            _filtreArticles =
                string.Empty;

            InitializeComponent();

            IconaAplicacio.Aplicar(
                this);

            /*
             * Evitem que la tipografia depengui del procés
             * que allotja el formulari: executable propi o a3ERP.
             */
            AplicarRenderitzatText(this);

            _motorCalculConfigurable =
                new MotorCalculTarifesConfigurable();

            _articles =
                new List<ArticleCalculTarifes>();

            _resultatsPrevisualitzacio =
                new List<ResultatCalculTarifes>();

            _previsualitzacioValida =
                false;

            _repositoriArticles =
                new RepositoriArticlesCalculTarifes();

            _repositoriAplicacioTarifes =
                new RepositoriAplicacioTarifes();

            InicialitzarLogCalculador();

            _inicialitzant =
                true;

            ConfigurarGraelles();
            PrepararSelectorGeneralArticles();
            ConnectarEsdeveniments();

            /*
             * Carreguem primer les fórmules configurables
             * de la base de dades.
             *
             * Si la càrrega o la validació falla,
             * el Calculador no continua silenciosament
             * sense cap configuració vàlida.
             */
            CarregarFormulesConfigurablesActives();

            CarregarFormulesAlSelector();

            _inicialitzant =
                false;

            AplicarFormulaSeleccionada();
            ActualitzarComptadorArticles();
            InvalidarPrevisualitzacio();

            btnAplicar.Enabled =
                false;

            AplicarPasProces(1);
            MostrarContextA3Erp();

            CalculadorTarifesLogger.Informacio(
                "S'inicia el calculador de tarifes.",
                CrearCampsContextLog(
                    new Dictionary<string, object>
                    {
                        {
                            "LogActiu",
                            CalculadorTarifesLogger.LogActiu
                        }
                    }));
        }

        /// <summary>
        /// Connecta els esdeveniments utilitzats
        /// per la pantalla.
        /// </summary>
        private void ConnectarEsdeveniments()
        {
            cmbPlantilla.SelectedIndexChanged +=
                cmbPlantilla_SelectedIndexChanged;

            btnRestablirValors.Click +=
                btnRestablirValors_Click;

            btnGestionarFormules.Click +=
                btnGestionarFormules_Click;

            btnPrevisualitzar.Click +=
                btnPrevisualitzar_Click;

            btnAplicar.Click +=
                btnAplicar_Click;

            btnTancar.Click +=
                btnTancar_Click;

            controlCercaArticles.CercaSollicitada +=
                controlCercaArticles_CercaSollicitada;

            controlCercaArticles.NetejaSollicitada +=
                controlCercaArticles_NetejaSollicitada;

            chkSeleccionarTots.CheckStateChanged +=
                chkSeleccionarTots_CheckStateChanged;

            dgvArticles.CurrentCellDirtyStateChanged +=
                dgvArticles_CurrentCellDirtyStateChanged;

            dgvArticles.CellValueChanged +=
                dgvArticles_CellValueChanged;

            numTarifa1.ValueChanged +=
                parametre_ValueChanged;

            numTarifa2.ValueChanged +=
                parametre_ValueChanged;

            numTarifa3.ValueChanged +=
                parametre_ValueChanged;

            numTarifa4.ValueChanged +=
                parametre_ValueChanged;

            numDescompte1.ValueChanged +=
                parametre_ValueChanged;

            numDescompte2.ValueChanged +=
                parametre_ValueChanged;

            numDescompte3.ValueChanged +=
                parametre_ValueChanged;

            numDescompte4.ValueChanged +=
                parametre_ValueChanged;

            foreach (
                NumericUpDown control
                in ObtenirControlsNumericsEditables())
            {
                control.ThousandsSeparator =
                    false;

                control.KeyPress +=
                    numericEditable_KeyPress;

                control.TextChanged +=
                    numericEditable_TextChanged;

                control.Validating +=
                    numericEditable_Validating;

                ConnectarFiltreEnganxatNumericEditable(
                    control);
            }

            Shown +=
                FrmCalculadorTarifes_Shown;

            FormClosed +=
                FrmCalculadorTarifes_FormClosed;
        }

        private void InicialitzarLogCalculador()
        {
            CalculadorTarifesLogger.InicialitzarLogReserva();

            if (!_contextA3Erp.ConnexioDisponible)
            {
                return;
            }

            try
            {
                RepositoriConfiguracioLogCalculadorTarifes
                    repositoriConfiguracio =
                        new RepositoriConfiguracioLogCalculadorTarifes();

                ConfiguracioLogCalculadorTarifes configuracio =
                    repositoriConfiguracio.Carregar(
                        _contextA3Erp.CadenaConnexio);

                CalculadorTarifesLogger.AplicarConfiguracioLog(
                    configuracio);
            }
            catch (Exception ex)
            {
                CalculadorTarifesLogger.Error(
                    "No s'ha pogut llegir la configuració del log del calculador. Es continua amb el log local de reserva.",
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

        /// <summary>
        /// Configura les dues graelles del formulari.
        /// </summary>
        private void ConfigurarGraelles()
        {
            ConfigurarGraellaBase(
                dgvArticles);

            ConfigurarGraellaBase(
                dgvPrevisualitzacio);

            CrearColumnesGraellaArticles();
            CrearColumnesGraellaPrevisualitzacio();
        }

        /// <summary>
        /// Aplica l'estil comú a una graella.
        /// </summary>
        private static void ConfigurarGraellaBase(
            DataGridView graella)
        {
            graella.AllowUserToAddRows =
                false;

            graella.AllowUserToDeleteRows =
                false;

            graella.AllowUserToResizeRows =
                false;

            graella.AutoGenerateColumns =
                false;

            graella.BackgroundColor =
                Color.White;

            graella.BorderStyle =
                BorderStyle.None;

            graella.CellBorderStyle =
                DataGridViewCellBorderStyle.SingleHorizontal;

            graella.ColumnHeadersBorderStyle =
                DataGridViewHeaderBorderStyle.None;

            graella.ColumnHeadersDefaultCellStyle.BackColor =
                Color.FromArgb(
                    241,
                    245,
                    249);

            graella.ColumnHeadersDefaultCellStyle.ForeColor =
                Color.FromArgb(
                    51,
                    65,
                    85);

            graella.ColumnHeadersDefaultCellStyle.Font =
                new Font(
                    "Segoe UI Semibold",
                    9F);

            graella.ColumnHeadersDefaultCellStyle.SelectionBackColor =
                Color.FromArgb(
                    241,
                    245,
                    249);

            graella.ColumnHeadersHeight =
                38;

            graella.DefaultCellStyle.BackColor =
                Color.White;

            graella.DefaultCellStyle.ForeColor =
                Color.FromArgb(
                    30,
                    41,
                    59);

            graella.DefaultCellStyle.SelectionBackColor =
                Color.FromArgb(
                    224,
                    242,
                    254);

            graella.DefaultCellStyle.SelectionForeColor =
                Color.FromArgb(
                    12,
                    74,
                    110);

            graella.DefaultCellStyle.Padding =
                new Padding(4);

            graella.EnableHeadersVisualStyles =
                false;

            graella.GridColor =
                Color.FromArgb(
                    226,
                    232,
                    240);

            graella.MultiSelect =
                false;

            graella.RowHeadersVisible =
                false;

            graella.RowTemplate.Height =
                34;

            graella.SelectionMode =
                DataGridViewSelectionMode.FullRowSelect;
        }

        /// <summary>
        /// Crea les columnes de selecció d'articles.
        /// </summary>
        private void CrearColumnesGraellaArticles()
        {
            dgvArticles.Columns.Clear();

            DataGridViewCheckBoxColumn columnaSeleccionat =
                new DataGridViewCheckBoxColumn
                {
                    Name =
                        "colSeleccionat",

                    HeaderText =
                        string.Empty,

                    Width =
                        42,

                    ReadOnly =
                        false,

                    Frozen =
                        true,

                    SortMode =
                        DataGridViewColumnSortMode.NotSortable
                };

            dgvArticles.Columns.Add(
                columnaSeleccionat);

            dgvArticles.Columns.Add(
                CrearColumnaText(
                    "colCodiArticle",
                    "Codi",
                    90));

            DataGridViewTextBoxColumn columnaDescripcio =
                CrearColumnaText(
                    "colDescripcio",
                    "Descripció",
                    240);

            columnaDescripcio.AutoSizeMode =
                DataGridViewAutoSizeColumnMode.Fill;

            dgvArticles.Columns.Add(
                columnaDescripcio);

            dgvArticles.Columns.Add(
                CrearColumnaText(
                    "colFamilia",
                    "Família",
                    90));

            dgvArticles.Columns.Add(
                CrearColumnaDecimal(
                    "colPreuCompra",
                    "PRCCOMPRA",
                    105));

            dgvArticles.Columns.Add(
                CrearColumnaDecimal(
                    "colPreuCost",
                    "PRCCOSTE",
                    105));

            dgvArticles.Columns.Add(
                CrearColumnaDecimal(
                    "colTransport",
                    "Transport",
                    100));

            foreach (
                DataGridViewColumn columna
                in dgvArticles.Columns)
            {
                if (columna.Name !=
                    "colSeleccionat")
                {
                    columna.ReadOnly =
                        true;
                }
            }
        }

        /// <summary>
        /// Crea les columnes de la previsualització.
        /// </summary>
        private void CrearColumnesGraellaPrevisualitzacio()
        {
            dgvPrevisualitzacio.Columns.Clear();

            dgvPrevisualitzacio.ReadOnly =
                true;

            dgvPrevisualitzacio.Columns.Add(
                CrearColumnaText(
                    "colPrevCodi",
                    "Codi",
                    90));

            DataGridViewTextBoxColumn columnaDescripcio =
                CrearColumnaText(
                    "colPrevDescripcio",
                    "Descripció",
                    230);

            columnaDescripcio.AutoSizeMode =
                DataGridViewAutoSizeColumnMode.Fill;

            dgvPrevisualitzacio.Columns.Add(
                columnaDescripcio);

            for (int tarifa = 1;
                 tarifa <= 6;
                 tarifa++)
            {
                dgvPrevisualitzacio.Columns.Add(
                    CrearColumnaDecimal(
                        "colTarifa" + tarifa,
                        "T" + tarifa,
                        78));
            }

            for (int grup = 1;
                 grup <= 4;
                 grup++)
            {
                dgvPrevisualitzacio.Columns.Add(
                    CrearColumnaDecimal(
                        "colDescompte" + grup,
                        "D" + grup + " %",
                        76));
            }

            dgvPrevisualitzacio.Columns.Add(
                CrearColumnaText(
                    "colResultat",
                    "Resultat",
                    190));
        }

        private static DataGridViewTextBoxColumn
            CrearColumnaText(
                string nom,
                string titol,
                int amplada)
        {
            return new DataGridViewTextBoxColumn
            {
                Name =
                    nom,

                HeaderText =
                    titol,

                Width =
                    amplada,

                SortMode =
                    DataGridViewColumnSortMode.NotSortable
            };
        }

        private static DataGridViewTextBoxColumn
            CrearColumnaDecimal(
                string nom,
                string titol,
                int amplada)
        {
            DataGridViewTextBoxColumn columna =
                CrearColumnaText(
                    nom,
                    titol,
                    amplada);

            columna.DefaultCellStyle.Alignment =
                DataGridViewContentAlignment.MiddleRight;

            columna.DefaultCellStyle.Format =
                "N2";

            return columna;
        }

        /// <summary>
        /// Carrega al selector les fórmules actives
        /// recuperades de la base de dades.
        ///
        /// Quan és possible conserva seleccionada
        /// la fórmula indicada.
        /// </summary>
        private void CarregarFormulesAlSelector(
            int? idFormulaPreferida = null)
        {
            if (!HiHaFormulesConfigurablesCarregades)
            {
                throw new InvalidOperationException(
                    "No hi ha fórmules configurables "
                    + "disponibles per carregar al selector.");
            }

            List<FormulaTarifa> formulesSelector =
                _formulesConfigurablesActives
                    .ToList();

            FormulaTarifa formulaASeleccionar =
                idFormulaPreferida.HasValue
                    ? formulesSelector.FirstOrDefault(
                        formula =>
                            formula.Id ==
                            idFormulaPreferida.Value)
                    : null;

            if (formulaASeleccionar == null)
            {
                formulaASeleccionar =
                    formulesSelector.First();
            }

            cmbPlantilla.DataSource =
                null;

            cmbPlantilla.DisplayMember =
                nameof(
                    FormulaTarifa.TextSelector);

            cmbPlantilla.ValueMember =
                nameof(
                    FormulaTarifa.Id);

            cmbPlantilla.DataSource =
                formulesSelector;

            cmbPlantilla.SelectedValue =
                formulaASeleccionar.Id;
        }

        private async void FrmCalculadorTarifes_Shown(
            object sender,
            EventArgs e)
        {
            if (_carregaArticlesIniciada ||
                !_contextA3Erp.ConnexioDisponible)
            {
                return;
            }

            _carregaArticlesIniciada =
                true;

            await CarregarArticlesFormulaSeleccionadaAsync();
        }

        /// <summary>
        /// Consulta els articles fora del fil de la interfície
        /// i els mostra a la graella quan finalitza la lectura.
        /// </summary>
        private async Task CarregarArticlesFormulaSeleccionadaAsync()
        {
            FormulaTarifa formula =
                cmbPlantilla.SelectedItem
                as FormulaTarifa;

            if (formula == null)
            {
                MostrarArticles(
                    new List<ArticleCalculTarifes>());

                lblEstatGeneral.Text =
                    "No s'ha seleccionat cap fórmula de càlcul.";

                return;
            }

            int idFormula =
                formula.Id;

            int versioCarrega =
                ++_versioCarregaArticles;

            PrepararInterficiePerCarrega();

            try
            {
                List<ArticleCalculTarifes> articles =
                    await Task.Run(
                        delegate
                        {
                            return _repositoriArticles
                                .ObtenirPerFormula(
                                    _contextA3Erp
                                        .CadenaConnexio,
                                    idFormula);
                        });

                if (IsDisposed ||
                    Disposing ||
                    !EsCarregaArticlesVigent(
                        versioCarrega,
                        idFormula))
                {
                    return;
                }

                MostrarArticles(
                    articles);

                lblEstatGeneral.Text =
                    articles.Count == 0
                        ? "La fórmula seleccionada no té articles assignats."
                        : articles.Count
                          + " articles carregats per a la fórmula seleccionada.";

                CalculadorTarifesLogger.Informacio(
                    articles.Count == 0
                        ? "La fórmula seleccionada no té articles assignats."
                        : "Càrrega d'articles per fórmula correcta.",
                    CrearCampsContextLog(
                        new Dictionary<string, object>
                        {
                            {
                                "IdFormula",
                                formula.Id
                            },
                            {
                                "CodiFormula",
                                formula.Codi
                            },
                            {
                                "NomFormula",
                                formula.Nom
                            },
                            {
                                "ArticlesCarregats",
                                articles.Count
                            }
                        }));
            }
            catch (Exception ex)
            {
                if (IsDisposed ||
                    Disposing ||
                    !EsCarregaArticlesVigent(
                        versioCarrega,
                        idFormula))
                {
                    return;
                }

                MostrarArticles(
                    new List<ArticleCalculTarifes>());

                lblEstatGeneral.Text =
                    "No s'han pogut carregar els articles "
                    + "de la fórmula seleccionada. Comprova la connexió "
                    + "i torna-ho a provar.";

                CalculadorTarifesLogger.Error(
                    "Error en la càrrega d'articles per fórmula.",
                    ex,
                    CrearCampsContextLog(
                        new Dictionary<string, object>
                        {
                            {
                                "IdFormula",
                                formula.Id
                            },
                            {
                                "CodiFormula",
                                formula.Codi
                            },
                            {
                                "NomFormula",
                                formula.Nom
                            }
                        }));
            }
            finally
            {
                if (!IsDisposed &&
                    !Disposing &&
                    EsCarregaArticlesVigent(
                        versioCarrega,
                        idFormula))
                {
                    dgvArticles.Enabled =
                        true;

                    EstablirCursorEspera(
                        false);
                }
            }
        }

        private bool EsCarregaArticlesVigent(
            int versioCarrega,
            int idFormula)
        {
            FormulaTarifa formulaActual =
                cmbPlantilla.SelectedItem
                as FormulaTarifa;

            return versioCarrega == _versioCarregaArticles
                   &&
                   formulaActual != null
                   &&
                   formulaActual.Id == idFormula;
        }

        private void PrepararInterficiePerCarrega()
        {
            MostrarArticles(
                new List<ArticleCalculTarifes>());

            dgvArticles.Enabled =
                false;

            btnPrevisualitzar.Enabled =
                false;

            lblEstatGeneral.Text =
                "Carregant els articles de la fórmula seleccionada...";

            EstablirCursorEspera(
                true);
        }

        /// <summary>
        /// Activa o restaura de manera explícita
        /// el cursor d'espera del formulari i les graelles.
        /// </summary>
        private void EstablirCursorEspera(
            bool actiu)
        {
            Cursor cursor =
                actiu
                    ? Cursors.WaitCursor
                    : Cursors.Default;

            UseWaitCursor =
                actiu;

            Cursor =
                cursor;

            dgvArticles.UseWaitCursor =
                actiu;

            dgvArticles.Cursor =
                cursor;

            dgvPrevisualitzacio.UseWaitCursor =
                actiu;

            dgvPrevisualitzacio.Cursor =
                cursor;

            if (!actiu)
            {
                Cursor.Current =
                    Cursors.Default;

                dgvArticles.Invalidate();
                dgvPrevisualitzacio.Invalidate();
            }
        }

        private void ReiniciarProgresAplicacioTarifes()
        {
            prgAplicacioTarifes.Style =
                ProgressBarStyle.Continuous;

            prgAplicacioTarifes.Minimum =
                0;

            prgAplicacioTarifes.Maximum =
                100;

            prgAplicacioTarifes.Value =
                0;
        }

        private void CompletarProgresAplicacioTarifes()
        {
            prgAplicacioTarifes.Value =
                prgAplicacioTarifes.Maximum;
        }

        private void ActualitzarProgresAplicacioTarifes(
            ProgresAplicacioTarifes progres,
            string textBase)
        {
            if (progres == null)
            {
                return;
            }

            int percentatge =
                Math.Max(
                    prgAplicacioTarifes.Minimum,
                    Math.Min(
                        prgAplicacioTarifes.Maximum,
                        progres.Percentatge));

            prgAplicacioTarifes.Value =
                percentatge;

            lblEstatGeneral.Text =
                textBase
                + " "
                + progres.Processats.ToString(
                    "N0",
                    CultureInfo.CurrentCulture)
                + " de "
                + progres.Total.ToString(
                    "N0",
                    CultureInfo.CurrentCulture)
                + " articles.";
        }

        /// <summary>
        /// Substitueix el contingut de la graella.
        /// Tots els articles s'inicialitzen desmarcats.
        /// </summary>
        private void MostrarArticles(
            IEnumerable<ArticleCalculTarifes> articles)
        {
            _articles.Clear();

            if (articles != null)
            {
                _articles.AddRange(
                    articles);
            }

            _filtreArticles =
                string.Empty;

            if (controlCercaArticles != null)
            {
                controlCercaArticles.TextCerca =
                    string.Empty;
            }

            _actualitzantSeleccio =
                true;

            try
            {
                foreach (
                    ArticleCalculTarifes article
                    in _articles)
                {
                    article.Seleccionat =
                        false;
                }

                MostrarArticlesGraella(
                    ObtenirArticlesVisibles());
            }
            finally
            {
                _actualitzantSeleccio =
                    false;
            }

            ActualitzarComptadorArticles();
            InvalidarPrevisualitzacio();
            AplicarPasProces(1);
        }

        private IEnumerable<ArticleCalculTarifes> ObtenirArticlesVisibles()
        {
            if (!HiHaFiltreArticlesActiu())
            {
                return _articles;
            }

            string filtre =
                _filtreArticles.Trim();

            return _articles.Where(
                article =>
                    ArticleCompleixFiltre(
                        article,
                        filtre));
        }

        private static bool ArticleCompleixFiltre(
            ArticleCalculTarifes article,
            string filtre)
        {
            if (article == null)
            {
                return false;
            }

            if (string.IsNullOrWhiteSpace(
                filtre))
            {
                return true;
            }

            return ConteTextCerca(
                article.CodiArticleBaseDades,
                filtre)
                ||
                ConteTextCerca(
                    article.CodiArticle,
                    filtre)
                ||
                ConteTextCerca(
                    article.Descripcio,
                    filtre);
        }

        private static bool ConteTextCerca(
            string valor,
            string filtre)
        {
            if (string.IsNullOrEmpty(
                valor))
            {
                return false;
            }

            return valor.IndexOf(
                filtre,
                StringComparison.CurrentCultureIgnoreCase) >= 0;
        }

        private void MostrarArticlesGraella(
            IEnumerable<ArticleCalculTarifes> articles)
        {
            dgvArticles.Rows.Clear();
            dgvArticles.SuspendLayout();

            try
            {
                if (articles == null)
                {
                    return;
                }

                foreach (
                    ArticleCalculTarifes article
                    in articles)
                {
                    AfegirArticleGraella(
                        article);
                }
            }
            finally
            {
                dgvArticles.ResumeLayout();
            }
        }

        private void AfegirArticleGraella(
            ArticleCalculTarifes article)
        {
            if (article == null)
            {
                return;
            }

            int indexFila =
                dgvArticles.Rows.Add(
                    article.Seleccionat,
                    article.CodiArticle,
                    article.Descripcio,
                    article.Familia,
                    article.PreuCompra,
                    article.PreuCost,
                    article.PreuTransport);

            dgvArticles
                .Rows[indexFila]
                .Tag =
                    article;
        }

        private void MostrarContextA3Erp()
        {
            if (!_contextA3Erp.ConnexioDisponible)
            {
                lblMode.Text =
                    "Sense connexió a a3ERP";

                lblEstatGeneral.Text =
                    "Obre el calculador des d'a3ERP per carregar "
                    + "els articles de l'empresa activa.";

                btnPrevisualitzar.Enabled =
                    false;

                return;
            }

            if (!string.IsNullOrWhiteSpace(
                _contextA3Erp.EmpresaActiva))
            {
                lblMode.Text =
                    _contextA3Erp
                        .EmpresaActiva
                        .Trim();
            }
            else if (!string.IsNullOrWhiteSpace(
                _contextA3Erp.BaseDadesEmpresa))
            {
                lblMode.Text =
                    _contextA3Erp
                        .BaseDadesEmpresa
                        .Trim();
            }
            else
            {
                lblMode.Text =
                    "Empresa activa";
            }

            lblEstatGeneral.Text =
                "Pendent de carregar els articles "
                + "de l'empresa activa.";

            btnPrevisualitzar.Enabled =
                false;
        }

        private async void cmbPlantilla_SelectedIndexChanged(
            object sender,
            EventArgs e)
                {
                    if (_inicialitzant)
                    {
                        return;
                    }

                    AplicarFormulaSeleccionada();
                    InvalidarPrevisualitzacio();
                    AplicarPasProces(2);

                    if (_contextA3Erp.ConnexioDisponible)
                    {
                        _carregaArticlesIniciada =
                            true;

                        await CarregarArticlesFormulaSeleccionadaAsync();
                    }
                }

        /// <summary>
        /// Carrega a la pantalla els valors inicials
        /// de la fórmula configurable seleccionada.
        /// </summary>
        private void AplicarFormulaSeleccionada()
        {
            FormulaTarifa formula =
                cmbPlantilla.SelectedItem
                as FormulaTarifa;

            if (formula == null)
            {
                return;
            }

            _inicialitzant =
                true;

            try
            {
                numTarifa1.Value =
                    formula.ValorInicialP1;

                numTarifa2.Value =
                    formula.ValorInicialP2;

                numTarifa3.Value =
                    formula.ValorInicialP3;

                numTarifa4.Value =
                    formula.ValorInicialP4;

                numDescompte1.Value =
                    formula.DescompteInicialGrup1;

                numDescompte2.Value =
                    formula.DescompteInicialGrup2;

                numDescompte3.Value =
                    formula.DescompteInicialGrup3;

                numDescompte4.Value =
                    formula.DescompteInicialGrup4;
            }
            finally
            {
                _inicialitzant =
                    false;
            }

            lblDescripcioPlantilla.Text =
                formula.Descripcio;

            lblTipusValors.Text =
                formula.UtilitzaValorsTarifes
                    ? "Tarifes 1–4 · "
                      + formula.TipusValors
                    : "Tarifes 1–4 · Calculades directament per la fórmula";

            tblValorsTarifes.Enabled =
                formula.UtilitzaValorsTarifes;

            lblTitolDescomptes.Text =
                formula.GeneraDescomptes
                    ? "Descomptes per grup de client"
                    : "Descomptes · Aquesta fórmula no en genera";

            tblDescomptes.Enabled =
                formula.GeneraDescomptes;

            ConfigurarDecimalsSegonsFormula(
                formula);
        }

        /// <summary>
        /// Mostra quatre decimals quan la fórmula
        /// utilitza coeficients i dos decimals
        /// per a la resta de valors.
        /// </summary>
        private void ConfigurarDecimalsSegonsFormula(
            FormulaTarifa formula)
        {
            if (formula == null)
            {
                return;
            }

            bool utilitzaCoeficients =
                formula.UtilitzaValorsTarifes
                &&
                !string.IsNullOrWhiteSpace(
                    formula.TipusValors)
                &&
                formula.TipusValors.IndexOf(
                    "coeficient",
                    StringComparison.OrdinalIgnoreCase)
                >= 0;

            int decimals =
                utilitzaCoeficients
                    ? 4
                    : 2;

            numTarifa1.DecimalPlaces =
                decimals;

            numTarifa2.DecimalPlaces =
                decimals;

            numTarifa3.DecimalPlaces =
                decimals;

            numTarifa4.DecimalPlaces =
                decimals;
        }

        private void btnRestablirValors_Click(
            object sender,
            EventArgs e)
                {
                    AplicarFormulaSeleccionada();
                    InvalidarPrevisualitzacio();

                    lblEstatGeneral.Text =
                        "S'han restablert els valors inicials de la fórmula.";
                }

        private void controlCercaArticles_CercaSollicitada(
            object sender,
            EventArgs e)
        {
            AplicarFiltreArticles();
        }

        private void controlCercaArticles_NetejaSollicitada(
            object sender,
            EventArgs e)
        {
            NetejarFiltreArticles();
        }

        private void AplicarFiltreArticles()
        {
            SincronitzarSeleccionsArticlesVisibles();

            _filtreArticles =
                (controlCercaArticles.TextCerca ?? string.Empty)
                    .Trim();

            _actualitzantSeleccio =
                true;

            try
            {
                MostrarArticlesGraella(
                    ObtenirArticlesVisibles());
            }
            finally
            {
                _actualitzantSeleccio =
                    false;
            }

            ActualitzarComptadorArticles();
        }

        private void NetejarFiltreArticles()
        {
            SincronitzarSeleccionsArticlesVisibles();

            if (controlCercaArticles.TextCerca.Length > 0)
            {
                controlCercaArticles.TextCerca =
                    string.Empty;
            }

            _filtreArticles =
                string.Empty;

            _actualitzantSeleccio =
                true;

            try
            {
                MostrarArticlesGraella(
                    ObtenirArticlesVisibles());
            }
            finally
            {
                _actualitzantSeleccio =
                    false;
            }

            ActualitzarComptadorArticles();
            controlCercaArticles.EnfocarCerca();
        }

        /// <summary>
        /// Obre el gestor de fórmules amb el mateix
        /// context d'empresa i connexió que utilitza
        /// el Calculador de tarifes.
        ///
        /// El gestor no inicia una nova connexió
        /// ni una nova instància d'a3ERP.
        /// </summary>
        private async void btnGestionarFormules_Click(
            object sender,
            EventArgs e)
        {
            try
            {
                if (!_contextA3Erp.ConnexioDisponible)
                {
                    MessageBox.Show(
                        this,
                        "No hi ha una connexió d'empresa disponible.\r\n\r\n"
                        + "El gestor de fórmules s'ha d'obrir "
                        + "des del context d'una empresa d'a3ERP.",
                        "Gestor de fórmules",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);

                    return;
                }

                PuntEntradaGestorFormules.Obrir(
                 empresaActiva:
                     _contextA3Erp.EmpresaActiva,

                 baseDadesEmpresa:
                     _contextA3Erp.BaseDadesEmpresa,

                 cadenaConnexio:
                     _contextA3Erp.CadenaConnexio,

                 utilitzaConnexioAlternativa:
                     _contextA3Erp.UtilitzaConnexioAlternativa);

                /*
                 * PuntEntradaGestorFormules.Obrir és modal.
                 * Aquesta línia s'executa quan el gestor
                 * ja s'ha tancat.
                 */
                await RecarregarFormulesDespresDelGestorAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    this,
                    "No s'ha pogut obrir el gestor "
                    + "de fórmules.\r\n\r\n"
                    + ex.Message,
                    "Error en obrir el gestor",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Calcula les tarifes dels articles seleccionats,
        /// conserva els resultats internament i els mostra
        /// sense guardar-los a a3ERP.
        /// </summary>
        private void btnPrevisualitzar_Click(
            object sender,
            EventArgs e)
        {
            if (!ValidarControlsNumericsEditables())
            {
                InvalidarPrevisualitzacio();

                return;
            }

            List<ArticleCalculTarifes> articlesSeleccionats =
                ObtenirArticlesSeleccionats();

            if (articlesSeleccionats.Count == 0)
            {
                InvalidarPrevisualitzacio();

                CalculadorTarifesLogger.Advertencia(
                    "Previsualització cancel·lada: cap article seleccionat.",
                    CrearCampsContextLog());

                MessageBox.Show(
                    this,
                    "Selecciona almenys un article abans de calcular.",
                    "Calculador de tarifes",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);

                AplicarPasProces(1);

                return;
            }

            FormulaTarifa formula =
                cmbPlantilla.SelectedItem
                as FormulaTarifa;

            if (formula == null)
            {
                InvalidarPrevisualitzacio();

                CalculadorTarifesLogger.Advertencia(
                    "Previsualització cancel·lada: no hi ha fórmula seleccionada.",
                    CrearCampsContextLog());

                MessageBox.Show(
                    this,
                    "No s'ha seleccionat cap fórmula de càlcul.",
                    "Calculador de tarifes",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);

                return;
            }

            ParametresCalculTarifes parametres =
             ObtenirParametresPantalla(
                 formula);

            CalculadorTarifesLogger.Informacio(
                "S'inicia la previsualització.",
                CrearCampsContextLog(
                    new Dictionary<string, object>
                    {
                        {
                            "FormulaId",
                            formula.Id
                        },
                        {
                            "FormulaCodi",
                            formula.Codi
                        },
                        {
                            "FormulaNom",
                            formula.Nom
                        },
                        {
                            "ArticlesSeleccionats",
                            articlesSeleccionats.Count
                        },
                        {
                            "GeneraDescomptes",
                            formula.GeneraDescomptes
                        }
                    }));

            /*
             * Comença una previsualització nova.
             *
             * No reutilitzem mai resultats anteriors.
             */
            _resultatsPrevisualitzacio.Clear();

            _previsualitzacioValida =
                false;

            dgvPrevisualitzacio.Rows.Clear();

            int correctes =
                0;

            int errors =
                0;

            foreach (
                ArticleCalculTarifes article
                in articlesSeleccionats)
            {
                ResultatCalculTarifes resultat =
                 _motorCalculConfigurable.Calcular(
                     article,
                     formula,
                     parametres);

                /*
                 * Conservem l'objecte calculat complet,
                 * inclòs CodiArticleBaseDades.
                 */
                _resultatsPrevisualitzacio.Add(
                    resultat);

                AfegirResultatGraella(
                    resultat);

                if (resultat.Correcte)
                {
                    correctes++;
                }
                else
                {
                    errors++;
                }
            }

            lblResumSeleccionats.Text =
                "Seleccionats: "
                + articlesSeleccionats.Count;

            lblResumCorrectes.Text =
                "Correctes: "
                + correctes;

            lblResumErrors.Text =
                "Errors: "
                + errors;

            /*
             * Una previsualització només és aplicable quan:
             * - conté almenys un resultat;
             * - tots els articles s'han calculat correctament;
             * - tots conserven el CODART literal d'a3ERP;
             * - el nombre de resultats coincideix amb
             *   el nombre d'articles seleccionats.
             */
            _previsualitzacioValida =
                correctes > 0
                &&
                errors == 0
                &&
                _resultatsPrevisualitzacio.Count ==
                    articlesSeleccionats.Count
                &&
                _resultatsPrevisualitzacio.All(
                    resultat =>
                        resultat != null
                        &&
                        resultat.Correcte
                        &&
                        !string.IsNullOrWhiteSpace(
                            resultat.CodiArticleBaseDades));

            btnAplicar.Enabled =
                _previsualitzacioValida
                &&
                _contextA3Erp.ConnexioDisponible;

            lblEstatGeneral.Text =
                _previsualitzacioValida
                    ? "Previsualització correcta. Revisa els imports abans d'aplicar-los."
                    : "La previsualització conté errors. Revisa els paràmetres.";

            Dictionary<string, object> campsResumPrevisualitzacio =
                new Dictionary<string, object>
                {
                    {
                        "FormulaId",
                        formula.Id
                    },
                    {
                        "FormulaCodi",
                        formula.Codi
                    },
                    {
                        "FormulaNom",
                        formula.Nom
                    },
                    {
                        "ArticlesSeleccionats",
                        articlesSeleccionats.Count
                    },
                    {
                        "GeneraDescomptes",
                        formula.GeneraDescomptes
                    },
                    {
                        "Correctes",
                        correctes
                    },
                    {
                        "Errors",
                        errors
                    }
                };

            if (errors > 0)
            {
                CalculadorTarifesLogger.Advertencia(
                    "Previsualització amb errors.",
                    CrearCampsContextLog(
                        campsResumPrevisualitzacio));

                foreach (
                    ResultatCalculTarifes resultatIncorrecte
                    in _resultatsPrevisualitzacio.Where(
                        resultat =>
                            resultat != null
                            &&
                            !resultat.Correcte))
                {
                    CalculadorTarifesLogger.Advertencia(
                        "Resultat de previsualització incorrecte.",
                        new Dictionary<string, object>
                        {
                            {
                                "Article",
                                resultatIncorrecte.CodiArticle
                            },
                            {
                                "Motiu",
                                resultatIncorrecte.Missatge
                            }
                        });
                }
            }
            else
            {
                CalculadorTarifesLogger.Informacio(
                    _previsualitzacioValida
                        ? "Previsualització correcta."
                        : "Previsualització amb errors.",
                    CrearCampsContextLog(
                        campsResumPrevisualitzacio));
            }

            AplicarPasProces(3);
        }

        /// <summary>
        /// Recull els paràmetres actuals de pantalla
        /// per executar la fórmula seleccionada.
        /// </summary>
        private ParametresCalculTarifes
            ObtenirParametresPantalla(
                FormulaTarifa formula)
        {
            if (formula == null)
            {
                throw new ArgumentNullException(
                    nameof(formula));
            }

            return new ParametresCalculTarifes
            {
                ValorTarifa1 =
                    numTarifa1.Value,

                ValorTarifa2 =
                    numTarifa2.Value,

                ValorTarifa3 =
                    numTarifa3.Value,

                ValorTarifa4 =
                    numTarifa4.Value,

                GeneraDescomptes =
                    formula.GeneraDescomptes,

                DescompteGrup1 =
                    numDescompte1.Value,

                DescompteGrup2 =
                    numDescompte2.Value,

                DescompteGrup3 =
                    numDescompte3.Value,

                DescompteGrup4 =
                    numDescompte4.Value
            };
        }

        /// <summary>
        /// Afegeix una fila calculada
        /// a la graella de previsualització.
        /// </summary>
        private void AfegirResultatGraella(
            ResultatCalculTarifes resultat)
        {
            object descompte1 =
                resultat.GeneraDescomptes
                    ? (object)resultat.DescompteGrup1
                    : null;

            object descompte2 =
                resultat.GeneraDescomptes
                    ? (object)resultat.DescompteGrup2
                    : null;

            object descompte3 =
                resultat.GeneraDescomptes
                    ? (object)resultat.DescompteGrup3
                    : null;

            object descompte4 =
                resultat.GeneraDescomptes
                    ? (object)resultat.DescompteGrup4
                    : null;

            int indexFila =
                dgvPrevisualitzacio.Rows.Add(
                    resultat.CodiArticle,
                    resultat.Descripcio,

                    resultat.Correcte
                        ? (object)resultat.Tarifa1
                        : null,

                    resultat.Correcte
                        ? (object)resultat.Tarifa2
                        : null,

                    resultat.Correcte
                        ? (object)resultat.Tarifa3
                        : null,

                    resultat.Correcte
                        ? (object)resultat.Tarifa4
                        : null,

                    resultat.Correcte
                        ? (object)resultat.Tarifa5
                        : null,

                    resultat.Correcte
                        ? (object)resultat.Tarifa6
                        : null,

                    descompte1,
                    descompte2,
                    descompte3,
                    descompte4,
                    resultat.Missatge);

            /*
            * La fila conserva una referència al mateix
            * resultat que tenim a la llista interna.
            *
            * La futura persistència no llegirà els imports
            * de les cel·les de la graella.
            */
            dgvPrevisualitzacio
                .Rows[indexFila]
                .Tag =
                    resultat;

            if (!resultat.Correcte)
            {
                dgvPrevisualitzacio
                    .Rows[indexFila]
                    .DefaultCellStyle
                    .BackColor =
                        Color.FromArgb(
                            254,
                            226,
                            226);

                dgvPrevisualitzacio
                    .Rows[indexFila]
                    .DefaultCellStyle
                    .ForeColor =
                        Color.FromArgb(
                            153,
                            27,
                            27);
            }
        }

        /// <summary>
        /// Retorna els articles marcats
        /// a la graella superior.
        /// </summary>
        private List<ArticleCalculTarifes>
            ObtenirArticlesSeleccionats()
        {
            SincronitzarSeleccionsArticlesVisibles();

            return _articles
                .Where(
                    article =>
                        article != null
                        &&
                        article.Seleccionat)
                .ToList();
        }

        private void SincronitzarSeleccionsArticlesVisibles()
        {
            dgvArticles.EndEdit();

            foreach (
                DataGridViewRow fila
                in dgvArticles.Rows)
            {
                if (fila.IsNewRow)
                {
                    continue;
                }

                ArticleCalculTarifes article =
                    fila.Tag
                    as ArticleCalculTarifes;

                if (article == null)
                {
                    continue;
                }

                bool seleccionat =
                    ObtenirValorSeleccioFila(
                        fila);

                article.Seleccionat =
                    seleccionat;
            }
        }

        private void dgvArticles_CurrentCellDirtyStateChanged(
            object sender,
            EventArgs e)
        {
            if (dgvArticles.IsCurrentCellDirty)
            {
                dgvArticles.CommitEdit(
                    DataGridViewDataErrorContexts.Commit);
            }
        }

        private void dgvArticles_CellValueChanged(
            object sender,
            DataGridViewCellEventArgs e)
        {
            if (_actualitzantSeleccio ||
                e.RowIndex < 0 ||
                e.ColumnIndex < 0)
            {
                return;
            }

            if (dgvArticles
                    .Columns[e.ColumnIndex]
                    .Name !=
                "colSeleccionat")
            {
                return;
            }

            DataGridViewRow fila =
                dgvArticles.Rows[e.RowIndex];

            ArticleCalculTarifes article =
                fila.Tag
                as ArticleCalculTarifes;

            if (article != null)
            {
                article.Seleccionat =
                    ObtenirValorSeleccioFila(
                        fila);
            }

            ActualitzarComptadorArticles();
            InvalidarPrevisualitzacio();
            AplicarPasProces(2);
        }

        private void chkSeleccionarTots_CheckStateChanged(
            object sender,
            EventArgs e)
        {
            if (_actualitzantSeleccio ||
                chkSeleccionarTots.CheckState ==
                    CheckState.Indeterminate)
            {
                return;
            }

            bool seleccionar =
                chkSeleccionarTots.Checked;

            dgvArticles.EndEdit();

            _actualitzantSeleccio =
                true;

            try
            {
                foreach (
                    DataGridViewRow fila
                    in dgvArticles.Rows)
                {
                    if (fila.IsNewRow)
                    {
                        continue;
                    }

                    ArticleCalculTarifes article =
                        fila.Tag
                        as ArticleCalculTarifes;

                    AplicarSeleccioFila(
                        fila,
                        article,
                        seleccionar);
                }
            }
            finally
            {
                _actualitzantSeleccio =
                    false;
            }

            dgvArticles.EndEdit();
            dgvArticles.RefreshEdit();
            dgvArticles.Invalidate();

            ActualitzarComptadorArticles();
            InvalidarPrevisualitzacio();
            AplicarPasProces(2);
        }

        /// <summary>
        /// Actualitza el nombre d'articles seleccionats
        /// i l'estat del selector general.
        /// </summary>
        private void ActualitzarComptadorArticles()
        {
            int total =
                _articles.Count;

            int mostrats =
                ComptarFilesArticlesVisibles();

            int seleccionatsVisibles =
                dgvArticles.Rows
                    .Cast<DataGridViewRow>()
                    .Count(
                        fila =>
                            !fila.IsNewRow &&
                            ObtenirValorSeleccioFila(
                                fila));

            int seleccionats =
                _articles
                    .Count(
                        article =>
                            article != null
                            &&
                            article.Seleccionat);

            int foraFiltre =
                seleccionats - seleccionatsVisibles;

            string textComptador =
                "Mostrats: "
                + FormatarNombreEnter(
                    mostrats)
                + " de "
                + FormatarNombreEnter(
                    total)
                + " · Seleccionats: "
                + FormatarNombreEnter(
                    seleccionats);

            if (HiHaFiltreArticlesActiu() &&
                foraFiltre > 0)
            {
                textComptador +=
                    " · Fora del filtre: "
                    + FormatarNombreEnter(
                        foraFiltre);
            }

            lblComptadorArticles.Text =
                textComptador;

            btnPrevisualitzar.Enabled =
                seleccionats > 0;

            _actualitzantSeleccio =
                true;

            try
            {
                chkSeleccionarTots.Enabled =
                    mostrats > 0;

                if (seleccionatsVisibles == 0)
                {
                    chkSeleccionarTots.CheckState =
                        CheckState.Unchecked;
                }
                else if (seleccionatsVisibles == mostrats &&
                         mostrats > 0)
                {
                    chkSeleccionarTots.CheckState =
                        CheckState.Checked;
                }
                else
                {
                    chkSeleccionarTots.CheckState =
                        CheckState.Indeterminate;
                }
            }
            finally
            {
                _actualitzantSeleccio =
                    false;
            }
        }

        private int ComptarFilesArticlesVisibles()
        {
            return dgvArticles.Rows
                .Cast<DataGridViewRow>()
                .Count(
                    fila =>
                        !fila.IsNewRow);
        }

        private bool HiHaFiltreArticlesActiu()
        {
            return !string.IsNullOrWhiteSpace(
                _filtreArticles);
        }

        private static string FormatarNombreEnter(
            int valor)
        {
            return valor.ToString(
                "N0",
                CultureInfo.CurrentCulture);
        }

        private static bool ObtenirValorSeleccioFila(
            DataGridViewRow fila)
        {
            if (fila == null ||
                fila.IsNewRow)
            {
                return false;
            }

            object valor =
                fila
                    .Cells["colSeleccionat"]
                    .Value;

            return
                valor is bool seleccionat &&
                seleccionat;
        }

        private static void AplicarSeleccioFila(
            DataGridViewRow fila,
            ArticleCalculTarifes article,
            bool seleccionar)
        {
            if (fila == null ||
                fila.IsNewRow)
            {
                return;
            }

            if (article != null)
            {
                article.Seleccionat =
                    seleccionar;
            }

            fila
                .Cells["colSeleccionat"]
                .Value =
                    seleccionar;
        }

        private void parametre_ValueChanged(
            object sender,
            EventArgs e)
        {
            if (_inicialitzant)
            {
                return;
            }

            InvalidarPrevisualitzacio();
            AplicarPasProces(2);
        }

        private IEnumerable<NumericUpDown>
            ObtenirControlsNumericsEditables()
        {
            yield return numTarifa1;
            yield return numTarifa2;
            yield return numTarifa3;
            yield return numTarifa4;
            yield return numDescompte1;
            yield return numDescompte2;
            yield return numDescompte3;
            yield return numDescompte4;
        }

        private void ConnectarFiltreEnganxatNumericEditable(
            NumericUpDown control)
        {
            if (control == null ||
                _filtresEnganxatNumericEditable.ContainsKey(
                    control))
            {
                return;
            }

            TextBoxBase editor =
                ObtenirEditorInternNumericEditable(
                    control);

            if (editor == null)
            {
                return;
            }

            _filtresEnganxatNumericEditable.Add(
                control,
                new FiltreEnganxatNumericEditable(
                    this,
                    control,
                    editor));
        }

        private static TextBoxBase ObtenirEditorInternNumericEditable(
            NumericUpDown control)
        {
            if (control == null)
            {
                return null;
            }

            foreach (Control fill in control.Controls)
            {
                TextBoxBase editor =
                    fill as TextBoxBase;

                if (editor != null)
                {
                    return editor;
                }
            }

            return null;
        }

        private void numericEditable_KeyPress(
            object sender,
            KeyPressEventArgs e)
        {
            if (char.IsControl(
                e.KeyChar))
            {
                return;
            }

            if (e.KeyChar == '.' ||
                e.KeyChar == ',')
            {
                string separadorDecimal =
                    CultureInfo.CurrentCulture
                        .NumberFormat
                        .NumberDecimalSeparator;

                if (!string.IsNullOrEmpty(
                    separadorDecimal))
                {
                    e.KeyChar =
                        separadorDecimal[0];
                }

                NumericUpDown control =
                    sender as NumericUpDown;

                if (control != null &&
                    HiHaSeparadorDecimalEditableForaSeleccio(
                        control))
                {
                    e.Handled =
                        true;
                }
            }
        }

        private static bool HiHaSeparadorDecimalEditableForaSeleccio(
            NumericUpDown control)
        {
            TextBoxBase editor =
                ObtenirEditorInternNumericEditable(
                    control);

            string text;

            if (editor == null)
            {
                text =
                    control.Text ?? string.Empty;
            }
            else
            {
                text =
                    editor.Text ?? string.Empty;
            }

            if (editor != null &&
                editor.SelectionLength > 0)
            {
                text =
                    text.Remove(
                        editor.SelectionStart,
                        editor.SelectionLength);
            }

            return ComptarCaracters(
                text,
                '.') +
                ComptarCaracters(
                    text,
                    ',') > 0;
        }

        private void numericEditable_TextChanged(
            object sender,
            EventArgs e)
        {
            if (_normalitzantControlNumericEditable)
            {
                return;
            }

            NumericUpDown control =
                sender as NumericUpDown;

            if (control == null)
            {
                return;
            }

            string text =
                control.Text ?? string.Empty;

            string textNormalitzat;

            if (IntentarNormalitzarPuntDecimal(
                text,
                out textNormalitzat))
            {
                _normalitzantControlNumericEditable =
                    true;

                try
                {
                    control.Text =
                        textNormalitzat;

                    control.Select(
                        textNormalitzat.Length,
                        0);
                }
                finally
                {
                    _normalitzantControlNumericEditable =
                        false;
                }
            }

            if (!_inicialitzant)
            {
                InvalidarPrevisualitzacio();
                AplicarPasProces(2);
            }
        }

        private void numericEditable_Validating(
            object sender,
            CancelEventArgs e)
        {
            NumericUpDown control =
                sender as NumericUpDown;

            if (control == null)
            {
                return;
            }

            if (!ValidarControlNumericEditable(
                control,
                true))
            {
                e.Cancel =
                    true;
            }
        }

        private bool ValidarControlsNumericsEditables()
        {
            foreach (
                NumericUpDown control
                in ObtenirControlsNumericsEditables())
            {
                if (!ValidarControlNumericEditable(
                    control,
                    true))
                {
                    control.Focus();

                    return false;
                }
            }

            return true;
        }

        private bool ProcessarEnganxatNumericEditable(
            NumericUpDown control,
            TextBoxBase editor)
        {
            string textPortaRetalls;

            if (control == null ||
                editor == null ||
                !IntentarObtenirTextPortaRetalls(
                    out textPortaRetalls))
            {
                return true;
            }

            string textOriginal =
                (textPortaRetalls ?? string.Empty).Trim();

            decimal valorOriginal;

            if (!IntentarLlegirDecimalEditable(
                textOriginal,
                out valorOriginal))
            {
                MostrarMissatgeValorDecimalInvalid();
                control.Focus();

                return true;
            }

            string textNormalitzat =
                NormalitzarSeparadorDecimalEditable(
                    textOriginal);

            string textResultant =
                CrearTextAmbSeleccioSubstituida(
                    editor,
                    textNormalitzat);

            decimal valorResultant;

            if (!IntentarLlegirDecimalEditable(
                textResultant,
                out valorResultant)
                ||
                valorResultant < control.Minimum
                ||
                valorResultant > control.Maximum)
            {
                MostrarMissatgeValorDecimalInvalid();
                control.Focus();

                return true;
            }

            editor.SelectedText =
                textNormalitzat;

            return true;
        }

        private bool ValidarControlNumericEditable(
            NumericUpDown control,
            bool mostrarMissatge)
        {
            if (control == null)
            {
                return true;
            }

            decimal valor;

            if (!IntentarLlegirDecimalEditable(
                control.Text,
                out valor)
                ||
                valor < control.Minimum
                ||
                valor > control.Maximum)
            {
                if (mostrarMissatge)
                {
                    MostrarMissatgeValorDecimalInvalid();
                }

                return false;
            }

            _normalitzantControlNumericEditable =
                true;

            try
            {
                control.Value =
                    valor;

                control.Text =
                    control.Value.ToString(
                        "F" + control.DecimalPlaces,
                        CultureInfo.CurrentCulture);
            }
            finally
            {
                _normalitzantControlNumericEditable =
                    false;
            }

            return true;
        }

        private void MostrarMissatgeValorDecimalInvalid()
        {
            MessageBox.Show(
                this,
                "Introdueix un valor decimal vàlid. "
                + "Pots utilitzar coma o punt com a separador decimal.",
                "Calculador de tarifes",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning);
        }

        private static bool IntentarObtenirTextPortaRetalls(
            out string text)
        {
            text =
                string.Empty;

            try
            {
                if (!Clipboard.ContainsText(
                    TextDataFormat.Text))
                {
                    return false;
                }

                text =
                    Clipboard.GetText(
                        TextDataFormat.Text);

                return true;
            }
            catch (ExternalException)
            {
                return false;
            }
        }

        private static string CrearTextAmbSeleccioSubstituida(
            TextBoxBase editor,
            string textSubstitut)
        {
            string textActual =
                editor.Text ?? string.Empty;

            int inici =
                Math.Max(
                    0,
                    Math.Min(
                        editor.SelectionStart,
                        textActual.Length));

            int longitud =
                Math.Max(
                    0,
                    Math.Min(
                        editor.SelectionLength,
                        textActual.Length - inici));

            return textActual
                .Remove(
                    inici,
                    longitud)
                .Insert(
                    inici,
                    textSubstitut ?? string.Empty);
        }

        private static string NormalitzarSeparadorDecimalEditable(
            string text)
        {
            string textNormalitzat =
                text ?? string.Empty;

            string separadorDecimal =
                CultureInfo.CurrentCulture
                    .NumberFormat
                    .NumberDecimalSeparator;

            if (string.IsNullOrEmpty(
                separadorDecimal))
            {
                return textNormalitzat;
            }

            return textNormalitzat
                .Replace(
                    ".",
                    separadorDecimal)
                .Replace(
                    ",",
                    separadorDecimal);
        }

        private static bool IntentarNormalitzarPuntDecimal(
            string text,
            out string textNormalitzat)
        {
            textNormalitzat =
                text ?? string.Empty;

            if (string.IsNullOrEmpty(
                textNormalitzat)
                ||
                textNormalitzat.IndexOf('.') < 0
                ||
                textNormalitzat.IndexOf(',') >= 0)
            {
                return false;
            }

            if (ComptarCaracters(
                textNormalitzat,
                '.') != 1)
            {
                return false;
            }

            string separadorDecimal =
                CultureInfo.CurrentCulture
                    .NumberFormat
                    .NumberDecimalSeparator;

            if (string.IsNullOrEmpty(
                separadorDecimal)
                ||
                separadorDecimal == ".")
            {
                return false;
            }

            textNormalitzat =
                textNormalitzat.Replace(
                    ".",
                    separadorDecimal);

            return true;
        }

        private static bool IntentarLlegirDecimalEditable(
            string text,
            out decimal valor)
        {
            valor =
                0M;

            string valorText =
                (text ?? string.Empty).Trim();

            if (string.IsNullOrWhiteSpace(
                valorText))
            {
                return false;
            }

            int punts =
                ComptarCaracters(
                    valorText,
                    '.');

            int comes =
                ComptarCaracters(
                    valorText,
                    ',');

            if (punts + comes > 1)
            {
                return false;
            }

            string invariant =
                valorText.Replace(
                    ',',
                    '.');

            return decimal.TryParse(
                invariant,
                NumberStyles.AllowLeadingSign |
                NumberStyles.AllowDecimalPoint,
                CultureInfo.InvariantCulture,
                out valor);
        }

        private static int ComptarCaracters(
            string text,
            char caracter)
        {
            if (string.IsNullOrEmpty(
                text))
            {
                return 0;
            }

            int total =
                0;

            foreach (char valor in text)
            {
                if (valor == caracter)
                {
                    total++;
                }
            }

            return total;
        }

        /// <summary>
        /// Invalida completament els resultats quan canvia
        /// qualsevol selecció, fórmula o paràmetre.
        ///
        /// Després d'aquest mètode no es pot aplicar
        /// cap resultat anterior.
        /// </summary>
        private void InvalidarPrevisualitzacio()
        {
            _resultatsPrevisualitzacio.Clear();

            _previsualitzacioValida =
                false;

            dgvPrevisualitzacio.Rows.Clear();

            lblResumSeleccionats.Text =
                "Seleccionats: 0";

            lblResumCorrectes.Text =
                "Correctes: 0";

            lblResumErrors.Text =
                "Errors: 0";

            btnAplicar.Enabled =
                false;

            lblEstatGeneral.Text =
                "Hi ha canvis pendents. Genera una nova previsualització.";
        }

        /// <summary>
        /// Confirma una sola vegada l'operació,
        /// comprova internament totes les dades
        /// i aplica definitivament els canvis.
        ///
        /// El flux normal només mostra:
        /// - una finestra de confirmació inicial;
        /// - una finestra amb el resultat final.
        /// </summary>
        private async void btnAplicar_Click(
            object sender,
            EventArgs e)
        {
            if (!ValidarControlsNumericsEditables())
            {
                return;
            }

            if (!TePrevisualitzacioValida())
            {
                btnAplicar.Enabled =
                    false;

                CalculadorTarifesLogger.Advertencia(
                    "Aplicació cancel·lada: previsualització no vàlida.",
                    CrearCampsContextLog());

                MessageBox.Show(
                    this,
                    "La previsualització ja no és vàlida."
                    + Environment.NewLine
                    + Environment.NewLine
                    + "Genera-la novament abans d'aplicar "
                    + "les tarifes.",
                    "Calculador de tarifes",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);

                return;
            }

            if (!_contextA3Erp.ConnexioDisponible)
            {
                CalculadorTarifesLogger.Advertencia(
                    "Aplicació cancel·lada: connexió no disponible.",
                    CrearCampsContextLog());

                MessageBox.Show(
                    this,
                    "No hi ha cap connexió disponible "
                    + "amb l'empresa activa.",
                    "Calculador de tarifes",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);

                return;
            }

            /*
             * Fem una còpia estable dels resultats.
             * No llegirem els imports directament
             * de les cel·les de la graella.
             */
            List<ResultatCalculTarifes> lot =
                _resultatsPrevisualitzacio
                    .ToList();

            FormulaTarifa formulaAplicada =
                cmbPlantilla.SelectedItem
                as FormulaTarifa;

            int nombreArticles =
                lot.Count;

            int nombreTarifes =
                nombreArticles * 6;

            int nombreArticlesAmbDescomptes =
                lot.Count(
                    resultat =>
                        resultat.GeneraDescomptes);

            int nombreDescomptes =
                nombreArticlesAmbDescomptes * 4;

            int nombreArticlesSenseDescomptes =
                lot.Count(
                    resultat =>
                        !resultat.GeneraDescomptes);

            int nombreDescomptesEliminacioPrevistos =
                nombreArticlesSenseDescomptes * 4;

            bool hiHaDescomptes =
                nombreDescomptes > 0;

            bool hiHaEliminacioDescomptes =
                nombreDescomptesEliminacioPrevistos > 0;

            string resumOperacions =
                "Articles: "
                + nombreArticles
                + Environment.NewLine
                + "Tarifes: "
                + nombreTarifes
                + Environment.NewLine
                + "Descomptes creats/actualitzats: "
                + nombreDescomptes
                + Environment.NewLine
                + "Descomptes gestionats a eliminar: "
                + nombreDescomptesEliminacioPrevistos;

            string textConfirmacio =
                hiHaDescomptes
                    ? "S'aplicaran les tarifes i es crearan "
                      + "o actualitzaran els descomptes següents:"
                    : "S'aplicaran les tarifes següents:";

            textConfirmacio +=
                Environment.NewLine
                + Environment.NewLine
                + "Fórmula: "
                + (formulaAplicada == null
                    ? string.Empty
                    : formulaAplicada.Nom)
                + Environment.NewLine
                + Environment.NewLine
                + resumOperacions
                + Environment.NewLine;

            if (hiHaEliminacioDescomptes)
            {
                textConfirmacio +=
                    Environment.NewLine
                    + "Aquesta fórmula no genera descomptes. "
                    + "S'eliminaran els descomptes dels grups "
                    + "1-4 gestionats pel Calculador per als "
                    + "articles seleccionats."
                    + Environment.NewLine;
            }

            textConfirmacio +=
                Environment.NewLine
                + "Abans de guardar, el sistema comprovarà "
                + "totes les operacions."
                + Environment.NewLine
                + "Si es detecta qualsevol error, "
                + "no es guardarà cap canvi."
                + Environment.NewLine
                + Environment.NewLine
                + "Vols continuar?";

            DialogResult confirmacio =
                MessageBox.Show(
                    this,
                    textConfirmacio,
                    hiHaDescomptes
                        ? "Aplicar tarifes i descomptes"
                        : "Aplicar tarifes i netejar descomptes",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question,
                    MessageBoxDefaultButton.Button2);

            if (confirmacio !=
                DialogResult.Yes)
            {
                CalculadorTarifesLogger.Informacio(
                    "Aplicació cancel·lada per l'usuari.",
                    CrearCampsContextLog(
                        new Dictionary<string, object>
                        {
                            {
                                "Articles",
                                nombreArticles
                            },
                            {
                                "TarifesPrevistes",
                                nombreTarifes
                            },
                            {
                                "DescomptesPrevistos",
                                nombreDescomptes
                            },
                            {
                                "DescomptesEliminacioPrevistos",
                                nombreDescomptesEliminacioPrevistos
                            }
                        }));

                return;
            }

            CalculadorTarifesLogger.Informacio(
                "S'inicia el procés després de confirmar.",
                CrearCampsContextLog(
                    new Dictionary<string, object>
                    {
                        {
                            "Articles",
                            nombreArticles
                        },
                        {
                            "FormulaId",
                            formulaAplicada == null
                                ? 0
                                : formulaAplicada.Id
                        },
                        {
                            "FormulaCodi",
                            formulaAplicada == null
                                ? string.Empty
                                : formulaAplicada.Codi
                        },
                        {
                            "FormulaNom",
                            formulaAplicada == null
                                ? string.Empty
                                : formulaAplicada.Nom
                        },
                        {
                            "GeneraDescomptes",
                            formulaAplicada == null
                                ? hiHaDescomptes
                                : formulaAplicada.GeneraDescomptes
                        },
                        {
                            "TarifesPrevistes",
                            nombreTarifes
                        },
                        {
                            "DescomptesPrevistos",
                            nombreDescomptes
                        },
                        {
                            "DescomptesEliminacioPrevistos",
                            nombreDescomptesEliminacioPrevistos
                        }
                    }));

            bool aplicacioCompletada =
                false;

            /*
             * Bloquegem els controls mentre
             * s'executa el procés.
             */
            btnAplicar.Enabled =
                false;

            btnPrevisualitzar.Enabled =
                false;

            dgvArticles.Enabled =
                false;

            cmbPlantilla.Enabled =
                false;

            numTarifa1.Enabled =
                false;

            numTarifa2.Enabled =
                false;

            numTarifa3.Enabled =
                false;

            numTarifa4.Enabled =
                false;

            numDescompte1.Enabled =
                false;

            numDescompte2.Enabled =
                false;

            numDescompte3.Enabled =
                false;

            numDescompte4.Enabled =
                false;

            btnRestablirValors.Enabled =
                false;

            EstablirCursorEspera(
                true);

            try
            {
                /*
                 * FASE 1:
                 * comprovació interna amb ROLLBACK.
                 *
                 * No es mostra cap finestra intermèdia.
                 */
                string textProgresSimulacio =
                    hiHaDescomptes
                        ? "Comprovant tarifes i descomptes..."
                        : "Comprovant tarifes i neteja de descomptes...";

                lblEstatGeneral.Text =
                    textProgresSimulacio;

                ReiniciarProgresAplicacioTarifes();

                IProgress<ProgresAplicacioTarifes> progresSimulacio =
                    new Progress<ProgresAplicacioTarifes>(
                        delegate (ProgresAplicacioTarifes progres)
                        {
                            ActualitzarProgresAplicacioTarifes(
                                progres,
                                textProgresSimulacio);
                        });

                CalculadorTarifesLogger.Informacio(
                    "S'inicia la simulació.",
                    CrearCampsContextLog(
                        new Dictionary<string, object>
                        {
                            {
                                "Articles",
                                nombreArticles
                            },
                            {
                                "TarifesPrevistes",
                                nombreTarifes
                            },
                            {
                                "DescomptesPrevistos",
                                nombreDescomptes
                            },
                            {
                                "DescomptesEliminacioPrevistos",
                                nombreDescomptesEliminacioPrevistos
                            }
                        }));

                ResultatSimulacioAplicacioTarifes simulacio =
                    await Task.Run(
                        delegate
                        {
                            return _repositoriAplicacioTarifes
                                .Simular(
                                    _contextA3Erp
                                        .CadenaConnexio,
                                    lot,
                                    progresSimulacio);
                        });

                if (IsDisposed ||
                    Disposing)
                {
                    return;
                }

                if (!simulacio.Correcte)
                {
                    CalculadorTarifesLogger.Error(
                        "Error en la simulació. ROLLBACK executat.",
                        new InvalidOperationException(
                            simulacio.Missatge),
                        CrearCampsContextLog(
                            new Dictionary<string, object>
                            {
                                {
                                    "Articles",
                                    nombreArticles
                                },
                                {
                                    "TarifesPrevistes",
                                    nombreTarifes
                                },
                                {
                                    "DescomptesPrevistos",
                                    nombreDescomptes
                                },
                                {
                                    "DescomptesEliminacioPrevistos",
                                    nombreDescomptesEliminacioPrevistos
                                }
                            }));

                    EstablirCursorEspera(
                        false);

                    lblEstatGeneral.Text =
                        "No s'han pogut comprovar les dades.";

                    MessageBox.Show(
                        this,
                        "No s'han pogut aplicar els canvis."
                        + Environment.NewLine
                        + Environment.NewLine
                        + "No s'ha guardat cap canvi."
                        + Environment.NewLine
                        + Environment.NewLine
                        + simulacio.Missatge,
                        "Procés no completat",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error);

                    return;
                }

                CompletarProgresAplicacioTarifes();

                CalculadorTarifesLogger.Informacio(
                    "Simulació finalitzada correctament.",
                    CrearCampsContextLog(
                        new Dictionary<string, object>
                        {
                            {
                                "OperacionsComprovades",
                                simulacio.TarifesProcessades
                                + simulacio.DescomptesProcessats
                                + simulacio.DescomptesEliminats
                            },
                            {
                                "Articles",
                                simulacio.ArticlesProcessats
                            },
                            {
                                "Tarifes",
                                simulacio.TarifesProcessades
                            },
                            {
                                "Descomptes",
                                simulacio.DescomptesProcessats
                            },
                            {
                                "DescomptesEliminats",
                                simulacio.DescomptesEliminats
                            }
                        }));

                /*
                 * FASE 2:
                 * aplicació definitiva amb COMMIT.
                 *
                 * La simulació ha estat correcta,
                 * per tant continuem automàticament.
                 */
                string textProgresAplicacio =
                    hiHaDescomptes
                        ? "Aplicant tarifes i descomptes..."
                        : "Aplicant tarifes i eliminant descomptes...";

                lblEstatGeneral.Text =
                    textProgresAplicacio;

                ReiniciarProgresAplicacioTarifes();

                IProgress<ProgresAplicacioTarifes> progresAplicacio =
                    new Progress<ProgresAplicacioTarifes>(
                        delegate (ProgresAplicacioTarifes progres)
                        {
                            ActualitzarProgresAplicacioTarifes(
                                progres,
                                textProgresAplicacio);
                        });

                CalculadorTarifesLogger.Informacio(
                    "S'inicia l'aplicació definitiva.",
                    CrearCampsContextLog(
                        new Dictionary<string, object>
                        {
                            {
                                "Articles",
                                nombreArticles
                            },
                            {
                                "TarifesPrevistes",
                                nombreTarifes
                            },
                            {
                                "DescomptesPrevistos",
                                nombreDescomptes
                            },
                            {
                                "DescomptesEliminacioPrevistos",
                                nombreDescomptesEliminacioPrevistos
                            }
                        }));

                ResultatAplicacioTarifes aplicacio =
                    await Task.Run(
                        delegate
                        {
                            return _repositoriAplicacioTarifes
                                .Aplicar(
                                    _contextA3Erp
                                        .CadenaConnexio,
                                    lot,
                                    progresAplicacio);
                        });

                if (IsDisposed ||
                    Disposing)
                {
                    return;
                }

                EstablirCursorEspera(
                    false);

                if (!aplicacio.Correcte)
                {
                    CalculadorTarifesLogger.Error(
                        "Error en l'aplicació definitiva. ROLLBACK executat.",
                        new InvalidOperationException(
                            aplicacio.Missatge),
                        CrearCampsContextLog(
                            new Dictionary<string, object>
                            {
                                {
                                    "Articles",
                                    nombreArticles
                                },
                                {
                                    "TarifesPrevistes",
                                    nombreTarifes
                                },
                                {
                                    "DescomptesPrevistos",
                                    nombreDescomptes
                                },
                                {
                                    "DescomptesEliminacioPrevistos",
                                    nombreDescomptesEliminacioPrevistos
                                }
                            }));

                    lblEstatGeneral.Text =
                        "No s'han pogut aplicar els canvis.";

                    MessageBox.Show(
                        this,
                        "No s'han pogut aplicar els canvis."
                        + Environment.NewLine
                        + Environment.NewLine
                        + "No s'ha guardat cap canvi."
                        + Environment.NewLine
                        + Environment.NewLine
                        + aplicacio.Missatge,
                        "Procés no completat",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error);

                    return;
                }

                CompletarProgresAplicacioTarifes();

                aplicacioCompletada =
                    true;

                CalculadorTarifesLogger.Informacio(
                    "COMMIT correcte de l'aplicació definitiva.",
                    CrearCampsContextLog(
                        new Dictionary<string, object>
                        {
                            {
                                "Articles",
                                aplicacio.ArticlesProcessats
                            },
                            {
                                "FormulaId",
                                formulaAplicada == null
                                    ? 0
                                    : formulaAplicada.Id
                            },
                            {
                                "FormulaCodi",
                                formulaAplicada == null
                                    ? string.Empty
                                    : formulaAplicada.Codi
                            },
                            {
                                "FormulaNom",
                                formulaAplicada == null
                                    ? string.Empty
                                    : formulaAplicada.Nom
                            },
                            {
                                "GeneraDescomptes",
                                formulaAplicada == null
                                    ? hiHaDescomptes
                                    : formulaAplicada.GeneraDescomptes
                            },
                            {
                                "Tarifes",
                                aplicacio.TarifesProcessades
                            },
                            {
                                "Descomptes",
                                aplicacio.DescomptesProcessats
                            },
                            {
                                "DescomptesEliminats",
                                aplicacio.DescomptesEliminats
                            }
                        }));

                /*
                 * Evitem que el mateix lot es pugui
                 * tornar a aplicar accidentalment.
                 */
                _previsualitzacioValida =
                    false;

                btnAplicar.Enabled =
                    false;

                lblEstatGeneral.Text =
                    hiHaDescomptes
                        ? "Tarifes i descomptes aplicats correctament."
                        : "Tarifes aplicades.";

                AplicarPasProces(4);

                MessageBox.Show(
                    this,
                    "Procés completat correctament."
                    + Environment.NewLine
                    + Environment.NewLine
                    + "Articles processats: "
                    + aplicacio.ArticlesProcessats
                    + Environment.NewLine
                    + "Tarifes aplicades: "
                    + aplicacio.TarifesProcessades
                    + Environment.NewLine
                    + "Descomptes creats/actualitzats: "
                    + aplicacio.DescomptesProcessats
                    + Environment.NewLine
                    + "Descomptes eliminats: "
                    + aplicacio.DescomptesEliminats
                    + Environment.NewLine
                    + "Errors: 0",
                    "Procés completat",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                CalculadorTarifesLogger.Error(
                    "Error inesperat en el procés d'aplicació.",
                    ex,
                    CrearCampsContextLog(
                        new Dictionary<string, object>
                        {
                            {
                                "Articles",
                                nombreArticles
                            },
                            {
                                "TarifesPrevistes",
                                nombreTarifes
                            },
                            {
                                "DescomptesPrevistos",
                                nombreDescomptes
                            },
                            {
                                "DescomptesEliminacioPrevistos",
                                nombreDescomptesEliminacioPrevistos
                            }
                        }));

                EstablirCursorEspera(
                    false);

                lblEstatGeneral.Text =
                    "No s'han pogut aplicar els canvis.";

                MessageBox.Show(
                    this,
                    "No s'han pogut aplicar els canvis."
                    + Environment.NewLine
                    + Environment.NewLine
                    + "No s'ha guardat cap canvi."
                    + Environment.NewLine
                    + Environment.NewLine
                    + ex.Message,
                    "Procés no completat",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
            finally
            {
                if (!IsDisposed &&
                    !Disposing)
                {
                    EstablirCursorEspera(
                        false);

                    dgvArticles.Enabled =
                        true;

                    cmbPlantilla.Enabled =
                        true;

                    numTarifa1.Enabled =
                        true;

                    numTarifa2.Enabled =
                        true;

                    numTarifa3.Enabled =
                        true;

                    numTarifa4.Enabled =
                        true;

                    numDescompte1.Enabled =
                        true;

                    numDescompte2.Enabled =
                        true;

                    numDescompte3.Enabled =
                        true;

                    numDescompte4.Enabled =
                        true;

                    btnRestablirValors.Enabled =
                        true;

                    btnPrevisualitzar.Enabled =
                        ObtenirArticlesSeleccionats()
                            .Count > 0;

                    btnAplicar.Enabled =
                        !aplicacioCompletada
                        &&
                        TePrevisualitzacioValida();
                }
            }
        }



        private void btnTancar_Click(
            object sender,
            EventArgs e)
        {
            Close();
        }

        private void FrmCalculadorTarifes_FormClosed(
            object sender,
            FormClosedEventArgs e)
        {
            AlliberarFiltresEnganxatNumericEditable();

            CalculadorTarifesLogger.Informacio(
                "Es tanca el calculador de tarifes.",
                CrearCampsContextLog());
        }

        private void AlliberarFiltresEnganxatNumericEditable()
        {
            foreach (
                FiltreEnganxatNumericEditable filtre
                in _filtresEnganxatNumericEditable.Values)
            {
                filtre.Dispose();
            }

            _filtresEnganxatNumericEditable.Clear();
        }

        /// <summary>
        /// Actualitza visualment els tres passos.
        /// </summary>
        private void AplicarPasProces(
            int pasActiu)
        {
            AplicarEstatPas(
                pnlIndicadorPas1,
                lblNumeroPas1,
                pasActiu,
                1);

            AplicarEstatPas(
                pnlIndicadorPas2,
                lblNumeroPas2,
                pasActiu,
                2);

            AplicarEstatPas(
                pnlIndicadorPas3,
                lblNumeroPas3,
                pasActiu,
                3);
        }

        private static void AplicarEstatPas(
            Panel indicador,
            Label numero,
            int pasActiu,
            int numeroPas)
        {
            if (numeroPas < pasActiu)
            {
                indicador.BackColor =
                    Color.FromArgb(
                        16,
                        185,
                        129);

                numero.ForeColor =
                    Color.White;

                numero.Text =
                    "✓";

                return;
            }

            if (numeroPas == pasActiu)
            {
                indicador.BackColor =
                    Color.FromArgb(
                        14,
                        165,
                        233);

                numero.ForeColor =
                    Color.White;

                numero.Text =
                    numeroPas.ToString();

                return;
            }

            indicador.BackColor =
                Color.FromArgb(
                    226,
                    232,
                    240);

            numero.ForeColor =
                Color.FromArgb(
                    71,
                    85,
                    105);

            numero.Text =
                numeroPas.ToString();
        }

        /// <summary>
        /// Força el mateix sistema de renderització de text
        /// tant quan el formulari s'executa directament com
        /// quan s'obre dins del procés d'a3ERP.
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

            foreach (
                Control controlFill
                in control.Controls)
            {
                AplicarRenderitzatText(
                    controlFill);
            }
        }

        /// <summary>
        /// Col·loca la casella de selecció general dins
        /// del capçal de la primera columna de la graella.
        ///
        /// Permet seleccionar o desseleccionar tots els articles
        /// i mostra l'estat intermedi quan només n'hi ha alguns.
        /// </summary>
        private void PrepararSelectorGeneralArticles()
        {
            chkSeleccionarTots.Text =
                string.Empty;

            chkSeleccionarTots.AutoSize =
                false;

            chkSeleccionarTots.Size =
                new Size(
                    18,
                    18);

            chkSeleccionarTots.ThreeState =
                true;

            chkSeleccionarTots.TabStop =
                false;

            chkSeleccionarTots.Cursor =
                Cursors.Hand;

            chkSeleccionarTots.BackColor =
                dgvArticles
                    .ColumnHeadersDefaultCellStyle
                    .BackColor;

            /*
             * Canviem el pare del control.
             * Això el retira automàticament
             * del panell superior d'articles.
             */
            chkSeleccionarTots.Parent =
                dgvArticles;

            chkSeleccionarTots.BringToFront();

            PosicionarSelectorGeneralArticles();

            dgvArticles.Resize +=
                delegate
                {
                    PosicionarSelectorGeneralArticles();
                };

            dgvArticles.Scroll +=
                delegate
                {
                    PosicionarSelectorGeneralArticles();
                };

            dgvArticles.ColumnWidthChanged +=
                delegate
                {
                    PosicionarSelectorGeneralArticles();
                };
        }

        /// <summary>
        /// Centra la casella dins del capçal
        /// de la columna de selecció.
        /// </summary>
        private void PosicionarSelectorGeneralArticles()
        {
            if (!dgvArticles.Columns.Contains(
                "colSeleccionat"))
            {
                return;
            }

            int ampladaColumna =
                dgvArticles
                    .Columns["colSeleccionat"]
                    .Width;

            int posicioX =
                Math.Max(
                    0,
                    (
                        ampladaColumna
                        - chkSeleccionarTots.Width
                    )
                    / 2);

            int posicioY =
                Math.Max(
                    0,
                    (
                        dgvArticles.ColumnHeadersHeight
                        - chkSeleccionarTots.Height
                    )
                    / 2);

            chkSeleccionarTots.Location =
                new Point(
                    posicioX,
                    posicioY);

            chkSeleccionarTots.BringToFront();
        }

        /// <summary>
        /// Comprova que existeix una previsualització
        /// completa, correcta i encara vigent.
        /// </summary>
        private bool TePrevisualitzacioValida()
        {
            if (!_previsualitzacioValida ||
                _resultatsPrevisualitzacio.Count == 0)
            {
                return false;
            }

            return _resultatsPrevisualitzacio.All(
                resultat =>
                    resultat != null
                    &&
                    resultat.Correcte
                    &&
                    !string.IsNullOrWhiteSpace(
                        resultat.CodiArticleBaseDades));
        }

        /// <summary>
        /// Recupera i valida les fórmules actives
        /// configurades a la base de dades.
        ///
        /// El resultat alimenta el selector
        /// de fórmules del Calculador.
        /// </summary>
        private void CarregarFormulesConfigurablesActives()
        {
            if (!_contextA3Erp.ConnexioDisponible)
            {
                throw new InvalidOperationException(
                    "No es poden carregar les fórmules "
                    + "perquè no hi ha connexió amb a3ERP.");
            }

            _formulesConfigurablesActives =
                _serveiCatalegFormulesConfigurables.Carregar(
                    _contextA3Erp.CadenaConnexio);

            CalculadorTarifesLogger.Informacio(
                "S'han carregat les fórmules configurables actives.",
                CrearCampsContextLog(
                    new Dictionary<string, object>
                    {
                {
                    "TotalFormulesConfigurables",
                    _formulesConfigurablesActives.Count
                }
                    }));
        }

        /// <summary>
        /// Torna a consultar les fórmules actives
        /// després de tancar el Gestor.
        ///
        /// Conserva la fórmula seleccionada quan
        /// continua activa. Si ha estat desactivada
        /// o eliminada, selecciona la primera disponible.
        /// </summary>
        private async Task RecarregarFormulesDespresDelGestorAsync()
        {
            FormulaTarifa formulaAnterior =
                cmbPlantilla.SelectedItem
                as FormulaTarifa;

            int? idFormulaAnterior =
                formulaAnterior != null
                    ? (int?)formulaAnterior.Id
                    : null;

            _inicialitzant =
                true;

            try
            {
                CarregarFormulesConfigurablesActives();

                CarregarFormulesAlSelector(
                    idFormulaAnterior);
            }
            finally
            {
                _inicialitzant =
                    false;
            }

            AplicarFormulaSeleccionada();
            InvalidarPrevisualitzacio();
            AplicarPasProces(2);

            FormulaTarifa formulaActual =
                cmbPlantilla.SelectedItem
                as FormulaTarifa;

            CalculadorTarifesLogger.Informacio(
                "S'ha recarregat el catàleg de fórmules "
                + "després de tancar el gestor.",
                CrearCampsContextLog(
                    new Dictionary<string, object>
                    {
                {
                    "TotalFormulesConfigurables",
                    _formulesConfigurablesActives.Count
                },
                {
                    "FormulaSeleccionadaId",
                    formulaActual != null
                        ? (object)formulaActual.Id
                        : string.Empty
                },
                {
                    "FormulaSeleccionadaCodi",
                    formulaActual != null
                        ? (object)formulaActual.Codi
                        : string.Empty
                }
                    }));

            if (_contextA3Erp.ConnexioDisponible)
            {
                _carregaArticlesIniciada =
                    true;

                await CarregarArticlesFormulaSeleccionadaAsync();
            }
        }

        private sealed class FiltreEnganxatNumericEditable :
            NativeWindow,
            IDisposable
        {
            private const int WmPaste =
                0x0302;

            private readonly FrmCalculadorTarifes _formulari;

            private readonly NumericUpDown _control;

            private readonly TextBoxBase _editor;

            private bool _handleAssignat;

            public FiltreEnganxatNumericEditable(
                FrmCalculadorTarifes formulari,
                NumericUpDown control,
                TextBoxBase editor)
            {
                _formulari =
                    formulari;

                _control =
                    control;

                _editor =
                    editor;

                _editor.HandleCreated +=
                    editor_HandleCreated;

                _editor.HandleDestroyed +=
                    editor_HandleDestroyed;

                AssignarHandleEditor();
            }

            protected override void WndProc(
                ref Message m)
            {
                if (m.Msg == WmPaste)
                {
                    if (_formulari.ProcessarEnganxatNumericEditable(
                        _control,
                        _editor))
                    {
                        return;
                    }
                }

                base.WndProc(
                    ref m);
            }

            public void Dispose()
            {
                _editor.HandleCreated -=
                    editor_HandleCreated;

                _editor.HandleDestroyed -=
                    editor_HandleDestroyed;

                AlliberarHandleEditor();
            }

            private void editor_HandleCreated(
                object sender,
                EventArgs e)
            {
                AssignarHandleEditor();
            }

            private void editor_HandleDestroyed(
                object sender,
                EventArgs e)
            {
                AlliberarHandleEditor();
            }

            private void AssignarHandleEditor()
            {
                if (_handleAssignat ||
                    !_editor.IsHandleCreated)
                {
                    return;
                }

                AssignHandle(
                    _editor.Handle);

                _handleAssignat =
                    true;
            }

            private void AlliberarHandleEditor()
            {
                if (!_handleAssignat)
                {
                    return;
                }

                ReleaseHandle();

                _handleAssignat =
                    false;
            }
        }
    }
}
