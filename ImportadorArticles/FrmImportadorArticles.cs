using A3ErpImportadorArticles.Infrastructure.Logging;
using A3ErpImportadorArticles.Models;
using A3ErpImportadorArticles.Readers;
using A3ErpImportadorArticles.Services;
using MAT0943Net.Infrastructure;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace A3ErpImportadorArticles
{
    public partial class FrmImportadorArticles : Form
    {
        /// <summary>
        /// Representa l'estat visual del procés d'importació.
        /// Aquest estat només controla la interfície; no executa la importació.
        /// </summary>
        private enum EstatVisualImportacio
        {
            Desactivat,
            Preparat,
            Processant,
            Finalitzat
        }

        /// <summary>
        /// Estats visuals possibles d'un pas del procés.
        /// </summary>
        private enum EstatVisualPasProces
        {
            Pendent,
            Actiu,
            Completat
        }

        /// <summary>
        /// Controls visuals associats a un pas.
        /// </summary>
        private sealed class ControlsPasProces
        {
            public int Numero { get; set; }

            public Label EtiquetaNumero { get; set; }

            public Label EtiquetaText { get; set; }
        }

        /// <summary>
        /// Passos visuals de l'importador, indexats pel seu número.
        /// </summary>
        private readonly Dictionary<int, ControlsPasProces>
            controlsPassosProces =
                new Dictionary<int, ControlsPasProces>();

        /// <summary>
        /// Context d'a3ERP resolt abans d'obrir el formulari.
        /// </summary>
        private readonly ContextImportadorA3Erp contextA3Erp;
        // Serveis encarregats de llegir i interpretar els fitxers.
        private readonly LectorFitxerArticles lectorFitxers =
            new LectorFitxerArticles();

        private readonly ConvertidorArticlesImportacio convertidorArticles =
            new ConvertidorArticlesImportacio();
        /// <summary>
        /// Valida els articles de l'Excel contra l'empresa activa d'a3ERP.
        /// </summary>
        private readonly ServeiValidacioArticlesA3Erp serveiValidacioArticles =
            new ServeiValidacioArticlesA3Erp();

        // Ruta completa del fitxer seleccionat per l'usuari.
        private string rutaFitxerSeleccionat = string.Empty;

        private readonly Color colorFons = Color.FromArgb(242, 245, 249);
        private readonly Color colorPrincipal = Color.FromArgb(28, 43, 65);
        private readonly Color colorAccent = Color.FromArgb(24, 132, 150);
        private readonly Color colorTextSecundari = Color.FromArgb(100, 112, 128);
        private readonly Color colorBorde = Color.FromArgb(218, 224, 232);

        private readonly BindingSource fontDades = new BindingSource();
        private readonly List<Button> botonsFiltre = new List<Button>();

        private List<ArticleImportacio> articles = new List<ArticleImportacio>();

        private DataGridView dgvArticles;

        private Label lblFitxer;
        private Label lblDetallFitxer;
        private Label lblEmpresa;
        private Label lblEstatProces;
        private Label lblRegistresMostrats;

        private Label lblTotal;
        private Label lblImportats;
        private Label lblNous;
        private Label lblActualitzacions;
        private Label lblErrors;
        private Label lblSenseCanvis;

        private Button btnSeleccionarFitxer;
        private Button btnAnalitzar;
        private Button btnImportar;

        private ProgressBar prgImportacio;
        /// <summary>
        /// Checkbox de selecció massiva situat a la capçalera de la graella.
        /// </summary>
        private CheckBox chkSeleccionarTots;

        /// <summary>
        /// Evita bucles d'esdeveniments durant els canvis programàtics.
        /// </summary>
        private bool actualitzantSeleccioMassiva;

        /// <summary>
        /// Constructor utilitzat temporalment quan el projecte
        /// s'executa directament des de Visual Studio.
        /// </summary>
        public FrmImportadorArticles()
            : this(ContextImportadorA3Erp.CrearModeDesenvolupament())
        {
        }

        /// <summary>
        /// Constructor principal de l'importador.
        ///
        /// En la versió integrada, el punt d'entrada d'a3ERP
        /// haurà de proporcionar el context abans d'obrir el formulari.
        /// </summary>
        /// <param name="context">
        /// Empresa activa i connexió ja resoltes.
        /// </param>
        public FrmImportadorArticles(
            ContextImportadorA3Erp context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            contextA3Erp = context;

            InitializeComponent();

            IconaAplicacio.Aplicar(this);

            ConfigurarFormulari();
            ConstruirInterficie();
            /*
             * Evitem que la tipografia depengui del procés
             * que allotja el formulari: executable propi o a3ERP.
             */
            AplicarRenderitzatText(this);

            // La graella s'inicia buida fins que l'usuari
            // seleccioni i analitzi un fitxer.
            articles = new List<ArticleImportacio>();

            MostrarArticles(articles);
            ActualitzarResum();

            AplicarEstatVisualImportacio(
                EstatVisualImportacio.Desactivat);

            MostrarContextA3Erp();
        }

        private void ConfigurarFormulari()
        {
            Text = "Importador d’articles - a3ERP";
            StartPosition = FormStartPosition.CenterScreen;
            MinimumSize = new Size(1180, 720);
            Size = new Size(1450, 880);
            WindowState = FormWindowState.Maximized;
            BackColor = colorFons;
            Font = new Font("Segoe UI", 9F);
            AutoScaleMode = AutoScaleMode.Dpi;
            DoubleBuffered = true;
        }

        /// <summary>
        /// Mostra a la capçalera l'empresa rebuda des del context d'a3ERP.
        ///
        /// En mode de desenvolupament encara no hi ha connexió,
        /// per això es manté el missatge provisional.
        /// </summary>
        private void MostrarContextA3Erp()
        {
            if (!contextA3Erp.ConnexioDisponible)
            {
                lblEmpresa.Text =
                    "Pendent de connexió amb a3ERP";

                return;
            }

            if (!string.IsNullOrWhiteSpace(
                contextA3Erp.EmpresaActiva))
            {
                lblEmpresa.Text =
                    contextA3Erp.EmpresaActiva.Trim();

                return;
            }

            if (!string.IsNullOrWhiteSpace(
                contextA3Erp.BaseDadesEmpresa))
            {
                lblEmpresa.Text =
                    contextA3Erp.BaseDadesEmpresa.Trim();

                return;
            }

            lblEmpresa.Text =
                "Empresa activa no identificada";
        }

        private void ConstruirInterficie()
        {
            SuspendLayout();
            Controls.Clear();

            TableLayoutPanel layoutPrincipal = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                BackColor = colorFons,
                ColumnCount = 1,
                RowCount = 7,
                Padding = new Padding(0)
            };

            layoutPrincipal.RowStyles.Add(new RowStyle(SizeType.Absolute, 92F));
            layoutPrincipal.RowStyles.Add(new RowStyle(SizeType.Absolute, 62F));
            layoutPrincipal.RowStyles.Add(new RowStyle(SizeType.Absolute, 115F));
            layoutPrincipal.RowStyles.Add(new RowStyle(SizeType.Absolute, 56F));
            layoutPrincipal.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            layoutPrincipal.RowStyles.Add(new RowStyle(SizeType.Absolute, 100F));
            layoutPrincipal.RowStyles.Add(new RowStyle(SizeType.Absolute, 78F));

            layoutPrincipal.Controls.Add(CrearCapcalera(), 0, 0);
            layoutPrincipal.Controls.Add(CrearPassosProces(), 0, 1);
            layoutPrincipal.Controls.Add(CrearSeccioFitxer(), 0, 2);
            layoutPrincipal.Controls.Add(CrearFiltres(), 0, 3);
            layoutPrincipal.Controls.Add(CrearGraella(), 0, 4);
            layoutPrincipal.Controls.Add(CrearResum(), 0, 5);
            layoutPrincipal.Controls.Add(CrearPeuAccions(), 0, 6);

            Controls.Add(layoutPrincipal);

            ResumeLayout(true);
        }

        private Control CrearCapcalera()
        {
            Panel panel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = colorPrincipal,
                Margin = new Padding(0)
            };

            TableLayoutPanel taula = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 1,

                // Reduïm una mica el padding superior i inferior
                // perquè el bloc del títol quedi més centrat visualment.
                Padding = new Padding(28, 10, 24, 10)
            };

            taula.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 70F));
            taula.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 30F));

            TableLayoutPanel zonaTitols = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 2
            };

            zonaTitols.RowStyles.Add(new RowStyle(SizeType.Percent, 58F));
            zonaTitols.RowStyles.Add(new RowStyle(SizeType.Percent, 42F));

            Label lblTitol = new Label
            {
                Text = "IMPORTADOR D’ARTICLES",
                Dock = DockStyle.Fill,
                Margin = new Padding(0),
                ForeColor = Color.White,
                Font = new Font("Segoe UI Semibold", 20F),

                // En lloc d'enganxar el text a baix,
                // el deixem més equilibrat dins del seu espai.
                TextAlign = ContentAlignment.MiddleLeft
            };

            Label lblSubtitol = new Label
            {
                Text = "Creació i actualització massiva d’articles a a3ERP",
                Dock = DockStyle.Fill,
                Margin = new Padding(0),
                ForeColor = Color.FromArgb(196, 208, 222),
                Font = new Font("Segoe UI", 10F),
                TextAlign = ContentAlignment.TopLeft
            };

            zonaTitols.Controls.Add(lblTitol, 0, 0);
            zonaTitols.Controls.Add(lblSubtitol, 0, 1);

            // Contenidor de la informació de l'empresa activa.
            Panel contenidorEmpresa = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(0, 9, 0, 9)
            };

            // Targeta alineada a la dreta amb prou amplada
            // per mostrar el nom de l'empresa.
            Panel targetaEmpresa = new Panel
            {
                Dock = DockStyle.Right,
                Width = 430,

                // Fem servir el mateix color de la capçalera
                // perquè no sembli una targeta separada.
                BackColor = colorPrincipal
            };

            Label lblEmpresaTitol = new Label
            {
                Text = "EMPRESA ACTIVA",
                Location = new Point(15, 7),
                AutoSize = true,
                ForeColor = Color.FromArgb(150, 170, 194),
                Font = new Font("Segoe UI Semibold", 7.5F)
            };

            lblEmpresa = new Label
            {
                Text = "Pendent de connexió amb a3ERP",
                Location = new Point(15, 22),
                Size = new Size(395, 19),
                AutoEllipsis = true,
                ForeColor = Color.White,
                Font = new Font("Segoe UI Semibold", 9.5F)
            };

            targetaEmpresa.Controls.Add(lblEmpresaTitol);
            targetaEmpresa.Controls.Add(lblEmpresa);
            contenidorEmpresa.Controls.Add(targetaEmpresa);

            taula.Controls.Add(zonaTitols, 0, 0);
            taula.Controls.Add(contenidorEmpresa, 1, 0);

            panel.Controls.Add(taula);
            
            return panel;
        }

        /// <summary>
        /// Construeix la barra visual amb les quatre fases
        /// del procés d'importació.
        /// </summary>
        private Control CrearPassosProces()
        {
            Panel contenidor = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = colorFons,
                Padding = new Padding(20, 10, 20, 6)
            };

            FlowLayoutPanel passos = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = false,
                BackColor = Color.White,
                Padding = new Padding(16, 7, 16, 6)
            };

            /*
             * Netegem les referències per evitar duplicats
             * si la interfície es reconstruís.
             */
            controlsPassosProces.Clear();

            passos.Controls.Add(
                CrearPas(1, "FITXER"));

            passos.Controls.Add(
                CrearSeparadorPas());

            passos.Controls.Add(
                CrearPas(2, "VALIDACIÓ"));

            passos.Controls.Add(
                CrearSeparadorPas());

            passos.Controls.Add(
                CrearPas(3, "REVISIÓ"));

            passos.Controls.Add(
                CrearSeparadorPas());

            passos.Controls.Add(
                CrearPas(4, "IMPORTACIÓ"));

            contenidor.Controls.Add(passos);

            /*
             * En obrir el formulari ens trobem
             * sempre a la fase de selecció del fitxer.
             */
            AplicarPasProces(1);

            return contenidor;
        }

        /// <summary>
        /// Crea un pas visual i conserva les seves etiquetes
        /// perquè posteriorment puguem canviar-ne l'estat.
        /// </summary>
        private Control CrearPas(
            int numero,
            string text)
        {
            Panel panel = new Panel
            {
                Width = 170,
                Height = 36,
                Margin = new Padding(0)
            };

            Label lblNumero = new Label
            {
                Text = numero.ToString(),
                Size = new Size(30, 30),
                Location = new Point(0, 2),
                TextAlign = ContentAlignment.MiddleCenter,
                BackColor = Color.FromArgb(226, 231, 237),
                ForeColor = colorTextSecundari,
                Font = new Font(
                    "Segoe UI Semibold",
                    9F)
            };

            Label lblText = new Label
            {
                Text = text,
                AutoSize = true,
                Location = new Point(40, 10),
                ForeColor = colorTextSecundari,
                Font = new Font(
                    "Segoe UI Semibold",
                    9F)
            };

            panel.Controls.Add(lblNumero);
            panel.Controls.Add(lblText);

            controlsPassosProces[numero] =
                new ControlsPasProces
                {
                    Numero = numero,
                    EtiquetaNumero = lblNumero,
                    EtiquetaText = lblText
                };

            return panel;
        }

        private Control CrearSeparadorPas()
        {
            return new Label
            {
                Text = "›",
                Width = 30,
                Height = 36,
                TextAlign = ContentAlignment.MiddleCenter,
                ForeColor = Color.FromArgb(170, 180, 192),
                Font = new Font("Segoe UI", 17F),
                Margin = new Padding(0)
            };
        }

        private Control CrearSeccioFitxer()
        {
            // Contenidor exterior que manté el marge respecte
            // a la resta de seccions del formulari.
            Panel contenidor = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = colorFons,
                Padding = new Padding(20, 5, 20, 8)
            };

            // Targeta blanca on es mostra la informació
            // del fitxer seleccionat i les accions disponibles.
            Panel targeta = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.White,
                Padding = new Padding(22, 14, 18, 14)
            };

            // La secció es divideix en dues columnes:
            // informació del fitxer i botons d'acció.
            TableLayoutPanel taula = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 1
            };

            taula.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 68F));
            taula.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 32F));

            // Zona esquerra amb el nom i els detalls del fitxer.
            TableLayoutPanel zonaText = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 3
            };

            zonaText.RowStyles.Add(new RowStyle(SizeType.Absolute, 18F));
            zonaText.RowStyles.Add(new RowStyle(SizeType.Absolute, 27F));
            zonaText.RowStyles.Add(new RowStyle(SizeType.Absolute, 24F));

            Label lblTitol = new Label
            {
                Text = "FITXER D’ORIGEN",
                Dock = DockStyle.Fill,
                ForeColor = colorTextSecundari,
                Font = new Font("Segoe UI Semibold", 8F),
                TextAlign = ContentAlignment.MiddleLeft
            };

            lblFitxer = new Label
            {
                Text = "Cap fitxer seleccionat",
                Dock = DockStyle.Fill,
                ForeColor = colorPrincipal,
                Font = new Font("Segoe UI Semibold", 12F),
                TextAlign = ContentAlignment.MiddleLeft
            };

            lblDetallFitxer = new Label
            {
                Text = "Formats compatibles: Excel (.xlsx, .xls) i CSV (.csv)",
                Dock = DockStyle.Fill,
                ForeColor = colorTextSecundari,
                Font = new Font("Segoe UI", 8.5F),
                TextAlign = ContentAlignment.TopLeft,
                AutoEllipsis = true
            };

            zonaText.Controls.Add(lblTitol, 0, 0);
            zonaText.Controls.Add(lblFitxer, 0, 1);
            zonaText.Controls.Add(lblDetallFitxer, 0, 2);

            // Zona dreta amb els botons de selecció i anàlisi.
            FlowLayoutPanel zonaBotons = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.RightToLeft,
                WrapContents = false,
                Padding = new Padding(0, 13, 0, 0)
            };

            btnAnalitzar = CrearBoto(
                "Analitzar fitxer",
                Color.FromArgb(14, 165, 233),
                Color.White,
                145);

            btnAnalitzar.Click += btnAnalitzar_Click;

            // Inicialment no es pot analitzar perquè encara no
            // s'ha seleccionat cap fitxer d'origen.
            AplicarEstatVisualBotoAnalitzar(false);

            btnSeleccionarFitxer = CrearBoto(
                "Seleccionar fitxer",
                Color.White,
                Color.FromArgb(51, 65, 85),
                155);

            btnSeleccionarFitxer.Click += btnSeleccionarFitxer_Click;

            zonaBotons.Controls.Add(btnAnalitzar);
            zonaBotons.Controls.Add(btnSeleccionarFitxer);

            // Muntem la jerarquia final de controls.
            taula.Controls.Add(zonaText, 0, 0);
            taula.Controls.Add(zonaBotons, 1, 0);

            targeta.Controls.Add(taula);
            contenidor.Controls.Add(targeta);

            return contenidor;
        }

        private Control CrearFiltres()
        {
            Panel contenidor = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = colorFons,
                Padding = new Padding(20, 4, 20, 5)
            };

            TableLayoutPanel taula = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 1
            };

            taula.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 75F));
            taula.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25F));

            FlowLayoutPanel filtres = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = false
            };

            AfegirBotoFiltre(filtres, "Tots", null, true);
            AfegirBotoFiltre(filtres, "Nous", EstatImportacio.Nou, false);
            AfegirBotoFiltre(filtres, "Actualitzacions", EstatImportacio.Actualitzacio, false);
            AfegirBotoFiltre(filtres, "Errors", EstatImportacio.Error, false);
            AfegirBotoFiltre(filtres, "Sense canvis", EstatImportacio.SenseCanvis, false);

            lblRegistresMostrats = new Label
            {
                Text = "Mostrant 0 registres",
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleRight,
                ForeColor = colorTextSecundari,
                Font = new Font("Segoe UI", 8.5F)
            };

            taula.Controls.Add(filtres, 0, 0);
            taula.Controls.Add(lblRegistresMostrats, 1, 0);

            contenidor.Controls.Add(taula);

            return contenidor;
        }

        private void AfegirBotoFiltre(
            FlowLayoutPanel contenidor,
            string text,
            EstatImportacio? estat,
            bool actiu)
        {
            Button boto = CrearBotoFiltre(text, actiu);

            boto.Tag = estat;
            boto.Click += delegate
            {
                AplicarFiltre(estat, boto);
            };

            botonsFiltre.Add(boto);
            contenidor.Controls.Add(boto);
        }

        private Button CrearBotoFiltre(string text, bool actiu)
        {
            Button boto = new Button
            {
                Text = text,
                AutoSize = true,
                Height = 34,
                MinimumSize = new Size(75, 34),
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand,
                BackColor = actiu ? colorPrincipal : Color.White,
                ForeColor = actiu ? Color.White : colorTextSecundari,
                Font = new Font("Segoe UI Semibold", 8.5F),
                Margin = new Padding(0, 0, 8, 0),
                Padding = new Padding(10, 0, 10, 0)
            };

            boto.FlatAppearance.BorderSize = actiu ? 0 : 1;
            boto.FlatAppearance.BorderColor = colorBorde;

            return boto;
        }

        private Control CrearGraella()
        {
            Panel contenidor = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = colorFons,
                Padding = new Padding(20, 0, 20, 8)
            };

            dgvArticles = new DataGridView
            {
                Dock = DockStyle.Fill,
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.None,
                AutoGenerateColumns = false,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                AllowUserToResizeRows = false,
                RowHeadersVisible = false,
                MultiSelect = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                EditMode = DataGridViewEditMode.EditOnEnter,
                EnableHeadersVisualStyles = false,
                ColumnHeadersHeight = 42,
                ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing,
                RowTemplate = { Height = 38 },
                GridColor = Color.FromArgb(232, 236, 241),
                DataSource = fontDades
            };

            dgvArticles.ColumnHeadersDefaultCellStyle.BackColor = colorPrincipal;
            dgvArticles.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            dgvArticles.ColumnHeadersDefaultCellStyle.Font =
                new Font("Segoe UI Semibold", 8.5F);

            dgvArticles.ColumnHeadersDefaultCellStyle.Alignment =
                DataGridViewContentAlignment.MiddleLeft;

            dgvArticles.DefaultCellStyle.BackColor = Color.White;
            dgvArticles.DefaultCellStyle.ForeColor = Color.FromArgb(52, 60, 72);
            dgvArticles.DefaultCellStyle.SelectionBackColor = Color.FromArgb(220, 235, 240);
            dgvArticles.DefaultCellStyle.SelectionForeColor = colorPrincipal;
            dgvArticles.DefaultCellStyle.Font = new Font("Segoe UI", 8.5F);
            dgvArticles.DefaultCellStyle.Padding = new Padding(4, 0, 4, 0);

            dgvArticles.AlternatingRowsDefaultCellStyle.BackColor =
                Color.FromArgb(249, 250, 252);

            ActivarDobleBufferGraella(dgvArticles);
            CrearColumnesGraella();

            dgvArticles.DataBindingComplete += dgvArticles_DataBindingComplete;

            dgvArticles.CellFormatting += dgvArticles_CellFormatting;

            contenidor.Controls.Add(dgvArticles);

            return contenidor;
        }

        /// <summary>
        /// Crea les columnes de la graella principal
        /// segons la plantilla actual d'importació d'articles.
        ///
        /// Els camps antics es conserven als models com a reserva,
        /// però no es mostren ni participen en el procés actual.
        /// </summary>
        private void CrearColumnesGraella()
        {
            /*
             * Evitem duplicar columnes en cas que la interfície
             * es reconstruís més d'una vegada.
             */
            dgvArticles.Columns.Clear();

            dgvArticles.Columns.Add(
                new DataGridViewCheckBoxColumn
                {
                    Name = "colSeleccionat",
                    HeaderText = "",
                    DataPropertyName = "Seleccionat",
                    Width = 44,
                    ThreeState = false,
                    Frozen = true
                });

            dgvArticles.Columns.Add(
                new DataGridViewTextBoxColumn
                {
                    Name = "colFila",
                    HeaderText = "FILA",
                    DataPropertyName = "NumeroFila",
                    Width = 58,
                    ReadOnly = true,
                    Frozen = true,
                    DefaultCellStyle =
                    {
                Alignment =
                    DataGridViewContentAlignment.MiddleCenter
                    }
                });

            dgvArticles.Columns.Add(
                new DataGridViewTextBoxColumn
                {
                    Name = "colEstat",
                    HeaderText = "ESTAT",
                    DataPropertyName = "EstatText",
                    Width = 110,
                    ReadOnly = true,
                    Frozen = true
                });

            dgvArticles.Columns.Add(
                new DataGridViewTextBoxColumn
                {
                    Name = "colCodi",
                    HeaderText = "CODI",
                    DataPropertyName = "CodiArticle",
                    Width = 125,
                    ReadOnly = true,
                    Frozen = true
                });

            dgvArticles.Columns.Add(
                new DataGridViewTextBoxColumn
                {
                    Name = "colDescripcio",
                    HeaderText = "DESCRIPCIÓ",
                    DataPropertyName = "Descripcio",
                    AutoSizeMode =
                        DataGridViewAutoSizeColumnMode.Fill,
                    MinimumWidth = 230,
                    ReadOnly = true
                });

            dgvArticles.Columns.Add(
                new DataGridViewTextBoxColumn
                {
                    Name = "colFamilia",
                    HeaderText = "FAMÍLIA",
                    DataPropertyName = "Familia",
                    Width = 95,
                    ReadOnly = true
                });

            dgvArticles.Columns.Add(
                new DataGridViewTextBoxColumn
                {
                    Name = "colUnitats",
                    HeaderText = "UNITATS",
                    DataPropertyName = "Unitats",
                    Width = 90,
                    ReadOnly = true
                });

            dgvArticles.Columns.Add(
                new DataGridViewTextBoxColumn
                {
                    Name = "colTipusUnitat",
                    HeaderText = "TIPUS UNITAT",
                    DataPropertyName = "TipusUnitat",
                    Width = 125,
                    ReadOnly = true
                });

            dgvArticles.Columns.Add(
                new DataGridViewTextBoxColumn
                {
                    Name = "colCodiProveidor",
                    HeaderText = "CODI PROV.",
                    DataPropertyName = "CodiProveidor",
                    Width = 100,
                    ReadOnly = true
                });

            dgvArticles.Columns.Add(
                new DataGridViewTextBoxColumn
                {
                    Name = "colReferenciaProveidor",
                    HeaderText = "REFERÈNCIA PROV.",
                    DataPropertyName = "ReferenciaProveidor",
                    Width = 145,
                    ReadOnly = true
                });

            DataGridViewTextBoxColumn colPreuCompra =
                new DataGridViewTextBoxColumn
                {
                    Name = "colPreuCompra",
                    HeaderText = "PREU COMPRA",
                    DataPropertyName = "PreuCompra",
                    Width = 115,
                    ReadOnly = true
                };

            colPreuCompra.DefaultCellStyle.Format =
                "N2";

            colPreuCompra.DefaultCellStyle.Alignment =
                DataGridViewContentAlignment.MiddleRight;

            dgvArticles.Columns.Add(
                colPreuCompra);

            DataGridViewTextBoxColumn colDescompte1 =
                new DataGridViewTextBoxColumn
                {
                    Name = "colDescompte1",
                    HeaderText = "DTO. 1 %",
                    DataPropertyName = "Descompte1",
                    Width = 85,
                    ReadOnly = true
                };

            colDescompte1.DefaultCellStyle.Format =
                "N4";

            colDescompte1.DefaultCellStyle.Alignment =
                DataGridViewContentAlignment.MiddleRight;

            dgvArticles.Columns.Add(
                colDescompte1);

            DataGridViewTextBoxColumn colDescompte2 =
                new DataGridViewTextBoxColumn
                {
                    Name = "colDescompte2",
                    HeaderText = "DTO. 2 %",
                    DataPropertyName = "Descompte2",
                    Width = 85,
                    ReadOnly = true
                };

            colDescompte2.DefaultCellStyle.Format =
                "N4";

            colDescompte2.DefaultCellStyle.Alignment =
                DataGridViewContentAlignment.MiddleRight;

            dgvArticles.Columns.Add(
                colDescompte2);

            DataGridViewTextBoxColumn colDescompte3 =
                new DataGridViewTextBoxColumn
                {
                    Name = "colDescompte3",
                    HeaderText = "DTO. 3 %",
                    DataPropertyName = "Descompte3",
                    Width = 85,
                    ReadOnly = true
                };

            colDescompte3.DefaultCellStyle.Format =
                "N4";

            colDescompte3.DefaultCellStyle.Alignment =
                DataGridViewContentAlignment.MiddleRight;

            dgvArticles.Columns.Add(
                colDescompte3);

            DataGridViewTextBoxColumn colPreuCost =
                new DataGridViewTextBoxColumn
                {
                    Name = "colPreuCost",
                    HeaderText = "PREU COST",
                    DataPropertyName = "PreuCost",
                    Width = 105,
                    ReadOnly = true
                };

            colPreuCost.DefaultCellStyle.Format =
                "N2";

            colPreuCost.DefaultCellStyle.Alignment =
                DataGridViewContentAlignment.MiddleRight;

            dgvArticles.Columns.Add(
                colPreuCost);

            DataGridViewTextBoxColumn colPreuTransport =
                new DataGridViewTextBoxColumn
                {
                    Name = "colPreuTransport",
                    HeaderText = "TRANSPORT",
                    DataPropertyName = "PreuTransport",
                    Width = 105,
                    ReadOnly = true
                };

            colPreuTransport.DefaultCellStyle.Format =
                "N2";

            colPreuTransport.DefaultCellStyle.Alignment =
                DataGridViewContentAlignment.MiddleRight;

            dgvArticles.Columns.Add(
                colPreuTransport);

            dgvArticles.Columns.Add(
                new DataGridViewTextBoxColumn
                {
                    Name = "colIdFormula",
                    HeaderText = "IDFORMULA",
                    DataPropertyName = "IdFormula",
                    Width = 95,
                    ReadOnly = true,
                    DefaultCellStyle =
                    {
                Alignment =
                    DataGridViewContentAlignment.MiddleRight
                    }
                });

            dgvArticles.Columns.Add(
                new DataGridViewTextBoxColumn
                {
                    Name = "colMissatge",
                    HeaderText = "RESULTAT / INCIDÈNCIA",
                    DataPropertyName = "Missatge",
                    Width = 260,
                    ReadOnly = true
                });

            /*
             * Afegeix el checkbox de selecció massiva
             * a la capçalera de la primera columna.
             */
            ConfigurarCheckboxSeleccionarTots(
                dgvArticles);
        }

        private DataGridViewCheckBoxColumn CrearColumnaBooleana(
            string nom,
            string titol,
            string camp)
        {
            return new DataGridViewCheckBoxColumn
            {
                Name = nom,
                HeaderText = titol,
                DataPropertyName = camp,
                Width = 62,
                ReadOnly = true,
                ThreeState = true,
                TrueValue = true,
                FalseValue = false,
                IndeterminateValue = null
            };
        }

        /// <summary>
        /// Crea les targetes amb el resum dels articles
        /// segons el seu estat dins del procés.
        /// </summary>
        private Control CrearResum()
        {
            Panel contenidor = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = colorFons,
                Padding = new Padding(20, 4, 20, 8)
            };

            TableLayoutPanel taula = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 6,
                RowCount = 1
            };

            /*
             * Repartim les sis targetes uniformement
             * per tota l'amplada disponible.
             */
            float percentatgeColumna =
                100F / 6F;

            for (int i = 0; i < 6; i++)
            {
                taula.ColumnStyles.Add(
                    new ColumnStyle(
                        SizeType.Percent,
                        percentatgeColumna));
            }

            taula.Controls.Add(
                CrearTargetaResum(
                    "TOTAL REGISTRES",
                    out lblTotal),
                0,
                0);

            taula.Controls.Add(
                CrearTargetaResum(
                    "ARTICLES NOUS",
                    out lblNous),
                1,
                0);

            taula.Controls.Add(
                CrearTargetaResum(
                    "ACTUALITZACIONS",
                    out lblActualitzacions),
                2,
                0);

            taula.Controls.Add(
                CrearTargetaResum(
                    "IMPORTATS",
                    out lblImportats),
                3,
                0);

            taula.Controls.Add(
                CrearTargetaResum(
                    "ERRORS",
                    out lblErrors),
                4,
                0);

            taula.Controls.Add(
                CrearTargetaResum(
                    "SENSE CANVIS",
                    out lblSenseCanvis),
                5,
                0);

            contenidor.Controls.Add(taula);

            return contenidor;
        }

        private Panel CrearTargetaResum(string titol, out Label lblValor)
        {
            Panel targeta = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.White,
                Margin = new Padding(0, 0, 10, 0),
                Padding = new Padding(16, 10, 12, 8)
            };

            Label lblTitol = new Label
            {
                Text = titol,
                Dock = DockStyle.Top,
                Height = 22,
                ForeColor = colorTextSecundari,
                Font = new Font("Segoe UI Semibold", 7.5F)
            };

            lblValor = new Label
            {
                Text = "0",
                Dock = DockStyle.Fill,
                ForeColor = colorPrincipal,
                Font = new Font("Segoe UI Semibold", 19F),
                TextAlign = ContentAlignment.MiddleLeft
            };

            targeta.Controls.Add(lblValor);
            targeta.Controls.Add(lblTitol);

            return targeta;
        }

        private Control CrearPeuAccions()
        {
            Panel contenidor = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.White,
                Padding = new Padding(20, 12, 20, 12)
            };

            TableLayoutPanel taula = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 1
            };

            taula.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            taula.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 310F));

            Panel zonaEstat = new Panel
            {
                Dock = DockStyle.Fill,
                Margin = new Padding(0),
                Padding = new Padding(0)
            };

            lblEstatProces = new Label
            {
                Text = "Mode demostració · Encara no s’ha carregat cap fitxer real",
                AutoSize = false,
                Location = new Point(0, 0),
                Size = new Size(620, 25),
                ForeColor = colorTextSecundari,
                Font = new Font("Segoe UI", 9.5F),
                TextAlign = ContentAlignment.MiddleLeft,
                Anchor = AnchorStyles.Left | AnchorStyles.Top
            };

            prgImportacio = new ProgressBar
            {
                Location = new Point(0, 31),
                Size = new Size(560, 12),
                Minimum = 0,
                Maximum = 100,
                Value = 0,
                Style = ProgressBarStyle.Continuous,
                Anchor = AnchorStyles.Left | AnchorStyles.Top
            };

            zonaEstat.Controls.Add(lblEstatProces);
            zonaEstat.Controls.Add(prgImportacio);

            FlowLayoutPanel zonaBotons = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = false
            };

            btnImportar = CrearBoto(
                "Importar seleccionats",
                Color.FromArgb(5, 150, 105),
                Color.White,
                190);

            btnImportar.Click += btnImportar_Click;

            Button btnTancar = CrearBoto(
                "Tancar",
                Color.White,
                Color.FromArgb(51, 65, 85),
                100);

            btnTancar.Click += delegate
            {
                Close();
            };

            zonaBotons.Controls.Add(btnTancar);
            zonaBotons.Controls.Add(btnImportar);

            taula.Controls.Add(zonaEstat, 0, 0);
            taula.Controls.Add(zonaBotons, 1, 0);

            contenidor.Controls.Add(taula);

            return contenidor;
        }

        /// <summary>
        /// Aplica l'aparença corresponent a l'estat actual del procés.
        ///
        /// Centralitzar aquesta lògica evita modificar colors, textos i controls
        /// des de diferents punts del formulari.
        /// </summary>
        /// <param name="estat">Estat visual que s'ha d'aplicar.</param>
        /// <param name="missatge">
        /// Missatge opcional que es mostrarà a la zona inferior.
        /// </param>
        private void AplicarEstatVisualImportacio(
            EstatVisualImportacio estat,
            string missatge = null)
        {
            switch (estat)
            {
                case EstatVisualImportacio.Desactivat:
                    btnImportar.Enabled = false;
                    btnImportar.Text = "Importar seleccionats";
                    btnImportar.BackColor = Color.FromArgb(218, 224, 232);
                    btnImportar.ForeColor = Color.FromArgb(125, 136, 150);
                    btnImportar.Cursor = Cursors.Default;

                    prgImportacio.Style = ProgressBarStyle.Continuous;
                    prgImportacio.Value = 0;

                    lblEstatProces.Text =
                        missatge ??
                        "Selecciona i analitza un fitxer abans d’iniciar la importació.";
                    break;

                case EstatVisualImportacio.Preparat:
                    btnImportar.Enabled = true;
                    btnImportar.Text = "Importar seleccionats";
                    btnImportar.BackColor = Color.FromArgb(5, 150, 105);
                    btnImportar.ForeColor = Color.White;
                    btnImportar.Cursor = Cursors.Hand;

                    prgImportacio.Style = ProgressBarStyle.Continuous;
                    prgImportacio.Value = 0;

                    lblEstatProces.Text =
                        missatge ??
                        "Anàlisi finalitzada. Revisa els registres abans d’importar.";
                    break;

                case EstatVisualImportacio.Processant:
                    btnImportar.Enabled = false;
                    btnImportar.Text = "Important articles...";
                    btnImportar.BackColor = Color.FromArgb(218, 224, 232);
                    btnImportar.ForeColor = Color.FromArgb(125, 136, 150);
                    btnImportar.Cursor = Cursors.WaitCursor;

                    // Fins que disposem del percentatge real, la barra
                    // mostra que el procés està treballant.
                    prgImportacio.Style = ProgressBarStyle.Marquee;

                    lblEstatProces.Text =
                        missatge ??
                        "Importació en curs. No tanquis aquesta finestra.";
                    break;

                case EstatVisualImportacio.Finalitzat:
                    btnImportar.Enabled = false;
                    btnImportar.Text = "Importació finalitzada";
                    btnImportar.BackColor = Color.FromArgb(218, 224, 232);
                    btnImportar.ForeColor = Color.FromArgb(125, 136, 150);
                    btnImportar.Cursor = Cursors.Default;

                    prgImportacio.Style = ProgressBarStyle.Continuous;
                    prgImportacio.Value = 100;

                    lblEstatProces.Text =
                        missatge ??
                        "El procés d’importació ha finalitzat correctament.";
                    break;
            }

            btnImportar.Refresh();
            lblEstatProces.Refresh();
        }


        private Button CrearBoto(
            string text,
            Color colorFonsBoto,
            Color colorText,
            int amplada)
        {
            Button boto = new Button
            {
                Text = text,
                FlatStyle = FlatStyle.Flat,
                UseVisualStyleBackColor = false,
                BackColor = colorFonsBoto,
                ForeColor = colorText,
                Cursor = Cursors.Hand,
                Font = new Font("Segoe UI Semibold", 9.5F),
                Margin = new Padding(6, 0, 0, 0),
                Size = new Size(amplada, 44)
            };

            boto.FlatAppearance.BorderColor =
                Color.FromArgb(203, 213, 225);

            return boto;
        }

        private void MostrarArticles(IEnumerable<ArticleImportacio> registres)
        {
            List<ArticleImportacio> resultat = registres.ToList();

            fontDades.DataSource =
                new BindingList<ArticleImportacio>(resultat);

            lblRegistresMostrats.Text =
                string.Format(
                    "Mostrant {0} de {1} registres",
                    resultat.Count,
                    articles.Count);
        }

        private void dgvArticles_CellFormatting(
            object sender,
            DataGridViewCellFormattingEventArgs e)
        {
            if (e.RowIndex < 0 ||
                e.ColumnIndex < 0 ||
                dgvArticles.Columns[e.ColumnIndex].Name != "colIdFormula")
            {
                return;
            }

            ArticleImportacio article =
                dgvArticles.Rows[e.RowIndex].DataBoundItem
                as ArticleImportacio;

            if (article == null)
            {
                return;
            }

            if (article.IdFormula.HasValue)
            {
                e.Value =
                    article.IdFormula.Value.ToString();

                e.FormattingApplied =
                    true;

                return;
            }

            e.Value =
                string.IsNullOrWhiteSpace(
                    article.IdFormulaOriginal)
                    ? string.Empty
                    : article.IdFormulaOriginal.Trim();

            e.FormattingApplied =
                true;
        }

        private void AplicarFiltre(
            EstatImportacio? estat,
            Button botoSeleccionat)
        {
            foreach (Button boto in botonsFiltre)
            {
                bool actiu = boto == botoSeleccionat;

                boto.BackColor = actiu ? colorPrincipal : Color.White;
                boto.ForeColor = actiu ? Color.White : colorTextSecundari;
                boto.FlatAppearance.BorderSize = actiu ? 0 : 1;
            }

            IEnumerable<ArticleImportacio> resultat = articles;

            if (estat.HasValue)
            {
                resultat = articles.Where(
                    article => article.Estat == estat.Value);
            }

            MostrarArticles(resultat);
        }

        /// <summary>
        /// Actualitza els comptadors del resum segons
        /// l'estat actual de tots els articles.
        /// </summary>
        private void ActualitzarResum()
        {
            lblTotal.Text =
                articles.Count.ToString();

            lblNous.Text =
                articles.Count(
                    article =>
                        article.Estat == EstatImportacio.Nou)
                .ToString();

            lblActualitzacions.Text =
                articles.Count(
                    article =>
                        article.Estat == EstatImportacio.Actualitzacio)
                .ToString();

            lblImportats.Text =
                articles.Count(
                    article =>
                        article.Estat == EstatImportacio.Importat)
                .ToString();

            lblErrors.Text =
                articles.Count(
                    article =>
                        article.Estat == EstatImportacio.Error)
                .ToString();

            lblSenseCanvis.Text =
                articles.Count(
                    article =>
                        article.Estat == EstatImportacio.SenseCanvis)
                .ToString();
        }

        /// <summary>
        /// Actualitza l'estat visual del botó d'anàlisi.
        /// El botó només queda disponible quan s'ha seleccionat un fitxer.
        /// </summary>
        /// <param name="disponible">
        /// Indica si el botó s'ha d'activar o desactivar.
        /// </param>
        private void AplicarEstatVisualBotoAnalitzar(bool disponible)
        {
            btnAnalitzar.Enabled = disponible;

            if (disponible)
            {
                // Fitxer seleccionat: l'anàlisi ja es pot executar.
                btnAnalitzar.BackColor = Color.FromArgb(14, 165, 233);
                btnAnalitzar.ForeColor = Color.White;
                btnAnalitzar.Cursor = Cursors.Hand;
            }
            else
            {
                // Sense fitxer: el botó queda desactivat i visualment atenuat.
                btnAnalitzar.BackColor = Color.FromArgb(218, 224, 232);
                btnAnalitzar.ForeColor = Color.FromArgb(125, 136, 150);
                btnAnalitzar.Cursor = Cursors.Default;
            }

            btnAnalitzar.Refresh();
        }

        /// <summary>
        /// Permet seleccionar un fitxer Excel o CSV.
        /// La selecció no llegeix encara el contingut:
        /// l'usuari haurà de prémer posteriorment «Analitzar fitxer».
        /// </summary>
        private void btnSeleccionarFitxer_Click(
            object sender,
            EventArgs e)
        {
            using (OpenFileDialog dialeg = new OpenFileDialog())
            {
                dialeg.Title = "Seleccionar fitxer d'articles";

                dialeg.Filter =
                    "Fitxers compatibles (*.xlsx;*.xls;*.csv)|*.xlsx;*.xls;*.csv|" +
                    "Fitxers Excel (*.xlsx;*.xls)|*.xlsx;*.xls|" +
                    "Fitxers CSV (*.csv)|*.csv";

                dialeg.Multiselect = false;
                dialeg.CheckFileExists = true;

                if (dialeg.ShowDialog(this) != DialogResult.OK)
                {
                    return;
                }

                FileInfo fitxer = new FileInfo(dialeg.FileName);

                rutaFitxerSeleccionat = fitxer.FullName;

                lblFitxer.Text = fitxer.Name;

                lblDetallFitxer.Text = string.Format(
                    "{0} · {1:N1} KB · {2}",
                    fitxer.Extension.ToUpperInvariant(),
                    fitxer.Length / 1024D,
                    fitxer.DirectoryName);

                // Eliminem qualsevol resultat anterior perquè corresponia
                // a un altre fitxer.
                articles = new List<ArticleImportacio>();

                MostrarArticles(articles);
                ActualitzarResum();

                AplicarEstatVisualBotoAnalitzar(true);

                AplicarEstatVisualImportacio(
                    EstatVisualImportacio.Desactivat,
                    "Fitxer seleccionat. Prem «Analitzar fitxer» " +
                    "per revisar-ne el contingut.");
            }
        }

        /// <summary>
        /// Llegeix el fitxer seleccionat, converteix les files
        /// al model intern i mostra els registres a la graella.
        ///
        /// Quan hi ha una connexió disponible, consulta a3ERP
        /// i classifica els articles com a nous, actualitzacions,
        /// sense canvis o errors.
        ///
        /// També registra al log les fases principals de l'anàlisi,
        /// les incidències detectades i el resum final.
        /// </summary>
        private async void btnAnalitzar_Click(
            object sender,
            EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(rutaFitxerSeleccionat))
            {
                ImportadorArticlesLogger.Advertencia(
                    "S'ha intentat analitzar sense seleccionar cap fitxer.",
                    new Dictionary<string, object>
                    {
                {
                    "Empresa",
                    contextA3Erp?.EmpresaActiva
                    ?? string.Empty
                },
                {
                    "BaseDades",
                    contextA3Erp?.BaseDadesEmpresa
                    ?? string.Empty
                }
                    });

                MessageBox.Show(
                    this,
                    "Selecciona primer un fitxer Excel o CSV.",
                    "Importador d'articles",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);

                AplicarPasProces(1);

                return;
            }

            /*
             * Identificador que permet relacionar totes les entrades
             * generades durant una mateixa anàlisi.
             */
            string identificadorAnalisi =
                Guid.NewGuid()
                    .ToString("N")
                    .Substring(0, 12);

            Stopwatch cronometreAnalisi =
                Stopwatch.StartNew();

            Dictionary<string, object> campsComunsLog =
                new Dictionary<string, object>
                {
            {
                "IdAnalisi",
                identificadorAnalisi
            },
            {
                "Empresa",
                contextA3Erp?.EmpresaActiva
                ?? string.Empty
            },
            {
                "BaseDades",
                contextA3Erp?.BaseDadesEmpresa
                ?? string.Empty
            },
            {
                "Fitxer",
                System.IO.Path.GetFileName(
                    rutaFitxerSeleccionat)
            },
            {
                "ConnexioDisponible",
                contextA3Erp != null &&
                contextA3Erp.ConnexioDisponible
            }
                };

            ImportadorArticlesLogger.Informacio(
                "S'inicia l'anàlisi del fitxer d'articles.",
                campsComunsLog);

            ImportadorArticlesLogger.Debug(
                "Ruta completa del fitxer seleccionat per a l'anàlisi.",
                new Dictionary<string, object>
                {
                    {
                        "IdAnalisi",
                        identificadorAnalisi
                    },
                    {
                        "RutaFitxer",
                        rutaFitxerSeleccionat
                    }
                });

            AplicarEstatVisualBotoAnalitzar(false);
            btnSeleccionarFitxer.Enabled = false;
            UseWaitCursor = true;

            lblEstatProces.Text =
                "Llegint i interpretant el fitxer d'articles...";

            /*
             * La lectura i la validació corresponen
             * al segon pas del procés.
             */
            AplicarPasProces(2);

            try
            {
                /*
                 * Llegim físicament el fitxer Excel o CSV
                 * sense bloquejar la interfície d'usuari.
                 */
                ResultatLecturaFitxer resultatLectura =
                    await Task.Run(
                        delegate
                        {
                            return lectorFitxers.Llegir(
                                rutaFitxerSeleccionat);
                        });

                if (!resultatLectura.Correcte)
                {
                    cronometreAnalisi.Stop();

                    articles =
                        new List<ArticleImportacio>();

                    MostrarArticles(articles);
                    ActualitzarResum();
                    ActualitzarCheckboxSeleccionarTots();

                    AplicarPasProces(1);

                    AplicarEstatVisualImportacio(
                        EstatVisualImportacio.Desactivat,
                        "No s'ha pogut analitzar el fitxer.");

                    Dictionary<string, object> campsErrorLectura =
                        new Dictionary<string, object>(
                            campsComunsLog);

                    campsErrorLectura.Add(
                        "Missatge",
                        resultatLectura.Missatge);

                    campsErrorLectura.Add(
                        "DuradaMs",
                        cronometreAnalisi.ElapsedMilliseconds);

                    ImportadorArticlesLogger.Advertencia(
                        "La lectura del fitxer no ha finalitzat correctament.",
                        campsErrorLectura);

                    MessageBox.Show(
                        this,
                        resultatLectura.Missatge,
                        "Error de lectura",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error);

                    return;
                }

                Dictionary<string, object> campsLectura =
                    new Dictionary<string, object>(
                        campsComunsLog);

                campsLectura.Add(
                    "Full",
                    resultatLectura.NomFull);

                campsLectura.Add(
                    "TotalRegistres",
                    resultatLectura.TotalRegistres);

                ImportadorArticlesLogger.Informacio(
                    "La lectura física del fitxer ha finalitzat correctament.",
                    campsLectura);

                /*
                 * Convertim les files del fitxer al model intern
                 * utilitzat per la graella i la importació.
                 */
                List<ArticleImportacio> articlesLlegits =
                    await Task.Run(
                        delegate
                        {
                            return convertidorArticles.Convertir(
                                resultatLectura.Dades);
                        });

                int errorsConversio =
                    articlesLlegits.Count(
                        article =>
                            article != null &&
                            article.Estat ==
                            EstatImportacio.Error);

                Dictionary<string, object> campsConversio =
                    new Dictionary<string, object>(
                        campsComunsLog);

                campsConversio.Add(
                    "TotalConvertits",
                    articlesLlegits.Count);

                campsConversio.Add(
                    "ErrorsConversio",
                    errorsConversio);

                ImportadorArticlesLogger.Informacio(
                    "Les files del fitxer s'han convertit al model intern.",
                    campsConversio);

                /*
                 * Registrem les incidències detectades durant
                 * la conversió abans de consultar a3ERP.
                 */
                foreach (
                    ArticleImportacio articleError
                    in articlesLlegits.Where(
                        article =>
                            article != null &&
                            article.Estat ==
                            EstatImportacio.Error))
                {
                    ImportadorArticlesLogger.Advertencia(
                        "S'ha detectat una fila incorrecta durant la conversió.",
                        new Dictionary<string, object>
                        {
                    {
                        "IdAnalisi",
                        identificadorAnalisi
                    },
                    {
                        "Fila",
                        articleError.NumeroFila
                    },
                    {
                        "CODART",
                        articleError.CodiArticle
                        ?? string.Empty
                    },
                    {
                        "IDFORMULA",
                        articleError.IdFormula.HasValue
                            ? (object)articleError.IdFormula.Value
                            : articleError.IdFormulaOriginal
                              ?? string.Empty
                    },
                    {
                        "Missatge",
                        articleError.Missatge
                        ?? string.Empty
                    }
                        });
                }

                /*
                 * Quan el formulari s'ha obert des del menú d'a3ERP,
                 * el context conté una connexió validada amb l'empresa activa.
                 */
                if (contextA3Erp.ConnexioDisponible)
                {
                    lblEstatProces.Text =
                        "Validant els articles amb l'empresa activa d'a3ERP...";

                    ImportadorArticlesLogger.Informacio(
                        "S'inicia la validació dels articles contra a3ERP.",
                        new Dictionary<string, object>
                        {
                    {
                        "IdAnalisi",
                        identificadorAnalisi
                    },
                    {
                        "Empresa",
                        contextA3Erp.EmpresaActiva
                    },
                    {
                        "BaseDades",
                        contextA3Erp.BaseDadesEmpresa
                    },
                    {
                        "TotalArticles",
                        articlesLlegits.Count
                    }
                        });

                    await Task.Run(
                        delegate
                        {
                            serveiValidacioArticles.Validar(
                                contextA3Erp.CadenaConnexio,
                                articlesLlegits);
                        });
                }
                else
                {
                    /*
                     * En executar directament l'EXE no disposem
                     * de connexió amb cap empresa d'a3ERP.
                     *
                     * En aquest mode només validem l'estructura
                     * i el contingut bàsic del fitxer.
                     */
                    foreach (ArticleImportacio article in articlesLlegits)
                    {
                        if (article == null)
                        {
                            continue;
                        }

                        if (article.Estat == EstatImportacio.Error)
                        {
                            article.Seleccionat = false;
                            continue;
                        }

                        article.Estat =
                            EstatImportacio.Pendent;

                        article.Seleccionat =
                            false;

                        article.Missatge =
                            "Pendent de validació amb a3ERP.";
                    }

                    ImportadorArticlesLogger.Advertencia(
                        "L'anàlisi s'ha executat sense connexió amb a3ERP.",
                        new Dictionary<string, object>
                        {
                    {
                        "IdAnalisi",
                        identificadorAnalisi
                    },
                    {
                        "TotalArticles",
                        articlesLlegits.Count
                    }
                        });
                }

                articles =
                    articlesLlegits;

                MostrarArticles(articles);
                ActualitzarResum();
                ActualitzarCheckboxSeleccionarTots();

                /*
                 * Des d'a3ERP, la validació ja ha acabat
                 * i l'usuari passa a la fase de revisió.
                 *
                 * En mode EXE independent ens mantenim
                 * al pas de validació perquè no hi ha connexió.
                 */
                AplicarPasProces(
                    contextA3Erp.ConnexioDisponible
                        ? 3
                        : 2);

                lblDetallFitxer.Text =
                    string.Format(
                        "Full: {0} · {1} registres llegits",
                        resultatLectura.NomFull,
                        resultatLectura.TotalRegistres);

                int totalNous =
                    articles.Count(
                        article =>
                            article != null &&
                            article.Estat ==
                            EstatImportacio.Nou);

                int totalActualitzacions =
                    articles.Count(
                        article =>
                            article != null &&
                            article.Estat ==
                            EstatImportacio.Actualitzacio);

                int totalSenseCanvis =
                    articles.Count(
                        article =>
                            article != null &&
                            article.Estat ==
                            EstatImportacio.SenseCanvis);

                int totalErrors =
                    articles.Count(
                        article =>
                            article != null &&
                            article.Estat ==
                            EstatImportacio.Error);

                bool hiHaArticlesImportables =
                    articles.Any(
                        article =>
                            article != null &&
                            article.Seleccionat &&
                            (
                                article.Estat ==
                                EstatImportacio.Nou
                                ||
                                article.Estat ==
                                EstatImportacio.Actualitzacio
                            ));

                /*
                 * Registrem les incidències que hagin aparegut
                 * durant la validació contra a3ERP.
                 */
                foreach (
                    ArticleImportacio articleError
                    in articles.Where(
                        article =>
                            article != null &&
                            article.Estat ==
                            EstatImportacio.Error))
                {
                    Dictionary<string, object> campsIncidencia =
                        new Dictionary<string, object>
                        {
                    {
                        "IdAnalisi",
                        identificadorAnalisi
                    },
                    {
                        "Fila",
                        articleError.NumeroFila
                    },
                    {
                        "CODART",
                        articleError.CodiArticle
                        ?? string.Empty
                    },
                    {
                        "CAR1",
                        articleError.Familia
                        ?? string.Empty
                    },
                    {
                        "IDFORMULA",
                        articleError.IdFormula.HasValue
                            ? (object)articleError.IdFormula.Value
                            : articleError.IdFormulaOriginal
                              ?? string.Empty
                    },
                    {
                        "Missatge",
                        articleError.Missatge
                        ?? string.Empty
                    }
                        };

                    ImportadorArticlesLogger.Advertencia(
                        "L'article presenta una incidència de validació.",
                        campsIncidencia);
                }

                if (contextA3Erp.ConnexioDisponible)
                {
                    /*
                     * La validació contra a3ERP ja s'ha completat.
                     * Activem el botó d'importació només quan hi ha
                     * articles nous o actualitzacions seleccionades.
                     */
                    AplicarEstatVisualImportacio(
                        hiHaArticlesImportables
                            ? EstatVisualImportacio.Preparat
                            : EstatVisualImportacio.Desactivat,
                        string.Format(
                            "Validació finalitzada: {0} nous, " +
                            "{1} actualitzacions, {2} sense canvis i {3} errors.",
                            totalNous,
                            totalActualitzacions,
                            totalSenseCanvis,
                            totalErrors));
                }
                else
                {
                    /*
                     * En mode executable independent no podem consultar
                     * la taula ARTICULO de cap empresa.
                     */
                    AplicarEstatVisualImportacio(
                        EstatVisualImportacio.Desactivat,
                        string.Format(
                            "Lectura finalitzada: {0} registres i {1} errors. " +
                            "Pendent de validar els articles amb a3ERP.",
                            articles.Count,
                            totalErrors));
                }

                cronometreAnalisi.Stop();

                Dictionary<string, object> campsResum =
                    new Dictionary<string, object>(
                        campsComunsLog);

                campsResum.Add(
                    "Full",
                    resultatLectura.NomFull);

                campsResum.Add(
                    "Total",
                    articles.Count);

                campsResum.Add(
                    "Nous",
                    totalNous);

                campsResum.Add(
                    "Actualitzacions",
                    totalActualitzacions);

                campsResum.Add(
                    "SenseCanvis",
                    totalSenseCanvis);

                campsResum.Add(
                    "Errors",
                    totalErrors);

                campsResum.Add(
                    "ImportablesSeleccionats",
                    articles.Count(
                        article =>
                            article != null &&
                            article.Seleccionat &&
                            (
                                article.Estat ==
                                EstatImportacio.Nou
                                ||
                                article.Estat ==
                                EstatImportacio.Actualitzacio
                            )));

                campsResum.Add(
                    "DuradaMs",
                    cronometreAnalisi.ElapsedMilliseconds);

                string missatgeResumAnalisi =
                    contextA3Erp.ConnexioDisponible
                        ? "Ha finalitzat la lectura i validació del fitxer."
                        : "Ha finalitzat la lectura del fitxer sense validació amb a3ERP.";

                if (totalErrors > 0)
                {
                    ImportadorArticlesLogger.Advertencia(
                        missatgeResumAnalisi,
                        campsResum);
                }
                else
                {
                    ImportadorArticlesLogger.Informacio(
                        missatgeResumAnalisi,
                        campsResum);
                }
            }
            catch (Exception ex)
            {
                cronometreAnalisi.Stop();

                articles =
                    new List<ArticleImportacio>();

                MostrarArticles(articles);
                ActualitzarResum();
                ActualitzarCheckboxSeleccionarTots();

                /*
                 * Si l'anàlisi falla, tornem al primer pas
                 * perquè l'usuari pugui revisar o seleccionar el fitxer.
                 */
                AplicarPasProces(1);

                AplicarEstatVisualImportacio(
                    EstatVisualImportacio.Desactivat,
                    "S'ha produït un error durant l'anàlisi.");

                Dictionary<string, object> campsError =
                    new Dictionary<string, object>(
                        campsComunsLog);

                campsError.Add(
                    "DuradaMs",
                    cronometreAnalisi.ElapsedMilliseconds);

                ImportadorArticlesLogger.Error(
                    "No s'ha pogut completar l'anàlisi del fitxer.",
                    ex,
                    campsError);

                MessageBox.Show(
                    this,
                    "No s'ha pogut completar l'anàlisi del fitxer."
                    + Environment.NewLine
                    + Environment.NewLine
                    + ex.Message,
                    "Importador d'articles",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
            finally
            {
                if (cronometreAnalisi.IsRunning)
                {
                    cronometreAnalisi.Stop();
                }

                UseWaitCursor = false;
                btnSeleccionarFitxer.Enabled = true;

                AplicarEstatVisualBotoAnalitzar(true);
            }
        }

        /// <summary>
        /// Importa els articles seleccionats mitjançant a3ERP ActiveX.
        ///
        /// L'operació s'executa al mateix fil STA del formulari.
        /// No s'utilitza Task.Run perquè els objectes COM d'a3ERP
        /// poden dependre del fil on han estat creats.
        /// </summary>
        private void btnImportar_Click(
            object sender,
            EventArgs e)
        {
            if (!contextA3Erp.ConnexioDisponible)
            {
                MessageBox.Show(
                    this,
                    "No hi ha cap connexió disponible amb una empresa d'a3ERP.",
                    "Importador d'articles",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);

                return;
            }

            List<ArticleImportacio> articlesSeleccionats =
                articles
                    .Where(article =>
                        article != null &&
                        article.Seleccionat &&
                        (
                            article.Estat == EstatImportacio.Nou ||
                            article.Estat == EstatImportacio.Actualitzacio
                        ))
                    .ToList();

            if (articlesSeleccionats.Count == 0)
            {
                MessageBox.Show(
                    this,
                    "No hi ha cap article nou o actualització seleccionada.",
                    "Importador d'articles",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);

                return;
            }

            string empresa =
                !string.IsNullOrWhiteSpace(contextA3Erp.EmpresaActiva)
                    ? contextA3Erp.EmpresaActiva
                    : contextA3Erp.BaseDadesEmpresa;

            DialogResult confirmacio =
                MessageBox.Show(
                    this,
                    string.Format(
                        "S'importaran {0} articles a l'empresa {1}."
                        + Environment.NewLine
                        + Environment.NewLine
                        + "Aquesta operació crearà articles nous i modificarà "
                        + "els articles existents seleccionats."
                        + Environment.NewLine
                        + Environment.NewLine
                        + "Vols continuar?",
                        articlesSeleccionats.Count,
                        empresa),
                    "Confirmar importació",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Warning,
                    MessageBoxDefaultButton.Button2);

            if (confirmacio != DialogResult.Yes)
            {
                /*
                 * L'usuari continua a la fase de revisió.
                 */
                AplicarPasProces(3);

                return;
            }

            /*
             * Comença la quarta fase del procés.
             * Els tres passos anteriors queden completats.
             */
            AplicarPasProces(4);

            btnSeleccionarFitxer.Enabled = false;
            AplicarEstatVisualBotoAnalitzar(false);
            UseWaitCursor = true;

            /*
             * La barra sempre treballa en percentatge.
             */
            prgImportacio.Minimum = 0;
            prgImportacio.Maximum = 100;
            prgImportacio.Value = 0;

            AplicarEstatVisualImportacio(
                EstatVisualImportacio.Processant,
                string.Format(
                    "Iniciant la importació de {0} articles...",
                    articlesSeleccionats.Count));

            try
            {
                ProcessadorImportacioArticles processador =
                    new ProcessadorImportacioArticles();

                ResultatImportacioLot resultat =
                    processador.Processar(
                        articles,
                        delegate (
                            int posicio,
                            int total,
                            ArticleImportacio article)
                        {
                            int percentatge =
                                total <= 0
                                    ? 0
                                    : (int)Math.Round(
                                        posicio * 100.0 / total);

                            percentatge =
                                Math.Max(
                                    prgImportacio.Minimum,
                                    Math.Min(
                                        prgImportacio.Maximum,
                                        percentatge));

                            prgImportacio.Value =
                                percentatge;

                            lblEstatProces.Text =
                                string.Format(
                                    "Important article {0} de {1}: {2}",
                                    posicio,
                                    total,
                                    article.CodiArticle);

                            prgImportacio.Refresh();
                            lblEstatProces.Refresh();

                            /*
                             * Permet actualitzar visualment la barra
                             * mentre ActiveX processa el lot.
                             */
                            Application.DoEvents();
                        });

                MostrarArticles(articles);
                ActualitzarResum();
                ActualitzarCheckboxSeleccionarTots();

                string resumFinal =
                    string.Format(
                        "Importació finalitzada: {0} creats, " +
                        "{1} actualitzats i {2} errors.",
                        resultat.TotalCreats,
                        resultat.TotalActualitzats,
                        resultat.TotalErrors);

                AplicarEstatVisualImportacio(
                    EstatVisualImportacio.Finalitzat,
                    resumFinal);

                /*
                 * La fase d'importació ha acabat.
                 * Marquem els quatre passos com a completats,
                 * encara que alguna fila hagi tingut un error controlat.
                 */
                AplicarPasProces(
                    4,
                    procesFinalitzat: true);

                MessageBoxIcon icona =
                    resultat.TeErrors
                        ? MessageBoxIcon.Warning
                        : MessageBoxIcon.Information;

                MessageBox.Show(
                    this,
                    resumFinal,
                    "Resultat de la importació",
                    MessageBoxButtons.OK,
                    icona);
            }
            catch (Exception ex)
            {
                MostrarArticles(articles);
                ActualitzarResum();
                ActualitzarCheckboxSeleccionarTots();

                /*
                 * En cas d'error inesperat tornem a Revisió,
                 * perquè l'usuari pugui comprovar les files.
                 */
                AplicarPasProces(3);

                bool quedenArticlesImportables =
                    articles.Any(article =>
                        article != null &&
                        article.Seleccionat &&
                        (
                            article.Estat == EstatImportacio.Nou ||
                            article.Estat == EstatImportacio.Actualitzacio
                        ));

                AplicarEstatVisualImportacio(
                    quedenArticlesImportables
                        ? EstatVisualImportacio.Preparat
                        : EstatVisualImportacio.Desactivat,
                    "S'ha produït un error inesperat durant la importació.");

                MessageBox.Show(
                    this,
                    "No s'ha pogut completar la importació."
                    + Environment.NewLine
                    + Environment.NewLine
                    + ex.Message,
                    "Importador d'articles",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
            finally
            {
                UseWaitCursor = false;
                btnSeleccionarFitxer.Enabled = true;

                AplicarEstatVisualBotoAnalitzar(true);
            }
        }

        /// <summary>
        /// Aplica un fons neutre a les files de la graella.
        /// Els estats es mostren mitjançant la columna ESTAT i el missatge,
        /// sense utilitzar colors intensos de fons.
        /// </summary>
        private void dgvArticles_DataBindingComplete(
            object sender,
            DataGridViewBindingCompleteEventArgs e)
        {
            foreach (DataGridViewRow fila in dgvArticles.Rows)
            {
                fila.DefaultCellStyle.BackColor =
                    fila.Index % 2 == 0
                        ? Color.White
                        : Color.FromArgb(249, 250, 252);
            }
        }

        private static void ActivarDobleBufferGraella(DataGridView graella)
        {
            typeof(DataGridView).InvokeMember(
                "DoubleBuffered",
                BindingFlags.NonPublic |
                BindingFlags.Instance |
                BindingFlags.SetProperty,
                null,
                graella,
                new object[] { true });
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
                etiqueta.UseCompatibleTextRendering = false;
            }
            else if (control is ButtonBase boto)
            {
                boto.UseCompatibleTextRendering = false;
            }

            foreach (Control controlFill in control.Controls)
            {
                AplicarRenderitzatText(controlFill);
            }
        }

        /// <summary>
        /// Afegeix un checkbox a la capçalera de la columna de selecció.
        /// </summary>
        private void ConfigurarCheckboxSeleccionarTots(
            DataGridView graella)
        {
            if (graella == null)
            {
                throw new ArgumentNullException(nameof(graella));
            }

            if (!graella.Columns.Contains("colSeleccionat"))
            {
                throw new InvalidOperationException(
                    "No existeix la columna colSeleccionat.");
            }

            chkSeleccionarTots = new CheckBox
            {
                AutoSize = false,
                Width = 16,
                Height = 16,
                TabStop = false,
                Cursor = Cursors.Hand,
                BackColor = graella.ColumnHeadersDefaultCellStyle.BackColor
            };

            chkSeleccionarTots.Click +=
                chkSeleccionarTots_Click;

            graella.Controls.Add(
                chkSeleccionarTots);

            graella.Scroll +=
                dgvArticles_RecolocarSelectorCapcalera;

            graella.SizeChanged +=
                dgvArticles_RecolocarSelectorCapcalera;

            graella.ColumnWidthChanged +=
                dgvArticles_ColumnWidthChanged;

            graella.DataBindingComplete +=
                dgvArticles_DataBindingCompleteSelector;

            graella.CurrentCellDirtyStateChanged +=
                dgvArticles_CurrentCellDirtyStateChangedSelector;

            graella.CellValueChanged +=
                dgvArticles_CellValueChangedSelector;

            PosicionarCheckboxSeleccionarTots();
            ActualitzarCheckboxSeleccionarTots();
        }

        /// <summary>
        /// Marca o desmarca tots els articles importables.
        /// </summary>
        private void chkSeleccionarTots_Click(
            object sender,
            EventArgs e)
        {
            if (actualitzantSeleccioMassiva ||
                articles == null)
            {
                return;
            }

            dgvArticles.EndEdit();

            bool seleccionar =
                !TotsElsArticlesImportablesEstanSeleccionats();

            actualitzantSeleccioMassiva = true;

            try
            {
                foreach (ArticleImportacio article in articles)
                {
                    if (article == null)
                    {
                        continue;
                    }

                    article.Seleccionat =
                        EsArticleSeleccionable(article) &&
                        seleccionar;
                }

                foreach (DataGridViewRow fila in dgvArticles.Rows)
                {
                    ArticleImportacio article =
                        fila.DataBoundItem as ArticleImportacio;

                    if (article == null)
                    {
                        continue;
                    }

                    fila.Cells["colSeleccionat"].Value =
                        article.Seleccionat;
                }
            }
            finally
            {
                actualitzantSeleccioMassiva = false;
            }

            dgvArticles.Refresh();

            ActualitzarCheckboxSeleccionarTots();
            ActualitzarEstatImportacioSegonsSeleccio();
        }

        /// <summary>
        /// Indica si una fila pot ser seleccionada per importar.
        /// </summary>
        private static bool EsArticleSeleccionable(
            ArticleImportacio article)
        {
            if (article == null)
            {
                return false;
            }

            return
                article.Estat == EstatImportacio.Nou ||
                article.Estat == EstatImportacio.Actualitzacio;
        }

        /// <summary>
        /// Indica si tots els articles importables estan seleccionats.
        /// </summary>
        private bool TotsElsArticlesImportablesEstanSeleccionats()
        {
            if (articles == null)
            {
                return false;
            }

            List<ArticleImportacio> importables =
                articles
                    .Where(EsArticleSeleccionable)
                    .ToList();

            return
                importables.Count > 0 &&
                importables.All(
                    article => article.Seleccionat);
        }

        /// <summary>
        /// Actualitza el checkbox de la capçalera segons les files.
        /// </summary>
        private void ActualitzarCheckboxSeleccionarTots()
        {
            if (chkSeleccionarTots == null)
            {
                return;
            }

            bool hiHaImportables =
                articles != null &&
                articles.Any(EsArticleSeleccionable);

            actualitzantSeleccioMassiva = true;

            try
            {
                chkSeleccionarTots.Enabled =
                    hiHaImportables;

                chkSeleccionarTots.Checked =
                    hiHaImportables &&
                    TotsElsArticlesImportablesEstanSeleccionats();
            }
            finally
            {
                actualitzantSeleccioMassiva = false;
            }
        }

        /// <summary>
        /// Centra el checkbox sobre la capçalera de colSeleccionat.
        /// </summary>
        private void PosicionarCheckboxSeleccionarTots()
        {
            if (chkSeleccionarTots == null ||
                dgvArticles == null ||
                !dgvArticles.Columns.Contains("colSeleccionat"))
            {
                return;
            }

            int indexColumna =
                dgvArticles.Columns["colSeleccionat"].Index;

            Rectangle rectangle =
                dgvArticles.GetCellDisplayRectangle(
                    indexColumna,
                    -1,
                    true);

            if (rectangle.Width <= 0 ||
                rectangle.Height <= 0)
            {
                chkSeleccionarTots.Visible = false;
                return;
            }

            chkSeleccionarTots.Visible = true;

            chkSeleccionarTots.Location =
                new Point(
                    rectangle.Left +
                    (rectangle.Width - chkSeleccionarTots.Width) / 2,
                    rectangle.Top +
                    (rectangle.Height - chkSeleccionarTots.Height) / 2);

            chkSeleccionarTots.BringToFront();
        }

        /// <summary>
        /// Actualitza el botó d'importació segons la selecció actual.
        /// </summary>
        private void ActualitzarEstatImportacioSegonsSeleccio()
        {
            int totalSeleccionats =
                articles == null
                    ? 0
                    : articles.Count(
                        article =>
                            EsArticleSeleccionable(article) &&
                            article.Seleccionat);

            if (totalSeleccionats > 0)
            {
                AplicarEstatVisualImportacio(
                    EstatVisualImportacio.Preparat,
                    string.Format(
                        "{0} articles seleccionats per importar.",
                        totalSeleccionats));
            }
            else
            {
                AplicarEstatVisualImportacio(
                    EstatVisualImportacio.Desactivat,
                    "No hi ha cap article seleccionat per importar.");
            }
        }

        private void dgvArticles_CurrentCellDirtyStateChangedSelector(
            object sender,
            EventArgs e)
        {
            if (dgvArticles.CurrentCell == null ||
                dgvArticles.CurrentCell.OwningColumn.Name !=
                "colSeleccionat" ||
                !dgvArticles.IsCurrentCellDirty)
            {
                return;
            }

            dgvArticles.CommitEdit(
                DataGridViewDataErrorContexts.Commit);
        }

        private void dgvArticles_CellValueChangedSelector(
            object sender,
            DataGridViewCellEventArgs e)
        {
            if (actualitzantSeleccioMassiva ||
                e.RowIndex < 0 ||
                e.ColumnIndex < 0 ||
                dgvArticles.Columns[e.ColumnIndex].Name !=
                "colSeleccionat")
            {
                return;
            }

            DataGridViewRow fila =
                dgvArticles.Rows[e.RowIndex];

            ArticleImportacio article =
                fila.DataBoundItem as ArticleImportacio;

            if (article == null)
            {
                return;
            }

            bool seleccionat =
                fila.Cells["colSeleccionat"].Value is bool valor &&
                valor;

            if (!EsArticleSeleccionable(article))
            {
                seleccionat = false;

                actualitzantSeleccioMassiva = true;

                try
                {
                    fila.Cells["colSeleccionat"].Value = false;
                }
                finally
                {
                    actualitzantSeleccioMassiva = false;
                }
            }

            article.Seleccionat =
                seleccionat;

            ActualitzarCheckboxSeleccionarTots();
            ActualitzarEstatImportacioSegonsSeleccio();
        }

        private void dgvArticles_DataBindingCompleteSelector(
            object sender,
            DataGridViewBindingCompleteEventArgs e)
        {
            PosicionarCheckboxSeleccionarTots();
            ActualitzarCheckboxSeleccionarTots();
        }

        private void dgvArticles_RecolocarSelectorCapcalera(
            object sender,
            EventArgs e)
        {
            PosicionarCheckboxSeleccionarTots();
        }

        private void dgvArticles_ColumnWidthChanged(
            object sender,
            DataGridViewColumnEventArgs e)
        {
            PosicionarCheckboxSeleccionarTots();
        }

        /// <summary>
        /// Situa la barra en una fase determinada.
        ///
        /// Els passos anteriors queden completats,
        /// el pas indicat queda actiu i els posteriors pendents.
        /// </summary>
        private void AplicarPasProces(
            int pasActiu,
            bool procesFinalitzat = false)
        {
            foreach (
                KeyValuePair<int, ControlsPasProces> element
                in controlsPassosProces)
            {
                int numeroPas = element.Key;

                EstatVisualPasProces estat;

                if (procesFinalitzat)
                {
                    estat =
                        EstatVisualPasProces.Completat;
                }
                else if (numeroPas < pasActiu)
                {
                    estat =
                        EstatVisualPasProces.Completat;
                }
                else if (numeroPas == pasActiu)
                {
                    estat =
                        EstatVisualPasProces.Actiu;
                }
                else
                {
                    estat =
                        EstatVisualPasProces.Pendent;
                }

                AplicarEstatVisualPas(
                    element.Value,
                    estat);
            }
        }

        /// <summary>
        /// Aplica colors, text i tipografia segons
        /// l'estat actual del pas.
        /// </summary>
        private void AplicarEstatVisualPas(
            ControlsPasProces controls,
            EstatVisualPasProces estat)
        {
            if (controls == null ||
                controls.EtiquetaNumero == null ||
                controls.EtiquetaText == null)
            {
                return;
            }

            switch (estat)
            {
                case EstatVisualPasProces.Actiu:
                    controls.EtiquetaNumero.Text =
                        controls.Numero.ToString();

                    controls.EtiquetaNumero.BackColor =
                        colorAccent;

                    controls.EtiquetaNumero.ForeColor =
                        Color.White;

                    controls.EtiquetaText.ForeColor =
                        colorPrincipal;

                    controls.EtiquetaText.Font =
                        new Font(
                            "Segoe UI Semibold",
                            9F);

                    break;

                case EstatVisualPasProces.Completat:
                    controls.EtiquetaNumero.Text =
                        "✓";

                    controls.EtiquetaNumero.BackColor =
                        colorPrincipal;

                    controls.EtiquetaNumero.ForeColor =
                        Color.White;

                    controls.EtiquetaText.ForeColor =
                        colorPrincipal;

                    controls.EtiquetaText.Font =
                        new Font(
                            "Segoe UI Semibold",
                            9F);

                    break;

                default:
                    controls.EtiquetaNumero.Text =
                        controls.Numero.ToString();

                    controls.EtiquetaNumero.BackColor =
                        Color.FromArgb(226, 231, 237);

                    controls.EtiquetaNumero.ForeColor =
                        colorTextSecundari;

                    controls.EtiquetaText.ForeColor =
                        colorTextSecundari;

                    controls.EtiquetaText.Font =
                        new Font(
                            "Segoe UI Semibold",
                            9F);

                    break;
            }
        }
    }
}
