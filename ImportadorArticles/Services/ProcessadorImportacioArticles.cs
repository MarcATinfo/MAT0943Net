using A3ErpImportadorArticles.Infrastructure.Logging;
using A3ErpImportadorArticles.Models;
using a3ERPActiveX;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;

namespace A3ErpImportadorArticles.Services
{
    /// <summary>
    /// Processa els articles seleccionats de la graella
    /// utilitzant el servei ActiveX d'a3ERP.
    ///
    /// Cada article es tracta de manera independent:
    /// un error en una fila no atura la resta del lot.
    ///
    /// També registra l'inici, el resultat i la durada
    /// del lot i de cada article quan el log està actiu.
    /// </summary>
    public sealed class ProcessadorImportacioArticles
    {
        private readonly ServeiImportacioArticlesActiveX
            _serveiImportacio;

        public ProcessadorImportacioArticles()
        {
            _serveiImportacio =
                new ServeiImportacioArticlesActiveX();
        }

        /// <summary>
        /// Importa els articles seleccionats que estiguin
        /// classificats com a nous o actualitzacions.
        /// </summary>
        /// <param name="articles">
        /// Llista completa d'articles mostrats a la graella.
        /// </param>
        /// <param name="notificarProgres">
        /// Retorna la posició actual, el total i l'article processat.
        /// Pot ser null.
        /// </param>
        public ResultatImportacioLot Processar(
            IList<ArticleImportacio> articles,
            Action<int, int, ArticleImportacio> notificarProgres)
        {
            if (articles == null)
            {
                throw new ArgumentNullException(
                    nameof(articles));
            }

            List<ArticleImportacio> articlesAProcessar =
                articles
                    .Where(EsArticleImportable)
                    .ToList();

            ResultatImportacioLot resultatLot =
                new ResultatImportacioLot();

            int total =
                articlesAProcessar.Count;

            Stopwatch cronometreLot =
                Stopwatch.StartNew();

            IEnlace enlaceActiveX =
                null;

            ImportadorArticlesLogger.Informacio(
                "S'inicia el lot d'importació d'articles.",
                new Dictionary<string, object>
                {
                    {
                        "TotalSeleccionats",
                        total
                    }
                });

            try
            {
                if (RequereixConnexioActiveX(
                    articlesAProcessar))
                {
                    if (!InicialitzarEnlaceActiveX(
                        articlesAProcessar,
                        resultatLot,
                        notificarProgres,
                        total,
                        out enlaceActiveX))
                    {
                        cronometreLot.Stop();

                        RegistrarResumLot(
                            "Ha finalitzat el lot d'importació d'articles.",
                            resultatLot,
                            cronometreLot.ElapsedMilliseconds);

                        return resultatLot;
                    }
                }
                for (int index = 0;
                     index < total;
                     index++)
                {
                    ArticleImportacio article =
                        articlesAProcessar[index];

                    ProcessarArticle(
                        article,
                        index + 1,
                        total,
                        resultatLot);

                    notificarProgres?.Invoke(
                        index + 1,
                        total,
                        article);
                }

                cronometreLot.Stop();

                RegistrarResumLot(
                    "Ha finalitzat el lot d'importació d'articles.",
                    resultatLot,
                    cronometreLot.ElapsedMilliseconds);

                return resultatLot;
            }
            catch (Exception ex)
            {
                cronometreLot.Stop();

                ImportadorArticlesLogger.Error(
                    "S'ha produït un error inesperat durant el lot d'importació.",
                    ex,
                    new Dictionary<string, object>
                    {
                        {
                            "TotalProcessats",
                            resultatLot.TotalProcessats
                        },
                        {
                            "Creats",
                            resultatLot.TotalCreats
                        },
                        {
                            "Actualitzats",
                            resultatLot.TotalActualitzats
                        },
                        {
                            "Errors",
                            resultatLot.TotalErrors
                        },
                        {
                            "DuradaMs",
                            cronometreLot.ElapsedMilliseconds
                        }
                    });

                /*
                 * Conservem el comportament normal:
                 * l'error inesperat puja fins al formulari,
                 * que mostrarà el missatge corresponent.
                 */
                throw;
            }
            finally
            {
                FinalitzarEnlaceActiveX(
                    ref enlaceActiveX);
            }
        }

