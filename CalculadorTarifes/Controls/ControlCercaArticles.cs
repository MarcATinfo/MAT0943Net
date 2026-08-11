using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace A3ErpCalculadorTarifes.Controls
{
    public sealed class ControlCercaArticles : UserControl
    {
        private const int EmSetCueBanner =
            0x1501;

        private const int AmpladaIcona =
            32;

        private const int GruixVora =
            1;

        private readonly Panel pnlIconaCerca;

        private readonly TextBox txtCerca;

        private readonly Panel pnlNetejar;

        private readonly ToolTip toolTip;

        private bool _teFocusIntern;

        [DllImport(
            "user32.dll",
            CharSet = CharSet.Unicode)]
        private static extern IntPtr SendMessage(
            IntPtr hWnd,
            int msg,
            IntPtr wParam,
            string lParam);

        public ControlCercaArticles()
        {
            SetStyle(
                ControlStyles.UserPaint
                | ControlStyles.AllPaintingInWmPaint
                | ControlStyles.OptimizedDoubleBuffer
                | ControlStyles.ResizeRedraw,
                true);

            pnlIconaCerca =
                new Panel();

            pnlIconaCerca.BackColor =
                Color.White;

            pnlIconaCerca.Cursor =
                Cursors.Hand;

            pnlIconaCerca.Margin =
                new Padding(
                    0);

            txtCerca =
                new TextBox();

            txtCerca.BorderStyle =
                BorderStyle.None;

            txtCerca.Font =
                new Font(
                    "Segoe UI",
                    9.5F);

            txtCerca.ForeColor =
                Color.FromArgb(
                    30,
                    41,
                    59);

            txtCerca.Margin =
                new Padding(
                    0);

            pnlNetejar =
                new Panel();

            pnlNetejar.BackColor =
                Color.White;

            pnlNetejar.Cursor =
                Cursors.Hand;

            pnlNetejar.Margin =
                new Padding(
                    0);

            pnlNetejar.Visible =
                false;

            Controls.Add(
                pnlIconaCerca);

            Controls.Add(
                txtCerca);

            Controls.Add(
                pnlNetejar);

            toolTip =
                new ToolTip();

            toolTip.SetToolTip(
                pnlIconaCerca,
                "Cercar");

            toolTip.SetToolTip(
                pnlNetejar,
                "Netejar cerca");

            pnlIconaCerca.Paint +=
                pnlIconaCerca_Paint;

            pnlIconaCerca.Click +=
                pnlIconaCerca_Click;

            txtCerca.TextChanged +=
                txtCerca_TextChanged;

            txtCerca.KeyDown +=
                txtCerca_KeyDown;

            txtCerca.HandleCreated +=
                txtCerca_HandleCreated;

            txtCerca.Enter +=
                controlIntern_Enter;

            txtCerca.Leave +=
                controlIntern_Leave;

            pnlNetejar.Paint +=
                pnlNetejar_Paint;

            pnlNetejar.Click +=
                pnlNetejar_Click;

            BackColor =
                Color.White;

            MinimumSize =
                new Size(
                    180,
                    32);

            Size =
                new Size(
                    540,
                    34);

            Margin =
                new Padding(
                    0);

            Padding =
                new Padding(
                    0);

            AplicarTextOrientatiu();
            PerformLayout();
        }

        public event EventHandler CercaSollicitada;

        public event EventHandler NetejaSollicitada;

        public string TextCerca
        {
            get
            {
                return txtCerca.Text;
            }
            set
            {
                txtCerca.Text =
                    value ?? string.Empty;
            }
        }

        public void EnfocarCerca()
        {
            txtCerca.Focus();
        }

        protected override void Dispose(
            bool disposing)
        {
            if (disposing)
            {
                if (toolTip != null)
                {
                    toolTip.Dispose();
                }
            }

            base.Dispose(
                disposing);
        }

        protected override void OnLayout(
            LayoutEventArgs e)
        {
            base.OnLayout(
                e);

            if (pnlIconaCerca == null ||
                pnlNetejar == null ||
                txtCerca == null)
            {
                return;
            }

            int alturaInterior =
                Math.Max(
                    0,
                    Height - (GruixVora * 2));

            int ampladaNetejar =
                pnlNetejar.Visible
                    ? AmpladaIcona
                    : 0;

            pnlIconaCerca.SetBounds(
                GruixVora,
                GruixVora,
                AmpladaIcona,
                alturaInterior);

            pnlNetejar.SetBounds(
                Width - GruixVora - ampladaNetejar,
                GruixVora,
                ampladaNetejar,
                alturaInterior);

            int xText =
                GruixVora + AmpladaIcona;

            int ampladaText =
                Math.Max(
                    0,
                    Width - (GruixVora * 2) - AmpladaIcona - ampladaNetejar);

            int alturaText =
                txtCerca.PreferredHeight;

            int yText =
                GruixVora
                + Math.Max(
                    0,
                    (alturaInterior - alturaText) / 2);

            txtCerca.SetBounds(
                xText,
                yText,
                ampladaText,
                alturaText);
        }

        protected override void OnPaint(
            PaintEventArgs e)
        {
            base.OnPaint(
                e);

            Color colorVora =
                _teFocusIntern
                    ? Color.FromArgb(
                        14,
                        165,
                        233)
                    : Color.FromArgb(
                        203,
                        213,
                        225);

            using (Pen pen = new Pen(colorVora))
            {
                Rectangle rectangle =
                    new Rectangle(
                        0,
                        0,
                        Width - 1,
                        Height - 1);

                e.Graphics.DrawRectangle(
                    pen,
                    rectangle);
            }
        }

        protected override void OnBackColorChanged(
            EventArgs e)
        {
            base.OnBackColorChanged(
                e);

            if (pnlIconaCerca != null)
            {
                pnlIconaCerca.BackColor =
                    BackColor;
            }

            if (txtCerca != null)
            {
                txtCerca.BackColor =
                    BackColor;
            }

            if (pnlNetejar != null)
            {
                pnlNetejar.BackColor =
                    BackColor;
            }
        }

        private void txtCerca_KeyDown(
            object sender,
            KeyEventArgs e)
        {
            if (e.KeyCode != Keys.Enter)
            {
                return;
            }

            e.SuppressKeyPress =
                true;

            OnCercaSollicitada(
                EventArgs.Empty);
        }

        private void txtCerca_TextChanged(
            object sender,
            EventArgs e)
        {
            pnlNetejar.Visible =
                txtCerca.TextLength > 0;

            PerformLayout();
        }

        private void txtCerca_HandleCreated(
            object sender,
            EventArgs e)
        {
            AplicarTextOrientatiu();
        }

        private void pnlIconaCerca_Click(
            object sender,
            EventArgs e)
        {
            OnCercaSollicitada(
                EventArgs.Empty);
        }

        private void pnlNetejar_Click(
            object sender,
            EventArgs e)
        {
            if (txtCerca.TextLength > 0)
            {
                txtCerca.Clear();
            }

            OnNetejaSollicitada(
                EventArgs.Empty);

            txtCerca.Focus();
        }

        private void controlIntern_Enter(
            object sender,
            EventArgs e)
        {
            _teFocusIntern =
                true;

            Invalidate();
        }

        private void controlIntern_Leave(
            object sender,
            EventArgs e)
        {
            if (!IsHandleCreated || IsDisposed)
            {
                return;
            }

            BeginInvoke(
                new MethodInvoker(ActualitzarFocusIntern));
        }

        private void ActualitzarFocusIntern()
        {
            bool teFocus =
                ContainsFocus;

            if (_teFocusIntern == teFocus)
            {
                return;
            }

            _teFocusIntern =
                teFocus;

            Invalidate();
        }

        private void pnlIconaCerca_Paint(
            object sender,
            PaintEventArgs e)
        {
            e.Graphics.SmoothingMode =
                SmoothingMode.AntiAlias;

            using (Pen pen = new Pen(Color.DimGray, 1.6F))
            {
                int centreX =
                    pnlIconaCerca.Width / 2;

                int centreY =
                    pnlIconaCerca.Height / 2;

                Rectangle cercle =
                    new Rectangle(
                        centreX - 7,
                        centreY - 7,
                        11,
                        11);

                e.Graphics.DrawEllipse(
                    pen,
                    cercle);

                e.Graphics.DrawLine(
                    pen,
                    centreX + 2,
                    centreY + 2,
                    centreX + 8,
                    centreY + 8);
            }
        }

        private void pnlNetejar_Paint(
            object sender,
            PaintEventArgs e)
        {
            e.Graphics.SmoothingMode =
                SmoothingMode.AntiAlias;

            using (Pen pen = new Pen(Color.DimGray, 1.7F))
            {
                int centreX =
                    pnlNetejar.Width / 2;

                int centreY =
                    pnlNetejar.Height / 2;

                int radi =
                    5;

                e.Graphics.DrawLine(
                    pen,
                    centreX - radi,
                    centreY - radi,
                    centreX + radi,
                    centreY + radi);

                e.Graphics.DrawLine(
                    pen,
                    centreX + radi,
                    centreY - radi,
                    centreX - radi,
                    centreY + radi);
            }
        }

        private void AplicarTextOrientatiu()
        {
            if (!txtCerca.IsHandleCreated)
            {
                return;
            }

            SendMessage(
                txtCerca.Handle,
                EmSetCueBanner,
                IntPtr.Zero,
                "Cercar per codi o descripció...");
        }

        private void OnCercaSollicitada(
            EventArgs e)
        {
            EventHandler handler =
                CercaSollicitada;

            if (handler != null)
            {
                handler(
                    this,
                    e);
            }
        }

        private void OnNetejaSollicitada(
            EventArgs e)
        {
            EventHandler handler =
                NetejaSollicitada;

            if (handler != null)
            {
                handler(
                    this,
                    e);
            }
        }
    }
}
