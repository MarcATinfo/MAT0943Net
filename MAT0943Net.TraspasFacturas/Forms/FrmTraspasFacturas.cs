using MAT0943Net.TraspasFacturas.Infrastructure.Logging;
using MAT0943Net.TraspasFacturas.Infrastructure.UI;
using MAT0943Net.TraspasFacturas.Models;
using MAT0943Net.TraspasFacturas.Services;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Windows.Forms;

namespace MAT0943Net.TraspasFacturas.Forms
{
    public sealed class FrmTraspasFacturas : Form
    {
        private const string TextOrigenNoInformat =
            "NO INFORMADA";
        private readonly string _baseDatosOrigen;
        private readonly ConsultaFacturasOrigenService _consultaService =
            new ConsultaFacturasOrigenService();

        private readonly ConsultaEmpresasActiveXService _consultaEmpresasService =
             new ConsultaEmpresasActiveXService();
        private readonly ServeiValidacioFacturaVendaDestino
                    _serveiValidacioDesti =
                        new ServeiValidacioFacturaVendaDestino();
        private readonly ServeiTraspasFacturesActiveX
            _serveiTraspasActiveX =
                new ServeiTraspasFacturesActiveX();
        private readonly ServeiVincleTraspasFactures
            _serveiVincleTraspas =
                new ServeiVincleTraspasFactures();
        private Button _btnNetejarCerca;
        private int _pasActual = 1;
        private string _empresaDestiSeleccionada =
                    string.Empty;
        private Control _contingutPasFactures;
        private Panel _hostContingut;
        private Panel _hostPassos;
        private ComboBox _cmbEmpresaDesti;
        private Label _lblMissatgeDesti;
        private Button _btnEnrere;
        private List<string> _empresesDesti =
                    new List<string>();
        private DataGridView _dgvFactures;
        private DateTimePicker _dtpDataDesDe;
        private DateTimePicker _dtpDataFins;
        private TextBox _txtFiltreText;
        private ComboBox _cmbTipusFactura;
        private ComboBox _cmbEstatTraspas;
        private Label _lblResumGraella;
        private Label _lblEstat;
        private Button _btnSeguent;
        private Button _btnCercar;
        private List<FacturaOrigenDto> _factures =
            new List<FacturaOrigenDto>();
        private Control _contingutPasTraspas;
        private DataGridView _dgvResumTraspas;
        private Label _lblOrigenResum;
        private Label _lblDestiResum;
        private Label _lblFacturesResum;
        private Label _lblTotalResum;

        public FrmTraspasFacturas(
            string baseDatosOrigen)
        {
            _baseDatosOrigen =
                baseDatosOrigen ?? string.Empty;

            InicialitzarComponents();

            Shown += FrmTraspasFacturas_Shown;
        }

        private void InicialitzarComponents()
        {
            SuspendLayout();

            Text = "Traspàs de factures · a3ERP";

            Assembly assembly =
                Assembly.GetExecutingAssembly();

            using (var stream =
                   assembly.GetManifestResourceStream(
                       "MAT0943Net.TraspasFacturas.Resources.at.ico"))
            {
                if (stream != null)
                {
                    Icon = new Icon(stream);
                }
            }

            StartPosition = FormStartPosition.CenterScreen;
            AutoScaleMode = AutoScaleMode.Dpi;
            AutoScaleDimensions = new SizeF(96F, 96F);
            BackColor = TraspasFacturasTheme.Fons;
            Font = TraspasFacturasTheme.FontBase(9F);
            ClientSize = new Size(1450, 880);
            MinimumSize = new Size(1180, 720);
            WindowState = FormWindowState.Maximized;
            DoubleBuffered = true;

            Controls.Add(
                CrearLayoutPrincipal());

            ResumeLayout(true);
        }

        private Control CrearLayoutPrincipal()
        {
            TableLayoutPanel layout =
                new TableLayoutPanel
                {
                    BackColor = TraspasFacturasTheme.Fons,
                    ColumnCount = 1,
                    Dock = DockStyle.Fill,
                    Padding = new Padding(0),
                    RowCount = 4
                };

            layout.ColumnStyles.Add(
                new ColumnStyle(SizeType.Percent, 100F));
            layout.RowStyles.Add(
                new RowStyle(SizeType.Absolute, 96F));
            layout.RowStyles.Add(
                new RowStyle(SizeType.Absolute, 68F));
            layout.RowStyles.Add(
                new RowStyle(SizeType.Percent, 100F));
            layout.RowStyles.Add(
                new RowStyle(SizeType.Absolute, 78F));
            layout.Controls.Add(CrearCapcalera(), 0, 0);
            layout.Controls.Add(CrearHostPassos(), 0, 1);
            layout.Controls.Add(CrearHostContingut(), 0, 2);
            layout.Controls.Add(CrearPeu(), 0, 3);

            return layout;
        }

        private Control CrearCapcalera()
        {
            Panel panel =
                new Panel
                {
                    BackColor = TraspasFacturasTheme.Capcalera,
                    Dock = DockStyle.Fill
                };

            TableLayoutPanel taula =
                new TableLayoutPanel
                {
                    ColumnCount = 2,
                    Dock = DockStyle.Fill,
                    Padding = new Padding(28, 10, 24, 10),
                    RowCount = 1
                };

            taula.ColumnStyles.Add(
                new ColumnStyle(SizeType.Percent, 70F));
            taula.ColumnStyles.Add(
                new ColumnStyle(SizeType.Percent, 30F));

            TableLayoutPanel zonaTitols =
                new TableLayoutPanel
                {
                    ColumnCount = 1,
                    Dock = DockStyle.Fill,
                    RowCount = 2
                };

            zonaTitols.RowStyles.Add(
                new RowStyle(SizeType.Percent, 58F));
            zonaTitols.RowStyles.Add(
                new RowStyle(SizeType.Percent, 42F));

            Label lblTitol =
                new Label
                {
                    Dock = DockStyle.Fill,
                    Font = TraspasFacturasTheme.FontSemibold(22F),
                    ForeColor = Color.White,
                    Margin = new Padding(0),
                    Text = "TRASPÀS DE FACTURES",
                    TextAlign = ContentAlignment.MiddleLeft
                };

            Label lblSubtitol =
                new Label
                {
                    Dock = DockStyle.Fill,
                    Font = TraspasFacturasTheme.FontBase(10F),
                    ForeColor =
                        TraspasFacturasTheme.TextCapcaleraSecundari,
                    Margin = new Padding(0),
                    Text =
                        "Selecciona les factures i traspassa-les a una altra empresa d'a3ERP.",
                    TextAlign = ContentAlignment.TopLeft
                };

            zonaTitols.Controls.Add(lblTitol, 0, 0);
            zonaTitols.Controls.Add(lblSubtitol, 0, 1);

            Panel contenidorEmpresa =
                new Panel
                {
                    Dock = DockStyle.Fill,
                    Padding = new Padding(0, 9, 0, 9)
                };

            Panel targetaEmpresa =
                new Panel
                {
                    BackColor = TraspasFacturasTheme.Capcalera,
                    Dock = DockStyle.Right,
                    Width = 430
                };

            Label lblEmpresaTitol =
                new Label
                {
                    AutoSize = true,
                    Font = TraspasFacturasTheme.FontSemibold(7.5F),
                    ForeColor =
                        TraspasFacturasTheme.TextEtiquetaCapcalera,
                    Location = new Point(15, 7),
                    Text = "EMPRESA ORIGEN"
                };

            Label lblEmpresa =
                new Label
                {
                    AutoEllipsis = true,
                    Font = TraspasFacturasTheme.FontSemibold(9.5F),
                    ForeColor = Color.White,
                    Location = new Point(15, 22),
                    Size = new Size(395, 19),
                    Text = ObtenirTextOrigen()
                };

            targetaEmpresa.Controls.Add(lblEmpresaTitol);
            targetaEmpresa.Controls.Add(lblEmpresa);
            contenidorEmpresa.Controls.Add(targetaEmpresa);

            taula.Controls.Add(zonaTitols, 0, 0);
            taula.Controls.Add(contenidorEmpresa, 1, 0);
            panel.Controls.Add(taula);

            return panel;
        }