        /// <summary>
        /// Processa un article individual i registra
        /// el resultat de l'operació.
        /// </summary>
        private void ProcessarArticle(
            ArticleImportacio article,
            int posicio,
            int total,
            ResultatImportacioLot resultatLot)
        {
            string operacioPrevista =
                DeterminarOperacioPrevista(
                    article);

            string origenImportacio =
                DeterminarOrigenImportacio(
                    article);

            ImportadorArticlesLogger.Informacio(
                "S'inicia el processament de l'article.",
                CrearCampsIniciArticle(
                    article,
                    posicio,
                    total,
                    operacioPrevista));

            Stopwatch cronometreArticle =
                Stopwatch.StartNew();

            ResultatImportacioArticle resultatArticle =
                ImportarArticle(
                    article);

            cronometreArticle.Stop();

            ActualitzarArticle(
                article,
                resultatArticle,
                resultatLot);

            if (resultatArticle == null)
            {
                Dictionary<string, object> campsErrorNull =
                    CrearCampsErrorArticle(
                        article,
                        operacioPrevista,
                        "Retorn servei d'importació");

                campsErrorNull.Add(
                    "Missatge",
                    article.Missatge);

                campsErrorNull.Add(
                    "DuradaMs",
                    cronometreArticle.ElapsedMilliseconds);

                ImportadorArticlesLogger.Error(
                    "a3ERP no ha retornat cap resultat per a l'article.",
                    null,
                    campsErrorNull);

                return;
            }

            if (!resultatArticle.Correcte)
            {
                Dictionary<string, object> campsError =
                    CrearCampsErrorArticle(
                        article,
                        operacioPrevista,
                        origenImportacio);

                campsError.Add(
                    "Missatge",
                    resultatArticle.Missatge);

                campsError.Add(
                    "DuradaMs",
                    cronometreArticle.ElapsedMilliseconds);

                ImportadorArticlesLogger.Error(
                    "L'article no s'ha pogut importar.",
                    null,
                    campsError);

                return;
            }

            string operacioRealitzada;

            if (resultatArticle.Creat)
            {
                operacioRealitzada =
                    "ALTA";
            }
            else if (resultatArticle.Actualitzat)
            {
                operacioRealitzada =
                    "MODIFICACIO";
            }
            else
            {
                operacioRealitzada =
                    "OPERACIO_CORRECTA";
            }

            ImportadorArticlesLogger.Informacio(
                "L'article s'ha importat correctament.",
                CrearCampsResultatCorrecte(
                    article,
                    operacioRealitzada,
                    resultatArticle,
                    cronometreArticle.ElapsedMilliseconds));
        }

        /// <summary>
        /// Indica si una fila està preparada
        /// per importar-se.
        /// </summary>
        private static bool EsArticleImportable(
            ArticleImportacio article)
        {
            if (article == null ||
                !article.Seleccionat)
            {
                return false;
            }

            return
                article.Estat == EstatImportacio.Nou ||
                article.Estat == EstatImportacio.Actualitzacio;
        }

        private static bool RequereixConnexioActiveX(
            IEnumerable<ArticleImportacio> articles)
        {
            if (articles == null)
            {
                return false;
            }

            return articles.Any(
                article =>
                    article != null &&
                    (
                        article.Estat == EstatImportacio.Nou ||
                        article.Estat == EstatImportacio.Actualitzacio
                    ));
        }

        private ResultatImportacioArticle ImportarArticle(
            ArticleImportacio article)
        {
            return _serveiImportacio.Importar(
                article);
        }

        private static string DeterminarOrigenImportacio(
            ArticleImportacio article)
        {
            return "Importació ActiveX";
        }

        /// <summary>
        /// Retorna l'operació prevista segons
        /// la classificació prèvia de la fila.
        /// </summary>
        private static string DeterminarOperacioPrevista(
            ArticleImportacio article)
        {
            if (article == null)
            {
                return "DESCONEGUDA";
            }

            switch (article.Estat)
            {
                case EstatImportacio.Nou:
                    return "ALTA";

                case EstatImportacio.Actualitzacio:
                    return "MODIFICACIO";

                default:
                    return "DESCONEGUDA";
            }
        }

