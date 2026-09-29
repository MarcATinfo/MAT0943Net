using System;
using System.Runtime.InteropServices;

namespace MAT0943Net.ActiveXPoC
{
    internal static class Program
    {
        private static int Main(string[] args)
        {
            Console.OutputEncoding =
                System.Text.Encoding.UTF8;

            // NOVA PROVA: llistar empreses d'a3ERPActiveX
            if (args != null &&
                args.Length == 1 &&
                string.Equals(
                    args[0],
                    "--empresas",
                    StringComparison.OrdinalIgnoreCase))
            {
                return ProvarEmpresas();
            }

            // FUNCIONAMENT EXISTENT: prova d'articles
            if (args == null ||
                args.Length != 2)
            {
                Console.Error.WriteLine(
                    "Ús:");
                Console.Error.WriteLine(
                    "  MAT0943Net.ActiveXPoC.exe CODART \"NOVA DESCART\"");
                Console.Error.WriteLine(
                    "  MAT0943Net.ActiveXPoC.exe --empresas");

                return 2;
            }

            string codiArticle =
                args[0];

            string novaDescripcio =
                args[1];

            try
            {
                ActiveXArticleTest test =
                    new ActiveXArticleTest();

                test.Executar(
                    codiArticle,
                    novaDescripcio);

                return 0;
            }
            catch (COMException ex)
            {
                Console.Error.WriteLine();
                Console.Error.WriteLine(
                    "Error COM no controlat pel test.");

                Console.Error.WriteLine(
                    "HRESULT: 0x" +
                    ex.ErrorCode.ToString("X8"));

                Console.Error.WriteLine(
                    "Missatge: " +
                    ex.Message);

                Console.Error.WriteLine(
                    ex.ToString());

                return 1;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine();
                Console.Error.WriteLine(
                    "Error no controlat pel test.");

                Console.Error.WriteLine(
                    ex.ToString());

                return 1;
            }
        }

        private static int ProvarEmpresas()
        {
            a3ERPActiveX.Enlace enlace =
                null;

            try
            {
                enlace =
                    new a3ERPActiveX.Enlace();

                enlace.RaiseOnException = true;

                // IMPORTANT:
                // posa aquí les mateixes credencials
                // que utilitzes al projecte de traspàs.
                string usuari =
                    "sa";

                string password =
                    "";

                bool loginOk =
                    enlace.LoginUsuario(
                        usuari,
                        password);

                if (!loginOk)
                {
                    Console.Error.WriteLine(
                        "LoginUsuario ha retornat false.");

                    return 1;
                }

                Console.WriteLine(
                    "LOGIN OK");

                Console.WriteLine();

                // -----------------------------------------
                // PROVA NOVA:
                // empresa ActiveX -> informació connexió SQL
                // -----------------------------------------

                string empresaProva =
                    "CPORRAS A";

                Console.WriteLine(
                    "=== PROVA CONNEXIÓ EMPRESA ===");

                Console.WriteLine(
                    "Empresa ActiveX: " +
                    empresaProva);

                Console.WriteLine();

                object paramConexion =
     enlace.ParamConexion(
         empresaProva);

                Console.WriteLine(
                    "ParamConexion:");

                if (paramConexion == null)
                {
                    Console.WriteLine("<NULL>");
                }
                else
                {
                    Console.WriteLine(
                        "TIPUS: " +
                        paramConexion.GetType().FullName);

                    if (paramConexion is Array arrayConexion)
                    {
                        Console.WriteLine(
                            "RANK: " +
                            arrayConexion.Rank);

                        Console.WriteLine(
                            "LENGTH: " +
                            arrayConexion.Length);

                        for (int i = 0;
                             i < arrayConexion.Rank;
                             i++)
                        {
                            Console.WriteLine(
                                "DIM " +
                                i +
                                ": " +
                                arrayConexion.GetLowerBound(i) +
                                ".." +
                                arrayConexion.GetUpperBound(i));
                        }

                        Console.WriteLine();
                        Console.WriteLine(
                            "VALORS ParamConexion:");

                        int index = 0;

                        foreach (object item in arrayConexion)
                        {
                            Console.WriteLine(
                                "[" +
                                index +
                                "] = " +
                                (
                                    item == null
                                        ? "<NULL>"
                                        : item.ToString()
                                ));

                            index++;
                        }
                    }
                    else
                    {
                        Console.WriteLine(
                            paramConexion.ToString());
                    }
                }

                Console.WriteLine();

                //object conexionDb =
                //    enlace.GetConexionDB(
                //        empresaProva);

                //Console.WriteLine(
                //    "GetConexionDB:");

                //Console.WriteLine(
                //    conexionDb == null
                //        ? "<NULL>"
                //        : conexionDb.ToString());

                //Console.WriteLine(
                //    "TIPUS GetConexionDB: " +
                //    (
                //        conexionDb == null
                //            ? "<NULL>"
                //            : conexionDb
                //                .GetType()
                //                .FullName
                //    ));

                Console.WriteLine(
                    "==============================");

                Console.WriteLine();

                // -----------------------------------------
                // PROVA EXISTENT: Empresas()
                // -----------------------------------------

                object empresas =
                    enlace.Empresas();

                Console.WriteLine(
                    "RESULTAT Empresas()");

                Console.WriteLine(
                    "------------------");

                if (empresas == null)
                {
                    Console.WriteLine(
                        "Empresas() retorna NULL");

                    return 0;
                }

                Console.WriteLine(
                    "TIPUS: " +
                    empresas.GetType().FullName);

                if (empresas is Array array)
                {
                    Console.WriteLine(
                        "RANK: " +
                        array.Rank);

                    Console.WriteLine(
                        "LENGTH: " +
                        array.Length);

                    for (int i = 0;
                         i < array.Rank;
                         i++)
                    {
                        Console.WriteLine(
                            "DIM " +
                            i +
                            ": " +
                            array.GetLowerBound(i) +
                            ".." +
                            array.GetUpperBound(i));
                    }

                    Console.WriteLine();
                    Console.WriteLine(
                        "VALORS:");

                    foreach (object item in array)
                    {
                        Console.WriteLine(
                            item == null
                                ? "<NULL>"
                                : item.ToString());
                    }
                }
                else
                {
                    Console.WriteLine();
                    Console.WriteLine(
                        "VALOR:");

                    Console.WriteLine(
                        empresas.ToString());
                }

                Console.WriteLine();
                Console.WriteLine(
                    "PROVA INICIAR:");

                enlace.Iniciar(
                    "CPORRAS",
                    "");

                Console.WriteLine(
                    "EMPRESA ACTIVA:");

                Console.WriteLine(
                    enlace.EmpresaActiva);

                enlace.Acabar();

                enlace =
                    null;

                return 0;
            }
            catch (COMException ex)
            {
                Console.Error.WriteLine();

                Console.Error.WriteLine(
                    "Error COM a Empresas().");

                Console.Error.WriteLine(
                    "HRESULT: 0x" +
                    ex.ErrorCode.ToString("X8"));

                Console.Error.WriteLine(
                    "Missatge: " +
                    ex.Message);

                Console.Error.WriteLine(
                    ex.ToString());

                return 1;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine();

                Console.Error.WriteLine(
                    "Error a Empresas().");

                Console.Error.WriteLine(
                    ex.ToString());

                return 1;
            }
            finally
            {
                if (enlace != null)
                {
                    try
                    {
                        if (enlace.Estado ==
                            a3ERPActiveX.EstadoEnlace.estACTIVO)
                        {
                            enlace.Acabar();
                        }
                    }
                    catch
                    {
                    }
                }
            }
        }
    }
}