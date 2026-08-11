using System;
using System.Collections.Generic;

namespace A3ErpGestorFormulesTarifes.MotorExpressions
{
    /// <summary>
    /// Valida i avalua expressions matemàtiques
    /// utilitzades per calcular tarifes.
    ///
    /// Operacions admeses:
    /// - suma;
    /// - resta;
    /// - multiplicació;
    /// - divisió;
    /// - parèntesis;
    /// - signe negatiu.
    ///
    /// Respecta la prioritat matemàtica:
    /// parèntesis, multiplicació/divisió
    /// i finalment suma/resta.
    /// </summary>
    public sealed class AvaluadorExpressioFormula
    {
        private readonly TokenitzadorExpressio
            _tokenitzador;

        private IReadOnlyList<TokenExpressio>
            _tokens;

        private ContextAvaluacioFormula
            _context;

        private int
            _indexToken;

        /// <summary>
        /// Inicialitza l'avaluador amb
        /// el tokenitzador segur.
        /// </summary>
        public AvaluadorExpressioFormula()
        {
            _tokenitzador =
                new TokenitzadorExpressio();

            _tokens =
                Array.Empty<TokenExpressio>();

            _context =
                null;

            _indexToken =
                0;
        }

        /// <summary>
        /// Avalua una expressió utilitzant
        /// els valors del context indicat.
        /// </summary>
        public decimal Avaluar(
            string expressio,
            ContextAvaluacioFormula context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(
                    nameof(context));
            }

            try
            {
                _tokens =
                    _tokenitzador.Tokenitzar(
                        expressio);

                _context =
                    context;

                _indexToken =
                    0;

                decimal resultat =
                    LlegirExpressio();

                TokenExpressio tokenFinal =
                    ObtenirTokenActual();

                if (tokenFinal.Tipus
                    != TipusTokenExpressio.Final)
                {
                    throw CrearErrorSintaxi(
                        tokenFinal,
                        "S'ha trobat un element inesperat.");
                }

                return resultat;
            }
            catch (ExcepcioExpressioFormula)
            {
                throw;
            }
            catch (OverflowException ex)
            {
                throw new ExcepcioExpressioFormula(
                    "El resultat de l'expressió supera "
                    + "el rang numèric admès.",
                    ex);
            }
            catch (Exception ex)
            {
                throw new ExcepcioExpressioFormula(
                    "No s'ha pogut avaluar l'expressió.",
                    ex);
            }
            finally
            {
                _tokens =
                    Array.Empty<TokenExpressio>();

                _context =
                    null;

                _indexToken =
                    0;
            }
        }

        /// <summary>
        /// Avalua sumes i restes.
        ///
        /// Expressió:
        /// terme ((+ | -) terme)*
        /// </summary>
        private decimal LlegirExpressio()
        {
            decimal resultat =
                LlegirTerme();

            while (true)
            {
                TokenExpressio token =
                    ObtenirTokenActual();

                if (token.Tipus
                    == TipusTokenExpressio.Suma)
                {
                    AvancarToken();

                    decimal operand =
                        LlegirTerme();

                    resultat =
                        Sumar(
                            resultat,
                            operand);

                    continue;
                }

                if (token.Tipus
                    == TipusTokenExpressio.Resta)
                {
                    AvancarToken();

                    decimal operand =
                        LlegirTerme();

                    resultat =
                        Restar(
                            resultat,
                            operand);

                    continue;
                }

                return resultat;
            }
        }

        /// <summary>
        /// Avalua multiplicacions i divisions.
        ///
        /// Terme:
        /// factor ((* | /) factor)*
        /// </summary>
        private decimal LlegirTerme()
        {
            decimal resultat =
                LlegirFactor();

            while (true)
            {
                TokenExpressio token =
                    ObtenirTokenActual();

                if (token.Tipus
                    == TipusTokenExpressio.Multiplicacio)
                {
                    AvancarToken();

                    decimal operand =
                        LlegirFactor();

                    resultat =
                        Multiplicar(
                            resultat,
                            operand);

                    continue;
                }

                if (token.Tipus
                    == TipusTokenExpressio.Divisio)
                {
                    AvancarToken();

                    decimal divisor =
                        LlegirFactor();

                    if (divisor == 0M)
                    {
                        throw CrearErrorSintaxi(
                            token,
                            "No es pot dividir per zero.");
                    }

                    resultat =
                        Dividir(
                            resultat,
                            divisor);

                    continue;
                }

                return resultat;
            }
        }