        private Control CrearPassos()
        {
            Panel contenidor =
                new Panel
                {
                    BackColor = Color.White,
                    Dock = DockStyle.Fill,
                    Padding = new Padding(24, 8, 24, 8)
                };

            TableLayoutPanel passos =
                new TableLayoutPanel
                {
                    ColumnCount = 3,
                    Dock = DockStyle.Fill,
                    RowCount = 1
                };

            passos.ColumnStyles.Add(
                new ColumnStyle(SizeType.Percent, 33.333F));
            passos.ColumnStyles.Add(
                new ColumnStyle(SizeType.Percent, 33.333F));
            passos.ColumnStyles.Add(
                new ColumnStyle(SizeType.Percent, 33.334F));
            passos.RowStyles.Add(
                new RowStyle(SizeType.Percent, 100F));

            passos.Controls.Add(
                CrearPas(
                    "1",
                    "FACTURES",
                    "Selecciona les factures",
                    _pasActual == 1),
                0,
                0);

            passos.Controls.Add(
                CrearPas(
                    "2",
                    "DESTÍ",
                    "Selecciona l'empresa",
                    _pasActual == 2),
                1,
                0);

            passos.Controls.Add(
                CrearPas(
                    "3",
                    "TRASPÀS",
                    "Revisa i confirma",
                    _pasActual == 3),
                2,
                0);

            contenidor.Controls.Add(passos);

            return contenidor;
        }

        private void ActualitzarPassos()
        {
            if (_hostPassos == null)
            {
                return;
            }

            _hostPassos.Controls.Clear();
            _hostPassos.Controls.Add(CrearPassos());
        }

        private static Control CrearPas(
            string numero,
            string titol,
            string subtitol,
            bool actiu)
        {
            Panel panel =
                new Panel
                {
                    BackColor = Color.White,
                    Dock = DockStyle.Fill,
                    Margin = new Padding(6, 0, 6, 0)
                };

            Panel indicador =
                new Panel
                {
                    BackColor =
                        actiu
                            ? TraspasFacturasTheme.Accent
                            : TraspasFacturasTheme.FonsPasInactiu,
                    Location = new Point(8, 7),
                    Size = new Size(42, 42)
                };

            Label lblNumero =
                new Label
                {
                    Dock = DockStyle.Fill,
                    Font = TraspasFacturasTheme.FontSemibold(12F),
                    ForeColor =
                        actiu ? Color.White : TraspasFacturasTheme.TextPeu,
                    Text = numero,
                    TextAlign = ContentAlignment.MiddleCenter
                };

            Label lblTitol =
                new Label
                {
                    AutoSize = true,
                    Font = TraspasFacturasTheme.FontSemibold(10F),
                    ForeColor =
                        actiu
                            ? TraspasFacturasTheme.TextPrincipal
                            : TraspasFacturasTheme.TextSecundari,
                    Location = new Point(61, 8),
                    Text = titol
                };

            Label lblSubtitol =
                new Label
                {
                    AutoSize = true,
                    Font = TraspasFacturasTheme.FontBase(8.5F),
                    ForeColor = TraspasFacturasTheme.TextSecundari,
                    Location = new Point(61, 29),
                    Text = subtitol
                };

            indicador.Controls.Add(lblNumero);
            panel.Controls.Add(indicador);
            panel.Controls.Add(lblTitol);
            panel.Controls.Add(lblSubtitol);

            return panel;
        }

        private Control CrearContingutPrincipal()
        {
            TableLayoutPanel contingut =
                new TableLayoutPanel
                {
                    BackColor = TraspasFacturasTheme.Fons,
                    ColumnCount = 1,
                    Dock = DockStyle.Fill,
                    Padding = new Padding(18, 14, 18, 12),
                    RowCount = 3
                };

            contingut.ColumnStyles.Add(
                new ColumnStyle(SizeType.Percent, 100F));
            contingut.RowStyles.Add(
                new RowStyle(SizeType.Absolute, 118F));
            contingut.RowStyles.Add(
                new RowStyle(SizeType.Percent, 100F));
            contingut.RowStyles.Add(
                new RowStyle(SizeType.Absolute, 34F));

            contingut.Controls.Add(CrearBlocFiltres(), 0, 0);
            contingut.Controls.Add(CrearBlocFactures(), 0, 1);
            contingut.Controls.Add(CrearResumGraella(), 0, 2);

            return contingut;
        }

        private Control CrearBlocFiltres()
        {
            Panel contenidor =
                new Panel
                {
                    BackColor = TraspasFacturasTheme.Fons,
                    Dock = DockStyle.Fill,
                    Padding = new Padding(0, 0, 0, 12)
                };

            Panel targeta =
                new Panel
                {
                    BackColor = Color.White,
                    Dock = DockStyle.Fill,
                    Padding = new Padding(22, 14, 18, 14)
                };

            TableLayoutPanel taula =
                new TableLayoutPanel
                {
                    ColumnCount = 7,
                    Dock = DockStyle.Fill,
                    RowCount = 2
                };

            taula.ColumnStyles.Add(
                new ColumnStyle(SizeType.Absolute, 180F));

            taula.ColumnStyles.Add(
                new ColumnStyle(SizeType.Absolute, 180F));

            taula.ColumnStyles.Add(
                new ColumnStyle(SizeType.Percent, 100F));

            taula.ColumnStyles.Add(
                new ColumnStyle(SizeType.Absolute, 160F));

            taula.ColumnStyles.Add(
                new ColumnStyle(SizeType.Absolute, 170F));

            taula.ColumnStyles.Add(
                new ColumnStyle(SizeType.Absolute, 150F));

            taula.ColumnStyles.Add(
                new ColumnStyle(SizeType.Absolute, 18F));

            taula.RowStyles.Add(
                new RowStyle(SizeType.Absolute, 24F));

            taula.RowStyles.Add(
                new RowStyle(SizeType.Percent, 100F));

            Label titol =
                new Label
                {
                    Dock = DockStyle.Fill,
                    Font = TraspasFacturasTheme.FontSemibold(8F),
                    ForeColor = TraspasFacturasTheme.TextSecundari,
                    Text = "FILTRES",
                    TextAlign = ContentAlignment.MiddleLeft
                };

            taula.Controls.Add(titol, 0, 0);
            taula.SetColumnSpan(titol, 6);

            _dtpDataDesDe =
                CrearSelectorData();

            _dtpDataFins =
                CrearSelectorData();

            _txtFiltreText =
                CrearCaixaText();

            _cmbTipusFactura =
                CrearComboFiltre();

            _cmbTipusFactura.Items.AddRange(
                new object[]
                {
                    "Tots",
                    "Normal",
                    "Rectificativa"
                });

            _cmbTipusFactura.SelectedIndex =
                0;

            _cmbEstatTraspas =
                CrearComboFiltre();

            _cmbEstatTraspas.Items.AddRange(
                new object[]
                {
                    "Totes",
                    "Pendents",
                    "Traspassades"
                });

            /*
             * Jo deixaria "Pendents" per defecte.
             * És el que té més sentit operativament:
             * l'usuari entra per traspassar factures noves.
             */
            _cmbEstatTraspas.SelectedIndex =
                1;

            taula.Controls.Add(
                CrearCampFiltre(
                    "Data des de",
                    _dtpDataDesDe),
                0,
                1);

            taula.Controls.Add(
                CrearCampFiltre(
                    "Data fins",
                    _dtpDataFins),
                1,
                1);

            taula.Controls.Add(
                CrearCampCerca(),
                2,
                1);

            taula.Controls.Add(
                CrearCampFiltre(
                    "Tipus",
                    _cmbTipusFactura),
                3,
                1);

            taula.Controls.Add(
                CrearCampFiltre(
                    "Estat",
                    _cmbEstatTraspas),
                4,
                1);

            _btnCercar =
                new Button();

            TraspasFacturasTheme.ConfigurarBoto(
                _btnCercar,
                "Cercar",
                TraspasFacturasTheme.AccioPrimaria,
                Color.White,
                130);

            _btnCercar.Dock =
                DockStyle.Top;

            _btnCercar.Click +=
                BotoCercar_Click;

            AcceptButton =
                _btnCercar;

            taula.Controls.Add(
                _btnCercar,
                5,
                1);

            targeta.Controls.Add(taula);
            contenidor.Controls.Add(targeta);

            return contenidor;
        }

        private static DateTimePicker CrearSelectorData()
        {
            return new DateTimePicker
            {
                Dock = DockStyle.Top,
                Font = TraspasFacturasTheme.FontBase(9.5F),
                Format = DateTimePickerFormat.Custom,
                CustomFormat = "dd/MM/yyyy",
                ShowCheckBox = true,
                Checked = false,
                Height = 26
            };
        }

        private static TextBox CrearCaixaText()
        {
            return new TextBox
            {
                BorderStyle = BorderStyle.FixedSingle,
                Dock = DockStyle.Top,
                Font = TraspasFacturasTheme.FontBase(9.5F),
                Height = 26
            };
        }