        private static void RegistrarResumLot(
            string missatge,
            ResultatImportacioLot resultatLot,
            long duradaMs)
        {
            Dictionary<string, object> camps =
                new Dictionary<string, object>
                {
                    {
                        "TotalProcessats",
                        resultatLot.TotalProcessats
                    },
                    {
                        "Creats",
                        resultatLot.TotalCreats
                    },
                    {
                        "Actualitzats",
                        resultatLot.TotalActualitzats
                    },
                    {
                        "Errors",
                        resultatLot.TotalErrors
                    },
                    {
                        "DuradaMs",
                        duradaMs
                    }
                };

            if (resultatLot.TotalProcessats == 0 ||
                resultatLot.TotalErrors ==
                    resultatLot.TotalProcessats)
            {
                ImportadorArticlesLogger.Error(
                    missatge,
                    null,
                    camps);

                return;
            }

            if (resultatLot.TotalErrors > 0)
            {
                ImportadorArticlesLogger.Advertencia(
                    missatge,
                    camps);

                return;
            }

            ImportadorArticlesLogger.Informacio(
                missatge,
                camps);
        }

        private static bool InicialitzarEnlaceActiveX(
            IList<ArticleImportacio> articlesAProcessar,
            ResultatImportacioLot resultatLot,
            Action<int, int, ArticleImportacio> notificarProgres,
            int total,
            out IEnlace enlaceActiveX)
        {
            enlaceActiveX =
                null;

            const string missatgeError =
                "No s'ha pogut iniciar la connexió ActiveX amb l'empresa activa d'a3ERP.";

            try
            {
                enlaceActiveX =
                    new Enlace();

                bool connexioActiva =
                    enlaceActiveX.SelecEmpresaActiva();

                Dictionary<string, object> camps =
                    CrearCampsConnexioActiveX(
                        enlaceActiveX,
                        connexioActiva,
                        total);

                bool estatActiu =
                    enlaceActiveX.Estado ==
                    EstadoEnlace.estACTIVO;

                if (connexioActiva &&
                    !enlaceActiveX.bError &&
                    estatActiu)
                {
                    ImportadorArticlesLogger.Informacio(
                        "S'ha iniciat la connexió ActiveX amb l'empresa activa d'a3ERP.",
                        camps);

                    return true;
                }

                ImportadorArticlesLogger.Error(
                    missatgeError,
                    null,
                    camps);

                RegistrarErrorConnexioActiveX(
                    articlesAProcessar,
                    resultatLot,
                    notificarProgres,
                    missatgeError);

                return false;
            }
            catch (Exception ex)
            {
                ImportadorArticlesLogger.Error(
                    missatgeError,
                    ex,
                    CrearCampsConnexioActiveX(
                        enlaceActiveX,
                        false,
                        total));

                RegistrarErrorConnexioActiveX(
                    articlesAProcessar,
                    resultatLot,
                    notificarProgres,
                    missatgeError);

                return false;
            }
        }

        private static void RegistrarErrorConnexioActiveX(
            IList<ArticleImportacio> articlesAProcessar,
            ResultatImportacioLot resultatLot,
            Action<int, int, ArticleImportacio> notificarProgres,
            string missatge)
        {
            int total =
                articlesAProcessar?.Count ?? 0;

            for (int index = 0;
                 index < total;
                 index++)
            {
                ArticleImportacio article =
                    articlesAProcessar[index];

                ActualitzarArticle(
                    article,
                    ResultatImportacioArticle.CrearError(
                        article?.CodiArticle,
                        missatge),
                    resultatLot);

                notificarProgres?.Invoke(
                    index + 1,
                    total,
                    article);
            }
        }

        private static Dictionary<string, object> CrearCampsConnexioActiveX(
            IEnlace enlaceActiveX,
            bool connexioActiva,
            int totalSeleccionats)
        {
            return new Dictionary<string, object>
            {
                {
                    "Etapa",
                    "Connexió ActiveX"
                },
                {
                    "MetodeConnexio",
                    "SelecEmpresaActiva"
                },
                {
                    "ConnexioActiva",
                    connexioActiva
                },
                {
                    "EmpresaActiveX",
                    LlegirEmpresaActiva(
                        enlaceActiveX)
                },
                {
                    "EstadoEnlace",
                    LlegirEstadoEnlace(
                        enlaceActiveX)
                },
                {
                    "NumeroError",
                    LlegirNumeroErrorEnlace(
                        enlaceActiveX)
                },
                {
                    "MissatgeEnlace",
                    LlegirMissatgeEnlace(
                        enlaceActiveX)
                },
                {
                    "TotalSeleccionats",
                    totalSeleccionats
                }
            };
        }

