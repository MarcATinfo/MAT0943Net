using A3ErpGestorFormulesTarifes.Dades;
using A3ErpGestorFormulesTarifes.Models;
using A3ErpGestorFormulesTarifes.Validacio;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace A3ErpCalculadorTarifes.Services
{
    /// <summary>
    /// Carrega i valida les fórmules actives
    /// disponibles per al Calculador.
    ///
    /// La base de dades és l'única font
    /// de fórmules en el nou sistema.
    /// </summary>
    internal sealed class ServeiCatalegFormulesConfigurables
    {
        private readonly RepositoriFormulesTarifes
            _repositori;

        private readonly ValidadorFormulaTarifa
            _validador;

        /// <summary>
        /// Inicialitza el servei amb
        /// les dependències necessàries.
        /// </summary>
        public ServeiCatalegFormulesConfigurables()
        {
            _repositori =
                new RepositoriFormulesTarifes();

            _validador =
                new ValidadorFormulaTarifa();
        }

        /// <summary>
        /// Recupera les fórmules actives
        /// i no eliminades de la base de dades.
        ///
        /// Abans de retornar-les comprova:
        /// - que existeixi almenys una fórmula;
        /// - que totes tinguin un ID vàlid;
        /// - que siguin actives i no eliminades;
        /// - que les dades obligatòries siguin correctes;
        /// - que no hi hagi codis repetits.
        /// </summary>
        public List<FormulaTarifa> Carregar(
            string cadenaConnexio)
        {
            if (string.IsNullOrWhiteSpace(
                cadenaConnexio))
            {
                throw new ArgumentException(
                    "La connexió amb a3ERP és buida.",
                    nameof(cadenaConnexio));
            }

            List<FormulaTarifa> formules =
                _repositori.ObtenirActives(
                    cadenaConnexio);

            if (formules == null
                || formules.Count == 0)
            {
                throw new InvalidOperationException(
                    "No hi ha cap fórmula activa disponible. "
                    + "El Calculador necessita almenys "
                    + "una fórmula activa.");
            }

            ValidarFormules(
                formules);

            /*
             * Ordenem explícitament el resultat,
             * encara que el repositori ja ho faci,
             * perquè aquest servei garanteixi
             * l'ordre que necessita la interfície.
             */
            return formules
                .OrderBy(
                    formula =>
                        formula.Ordre)
                .ThenBy(
                    formula =>
                        formula.Id)
                .ToList();
        }

        /// <summary>
        /// Valida individualment totes
        /// les fórmules carregades.
        /// </summary>
        private void ValidarFormules(
            IEnumerable<FormulaTarifa> formules)
        {
            var errors =
                new StringBuilder();

            foreach (
                FormulaTarifa formula
                in formules)
            {
                if (formula == null)
                {
                    AfegirError(
                        errors,
                        "S'ha recuperat una fórmula nul·la.");

                    continue;
                }

                string identificacio =
                    ObtenirIdentificacio(
                        formula);

                if (formula.Id <= 0)
                {
                    AfegirError(
                        errors,
                        identificacio
                        + ": l'identificador no és vàlid.");
                }

                if (!formula.Activa)
                {
                    AfegirError(
                        errors,
                        identificacio
                        + ": la fórmula carregada no està activa.");
                }

                if (formula.Eliminada)
                {
                    AfegirError(
                        errors,
                        identificacio
                        + ": la fórmula carregada està eliminada.");
                }

                ResultatValidacioFormula resultat =
                    _validador.Validar(
                        formula);

                if (!resultat.EsValid)
                {
                    AfegirError(
                        errors,
                        identificacio
                        + ":"
                        + "\r\n"
                        + resultat.ObtenirMissatge());
                }
            }

            ValidarCodisDuplicats(
                formules,
                errors);

            if (errors.Length > 0)
            {
                throw new InvalidOperationException(
                    "S'han detectat fórmules incorrectes "
                    + "a la base de dades:"
                    + "\r\n\r\n"
                    + errors);
            }
        }

        /// <summary>
        /// Comprova que no hi hagi dos registres
        /// actius amb el mateix codi.
        ///
        /// L'índex SQL ja ho protegeix, però
        /// també ho verifiquem abans de calcular.
        /// </summary>
        private static void ValidarCodisDuplicats(
            IEnumerable<FormulaTarifa> formules,
            StringBuilder errors)
        {
            IEnumerable<IGrouping<string, FormulaTarifa>>
                duplicats =
                    formules
                        .Where(
                            formula =>
                                formula != null)
                        .GroupBy(
                            formula =>
                                (
                                    formula.Codi
                                    ?? string.Empty
                                )
                                .Trim(),
                            StringComparer.OrdinalIgnoreCase)
                        .Where(
                            grup =>
                                grup.Count() > 1);

            foreach (
                IGrouping<string, FormulaTarifa> grup
                in duplicats)
            {
                AfegirError(
                    errors,
                    "El codi \""
                    + grup.Key
                    + "\" apareix repetit entre "
                    + "les fórmules actives.");
            }
        }

        /// <summary>
        /// Retorna una identificació llegible
        /// per als missatges de validació.
        /// </summary>
        private static string ObtenirIdentificacio(
            FormulaTarifa formula)
        {
            if (!string.IsNullOrWhiteSpace(
                formula.Codi))
            {
                return "Fórmula \""
                    + formula.Codi.Trim()
                    + "\"";
            }

            return "Fórmula ID "
                + formula.Id;
        }

        /// <summary>
        /// Afegeix una incidència al resum
        /// separant-la de les anteriors.
        /// </summary>
        private static void AfegirError(
            StringBuilder errors,
            string missatge)
        {
            if (errors == null
                || string.IsNullOrWhiteSpace(
                    missatge))
            {
                return;
            }

            if (errors.Length > 0)
            {
                errors.AppendLine();
                errors.AppendLine();
            }

            errors.Append(
                missatge.Trim());
        }
    }
}