using a3ERPActiveX;
using System;
using System.Runtime.InteropServices;

namespace MAT0943Net.ActiveXPoC
{
    internal sealed class ActiveXArticleTest
    {
        private const string NomMestreArticles =
            "ARTICULOS";

        public void Executar(
            string codiArticle,
            string novaDescripcio)
        {
            if (string.IsNullOrWhiteSpace(codiArticle))
            {
                throw new ArgumentException(
                    "El codi d'article és obligatori.",
                    nameof(codiArticle));
            }

            if (string.IsNullOrWhiteSpace(novaDescripcio))
            {
                throw new ArgumentException(
                    "La nova descripció és obligatòria.",
                    nameof(novaDescripcio));
            }

            IEnlace enlace =
                null;

            IMaestro maestro =
                null;

            bool maestroIniciat =
                false;

            bool operacioMaestroEnCurs =
                false;

            string estatEnlaceAbansGuarda =
                "NO_INICIAT";

            string estatMaestroAbansGuarda =
                "NO_INICIAT";

            try
            {
                EscriurePas(
                    "Creant Enlace ActiveX.");

                enlace =
                    new Enlace();

                EscriurePas(
                    "Seleccionant l'empresa activa amb SelecEmpresaActiva().");

                bool empresaSeleccionada =
                    enlace.SelecEmpresaActiva();

                EscriureEstatEnlace(
                    enlace,
                    empresaSeleccionada);

                if (!empresaSeleccionada ||
                    enlace.bError ||
                    enlace.Estado != EstadoEnlace.estACTIVO)
                {
                    throw new InvalidOperationException(
                        "No s'ha pogut seleccionar una empresa activa d'a3ERP.");
                }

                EscriurePas(
                    "Creant Maestro ActiveX.");

                maestro =
                    new Maestro();

                EscriurePas(
                    "Iniciant el mestre " +
                    NomMestreArticles +
                    ".");

                maestro.Iniciar(
                    NomMestreArticles);

                maestroIniciat =
                    true;

                EscriureEstatMaestro(
                    maestro,
                    "Després de Iniciar");

                EscriurePas(
                    "Activant OmitirMensajes.");

                maestro.OmitirMensajes =
                    true;

                EscriurePas(
                    "Buscant l'article literal: " +
                    codiArticle);

                bool articleExisteix =
                    maestro.Buscar(
                        codiArticle);

                EscriurePas(
                    "Resultat de Buscar: " +
                    articleExisteix);

                EscriureEstatMaestro(
                    maestro,
                    "Després de Buscar");

                if (!articleExisteix)
                {
                    throw new InvalidOperationException(
                        "No s'ha trobat l'article indicat.");
                }

                EscriurePas(
                    "Obrint l'article en mode edició amb Edita().");

                maestro.Edita();

                operacioMaestroEnCurs =
                    true;

                EscriureEstatMaestro(
                    maestro,
                    "Després de Edita");

                EscriurePas(
                    "Modificant només DESCART.");

                maestro.AsString["DESCART"] =
                    novaDescripcio.Trim();

                estatEnlaceAbansGuarda =
                    ObtenirEstatEnlace(
                        enlace);

                estatMaestroAbansGuarda =
                    ObtenirEstatMaestro(
                        maestro);

                EscriurePas(
                    "Estat abans de Guarda.");

                Console.WriteLine(
                    "  Enlace:  " +
                    estatEnlaceAbansGuarda);

                Console.WriteLine(
                    "  Maestro: " +
                    estatMaestroAbansGuarda);

                EscriurePas(
                    "Executant Maestro.Guarda(true).");

                maestro.Guarda(
                    true);

                operacioMaestroEnCurs =
                    false;

                EscriurePas(
                    "Maestro.Guarda(true) ha finalitzat correctament.");

                EscriureEstatMaestro(
                    maestro,
                    "Després de Guarda");
            }
            catch (COMException ex)
            {
                Console.Error.WriteLine();
                Console.Error.WriteLine("S'ha produït una COMException.");
                Console.Error.WriteLine(
                    "HRESULT: 0x" +
                    ex.ErrorCode.ToString("X8"));
                Console.Error.WriteLine(
                    "Missatge: " +
                    ex.Message);
                Console.Error.WriteLine(
                    "Enlace abans de Guarda: " +
                    estatEnlaceAbansGuarda);
                Console.Error.WriteLine(
                    "Maestro abans de Guarda: " +
                    estatMaestroAbansGuarda);
                EscriureEstatEnlaceError(
                    enlace);
                EscriureEstatMaestroError(
                    maestro);
                Console.Error.WriteLine(ex.ToString());

                throw;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine();
                Console.Error.WriteLine("S'ha produït un error.");
                Console.Error.WriteLine(
                    "Missatge: " +
                    ex.Message);
                Console.Error.WriteLine(
                    "Enlace abans de Guarda: " +
                    estatEnlaceAbansGuarda);
                Console.Error.WriteLine(
                    "Maestro abans de Guarda: " +
                    estatMaestroAbansGuarda);
                EscriureEstatEnlaceError(
                    enlace);
                EscriureEstatMaestroError(
                    maestro);

                throw;
            }
            finally
            {
                if (maestro != null &&
                    operacioMaestroEnCurs)
                {
                    IntentarCancelarMaestro(
                        maestro);
                }

                if (maestro != null &&
                    maestroIniciat)
                {
                    IntentarAcabarMaestro(
                        maestro);
                }

                AlliberarCom(
                    maestro,
                    "Maestro");

                /*
                 * No cridem Enlace.Acabar(): aquest PoC replica el patró
                 * validat amb SelecEmpresaActiva(), on només s'allibera
                 * la referència COM sense tancar la sessió activa d'a3ERP.
                 */
                AlliberarCom(
                    enlace,
                    "Enlace");
            }
        }