        private static void FinalitzarEnlaceActiveX(
            ref IEnlace enlaceActiveX)
        {
            if (enlaceActiveX == null)
            {
                return;
            }

            try
            {
                ImportadorArticlesLogger.Informacio(
                    "Es finalitza la referència ActiveX del lot sense tancar la sessió activa d'a3ERP.",
                    CrearCampsFinalitzacioReferenciaActiveX(
                        enlaceActiveX));

                if (Marshal.IsComObject(
                    enlaceActiveX))
                {
                    Marshal.ReleaseComObject(
                        enlaceActiveX);
                }

                ImportadorArticlesLogger.Informacio(
                    "S'ha alliberat la referència COM de l'Enlace del lot.",
                    new Dictionary<string, object>
                    {
                        {
                            "MetodeConnexio",
                            "SelecEmpresaActiva"
                        },
                        {
                            "FinalitzaSessioActiveX",
                            false
                        }
                    });
            }
            catch (Exception exAlliberar)
            {
                ImportadorArticlesLogger.Advertencia(
                    "No s'ha pogut alliberar la referència COM de l'Enlace del lot.",
                    new Dictionary<string, object>
                    {
                        {
                            "TipusExcepcio",
                            exAlliberar.GetType().FullName
                        },
                        {
                            "Missatge",
                            exAlliberar.Message
                        },
                        {
                            "MetodeConnexio",
                            "SelecEmpresaActiva"
                        },
                        {
                            "FinalitzaSessioActiveX",
                            false
                        }
                    });
            }
            finally
            {
                enlaceActiveX =
                    null;
            }
        }

        private static Dictionary<string, object>
            CrearCampsFinalitzacioReferenciaActiveX(
                IEnlace enlaceActiveX)
        {
            return new Dictionary<string, object>
            {
                {
                    "MetodeConnexio",
                    "SelecEmpresaActiva"
                },
                {
                    "EmpresaActiveX",
                    LlegirEmpresaActiva(
                        enlaceActiveX)
                },
                {
                    "EstadoEnlace",
                    LlegirEstadoEnlace(
                        enlaceActiveX)
                },
                {
                    "FinalitzaSessioActiveX",
                    false
                }
            };
        }

        private static string LlegirEmpresaActiva(
            IEnlace enlaceActiveX)
        {
            try
            {
                return enlaceActiveX == null
                    ? string.Empty
                    : enlaceActiveX.EmpresaActiva ?? string.Empty;
            }
            catch
            {
                return string.Empty;
            }
        }

        private static string LlegirEstadoEnlace(
            IEnlace enlaceActiveX)
        {
            try
            {
                return enlaceActiveX == null
                    ? string.Empty
                    : enlaceActiveX.Estado.ToString();
            }
            catch
            {
                return string.Empty;
            }
        }

        private static int LlegirNumeroErrorEnlace(
            IEnlace enlaceActiveX)
        {
            try
            {
                return enlaceActiveX == null
                    ? 0
                    : enlaceActiveX.nError;
            }
            catch
            {
                return 0;
            }
        }

        private static string LlegirMissatgeEnlace(
            IEnlace enlaceActiveX)
        {
            try
            {
                return enlaceActiveX == null
                    ? string.Empty
                    : enlaceActiveX.sMensaje ?? string.Empty;
            }
            catch
            {
                return string.Empty;
            }
        }

        private static Dictionary<string, object> CrearCampsIniciArticle(
            ArticleImportacio article,
            int posicio,
            int total,
            string operacioPrevista)
        {
            return new Dictionary<string, object>
            {
                {
                    "Fila",
                    article?.NumeroFila ?? 0
                },
                {
                    "CODART",
                    article?.CodiArticle ?? string.Empty
                },
                {
                    "Posicio",
                    posicio
                },
                {
                    "Total",
                    total
                },
                {
                    "OperacioPrevista",
                    operacioPrevista ?? string.Empty
                }
            };
        }

