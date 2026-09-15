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

            if (args == null ||
                args.Length != 2)
            {
                Console.Error.WriteLine(
                    "Ús: MAT0943Net.ActiveXPoC.exe CODART \"NOVA DESCART\"");

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
                Console.Error.WriteLine("Error COM no controlat pel test.");
                Console.Error.WriteLine(
                    "HRESULT: 0x" +
                    ex.ErrorCode.ToString("X8"));
                Console.Error.WriteLine(
                    "Missatge: " +
                    ex.Message);
                Console.Error.WriteLine(ex.ToString());

                return 1;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine();
                Console.Error.WriteLine("Error no controlat pel test.");
                Console.Error.WriteLine(ex.ToString());

                return 1;
            }
        }
    }
}
