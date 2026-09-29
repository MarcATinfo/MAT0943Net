using System.Drawing;
using System.Windows.Forms;

namespace MAT0943Net.TraspasFacturas.Infrastructure.UI
{
    internal static class TraspasFacturasTheme
    {
        public static readonly Color Fons =
            Color.FromArgb(245, 247, 250);

        public static readonly Color Capcalera =
            Color.FromArgb(15, 23, 42);

        public static readonly Color Principal =
            Color.FromArgb(28, 43, 65);

        public static readonly Color TextPrincipal =
            Color.FromArgb(30, 41, 59);

        public static readonly Color TextSecundari =
            Color.FromArgb(100, 116, 139);

        public static readonly Color TextPeu =
            Color.FromArgb(71, 85, 105);

        public static readonly Color TextCapcaleraSecundari =
            Color.FromArgb(203, 213, 225);

        public static readonly Color TextEtiquetaCapcalera =
            Color.FromArgb(150, 170, 194);

        public static readonly Color Accent =
            Color.FromArgb(24, 132, 150);

        public static readonly Color AccioPrimaria =
            Color.FromArgb(14, 165, 233);

        public static readonly Color AccioConfirmacio =
            Color.FromArgb(5, 150, 105);

        public static readonly Color Borde =
            Color.FromArgb(203, 213, 225);

        public static readonly Color BordeGraella =
            Color.FromArgb(232, 236, 241);

        public static readonly Color FonsPasInactiu =
            Color.FromArgb(226, 231, 237);

        public static readonly Color FonsBotoDeshabilitat =
            Color.FromArgb(218, 224, 232);

        public static readonly Color TextBotoDeshabilitat =
            Color.FromArgb(125, 136, 150);

        public static readonly Color SeleccioGraella =
            Color.FromArgb(220, 235, 240);

        public static Font FontBase(float mida)
        {
            return new Font("Segoe UI", mida);
        }

        public static Font FontSemibold(float mida)
        {
            return new Font("Segoe UI Semibold", mida);
        }

        public static void ConfigurarBoto(
            Button boto,
            string text,
            Color colorFons,
            Color colorText,
            int amplada)
        {
            boto.BackColor = colorFons;
            boto.FlatStyle = FlatStyle.Flat;
            boto.FlatAppearance.BorderColor = Borde;
            boto.Font = FontSemibold(9.5F);
            boto.ForeColor = colorText;
            boto.Margin = new Padding(6, 0, 0, 0);
            boto.Size = new Size(amplada, 44);
            boto.Text = text;
            boto.UseVisualStyleBackColor = false;
        }
    }
}
