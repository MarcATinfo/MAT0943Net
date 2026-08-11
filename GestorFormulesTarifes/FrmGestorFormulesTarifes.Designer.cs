namespace A3ErpGestorFormulesTarifes
{
    partial class FrmGestorFormulesTarifes
    {
        private System.ComponentModel.IContainer components = null;

        private System.Windows.Forms.TableLayoutPanel tblArrel;

        private System.Windows.Forms.Panel pnlCapcalera;
        private System.Windows.Forms.Label lblTitol;
        private System.Windows.Forms.Label lblSubtitol;

        private System.Windows.Forms.Panel pnlContingut;
        private System.Windows.Forms.DataGridView dgvFormules;

        private System.Windows.Forms.Panel pnlAccions;
        private System.Windows.Forms.Label lblEstat;
        private System.Windows.Forms.FlowLayoutPanel flpAccionsFormules;

        private System.Windows.Forms.Button btnNova;
        private System.Windows.Forms.Button btnEditar;
        private System.Windows.Forms.Button btnDuplicar;
        private System.Windows.Forms.Button btnCanviarEstat;
        private System.Windows.Forms.Button btnEliminar;
        private System.Windows.Forms.Button btnTancar;

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
        /// del gestor de fórmules.
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

            this.dgvFormules =
                new System.Windows.Forms.DataGridView();

            this.pnlAccions =
                new System.Windows.Forms.Panel();

            this.lblEstat =
                new System.Windows.Forms.Label();

            this.flpAccionsFormules =
                new System.Windows.Forms.FlowLayoutPanel();

            this.btnNova =
                new System.Windows.Forms.Button();

            this.btnEditar =
                new System.Windows.Forms.Button();

            this.btnDuplicar =
                new System.Windows.Forms.Button();

            this.btnCanviarEstat =
                new System.Windows.Forms.Button();

            this.btnEliminar =
                new System.Windows.Forms.Button();

            this.btnTancar =
                new System.Windows.Forms.Button();

            ((System.ComponentModel.ISupportInitialize)
                (this.dgvFormules))
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
                    1180,
                    720);

            this.Font =
                new System.Drawing.Font(
                    "Segoe UI",
                    9F);

            this.MinimumSize =
                new System.Drawing.Size(
                    980,
                    600);

            this.StartPosition =
                System.Windows.Forms.FormStartPosition.CenterParent;

            this.Text =
                "Gestor de fórmules de tarifes · a3ERP";

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
                "Gestor de fórmules de tarifes";

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
                "Crea, modifica, activa o elimina les fórmules disponibles al Calculador.";

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
                    20,
                    18,
                    20,
                    16);

            this.dgvFormules.AllowUserToAddRows =
                false;

            this.dgvFormules.AllowUserToDeleteRows =
                false;

            this.dgvFormules.AllowUserToResizeRows =
                false;

            this.dgvFormules.AutoGenerateColumns =
                false;

            this.dgvFormules.BackgroundColor =
                System.Drawing.Color.White;

            this.dgvFormules.BorderStyle =
                System.Windows.Forms.BorderStyle.FixedSingle;

            this.dgvFormules.Dock =
                System.Windows.Forms.DockStyle.Fill;

            this.dgvFormules.MultiSelect =
                false;

            this.dgvFormules.ReadOnly =
                true;

            this.dgvFormules.RowHeadersVisible =
                false;

            this.dgvFormules.SelectionMode =
                System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;

            this.pnlContingut.Controls.Add(
                this.dgvFormules);

            this.tblArrel.Controls.Add(
                this.pnlContingut,
                0,
                1);

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

            /*
             * Text informatiu situat a l'esquerra
             * de la barra inferior.
             */
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
                "Encara no s'han carregat les fórmules.";

            this.lblEstat.TextAlign =
                System.Drawing.ContentAlignment.MiddleLeft;

            /*
             * Contenidor de les accions disponibles
             * sobre el catàleg de fórmules.
             */
            this.flpAccionsFormules.AutoSize =
                true;

            this.flpAccionsFormules.AutoSizeMode =
                System.Windows.Forms.AutoSizeMode.GrowAndShrink;

            this.flpAccionsFormules.Dock =
                System.Windows.Forms.DockStyle.Right;

            this.flpAccionsFormules.FlowDirection =
                System.Windows.Forms.FlowDirection.LeftToRight;

            this.flpAccionsFormules.Margin =
                new System.Windows.Forms.Padding(
                    0);

            this.flpAccionsFormules.Padding =
                new System.Windows.Forms.Padding(
                    0);

            this.flpAccionsFormules.WrapContents =
                false;

            /*
             * Botons de gestió.
             *
             * En aquesta fase només es configura
             * la seva aparença. Els esdeveniments
             * s'afegiran posteriorment.
             */
            ConfigurarBotoAccio(
                this.btnNova,
                "Nova",
                82);

            ConfigurarBotoAccio(
                this.btnEditar,
                "Editar",
                82);

            ConfigurarBotoAccio(
                this.btnDuplicar,
                "Duplicar",
                94);

            ConfigurarBotoAccio(
                this.btnCanviarEstat,
                "Activar / Desactivar",
                145);

            ConfigurarBotoAccio(
                this.btnEliminar,
                "Eliminar",
                88);

            /*
             * El botó d'eliminació utilitza un text
             * diferenciat perquè representa una acció
             * potencialment destructiva.
             */
            this.btnEliminar.ForeColor =
                System.Drawing.Color.FromArgb(
                    185,
                    28,
                    28);

            this.btnEliminar.FlatAppearance.BorderColor =
                System.Drawing.Color.FromArgb(
                    254,
                    202,
                    202);

            /*
             * Botó de tancament.
             */
            ConfigurarBotoAccio(
                this.btnTancar,
                "Tancar",
                96);

            this.btnTancar.DialogResult =
                System.Windows.Forms.DialogResult.Cancel;

            /*
             * Separem visualment el botó Tancar
             * de les accions de gestió.
             */
            this.btnTancar.Margin =
                new System.Windows.Forms.Padding(
                    18,
                    0,
                    0,
                    0);

            /*
             * Ordre visual dels botons.
             */
            this.flpAccionsFormules.Controls.Add(
                this.btnNova);

            this.flpAccionsFormules.Controls.Add(
                this.btnEditar);

            this.flpAccionsFormules.Controls.Add(
                this.btnDuplicar);

            this.flpAccionsFormules.Controls.Add(
                this.btnCanviarEstat);

            this.flpAccionsFormules.Controls.Add(
                this.btnEliminar);

            this.flpAccionsFormules.Controls.Add(
                this.btnTancar);

            this.pnlAccions.Controls.Add(
                this.lblEstat);

            this.pnlAccions.Controls.Add(
                this.flpAccionsFormules);

            this.tblArrel.Controls.Add(
                this.pnlAccions,
                0,
                2);

            /*
             * El botó Tancar respon també
             * a la tecla Esc.
             */
            this.CancelButton =
                this.btnTancar;

            ((System.ComponentModel.ISupportInitialize)
                (this.dgvFormules))
                .EndInit();

            this.ResumeLayout(
                false);
        }

        /// <summary>
        /// Aplica l'estil comú als botons
        /// d'acció del gestor.
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
                    5,
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