        private static Dictionary<string, object> CrearCampsResultatCorrecte(
            ArticleImportacio article,
            string operacioRealitzada,
            ResultatImportacioArticle resultatArticle,
            long duradaMs)
        {
            return new Dictionary<string, object>
            {
                {
                    "Fila",
                    article?.NumeroFila ?? 0
                },
                {
                    "CODART",
                    article?.CodiArticle ?? string.Empty
                },
                {
                    "OperacioRealitzada",
                    operacioRealitzada ?? string.Empty
                },
                {
                    "Creat",
                    resultatArticle != null &&
                    resultatArticle.Creat
                },
                {
                    "Actualitzat",
                    resultatArticle != null &&
                    resultatArticle.Actualitzat
                },
                {
                    "DuradaMs",
                    duradaMs
                }
            };
        }

        private static Dictionary<string, object> CrearCampsErrorArticle(
            ArticleImportacio article,
            string operacioPrevista,
            string etapa)
        {
            Dictionary<string, object> camps =
                CrearCampsArticle(
                    article);

            camps["Etapa"] =
                etapa ?? string.Empty;

            camps["OperacioPrevista"] =
                operacioPrevista ?? string.Empty;

            return camps;
        }

        /// <summary>
        /// Construeix els camps variables que es registraran
        /// al log per a un article.
        ///
        /// Només s'afegeixen els camps informats al fitxer.
        /// Els camps antics no participen en el registre.
        /// </summary>
        private static Dictionary<string, object> CrearCampsArticle(
            ArticleImportacio article)
        {
            Dictionary<string, object> camps =
                new Dictionary<string, object>();

            if (article == null)
            {
                return camps;
            }

            camps.Add(
                "Fila",
                article.NumeroFila);

            camps.Add(
                "CODART",
                article.CodiArticle ?? string.Empty);

            AfegirTextSiInformat(
                camps,
                "DESCART",
                article.Descripcio);

            AfegirTextSiInformat(
                camps,
                "CAR1",
                article.Familia);

            AfegirTextSiInformat(
                camps,
                "CAR2",
                article.Unitats);

            AfegirTextSiInformat(
                camps,
                "PARAM1",
                article.TipusUnitat);

            AfegirTextSiInformat(
                camps,
                "CODPRO",
                article.CodiProveidor);

            AfegirTextSiInformat(
                camps,
                "ARTPRO",
                article.ReferenciaProveidor);

            AfegirDecimalSiInformat(
                camps,
                "PRCCOMPRA",
                article.PreuCompra);

            AfegirDecimalSiInformat(
                camps,
                "DESC1",
                article.Descompte1);

            AfegirDecimalSiInformat(
                camps,
                "DESC2",
                article.Descompte2);

            AfegirDecimalSiInformat(
                camps,
                "DESC3",
                article.Descompte3);

            AfegirDecimalSiInformat(
                camps,
                "PRCCOSTE",
                article.PreuCost);

            AfegirDecimalSiInformat(
                camps,
                "PRCSTANDARD",
                article.PreuTransport);

            AfegirEnterSiInformat(
                camps,
                "IDFORMULA",
                article.IdFormula);

            return camps;
        }

        /// <summary>
        /// Afegeix un camp textual només
        /// quan està informat.
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
        /// Afegeix un camp decimal només
        /// quan està informat.
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
        /// Afegeix un camp enter només
        /// quan està informat.
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
        /// Aplica a la fila el resultat retornat
        /// per ActiveX.
        /// </summary>
        private static void ActualitzarArticle(
            ArticleImportacio article,
            ResultatImportacioArticle resultat,
            ResultatImportacioLot resultatLot)
        {
            if (resultat == null)
            {
                article.Estat =
                    EstatImportacio.Error;

                article.Seleccionat =
                    false;

                article.Missatge =
                    "a3ERP no ha retornat cap resultat d'importació.";

                resultatLot.RegistrarError();

                return;
            }

            article.Seleccionat =
                false;

            article.Missatge =
                resultat.Missatge;

            if (!resultat.Correcte)
            {
                article.Estat =
                    EstatImportacio.Error;

                resultatLot.RegistrarError();

                return;
            }

            article.Estat =
                EstatImportacio.Importat;

            if (resultat.Creat)
            {
                resultatLot.RegistrarCreat();

                return;
            }

            if (resultat.Actualitzat)
            {
                resultatLot.RegistrarActualitzat();

                return;
            }

            /*
             * Cas defensiu: l'operació és correcta,
             * però no indica si s'ha creat o actualitzat.
             */
            resultatLot.RegistrarActualitzat();
        }
    }
}
