using System;
using System.Drawing;
using System.Reflection;
using System.Windows.Forms;

namespace MAT0943Net.Infrastructure
{
    internal static class IconaAplicacio
    {
        private const string NomRecursIcona =
            "MAT0943Net.at.ico";

        private static readonly object BloqueigCarrega =
            new object();

        private static Icon _iconaAplicacio;

        private static bool _carregaIntentada;

        public static void Aplicar(
            Form formulari)
        {
            if (formulari == null)
            {
                return;
            }

            Icon icona =
                ObtenirIcona();

            if (icona == null)
            {
                return;
            }

            Icon iconaFormulari =
                null;

            try
            {
                iconaFormulari =
                    (Icon)icona.Clone();

                formulari.Icon =
                    iconaFormulari;

                if (!formulari.ShowIcon)
                {
                    formulari.ShowIcon =
                        true;
                }

                formulari.Disposed +=
                    delegate
                    {
                        iconaFormulari.Dispose();
                    };
            }
            catch
            {
                if (iconaFormulari != null)
                {
                    iconaFormulari.Dispose();
                }

                /*
                 * La icona és només visual. Si el recurs no es pot
                 * aplicar, el formulari ha de continuar obrint-se.
                 */
            }
        }

        private static Icon ObtenirIcona()
        {
            if (_carregaIntentada)
            {
                return _iconaAplicacio;
            }

            lock (BloqueigCarrega)
            {
                if (_carregaIntentada)
                {
                    return _iconaAplicacio;
                }

                _carregaIntentada =
                    true;

                try
                {
                    Assembly assembly =
                        typeof(IconaAplicacio).Assembly;

                    using (System.IO.Stream stream =
                        assembly.GetManifestResourceStream(
                            NomRecursIcona))
                    {
                        if (stream == null)
                        {
                            return null;
                        }

                        using (Icon icona =
                            new Icon(
                                stream))
                        {
                            _iconaAplicacio =
                                (Icon)icona.Clone();
                        }
                    }
                }
                catch
                {
                    _iconaAplicacio =
                        null;
                }

                return _iconaAplicacio;
            }
        }
    }
}
