using System;

namespace A3ErpGestorFormulesTarifes.MotorExpressions
{
    /// <summary>
    /// Representa un error detectat durant
    /// la validació o l'avaluació d'una expressió.
    /// </summary>
    public sealed class ExcepcioExpressioFormula
        : Exception
    {
        /// <summary>
        /// Inicialitza l'excepció amb
        /// el missatge indicat.
        /// </summary>
        public ExcepcioExpressioFormula(
            string missatge)
            : base(missatge)
        {
        }

        /// <summary>
        /// Inicialitza l'excepció conservant
        /// també l'error original.
        /// </summary>
        public ExcepcioExpressioFormula(
            string missatge,
            Exception excepcioInterna)
            : base(
                missatge,
                excepcioInterna)
        {
        }
    }
}