        private static void EscriurePas(
            string missatge)
        {
            Console.WriteLine(
                DateTime.Now.ToString("HH:mm:ss.fff") +
                " - " +
                missatge);
        }

        private static void EscriureEstatEnlace(
            IEnlace enlace,
            bool empresaSeleccionada)
        {
            Console.WriteLine(
                "  SelecEmpresaActiva: " +
                empresaSeleccionada);

            Console.WriteLine(
                "  EstadoEnlace: " +
                LlegirSegur(
                    delegate
                    {
                        return enlace.Estado.ToString();
                    }));

            Console.WriteLine(
                "  bError: " +
                LlegirSegur(
                    delegate
                    {
                        return enlace.bError.ToString();
                    }));

            Console.WriteLine(
                "  nError: " +
                LlegirSegur(
                    delegate
                    {
                        return enlace.nError.ToString();
                    }));

            Console.WriteLine(
                "  sMensaje: " +
                LlegirSegur(
                    delegate
                    {
                        return enlace.sMensaje ?? string.Empty;
                    }));

            Console.WriteLine(
                "  Empresa activa: " +
                LlegirSegur(
                    delegate
                    {
                        return enlace.EmpresaActiva ?? string.Empty;
                    }));
        }

        private static void EscriureEstatMaestro(
            IMaestro maestro,
            string context)
        {
            Console.WriteLine(
                "  EstadoMaestro (" +
                context +
                "): " +
                ObtenirEstatMaestro(
                    maestro));
        }

        private static string ObtenirEstatEnlace(
            IEnlace enlace)
        {
            if (enlace == null)
            {
                return "NULL";
            }

            return LlegirSegur(
                delegate
                {
                    return
                        "Estado=" +
                        enlace.Estado +
                        "; bError=" +
                        enlace.bError +
                        "; nError=" +
                        enlace.nError +
                        "; sMensaje=" +
                        (enlace.sMensaje ?? string.Empty) +
                        "; EmpresaActiva=" +
                        (enlace.EmpresaActiva ?? string.Empty);
                });
        }

        private static string ObtenirEstatMaestro(
            IMaestro maestro)
        {
            if (maestro == null)
            {
                return "NULL";
            }

            return LlegirSegur(
                delegate
                {
                    return maestro.Estado.ToString();
                });
        }

        private static void EscriureEstatEnlaceError(
            IEnlace enlace)
        {
            Console.Error.WriteLine(
                "Estat actual Enlace: " +
                ObtenirEstatEnlace(
                    enlace));
        }

        private static void EscriureEstatMaestroError(
            IMaestro maestro)
        {
            Console.Error.WriteLine(
                "Estat actual Maestro: " +
                ObtenirEstatMaestro(
                    maestro));
        }

        private static string LlegirSegur(
            Func<string> llegir)
        {
            try
            {
                return llegir == null
                    ? string.Empty
                    : llegir();
            }
            catch (Exception ex)
            {
                return
                    "ERROR_LLEGINT_ESTAT: " +
                    ex.Message;
            }
        }

        private static void IntentarCancelarMaestro(
            IMaestro maestro)
        {
            try
            {
                EscriurePas(
                    "Cancel·lant l'operació del Maestro després d'un error.");

                maestro.Cancelar();
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(
                    "No s'ha pogut cancel·lar el Maestro: " +
                    ex.Message);
            }
        }

        private static void IntentarAcabarMaestro(
            IMaestro maestro)
        {
            try
            {
                EscriurePas(
                    "Finalitzant Maestro amb Acabar().");

                maestro.Acabar();
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(
                    "No s'ha pogut finalitzar el Maestro: " +
                    ex.Message);
            }
        }

        private static void AlliberarCom(
            object instancia,
            string nom)
        {
            if (instancia == null)
            {
                return;
            }

            try
            {
                if (Marshal.IsComObject(
                    instancia))
                {
                    Marshal.FinalReleaseComObject(
                        instancia);

                    EscriurePas(
                        "Referència COM alliberada: " +
                        nom +
                        ".");
                }
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(
                    "No s'ha pogut alliberar " +
                    nom +
                    ": " +
                    ex.Message);
            }
        }
    }
}
