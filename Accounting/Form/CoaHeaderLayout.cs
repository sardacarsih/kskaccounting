using DevExpress.XtraEditors;
using DevExpress.XtraGrid.Views.Grid;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace Accounting.Form
{
    /// <summary>
    /// Builds the Chart Of Account header (Periode / Aksi / Filter Akun) inside the form's
    /// <see cref="SidePanel"/>. Every size is derived from the form's current DPI, so the caller
    /// must call <see cref="CoaHeaderHandle.Relayout"/> once the form is loaded and again whenever
    /// the DPI changes.
    /// </summary>
    internal static class CoaHeaderLayout
    {
        private const float HeaderFontSize = 10F;
        private const int LogicalMonthWidth = 140;
        private const int LogicalYearWidth = 90;
        private const int LogicalAccountFilterWidth = 180;
        private const int LogicalNameFilterWidth = 280;

        public static CoaHeaderHandle Apply(
            XtraForm form,
            SidePanel host,
            ComboBoxEdit month,
            SpinEdit year,
            SimpleButton previousPeriod,
            SimpleButton nextPeriod,
            SimpleButton add,
            SimpleButton edit,
            SimpleButton delete,
            SimpleButton export,
            SimpleButton refresh,
            GridView accountView)
        {
            float scale = GetScale(form);

            SearchControl accountFilter = new()
            {
                Name = "accountFilterControl"
            };
            accountFilter.Properties.NullValuePrompt = "Account: 11 atau 11,13";
            accountFilter.Properties.NullValuePromptShowForEmptyValue = true;
            accountFilter.Properties.ShowClearButton = true;

            SearchControl nameFilter = new()
            {
                Name = "accountNameFilterControl"
            };
            nameFilter.Properties.NullValuePrompt = "Nama Perkiraan";
            nameFilter.Properties.NullValuePromptShowForEmptyValue = true;
            nameFilter.Properties.ShowClearButton = true;

            void ApplyAccountFilter()
            {
                accountView.ActiveFilterCriteria = CoaAccountSearchFilter.CreateCriteria(
                    accountFilter.Text,
                    nameFilter.Text);
            }

            accountFilter.EditValueChanged += (_, _) => ApplyAccountFilter();
            nameFilter.EditValueChanged += (_, _) => ApplyAccountFilter();

            accountView.OptionsView.ShowAutoFilterRow = false;

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

            ConfigureEditor(month);
            ConfigureEditor(year);
            ConfigureEditor(accountFilter);
            ConfigureEditor(nameFilter);
            ConfigureButton(previousPeriod);
            ConfigureButton(nextPeriod);
            ConfigureButton(add);
            ConfigureButton(edit);
            ConfigureButton(delete);
            ConfigureButton(export);
            ConfigureButton(refresh);

            GroupControl periodeGroup = CreateGroup("Periode", scale, out TableLayoutPanel periodeTable, 4, 1);
            periodeTable.Controls.Add(previousPeriod, 0, 0);
            periodeTable.Controls.Add(month, 1, 0);
            periodeTable.Controls.Add(year, 2, 0);
            periodeTable.Controls.Add(nextPeriod, 3, 0);

            GroupControl aksiGroup = CreateGroup("Aksi", scale, out TableLayoutPanel aksiTable, 4, 1);
            aksiTable.Controls.Add(add, 0, 0);
            aksiTable.Controls.Add(edit, 1, 0);
            aksiTable.Controls.Add(delete, 2, 0);
            aksiTable.Controls.Add(refresh, 3, 0);

            GroupControl filterGroup = CreateGroup("Filter Akun", scale, out TableLayoutPanel filterTable, 3, 1);
            filterTable.Controls.Add(accountFilter, 0, 0);
            filterTable.Controls.Add(nameFilter, 1, 0);
            filterTable.Controls.Add(export, 2, 0);

            toolbarFlow.Controls.AddRange(new Control[] { periodeGroup, aksiGroup, filterGroup });

            host.Controls.Clear();
            host.Controls.Add(toolbarFlow);

            host.ResumeLayout(false);
            form.ResumeLayout(false);

            List<SimpleButton> buttons = new() { previousPeriod, nextPeriod, add, edit, delete, export, refresh };

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
                    new(accountFilter, LogicalAccountFilterWidth),
                    new(nameFilter, LogicalNameFilterWidth)
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

        private static void ConfigureEditor(BaseEdit editor)
        {
            editor.Anchor = AnchorStyles.Left | AnchorStyles.Right;
            editor.AutoSize = false;
            editor.Properties.Appearance.Font = CreateHeaderFont();
            editor.Properties.Appearance.Options.UseFont = true;
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
