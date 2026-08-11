using System;
using System.Collections.Generic;
using System.Globalization;

namespace A3ErpGestorFormulesTarifes.MotorExpressions
{
    /// <summary>
    /// Converteix una expressió textual
    /// en una seqüència ordenada de tokens.
    ///
    /// Reconeix:
    /// - números enters i decimals;
    /// - variables;
    /// - suma, resta, multiplicació i divisió;
    /// - parèntesis.
    /// </summary>
    public sealed class TokenitzadorExpressio
    {
        /// <summary>
        /// Analitza l'expressió indicada
        /// i retorna tots els tokens detectats.
        /// </summary>
        public IReadOnlyList<TokenExpressio> Tokenitzar(
            string expressio)
        {
            if (string.IsNullOrWhiteSpace(
                expressio))
            {
                throw new ExcepcioExpressioFormula(
                    "L'expressió és buida.");
            }

            var tokens =
                new List<TokenExpressio>();

            int posicio =
                0;

            while (posicio < expressio.Length)
            {
                char caracter =
                    expressio[posicio];

                /*
                 * Els espais no tenen significat
                 * dins de l'expressió.
                 */
                if (char.IsWhiteSpace(
                    caracter))
                {
                    posicio++;

                    continue;
                }

                if (EsIniciNumero(
                    expressio,
                    posicio))
                {
                    tokens.Add(
                        LlegirNumero(
                            expressio,
                            ref posicio));

                    continue;
                }

                if (EsIniciVariable(
                    caracter))
                {
                    tokens.Add(
                        LlegirVariable(
                            expressio,
                            ref posicio));

                    continue;
                }

                switch (caracter)
                {
                    case '+':
                        tokens.Add(
                            new TokenExpressio(
                                TipusTokenExpressio.Suma,
                                caracter.ToString(),
                                posicio));

                        posicio++;

                        break;

                    case '-':
                        tokens.Add(
                            new TokenExpressio(
                                TipusTokenExpressio.Resta,
                                caracter.ToString(),
                                posicio));

                        posicio++;

                        break;

                    case '*':
                        tokens.Add(
                            new TokenExpressio(
                                TipusTokenExpressio.Multiplicacio,
                                caracter.ToString(),
                                posicio));

                        posicio++;

                        break;

                    case '/':
                        tokens.Add(
                            new TokenExpressio(
                                TipusTokenExpressio.Divisio,
                                caracter.ToString(),
                                posicio));

                        posicio++;

                        break;

                    case '(':
                        tokens.Add(
                            new TokenExpressio(
                                TipusTokenExpressio.ParentesiObert,
                                caracter.ToString(),
                                posicio));

                        posicio++;

                        break;

                    case ')':
                        tokens.Add(
                            new TokenExpressio(
                                TipusTokenExpressio.ParentesiTancat,
                                caracter.ToString(),
                                posicio));

                        posicio++;

                        break;

                    default:
                        throw new ExcepcioExpressioFormula(
                            "El caràcter \""
                            + caracter
                            + "\" no està permès "
                            + "a la posició "
                            + posicio
                            + ".");
                }
            }

            /*
             * El token Final facilita al futur
             * analitzador detectar el final
             * correcte de l'expressió.
             */
            tokens.Add(
                new TokenExpressio(
                    TipusTokenExpressio.Final,
                    string.Empty,
                    expressio.Length));

            return tokens.AsReadOnly();
        }

        /// <summary>
        /// Determina si la posició actual
        /// pot iniciar un número.
        ///
        /// S'admeten números com:
        /// 10
        /// 10.25
        /// 10,25
        /// .5
        /// ,5
        /// </summary>
        private static bool EsIniciNumero(
            string expressio,
            int posicio)
        {
            char caracter =
                expressio[posicio];

            if (char.IsDigit(
                caracter))
            {
                return true;
            }

            if (caracter != '.'
                && caracter != ',')
            {
                return false;
            }

            int posicioSeguent =
                posicio + 1;

            return posicioSeguent < expressio.Length
                && char.IsDigit(
                    expressio[posicioSeguent]);
        }

        /// <summary>
        /// Llegeix un número enter o decimal.
        ///
        /// S'accepta tant el punt com la coma
        /// com a separador decimal.
        /// </summary>
        private static TokenExpressio LlegirNumero(
            string expressio,
            ref int posicio)
        {
            int posicioInicial =
                posicio;

            bool separadorDecimalTrobat =
                false;

            while (posicio < expressio.Length)
            {
                char caracter =
                    expressio[posicio];

                if (char.IsDigit(
                    caracter))
                {
                    posicio++;

                    continue;
                }

                if (caracter == '.'
                    || caracter == ',')
                {
                    if (separadorDecimalTrobat)
                    {
                        throw new ExcepcioExpressioFormula(
                            "El número iniciat a la posició "
                            + posicioInicial
                            + " conté més d'un separador decimal.");
                    }

                    separadorDecimalTrobat =
                        true;

                    posicio++;

                    continue;
                }

                break;
            }

            string textOriginal =
                expressio.Substring(
                    posicioInicial,
                    posicio - posicioInicial);

            string textNormalitzat =
                textOriginal.Replace(
                    ',',
                    '.');

            decimal valor;

            bool conversioCorrecta =
                decimal.TryParse(
                    textNormalitzat,
                    NumberStyles.AllowDecimalPoint,
                    CultureInfo.InvariantCulture,
                    out valor);

            if (!conversioCorrecta)
            {
                throw new ExcepcioExpressioFormula(
                    "El valor \""
                    + textOriginal
                    + "\" no és un número vàlid.");
            }

            return new TokenExpressio(
                TipusTokenExpressio.Numero,
                textOriginal,
                posicioInicial,
                valor);
        }

        /// <summary>
        /// Determina si un caràcter
        /// pot iniciar una variable.
        /// </summary>
        private static bool EsIniciVariable(
            char caracter)
        {
            return char.IsLetter(
                caracter)
                || caracter == '_';
        }

        /// <summary>
        /// Determina si un caràcter
        /// pot formar part d'una variable.
        /// </summary>
        private static bool EsCaracterVariable(
            char caracter)
        {
            return char.IsLetterOrDigit(
                caracter)
                || caracter == '_';
        }

        /// <summary>
        /// Llegeix el nom complet d'una variable.
        ///
        /// La comprovació de si la variable està
        /// autoritzada es farà durant l'avaluació.
        /// </summary>
        private static TokenExpressio LlegirVariable(
            string expressio,
            ref int posicio)
        {
            int posicioInicial =
                posicio;

            while (posicio < expressio.Length
                && EsCaracterVariable(
                    expressio[posicio]))
            {
                posicio++;
            }

            string nomVariable =
                expressio.Substring(
                    posicioInicial,
                    posicio - posicioInicial);

            return new TokenExpressio(
                TipusTokenExpressio.Variable,
                nomVariable,
                posicioInicial);
        }
    }
}