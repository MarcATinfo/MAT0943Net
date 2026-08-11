using System;
using System.Collections.Generic;
using System.Text;

namespace A3ErpGestorFormulesTarifes.Validacio
{
    /// <summary>
    /// Conté el resultat de validar
    /// una fórmula de tarifes.
    ///
    /// Permet acumular totes les incidències
    /// detectades i mostrar-les conjuntament
    /// a l'usuari abans de desar.
    /// </summary>
    public sealed class ResultatValidacioFormula
    {
        private readonly List<string>
            _errors;

        /// <summary>
        /// Indica si no s'ha detectat
        /// cap error de validació.
        /// </summary>
        public bool EsValid
        {
            get
            {
                return _errors.Count == 0;
            }
        }

        /// <summary>
        /// Retorna la llista d'errors detectats
        /// en format de només lectura.
        /// </summary>
        public IReadOnlyList<string> Errors
        {
            get
            {
                return _errors.AsReadOnly();
            }
        }

        /// <summary>
        /// Inicialitza un resultat
        /// sense errors.
        /// </summary>
        public ResultatValidacioFormula()
        {
            _errors =
                new List<string>();
        }

        /// <summary>
        /// Afegeix una incidència de validació.
        ///
        /// Els textos buits no s'incorporen
        /// al resultat.
        /// </summary>
        public void AfegirError(
            string missatge)
        {
            if (string.IsNullOrWhiteSpace(
                missatge))
            {
                return;
            }

            _errors.Add(
                missatge.Trim());
        }

        /// <summary>
        /// Retorna tots els errors en un text
        /// preparat per mostrar en pantalla.
        /// </summary>
        public string ObtenirMissatge()
        {
            if (EsValid)
            {
                return string.Empty;
            }

            var text =
                new StringBuilder();

            for (int index = 0;
                 index < _errors.Count;
                 index++)
            {
                text.Append("• ");
                text.Append(
                    _errors[index]);

                if (index <
                    _errors.Count - 1)
                {
                    text.AppendLine();
                }
            }

            return text.ToString();
        }
    }
}