using System;
using System.Drawing;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;

namespace MAT0943Net.TraspasFacturas.Infrastructure.UI
{
    internal sealed class DataGridViewCheckBoxHeaderCell :
        DataGridViewColumnHeaderCell
    {
        public bool Checked { get; set; }

        public event EventHandler CheckedChanged;

        protected override void Paint(
            Graphics graphics,
            Rectangle clipBounds,
            Rectangle cellBounds,
            int rowIndex,
            DataGridViewElementStates dataGridViewElementState,
            object value,
            object formattedValue,
            string errorText,
            DataGridViewCellStyle cellStyle,
            DataGridViewAdvancedBorderStyle advancedBorderStyle,
            DataGridViewPaintParts paintParts)
        {
            base.Paint(
                graphics,
                clipBounds,
                cellBounds,
                rowIndex,
                dataGridViewElementState,
                value,
                formattedValue,
                errorText,
                cellStyle,
                advancedBorderStyle,
                paintParts &
                ~DataGridViewPaintParts.ContentForeground);

            CheckBoxState estat =
                Checked
                    ? CheckBoxState.CheckedNormal
                    : CheckBoxState.UncheckedNormal;

            Size mida =
                CheckBoxRenderer.GetGlyphSize(
                    graphics,
                    estat);

            Point punt =
                new Point(
                    cellBounds.Left +
                    (cellBounds.Width - mida.Width) / 2,
                    cellBounds.Top +
                    (cellBounds.Height - mida.Height) / 2);

            CheckBoxRenderer.DrawCheckBox(
                graphics,
                punt,
                estat);
        }

        protected override void OnMouseClick(
            DataGridViewCellMouseEventArgs e)
        {
            Checked =
                !Checked;

            DataGridView?.InvalidateCell(
                ColumnIndex,
                -1);

            CheckedChanged?.Invoke(
                this,
                EventArgs.Empty);

            base.OnMouseClick(e);
        }
    }
}