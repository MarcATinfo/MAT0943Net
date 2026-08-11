using A3ErpGestorFormulesTarifes.Models;
using A3ErpGestorFormulesTarifes.Validacio;
using MAT0943Net.Infrastructure;
using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace A3ErpGestorFormulesTarifes
{
    /// <summary>
    /// Formulari utilitzat per crear,
    /// editar o duplicar una fórmula.
    /// </summary>
    public partial class FrmEdicioFormulaTarifa : Form
    {
        private readonly ModeEdicioFormulaTarifa
            _mode;

        private readonly FormulaTarifa
            _formulaEdicio;

        private readonly ValidadorFormulaTarifa
            _validador;

        /// <summary>
        /// Retorna la còpia de la fórmula editada
        /// quan l'usuari accepta les dades validades.
        ///
        /// El repositori i el formulari principal
        /// són responsables de persistir-la.
        /// </summary>
        public FormulaTarifa FormulaResultat
        {
            get
            {
                return _formulaEdicio;
            }
        }

        /// <summary>
        /// Constructor necessari per al
        /// dissenyador visual de WinForms.
        /// </summary>
        public FrmEdicioFormulaTarifa()
            : this(
                ModeEdicioFormulaTarifa.Nova,
                new FormulaTarifa())
        {
        }

        /// <summary>
        /// Inicialitza el formulari segons
        /// l'operació sol·licitada.
        /// </summary>
        public FrmEdicioFormulaTarifa(
            ModeEdicioFormulaTarifa mode,
            FormulaTarifa formula)
        {
            if (formula == null)
            {
                throw new ArgumentNullException(
                    nameof(formula));
            }

            _mode =
                mode;

            _formulaEdicio =
                ClonarFormula(
                    formula);

            _validador =
                new ValidadorFormulaTarifa();

            PrepararFormulaSegonsMode();

            InitializeComponent();

            IconaAplicacio.Aplicar(
                this);

            AplicarRenderitzatText(
                this);

            ConfigurarTipusValors();

            ConfigurarModeVisual();

            CarregarControlsDesDeFormula();

            ConnectarEsdeveniments();

            ActualitzarEstatControls();
        }

        /// <summary>
        /// Prepara la còpia de treball segons
        /// si es crea, edita o duplica.
        /// </summary>
        private void PrepararFormulaSegonsMode()
        {
            switch (_mode)
            {
                case ModeEdicioFormulaTarifa.Nova:
                    PrepararFormulaNova();

                    break;

                case ModeEdicioFormulaTarifa.Editar:
                    /*
                     * En mode edició es conserva
                     * tota la informació original.
                     */
                    break;

                case ModeEdicioFormulaTarifa.Duplicar:
                    PrepararFormulaDuplicada();

                    break;

                default:
                    throw new ArgumentOutOfRangeException(
                        nameof(_mode),
                        _mode,
                        "El mode d'edició no és vàlid.");
            }
        }

        /// <summary>
        /// Aplica els valors inicials necessaris
        /// per crear una fórmula nova.
        /// </summary>
        private void PrepararFormulaNova()
        {
            _formulaEdicio.Id =
                0;

            if (_formulaEdicio.Ordre <= 0)
            {
                _formulaEdicio.Ordre =
                    1;
            }

            _formulaEdicio.Activa =
                true;

            _formulaEdicio.Eliminada =
                false;

            _formulaEdicio.DataAlta =
                null;

            _formulaEdicio.DataActualitzacio =
                null;

            _formulaEdicio.DataEliminacio =
                null;
        }

        /// <summary>
        /// Prepara una còpia independent
        /// d'una fórmula existent.
        ///
        /// El codi queda buit perquè l'usuari
        /// n'hagi d'informar un de nou.
        /// </summary>
        private void PrepararFormulaDuplicada()
        {
            _formulaEdicio.Id =
                0;

            _formulaEdicio.Codi =
                string.Empty;

            _formulaEdicio.Nom =
                string.IsNullOrWhiteSpace(
                    _formulaEdicio.Nom)
                    ? string.Empty
                    : _formulaEdicio.Nom.Trim()
                        + " (còpia)";

            _formulaEdicio.Ordre =
                Math.Max(
                    1,
                    _formulaEdicio.Ordre + 1);

            _formulaEdicio.Eliminada =
                false;

            _formulaEdicio.DataAlta =
                null;

            _formulaEdicio.DataActualitzacio =
                null;

            _formulaEdicio.DataEliminacio =
                null;
        }

        /// <summary>
        /// Configura els textos del formulari
        /// segons l'operació actual.
        /// </summary>
        private void ConfigurarModeVisual()
        {
            switch (_mode)
            {
                case ModeEdicioFormulaTarifa.Nova:
                    lblTitol.Text =
                        "Nova fórmula de tarifes";

                    lblSubtitol.Text =
                        "Defineix les dades de la nova fórmula.";

                    Text =
                        "Nova fórmula de tarifes · a3ERP";

                    break;

                case ModeEdicioFormulaTarifa.Editar:
                    lblTitol.Text =
                        "Editar fórmula de tarifes";

                    lblSubtitol.Text =
                        "Modifica les dades i expressions "
                        + "de la fórmula seleccionada.";

                    Text =
                        "Editar fórmula de tarifes · a3ERP";

                    break;

                case ModeEdicioFormulaTarifa.Duplicar:
                    lblTitol.Text =
                        "Duplicar fórmula de tarifes";

                    lblSubtitol.Text =
                        "Revisa la còpia i informa un codi "
                        + "nou abans de desar-la.";

                    Text =
                        "Duplicar fórmula de tarifes · a3ERP";

                    break;
            }
        }

        /// <summary>
        /// Configura les opcions disponibles
        /// al selector de tipus de valors.
        ///
        /// Si una fórmula existent conté
        /// un tipus diferent, també es conserva.
        /// </summary>
        private void ConfigurarTipusValors()
        {
            var tipus =
                new List<string>
                {
                    "No aplica",
                    "Coeficients divisors",
                    "Percentatges d'increment",
                    "Imports fixos"
                };

            if (!string.IsNullOrWhiteSpace(
                    _formulaEdicio.TipusValors)
                && !tipus.Contains(
                    _formulaEdicio.TipusValors))
            {
                tipus.Add(
                    _formulaEdicio.TipusValors);
            }

            cboTipusValors.DataSource =
                tipus;
        }

        /// <summary>
        /// Traspassa les dades de la fórmula
        /// als controls del formulari.
        /// </summary>
        private void CarregarControlsDesDeFormula()
        {
            txtCodi.Text =
                _formulaEdicio.Codi;

            txtNom.Text =
                _formulaEdicio.Nom;

            txtDescripcio.Text =
                _formulaEdicio.Descripcio;

            AssignarValorNumeric(
                nudOrdre,
                _formulaEdicio.Ordre);

            chkActiva.Checked =
                _formulaEdicio.Activa;

            chkUtilitzaValorsTarifes.Checked =
                _formulaEdicio.UtilitzaValorsTarifes;

            SeleccionarTipusValors(
                _formulaEdicio.TipusValors);

            AssignarValorNumeric(
                nudValorP1,
                _formulaEdicio.ValorInicialP1);

            AssignarValorNumeric(
                nudValorP2,
                _formulaEdicio.ValorInicialP2);

            AssignarValorNumeric(
                nudValorP3,
                _formulaEdicio.ValorInicialP3);

            AssignarValorNumeric(
                nudValorP4,
                _formulaEdicio.ValorInicialP4);

            txtExpressioTarifa1.Text =
                _formulaEdicio.ExpressioTarifa1;

            txtExpressioTarifa2.Text =
                _formulaEdicio.ExpressioTarifa2;

            txtExpressioTarifa3.Text =
                _formulaEdicio.ExpressioTarifa3;

            txtExpressioTarifa4.Text =
                _formulaEdicio.ExpressioTarifa4;

            txtExpressioTarifa5.Text =
                _formulaEdicio.ExpressioTarifa5;

            txtExpressioTarifa6.Text =
                _formulaEdicio.ExpressioTarifa6;

            chkGeneraDescomptes.Checked =
                _formulaEdicio.GeneraDescomptes;

            AssignarValorNumeric(
                nudDescompteGrup1,
                _formulaEdicio.DescompteInicialGrup1);

            AssignarValorNumeric(
                nudDescompteGrup2,
                _formulaEdicio.DescompteInicialGrup2);

            AssignarValorNumeric(
                nudDescompteGrup3,
                _formulaEdicio.DescompteInicialGrup3);

            AssignarValorNumeric(
                nudDescompteGrup4,
                _formulaEdicio.DescompteInicialGrup4);
        }

        /// <summary>
        /// Connecta els esdeveniments que afecten
        /// l'estat visual dels controls.
        /// </summary>
        private void ConnectarEsdeveniments()
        {
            chkUtilitzaValorsTarifes.CheckedChanged +=
                chkUtilitzaValorsTarifes_CheckedChanged;

            chkGeneraDescomptes.CheckedChanged +=
                chkGeneraDescomptes_CheckedChanged;

            btnDesar.Click +=
                btnDesar_Click;
        }

        /// <summary>
        /// Actualitza els controls quan canvia
        /// l'ús dels paràmetres P1–P4.
        /// </summary>
        private void chkUtilitzaValorsTarifes_CheckedChanged(
            object sender,
            EventArgs e)
        {
            ActualitzarEstatControls();
        }

        /// <summary>
        /// Actualitza els controls quan canvia
        /// la generació de descomptes.
        /// </summary>
        private void chkGeneraDescomptes_CheckedChanged(
            object sender,
            EventArgs e)
        {
            ActualitzarEstatControls();
        }

        /// <summary>
        /// Habilita o deshabilita els camps
        /// dependents de les opcions generals.
        ///
        /// Els valors no s'esborren quan
        /// els controls queden deshabilitats.
        /// </summary>
        private void ActualitzarEstatControls()
        {
            bool utilitzaValors =
                chkUtilitzaValorsTarifes.Checked;

            nudValorP1.Enabled =
                utilitzaValors;

            nudValorP2.Enabled =
                utilitzaValors;

            nudValorP3.Enabled =
                utilitzaValors;

            nudValorP4.Enabled =
                utilitzaValors;

            bool generaDescomptes =
                chkGeneraDescomptes.Checked;

            nudDescompteGrup1.Enabled =
                generaDescomptes;

            nudDescompteGrup2.Enabled =
                generaDescomptes;

            nudDescompteGrup3.Enabled =
                generaDescomptes;

            nudDescompteGrup4.Enabled =
                generaDescomptes;
        }

        /// <summary>
        /// Selecciona el tipus de valors
        /// de la fórmula actual.
        /// </summary>
        private void SeleccionarTipusValors(
            string tipusValors)
        {
            string valorSeleccionar =
                string.IsNullOrWhiteSpace(
                    tipusValors)
                    ? "No aplica"
                    : tipusValors;

            int index =
                cboTipusValors.FindStringExact(
                    valorSeleccionar);

            cboTipusValors.SelectedIndex =
                index >= 0
                    ? index
                    : 0;
        }

        /// <summary>
        /// Assigna un decimal a un NumericUpDown
        /// respectant-ne els límits configurats.
        /// </summary>
        private static void AssignarValorNumeric(
            NumericUpDown camp,
            decimal valor)
        {
            if (valor < camp.Minimum)
            {
                camp.Value =
                    camp.Minimum;

                return;
            }

            if (valor > camp.Maximum)
            {
                camp.Value =
                    camp.Maximum;

                return;
            }

            camp.Value =
                valor;
        }

        /// <summary>
        /// Crea una còpia independent
        /// de totes les dades de la fórmula.
        ///
        /// Això evita modificar la fórmula
        /// de la graella abans de desar.
        /// </summary>
        private static FormulaTarifa ClonarFormula(
            FormulaTarifa origen)
        {
            return new FormulaTarifa
            {
                Id =
                    origen.Id,

                Codi =
                    origen.Codi,

                Nom =
                    origen.Nom,

                Descripcio =
                    origen.Descripcio,

                Ordre =
                    origen.Ordre,

                Activa =
                    origen.Activa,

                Eliminada =
                    origen.Eliminada,

                TipusValors =
                    origen.TipusValors,

                UtilitzaValorsTarifes =
                    origen.UtilitzaValorsTarifes,

                ValorInicialP1 =
                    origen.ValorInicialP1,

                ValorInicialP2 =
                    origen.ValorInicialP2,

                ValorInicialP3 =
                    origen.ValorInicialP3,

                ValorInicialP4 =
                    origen.ValorInicialP4,

                ExpressioTarifa1 =
                    origen.ExpressioTarifa1,

                ExpressioTarifa2 =
                    origen.ExpressioTarifa2,

                ExpressioTarifa3 =
                    origen.ExpressioTarifa3,

                ExpressioTarifa4 =
                    origen.ExpressioTarifa4,

                ExpressioTarifa5 =
                    origen.ExpressioTarifa5,

                ExpressioTarifa6 =
                    origen.ExpressioTarifa6,

                GeneraDescomptes =
                    origen.GeneraDescomptes,

                DescompteInicialGrup1 =
                    origen.DescompteInicialGrup1,

                DescompteInicialGrup2 =
                    origen.DescompteInicialGrup2,

                DescompteInicialGrup3 =
                    origen.DescompteInicialGrup3,

                DescompteInicialGrup4 =
                    origen.DescompteInicialGrup4,

                DataAlta =
                    origen.DataAlta,

                DataActualitzacio =
                    origen.DataActualitzacio,

                DataEliminacio =
                    origen.DataEliminacio
            };
        }

        /// <summary>
        /// Evita problemes de renderitzat de text
        /// quan el formulari s'obre dins d'a3ERP.
        /// </summary>
        private static void AplicarRenderitzatText(
            Control control)
        {
            Label etiqueta =
                control as Label;

            if (etiqueta != null)
            {
                etiqueta.UseCompatibleTextRendering =
                    false;
            }

            ButtonBase boto =
                control as ButtonBase;

            if (boto != null)
            {
                boto.UseCompatibleTextRendering =
                    false;
            }

            foreach (
                Control fill
                in control.Controls)
            {
                AplicarRenderitzatText(
                    fill);
            }
        }

        /// <summary>
        /// Recull les dades introduïdes, valida
        /// la fórmula i tanca el formulari
        /// únicament quan el resultat és correcte.
        ///
        /// En aquesta fase no s'escriu encara
        /// cap informació a la base de dades.
        /// </summary>
        private void btnDesar_Click(
            object sender,
            EventArgs e)
        {
            TraspassarControlsAFormula();

            ResultatValidacioFormula resultat =
                _validador.Validar(
                    _formulaEdicio);

            if (!resultat.EsValid)
            {
                SeleccionarPestanyaAmbErrors(
                    resultat);

                lblEstat.Text =
                    "Revisa les dades indicades abans de desar.";

                MessageBox.Show(
                    this,
                    "No es pot desar la fórmula perquè "
                    + "s'han detectat les incidències següents:"
                    + "\r\n\r\n"
                    + resultat.ObtenirMissatge(),
                    "Dades de la fórmula incompletes",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);

                return;
            }

            lblEstat.Text =
                "Les dades de la fórmula són correctes.";

            DialogResult =
                DialogResult.OK;

            Close();
        }

        /// <summary>
        /// Copia els valors actuals dels controls
        /// a la fórmula de treball.
        ///
        /// Els textos s'emmagatzemen sense espais
        /// innecessaris al principi o al final.
        /// </summary>
        private void TraspassarControlsAFormula()
        {
            _formulaEdicio.Codi =
                NormalitzarText(
                    txtCodi.Text);

            _formulaEdicio.Nom =
                NormalitzarText(
                    txtNom.Text);

            _formulaEdicio.Descripcio =
                NormalitzarText(
                    txtDescripcio.Text);

            _formulaEdicio.Ordre =
                Decimal.ToInt32(
                    nudOrdre.Value);

            _formulaEdicio.TipusValors =
                cboTipusValors.SelectedItem == null
                    ? string.Empty
                    : NormalitzarText(
                        cboTipusValors.SelectedItem.ToString());

            _formulaEdicio.Activa =
                chkActiva.Checked;

            _formulaEdicio.UtilitzaValorsTarifes =
                chkUtilitzaValorsTarifes.Checked;

            _formulaEdicio.ValorInicialP1 =
                nudValorP1.Value;

            _formulaEdicio.ValorInicialP2 =
                nudValorP2.Value;

            _formulaEdicio.ValorInicialP3 =
                nudValorP3.Value;

            _formulaEdicio.ValorInicialP4 =
                nudValorP4.Value;

            _formulaEdicio.ExpressioTarifa1 =
                NormalitzarText(
                    txtExpressioTarifa1.Text);

            _formulaEdicio.ExpressioTarifa2 =
                NormalitzarText(
                    txtExpressioTarifa2.Text);

            _formulaEdicio.ExpressioTarifa3 =
                NormalitzarText(
                    txtExpressioTarifa3.Text);

            _formulaEdicio.ExpressioTarifa4 =
                NormalitzarText(
                    txtExpressioTarifa4.Text);

            _formulaEdicio.ExpressioTarifa5 =
                NormalitzarText(
                    txtExpressioTarifa5.Text);

            _formulaEdicio.ExpressioTarifa6 =
                NormalitzarText(
                    txtExpressioTarifa6.Text);

            _formulaEdicio.GeneraDescomptes =
                chkGeneraDescomptes.Checked;

            _formulaEdicio.DescompteInicialGrup1 =
                nudDescompteGrup1.Value;

            _formulaEdicio.DescompteInicialGrup2 =
                nudDescompteGrup2.Value;

            _formulaEdicio.DescompteInicialGrup3 =
                nudDescompteGrup3.Value;

            _formulaEdicio.DescompteInicialGrup4 =
                nudDescompteGrup4.Value;
        }

        /// <summary>
        /// Porta l'usuari a la primera pestanya
        /// que conté dades obligatòries incorrectes.
        /// </summary>
        private void SeleccionarPestanyaAmbErrors(
            ResultatValidacioFormula resultat)
        {
            if (resultat == null)
            {
                return;
            }

            /*
             * Primer comprovem les dades generals.
             */
            if (_formulaEdicio.Ordre <= 0
                || string.IsNullOrWhiteSpace(
                    _formulaEdicio.Codi)
                || string.IsNullOrWhiteSpace(
                    _formulaEdicio.Nom)
                || string.IsNullOrWhiteSpace(
                    _formulaEdicio.TipusValors))
            {
                tabPrincipal.SelectedTab =
                    tabGeneral;

                return;
            }

            /*
             * Després comprovem les sis expressions.
             */
            if (string.IsNullOrWhiteSpace(
                    _formulaEdicio.ExpressioTarifa1)
                || string.IsNullOrWhiteSpace(
                    _formulaEdicio.ExpressioTarifa2)
                || string.IsNullOrWhiteSpace(
                    _formulaEdicio.ExpressioTarifa3)
                || string.IsNullOrWhiteSpace(
                    _formulaEdicio.ExpressioTarifa4)
                || string.IsNullOrWhiteSpace(
                    _formulaEdicio.ExpressioTarifa5)
                || string.IsNullOrWhiteSpace(
                    _formulaEdicio.ExpressioTarifa6))
            {
                tabPrincipal.SelectedTab =
                    tabTarifes;

                return;
            }

            /*
             * Qualsevol altra incoherència d'estat
             * es mostra inicialment a la pestanya General.
             */
            tabPrincipal.SelectedTab =
                tabGeneral;
        }

        /// <summary>
        /// Retorna un text sense espais
        /// al principi ni al final.
        ///
        /// Els valors nuls es converteixen
        /// en una cadena buida.
        /// </summary>
        private static string NormalitzarText(
            string valor)
        {
            return string.IsNullOrWhiteSpace(
                valor)
                ? string.Empty
                : valor.Trim();
        }
    }
}
