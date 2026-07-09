using DevExpress.Utils;
using DevExpress.XtraEditors;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace Accounting.Form
{
    /// <summary>
    /// Builds the Chart Of Account header (Periode / Aksi / Filter Tampilan) inside the form's
    /// <see cref="SidePanel"/>. Every size is derived from the form's current DPI, so the caller
    /// must call <see cref="CoaHeaderHandle.Relayout"/> once the form is loaded and again whenever
    /// the DPI changes.
    /// </summary>
    internal static class CoaHeaderLayout
    {
        private const float HeaderFontSize = 10F;
        private const int LogicalMonthWidth = 140;
        private const int LogicalYearWidth = 90;
        private const int LogicalTipeAkunWidth = 188;

        public static CoaHeaderHandle Apply(
            XtraForm form,
            SidePanel host,
            LabelControl periodeCaption,
            ComboBoxEdit month,
            SpinEdit year,
            SimpleButton add,
            SimpleButton edit,
            SimpleButton delete,
            SimpleButton export,
            SimpleButton exportAdvanced,
            SimpleButton refresh,
            CheckEdit neraca,
            CheckEdit labaRugi,
            CheckEdit tbm,
            CheckEdit tm,
            CheckEdit mutasi,
            CheckEdit saldo,
            CheckEdit group,
            CheckEdit detail,
            LabelControl tipeAkunCaption,
            LookUpEdit tipeAkun)
        {
            float scale = GetScale(form);

            form.SuspendLayout();
            host.SuspendLayout();

            FlowLayoutPanel toolbarFlow = new()
            {
                AutoScroll = false,
                AutoSize = false,
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.LeftToRight,
                Margin = Padding.Empty,
                Padding = ScalePadding(8, 6, 8, 6, scale),
                WrapContents = true
            };

            ConfigureLabel(periodeCaption);
            ConfigureLabel(tipeAkunCaption);
            ConfigureEditor(month);
            ConfigureEditor(year);
            ConfigureEditor(tipeAkun);
            ConfigureCheck(neraca);
            ConfigureCheck(labaRugi);
            ConfigureCheck(tbm);
            ConfigureCheck(tm);
            ConfigureCheck(mutasi);
            ConfigureCheck(saldo);
            ConfigureCheck(group);
            ConfigureCheck(detail);
            ConfigureButton(add);
            ConfigureButton(edit);
            ConfigureButton(delete);
            ConfigureButton(export);
            ConfigureButton(exportAdvanced);
            ConfigureButton(refresh);

            GroupControl periodeGroup = CreateGroup("Periode", scale, out TableLayoutPanel periodeTable, 2, 2);
            periodeTable.Controls.Add(periodeCaption, 0, 0);
            periodeTable.SetRowSpan(periodeCaption, 2);
            periodeTable.Controls.Add(month, 1, 0);
            periodeTable.Controls.Add(year, 1, 1);

            GroupControl aksiGroup = CreateGroup("Aksi", scale, out TableLayoutPanel aksiTable, 3, 2);
            aksiTable.Controls.Add(add, 0, 0);
            aksiTable.Controls.Add(edit, 1, 0);
            aksiTable.Controls.Add(delete, 2, 0);
            aksiTable.Controls.Add(export, 0, 1);
            if (exportAdvanced == null)
            {
                aksiTable.Controls.Add(refresh, 1, 1);
            }
            else
            {
                aksiTable.Controls.Add(exportAdvanced, 1, 1);
                aksiTable.Controls.Add(refresh, 2, 1);
            }

            GroupControl filterGroup = CreateGroup("Filter Tampilan", scale, out TableLayoutPanel filterTable, 6, 2);
            filterTable.Controls.Add(neraca, 0, 0);
            filterTable.Controls.Add(labaRugi, 1, 0);
            filterTable.Controls.Add(tbm, 2, 0);
            filterTable.Controls.Add(tm, 3, 0);
            filterTable.Controls.Add(tipeAkunCaption, 4, 0);
            filterTable.Controls.Add(tipeAkun, 5, 0);
            filterTable.Controls.Add(mutasi, 0, 1);
            filterTable.Controls.Add(saldo, 1, 1);
            filterTable.Controls.Add(group, 2, 1);
            filterTable.Controls.Add(detail, 3, 1);

            toolbarFlow.Controls.AddRange(new Control[] { periodeGroup, aksiGroup, filterGroup });

            host.Controls.Clear();
            host.Controls.Add(toolbarFlow);

            host.ResumeLayout(false);
            form.ResumeLayout(false);

            List<SimpleButton> buttons = new() { add, edit, delete, export, refresh };
            if (exportAdvanced != null)
            {
                buttons.Add(exportAdvanced);
            }

            CoaHeaderHandle handle = new(
                form,
                host,
                toolbarFlow,
                new[] { periodeGroup, aksiGroup, filterGroup },
                buttons,
                new List<KeyValuePair<Control, int>>
                {
                    new(month, LogicalMonthWidth),
                    new(year, LogicalYearWidth),
                    new(tipeAkun, LogicalTipeAkunWidth)
                });

            handle.Relayout();
            return handle;
        }

        private static GroupControl CreateGroup(string caption, float scale, out TableLayoutPanel table, int columns, int rows)
        {
            table = new TableLayoutPanel
            {
                AutoSize = false,
                ColumnCount = columns,
                Dock = DockStyle.Fill,
                Margin = Padding.Empty,
                Padding = ScalePadding(6, 2, 6, 2, scale),
                RowCount = rows
            };
            for (int i = 0; i < columns; i++)
            {
                table.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            }
            for (int i = 0; i < rows; i++)
            {
                table.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            }

            GroupControl group = new()
            {
                AutoSize = false,
                Margin = ScalePadding(0, 0, 8, 8, scale),
                Text = caption
            };
            group.Controls.Add(table);
            return group;
        }

        private static void ConfigureLabel(LabelControl label)
        {
            label.Anchor = AnchorStyles.Left;
            label.AutoSizeMode = LabelAutoSizeMode.Default;
            label.Appearance.Font = CreateHeaderFont();
            label.Appearance.Options.UseFont = true;
            label.Appearance.TextOptions.VAlignment = VertAlignment.Center;
        }

        private static void ConfigureEditor(BaseEdit editor)
        {
            editor.Anchor = AnchorStyles.Left | AnchorStyles.Right;
            editor.AutoSize = false;
            editor.Properties.Appearance.Font = CreateHeaderFont();
            editor.Properties.Appearance.Options.UseFont = true;
        }

        private static void ConfigureCheck(CheckEdit check)
        {
            check.Anchor = AnchorStyles.Left;
            check.AutoSize = true;
            check.Properties.AutoWidth = true;
            check.Properties.Appearance.Font = CreateHeaderFont();
            check.Properties.Appearance.Options.UseFont = true;
        }

        private static void ConfigureButton(SimpleButton button)
        {
            if (button == null)
            {
                return;
            }

            button.Anchor = AnchorStyles.Left | AnchorStyles.Right;
            button.AutoSize = false;
            button.Appearance.Font = CreateHeaderFont();
            button.Appearance.Options.UseFont = true;
        }

        /// <summary>
        /// The font every header control is drawn with. Measuring must use this same font, not the
        /// form's inherited font, or rows come out taller than what is actually painted.
        /// </summary>
        internal static Font CreateHeaderFont()
        {
            return new Font("Segoe UI", HeaderFontSize, FontStyle.Regular, GraphicsUnit.Point);
        }

        internal static float GetScale(Control control)
        {
            return control.DeviceDpi > 0 ? control.DeviceDpi / 96F : 1F;
        }

        internal static int Scale(int value, float scale)
        {
            return (int)Math.Round(value * scale);
        }

        internal static Padding ScalePadding(int left, int top, int right, int bottom, float scale)
        {
            return new Padding(Scale(left, scale), Scale(top, scale), Scale(right, scale), Scale(bottom, scale));
        }
    }
}
