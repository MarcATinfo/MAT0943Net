using A3ErpGestorFormulesTarifes.Models;
using System;

namespace A3ErpGestorFormulesTarifes.Validacio
{
    /// <summary>
    /// Centralitza les regles de validació
    /// de les fórmules de tarifes.
    ///
    /// Aquestes comprovacions protegeixen
    /// la coherència funcional abans de desar
    /// qualsevol canvi a la base de dades.
    /// </summary>
    public sealed class ValidadorFormulaTarifa
    {
        /// <summary>
        /// Valida totes les propietats obligatòries
        /// i les regles d'estat d'una fórmula.
        /// </summary>
        public ResultatValidacioFormula Validar(
            FormulaTarifa formula)
        {
            var resultat =
                new ResultatValidacioFormula();

            if (formula == null)
            {
                resultat.AfegirError(
                    "No s'ha proporcionat cap fórmula per validar.");

                return resultat;
            }

            ValidarIdentificacio(
                formula,
                resultat);

            ValidarExpressions(
                formula,
                resultat);

            ValidarEstatLogic(
                formula,
                resultat);

            return resultat;
        }

        /// <summary>
        /// Valida les dades identificatives
        /// i d'ordenació de la fórmula.
        /// </summary>
        private static void ValidarIdentificacio(
            FormulaTarifa formula,
            ResultatValidacioFormula resultat)
        {
            if (formula.Ordre <= 0)
            {
                resultat.AfegirError(
                    "L'ordre ha de ser superior a zero.");
            }

            ValidarTextObligatori(
                resultat,
                formula.Codi,
                "El codi",
                50);

            ValidarTextObligatori(
                resultat,
                formula.Nom,
                "El nom",
                150);

            ValidarTextOpcional(
                resultat,
                formula.Descripcio,
                "La descripció",
                1000);

            ValidarTextObligatori(
                resultat,
                formula.TipusValors,
                "El tipus de valors",
                100);
        }

        /// <summary>
        /// Valida que les sis tarifes disposin
        /// d'una expressió i que no superin
        /// la longitud permesa per SQL.
        /// </summary>
        private static void ValidarExpressions(
            FormulaTarifa formula,
            ResultatValidacioFormula resultat)
        {
            ValidarTextObligatori(
                resultat,
                formula.ExpressioTarifa1,
                "L'expressió de la tarifa 1",
                500);

            ValidarTextObligatori(
                resultat,
                formula.ExpressioTarifa2,
                "L'expressió de la tarifa 2",
                500);

            ValidarTextObligatori(
                resultat,
                formula.ExpressioTarifa3,
                "L'expressió de la tarifa 3",
                500);

            ValidarTextObligatori(
                resultat,
                formula.ExpressioTarifa4,
                "L'expressió de la tarifa 4",
                500);

            ValidarTextObligatori(
                resultat,
                formula.ExpressioTarifa5,
                "L'expressió de la tarifa 5",
                500);

            ValidarTextObligatori(
                resultat,
                formula.ExpressioTarifa6,
                "L'expressió de la tarifa 6",
                500);
        }

        /// <summary>
        /// Valida la coherència entre els camps
        /// ACTIVO, ELIMINADO i FECHA_ELIMINACION.
        /// </summary>
        private static void ValidarEstatLogic(
            FormulaTarifa formula,
            ResultatValidacioFormula resultat)
        {
            if (formula.Eliminada
                && formula.Activa)
            {
                resultat.AfegirError(
                    "Una fórmula eliminada no pot continuar activa.");
            }

            if (formula.Eliminada
                && !formula.DataEliminacio.HasValue)
            {
                resultat.AfegirError(
                    "Una fórmula eliminada ha de tenir informada "
                    + "la data d'eliminació.");
            }

            if (!formula.Eliminada
                && formula.DataEliminacio.HasValue)
            {
                resultat.AfegirError(
                    "Una fórmula no eliminada no pot tenir "
                    + "informada la data d'eliminació.");
            }
        }

        /// <summary>
        /// Valida un text obligatori
        /// i la longitud màxima admesa.
        /// </summary>
        private static void ValidarTextObligatori(
            ResultatValidacioFormula resultat,
            string valor,
            string nomCamp,
            int longitudMaxima)
        {
            if (string.IsNullOrWhiteSpace(
                valor))
            {
                resultat.AfegirError(
                    nomCamp
                    + " és obligatori.");

                return;
            }

            if (valor.Trim().Length
                > longitudMaxima)
            {
                resultat.AfegirError(
                    nomCamp
                    + " no pot superar els "
                    + longitudMaxima
                    + " caràcters.");
            }
        }

        /// <summary>
        /// Valida únicament la longitud
        /// d'un text que pot quedar buit.
        /// </summary>
        private static void ValidarTextOpcional(
            ResultatValidacioFormula resultat,
            string valor,
            string nomCamp,
            int longitudMaxima)
        {
            if (string.IsNullOrEmpty(
                valor))
            {
                return;
            }

            if (valor.Length
                > longitudMaxima)
            {
                resultat.AfegirError(
                    nomCamp
                    + " no pot superar els "
                    + longitudMaxima
                    + " caràcters.");
            }
        }
    }
}