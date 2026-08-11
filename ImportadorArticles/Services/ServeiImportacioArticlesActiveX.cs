using A3ErpImportadorArticles.Infrastructure.Logging;
using A3ErpImportadorArticles.Models;
using a3ERPActiveX;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace A3ErpImportadorArticles.Services
{
    /// <summary>
    /// Crea o actualitza articles mitjançant a3ERP ActiveX.
    ///
    /// Aquesta classe no escriu directament a la base de dades.
    /// Utilitza el mestre ARTICULO perquè a3ERP apliqui
    /// les seves validacions i regles internes.
    ///
    /// Només importa els camps inclosos a la plantilla actual
    /// del client. Els camps antics conservats als models
    /// no participen en aquesta operació.
    /// </summary>
    public sealed class ServeiImportacioArticlesActiveX
    {
        private const string NomMestreArticles =
            "ARTICULOS";

        /// <summary>
        /// Importa un únic article mitjançant
        /// el mestre ARTICULO.
        ///
        /// Abans de guardar, torna a comprovar si el codi
        /// existeix, independentment de la classificació
        /// obtinguda durant la validació prèvia.
        /// </summary>
        public ResultatImportacioArticle Importar(
            ArticleImportacio article)
        {
            if (article == null)
            {
                return ResultatImportacioArticle.CrearError(
                    string.Empty,
                    "No s'ha rebut cap article per importar.");
            }

            string codiArticle =
                (article.CodiArticle ?? string.Empty)
                .Trim();

            if (string.IsNullOrWhiteSpace(codiArticle))
            {
                return ResultatImportacioArticle.CrearError(
                    string.Empty,
                    "El codi de l'article és obligatori.");
            }

            string operacioPrevista =
                DeterminarOperacioPrevista(
                    article);

            if (operacioPrevista == "NO_IMPORTABLE")
            {
                return CrearErrorControlat(
                    article,
                    codiArticle,
                    "L'article no es troba en un estat vàlid per ser importat.",
                    operacioPrevista,
                    "NO_INICIADA",
                    "Validació de l'estat importable");
            }

            IMaestro maestro =
                null;

            bool operacioEnCurs =
                false;

            string operacioActiveX =
                "NO_INICIADA";

            string etapaActual =
                "Creació de l'objecte ActiveX";

            try
            {
                maestro =
                    new Maestro();

                /*
                 * Primer inicialitzem el mestre.
                 *
                 * Algunes propietats d'a3ERP ActiveX
                 * no es poden utilitzar fins que Iniciar
                 * ha preparat el mestre.
                 */
                etapaActual =
                    "Inicialització del mestre ARTICULO";

                maestro.Iniciar(
                    NomMestreArticles);

                /*
                 * Ometre els missatges emergents és opcional.
                 *
                 * Si la versió d'a3ERP instal·lada no permet
                 * modificar aquesta propietat, la importació
                 * continuarà igualment.
                 */
                etapaActual =
                    "Configuració dels missatges ActiveX";

                IntentarOmetreMissatges(
                    maestro);

                if (article.Estat == EstatImportacio.Nou)
                {
                    etapaActual =
                        "Cerca de l'article";

                    bool existeix =
                        maestro.Buscar(
                            codiArticle);

                    if (existeix)
                    {
                        operacioActiveX =
                            "JA_EXISTENT";

                        return CrearErrorControlat(
                            article,
                            codiArticle,
                            "L'article estava previst com a alta, però el codi ja existeix al mestre ARTICULO d'a3ERP.",
                            operacioPrevista,
                            operacioActiveX,
                            etapaActual);
                    }

                    etapaActual =
                        "Creació d'un article nou";

                    operacioActiveX =
                        "ALTA";

                    maestro.Nuevo();

                    operacioEnCurs =
                        true;

                    etapaActual =
                        "Assignació del codi de l'article";

                    AssignarCodiArticleNou(
                        maestro,
                        codiArticle);
                }
                else if (article.Estat == EstatImportacio.Actualitzacio)
                {
                    string codiArticleLiteralA3Erp =
                        article.CodiArticleLiteralA3Erp;

                    if (string.IsNullOrWhiteSpace(
                        codiArticleLiteralA3Erp))
                    {
                        operacioActiveX =
                            "NO_LOCALITZAT";

                        return CrearErrorControlat(
                            article,
                            codiArticle,
                            "No s'ha pogut determinar el codi literal de l'article existent a a3ERP.",
                            operacioPrevista,
                            operacioActiveX,
                            "Validació del codi literal d'a3ERP");
                    }

                    etapaActual =
                        "Cerca de l'article existent";

                    bool existeix =
                        maestro.Buscar(
                            codiArticleLiteralA3Erp);

                    if (!existeix)
                    {
                        operacioActiveX =
                            "NO_LOCALITZAT";

                        return CrearErrorControlat(
                            article,
                            codiArticle,
                            "L'article estava previst com a actualització, però no s'ha pogut localitzar al mestre ARTICULO d'a3ERP.",
                            operacioPrevista,
                            operacioActiveX,
                            etapaActual);
                    }

                    etapaActual =
                        "Obertura de l'article en mode edició";

                    operacioActiveX =
                        "MODIFICACIO";

                    maestro.Edita();

                    operacioEnCurs =
                        true;
                }

                /*
                 * Només s'assignen els camps informats
                 * a la plantilla actual del client.
                 */
                etapaActual =
                    "Assignació dels camps informats";

                AssignarCampsInformats(
                    maestro,
                    article);

                /*
                 * Guarda(true) s'utilitza tant per a les altes
                 * com per a les modificacions.
                 */
                etapaActual =
                    "Desament de l'article";

                maestro.Guarda(true);

                operacioEnCurs =
                    false;

                if (article.Estat == EstatImportacio.Nou)
                {
                    return ResultatImportacioArticle.CrearNou(
                        codiArticle);
                }

                return ResultatImportacioArticle.CrearActualitzat(
                    codiArticle);
            }
            catch (Exception ex)
            {
                /*
                 * Registrem aquí l'excepció original completa.
                 *
                 * Exception.ToString(), utilitzat pel logger,
                 * conserva el tipus, el missatge, la pila
                 * d'execució i les excepcions internes.
                 */
                ImportadorArticlesLogger.Error(
                    "S'ha produït un error durant la importació ActiveX de l'article.",
                    ex,
                    CrearCampsLogError(
                        article,
                        codiArticle,
                        operacioPrevista,
                        operacioActiveX,
                        etapaActual));

                if (maestro != null &&
                    operacioEnCurs)
                {
                    try
                    {
                        maestro.Cancelar();
                    }
                    catch (Exception exCancelar)
                    {
                        /*
                         * Un error en cancel·lar no substitueix
                         * l'error principal, però queda registrat.
                         */
                        ImportadorArticlesLogger.Advertencia(
                            "a3ERP no ha pogut cancel·lar l'operació ActiveX després de l'error.",
                            new Dictionary<string, object>
                            {
                                {
                                    "CODART",
                                    codiArticle
                                },
                                {
                                    "TipusExcepcio",
                                    exCancelar.GetType().FullName
                                },
                                {
                                    "Missatge",
                                    exCancelar.Message
                                }
                            });
                    }
                }

                /*
                 * La graella mostra només el missatge resumit.
                 * El detall tècnic complet queda al fitxer de log.
                 */
                return ResultatImportacioArticle.CrearError(
                    codiArticle,
                    ex.Message);
            }
            finally
            {
                if (maestro != null)
                {
                    try
                    {
                        maestro.Acabar();
                    }
                    catch (Exception exAcabar)
                    {
                        /*
                         * Un error durant Acabar no ha d'ocultar
                         * el resultat principal, però queda registrat.
                         */
                        ImportadorArticlesLogger.Advertencia(
                            "S'ha produït un error en finalitzar el mestre ARTICULO.",
                            new Dictionary<string, object>
                            {
                                {
                                    "CODART",
                                    codiArticle
                                },
                                {
                                    "TipusExcepcio",
                                    exAcabar.GetType().FullName
                                },
                                {
                                    "Missatge",
                                    exAcabar.Message
                                }
                            });
                    }

                    try
                    {
                        if (Marshal.IsComObject(maestro))
                        {
                            Marshal.FinalReleaseComObject(
                                maestro);
                        }
                    }
                    catch (Exception exAlliberar)
                    {
                        /*
                         * L'objecte COM ja podria estar alliberat.
                         * Aquest error tampoc ha d'aturar el procés.
                         */
                        ImportadorArticlesLogger.Advertencia(
                            "No s'ha pogut alliberar completament l'objecte COM del mestre ARTICULO.",
                            new Dictionary<string, object>
                            {
                                {
                                    "CODART",
                                    codiArticle
                                },
                                {
                                    "TipusExcepcio",
                                    exAlliberar.GetType().FullName
                                },
                                {
                                    "Missatge",
                                    exAlliberar.Message
                                }
                            });
                    }
                }
            }
        }

        /// <summary>
        /// Construeix els camps que acompanyen
        /// l'excepció completa al fitxer de log.
        /// </summary>
        private static string DeterminarOperacioPrevista(
            ArticleImportacio article)
        {
            if (article == null)
            {
                return "NO_IMPORTABLE";
            }

            switch (article.Estat)
            {
                case EstatImportacio.Nou:
                    return "ALTA";

                case EstatImportacio.Actualitzacio:
                    return "MODIFICACIO";

                default:
                    return "NO_IMPORTABLE";
            }
        }

        private static ResultatImportacioArticle CrearErrorControlat(
            ArticleImportacio article,
            string codiArticle,
            string missatge,
            string operacioPrevista,
            string operacioActiveX,
            string etapaActual)
        {
            ImportadorArticlesLogger.Error(
                "S'ha produït un error controlat durant la importació ActiveX de l'article.",
                null,
                CrearCampsLogError(
                    article,
                    codiArticle,
                    operacioPrevista,
                    operacioActiveX,
                    etapaActual));

            return ResultatImportacioArticle.CrearError(
                codiArticle,
                missatge);
        }

        private static Dictionary<string, object>
            CrearCampsLogError(
                ArticleImportacio article,
                string codiArticle,
                string operacioPrevista,
                string operacioActiveX,
                string etapaActual)
        {
            Dictionary<string, object> camps =
                new Dictionary<string, object>
                {
                    {
                        "Fila",
                        article?.NumeroFila ?? 0
                    },
                    {
                        "CODART",
                        codiArticle
                    },
                    {
                        "OperacioPrevista",
                        operacioPrevista ?? string.Empty
                    },
                    {
                        "OperacioActiveX",
                        operacioActiveX ?? string.Empty
                    },
                    {
                        "Etapa",
                        etapaActual ?? string.Empty
                    }
                };

            AfegirTextSiInformat(
                camps,
                "DESCART",
                article?.Descripcio);

            AfegirTextSiInformat(
                camps,
                "CAR1",
                article?.Familia);

            AfegirTextSiInformat(
                camps,
                "CAR2",
                article?.Unitats);

            AfegirTextSiInformat(
                camps,
                "PARAM1",
                article?.TipusUnitat);

            AfegirTextSiInformat(
                camps,
                "CODPRO",
                article?.CodiProveidor);

            AfegirTextSiInformat(
                camps,
                "ARTPRO",
                article?.ReferenciaProveidor);

            AfegirDecimalSiInformat(
                camps,
                "PRCCOMPRA",
                article?.PreuCompra);

            AfegirDecimalSiInformat(
                camps,
                "DESC1",
                article?.Descompte1);

            AfegirDecimalSiInformat(
                camps,
                "DESC2",
                article?.Descompte2);

            AfegirDecimalSiInformat(
                camps,
                "DESC3",
                article?.Descompte3);

            AfegirDecimalSiInformat(
                camps,
                "PRCCOSTE",
                article?.PreuCost);

            AfegirDecimalSiInformat(
                camps,
                "PRCSTANDARD",
                article?.PreuTransport);

            AfegirEnterSiInformat(
                camps,
                "IDFORMULA",
                article?.IdFormula);

            return camps;
        }

        /// <summary>
        /// Afegeix al log un camp textual
        /// únicament quan està informat.
        /// </summary>
        private static void AfegirTextSiInformat(
            IDictionary<string, object> camps,
            string nomCamp,
            string valor)
        {
            if (camps == null ||
                string.IsNullOrWhiteSpace(valor))
            {
                return;
            }

            camps[nomCamp] =
                valor.Trim();
        }

        /// <summary>
        /// Afegeix al log un camp decimal
        /// únicament quan està informat.
        /// </summary>
        private static void AfegirDecimalSiInformat(
            IDictionary<string, object> camps,
            string nomCamp,
            decimal? valor)
        {
            if (camps == null ||
                !valor.HasValue)
            {
                return;
            }

            camps[nomCamp] =
                valor.Value;
        }

        /// <summary>
        /// Afegeix al log un camp enter
        /// únicament quan està informat.
        /// </summary>
        private static void AfegirEnterSiInformat(
            IDictionary<string, object> camps,
            string nomCamp,
            int? valor)
        {
            if (camps == null ||
                !valor.HasValue)
            {
                return;
            }

            camps[nomCamp] =
                valor.Value;
        }

        /// <summary>
        /// Assigna a a3ERP únicament els camps informats
        /// a la plantilla actual del client.
        ///
        /// Regla funcional:
        /// - camp informat: s'assigna;
        /// - camp buit: no es modifica.
        ///
        /// En un article nou, a3ERP aplicarà els seus
        /// valors predeterminats als camps no informats.
        /// </summary>
        private static void AssignarCampsInformats(
            IMaestro maestro,
            ArticleImportacio article)
        {
            AssignarTextSiInformat(
                maestro,
                "DESCART",
                article.Descripcio);

            AssignarTextSiInformat(
                maestro,
                "CAR1",
                article.Familia);

            AssignarTextSiInformat(
                maestro,
                "CAR2",
                article.Unitats);

            AssignarTextSiInformat(
                maestro,
                "PARAM1",
                article.TipusUnitat);

            AssignarTextSiInformat(
                maestro,
                "CODPRO",
                article.CodiProveidor);

            AssignarTextSiInformat(
                maestro,
                "ARTPRO",
                article.ReferenciaProveidor);

            AssignarDecimalSiInformat(
                maestro,
                "PRCCOMPRA",
                article.PreuCompra);

            AssignarDecimalSiInformat(
                maestro,
                "DESC1",
                article.Descompte1);

            AssignarDecimalSiInformat(
                maestro,
                "DESC2",
                article.Descompte2);

            AssignarDecimalSiInformat(
                maestro,
                "DESC3",
                article.Descompte3);

            AssignarDecimalSiInformat(
                maestro,
                "PRCCOSTE",
                article.PreuCost);

            AssignarDecimalSiInformat(
                maestro,
                "PRCSTANDARD",
                article.PreuTransport);

            AssignarEnterSiInformat(
                maestro,
                "AT_FORMULA_TARIFA_ID",
                article.IdFormula);
        }

        /// <summary>
        /// Assigna la clau CODART en el flux d'alta.
        /// </summary>
        private static void AssignarCodiArticleNou(
            IMaestro maestro,
            string codiArticle)
        {
            if (maestro == null)
            {
                throw new ArgumentNullException(
                    nameof(maestro));
            }

            if (string.IsNullOrWhiteSpace(codiArticle))
            {
                throw new InvalidOperationException(
                    "El codi de l'article és obligatori.");
            }

            if (codiArticle.Length > 15)
            {
                throw new InvalidOperationException(
                    "El codi de l'article supera els 15 caràcters permesos.");
            }

            maestro.AsString["CODART"] =
                codiArticle;
        }

        /// <summary>
        /// Assigna un camp textual obligatori.
        /// </summary>
        private static void AssignarTextObligatori(
            IMaestro maestro,
            string nomCamp,
            string valor)
        {
            if (string.IsNullOrWhiteSpace(valor))
            {
                throw new InvalidOperationException(
                    "El camp " +
                    nomCamp +
                    " és obligatori.");
            }

            ValidarExistenciaCamp(
                maestro,
                nomCamp);

            maestro.AsString[nomCamp] =
                valor.Trim();
        }

        /// <summary>
        /// Assigna un camp textual només
        /// quan està informat.
        /// </summary>
        private static void AssignarTextSiInformat(
            IMaestro maestro,
            string nomCamp,
            string valor)
        {
            if (string.IsNullOrWhiteSpace(valor))
            {
                return;
            }

            ValidarExistenciaCamp(
                maestro,
                nomCamp);

            maestro.AsString[nomCamp] =
                valor.Trim();
        }

        /// <summary>
        /// Assigna un valor decimal només
        /// quan està informat.
        /// </summary>
        private static void AssignarDecimalSiInformat(
            IMaestro maestro,
            string nomCamp,
            decimal? valor)
        {
            if (!valor.HasValue)
            {
                return;
            }

            ValidarExistenciaCamp(
                maestro,
                nomCamp);

            maestro.AsFloat[nomCamp] =
                Convert.ToDouble(
                    valor.Value);
        }

        /// <summary>
        /// Assigna un valor enter només
        /// quan està informat.
        /// </summary>
        private static void AssignarEnterSiInformat(
            IMaestro maestro,
            string nomCamp,
            int? valor)
        {
            if (!valor.HasValue)
            {
                return;
            }

            ValidarExistenciaCamp(
                maestro,
                nomCamp);

            maestro.AsInteger[nomCamp] =
                valor.Value;
        }

        /// <summary>
        /// Comprova que el camp existeixi
        /// al mestre ARTICULO d'a3ERP.
        /// </summary>
        private static void ValidarExistenciaCamp(
            IMaestro maestro,
            string nomCamp)
        {
            if (maestro == null)
            {
                throw new ArgumentNullException(
                    nameof(maestro));
            }

            if (maestro.ExisteCampo(nomCamp))
            {
                return;
            }

            throw new InvalidOperationException(
                "El camp '" +
                nomCamp +
                "' no existeix al mestre ARTICULO d'a3ERP.");
        }

        /// <summary>
        /// Intenta evitar que ActiveX mostri
        /// finestres pròpies durant una importació massiva.
        /// </summary>
        private static void IntentarOmetreMissatges(
            IMaestro maestro)
        {
            if (maestro == null)
            {
                return;
            }

            try
            {
                maestro.OmitirMensajes =
                    true;
            }
            catch
            {
                /*
                 * Continuem amb els missatges estàndard d'a3ERP.
                 * Aquesta propietat no ha de bloquejar la importació.
                 */
            }
        }
    }
}
