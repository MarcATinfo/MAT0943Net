namespace A3ErpCalculadorTarifes
{
    partial class FrmCalculadorTarifes
    {
        private System.ComponentModel.IContainer components = null;

        private System.Windows.Forms.TableLayoutPanel tblArrel;

        private System.Windows.Forms.Panel pnlCapcalera;
        private System.Windows.Forms.Label lblTitol;
        private System.Windows.Forms.Label lblSubtitol;
        private System.Windows.Forms.Label lblMode;

        private System.Windows.Forms.Panel pnlPassos;
        private System.Windows.Forms.TableLayoutPanel tblPassos;

        private System.Windows.Forms.Panel pnlPas1;
        private System.Windows.Forms.Panel pnlPas2;
        private System.Windows.Forms.Panel pnlPas3;

        private System.Windows.Forms.Panel pnlIndicadorPas1;
        private System.Windows.Forms.Panel pnlIndicadorPas2;
        private System.Windows.Forms.Panel pnlIndicadorPas3;

        private System.Windows.Forms.Label lblNumeroPas1;
        private System.Windows.Forms.Label lblNumeroPas2;
        private System.Windows.Forms.Label lblNumeroPas3;

        private System.Windows.Forms.Label lblTextPas1;
        private System.Windows.Forms.Label lblTextPas2;
        private System.Windows.Forms.Label lblTextPas3;

        private System.Windows.Forms.Label lblSubPas1;
        private System.Windows.Forms.Label lblSubPas2;
        private System.Windows.Forms.Label lblSubPas3;

        private System.Windows.Forms.TableLayoutPanel tblContingut;

        private System.Windows.Forms.Panel pnlConfiguracio;
        private System.Windows.Forms.TableLayoutPanel tblConfiguracio;
        private System.Windows.Forms.Label lblTitolConfiguracio;
        private System.Windows.Forms.Label lblSubtitolConfiguracio;
        private System.Windows.Forms.Label lblEtiquetaPlantilla;
        private System.Windows.Forms.ComboBox cmbPlantilla;
        private System.Windows.Forms.Panel pnlDescripcioPlantilla;
        private System.Windows.Forms.Label lblDescripcioPlantilla;
        private System.Windows.Forms.Label lblTipusValors;
        private System.Windows.Forms.TableLayoutPanel tblValorsTarifes;

        private System.Windows.Forms.NumericUpDown numTarifa1;
        private System.Windows.Forms.NumericUpDown numTarifa2;
        private System.Windows.Forms.NumericUpDown numTarifa3;
        private System.Windows.Forms.NumericUpDown numTarifa4;

        private System.Windows.Forms.Label lblTitolDescomptes;
        private System.Windows.Forms.TableLayoutPanel tblDescomptes;

        private System.Windows.Forms.NumericUpDown numDescompte1;
        private System.Windows.Forms.NumericUpDown numDescompte2;
        private System.Windows.Forms.NumericUpDown numDescompte3;
        private System.Windows.Forms.NumericUpDown numDescompte4;

        private System.Windows.Forms.Panel pnlInfoTarifesFixes;
        private System.Windows.Forms.TableLayoutPanel tblInfoTarifesFixes;
        private System.Windows.Forms.Label lblInfoTarifesFixes;
        private System.Windows.Forms.Panel pnlAccionsFormula;
        private System.Windows.Forms.Button btnGestionarFormules;
        private System.Windows.Forms.Button btnRestablirValors;

        private System.Windows.Forms.Panel pnlArticles;
        private System.Windows.Forms.Panel pnlCapcaleraArticles;
        private System.Windows.Forms.Panel pnlCapcaleraArticlesDreta;
        private System.Windows.Forms.Label lblTitolArticles;
        private System.Windows.Forms.Label lblSubtitolArticles;
        private System.Windows.Forms.Label lblComptadorArticles;
        private A3ErpCalculadorTarifes.Controls.ControlCercaArticles controlCercaArticles;
        private System.Windows.Forms.CheckBox chkSeleccionarTots;
        private System.Windows.Forms.DataGridView dgvArticles;

        private System.Windows.Forms.Panel pnlPrevisualitzacio;
        private System.Windows.Forms.Panel pnlCapcaleraPrevisualitzacio;
        private System.Windows.Forms.Label lblTitolPrevisualitzacio;
        private System.Windows.Forms.Label lblSubtitolPrevisualitzacio;
        private System.Windows.Forms.FlowLayoutPanel flpResum;
        private System.Windows.Forms.Label lblResumSeleccionats;
        private System.Windows.Forms.Label lblResumCorrectes;
        private System.Windows.Forms.Label lblResumErrors;
        private System.Windows.Forms.DataGridView dgvPrevisualitzacio;
        private System.Windows.Forms.Label lblNotaPrevisualitzacio;

        private System.Windows.Forms.Panel pnlAccions;
        private System.Windows.Forms.Label lblEstatGeneral;
        private System.Windows.Forms.ProgressBar prgAplicacioTarifes;

        private System.Windows.Forms.Button btnTancar;
        private System.Windows.Forms.Button btnPrevisualitzar;
        private System.Windows.Forms.Button btnAplicar;

        /// <summary>
        /// Allibera els recursos utilitzats pel formulari.
        /// </summary>
        protected override void Dispose(
            bool disposing)
        {
            if (disposing &&
                components != null)
            {
                components.Dispose();
            }

            base.Dispose(disposing);
        }

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

            this.lblMode =
                new System.Windows.Forms.Label();

            this.pnlPassos =
                new System.Windows.Forms.Panel();

            this.tblPassos =
                new System.Windows.Forms.TableLayoutPanel();

            this.pnlPas1 =
                new System.Windows.Forms.Panel();

            this.pnlPas2 =
                new System.Windows.Forms.Panel();

            this.pnlPas3 =
                new System.Windows.Forms.Panel();

            this.pnlIndicadorPas1 =
                new System.Windows.Forms.Panel();

            this.pnlIndicadorPas2 =
                new System.Windows.Forms.Panel();

            this.pnlIndicadorPas3 =
                new System.Windows.Forms.Panel();

            this.lblNumeroPas1 =
                new System.Windows.Forms.Label();

            this.lblNumeroPas2 =
                new System.Windows.Forms.Label();

            this.lblNumeroPas3 =
                new System.Windows.Forms.Label();

            this.lblTextPas1 =
                new System.Windows.Forms.Label();

            this.lblTextPas2 =
                new System.Windows.Forms.Label();

            this.lblTextPas3 =
                new System.Windows.Forms.Label();

            this.lblSubPas1 =
                new System.Windows.Forms.Label();

            this.lblSubPas2 =
                new System.Windows.Forms.Label();

            this.lblSubPas3 =
                new System.Windows.Forms.Label();

            this.tblContingut =
                new System.Windows.Forms.TableLayoutPanel();

            this.pnlConfiguracio =
                new System.Windows.Forms.Panel();

            this.tblConfiguracio =
                new System.Windows.Forms.TableLayoutPanel();

            this.lblTitolConfiguracio =
                new System.Windows.Forms.Label();

            this.lblSubtitolConfiguracio =
                new System.Windows.Forms.Label();

            this.lblEtiquetaPlantilla =
                new System.Windows.Forms.Label();

            this.cmbPlantilla =
                new System.Windows.Forms.ComboBox();

            this.pnlDescripcioPlantilla =
                new System.Windows.Forms.Panel();

            this.lblDescripcioPlantilla =
                new System.Windows.Forms.Label();

            this.lblTipusValors =
                new System.Windows.Forms.Label();

            this.tblValorsTarifes =
                new System.Windows.Forms.TableLayoutPanel();

            this.numTarifa1 =
                new System.Windows.Forms.NumericUpDown();

            this.numTarifa2 =
                new System.Windows.Forms.NumericUpDown();

            this.numTarifa3 =
                new System.Windows.Forms.NumericUpDown();

            this.numTarifa4 =
                new System.Windows.Forms.NumericUpDown();

            this.lblTitolDescomptes =
                new System.Windows.Forms.Label();

            this.tblDescomptes =
                new System.Windows.Forms.TableLayoutPanel();

            this.numDescompte1 =
                new System.Windows.Forms.NumericUpDown();

            this.numDescompte2 =
                new System.Windows.Forms.NumericUpDown();

            this.numDescompte3 =
                new System.Windows.Forms.NumericUpDown();

            this.numDescompte4 =
                new System.Windows.Forms.NumericUpDown();

            this.pnlInfoTarifesFixes =
                new System.Windows.Forms.Panel();

            this.tblInfoTarifesFixes =
                new System.Windows.Forms.TableLayoutPanel();

            this.lblInfoTarifesFixes =
                new System.Windows.Forms.Label();

            this.pnlAccionsFormula =
                new System.Windows.Forms.Panel();

            this.btnGestionarFormules =
                new System.Windows.Forms.Button();

            this.btnRestablirValors =
                new System.Windows.Forms.Button();

            this.pnlArticles =
                new System.Windows.Forms.Panel();

            this.pnlCapcaleraArticles =
                new System.Windows.Forms.Panel();

            this.pnlCapcaleraArticlesDreta =
                new System.Windows.Forms.Panel();

            this.lblTitolArticles =
                new System.Windows.Forms.Label();

            this.lblSubtitolArticles =
                new System.Windows.Forms.Label();

            this.lblComptadorArticles =
                new System.Windows.Forms.Label();

            this.controlCercaArticles =
                new A3ErpCalculadorTarifes.Controls.ControlCercaArticles();

            this.chkSeleccionarTots =
                new System.Windows.Forms.CheckBox();

            this.dgvArticles =
                new System.Windows.Forms.DataGridView();

            this.pnlPrevisualitzacio =
                new System.Windows.Forms.Panel();

            this.pnlCapcaleraPrevisualitzacio =
                new System.Windows.Forms.Panel();

            this.lblTitolPrevisualitzacio =
                new System.Windows.Forms.Label();

            this.lblSubtitolPrevisualitzacio =
                new System.Windows.Forms.Label();

            this.flpResum =
                new System.Windows.Forms.FlowLayoutPanel();

            this.lblResumSeleccionats =
                new System.Windows.Forms.Label();

            this.lblResumCorrectes =
                new System.Windows.Forms.Label();

            this.lblResumErrors =
                new System.Windows.Forms.Label();

            this.dgvPrevisualitzacio =
                new System.Windows.Forms.DataGridView();

            this.lblNotaPrevisualitzacio =
                new System.Windows.Forms.Label();

            this.pnlAccions =
                new System.Windows.Forms.Panel();

            this.lblEstatGeneral =
                new System.Windows.Forms.Label();

            this.prgAplicacioTarifes =
                new System.Windows.Forms.ProgressBar();

            this.btnTancar =
                new System.Windows.Forms.Button();

            this.btnPrevisualitzar =
                new System.Windows.Forms.Button();

            this.btnAplicar =
                new System.Windows.Forms.Button();

            ((System.ComponentModel.ISupportInitialize)
                (this.numTarifa1))
                .BeginInit();

            ((System.ComponentModel.ISupportInitialize)
                (this.numTarifa2))
                .BeginInit();

            ((System.ComponentModel.ISupportInitialize)
                (this.numTarifa3))
                .BeginInit();

            ((System.ComponentModel.ISupportInitialize)
                (this.numTarifa4))
                .BeginInit();

            ((System.ComponentModel.ISupportInitialize)
                (this.numDescompte1))
                .BeginInit();

            ((System.ComponentModel.ISupportInitialize)
                (this.numDescompte2))
                .BeginInit();

            ((System.ComponentModel.ISupportInitialize)
                (this.numDescompte3))
                .BeginInit();

            ((System.ComponentModel.ISupportInitialize)
                (this.numDescompte4))
                .BeginInit();

            ((System.ComponentModel.ISupportInitialize)
                (this.dgvArticles))
                .BeginInit();

            ((System.ComponentModel.ISupportInitialize)
                (this.dgvPrevisualitzacio))
                .BeginInit();

            this.SuspendLayout();

            /*
             * Formulari principal
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
                    1500,
                    920);

            this.Font =
                new System.Drawing.Font(
                    "Segoe UI",
                    9F);

            this.MinimumSize =
                new System.Drawing.Size(
                    1250,
                    780);

            this.StartPosition =
                System.Windows.Forms.FormStartPosition.CenterScreen;

            this.Text =
                "Calculador de tarifes · a3ERP";

            this.WindowState =
                System.Windows.Forms.FormWindowState.Maximized;

            /*
             * Estructura general
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
                4;

            this.tblArrel.RowStyles.Add(
                new System.Windows.Forms.RowStyle(
                    System.Windows.Forms.SizeType.Absolute,
                    96F));

            this.tblArrel.RowStyles.Add(
                new System.Windows.Forms.RowStyle(
                    System.Windows.Forms.SizeType.Absolute,
                    68F));

            this.tblArrel.RowStyles.Add(
                new System.Windows.Forms.RowStyle(
                    System.Windows.Forms.SizeType.Percent,
                    100F));

            this.tblArrel.RowStyles.Add(
                new System.Windows.Forms.RowStyle(
                    System.Windows.Forms.SizeType.Absolute,
                    78F));

            this.Controls.Add(
                this.tblArrel);

            /*
             * Capçalera
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
                    18,
                    28,
                    14);

            this.lblTitol.AutoSize =
                true;

            this.lblTitol.Font =
                new System.Drawing.Font(
                    "Segoe UI Semibold",
                    22F);

            this.lblTitol.ForeColor =
                System.Drawing.Color.White;

            this.lblTitol.Location =
                new System.Drawing.Point(
                    28,
                    15);

            this.lblTitol.Text =
                "Calculador de tarifes";

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
                    57);

            this.lblSubtitol.Text =
                "Calcula, compara i previsualitza les tarifes abans d'aplicar-les a a3ERP.";

            this.lblMode.Anchor =
                System.Windows.Forms.AnchorStyles.Top
                | System.Windows.Forms.AnchorStyles.Right;

            this.lblMode.AutoSize =
                true;

            this.lblMode.BackColor =
                System.Drawing.Color.FromArgb(
                    30,
                    41,
                    59);

            this.lblMode.Font =
                new System.Drawing.Font(
                    "Segoe UI Semibold",
                    9F);

            this.lblMode.ForeColor =
                System.Drawing.Color.FromArgb(
                    125,
                    211,
                    252);

            this.lblMode.Padding =
                new System.Windows.Forms.Padding(
                    12,
                    7,
                    12,
                    7);

            this.lblMode.Location =
                new System.Drawing.Point(
                    1160,
                    27);

            this.lblMode.Text =
                "MODE DEMOSTRACIÓ · Sense gravació";

            this.pnlCapcalera.Controls.Add(
                this.lblTitol);

            this.pnlCapcalera.Controls.Add(
                this.lblSubtitol);

            this.pnlCapcalera.Controls.Add(
                this.lblMode);

            this.tblArrel.Controls.Add(
                this.pnlCapcalera,
                0,
                0);

            /*
             * Passos visuals
             */
            this.pnlPassos.BackColor =
                System.Drawing.Color.White;

            this.pnlPassos.Dock =
                System.Windows.Forms.DockStyle.Fill;

            this.pnlPassos.Padding =
                new System.Windows.Forms.Padding(
                    24,
                    8,
                    24,
                    8);

            this.tblPassos.ColumnCount =
                3;

            this.tblPassos.ColumnStyles.Add(
                new System.Windows.Forms.ColumnStyle(
                    System.Windows.Forms.SizeType.Percent,
                    33.333F));

            this.tblPassos.ColumnStyles.Add(
                new System.Windows.Forms.ColumnStyle(
                    System.Windows.Forms.SizeType.Percent,
                    33.333F));

            this.tblPassos.ColumnStyles.Add(
                new System.Windows.Forms.ColumnStyle(
                    System.Windows.Forms.SizeType.Percent,
                    33.334F));

            this.tblPassos.Dock =
                System.Windows.Forms.DockStyle.Fill;

            this.tblPassos.RowCount =
                1;

            this.tblPassos.RowStyles.Add(
                new System.Windows.Forms.RowStyle(
                    System.Windows.Forms.SizeType.Percent,
                    100F));

            ConfigurarPas(
                this.pnlPas1,
                this.pnlIndicadorPas1,
                this.lblNumeroPas1,
                this.lblTextPas1,
                this.lblSubPas1,
                "1",
                "Articles",
                "Selecciona els registres");

            ConfigurarPas(
                this.pnlPas2,
                this.pnlIndicadorPas2,
                this.lblNumeroPas2,
                this.lblTextPas2,
                this.lblSubPas2,
                "2",
                "Càlcul",
                "Escull fórmula i valors");

            ConfigurarPas(
                this.pnlPas3,
                this.pnlIndicadorPas3,
                this.lblNumeroPas3,
                this.lblTextPas3,
                this.lblSubPas3,
                "3",
                "Aplicació",
                "Revisa abans de guardar");

            this.tblPassos.Controls.Add(
                this.pnlPas1,
                0,
                0);

            this.tblPassos.Controls.Add(
                this.pnlPas2,
                1,
                0);

            this.tblPassos.Controls.Add(
                this.pnlPas3,
                2,
                0);

            this.pnlPassos.Controls.Add(
                this.tblPassos);

            this.tblArrel.Controls.Add(
                this.pnlPassos,
                0,
                1);

            /*
             * Contingut:
             *
             * Columna esquerra:
             * Fórmula de càlcul ocupant les dues files.
             *
             * Columna dreta:
             * Articles a sobre i previsualització a sota.
             */
            this.tblContingut.BackColor =
                System.Drawing.Color.FromArgb(
                    245,
                    247,
                    250);

            this.tblContingut.ColumnCount =
                2;

            this.tblContingut.ColumnStyles.Add(
                new System.Windows.Forms.ColumnStyle(
                    System.Windows.Forms.SizeType.Percent,
                    31F));

            this.tblContingut.ColumnStyles.Add(
                new System.Windows.Forms.ColumnStyle(
                    System.Windows.Forms.SizeType.Percent,
                    69F));

            this.tblContingut.Dock =
                System.Windows.Forms.DockStyle.Fill;

            this.tblContingut.Padding =
                new System.Windows.Forms.Padding(
                    18,
                    14,
                    18,
                    12);

            this.tblContingut.RowCount =
                2;

            this.tblContingut.RowStyles.Add(
                new System.Windows.Forms.RowStyle(
                    System.Windows.Forms.SizeType.Percent,
                    52F));

            this.tblContingut.RowStyles.Add(
                new System.Windows.Forms.RowStyle(
                    System.Windows.Forms.SizeType.Percent,
                    48F));

            this.tblArrel.Controls.Add(
                this.tblContingut,
                0,
                2);

            /*
             * Fórmula de càlcul
             */
            this.pnlConfiguracio.AutoScroll =
                true;

            this.pnlConfiguracio.BackColor =
                System.Drawing.Color.White;

            this.pnlConfiguracio.BorderStyle =
                System.Windows.Forms.BorderStyle.FixedSingle;

            this.pnlConfiguracio.Dock =
                System.Windows.Forms.DockStyle.Fill;

            this.pnlConfiguracio.Margin =
                new System.Windows.Forms.Padding(
                    0,
                    0,
                    12,
                    0);

            this.pnlConfiguracio.Padding =
                new System.Windows.Forms.Padding(
                    18,
                    14,
                    18,
                    14);

            this.tblConfiguracio.ColumnCount =
                1;

            this.tblConfiguracio.ColumnStyles.Add(
                new System.Windows.Forms.ColumnStyle(
                    System.Windows.Forms.SizeType.Percent,
                    100F));

            this.tblConfiguracio.Dock =
                System.Windows.Forms.DockStyle.Top;

            this.tblConfiguracio.RowCount =
                10;

            this.tblConfiguracio.RowStyles.Add(
                new System.Windows.Forms.RowStyle(
                    System.Windows.Forms.SizeType.Absolute,
                    34F));

            this.tblConfiguracio.RowStyles.Add(
                new System.Windows.Forms.RowStyle(
                    System.Windows.Forms.SizeType.Absolute,
                    28F));

            this.tblConfiguracio.RowStyles.Add(
                new System.Windows.Forms.RowStyle(
                    System.Windows.Forms.SizeType.Absolute,
                    56F));

            /*
             * Reduïm el quadre blau de descripció.
             */
            this.tblConfiguracio.RowStyles.Add(
                new System.Windows.Forms.RowStyle(
                    System.Windows.Forms.SizeType.Absolute,
                    52F));

            this.tblConfiguracio.RowStyles.Add(
                new System.Windows.Forms.RowStyle(
                    System.Windows.Forms.SizeType.Absolute,
                    30F));

            /*
             * Donem més aire a les targetes de tarifes.
             */
            this.tblConfiguracio.RowStyles.Add(
                new System.Windows.Forms.RowStyle(
                    System.Windows.Forms.SizeType.Absolute,
                    78F));

            this.tblConfiguracio.RowStyles.Add(
                new System.Windows.Forms.RowStyle(
                    System.Windows.Forms.SizeType.Absolute,
                    30F));

            /*
             * Donem més aire a les targetes de descomptes.
             */
            this.tblConfiguracio.RowStyles.Add(
                new System.Windows.Forms.RowStyle(
                    System.Windows.Forms.SizeType.Absolute,
                    78F));

            this.tblConfiguracio.RowStyles.Add(
                new System.Windows.Forms.RowStyle(
                    System.Windows.Forms.SizeType.Absolute,
                    46F));

            this.tblConfiguracio.RowStyles.Add(
                new System.Windows.Forms.RowStyle(
                    System.Windows.Forms.SizeType.Absolute,
                    46F));

            this.tblConfiguracio.Height =
                478;

            /*
             * Fila del quadre verd de tarifes 5 i 6.
             */
            this.tblConfiguracio.RowStyles[8].SizeType =
                System.Windows.Forms.SizeType.Absolute;

            this.tblConfiguracio.RowStyles[8].Height =
                46F;

            this.lblTitolConfiguracio.AutoSize =
                true;

            this.lblTitolConfiguracio.Font =
                new System.Drawing.Font(
                    "Segoe UI Semibold",
                    15F);

            this.lblTitolConfiguracio.ForeColor =
                System.Drawing.Color.FromArgb(
                    15,
                    23,
                    42);

            this.lblTitolConfiguracio.Text =
                "Fórmula de càlcul";

            this.lblSubtitolConfiguracio.AutoSize =
                true;

            this.lblSubtitolConfiguracio.ForeColor =
                System.Drawing.Color.FromArgb(
                    100,
                    116,
                    139);

            this.lblSubtitolConfiguracio.Text =
                "Tots els valors es poden modificar abans de calcular.";

            this.lblEtiquetaPlantilla.AutoSize =
                true;

            this.lblEtiquetaPlantilla.Dock =
                System.Windows.Forms.DockStyle.Top;

            this.lblEtiquetaPlantilla.Font =
                new System.Drawing.Font(
                    "Segoe UI Semibold",
                    9F);

            this.lblEtiquetaPlantilla.ForeColor =
                System.Drawing.Color.FromArgb(
                    51,
                    65,
                    85);

            this.lblEtiquetaPlantilla.Height =
                26;

            this.lblEtiquetaPlantilla.Text =
                "Plantilla";

            this.cmbPlantilla.Dock =
                System.Windows.Forms.DockStyle.Fill;

            this.cmbPlantilla.DropDownStyle =
                System.Windows.Forms.ComboBoxStyle.DropDownList;

            this.cmbPlantilla.Font =
                new System.Drawing.Font(
                    "Segoe UI",
                    10F);

            this.pnlDescripcioPlantilla.BackColor =
                System.Drawing.Color.FromArgb(
                    239,
                    246,
                    255);

            this.pnlDescripcioPlantilla.Dock =
                System.Windows.Forms.DockStyle.Fill;

            this.pnlDescripcioPlantilla.Padding =
                new System.Windows.Forms.Padding(
                    10,
                    7,
                    10,
                    5);

            this.lblDescripcioPlantilla.Dock =
                System.Windows.Forms.DockStyle.Fill;

            this.lblDescripcioPlantilla.ForeColor =
                System.Drawing.Color.FromArgb(
                    30,
                    64,
                    175);

            this.lblDescripcioPlantilla.Text =
                "Selecciona una plantilla per veure'n la descripció.";

            this.pnlDescripcioPlantilla.Controls.Add(
                this.lblDescripcioPlantilla);

            this.lblTipusValors.AutoSize =
                true;

            this.lblTipusValors.Dock =
                System.Windows.Forms.DockStyle.Bottom;

            this.lblTipusValors.Font =
                new System.Drawing.Font(
                    "Segoe UI Semibold",
                    9F);

            this.lblTipusValors.ForeColor =
                System.Drawing.Color.FromArgb(
                    51,
                    65,
                    85);

            this.lblTipusValors.Text =
                "Paràmetres de les tarifes 1–4";

            this.tblValorsTarifes.ColumnCount =
                4;

            this.tblValorsTarifes.ColumnStyles.Add(
                new System.Windows.Forms.ColumnStyle(
                    System.Windows.Forms.SizeType.Percent,
                    25F));

            this.tblValorsTarifes.ColumnStyles.Add(
                new System.Windows.Forms.ColumnStyle(
                    System.Windows.Forms.SizeType.Percent,
                    25F));

            this.tblValorsTarifes.ColumnStyles.Add(
                new System.Windows.Forms.ColumnStyle(
                    System.Windows.Forms.SizeType.Percent,
                    25F));

            this.tblValorsTarifes.ColumnStyles.Add(
                new System.Windows.Forms.ColumnStyle(
                    System.Windows.Forms.SizeType.Percent,
                    25F));

            this.tblValorsTarifes.Dock =
                System.Windows.Forms.DockStyle.Fill;

            ConfigurarNumeric(
                this.numTarifa1);

            ConfigurarNumeric(
                this.numTarifa2);

            ConfigurarNumeric(
                this.numTarifa3);

            ConfigurarNumeric(
                this.numTarifa4);

            this.tblValorsTarifes.Controls.Add(
                CrearTargetaValor(
                    "Tarifa 1",
                    this.numTarifa1),
                0,
                0);

            this.tblValorsTarifes.Controls.Add(
                CrearTargetaValor(
                    "Tarifa 2",
                    this.numTarifa2),
                1,
                0);

            this.tblValorsTarifes.Controls.Add(
                CrearTargetaValor(
                    "Tarifa 3",
                    this.numTarifa3),
                2,
                0);

            this.tblValorsTarifes.Controls.Add(
                CrearTargetaValor(
                    "Tarifa 4",
                    this.numTarifa4),
                3,
                0);

            this.lblTitolDescomptes.AutoSize =
                true;

            this.lblTitolDescomptes.Dock =
                System.Windows.Forms.DockStyle.Bottom;

            this.lblTitolDescomptes.Font =
                new System.Drawing.Font(
                    "Segoe UI Semibold",
                    9F);

            this.lblTitolDescomptes.ForeColor =
                System.Drawing.Color.FromArgb(
                    51,
                    65,
                    85);

            this.lblTitolDescomptes.Text =
                "Descomptes per grup de client";

            this.tblDescomptes.ColumnCount =
                4;

            this.tblDescomptes.ColumnStyles.Add(
                new System.Windows.Forms.ColumnStyle(
                    System.Windows.Forms.SizeType.Percent,
                    25F));

            this.tblDescomptes.ColumnStyles.Add(
                new System.Windows.Forms.ColumnStyle(
                    System.Windows.Forms.SizeType.Percent,
                    25F));

            this.tblDescomptes.ColumnStyles.Add(
                new System.Windows.Forms.ColumnStyle(
                    System.Windows.Forms.SizeType.Percent,
                    25F));

            this.tblDescomptes.ColumnStyles.Add(
                new System.Windows.Forms.ColumnStyle(
                    System.Windows.Forms.SizeType.Percent,
                    25F));

            this.tblDescomptes.Dock =
                System.Windows.Forms.DockStyle.Fill;

            ConfigurarNumeric(
                this.numDescompte1);

            ConfigurarNumeric(
                this.numDescompte2);

            ConfigurarNumeric(
                this.numDescompte3);

            ConfigurarNumeric(
                this.numDescompte4);

            this.tblDescomptes.Controls.Add(
                CrearTargetaValor(
                    "Grup 1 %",
                    this.numDescompte1),
                0,
                0);

            this.tblDescomptes.Controls.Add(
                CrearTargetaValor(
                    "Grup 2 %",
                    this.numDescompte2),
                1,
                0);

            this.tblDescomptes.Controls.Add(
                CrearTargetaValor(
                    "Grup 3 %",
                    this.numDescompte3),
                2,
                0);

            this.tblDescomptes.Controls.Add(
                CrearTargetaValor(
                    "Grup 4 %",
                    this.numDescompte4),
                3,
                0);

            this.pnlInfoTarifesFixes.BackColor =
                System.Drawing.Color.FromArgb(
                    240,
                    253,
                    250);

            this.pnlInfoTarifesFixes.Dock =
                System.Windows.Forms.DockStyle.Fill;

            this.pnlInfoTarifesFixes.Padding =
                new System.Windows.Forms.Padding(
                    10,
                    5,
                    10,
                    4);

            this.tblInfoTarifesFixes.ColumnCount =
                1;

            this.tblInfoTarifesFixes.ColumnStyles.Add(
                new System.Windows.Forms.ColumnStyle(
                    System.Windows.Forms.SizeType.Percent,
                    100F));

            this.tblInfoTarifesFixes.Dock =
                System.Windows.Forms.DockStyle.Fill;

            this.tblInfoTarifesFixes.Margin =
                new System.Windows.Forms.Padding(
                    0);

            this.tblInfoTarifesFixes.Padding =
                new System.Windows.Forms.Padding(
                    0);

            this.tblInfoTarifesFixes.RowCount =
                1;

            this.tblInfoTarifesFixes.RowStyles.Add(
                new System.Windows.Forms.RowStyle(
                    System.Windows.Forms.SizeType.Percent,
                    100F));

            this.lblInfoTarifesFixes.Dock =
                System.Windows.Forms.DockStyle.Fill;

            this.lblInfoTarifesFixes.AutoSize =
                false;

            this.lblInfoTarifesFixes.Font =
                new System.Drawing.Font(
                    "Segoe UI Semibold",
                    9F);

            this.lblInfoTarifesFixes.ForeColor =
                System.Drawing.Color.FromArgb(
                    4,
                    120,
                    87);

            this.lblInfoTarifesFixes.Margin =
                new System.Windows.Forms.Padding(
                    0,
                    0,
                    0,
                    0);

            this.lblInfoTarifesFixes.Text =
                "Tarifa 5 = PRCCOSTE\r\nTarifa 6 = PRCCOSTE + PRCSTANDARD";

            this.tblInfoTarifesFixes.Controls.Add(
                this.lblInfoTarifesFixes,
                0,
                0);

            this.pnlInfoTarifesFixes.Controls.Add(
                this.tblInfoTarifesFixes);

            /*
 * Accions relacionades amb la fórmula seleccionada.
 *
 * El gestor queda a l'esquerra i el restabliment
 * dels valors actuals queda a la dreta.
 */
            this.pnlAccionsFormula.Dock =
                System.Windows.Forms.DockStyle.Fill;

            this.pnlAccionsFormula.Margin =
                new System.Windows.Forms.Padding(
                    0);

            this.pnlAccionsFormula.Padding =
                new System.Windows.Forms.Padding(
                    0,
                    5,
                    0,
                    5);

            this.btnGestionarFormules.BackColor =
                System.Drawing.Color.White;

            this.btnGestionarFormules.Dock =
                System.Windows.Forms.DockStyle.Left;

            this.btnGestionarFormules.FlatStyle =
                System.Windows.Forms.FlatStyle.Flat;

            this.btnGestionarFormules.FlatAppearance.BorderColor =
                System.Drawing.Color.FromArgb(
                    203,
                    213,
                    225);

            this.btnGestionarFormules.Font =
                new System.Drawing.Font(
                    "Segoe UI Semibold",
                    9F);

            this.btnGestionarFormules.ForeColor =
                System.Drawing.Color.FromArgb(
                    51,
                    65,
                    85);

            this.btnGestionarFormules.Size =
                new System.Drawing.Size(
                    180,
                    36);

            this.btnGestionarFormules.Text =
                "Gestionar fórmules...";

            this.btnGestionarFormules.UseVisualStyleBackColor =
                false;

            this.btnRestablirValors.Dock =
                System.Windows.Forms.DockStyle.Right;

            this.btnRestablirValors.FlatStyle =
                System.Windows.Forms.FlatStyle.Flat;

            this.btnRestablirValors.FlatAppearance.BorderColor =
                System.Drawing.Color.FromArgb(
                    203,
                    213,
                    225);

            this.btnRestablirValors.Font =
                new System.Drawing.Font(
                    "Segoe UI Semibold",
                    9F);

            this.btnRestablirValors.Size =
                new System.Drawing.Size(
                    190,
                    36);

            this.btnRestablirValors.Text =
                "Restablir valors inicials";

            this.tblConfiguracio.Controls.Add(
                this.lblTitolConfiguracio,
                0,
                0);

            this.tblConfiguracio.Controls.Add(
                this.lblSubtitolConfiguracio,
                0,
                1);

            this.tblConfiguracio.Controls.Add(
                CrearBlocPlantilla(),
                0,
                2);

            this.tblConfiguracio.Controls.Add(
                this.pnlDescripcioPlantilla,
                0,
                3);

            this.tblConfiguracio.Controls.Add(
                this.lblTipusValors,
                0,
                4);

            this.tblConfiguracio.Controls.Add(
                this.tblValorsTarifes,
                0,
                5);

            this.tblConfiguracio.Controls.Add(
                this.lblTitolDescomptes,
                0,
                6);

            this.tblConfiguracio.Controls.Add(
                this.tblDescomptes,
                0,
                7);

            this.tblConfiguracio.Controls.Add(
                this.pnlInfoTarifesFixes,
                0,
                8);

            this.tblConfiguracio.Controls.Add(
                this.pnlAccionsFormula,
                0,
                9);

            this.pnlAccionsFormula.Controls.Add(
                this.btnGestionarFormules);

            this.pnlAccionsFormula.Controls.Add(
                this.btnRestablirValors);

            this.pnlConfiguracio.Controls.Add(
                this.tblConfiguracio);

            this.tblContingut.Controls.Add(
                this.pnlConfiguracio,
                0,
                0);

            this.tblContingut.SetRowSpan(
                this.pnlConfiguracio,
                2);

            /*
             * Articles
             */
            this.pnlArticles.BackColor =
                System.Drawing.Color.White;

            this.pnlArticles.BorderStyle =
                System.Windows.Forms.BorderStyle.FixedSingle;

            this.pnlArticles.Dock =
                System.Windows.Forms.DockStyle.Fill;

            this.pnlArticles.Margin =
                new System.Windows.Forms.Padding(
                    0,
                    0,
                    0,
                    6);

            this.pnlCapcaleraArticles.BackColor =
                System.Drawing.Color.White;

            this.pnlCapcaleraArticles.Dock =
                System.Windows.Forms.DockStyle.Top;

            this.pnlCapcaleraArticles.Height =
                84;

            this.pnlCapcaleraArticles.Padding =
                new System.Windows.Forms.Padding(
                    18,
                    13,
                    18,
                    8);

            this.pnlCapcaleraArticlesDreta.BackColor =
                System.Drawing.Color.White;

            this.pnlCapcaleraArticlesDreta.Dock =
                System.Windows.Forms.DockStyle.Right;

            this.pnlCapcaleraArticlesDreta.Location =
                new System.Drawing.Point(
                    446,
                    13);

            this.pnlCapcaleraArticlesDreta.Size =
                new System.Drawing.Size(
                    540,
                    63);

            this.lblTitolArticles.AutoSize =
                true;

            this.lblTitolArticles.Font =
                new System.Drawing.Font(
                    "Segoe UI Semibold",
                    15F);

            this.lblTitolArticles.ForeColor =
                System.Drawing.Color.FromArgb(
                    15,
                    23,
                    42);

            this.lblTitolArticles.Location =
                new System.Drawing.Point(
                    17,
                    10);

            this.lblTitolArticles.Text =
                "Articles";

            this.lblSubtitolArticles.AutoSize =
                true;

            this.lblSubtitolArticles.ForeColor =
                System.Drawing.Color.FromArgb(
                    100,
                    116,
                    139);

            this.lblSubtitolArticles.Location =
                new System.Drawing.Point(
                    20,
                    43);

            this.lblSubtitolArticles.Text =
                "Selecciona els articles que vols incloure en el càlcul.";

            this.lblComptadorArticles.Anchor =
                System.Windows.Forms.AnchorStyles.Top
                | System.Windows.Forms.AnchorStyles.Left
                | System.Windows.Forms.AnchorStyles.Right;

            this.lblComptadorArticles.AutoSize =
                false;

            this.lblComptadorArticles.Font =
                new System.Drawing.Font(
                    "Segoe UI Semibold",
                    9F);

            this.lblComptadorArticles.ForeColor =
                System.Drawing.Color.FromArgb(
                    14,
                    116,
                    144);

            this.lblComptadorArticles.Location =
                new System.Drawing.Point(
                    0,
                    42);

            this.lblComptadorArticles.Size =
                new System.Drawing.Size(
                    540,
                    18);

            this.lblComptadorArticles.TextAlign =
                System.Drawing.ContentAlignment.MiddleRight;

            this.lblComptadorArticles.Text =
                "Mostrats: 0 de 0 · Seleccionats: 0";

            this.controlCercaArticles.Anchor =
                System.Windows.Forms.AnchorStyles.Top
                | System.Windows.Forms.AnchorStyles.Left
                | System.Windows.Forms.AnchorStyles.Right;

            this.controlCercaArticles.BackColor =
                System.Drawing.Color.White;

            this.controlCercaArticles.Location =
                new System.Drawing.Point(
                    0,
                    0);

            this.controlCercaArticles.Margin =
                new System.Windows.Forms.Padding(
                    0);

            this.controlCercaArticles.Name =
                "controlCercaArticles";

            this.controlCercaArticles.Size =
                new System.Drawing.Size(
                    540,
                    34);

            this.controlCercaArticles.TabIndex =
                0;

            this.chkSeleccionarTots.Anchor =
                System.Windows.Forms.AnchorStyles.Top
                | System.Windows.Forms.AnchorStyles.Right;

            this.chkSeleccionarTots.AutoSize =
                true;

            this.chkSeleccionarTots.Font =
                new System.Drawing.Font(
                    "Segoe UI Semibold",
                    9F);

            this.chkSeleccionarTots.Location =
                new System.Drawing.Point(
                    648,
                    44);

            this.chkSeleccionarTots.Text =
                "Seleccionar tots";

            this.chkSeleccionarTots.ThreeState =
                true;

            this.pnlCapcaleraArticles.Controls.Add(
                this.lblTitolArticles);

            this.pnlCapcaleraArticles.Controls.Add(
                this.lblSubtitolArticles);

            this.pnlCapcaleraArticlesDreta.Controls.Add(
                this.lblComptadorArticles);

            this.pnlCapcaleraArticlesDreta.Controls.Add(
                this.controlCercaArticles);

            this.pnlCapcaleraArticles.Controls.Add(
                this.pnlCapcaleraArticlesDreta);

            this.pnlCapcaleraArticles.Controls.Add(
                this.chkSeleccionarTots);

            this.dgvArticles.BackgroundColor =
                System.Drawing.Color.White;

            this.dgvArticles.BorderStyle =
                System.Windows.Forms.BorderStyle.None;

            this.dgvArticles.Dock =
                System.Windows.Forms.DockStyle.Fill;

            this.pnlArticles.Controls.Add(
                this.dgvArticles);

            this.pnlArticles.Controls.Add(
                this.pnlCapcaleraArticles);

            this.tblContingut.Controls.Add(
                this.pnlArticles,
                1,
                0);

            /*
             * Previsualització
             */
            this.pnlPrevisualitzacio.BackColor =
                System.Drawing.Color.White;

            this.pnlPrevisualitzacio.BorderStyle =
                System.Windows.Forms.BorderStyle.FixedSingle;

            this.pnlPrevisualitzacio.Dock =
                System.Windows.Forms.DockStyle.Fill;

            this.pnlPrevisualitzacio.Margin =
                new System.Windows.Forms.Padding(
                    0,
                    6,
                    0,
                    0);

            this.pnlPrevisualitzacio.MinimumSize =
                new System.Drawing.Size(
                    0,
                    260);

            this.pnlCapcaleraPrevisualitzacio.BackColor =
                System.Drawing.Color.White;

            this.pnlCapcaleraPrevisualitzacio.Dock =
                System.Windows.Forms.DockStyle.Top;

            this.pnlCapcaleraPrevisualitzacio.Height =
                68;

            this.pnlCapcaleraPrevisualitzacio.Padding =
                new System.Windows.Forms.Padding(
                    18,
                    10,
                    18,
                    8);

            this.lblTitolPrevisualitzacio.AutoSize =
                true;

            this.lblTitolPrevisualitzacio.Font =
                new System.Drawing.Font(
                    "Segoe UI Semibold",
                    15F);

            this.lblTitolPrevisualitzacio.ForeColor =
                System.Drawing.Color.FromArgb(
                    15,
                    23,
                    42);

            this.lblTitolPrevisualitzacio.Location =
                new System.Drawing.Point(
                    17,
                    8);

            this.lblTitolPrevisualitzacio.Text =
                "Previsualització";

            this.lblSubtitolPrevisualitzacio.AutoSize =
                true;

            this.lblSubtitolPrevisualitzacio.ForeColor =
                System.Drawing.Color.FromArgb(
                    100,
                    116,
                    139);

            this.lblSubtitolPrevisualitzacio.Location =
                new System.Drawing.Point(
                    20,
                    39);

            this.lblSubtitolPrevisualitzacio.Text =
                "Cap dada es guardarà fins que confirmis l'aplicació.";

            this.flpResum.Anchor =
                System.Windows.Forms.AnchorStyles.Top
                | System.Windows.Forms.AnchorStyles.Right;

            this.flpResum.AutoSize =
                true;

            this.flpResum.FlowDirection =
                System.Windows.Forms.FlowDirection.LeftToRight;

            this.flpResum.Location =
                new System.Drawing.Point(
                    700,
                    17);

            ConfigurarEtiquetaResum(
                this.lblResumSeleccionats,
                "Seleccionats: 0",
                System.Drawing.Color.FromArgb(
                    14,
                    116,
                    144));

            ConfigurarEtiquetaResum(
                this.lblResumCorrectes,
                "Correctes: 0",
                System.Drawing.Color.FromArgb(
                    5,
                    150,
                    105));

            ConfigurarEtiquetaResum(
                this.lblResumErrors,
                "Errors: 0",
                System.Drawing.Color.FromArgb(
                    220,
                    38,
                    38));

            this.flpResum.Controls.Add(
                this.lblResumSeleccionats);

            this.flpResum.Controls.Add(
                this.lblResumCorrectes);

            this.flpResum.Controls.Add(
                this.lblResumErrors);

            this.pnlCapcaleraPrevisualitzacio.Controls.Add(
                this.lblTitolPrevisualitzacio);

            this.pnlCapcaleraPrevisualitzacio.Controls.Add(
                this.lblSubtitolPrevisualitzacio);

            this.pnlCapcaleraPrevisualitzacio.Controls.Add(
                this.flpResum);

            this.dgvPrevisualitzacio.BackgroundColor =
                System.Drawing.Color.White;

            this.dgvPrevisualitzacio.BorderStyle =
                System.Windows.Forms.BorderStyle.None;

            this.dgvPrevisualitzacio.Dock =
                System.Windows.Forms.DockStyle.Fill;

            this.lblNotaPrevisualitzacio.BackColor =
                System.Drawing.Color.FromArgb(
                    248,
                    250,
                    252);

            this.lblNotaPrevisualitzacio.Dock =
                System.Windows.Forms.DockStyle.Bottom;

            this.lblNotaPrevisualitzacio.ForeColor =
                System.Drawing.Color.FromArgb(
                    100,
                    116,
                    139);

            this.lblNotaPrevisualitzacio.Height =
                34;

            this.lblNotaPrevisualitzacio.Padding =
                new System.Windows.Forms.Padding(
                    14,
                    8,
                    0,
                    0);

            this.lblNotaPrevisualitzacio.Text =
                "Les tarifes 5 i 6 es calculen sempre a partir del cost i el transport.";

            this.pnlPrevisualitzacio.Controls.Add(
                this.dgvPrevisualitzacio);

            this.pnlPrevisualitzacio.Controls.Add(
                this.lblNotaPrevisualitzacio);

            this.pnlPrevisualitzacio.Controls.Add(
                this.pnlCapcaleraPrevisualitzacio);

            this.tblContingut.Controls.Add(
                this.pnlPrevisualitzacio,
                1,
                1);

            /*
             * Barra inferior
             */
            this.pnlAccions.BackColor =
                System.Drawing.Color.White;

            this.pnlAccions.Dock =
                System.Windows.Forms.DockStyle.Fill;

            this.pnlAccions.Padding =
                new System.Windows.Forms.Padding(
                    24,
                    17,
                    24,
                    15);

            this.lblEstatGeneral.AutoSize =
                false;

            this.lblEstatGeneral.Anchor =
                System.Windows.Forms.AnchorStyles.Left
                | System.Windows.Forms.AnchorStyles.Top;

            this.lblEstatGeneral.Font =
                new System.Drawing.Font(
                    "Segoe UI",
                    9.5F);

            this.lblEstatGeneral.ForeColor =
                System.Drawing.Color.FromArgb(
                    71,
                    85,
                    105);

            this.lblEstatGeneral.TextAlign =
                System.Drawing.ContentAlignment.MiddleLeft;

            this.lblEstatGeneral.Location =
                new System.Drawing.Point(
                    24,
                    13);

            this.lblEstatGeneral.Size =
                new System.Drawing.Size(
                    620,
                    25);

            this.lblEstatGeneral.Text =
                "Selecciona articles, revisa la plantilla i genera una previsualització.";

            this.lblEstatGeneral.Margin =
                new System.Windows.Forms.Padding(0);

            this.prgAplicacioTarifes.Anchor =
                System.Windows.Forms.AnchorStyles.Left
                | System.Windows.Forms.AnchorStyles.Top;

            this.prgAplicacioTarifes.Minimum =
                0;

            this.prgAplicacioTarifes.Maximum =
                100;

            this.prgAplicacioTarifes.Value =
                0;

            this.prgAplicacioTarifes.Style =
                System.Windows.Forms.ProgressBarStyle.Continuous;

            this.prgAplicacioTarifes.Location =
                new System.Drawing.Point(
                    24,
                    44);

            this.prgAplicacioTarifes.Margin =
                new System.Windows.Forms.Padding(0);

            this.prgAplicacioTarifes.Size =
                new System.Drawing.Size(
                    560,
                    12);

            ConfigurarBoto(
                this.btnTancar,
                "Tancar",
                System.Drawing.Color.White,
                System.Drawing.Color.FromArgb(
                    51,
                    65,
                    85),
                110);

            this.btnTancar.Dock =
                System.Windows.Forms.DockStyle.Right;

            ConfigurarBoto(
                this.btnAplicar,
                "Aplicar tarifes",
                System.Drawing.Color.FromArgb(
                    5,
                    150,
                    105),
                System.Drawing.Color.White,
                170);

            this.btnAplicar.Dock =
                System.Windows.Forms.DockStyle.Right;

            this.btnAplicar.Enabled =
                false;

            ConfigurarBoto(
                this.btnPrevisualitzar,
                "Previsualitzar càlcul",
                System.Drawing.Color.FromArgb(
                    14,
                    165,
                    233),
                System.Drawing.Color.White,
                190);

            this.btnPrevisualitzar.Dock =
                System.Windows.Forms.DockStyle.Right;

            this.pnlAccions.Controls.Add(
                this.lblEstatGeneral);

            this.pnlAccions.Controls.Add(
                this.prgAplicacioTarifes);

            this.pnlAccions.Controls.Add(
                this.btnTancar);

            this.pnlAccions.Controls.Add(
                this.btnAplicar);

            this.pnlAccions.Controls.Add(
                this.btnPrevisualitzar);

            this.tblArrel.Controls.Add(
                this.pnlAccions,
                0,
                3);

            ((System.ComponentModel.ISupportInitialize)
                (this.numTarifa1))
                .EndInit();

            ((System.ComponentModel.ISupportInitialize)
                (this.numTarifa2))
                .EndInit();

            ((System.ComponentModel.ISupportInitialize)
                (this.numTarifa3))
                .EndInit();

            ((System.ComponentModel.ISupportInitialize)
                (this.numTarifa4))
                .EndInit();

            ((System.ComponentModel.ISupportInitialize)
                (this.numDescompte1))
                .EndInit();

            ((System.ComponentModel.ISupportInitialize)
                (this.numDescompte2))
                .EndInit();

            ((System.ComponentModel.ISupportInitialize)
                (this.numDescompte3))
                .EndInit();

            ((System.ComponentModel.ISupportInitialize)
                (this.numDescompte4))
                .EndInit();

            ((System.ComponentModel.ISupportInitialize)
                (this.dgvArticles))
                .EndInit();

            ((System.ComponentModel.ISupportInitialize)
                (this.dgvPrevisualitzacio))
                .EndInit();

            this.ResumeLayout(
                false);
        }

        /// <summary>
        /// Crea el bloc format per l'etiqueta
        /// i el selector de plantilla.
        /// </summary>
        private System.Windows.Forms.Panel CrearBlocPlantilla()
        {
            System.Windows.Forms.Panel panel =
                new System.Windows.Forms.Panel();

            panel.Dock =
                System.Windows.Forms.DockStyle.Fill;

            panel.Padding =
                new System.Windows.Forms.Padding(
                    0,
                    4,
                    0,
                    6);

            this.cmbPlantilla.Dock =
                System.Windows.Forms.DockStyle.Fill;

            panel.Controls.Add(
                this.cmbPlantilla);

            panel.Controls.Add(
                this.lblEtiquetaPlantilla);

            return panel;
        }

        /// <summary>
        /// Configura un camp numèric editable.
        /// </summary>
        private static void ConfigurarNumeric(
            System.Windows.Forms.NumericUpDown control)
        {
            control.DecimalPlaces =
                4;

            control.Dock =
                System.Windows.Forms.DockStyle.Fill;

            control.Font =
                new System.Drawing.Font(
                    "Segoe UI Semibold",
                    10F);

            control.Increment =
                0.05M;

            control.Maximum =
                100000M;

            control.Minimum =
                -100000M;

            control.TextAlign =
                System.Windows.Forms.HorizontalAlignment.Right;
            control.BorderStyle =
                System.Windows.Forms.BorderStyle.FixedSingle;

            control.ThousandsSeparator =
                true;
        }

        /// <summary>
        /// Crea una targeta compacta per a un valor editable.
        ///
        /// El títol i el camp numèric es mantenen pròxims,
        /// evitant espais verticals innecessaris.
        /// </summary>
        private static System.Windows.Forms.Panel
            CrearTargetaValor(
                string titol,
                System.Windows.Forms.NumericUpDown control)
        {
            System.Windows.Forms.Panel panel =
                new System.Windows.Forms.Panel();

            System.Windows.Forms.TableLayoutPanel disposicio =
                new System.Windows.Forms.TableLayoutPanel();

            System.Windows.Forms.Label label =
                new System.Windows.Forms.Label();

            panel.BackColor =
                System.Drawing.Color.FromArgb(
                    248,
                    250,
                    252);

            panel.Dock =
                System.Windows.Forms.DockStyle.Fill;

            panel.Margin =
                new System.Windows.Forms.Padding(
                    4,
                    3,
                    4,
                    4);

            panel.Padding =
                new System.Windows.Forms.Padding(
                    9,
                    6,
                    9,
                    6);

            /*
             * La disposició interna evita que el camp
             * quedi enganxat a la part inferior.
             */
            disposicio.ColumnCount =
                1;

            disposicio.ColumnStyles.Add(
                new System.Windows.Forms.ColumnStyle(
                    System.Windows.Forms.SizeType.Percent,
                    100F));

            disposicio.RowCount =
                2;

            disposicio.RowStyles.Add(
                new System.Windows.Forms.RowStyle(
                    System.Windows.Forms.SizeType.Absolute,
                    24F));

            disposicio.RowStyles.Add(
                new System.Windows.Forms.RowStyle(
                    System.Windows.Forms.SizeType.Absolute,
                    30F));

            disposicio.Dock =
                System.Windows.Forms.DockStyle.Fill;

            disposicio.Margin =
                new System.Windows.Forms.Padding(0);

            disposicio.Padding =
                new System.Windows.Forms.Padding(0);

            label.Dock =
                System.Windows.Forms.DockStyle.Fill;

            label.Font =
                new System.Drawing.Font(
                    "Segoe UI Semibold",
                    8.5F);

            label.ForeColor =
                System.Drawing.Color.FromArgb(
                    71,
                    85,
                    105);

            label.Margin =
                new System.Windows.Forms.Padding(0);

            label.Text =
                titol;

            label.TextAlign =
                System.Drawing.ContentAlignment.MiddleLeft;

            control.Dock =
                System.Windows.Forms.DockStyle.Top;

            control.Height =
                27;

            control.Margin =
                new System.Windows.Forms.Padding(
                    0,
                    2,
                    0,
                    0);

            disposicio.Controls.Add(
                label,
                0,
                0);

            disposicio.Controls.Add(
                control,
                0,
                1);

            panel.Controls.Add(
                disposicio);

            return panel;
        }

        /// <summary>
        /// Configura un pas visual de la capçalera.
        /// </summary>
        private static void ConfigurarPas(
            System.Windows.Forms.Panel panel,
            System.Windows.Forms.Panel indicador,
            System.Windows.Forms.Label numero,
            System.Windows.Forms.Label titol,
            System.Windows.Forms.Label subtitol,
            string textNumero,
            string textTitol,
            string textSubtitol)
        {
            panel.BackColor =
                System.Drawing.Color.White;

            panel.Dock =
                System.Windows.Forms.DockStyle.Fill;

            panel.Margin =
                new System.Windows.Forms.Padding(
                    6,
                    0,
                    6,
                    0);

            indicador.BackColor =
                System.Drawing.Color.FromArgb(
                    226,
                    232,
                    240);

            indicador.Location =
                new System.Drawing.Point(
                    8,
                    7);

            indicador.Size =
                new System.Drawing.Size(
                    42,
                    42);

            numero.Dock =
                System.Windows.Forms.DockStyle.Fill;

            numero.Font =
                new System.Drawing.Font(
                    "Segoe UI Semibold",
                    12F);

            numero.ForeColor =
                System.Drawing.Color.FromArgb(
                    71,
                    85,
                    105);

            numero.Text =
                textNumero;

            numero.TextAlign =
                System.Drawing.ContentAlignment.MiddleCenter;

            indicador.Controls.Add(
                numero);

            titol.AutoSize =
                true;

            titol.Font =
                new System.Drawing.Font(
                    "Segoe UI Semibold",
                    10F);

            titol.ForeColor =
                System.Drawing.Color.FromArgb(
                    30,
                    41,
                    59);

            titol.Location =
                new System.Drawing.Point(
                    61,
                    8);

            titol.Text =
                textTitol;

            subtitol.AutoSize =
                true;

            subtitol.Font =
                new System.Drawing.Font(
                    "Segoe UI",
                    8.5F);

            subtitol.ForeColor =
                System.Drawing.Color.FromArgb(
                    100,
                    116,
                    139);

            subtitol.Location =
                new System.Drawing.Point(
                    61,
                    29);

            subtitol.Text =
                textSubtitol;

            panel.Controls.Add(
                indicador);

            panel.Controls.Add(
                titol);

            panel.Controls.Add(
                subtitol);
        }

        /// <summary>
        /// Configura una etiqueta de resum.
        /// </summary>
        private static void ConfigurarEtiquetaResum(
            System.Windows.Forms.Label label,
            string text,
            System.Drawing.Color colorText)
        {
            label.AutoSize =
                true;

            label.BackColor =
                System.Drawing.Color.FromArgb(
                    248,
                    250,
                    252);

            label.Font =
                new System.Drawing.Font(
                    "Segoe UI Semibold",
                    8.5F);

            label.ForeColor =
                colorText;

            label.Margin =
                new System.Windows.Forms.Padding(
                    4,
                    3,
                    4,
                    3);

            label.Padding =
                new System.Windows.Forms.Padding(
                    10,
                    6,
                    10,
                    6);

            label.Text =
                text;
        }

        /// <summary>
        /// Configura l'estil general d'un botó.
        /// </summary>
        private static void ConfigurarBoto(
            System.Windows.Forms.Button boto,
            string text,
            System.Drawing.Color colorFons,
            System.Drawing.Color colorText,
            int amplada)
        {
            boto.BackColor =
                colorFons;

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
                    9.5F);

            boto.ForeColor =
                colorText;

            boto.Margin =
                new System.Windows.Forms.Padding(
                    6,
                    0,
                    0,
                    0);

            boto.Size =
                new System.Drawing.Size(
                    amplada,
                    44);

            boto.Text =
                text;

            boto.UseVisualStyleBackColor =
                false;
        }
    }
}
