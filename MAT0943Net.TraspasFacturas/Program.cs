using MAT0943Net.TraspasFacturas.Forms;
using MAT0943Net.TraspasFacturas.Infrastructure.Logging;
using System;
using System.Windows.Forms;

namespace MAT0943Net.TraspasFacturas
{
    internal static class Program
    {
        private const string ArgumentBaseDatosOrigen =
            "--baseDatosOrigen";

        [STAThread]
        private static void Main(string[] args)
        {
            string baseDatosOrigen =
                ObtenirBaseDatosOrigen(args);

            InicialitzadorLogTraspasFacturas.Inicialitzar(
                baseDatosOrigen);

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            Application.Run(
                new FrmTraspasFacturas(
                    baseDatosOrigen));
        }

        private static string ObtenirBaseDatosOrigen(
            string[] args)
        {
            if (args == null ||
                args.Length == 0)
            {
                return string.Empty;
            }

            for (int index = 0;
                 index < args.Length;
                 index++)
            {
                if (!string.Equals(
                    args[index],
                    ArgumentBaseDatosOrigen,
                    StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                int indexValor =
                    index + 1;

                if (indexValor >= args.Length)
                {
                    return string.Empty;
                }

                return args[indexValor] ?? string.Empty;
            }

            return string.Empty;
        }
    }
}