        private static ComboBox CrearComboFiltre()
        {
            return new ComboBox
            {
                Dock = DockStyle.Top,
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = TraspasFacturasTheme.FontBase(9.5F),
                Height = 26
            };
        }

        private Control CrearCampCerca()
        {
            Panel panel =
                new Panel
                {
                    Dock = DockStyle.Fill,
                    Margin = new Padding(0, 0, 12, 0),
                    Padding = new Padding(0)
                };

            Label label =
                new Label
                {
                    Dock = DockStyle.Top,
                    Font = TraspasFacturasTheme.FontSemibold(8.5F),
                    ForeColor = TraspasFacturasTheme.TextPeu,
                    Height = 23,
                    Text = "Client / factura / codi",
                    TextAlign = ContentAlignment.MiddleLeft
                };

            Panel zonaText =
                new Panel
                {
                    Dock = DockStyle.Top,
                    Height = 26,
                    Padding = new Padding(0),
                    Margin = new Padding(0)
                };

            /*
             * TextBox normal.
             * Manté exactament la seva vora nativa.
             */
            _txtFiltreText.BorderStyle =
                BorderStyle.FixedSingle;

            _txtFiltreText.Dock =
                DockStyle.Fill;

            _txtFiltreText.Margin =
                new Padding(0);

            /*
             * La X és una Label superposada damunt
             * del TextBox, no un control al costat.
             */
            Label lblNetejar =
                new Label
                {
                    Text = "×",
                    AutoSize = false,
                    Size = new Size(20, 20),
                    TextAlign = ContentAlignment.MiddleCenter,
                    BackColor = Color.White,
                    ForeColor = Color.FromArgb(80, 80, 80),
                    Font = TraspasFacturasTheme.FontSemibold(10F),
                    Cursor = Cursors.Hand,
                    Visible = false,
                    Anchor = AnchorStyles.Top | AnchorStyles.Right
                };

            zonaText.Controls.Add(
                _txtFiltreText);

            zonaText.Controls.Add(
                lblNetejar);

            Action posicionarCreu =
                () =>
                {
                    lblNetejar.Location =
                        new Point(
                            zonaText.ClientSize.Width -
                            lblNetejar.Width -
                            3,
                            3);

                    lblNetejar.BringToFront();
                };

            zonaText.Resize +=
                (sender, e) =>
                {
                    posicionarCreu();
                };

            lblNetejar.Click +=
                (sender, e) =>
                {
                    _txtFiltreText.Clear();
                    _txtFiltreText.Focus();
                };

            _txtFiltreText.TextChanged +=
                (sender, e) =>
                {
                    lblNetejar.Visible =
                        !string.IsNullOrWhiteSpace(
                            _txtFiltreText.Text);

                    if (lblNetejar.Visible)
                    {
                        lblNetejar.BringToFront();
                    }
                };

            panel.Controls.Add(
                zonaText);

            panel.Controls.Add(
                label);

            posicionarCreu();

            return panel;
        }

        private static Control CrearCampFiltre(
            string etiqueta,
            Control control)
        {
            Panel panel =
                new Panel
                {
                    Dock = DockStyle.Fill,
                    Margin = new Padding(0, 0, 12, 0),
                    Padding = new Padding(0)
                };

            Label label =
                new Label
                {
                    Dock = DockStyle.Top,
                    Font = TraspasFacturasTheme.FontSemibold(8.5F),
                    ForeColor = TraspasFacturasTheme.TextPeu,
                    Height = 23,
                    Text = etiqueta,
                    TextAlign = ContentAlignment.MiddleLeft
                };

            panel.Controls.Add(control);
            panel.Controls.Add(label);

            return panel;
        }

        private Control CrearBlocFactures()
        {
            Panel panel =
                new Panel
                {
                    BackColor = Color.White,
                    BorderStyle = BorderStyle.FixedSingle,
                    Dock = DockStyle.Fill,
                    Padding = new Padding(0)
                };

            Panel capcalera =
                CrearCapcaleraFactures();

            _dgvFactures =
                new DataGridView
                {
                    Dock = DockStyle.Fill
                };

            ConfigurarGraella();
            CrearColumnesGraella();
            ConnectarEsdevenimentsGraella();

            panel.Controls.Add(_dgvFactures);
            panel.Controls.Add(capcalera);

            return panel;
        }

        private static Panel CrearCapcaleraFactures()
        {
            Panel capcalera =
                new Panel
                {
                    BackColor = Color.White,
                    Dock = DockStyle.Top,
                    Height = 84,
                    Padding = new Padding(18, 13, 18, 8)
                };

            Label lblTitol =
                new Label
                {
                    AutoSize = true,
                    Font = TraspasFacturasTheme.FontSemibold(15F),
                    ForeColor = TraspasFacturasTheme.Capcalera,
                    Location = new Point(17, 10),
                    Text = "Factures"
                };

            Label lblSubtitol =
                new Label
                {
                    AutoSize = true,
                    ForeColor = TraspasFacturasTheme.TextSecundari,
                    Location = new Point(20, 43),
                    Text = "Selecciona les factures que vols traspassar."
                };

            capcalera.Controls.Add(lblTitol);
            capcalera.Controls.Add(lblSubtitol);

            return capcalera;
        }

        private void CapcaleraSeleccio_CheckedChanged(
             object sender,
             EventArgs e)
        {
            DataGridViewCheckBoxHeaderCell capcalera =
                sender as DataGridViewCheckBoxHeaderCell;

            if (capcalera == null)
            {
                return;
            }

            bool seleccionar =
                capcalera.Checked;

            /*
             * Tanquem qualsevol edició que pugui haver
             * quedat activa a la cel·la actual.
             */
            if (_dgvFactures.IsCurrentCellDirty)
            {
                _dgvFactures.CommitEdit(
                    DataGridViewDataErrorContexts.Commit);
            }

            _dgvFactures.EndEdit();

            foreach (DataGridViewRow fila in _dgvFactures.Rows)
            {
                if (!fila.Visible)
                {
                    continue;
                }

                FacturaOrigenDto factura =
                    fila.DataBoundItem as FacturaOrigenDto;

                if (factura == null)
                {
                    continue;
                }

                if (factura.Traspassada)
                {
                    factura.Seleccionada =
                        false;

                    fila.Cells["colSeleccionada"].Value =
                        false;

                    continue;
                }

                factura.Seleccionada =
                    seleccionar;

                /*
                 * Actualitzem també explícitament la cel·la.
                 * Això evita que la cel·la activa mantingui
                 * visualment el valor anterior.
                 */
                fila.Cells["colSeleccionada"].Value =
                    seleccionar;
            }

            _dgvFactures.EndEdit();
            _dgvFactures.Refresh();

            ActualitzarResumSeleccio();
        }

        private void ConfigurarGraella()
        {
            _dgvFactures.AllowUserToAddRows = false;
            _dgvFactures.AllowUserToDeleteRows = false;
            _dgvFactures.AllowUserToResizeRows = false;
            _dgvFactures.AutoGenerateColumns = false;
            _dgvFactures.BackgroundColor = Color.White;
            _dgvFactures.BorderStyle = BorderStyle.None;
            _dgvFactures.CellBorderStyle =
                DataGridViewCellBorderStyle.SingleHorizontal;
            _dgvFactures.ColumnHeadersBorderStyle =
                DataGridViewHeaderBorderStyle.None;
            _dgvFactures.ColumnHeadersHeight = 42;
            _dgvFactures.ColumnHeadersHeightSizeMode =
                DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            _dgvFactures.EnableHeadersVisualStyles = false;
            _dgvFactures.GridColor =
                TraspasFacturasTheme.BordeGraella;
            _dgvFactures.MultiSelect = false;
            _dgvFactures.ReadOnly = false;
            _dgvFactures.RowHeadersVisible = false;
            _dgvFactures.RowTemplate.Height = 38;
            _dgvFactures.SelectionMode =
                DataGridViewSelectionMode.FullRowSelect;

            _dgvFactures.ColumnHeadersDefaultCellStyle.BackColor =
                TraspasFacturasTheme.Principal;
            _dgvFactures.ColumnHeadersDefaultCellStyle.ForeColor =
                Color.White;
            _dgvFactures.ColumnHeadersDefaultCellStyle.Font =
                TraspasFacturasTheme.FontSemibold(8.5F);
            _dgvFactures.ColumnHeadersDefaultCellStyle.Alignment =
                DataGridViewContentAlignment.MiddleLeft;
            _dgvFactures.ColumnHeadersDefaultCellStyle.SelectionBackColor =
                TraspasFacturasTheme.Principal;
            _dgvFactures.ColumnHeadersDefaultCellStyle.SelectionForeColor =
                Color.White;

            _dgvFactures.DefaultCellStyle.BackColor = Color.White;
            _dgvFactures.DefaultCellStyle.ForeColor =
                Color.FromArgb(52, 60, 72);
            _dgvFactures.DefaultCellStyle.SelectionBackColor =
                TraspasFacturasTheme.SeleccioGraella;
            _dgvFactures.DefaultCellStyle.SelectionForeColor =
                TraspasFacturasTheme.Principal;
            _dgvFactures.DefaultCellStyle.Font =
                TraspasFacturasTheme.FontBase(8.5F);
            _dgvFactures.DefaultCellStyle.Padding =
                new Padding(4, 0, 4, 0);
            _dgvFactures.AlternatingRowsDefaultCellStyle.BackColor =
                Color.FromArgb(249, 250, 252);
        }

