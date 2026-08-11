namespace A3ErpGestorFormulesTarifes
{
    partial class FrmEdicioFormulaTarifa
    {
        private System.ComponentModel.IContainer components = null;

        private System.Windows.Forms.TableLayoutPanel tblArrel;

        private System.Windows.Forms.Panel pnlCapcalera;
        private System.Windows.Forms.Label lblTitol;
        private System.Windows.Forms.Label lblSubtitol;

        private System.Windows.Forms.Panel pnlContingut;
        private System.Windows.Forms.TabControl tabPrincipal;
        private System.Windows.Forms.TabPage tabGeneral;
        private System.Windows.Forms.TabPage tabTarifes;
        private System.Windows.Forms.TabPage tabDescomptes;

        private System.Windows.Forms.TableLayoutPanel tblGeneral;
        private System.Windows.Forms.TextBox txtCodi;
        private System.Windows.Forms.TextBox txtNom;
        private System.Windows.Forms.TextBox txtDescripcio;
        private System.Windows.Forms.NumericUpDown nudOrdre;
        private System.Windows.Forms.ComboBox cboTipusValors;
        private System.Windows.Forms.CheckBox chkActiva;
        private System.Windows.Forms.CheckBox chkUtilitzaValorsTarifes;
        private System.Windows.Forms.FlowLayoutPanel flpOpcionsGeneral;

        private System.Windows.Forms.TableLayoutPanel tblTarifes;
        private System.Windows.Forms.TableLayoutPanel tblValorsInicials;
        private System.Windows.Forms.NumericUpDown nudValorP1;
        private System.Windows.Forms.NumericUpDown nudValorP2;
        private System.Windows.Forms.NumericUpDown nudValorP3;
        private System.Windows.Forms.NumericUpDown nudValorP4;
        private System.Windows.Forms.TextBox txtExpressioTarifa1;
        private System.Windows.Forms.TextBox txtExpressioTarifa2;
        private System.Windows.Forms.TextBox txtExpressioTarifa3;
        private System.Windows.Forms.TextBox txtExpressioTarifa4;
        private System.Windows.Forms.TextBox txtExpressioTarifa5;
        private System.Windows.Forms.TextBox txtExpressioTarifa6;

        private System.Windows.Forms.TableLayoutPanel tblDescomptes;
        private System.Windows.Forms.CheckBox chkGeneraDescomptes;
        private System.Windows.Forms.NumericUpDown nudDescompteGrup1;
        private System.Windows.Forms.NumericUpDown nudDescompteGrup2;
        private System.Windows.Forms.NumericUpDown nudDescompteGrup3;
        private System.Windows.Forms.NumericUpDown nudDescompteGrup4;

        private System.Windows.Forms.Panel pnlAccions;
        private System.Windows.Forms.Label lblEstat;
        private System.Windows.Forms.FlowLayoutPanel flpAccions;
        private System.Windows.Forms.Button btnDesar;
        private System.Windows.Forms.Button btnCancelar;

        /// <summary>
        /// Allibera els recursos utilitzats
        /// pel formulari.
        /// </summary>
        protected override void Dispose(
            bool disposing)
        {
            if (disposing &&
                components != null)
            {
                components.Dispose();
            }

            base.Dispose(
                disposing);
        }

        #region Codi generat pel dissenyador

        /// <summary>
        /// Inicialitza els controls visuals
        /// del formulari d'edició.
        /// </summary>
        private void InitializeComponent()
        {
            this.components =
                new System.ComponentModel.Container();

            this.tblArrel =
                new System.Windows.Forms.TableLayoutPanel();

            this.pnlCapcalera =
                new System.Windows.Forms.Panel();

            this.lblTitol =
                new System.Windows.Forms.Label();

            this.lblSubtitol =
                new System.Windows.Forms.Label();

            this.pnlContingut =
                new System.Windows.Forms.Panel();

            this.tabPrincipal =
                new System.Windows.Forms.TabControl();

            this.tabGeneral =
                new System.Windows.Forms.TabPage();

            this.tabTarifes =
                new System.Windows.Forms.TabPage();

            this.tabDescomptes =
                new System.Windows.Forms.TabPage();

            this.tblGeneral =
                new System.Windows.Forms.TableLayoutPanel();

            this.txtCodi =
                new System.Windows.Forms.TextBox();

            this.txtNom =
                new System.Windows.Forms.TextBox();

            this.txtDescripcio =
                new System.Windows.Forms.TextBox();

            this.nudOrdre =
                new System.Windows.Forms.NumericUpDown();

            this.cboTipusValors =
                new System.Windows.Forms.ComboBox();

            this.chkActiva =
                new System.Windows.Forms.CheckBox();

            this.chkUtilitzaValorsTarifes =
                new System.Windows.Forms.CheckBox();

            this.flpOpcionsGeneral =
                new System.Windows.Forms.FlowLayoutPanel();

            this.tblTarifes =
                new System.Windows.Forms.TableLayoutPanel();

            this.tblValorsInicials =
                new System.Windows.Forms.TableLayoutPanel();

            this.nudValorP1 =
                new System.Windows.Forms.NumericUpDown();

            this.nudValorP2 =
                new System.Windows.Forms.NumericUpDown();

            this.nudValorP3 =
                new System.Windows.Forms.NumericUpDown();

            this.nudValorP4 =
                new System.Windows.Forms.NumericUpDown();

            this.txtExpressioTarifa1 =
                new System.Windows.Forms.TextBox();

            this.txtExpressioTarifa2 =
                new System.Windows.Forms.TextBox();

            this.txtExpressioTarifa3 =
                new System.Windows.Forms.TextBox();

            this.txtExpressioTarifa4 =
                new System.Windows.Forms.TextBox();

            this.txtExpressioTarifa5 =
                new System.Windows.Forms.TextBox();

            this.txtExpressioTarifa6 =
                new System.Windows.Forms.TextBox();

            this.tblDescomptes =
                new System.Windows.Forms.TableLayoutPanel();

            this.chkGeneraDescomptes =
                new System.Windows.Forms.CheckBox();

            this.nudDescompteGrup1 =
                new System.Windows.Forms.NumericUpDown();

            this.nudDescompteGrup2 =
                new System.Windows.Forms.NumericUpDown();

            this.nudDescompteGrup3 =
                new System.Windows.Forms.NumericUpDown();

            this.nudDescompteGrup4 =
                new System.Windows.Forms.NumericUpDown();

            this.pnlAccions =
                new System.Windows.Forms.Panel();

            this.lblEstat =
                new System.Windows.Forms.Label();

            this.flpAccions =
                new System.Windows.Forms.FlowLayoutPanel();

            this.btnDesar =
                new System.Windows.Forms.Button();

            this.btnCancelar =
                new System.Windows.Forms.Button();

            ((System.ComponentModel.ISupportInitialize)
                (this.nudOrdre))
                .BeginInit();

            ((System.ComponentModel.ISupportInitialize)
                (this.nudValorP1))
                .BeginInit();

            ((System.ComponentModel.ISupportInitialize)
                (this.nudValorP2))
                .BeginInit();

            ((System.ComponentModel.ISupportInitialize)
                (this.nudValorP3))
                .BeginInit();

            ((System.ComponentModel.ISupportInitialize)
                (this.nudValorP4))
                .BeginInit();

            ((System.ComponentModel.ISupportInitialize)
                (this.nudDescompteGrup1))
                .BeginInit();

            ((System.ComponentModel.ISupportInitialize)
                (this.nudDescompteGrup2))
                .BeginInit();

            ((System.ComponentModel.ISupportInitialize)
                (this.nudDescompteGrup3))
                .BeginInit();

            ((System.ComponentModel.ISupportInitialize)
                (this.nudDescompteGrup4))
                .BeginInit();

            this.SuspendLayout();

            /*
             * Formulari principal.
             */
            this.AutoScaleDimensions =
                new System.Drawing.SizeF(
                    96F,
                    96F);

            this.AutoScaleMode =
                System.Windows.Forms.AutoScaleMode.Dpi;

            this.BackColor =
                System.Drawing.Color.FromArgb(
                    245,
                    247,
                    250);

            this.ClientSize =
                new System.Drawing.Size(
                    1220,
                    780);

            this.Font =
                new System.Drawing.Font(
                    "Segoe UI",
                    9F);

            this.MinimumSize =
                new System.Drawing.Size(
                    1040,
                    680);

            this.MinimizeBox =
                false;

            this.StartPosition =
                System.Windows.Forms.FormStartPosition.CenterParent;

            this.Text =
                "Edició de fórmula de tarifes · a3ERP";

            /*
             * Estructura general.
             */
            this.tblArrel.ColumnCount =
                1;

            this.tblArrel.ColumnStyles.Add(
                new System.Windows.Forms.ColumnStyle(
                    System.Windows.Forms.SizeType.Percent,
                    100F));

            this.tblArrel.Dock =
                System.Windows.Forms.DockStyle.Fill;

            this.tblArrel.RowCount =
                3;

            this.tblArrel.RowStyles.Add(
                new System.Windows.Forms.RowStyle(
                    System.Windows.Forms.SizeType.Absolute,
                    92F));

            this.tblArrel.RowStyles.Add(
                new System.Windows.Forms.RowStyle(
                    System.Windows.Forms.SizeType.Percent,
                    100F));

            this.tblArrel.RowStyles.Add(
                new System.Windows.Forms.RowStyle(
                    System.Windows.Forms.SizeType.Absolute,
                    84F));

            this.Controls.Add(
                this.tblArrel);

            /*
             * Capçalera.
             */
            this.pnlCapcalera.BackColor =
                System.Drawing.Color.FromArgb(
                    15,
                    23,
                    42);

            this.pnlCapcalera.Dock =
                System.Windows.Forms.DockStyle.Fill;

            this.pnlCapcalera.Padding =
                new System.Windows.Forms.Padding(
                    28,
                    17,
                    28,
                    12);

            this.lblTitol.AutoSize =
                true;

            this.lblTitol.Font =
                new System.Drawing.Font(
                    "Segoe UI Semibold",
                    21F);

            this.lblTitol.ForeColor =
                System.Drawing.Color.White;

            this.lblTitol.Location =
                new System.Drawing.Point(
                    28,
                    14);

            this.lblTitol.Text =
                "Fórmula de tarifes";

            this.lblSubtitol.AutoSize =
                true;

            this.lblSubtitol.Font =
                new System.Drawing.Font(
                    "Segoe UI",
                    10F);

            this.lblSubtitol.ForeColor =
                System.Drawing.Color.FromArgb(
                    203,
                    213,
                    225);

            this.lblSubtitol.Location =
                new System.Drawing.Point(
                    31,
                    55);

            this.lblSubtitol.Text =
                "Defineix les dades generals, els valors inicials i les expressions de càlcul.";

            this.pnlCapcalera.Controls.Add(
                this.lblTitol);

            this.pnlCapcalera.Controls.Add(
                this.lblSubtitol);

            this.tblArrel.Controls.Add(
                this.pnlCapcalera,
                0,
                0);

            /*
             * Zona central.
             */
            this.pnlContingut.BackColor =
                System.Drawing.Color.FromArgb(
                    245,
                    247,
                    250);

            this.pnlContingut.Dock =
                System.Windows.Forms.DockStyle.Fill;

            this.pnlContingut.Padding =
                new System.Windows.Forms.Padding(
                    18,
                    16,
                    18,
                    16);

            this.tabPrincipal.Dock =
                System.Windows.Forms.DockStyle.Fill;

            this.tabPrincipal.Font =
                new System.Drawing.Font(
                    "Segoe UI Semibold",
                    9.5F);

            this.tabPrincipal.Controls.Add(
                this.tabGeneral);

            this.tabPrincipal.Controls.Add(
                this.tabTarifes);

            this.tabPrincipal.Controls.Add(
                this.tabDescomptes);

            this.pnlContingut.Controls.Add(
                this.tabPrincipal);

            this.tblArrel.Controls.Add(
                this.pnlContingut,
                0,
                1);

            /*
             * Pestanya General.
             */
            this.tabGeneral.AutoScroll =
                true;

            this.tabGeneral.BackColor =
                System.Drawing.Color.White;

            this.tabGeneral.Padding =
                new System.Windows.Forms.Padding(
                    12);

            this.tabGeneral.Text =
                "General";

            this.tblGeneral.AutoSize =
                true;

            this.tblGeneral.AutoSizeMode =
                System.Windows.Forms.AutoSizeMode.GrowAndShrink;

            this.tblGeneral.ColumnCount =
                4;

            this.tblGeneral.ColumnStyles.Add(
                new System.Windows.Forms.ColumnStyle(
                    System.Windows.Forms.SizeType.Absolute,
                    155F));

            this.tblGeneral.ColumnStyles.Add(
                new System.Windows.Forms.ColumnStyle(
                    System.Windows.Forms.SizeType.Percent,
                    50F));

            this.tblGeneral.ColumnStyles.Add(
                new System.Windows.Forms.ColumnStyle(
                    System.Windows.Forms.SizeType.Absolute,
                    175F));

            this.tblGeneral.ColumnStyles.Add(
                new System.Windows.Forms.ColumnStyle(
                    System.Windows.Forms.SizeType.Percent,
                    50F));

            this.tblGeneral.Dock =
                System.Windows.Forms.DockStyle.Top;

            this.tblGeneral.Padding =
                new System.Windows.Forms.Padding(
                    18);

            this.tblGeneral.RowCount =
                4;

            this.tblGeneral.RowStyles.Add(
                new System.Windows.Forms.RowStyle(
                    System.Windows.Forms.SizeType.Absolute,
                    54F));

            this.tblGeneral.RowStyles.Add(
                new System.Windows.Forms.RowStyle(
                    System.Windows.Forms.SizeType.Absolute,
                    54F));

            this.tblGeneral.RowStyles.Add(
                new System.Windows.Forms.RowStyle(
                    System.Windows.Forms.SizeType.Absolute,
                    160F));

            this.tblGeneral.RowStyles.Add(
                new System.Windows.Forms.RowStyle(
                    System.Windows.Forms.SizeType.Absolute,
                    58F));

            this.tblGeneral.Controls.Add(
                CrearEtiqueta("Codi *"),
                0,
                0);

            ConfigurarCampText(
                this.txtCodi);

            this.tblGeneral.Controls.Add(
                this.txtCodi,
                1,
                0);

            this.tblGeneral.Controls.Add(
                CrearEtiqueta("Ordre *"),
                2,
                0);

            ConfigurarNumeric(
                this.nudOrdre,
                0);

            this.nudOrdre.Minimum =
                1M;

            this.nudOrdre.Maximum =
                1000000M;

            this.nudOrdre.Value =
                1M;

            this.tblGeneral.Controls.Add(
                this.nudOrdre,
                3,
                0);

            this.tblGeneral.Controls.Add(
                CrearEtiqueta("Nom *"),
                0,
                1);

            ConfigurarCampText(
                this.txtNom);

            this.tblGeneral.Controls.Add(
                this.txtNom,
                1,
                1);

            this.tblGeneral.Controls.Add(
                CrearEtiqueta("Tipus de valors *"),
                2,
                1);

            this.cboTipusValors.Dock =
                System.Windows.Forms.DockStyle.Fill;

            this.cboTipusValors.DropDownStyle =
                System.Windows.Forms.ComboBoxStyle.DropDownList;

            this.cboTipusValors.Font =
                new System.Drawing.Font(
                    "Segoe UI",
                    9.5F);

            this.cboTipusValors.Margin =
                new System.Windows.Forms.Padding(
                    0,
                    8,
                    14,
                    8);

            this.tblGeneral.Controls.Add(
                this.cboTipusValors,
                3,
                1);

            this.tblGeneral.Controls.Add(
                CrearEtiqueta("Descripció"),
                0,
                2);

            ConfigurarCampText(
                this.txtDescripcio);

            this.txtDescripcio.AcceptsReturn =
                true;

            this.txtDescripcio.Multiline =
                true;

            this.txtDescripcio.ScrollBars =
                System.Windows.Forms.ScrollBars.Vertical;

            this.tblGeneral.Controls.Add(
                this.txtDescripcio,
                1,
                2);

            this.tblGeneral.SetColumnSpan(
                this.txtDescripcio,
                3);

            this.tblGeneral.Controls.Add(
                CrearEtiqueta("Estat i ús"),
                0,
                3);

            this.flpOpcionsGeneral.AutoSize =
                true;

            this.flpOpcionsGeneral.Dock =
                System.Windows.Forms.DockStyle.Fill;

            this.flpOpcionsGeneral.FlowDirection =
                System.Windows.Forms.FlowDirection.LeftToRight;

            this.flpOpcionsGeneral.Margin =
                new System.Windows.Forms.Padding(
                    0,
                    8,
                    0,
                    0);

            this.flpOpcionsGeneral.WrapContents =
                false;

            ConfigurarCheckBox(
                this.chkActiva,
                "Fórmula activa");

            this.chkActiva.Checked =
                true;

            ConfigurarCheckBox(
                this.chkUtilitzaValorsTarifes,
                "Utilitza valors inicials P1–P4");

            this.flpOpcionsGeneral.Controls.Add(
                this.chkActiva);

            this.flpOpcionsGeneral.Controls.Add(
                this.chkUtilitzaValorsTarifes);

            this.tblGeneral.Controls.Add(
                this.flpOpcionsGeneral,
                1,
                3);

            this.tblGeneral.SetColumnSpan(
                this.flpOpcionsGeneral,
                3);

            this.tabGeneral.Controls.Add(
                this.tblGeneral);

            /*
             * Pestanya Tarifes.
             */
            this.tabTarifes.AutoScroll =
                true;

            this.tabTarifes.BackColor =
                System.Drawing.Color.White;

            this.tabTarifes.Padding =
                new System.Windows.Forms.Padding(
                    12);

            this.tabTarifes.Text =
                "Tarifes";

            this.tblTarifes.AutoSize =
                true;

            this.tblTarifes.AutoSizeMode =
                System.Windows.Forms.AutoSizeMode.GrowAndShrink;

            this.tblTarifes.ColumnCount =
                2;

            this.tblTarifes.ColumnStyles.Add(
                new System.Windows.Forms.ColumnStyle(
                    System.Windows.Forms.SizeType.Absolute,
                    190F));

            this.tblTarifes.ColumnStyles.Add(
                new System.Windows.Forms.ColumnStyle(
                    System.Windows.Forms.SizeType.Percent,
                    100F));

            this.tblTarifes.Dock =
                System.Windows.Forms.DockStyle.Top;

            this.tblTarifes.Padding =
                new System.Windows.Forms.Padding(
                    18);

            this.tblTarifes.RowCount =
                9;

            this.tblTarifes.RowStyles.Add(
                new System.Windows.Forms.RowStyle(
                    System.Windows.Forms.SizeType.Absolute,
                    38F));

            this.tblTarifes.RowStyles.Add(
                new System.Windows.Forms.RowStyle(
                    System.Windows.Forms.SizeType.Absolute,
                    64F));

            this.tblTarifes.RowStyles.Add(
                new System.Windows.Forms.RowStyle(
                    System.Windows.Forms.SizeType.Absolute,
                    48F));

            for (int index = 0;
                 index < 6;
                 index++)
            {
                this.tblTarifes.RowStyles.Add(
                    new System.Windows.Forms.RowStyle(
                        System.Windows.Forms.SizeType.Absolute,
                        54F));
            }

            System.Windows.Forms.Label lblValorsInicials =
                CrearTitolSeccio(
                    "Valors inicials P1–P4");

            this.tblTarifes.Controls.Add(
                lblValorsInicials,
                0,
                0);

            this.tblTarifes.SetColumnSpan(
                lblValorsInicials,
                2);

            this.tblValorsInicials.ColumnCount =
                8;

            for (int index = 0;
                 index < 4;
                 index++)
            {
                this.tblValorsInicials.ColumnStyles.Add(
                    new System.Windows.Forms.ColumnStyle(
                        System.Windows.Forms.SizeType.Absolute,
                        42F));

                this.tblValorsInicials.ColumnStyles.Add(
                    new System.Windows.Forms.ColumnStyle(
                        System.Windows.Forms.SizeType.Percent,
                        25F));
            }

            this.tblValorsInicials.Dock =
                System.Windows.Forms.DockStyle.Fill;

            this.tblValorsInicials.RowCount =
                1;

            this.tblValorsInicials.RowStyles.Add(
                new System.Windows.Forms.RowStyle(
                    System.Windows.Forms.SizeType.Percent,
                    100F));

            ConfigurarNumeric(
                this.nudValorP1,
                4);

            ConfigurarNumeric(
                this.nudValorP2,
                4);

            ConfigurarNumeric(
                this.nudValorP3,
                4);

            ConfigurarNumeric(
                this.nudValorP4,
                4);

            this.tblValorsInicials.Controls.Add(
                CrearEtiquetaCompacta("P1"),
                0,
                0);

            this.tblValorsInicials.Controls.Add(
                this.nudValorP1,
                1,
                0);

            this.tblValorsInicials.Controls.Add(
                CrearEtiquetaCompacta("P2"),
                2,
                0);

            this.tblValorsInicials.Controls.Add(
                this.nudValorP2,
                3,
                0);

            this.tblValorsInicials.Controls.Add(
                CrearEtiquetaCompacta("P3"),
                4,
                0);

            this.tblValorsInicials.Controls.Add(
                this.nudValorP3,
                5,
                0);

            this.tblValorsInicials.Controls.Add(
                CrearEtiquetaCompacta("P4"),
                6,
                0);

            this.tblValorsInicials.Controls.Add(
                this.nudValorP4,
                7,
                0);

            this.tblTarifes.Controls.Add(
                this.tblValorsInicials,
                0,
                1);

            this.tblTarifes.SetColumnSpan(
                this.tblValorsInicials,
                2);

            System.Windows.Forms.Label lblExpressions =
                CrearTitolSeccio(
                    "Expressions de càlcul");

            lblExpressions.Text +=
                " · Variables disponibles: PRCCOMPRA, PRCCOSTE, PRCSTANDARD i P1–P4";

            this.tblTarifes.Controls.Add(
                lblExpressions,
                0,
                2);

            this.tblTarifes.SetColumnSpan(
                lblExpressions,
                2);

            ConfigurarExpressio(
                this.txtExpressioTarifa1);

            ConfigurarExpressio(
                this.txtExpressioTarifa2);

            ConfigurarExpressio(
                this.txtExpressioTarifa3);

            ConfigurarExpressio(
                this.txtExpressioTarifa4);

            ConfigurarExpressio(
                this.txtExpressioTarifa5);

            ConfigurarExpressio(
                this.txtExpressioTarifa6);

            this.tblTarifes.Controls.Add(
                CrearEtiqueta("Tarifa 1 *"),
                0,
                3);

            this.tblTarifes.Controls.Add(
                this.txtExpressioTarifa1,
                1,
                3);

            this.tblTarifes.Controls.Add(
                CrearEtiqueta("Tarifa 2 *"),
                0,
                4);

            this.tblTarifes.Controls.Add(
                this.txtExpressioTarifa2,
                1,
                4);

            this.tblTarifes.Controls.Add(
                CrearEtiqueta("Tarifa 3 *"),
                0,
                5);

            this.tblTarifes.Controls.Add(
                this.txtExpressioTarifa3,
                1,
                5);

            this.tblTarifes.Controls.Add(
                CrearEtiqueta("Tarifa 4 *"),
                0,
                6);

            this.tblTarifes.Controls.Add(
                this.txtExpressioTarifa4,
                1,
                6);

            this.tblTarifes.Controls.Add(
                CrearEtiqueta("Tarifa 5 *"),
                0,
                7);

            this.tblTarifes.Controls.Add(
                this.txtExpressioTarifa5,
                1,
                7);

            this.tblTarifes.Controls.Add(
                CrearEtiqueta("Tarifa 6 *"),
                0,
                8);

            this.tblTarifes.Controls.Add(
                this.txtExpressioTarifa6,
                1,
                8);

            this.tabTarifes.Controls.Add(
                this.tblTarifes);

            /*
             * Pestanya Descomptes.
             */
            this.tabDescomptes.AutoScroll =
                true;

            this.tabDescomptes.BackColor =
                System.Drawing.Color.White;

            this.tabDescomptes.Padding =
                new System.Windows.Forms.Padding(
                    12);

            this.tabDescomptes.Text =
                "Descomptes";

            this.tblDescomptes.AutoSize =
                true;

            this.tblDescomptes.AutoSizeMode =
                System.Windows.Forms.AutoSizeMode.GrowAndShrink;

            this.tblDescomptes.ColumnCount =
                4;

            this.tblDescomptes.ColumnStyles.Add(
                new System.Windows.Forms.ColumnStyle(
                    System.Windows.Forms.SizeType.Absolute,
                    180F));

            this.tblDescomptes.ColumnStyles.Add(
                new System.Windows.Forms.ColumnStyle(
                    System.Windows.Forms.SizeType.Percent,
                    50F));

            this.tblDescomptes.ColumnStyles.Add(
                new System.Windows.Forms.ColumnStyle(
                    System.Windows.Forms.SizeType.Absolute,
                    180F));

            this.tblDescomptes.ColumnStyles.Add(
                new System.Windows.Forms.ColumnStyle(
                    System.Windows.Forms.SizeType.Percent,
                    50F));

            this.tblDescomptes.Dock =
                System.Windows.Forms.DockStyle.Top;

            this.tblDescomptes.Padding =
                new System.Windows.Forms.Padding(
                    18);

            this.tblDescomptes.RowCount =
                4;

            this.tblDescomptes.RowStyles.Add(
                new System.Windows.Forms.RowStyle(
                    System.Windows.Forms.SizeType.Absolute,
                    52F));

            this.tblDescomptes.RowStyles.Add(
                new System.Windows.Forms.RowStyle(
                    System.Windows.Forms.SizeType.Absolute,
                    58F));

            this.tblDescomptes.RowStyles.Add(
                new System.Windows.Forms.RowStyle(
                    System.Windows.Forms.SizeType.Absolute,
                    58F));

            this.tblDescomptes.RowStyles.Add(
                new System.Windows.Forms.RowStyle(
                    System.Windows.Forms.SizeType.Absolute,
                    58F));

            System.Windows.Forms.Label lblInfoDescomptes =
                CrearTitolSeccio(
                    "Configuració dels descomptes inicials");

            lblInfoDescomptes.Text +=
                " · Només s'aplicaran quan la fórmula generi descomptes.";

            this.tblDescomptes.Controls.Add(
                lblInfoDescomptes,
                0,
                0);

            this.tblDescomptes.SetColumnSpan(
                lblInfoDescomptes,
                4);

            ConfigurarCheckBox(
                this.chkGeneraDescomptes,
                "Aquesta fórmula genera descomptes");

            this.tblDescomptes.Controls.Add(
                this.chkGeneraDescomptes,
                0,
                1);

            this.tblDescomptes.SetColumnSpan(
                this.chkGeneraDescomptes,
                4);

            ConfigurarNumeric(
                this.nudDescompteGrup1,
                4);

            ConfigurarNumeric(
                this.nudDescompteGrup2,
                4);

            ConfigurarNumeric(
                this.nudDescompteGrup3,
                4);

            ConfigurarNumeric(
                this.nudDescompteGrup4,
                4);

            this.tblDescomptes.Controls.Add(
                CrearEtiqueta("Descompte grup 1"),
                0,
                2);

            this.tblDescomptes.Controls.Add(
                this.nudDescompteGrup1,
                1,
                2);

            this.tblDescomptes.Controls.Add(
                CrearEtiqueta("Descompte grup 2"),
                2,
                2);

            this.tblDescomptes.Controls.Add(
                this.nudDescompteGrup2,
                3,
                2);

            this.tblDescomptes.Controls.Add(
                CrearEtiqueta("Descompte grup 3"),
                0,
                3);

            this.tblDescomptes.Controls.Add(
                this.nudDescompteGrup3,
                1,
                3);

            this.tblDescomptes.Controls.Add(
                CrearEtiqueta("Descompte grup 4"),
                2,
                3);

            this.tblDescomptes.Controls.Add(
                this.nudDescompteGrup4,
                3,
                3);

            this.tabDescomptes.Controls.Add(
                this.tblDescomptes);

            /*
             * Barra inferior.
             */
            this.pnlAccions.BackColor =
                System.Drawing.Color.White;

            this.pnlAccions.Dock =
                System.Windows.Forms.DockStyle.Fill;

            this.pnlAccions.Padding =
                new System.Windows.Forms.Padding(
                    22,
                    16,
                    22,
                    16);

            this.lblEstat.Dock =
                System.Windows.Forms.DockStyle.Fill;

            this.lblEstat.ForeColor =
                System.Drawing.Color.FromArgb(
                    71,
                    85,
                    105);

            this.lblEstat.Padding =
                new System.Windows.Forms.Padding(
                    0,
                    0,
                    12,
                    0);

            this.lblEstat.Text =
                "Completa les dades obligatòries marcades amb un asterisc.";

            this.lblEstat.TextAlign =
                System.Drawing.ContentAlignment.MiddleLeft;

            this.flpAccions.AutoSize =
                true;

            this.flpAccions.AutoSizeMode =
                System.Windows.Forms.AutoSizeMode.GrowAndShrink;

            this.flpAccions.Dock =
                System.Windows.Forms.DockStyle.Right;

            this.flpAccions.FlowDirection =
                System.Windows.Forms.FlowDirection.LeftToRight;

            this.flpAccions.Margin =
                new System.Windows.Forms.Padding(
                    0);

            this.flpAccions.Padding =
                new System.Windows.Forms.Padding(
                    0);

            this.flpAccions.WrapContents =
                false;

            ConfigurarBotoAccio(
                this.btnDesar,
                "Desar",
                110);

            this.btnDesar.BackColor =
                System.Drawing.Color.FromArgb(
                    14,
                    116,
                    144);

            this.btnDesar.ForeColor =
                System.Drawing.Color.White;

            this.btnDesar.FlatAppearance.BorderColor =
                System.Drawing.Color.FromArgb(
                    14,
                    116,
                    144);

            ConfigurarBotoAccio(
                this.btnCancelar,
                "Cancel·lar",
                110);

            this.btnCancelar.DialogResult =
                System.Windows.Forms.DialogResult.Cancel;

            this.flpAccions.Controls.Add(
                this.btnDesar);

            this.flpAccions.Controls.Add(
                this.btnCancelar);

            this.pnlAccions.Controls.Add(
                this.lblEstat);

            this.pnlAccions.Controls.Add(
                this.flpAccions);

            this.tblArrel.Controls.Add(
                this.pnlAccions,
                0,
                2);

            /*
             * Comportament de teclat.
             */
            this.AcceptButton =
                this.btnDesar;

            this.CancelButton =
                this.btnCancelar;

            ((System.ComponentModel.ISupportInitialize)
                (this.nudOrdre))
                .EndInit();

            ((System.ComponentModel.ISupportInitialize)
                (this.nudValorP1))
                .EndInit();

            ((System.ComponentModel.ISupportInitialize)
                (this.nudValorP2))
                .EndInit();

            ((System.ComponentModel.ISupportInitialize)
                (this.nudValorP3))
                .EndInit();

            ((System.ComponentModel.ISupportInitialize)
                (this.nudValorP4))
                .EndInit();

            ((System.ComponentModel.ISupportInitialize)
                (this.nudDescompteGrup1))
                .EndInit();

            ((System.ComponentModel.ISupportInitialize)
                (this.nudDescompteGrup2))
                .EndInit();

            ((System.ComponentModel.ISupportInitialize)
                (this.nudDescompteGrup3))
                .EndInit();

            ((System.ComponentModel.ISupportInitialize)
                (this.nudDescompteGrup4))
                .EndInit();

            this.ResumeLayout(
                false);
        }

        /// <summary>
        /// Crea una etiqueta estàndard
        /// per als camps del formulari.
        /// </summary>
        private static System.Windows.Forms.Label CrearEtiqueta(
            string text)
        {
            return new System.Windows.Forms.Label
            {
                AutoSize = true,
                Anchor =
                    System.Windows.Forms.AnchorStyles.Left,
                Font =
                    new System.Drawing.Font(
                        "Segoe UI Semibold",
                        9F),
                ForeColor =
                    System.Drawing.Color.FromArgb(
                        51,
                        65,
                        85),
                Margin =
                    new System.Windows.Forms.Padding(
                        0,
                        0,
                        12,
                        0),
                Text =
                    text
            };
        }

        /// <summary>
        /// Crea una etiqueta compacta
        /// per als paràmetres P1–P4.
        /// </summary>
        private static System.Windows.Forms.Label CrearEtiquetaCompacta(
            string text)
        {
            return new System.Windows.Forms.Label
            {
                AutoSize = true,
                Anchor =
                    System.Windows.Forms.AnchorStyles.Left,
                Font =
                    new System.Drawing.Font(
                        "Segoe UI Semibold",
                        9F),
                ForeColor =
                    System.Drawing.Color.FromArgb(
                        51,
                        65,
                        85),
                Text =
                    text
            };
        }

        /// <summary>
        /// Crea un títol informatiu
        /// per a una secció interna.
        /// </summary>
        private static System.Windows.Forms.Label CrearTitolSeccio(
            string text)
        {
            return new System.Windows.Forms.Label
            {
                AutoSize = true,
                Anchor =
                    System.Windows.Forms.AnchorStyles.Left,
                Font =
                    new System.Drawing.Font(
                        "Segoe UI Semibold",
                        10F),
                ForeColor =
                    System.Drawing.Color.FromArgb(
                        15,
                        23,
                        42),
                Text =
                    text
            };
        }

        /// <summary>
        /// Aplica l'estil comú
        /// als camps de text.
        /// </summary>
        private static void ConfigurarCampText(
            System.Windows.Forms.TextBox camp)
        {
            camp.BorderStyle =
                System.Windows.Forms.BorderStyle.FixedSingle;

            camp.Dock =
                System.Windows.Forms.DockStyle.Fill;

            camp.Font =
                new System.Drawing.Font(
                    "Segoe UI",
                    9.5F);

            camp.Margin =
                new System.Windows.Forms.Padding(
                    0,
                    8,
                    14,
                    8);
        }

        /// <summary>
        /// Aplica l'estil dels camps
        /// que contenen expressions.
        /// </summary>
        private static void ConfigurarExpressio(
            System.Windows.Forms.TextBox camp)
        {
            camp.BorderStyle =
                System.Windows.Forms.BorderStyle.FixedSingle;

            camp.Dock =
                System.Windows.Forms.DockStyle.Fill;

            camp.Font =
                new System.Drawing.Font(
                    "Consolas",
                    9.5F);

            camp.Margin =
                new System.Windows.Forms.Padding(
                    0,
                    8,
                    0,
                    8);
        }

        /// <summary>
        /// Configura un camp numèric
        /// amb els decimals indicats.
        /// </summary>
        private static void ConfigurarNumeric(
            System.Windows.Forms.NumericUpDown camp,
            int decimals)
        {
            camp.DecimalPlaces =
                decimals;

            camp.Dock =
                System.Windows.Forms.DockStyle.Fill;

            camp.Font =
                new System.Drawing.Font(
                    "Segoe UI",
                    9.5F);

            camp.Increment =
                decimals > 0
                    ? 0.01M
                    : 1M;

            camp.Maximum =
                999999999M;

            camp.Minimum =
                -999999999M;

            camp.Margin =
                new System.Windows.Forms.Padding(
                    0,
                    8,
                    14,
                    8);

            camp.TextAlign =
                System.Windows.Forms.HorizontalAlignment.Right;

            camp.ThousandsSeparator =
                true;
        }

        /// <summary>
        /// Aplica l'estil comú
        /// a les caselles de selecció.
        /// </summary>
        private static void ConfigurarCheckBox(
            System.Windows.Forms.CheckBox casella,
            string text)
        {
            casella.AutoSize =
                true;

            casella.Font =
                new System.Drawing.Font(
                    "Segoe UI",
                    9.5F);

            casella.ForeColor =
                System.Drawing.Color.FromArgb(
                    51,
                    65,
                    85);

            casella.Margin =
                new System.Windows.Forms.Padding(
                    0,
                    8,
                    28,
                    0);

            casella.Text =
                text;

            casella.UseVisualStyleBackColor =
                true;
        }

        /// <summary>
        /// Aplica l'estil comú
        /// als botons inferiors.
        /// </summary>
        private static void ConfigurarBotoAccio(
            System.Windows.Forms.Button boto,
            string text,
            int amplada)
        {
            boto.BackColor =
                System.Drawing.Color.White;

            boto.FlatStyle =
                System.Windows.Forms.FlatStyle.Flat;

            boto.FlatAppearance.BorderColor =
                System.Drawing.Color.FromArgb(
                    203,
                    213,
                    225);

            boto.Font =
                new System.Drawing.Font(
                    "Segoe UI Semibold",
                    9F);

            boto.ForeColor =
                System.Drawing.Color.FromArgb(
                    51,
                    65,
                    85);

            boto.Margin =
                new System.Windows.Forms.Padding(
                    6,
                    0,
                    0,
                    0);

            boto.Size =
                new System.Drawing.Size(
                    amplada,
                    42);

            boto.Text =
                text;

            boto.UseVisualStyleBackColor =
                false;
        }

        #endregion
    }
}