        /// <summary>
        /// Avalua un factor individual.
        ///
        /// Admet:
        /// - signe negatiu;
        /// - número;
        /// - variable;
        /// - expressió entre parèntesis.
        ///
        /// No s'admet el signe positiu unari.
        /// Això permet detectar expressions
        /// incorrectes com PRCCOSTE ++ P1.
        /// </summary>
        private decimal LlegirFactor()
        {
            TokenExpressio token =
                ObtenirTokenActual();

            if (token.Tipus
                == TipusTokenExpressio.Resta)
            {
                AvancarToken();

                decimal valor =
                    LlegirFactor();

                return Negar(
                    valor);
            }

            if (token.Tipus
                == TipusTokenExpressio.Numero)
            {
                AvancarToken();

                if (!token.ValorNumeric.HasValue)
                {
                    throw CrearErrorSintaxi(
                        token,
                        "El número no disposa d'un valor vàlid.");
                }

                return token.ValorNumeric.Value;
            }

            if (token.Tipus
                == TipusTokenExpressio.Variable)
            {
                AvancarToken();

                return _context.ObtenirValor(
                    token.Text);
            }

            if (token.Tipus
                == TipusTokenExpressio.ParentesiObert)
            {
                AvancarToken();

                decimal resultat =
                    LlegirExpressio();

                TokenExpressio parentesiTancat =
                    ObtenirTokenActual();

                if (parentesiTancat.Tipus
                    != TipusTokenExpressio.ParentesiTancat)
                {
                    throw CrearErrorSintaxi(
                        parentesiTancat,
                        "Falta tancar el parèntesi.");
                }

                AvancarToken();

                return resultat;
            }

            if (token.Tipus
                == TipusTokenExpressio.ParentesiTancat)
            {
                throw CrearErrorSintaxi(
                    token,
                    "S'ha trobat un parèntesi de tancament inesperat.");
            }

            if (token.Tipus
                == TipusTokenExpressio.Final)
            {
                throw CrearErrorSintaxi(
                    token,
                    "L'expressió està incompleta.");
            }

            throw CrearErrorSintaxi(
                token,
                "S'esperava un número, una variable "
                + "o un parèntesi d'obertura.");
        }

        /// <summary>
        /// Retorna el token que s'està
        /// processant actualment.
        /// </summary>
        private TokenExpressio ObtenirTokenActual()
        {
            if (_tokens == null
                || _tokens.Count == 0)
            {
                throw new ExcepcioExpressioFormula(
                    "No hi ha tokens disponibles per avaluar.");
            }

            if (_indexToken < 0
                || _indexToken >= _tokens.Count)
            {
                return _tokens[
                    _tokens.Count - 1];
            }

            return _tokens[
                _indexToken];
        }

        /// <summary>
        /// Avança al token següent.
        /// </summary>
        private void AvancarToken()
        {
            if (_indexToken
                < _tokens.Count - 1)
            {
                _indexToken++;
            }
        }

        /// <summary>
        /// Genera un error de sintaxi
        /// indicant la posició aproximada.
        /// </summary>
        private static ExcepcioExpressioFormula CrearErrorSintaxi(
            TokenExpressio token,
            string missatge)
        {
            if (token == null)
            {
                return new ExcepcioExpressioFormula(
                    missatge);
            }

            return new ExcepcioExpressioFormula(
                missatge
                + " Posició: "
                + token.Posicio
                + ".");
        }

        /// <summary>
        /// Executa una suma decimal
        /// detectant desbordaments.
        /// </summary>
        private static decimal Sumar(
            decimal esquerra,
            decimal dreta)
        {
            checked
            {
                return esquerra
                    + dreta;
            }
        }

        /// <summary>
        /// Executa una resta decimal
        /// detectant desbordaments.
        /// </summary>
        private static decimal Restar(
            decimal esquerra,
            decimal dreta)
        {
            checked
            {
                return esquerra
                    - dreta;
            }
        }

        /// <summary>
        /// Executa una multiplicació decimal
        /// detectant desbordaments.
        /// </summary>
        private static decimal Multiplicar(
            decimal esquerra,
            decimal dreta)
        {
            checked
            {
                return esquerra
                    * dreta;
            }
        }

        /// <summary>
        /// Executa una divisió decimal
        /// detectant desbordaments.
        /// </summary>
        private static decimal Dividir(
            decimal dividend,
            decimal divisor)
        {
            checked
            {
                return dividend
                    / divisor;
            }
        }

        /// <summary>
        /// Aplica el signe negatiu
        /// detectant desbordaments.
        /// </summary>
        private static decimal Negar(
            decimal valor)
        {
            checked
            {
                return -valor;
            }
        }
    }
}