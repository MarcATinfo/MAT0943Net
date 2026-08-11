using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace A3ErpGestorFormulesTarifes.Models
{
    /// <summary>
    /// Representa una fórmula configurable
    /// utilitzada pel Calculador de tarifes.
    ///
    /// Aquesta classe només conté dades.
    /// No accedeix a la base de dades,
    /// no valida expressions i no executa càlculs.
    /// </summary>
    public sealed class FormulaTarifa
    {
        #region Identificació

        /// <summary>
        /// Identificador únic de la fórmula
        /// a la base de dades.
        ///
        /// El valor zero indica que la fórmula
        /// encara no ha estat desada.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Codi intern i estable de la fórmula.
        ///
        /// Permet identificar-la encara que
        /// l'usuari modifiqui el nom o l'ordre.
        ///
        /// No és un text destinat a mostrar-se
        /// directament a l'usuari.
        /// </summary>
        public string Codi { get; set; }

        /// <summary>
        /// Nom descriptiu que es mostrarà
        /// al gestor i al Calculador.
        /// </summary>
        public string Nom { get; set; }

        /// <summary>
        /// Explicació funcional del criteri
        /// de càlcul de la fórmula.
        /// </summary>
        public string Descripcio { get; set; }

        #endregion

        #region Configuració funcional

        /// <summary>
        /// Posició en què la fórmula es mostrarà
        /// al gestor i al desplegable del Calculador.
        ///
        /// L'ordre és independent del nom.
        /// Això permet reordenar les fórmules
        /// sense modificar-ne el text visible.
        /// </summary>
        public int Ordre { get; set; }

        /// <summary>
        /// Indica si la fórmula està disponible
        /// per utilitzar-la al Calculador.
        ///
        /// Una fórmula inactiva es conserva
        /// a la base de dades, però no apareix
        /// entre les opcions de càlcul disponibles.
        /// </summary>
        public bool Activa { get; set; }

        /// <summary>
        /// Indica si la fórmula ha estat eliminada
        /// de manera lògica.
        ///
        /// Una fórmula eliminada no apareix al Calculador
        /// ni al llistat habitual del gestor, però es conserva
        /// a la base de dades perquè es pugui recuperar.
        ///
        /// L'eliminació lògica evita perdre configuracions
        /// accidentalment i manté la traçabilitat del registre.
        /// </summary>
        public bool Eliminada { get; set; }

        /// <summary>
        /// Text que explica què representen
        /// els quatre valors editables.
        ///
        /// Exemples:
        /// - Coeficients divisors
        /// - Percentatges d'increment
        /// - Imports fixos
        /// - No aplica
        /// </summary>
        public string TipusValors { get; set; }

        /// <summary>
        /// Indica si la fórmula utilitza
        /// els quatre valors editables P1-P4.
        ///
        /// Aquesta propietat permetrà habilitar
        /// o deshabilitar els controls corresponents
        /// al Calculador.
        /// </summary>
        public bool UtilitzaValorsTarifes { get; set; }

        #endregion

        #region Valors inicials dels paràmetres

        /// <summary>
        /// Valor inicial del paràmetre P1.
        ///
        /// Segons la fórmula, pot representar
        /// un coeficient, un percentatge
        /// o un import fix.
        /// </summary>
        public decimal ValorInicialP1 { get; set; }

        /// <summary>
        /// Valor inicial del paràmetre P2.
        /// </summary>
        public decimal ValorInicialP2 { get; set; }

        /// <summary>
        /// Valor inicial del paràmetre P3.
        /// </summary>
        public decimal ValorInicialP3 { get; set; }

        /// <summary>
        /// Valor inicial del paràmetre P4.
        /// </summary>
        public decimal ValorInicialP4 { get; set; }

        #endregion

        #region Expressions de càlcul

        /// <summary>
        /// Expressió utilitzada per calcular la Tarifa 1.
        ///
        /// Pot utilitzar els camps i paràmetres
        /// autoritzats pel futur motor d'expressions.
        ///
        /// Exemple:
        /// PRCCOSTE / P1
        /// </summary>
        public string ExpressioTarifa1 { get; set; }

        /// <summary>
        /// Expressió utilitzada per calcular la Tarifa 2.
        ///
        /// Exemple:
        /// PRCCOSTE / P2
        /// </summary>
        public string ExpressioTarifa2 { get; set; }

        /// <summary>
        /// Expressió utilitzada per calcular la Tarifa 3.
        ///
        /// Exemple:
        /// PRCCOSTE / P3
        /// </summary>
        public string ExpressioTarifa3 { get; set; }

        /// <summary>
        /// Expressió utilitzada per calcular la Tarifa 4.
        ///
        /// Exemple:
        /// PRCCOSTE / P4
        /// </summary>
        public string ExpressioTarifa4 { get; set; }

        /// <summary>
        /// Expressió utilitzada per calcular la Tarifa 5.
        ///
        /// En les fórmules actuals equival a:
        /// PRCCOSTE
        ///
        /// Es guarda com a expressió configurable
        /// perquè el client pugui modificar-ne el criteri
        /// sense recompilar l'aplicació.
        /// </summary>
        public string ExpressioTarifa5 { get; set; }

        /// <summary>
        /// Expressió utilitzada per calcular la Tarifa 6.
        ///
        /// En les fórmules actuals equival a:
        /// PRCCOSTE + PRCSTANDARD
        ///
        /// Es guarda com a expressió configurable
        /// perquè el client pugui modificar-ne el criteri
        /// sense recompilar l'aplicació.
        /// </summary>
        public string ExpressioTarifa6 { get; set; }

        #endregion

        #region Configuració dels descomptes

        /// <summary>
        /// Indica si la fórmula genera descomptes
        /// per família de client.
        ///
        /// Quan és false, el Calculador només ha
        /// d'actualitzar les tarifes i no ha de
        /// modificar els descomptes existents.
        /// </summary>
        public bool GeneraDescomptes { get; set; }

        /// <summary>
        /// Descompte inicial aplicable
        /// a la família de client 1.
        /// </summary>
        public decimal DescompteInicialGrup1 { get; set; }

        /// <summary>
        /// Descompte inicial aplicable
        /// a la família de client 2.
        /// </summary>
        public decimal DescompteInicialGrup2 { get; set; }

        /// <summary>
        /// Descompte inicial aplicable
        /// a la família de client 3.
        /// </summary>
        public decimal DescompteInicialGrup3 { get; set; }

        /// <summary>
        /// Descompte inicial aplicable
        /// a la família de client 4.
        /// </summary>
        public decimal DescompteInicialGrup4 { get; set; }

        #endregion

        #region Auditoria

        /// <summary>
        /// Data en què la fórmula va ser eliminada
        /// de manera lògica.
        ///
        /// És nul·la mentre la fórmula no estigui eliminada.
        /// Si la fórmula es recupera, tornarà a ser nul·la.
        /// </summary>
        public DateTime? DataEliminacio { get; set; }

        /// <summary>
        /// Data en què la fórmula es va crear
        /// a la base de dades.
        ///
        /// És nul·la mentre la fórmula encara
        /// no hagi estat desada.
        /// </summary>
        public DateTime? DataAlta { get; set; }

        /// <summary>
        /// Data de l'última modificació
        /// de la fórmula.
        ///
        /// És nul·la si la fórmula encara
        /// no ha estat modificada després de crear-la.
        /// </summary>
        public DateTime? DataActualitzacio { get; set; }

        #endregion

        /// <summary>
        /// Text utilitzat pels selectors de fórmules.
        /// </summary>
        public string TextSelector
        {
            get
            {
                return Ordre
                    + " · "
                    + Nom;
            }
        }

        /// <summary>
        /// Inicialitza els textos per evitar
        /// valors nuls en fórmules noves.
        /// </summary>
        public FormulaTarifa()
        {
            Codi = string.Empty;
            Nom = string.Empty;
            Descripcio = string.Empty;
            TipusValors = string.Empty;

            ExpressioTarifa1 = string.Empty;
            ExpressioTarifa2 = string.Empty;
            ExpressioTarifa3 = string.Empty;
            ExpressioTarifa4 = string.Empty;
            ExpressioTarifa5 = string.Empty;
            ExpressioTarifa6 = string.Empty;

            /*
             * Una fórmula nova queda activa per defecte.
             * L'usuari podrà desactivar-la posteriorment
             * des del gestor.
             */
            Activa = true;
        }

        /// <summary>
        /// Retorna el text visible de la fórmula.
        ///
        /// Si disposa d'un ordre vàlid, el mostra
        /// davant del nom, igual que al Calculador actual.
        /// </summary>
        public override string ToString()
        {
            if (Ordre > 0)
            {
                return Ordre
                    + " · "
                    + Nom;
            }

            return Nom;
        }
    }
}