        private void CrearColumnesGraella()
        {
            _dgvFactures.Columns.Clear();

            DataGridViewCheckBoxHeaderCell capcaleraSeleccio =
                new DataGridViewCheckBoxHeaderCell();

            capcaleraSeleccio.CheckedChanged +=
                CapcaleraSeleccio_CheckedChanged;

            DataGridViewCheckBoxColumn columnaSeleccio =
                new DataGridViewCheckBoxColumn
                {
                    HeaderText = string.Empty,
                    HeaderCell = capcaleraSeleccio,
                    Name = "colSeleccionada",
                    DataPropertyName = "Seleccionada",
                    ReadOnly = false,
                    SortMode = DataGridViewColumnSortMode.NotSortable,
                    Width = 42
                };

            _dgvFactures.Columns.Add(
                columnaSeleccio);

            _dgvFactures.Columns.Add(
                CrearColumnaText(
                    "colFactura",
                    "Factura",
                    "Factura",
                    110));

            _dgvFactures.Columns.Add(
                CrearColumnaText(
                    "colData",
                    "Data",
                    "FechaTexto",
                    95));

            _dgvFactures.Columns.Add(
                CrearColumnaText(
                    "colCodiClient",
                    "Codi client",
                    "CodCli",
                    110));

            DataGridViewTextBoxColumn columnaClient =
                CrearColumnaText(
                    "colClient",
                    "Client",
                    "NomCli",
                    240);

            columnaClient.AutoSizeMode =
                DataGridViewAutoSizeColumnMode.Fill;

            _dgvFactures.Columns.Add(
                columnaClient);

            _dgvFactures.Columns.Add(
                CrearColumnaDecimal(
                    "colBase",
                    "Base",
                    "Base",
                    95));

            _dgvFactures.Columns.Add(
                CrearColumnaDecimal(
                    "colIva",
                    "IVA",
                    "Iva",
                    85));

            _dgvFactures.Columns.Add(
                CrearColumnaDecimal(
                    "colTotal",
                    "Total",
                    "Total",
                    105));

            _dgvFactures.Columns.Add(
                CrearColumnaText(
                    "colTipus",
                    "Tipus",
                    "Tipo",
                    140));

            _dgvFactures.Columns.Add(
                CrearColumnaText(
                    "colEstat",
                    "Estat",
                    "Estat",
                    110));
        }

        private static DataGridViewTextBoxColumn CrearColumnaText(
            string nom,
            string titol,
            string propietat,
            int amplada)
        {
            return new DataGridViewTextBoxColumn
            {
                DataPropertyName = propietat,
                HeaderText = titol,
                Name = nom,
                ReadOnly = true,
                SortMode = DataGridViewColumnSortMode.NotSortable,
                Width = amplada
            };
        }

        private static DataGridViewTextBoxColumn CrearColumnaDecimal(
            string nom,
            string titol,
            string propietat,
            int amplada)
        {
            DataGridViewTextBoxColumn columna =
                CrearColumnaText(nom, titol, propietat, amplada);

            columna.DefaultCellStyle.Alignment =
                DataGridViewContentAlignment.MiddleRight;
            columna.DefaultCellStyle.Format = "N2";

            return columna;
        }

        private Control CrearResumGraella()
        {
            Panel panel =
                new Panel
                {
                    BackColor = TraspasFacturasTheme.Fons,
                    Dock = DockStyle.Fill,
                    Padding = new Padding(2, 8, 2, 0)
                };

            _lblResumGraella =
                new Label
                {
                    Dock = DockStyle.Fill,
                    Font = TraspasFacturasTheme.FontSemibold(9F),
                    ForeColor = TraspasFacturasTheme.Accent,
                    Text = "0 factures carregades · Seleccionades: 0",
                    TextAlign = ContentAlignment.MiddleLeft
                };

            panel.Controls.Add(_lblResumGraella);

            return panel;
        }

        private Control CrearHostPassos()
        {
            _hostPassos = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.White
            };

            _hostPassos.Controls.Add(CrearPassos());

            return _hostPassos;
        }

        private Control CrearHostContingut()
        {
            _hostContingut = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = TraspasFacturasTheme.Fons
            };

            _contingutPasFactures = CrearContingutPrincipal();

            _hostContingut.Controls.Add(_contingutPasFactures);

            return _hostContingut;
        }

        private Control CrearPeu()
        {
            Panel panel =
                new Panel
                {
                    BackColor = Color.White,
                    Dock = DockStyle.Fill,
                    Padding = new Padding(20, 12, 20, 12)
                };

            _lblEstat =
                new Label
                {
                    AutoSize = false,
                    Font = TraspasFacturasTheme.FontBase(9.5F),
                    ForeColor = TraspasFacturasTheme.TextPeu,
                    Location = new Point(24, 13),
                    Size = new Size(720, 25),
                    Text =
                        "Preparat per seleccionar factures. Encara no hi ha cap connexió ni procés de traspàs configurat.",
                    TextAlign = ContentAlignment.MiddleLeft
                };

            _btnSeguent =
                new Button();

            TraspasFacturasTheme.ConfigurarBoto(
                _btnSeguent,
                "Següent",
                TraspasFacturasTheme.AccioConfirmacio,
                Color.White,
                120);

            _btnSeguent.Dock = DockStyle.Right;
            _btnSeguent.Click += BotoSeguent_Click;
            ActualitzarEstatBotoSeguent(0);

            _btnEnrere = new Button();

            TraspasFacturasTheme.ConfigurarBoto(
                _btnEnrere,
                "Enrere",
                Color.White,
                Color.FromArgb(51, 65, 85),
                110);

            _btnEnrere.Dock = DockStyle.Right;
            _btnEnrere.Visible = false;
            _btnEnrere.Click += BotoEnrere_Click;

            Button btnTancar =
                new Button();

            TraspasFacturasTheme.ConfigurarBoto(
                btnTancar,
                "Tancar",
                Color.White,
                Color.FromArgb(51, 65, 85),
                110);

            btnTancar.Dock = DockStyle.Right;
            btnTancar.Click += BotoTancar_Click;

            panel.Controls.Add(_lblEstat);
            panel.Controls.Add(_btnEnrere);
            panel.Controls.Add(btnTancar);
            panel.Controls.Add(_btnSeguent);

            return panel;
        }

        private void ConnectarEsdevenimentsGraella()
        {
            _dgvFactures.CurrentCellDirtyStateChanged +=
                DgvFactures_CurrentCellDirtyStateChanged;

            _dgvFactures.CellValueChanged +=
                DgvFactures_CellValueChanged;

            _dgvFactures.CellBeginEdit +=
                DgvFactures_CellBeginEdit;

            _dgvFactures.CellFormatting +=
                DgvFactures_CellFormatting;

            _dgvFactures.CellPainting +=
                DgvFactures_CellPainting;
        }

        private void FrmTraspasFacturas_Shown(
            object sender,
            EventArgs e)
        {
            CarregarFactures();
        }

        private void CarregarFactures()
        {
            try
            {
                if (string.IsNullOrWhiteSpace(_baseDatosOrigen))
                {
                    MostrarErrorControlat(
                        "No s'ha informat --baseDatosOrigen. " +
                        "El formulari queda obert, però no es poden consultar factures.");
                    return;
                }

                ActualitzarEstat(
                    "Consultant les darreres factures de l'empresa origen...");

                DateTime? dataDesDe =
                    _dtpDataDesDe != null && _dtpDataDesDe.Checked
                        ? (DateTime?)_dtpDataDesDe.Value.Date
                        : null;

                DateTime? dataFins =
                    _dtpDataFins != null && _dtpDataFins.Checked
                        ? (DateTime?)_dtpDataFins.Value.Date
                        : null;

                string text =
                    _txtFiltreText?.Text ?? string.Empty;

                string tipus =
                    _cmbTipusFactura?.SelectedItem?.ToString()
                    ?? "Tots";

                string estat =
                    _cmbEstatTraspas?.SelectedItem?.ToString()
                    ?? "Totes";

                _factures =
                    _consultaService.Consultar(
                        _baseDatosOrigen,
                        dataDesDe,
                        dataFins,
                        text,
                        tipus,
                        estat);

                _dgvFactures.DataSource = null;
                _dgvFactures.DataSource = _factures;

                ActualitzarResumSeleccio();
                ActualitzarEstat(
                    "Les factures marcades com a «Traspassada» no es poden tornar a seleccionar.");
            }
            catch (Exception ex)
            {
                _factures =
                    new List<FacturaOrigenDto>();

                _dgvFactures.DataSource = null;
                ActualitzarResumSeleccio();
                MostrarErrorControlat(
                    "No s'han pogut carregar les factures.\n\n" +
                    ex.Message);
            }
        }

        private void MostrarErrorControlat(
            string missatge)
        {
            ActualitzarEstat(missatge);

            MessageBox.Show(
                this,
                missatge,
                "Traspàs de factures",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning);
        }

        private void ActualitzarResumSeleccio()
        {
            int seleccionades =
                ComptarSeleccionades();

            if (_lblResumGraella != null)
            {
                _lblResumGraella.Text =
                    _factures.Count +
                    " factures carregades · Seleccionades: " +
                    seleccionades;
            }

            ActualitzarEstatBotoSeguent(seleccionades);
        }

        private int ComptarSeleccionades()
        {
            int total =
                0;

            foreach (FacturaOrigenDto factura in _factures)
            {
                if (factura.Seleccionada)
                {
                    total++;
                }
            }

            return total;
        }

        private void ActualitzarEstatBotoSeguent(
            int seleccionades)
        {
            if (_btnSeguent == null)
            {
                return;
            }

            bool habilitat =
                seleccionades > 0;

            _btnSeguent.Enabled =
                habilitat;
            _btnSeguent.BackColor =
                habilitat
                    ? TraspasFacturasTheme.AccioConfirmacio
                    : TraspasFacturasTheme.FonsBotoDeshabilitat;
            _btnSeguent.ForeColor =
                habilitat
                    ? Color.White
                    : TraspasFacturasTheme.TextBotoDeshabilitat;
        }

        private void ActualitzarEstat(
            string text)
        {
            if (_lblEstat != null)
            {
                _lblEstat.Text =
                    text ?? string.Empty;
            }
        }

        private string ObtenirTextOrigen()
        {
            return string.IsNullOrWhiteSpace(_baseDatosOrigen)
                ? TextOrigenNoInformat
                : _baseDatosOrigen.Trim();
        }

        private void BotoTancar_Click(
            object sender,
            EventArgs e)
        {
            Close();
        }

        private void BotoCercar_Click(
            object sender,
            EventArgs e)
        {
            CarregarFactures();
        }

        private void BotoEnrere_Click(
            object sender,
            EventArgs e)
        {
            if (_pasActual == 3)
            {
                _pasActual = 2;

                ActualitzarPassos();

                _hostContingut.Controls.Clear();
                _hostContingut.Controls.Add(
                    CrearContingutDesti());

                CarregarEmpresesDesti();

                if (_cmbEmpresaDesti.Items.Contains(
                    _empresaDestiSeleccionada))
                {
                    _cmbEmpresaDesti.SelectedItem =
                        _empresaDestiSeleccionada;
                }

                _btnSeguent.Text =
                    "Següent";

                ActualitzarEstat(
                    "Selecciona l'empresa de destinació.");

                return;
            }

            if (_pasActual == 2)
            {
                _pasActual = 1;

                ActualitzarPassos();

                _hostContingut.Controls.Clear();
                _hostContingut.Controls.Add(
                    _contingutPasFactures);

                _btnEnrere.Visible = false;

                _btnSeguent.Text =
                    "Següent";

                _btnSeguent.Width =
                    120;

                ActualitzarResumSeleccio();

                ActualitzarEstat(
                    "Les factures marcades com a «Traspassada» no es poden tornar a seleccionar.");
            }
        }

        private void BotoSeguent_Click(
            object sender,
            EventArgs e)
        {
            if (_pasActual == 1)
            {
                if (ComptarSeleccionades() == 0)
                {
                    return;
                }

                AnarAlPasDesti();

                return;
            }

            if (_pasActual == 2)
            {
                if (_cmbEmpresaDesti == null ||
                    _cmbEmpresaDesti.SelectedItem == null)
                {
                    return;
                }

                _empresaDestiSeleccionada =
                    _cmbEmpresaDesti.SelectedItem.ToString();

                AnarAlPasTraspas();

                return;
            }

            if (_pasActual == 3)
            {
                try
                {
                    _btnSeguent.Enabled =
                        false;

                    ActualitzarEstat(
                        "Validant factures i dades mestres a l'empresa de destinació...");

                    List<FacturaOrigenDto> seleccionades =
                        new List<FacturaOrigenDto>();

                    foreach (FacturaOrigenDto factura in _factures)
                    {
                        if (factura.Seleccionada)
                        {
                            seleccionades.Add(
                                factura);
                        }
                    }

                    if (seleccionades.Count == 0)
                    {
                        MessageBox.Show(
                            this,
                            "No hi ha cap factura seleccionada.",
                            "Traspàs de factures",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Information);

                        return;
                    }

                    // -------------------------------------------------
                    // 1. VALIDACIÓ DE MESTRES A DESTÍ
                    // -------------------------------------------------

                    ResultadoValidacionTraspasoDto resultadoValidacion =
                        _serveiValidacioDesti.Validar(
                            _baseDatosOrigen,
                            _empresaDestiSeleccionada,
                            seleccionades);

                    if (!resultadoValidacion.EsCorrecto)
                    {
                        var linies =
                            new List<string>();

                        linies.Add(
                            "S'han detectat incidències abans del traspàs.");

                        linies.Add(
                            string.Empty);

                        linies.Add(
                            "Empresa destí: " +
                            resultadoValidacion.EmpresaDestino);

                        linies.Add(
                            "Base de dades destí: " +
                            resultadoValidacion.BaseDatosDestino);

                        linies.Add(
                            string.Empty);

                        int limit =
                            Math.Min(
                                resultadoValidacion.Incidencias.Count,
                                30);

                        for (int i = 0;
                             i < limit;
                             i++)
                        {
                            IncidenciaValidacionDto incidencia =
                                resultadoValidacion.Incidencias[i];

                            string text =
                                incidencia.Factura +
                                " · " +
                                incidencia.Tipo;

                            if (!string.IsNullOrWhiteSpace(
                                    incidencia.Codigo))
                            {
                                text +=
                                    " [" +
                                    incidencia.Codigo +
                                    "]";
                            }

                            text +=
                                Environment.NewLine +
                                incidencia.Mensaje;

                            linies.Add(text);
                            linies.Add(string.Empty);
                        }

                        if (resultadoValidacion.Incidencias.Count >
                            limit)
                        {
                            linies.Add(
                                "... i " +
                                (
                                    resultadoValidacion.Incidencias.Count -
                                    limit
                                ) +
                                " incidències més.");
                        }

                        MessageBox.Show(
                            this,
                            string.Join(
                                Environment.NewLine,
                                linies),
                            "Validació del traspàs",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Warning);

                        ActualitzarEstat(
                            "El traspàs no es pot iniciar perquè hi ha incidències de validació.");

                        return;
                    }

                    // -------------------------------------------------
                    // 2. VALIDACIÓ ANTI-DUPLICAT DE TOT EL LOT
                    //    IMPORTANT: encara no crea res.
                    // -------------------------------------------------

                    foreach (FacturaOrigenDto factura in seleccionades)
                    {
                        try
                        {
                            _serveiVincleTraspas
                                .ValidarDisponibleParaTraspaso(
                                    _baseDatosOrigen,
                                    factura.IdFacv);
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show(
                                this,
                                "No es pot iniciar el traspàs." +
                                Environment.NewLine +
                                Environment.NewLine +
                                "Factura: " +
                                factura.Factura +
                                Environment.NewLine +
                                ex.Message,
                                "Traspàs de factures",
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Warning);

                            ActualitzarEstat(
                                "El lot conté una factura que no es pot traspassar.");

                            return;
                        }
                    }

                    // -------------------------------------------------
                    // 3. CONFIRMACIÓ DE TOT EL LOT
                    // -------------------------------------------------

                    decimal importTotal =
                        0m;

                    foreach (FacturaOrigenDto factura in seleccionades)
                    {
                        importTotal +=
                            factura.Total;
                    }

                    DialogResult confirmacio =
                        MessageBox.Show(
                            this,
                            "La validació és correcta." +
                            Environment.NewLine +
                            Environment.NewLine +
                            "Empresa destí: " +
                            _empresaDestiSeleccionada +
                            Environment.NewLine +
                            "Factures: " +
                            seleccionades.Count +
                            Environment.NewLine +
                            "Import total: " +
                            importTotal.ToString("N2") +
                            " €" +
                            Environment.NewLine +
                            Environment.NewLine +
                            "Es crearan les factures amb data " +
                            DateTime.Today.ToString("dd/MM/yyyy") +
                            "." +
                            Environment.NewLine +
                            Environment.NewLine +
                            "Vols continuar?",
                            "Confirmar traspàs",
                            MessageBoxButtons.YesNo,
                            MessageBoxIcon.Warning);

                    if (confirmacio != DialogResult.Yes)
                    {
                        ActualitzarEstat(
                            "Traspàs cancel·lat per l'usuari.");

                        return;
                    }

                    TraspasFacturasLogger.Informacio(
                        "S'inicia el traspàs d'un conjunt de factures.",
                        new Dictionary<string, object>
                        {
                            {
                                "BaseDadesOrigen",
                                _baseDatosOrigen
                            },
                            {
                                "EmpresaDesti",
                                _empresaDestiSeleccionada
                            },
                            {
                                "BaseDadesDesti",
                                resultadoValidacion.BaseDatosDestino
                            },
                            {
                                "Factures",
                                seleccionades.Count
                            },
                            {
                                "ImportTotal",
                                importTotal
                            }
                        });

                    // -------------------------------------------------
                    // 4. TRASPÀS REAL
                    // -------------------------------------------------

                    int creades =
                        0;

                    var resultats =
                        new List<string>();

                    foreach (FacturaOrigenDto factura in seleccionades)
                    {
                        ActualitzarEstat(
                            "Traspassant factura " +
                            factura.Factura +
                            " (" +
                            (creades + 1) +
                            " de " +
                            seleccionades.Count +
                            ")...");

                        Application.DoEvents();

                        ResultadoTraspasFacturaDto traspas =
                            _serveiTraspasActiveX.CrearFacturaVenda(
                                _baseDatosOrigen,
                                _empresaDestiSeleccionada,
                                resultadoValidacion.BaseDatosDestino,
                                factura);

                        if (!traspas.Correcto)
                        {
                            string missatgeError =
                                "El traspàs s'ha aturat." +
                                Environment.NewLine +
                                Environment.NewLine +
                                "Factura amb error: " +
                                factura.Factura +
                                Environment.NewLine +
                                traspas.Error +
                                Environment.NewLine +
                                Environment.NewLine +
                                "Factures creades abans de l'error: " +
                                creades +
                                " de " +
                                seleccionades.Count +
                                ".";

                            TraspasFacturasLogger.Advertencia(
                                "El traspàs del conjunt de factures s'ha aturat abans de completar-se.",
                                new Dictionary<string, object>
                                {
                                    {
                                        "FacturaError",
                                        factura.Factura
                                    },
                                    {
                                        "IDFACVOrigen",
                                        factura.IdFacv
                                    },
                                    {
                                        "FacturesPrevistes",
                                        seleccionades.Count
                                    },
                                    {
                                        "FacturesCreades",
                                        creades
                                    },
                                    {
                                        "EmpresaDesti",
                                        _empresaDestiSeleccionada
                                    },
                                    {
                                        "Error",
                                        traspas.Error
                                    }
                                });

                            MessageBox.Show(
                                this,
                                missatgeError,
                                "Error en el traspàs",
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Warning);

                            ActualitzarEstat(
                                "Traspàs aturat per un error a la factura " +
                                factura.Factura +
                                ".");

                            return;
                        }

                        creades++;

                        resultats.Add(
                            factura.Factura +
                            " → " +
                            traspas.Serie.Trim() +
                            "-" +
                            traspas.NumDoc);
                    }

                    // -------------------------------------------------
                    // 5. LOT COMPLETAT
                    // -------------------------------------------------

                    string resum =
                        "Traspàs completat correctament." +
                        Environment.NewLine +
                        Environment.NewLine +
                        "Empresa destí: " +
                        _empresaDestiSeleccionada +
                        Environment.NewLine +
                        "Factures creades: " +
                        creades +
                        Environment.NewLine +
                        Environment.NewLine +
                        string.Join(
                            Environment.NewLine,
                            resultats);

                    TraspasFacturasLogger.Informacio(
                        "El traspàs del conjunt de factures ha finalitzat correctament.",
                        new Dictionary<string, object>
                        {
                            {
                                "BaseDadesOrigen",
                                _baseDatosOrigen
                            },
                            {
                                "EmpresaDesti",
                                _empresaDestiSeleccionada
                            },
                            {
                                "BaseDadesDesti",
                                resultadoValidacion.BaseDatosDestino
                            },
                            {
                                "FacturesCreades",
                                creades
                            },
                            {
                                "ImportTotal",
                                importTotal
                            }
                        });

                    MessageBox.Show(
                        this,
                        resum,
                        "Traspàs de factures",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);

                    /*
                     * Actualitzem els DTO que ja té vinculats
                     * la graella del PAS 1.
                     *
                     * No cal tornar a consultar SQL:
                     * quan l'usuari torni al PAS 1 aquestes
                     * factures ja apareixeran com Traspassada.
                     */
                    foreach (FacturaOrigenDto factura in seleccionades)
                    {
                        factura.Traspassada =
                            true;

                        factura.Seleccionada =
                            false;
                    }

                    _dgvFactures.Refresh();

                    ActualitzarEstat(
                        creades +
                        " factures traspassades correctament a " +
                        _empresaDestiSeleccionada +
                        ".");
                }
                catch (Exception ex)
                {
                    MostrarErrorControlat(
                        "No s'ha pogut completar el traspàs." +
                        Environment.NewLine +
                        Environment.NewLine +
                        ex.Message);
                }
                finally
                {
                    _btnSeguent.Enabled =
                        true;

                    _btnSeguent.BackColor =
                        TraspasFacturasTheme.AccioConfirmacio;

                    _btnSeguent.ForeColor =
                        Color.White;
                }

                return;
            }
        }

        private void CmbEmpresaDesti_SelectedIndexChanged(
            object sender,
            EventArgs e)
        {
            bool seleccionada =
                _cmbEmpresaDesti != null &&
                _cmbEmpresaDesti.SelectedItem != null;

            if (_lblMissatgeDesti != null)
            {
                if (seleccionada)
                {
                    _lblMissatgeDesti.Text =
                        "Empresa seleccionada: " +
                        _cmbEmpresaDesti.SelectedItem;
                }
                else
                {
                    _lblMissatgeDesti.Text =
                        _empresesDesti.Count == 1
                            ? "1 empresa de destinació disponible."
                            : _empresesDesti.Count +
                              " empreses de destinació disponibles.";
                }
            }

            if (seleccionada)
            {
                ActualitzarEstat(
                    "Empresa de destinació seleccionada: " +
                    _cmbEmpresaDesti.SelectedItem +
                    ".");
            }
            else
            {
                ActualitzarEstat(
                    "Selecciona l'empresa de destinació.");
            }

            _btnSeguent.Enabled = seleccionada;

            _btnSeguent.BackColor =
                seleccionada
                    ? TraspasFacturasTheme.AccioConfirmacio
                    : TraspasFacturasTheme.FonsBotoDeshabilitat;

            _btnSeguent.ForeColor =
                seleccionada
                    ? Color.White
                    : TraspasFacturasTheme.TextBotoDeshabilitat;
        }

        private void DgvFactures_CurrentCellDirtyStateChanged(
            object sender,
            EventArgs e)
        {
            if (_dgvFactures.IsCurrentCellDirty)
            {
                _dgvFactures.CommitEdit(
                    DataGridViewDataErrorContexts.Commit);
            }
        }

        private void DgvFactures_CellValueChanged(
            object sender,
            DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0 ||
                e.ColumnIndex < 0)
            {
                return;
            }

            if (_dgvFactures.Columns[e.ColumnIndex].Name !=
                "colSeleccionada")
            {
                return;
            }

            ActualitzarResumSeleccio();
        }

        private void DgvFactures_CellBeginEdit(
            object sender,
            DataGridViewCellCancelEventArgs e)
        {
            if (e.RowIndex < 0 ||
                e.ColumnIndex < 0)
            {
                return;
            }

            if (_dgvFactures.Columns[e.ColumnIndex].Name !=
                "colSeleccionada")
            {
                return;
            }

            DataGridViewRow fila =
                _dgvFactures.Rows[e.RowIndex];

            FacturaOrigenDto factura =
                fila.DataBoundItem as FacturaOrigenDto;

            if (factura == null)
            {
                return;
            }

            if (factura.Traspassada)
            {
                e.Cancel = true;
            }
        }

        private void DgvFactures_CellFormatting(
            object sender,
            DataGridViewCellFormattingEventArgs e)
        {
            if (e.RowIndex < 0)
            {
                return;
            }

            DataGridViewRow fila =
                _dgvFactures.Rows[e.RowIndex];

            FacturaOrigenDto factura =
                fila.DataBoundItem as FacturaOrigenDto;

            if (factura == null)
            {
                return;
            }

            if (factura.Traspassada)
            {
                e.CellStyle.ForeColor =
                    Color.Gray;

                e.CellStyle.BackColor =
                    Color.FromArgb(245, 245, 245);
            }
        }

        private void DgvFactures_CellPainting(
            object sender,
            DataGridViewCellPaintingEventArgs e)
        {
            if (e.RowIndex < 0 ||
                e.ColumnIndex < 0)
            {
                return;
            }

            if (_dgvFactures.Columns[e.ColumnIndex].Name !=
                "colSeleccionada")
            {
                return;
            }

            DataGridViewRow fila =
                _dgvFactures.Rows[e.RowIndex];

            FacturaOrigenDto factura =
                fila.DataBoundItem as FacturaOrigenDto;

            if (factura == null ||
                !factura.Traspassada)
            {
                return;
            }

            e.PaintBackground(
                e.CellBounds,
                true);

            Size midaCheck =
                CheckBoxRenderer.GetGlyphSize(
                    e.Graphics,
                    System.Windows.Forms.VisualStyles
                        .CheckBoxState.UncheckedDisabled);

            Point punt =
                new Point(
                    e.CellBounds.Left +
                    (
                        e.CellBounds.Width -
                        midaCheck.Width
                    ) / 2,
                    e.CellBounds.Top +
                    (
                        e.CellBounds.Height -
                        midaCheck.Height
                    ) / 2);

            CheckBoxRenderer.DrawCheckBox(
                e.Graphics,
                punt,
                System.Windows.Forms.VisualStyles
                    .CheckBoxState.UncheckedDisabled);

            e.Handled =
                true;
        }

        private Control CrearContingutDesti()
        {
            Panel exterior = new Panel
            {
                BackColor = TraspasFacturasTheme.Fons,
                Dock = DockStyle.Fill,
                Padding = new Padding(18, 18, 18, 18)
            };

            Panel targeta = new Panel
            {
                BackColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle,
                Dock = DockStyle.Top,
                Height = 310,
                Padding = new Padding(28)
            };

            Label titol = new Label
            {
                AutoSize = true,
                Font = TraspasFacturasTheme.FontSemibold(15F),
                ForeColor = TraspasFacturasTheme.Capcalera,
                Location = new Point(28, 25),
                Text = "Empresa de destinació"
            };

            Label subtitol = new Label
            {
                AutoSize = true,
                Font = TraspasFacturasTheme.FontBase(9.5F),
                ForeColor = TraspasFacturasTheme.TextSecundari,
                Location = new Point(30, 60),
                Text = "Selecciona l'empresa d'a3ERP on vols traspassar les factures seleccionades."
            };

            Label lblEmpresa = new Label
            {
                AutoSize = true,
                Font = TraspasFacturasTheme.FontSemibold(8.5F),
                ForeColor = TraspasFacturasTheme.TextPeu,
                Location = new Point(30, 108),
                Text = "EMPRESA DESTÍ"
            };

            _lblMissatgeDesti = new Label
            {
                AutoSize = false,
                Font = TraspasFacturasTheme.FontBase(9.5F),
                ForeColor = TraspasFacturasTheme.TextSecundari,
                Location = new Point(30, 132),
                Size = new Size(900, 24),
                Text = string.Empty
            };

            _cmbEmpresaDesti = new ComboBox
            {
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = TraspasFacturasTheme.FontBase(10F),
                Location = new Point(30, 162),
                Size = new Size(430, 28)
            };

            _cmbEmpresaDesti.SelectedIndexChanged +=
                CmbEmpresaDesti_SelectedIndexChanged;

            targeta.Controls.Add(titol);
            targeta.Controls.Add(subtitol);
            targeta.Controls.Add(lblEmpresa);
            targeta.Controls.Add(_lblMissatgeDesti);
            targeta.Controls.Add(_cmbEmpresaDesti);

            exterior.Controls.Add(targeta);

            return exterior;
        }

        private void AnarAlPasDesti()
        {
            _pasActual = 2;

            ActualitzarPassos();

            _hostContingut.Controls.Clear();
            _hostContingut.Controls.Add(
                CrearContingutDesti());

            _btnEnrere.Visible = true;

            _btnSeguent.Enabled = false;
            _btnSeguent.BackColor =
                TraspasFacturasTheme.FonsBotoDeshabilitat;
            _btnSeguent.ForeColor =
                TraspasFacturasTheme.TextBotoDeshabilitat;

            CarregarEmpresesDesti();
        }

        private void CarregarEmpresesDesti()
        {
            try
            {
                ActualitzarEstat(
                    "Consultant empreses disponibles d'a3ERP...");

                _empresesDesti =
                    new List<string>(
                        _consultaEmpresasService.ObtenirEmpresasDisponibles(
                            _baseDatosOrigen));

                _cmbEmpresaDesti.Items.Clear();

                foreach (string empresa in _empresesDesti)
                {
                    _cmbEmpresaDesti.Items.Add(empresa);
                }

                if (_empresesDesti.Count == 0)
                {
                    _cmbEmpresaDesti.Enabled = false;

                    _lblMissatgeDesti.Text =
                        "No hi ha cap empresa de destinació disponible. " +
                        "Actualment només està disponible l'empresa origen " +
                        ObtenirTextOrigen() +
                        ".";

                    ActualitzarEstat(
                        "No hi ha cap empresa de destinació disponible.");

                    return;
                }

                _cmbEmpresaDesti.Enabled = true;

                _lblMissatgeDesti.Text =
                    _empresesDesti.Count == 1
                        ? "1 empresa de destinació disponible."
                        : _empresesDesti.Count +
                          " empreses de destinació disponibles.";

                ActualitzarEstat(
                    "Selecciona l'empresa de destinació.");
            }
            catch (Exception ex)
            {
                _cmbEmpresaDesti.Enabled = false;

                MostrarErrorControlat(
                    "No s'han pogut consultar les empreses disponibles.\n\n" +
                    ex.Message);
            }
        }

        private Control CrearContingutTraspas()
        {
            Panel exterior = new Panel
            {
                BackColor = TraspasFacturasTheme.Fons,
                Dock = DockStyle.Fill,
                Padding = new Padding(18)
            };

            TableLayoutPanel layout = new TableLayoutPanel
            {
                BackColor = TraspasFacturasTheme.Fons,
                ColumnCount = 1,
                Dock = DockStyle.Fill,
                RowCount = 2
            };

            layout.RowStyles.Add(
                new RowStyle(SizeType.Absolute, 150F));

            layout.RowStyles.Add(
                new RowStyle(SizeType.Percent, 100F));

            Panel resum = new Panel
            {
                BackColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle,
                Dock = DockStyle.Fill,
                Padding = new Padding(28, 28, 28, 8)
            };

            Label titol = new Label
            {
                AutoSize = true,
                Font = TraspasFacturasTheme.FontSemibold(15F),
                ForeColor = TraspasFacturasTheme.Capcalera,
                Location = new Point(28, 20),
                Text = "Revisió del traspàs"
            };

            Label subtitol = new Label
            {
                AutoSize = true,
                Font = TraspasFacturasTheme.FontBase(9.5F),
                ForeColor = TraspasFacturasTheme.TextSecundari,
                Location = new Point(30, 52),
                Text =
                    "Revisa les dades abans de confirmar el traspàs de factures."
            };

            decimal totalSeleccionat = 0M;

            foreach (FacturaOrigenDto factura in _factures)
            {
                if (factura.Seleccionada)
                {
                    totalSeleccionat += factura.Total;
                }
            }

            TableLayoutPanel taulaResum = new TableLayoutPanel
            {
                ColumnCount = 5,
                RowCount = 1,
                Dock = DockStyle.Bottom,
                Height = 55,
                Margin = new Padding(0),
                Padding = new Padding(0)
            };

            taulaResum.ColumnStyles.Add(
                new ColumnStyle(SizeType.Percent, 20F));

            taulaResum.ColumnStyles.Add(
                new ColumnStyle(SizeType.Percent, 20F));

            taulaResum.ColumnStyles.Add(
                new ColumnStyle(SizeType.Percent, 20F));

            taulaResum.ColumnStyles.Add(
                new ColumnStyle(SizeType.Percent, 20F));

            taulaResum.ColumnStyles.Add(
                new ColumnStyle(SizeType.Percent, 20F));

            taulaResum.Controls.Add(
                CrearBlocResum(
                    "EMPRESA ORIGEN",
                    ObtenirTextOrigen()),
                0,
                0);

            taulaResum.Controls.Add(
                CrearBlocResum(
                    "EMPRESA DESTÍ",
                    _empresaDestiSeleccionada),
                1,
                0);

            taulaResum.Controls.Add(
                CrearBlocResum(
                    "FACTURES",
                    ComptarSeleccionades().ToString()),
                2,
                0);

            taulaResum.Controls.Add(
                CrearBlocResum(
                    "IMPORT TOTAL",
                    totalSeleccionat.ToString("N2") + " €"),
                3,
                0);
            taulaResum.Controls.Add(
                CrearBlocResum(
                    "DATA DE CREACIÓ",
                    DateTime.Today.ToString("dd/MM/yyyy")),
                4,
                0);

            resum.Controls.Add(taulaResum);

            resum.Controls.Add(titol);
            resum.Controls.Add(subtitol);
            resum.Controls.Add(_lblOrigenResum);
            resum.Controls.Add(_lblDestiResum);
            resum.Controls.Add(_lblFacturesResum);
            resum.Controls.Add(_lblTotalResum);

            Panel zonaGraella = new Panel
            {
                BackColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle,
                Dock = DockStyle.Fill,
                Padding = new Padding(0)
            };

            _dgvResumTraspas = new DataGridView
            {
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                AllowUserToResizeRows = false,
                AutoGenerateColumns = false,
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.None,
                CellBorderStyle =
                    DataGridViewCellBorderStyle.SingleHorizontal,
                ColumnHeadersBorderStyle =
                    DataGridViewHeaderBorderStyle.None,
                ColumnHeadersHeight = 42,
                ColumnHeadersHeightSizeMode =
                    DataGridViewColumnHeadersHeightSizeMode.DisableResizing,
                Dock = DockStyle.Fill,
                EnableHeadersVisualStyles = false,
                GridColor = TraspasFacturasTheme.BordeGraella,
                MultiSelect = false,
                ReadOnly = true,
                RowHeadersVisible = false,
                SelectionMode =
                    DataGridViewSelectionMode.FullRowSelect
            };

            _dgvResumTraspas.ColumnHeadersDefaultCellStyle.BackColor =
                TraspasFacturasTheme.Principal;

            _dgvResumTraspas.ColumnHeadersDefaultCellStyle.ForeColor =
                Color.White;

            _dgvResumTraspas.ColumnHeadersDefaultCellStyle.Font =
                TraspasFacturasTheme.FontSemibold(8.5F);

            _dgvResumTraspas.DefaultCellStyle.Font =
                TraspasFacturasTheme.FontBase(8.5F);

            _dgvResumTraspas.DefaultCellStyle.SelectionBackColor =
                TraspasFacturasTheme.SeleccioGraella;

            _dgvResumTraspas.DefaultCellStyle.SelectionForeColor =
                TraspasFacturasTheme.Principal;

            _dgvResumTraspas.RowTemplate.Height = 38;

            CrearColumnesResumTraspas();

            zonaGraella.Controls.Add(_dgvResumTraspas);

            layout.Controls.Add(resum, 0, 0);
            layout.Controls.Add(zonaGraella, 0, 1);

            exterior.Controls.Add(layout);

            return exterior;
        }

        private Panel CrearBlocResum(
            string titol,
            string valor)
        {
            Panel panel = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(0, 0, 15, 0)
            };

            Label lblTitol = new Label
            {
                AutoSize = true,
                Font = TraspasFacturasTheme.FontSemibold(8F),
                ForeColor = TraspasFacturasTheme.TextSecundari,
                Location = new Point(0, 0),
                Text = titol
            };

            Label lblValor = new Label
            {
                AutoSize = true,
                Font = TraspasFacturasTheme.FontSemibold(11F),
                ForeColor = TraspasFacturasTheme.Capcalera,
                Location = new Point(0, 20),
                Text = valor
            };

            panel.Controls.Add(lblTitol);
            panel.Controls.Add(lblValor);

            return panel;
        }

        private void CrearColumnesResumTraspas()
        {
            _dgvResumTraspas.Columns.Clear();

            _dgvResumTraspas.Columns.Add(
                CrearColumnaText(
                    "colFacturaResum",
                    "Factura",
                    "Factura",
                    120));

            _dgvResumTraspas.Columns.Add(
                CrearColumnaText(
                    "colDataResum",
                    "Data",
                    "FechaTexto",
                    100));

            _dgvResumTraspas.Columns.Add(
                CrearColumnaText(
                    "colCodiClientResum",
                    "Codi client",
                    "CodCli",
                    110));

            DataGridViewTextBoxColumn client =
                CrearColumnaText(
                    "colClientResum",
                    "Client",
                    "NomCli",
                    260);

            client.AutoSizeMode =
                DataGridViewAutoSizeColumnMode.Fill;

            _dgvResumTraspas.Columns.Add(client);

            _dgvResumTraspas.Columns.Add(
                CrearColumnaDecimal(
                    "colBaseResum",
                    "Base",
                    "Base",
                    100));

            _dgvResumTraspas.Columns.Add(
                CrearColumnaDecimal(
                    "colIvaResum",
                    "IVA",
                    "Iva",
                    90));

            _dgvResumTraspas.Columns.Add(
                CrearColumnaDecimal(
                    "colTotalResum",
                    "Total",
                    "Total",
                    110));

            _dgvResumTraspas.Columns.Add(
                CrearColumnaText(
                    "colTipusResum",
                    "Tipus",
                    "Tipo",
                    140));
        }

        private void AnarAlPasTraspas()
        {
            List<FacturaOrigenDto> seleccionades =
                new List<FacturaOrigenDto>();

            foreach (FacturaOrigenDto factura in _factures)
            {
                if (factura.Seleccionada)
                {
                    seleccionades.Add(factura);
                }
            }

            if (seleccionades.Count == 0)
            {
                MostrarErrorControlat(
                    "No hi ha cap factura seleccionada.");

                return;
            }

            if (_cmbEmpresaDesti == null ||
                _cmbEmpresaDesti.SelectedItem == null)
            {
                MostrarErrorControlat(
                    "No s'ha seleccionat cap empresa de destinació.");

                return;
            }

            _pasActual = 3;

            ActualitzarPassos();

            _hostContingut.Controls.Clear();

            _contingutPasTraspas =
                CrearContingutTraspas();

            _hostContingut.Controls.Add(
                _contingutPasTraspas);

            _dgvResumTraspas.DataSource = null;
            _dgvResumTraspas.DataSource = seleccionades;

            _btnEnrere.Visible = true;

            _btnSeguent.Text =
                "Confirmar traspàs";

            _btnSeguent.Width =
                160;

            _btnSeguent.Enabled = true;

            _btnSeguent.BackColor =
                TraspasFacturasTheme.AccioConfirmacio;

            _btnSeguent.ForeColor =
                Color.White;

            ActualitzarEstat(
                "Revisa les dades abans de confirmar el traspàs.");
        }
        private void BotoNetejarCerca_Click(
            object sender,
            EventArgs e)
        {
            _txtFiltreText.Clear();
            _txtFiltreText.Focus();
        }

        private void TxtFiltreText_TextChanged(
            object sender,
            EventArgs e)
        {
            if (_btnNetejarCerca != null)
            {
                _btnNetejarCerca.Visible =
                    !string.IsNullOrWhiteSpace(
                        _txtFiltreText.Text);
            }
        }

    }
}
