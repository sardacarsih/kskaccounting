using DevExpress.Utils;
using DevExpress.XtraEditors;
using DevExpress.XtraGrid;
using DevExpress.XtraGrid.Views.Grid;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace Accounting.Form
{
    internal static class JurnalImportFormLayout
    {
        private const int LogicalMinimumWidth = 1024;
        private const int LogicalMinimumHeight = 768;
        private const int CompactWidthThreshold = 1120;

        private enum CommandLayoutMode
        {
            Normal,
            Compact
        }

        public static void Apply(
            XtraForm form,
            ComboBoxEdit cmbbulan,
            SpinEdit setahun,
            SimpleButton sbbrowse,
            LabelControl txtPath,
            LabelControl labelControl1,
            ComboBoxEdit cboSheet,
            SimpleButton SBImport,
            LabelControl lblrecord,
            LabelControl lblImportProgress,
            ProgressBarControl progressImport,
            GridControl gridControl1,
            GridView gridView1)
        {
            float scale = GetScale(form);
            int commandControlHeight = GetCommandControlHeight(form, scale);

            form.SuspendLayout();
            form.AutoScaleDimensions = new SizeF(96F, 96F);
            form.AutoScaleMode = AutoScaleMode.Dpi;
            form.ClientSize = ScaleSize(1184, 681, scale);
            form.MinimumSize = ScaleSize(LogicalMinimumWidth, LogicalMinimumHeight, scale);

            ConfigureCommandControls(cmbbulan, setahun, sbbrowse, txtPath, labelControl1, cboSheet, SBImport, lblrecord, scale, commandControlHeight);
            ConfigureProgressControls(lblImportProgress, progressImport, scale);
            ConfigureGrid(gridControl1, gridView1);

            CommandLayoutMode commandMode = GetCommandLayoutMode(form.ClientSize.Width, scale);
            TableLayoutPanel rootLayout = CreateRootLayout(scale, commandMode, commandControlHeight);
            TableLayoutPanel commandLayout = CreateCommandLayout(scale, commandMode);
            PanelControl gridPanel = CreateGridPanel(gridControl1, scale);

            AddCommandControls(commandLayout, cmbbulan, setahun, sbbrowse, txtPath, labelControl1, cboSheet, SBImport, lblrecord, scale, commandMode);
            rootLayout.Controls.Add(commandLayout, 0, 0);
            rootLayout.Controls.Add(lblImportProgress, 0, 1);
            rootLayout.Controls.Add(progressImport, 0, 2);
            AttachProgressVisibilityHandlers(rootLayout, lblImportProgress, progressImport, scale);
            rootLayout.Controls.Add(gridPanel, 0, 3);

            form.Controls.Add(rootLayout);
            form.Resize += (_, _) =>
            {
                CommandLayoutMode nextMode = GetCommandLayoutMode(form.ClientSize.Width, scale);
                if (nextMode == commandMode)
                {
                    return;
                }

                commandMode = nextMode;
                commandLayout = RebuildCommandLayout(
                    rootLayout,
                    commandLayout,
                    cmbbulan,
                    setahun,
                    sbbrowse,
                    txtPath,
                    labelControl1,
                    cboSheet,
                    SBImport,
                    lblrecord,
                    scale,
                    commandMode,
                    commandControlHeight);
            };
            form.ResumeLayout(false);
        }

        public static void SetPath(LabelControl txtPath, ToolTip filePathToolTip, string path)
        {
            txtPath.Text = string.IsNullOrWhiteSpace(path) ? "Lokasi File" : path;
            filePathToolTip.SetToolTip(txtPath, path);
        }

        private static void ConfigureCommandControls(
            ComboBoxEdit cmbbulan,
            SpinEdit setahun,
            SimpleButton sbbrowse,
            LabelControl txtPath,
            LabelControl labelControl1,
            ComboBoxEdit cboSheet,
            SimpleButton SBImport,
            LabelControl lblrecord,
            float scale,
            int commandControlHeight)
        {
            cmbbulan.Properties.ImmediatePopup = true;
            cmbbulan.Properties.TextEditStyle = DevExpress.XtraEditors.Controls.TextEditStyles.DisableTextEditor;
            cmbbulan.MinimumSize = new Size(Scale(120, scale), commandControlHeight);

            setahun.MinimumSize = new Size(Scale(76, scale), commandControlHeight);

            sbbrowse.ImageOptions.Image = Properties.Resources.open2_16x16;
            sbbrowse.Text = "Browse File";
            sbbrowse.MinimumSize = new Size(Scale(128, scale), commandControlHeight);

            txtPath.Text = "Lokasi File";
            txtPath.AutoSizeMode = LabelAutoSizeMode.None;
            txtPath.Appearance.TextOptions.Trimming = Trimming.EllipsisPath;
            txtPath.Appearance.TextOptions.VAlignment = VertAlignment.Center;
            txtPath.BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.Simple;
            txtPath.Padding = ScalePadding(8, 0, 8, 0, scale);

            labelControl1.Text = "Sheet";
            labelControl1.AutoSizeMode = LabelAutoSizeMode.None;
            labelControl1.Appearance.TextOptions.VAlignment = VertAlignment.Center;

            cboSheet.Properties.TextEditStyle = DevExpress.XtraEditors.Controls.TextEditStyles.DisableTextEditor;
            cboSheet.MinimumSize = new Size(Scale(128, scale), commandControlHeight);

            SBImport.Enabled = false;
            SBImport.ImageOptions.Image = Properties.Resources.editdatasource_16x16;
            SBImport.Text = "Import";
            SBImport.MinimumSize = new Size(Scale(112, scale), commandControlHeight);

            lblrecord.Text = "JlhRecord";
            lblrecord.AutoSizeMode = LabelAutoSizeMode.None;
            lblrecord.Appearance.TextOptions.HAlignment = HorzAlignment.Far;
            lblrecord.Appearance.TextOptions.VAlignment = VertAlignment.Center;
        }

        private static void ConfigureProgressControls(LabelControl lblImportProgress, ProgressBarControl progressImport, float scale)
        {
            lblImportProgress.Dock = DockStyle.Fill;
            lblImportProgress.AutoSizeMode = LabelAutoSizeMode.None;
            lblImportProgress.Appearance.TextOptions.VAlignment = VertAlignment.Center;
            lblImportProgress.MinimumSize = new Size(0, Scale(22, scale));
            lblImportProgress.Visible = false;

            progressImport.Dock = DockStyle.Fill;
            progressImport.MinimumSize = new Size(0, Scale(18, scale));
            progressImport.Properties.Maximum = 100;
            progressImport.Properties.Minimum = 0;
            progressImport.Properties.PercentView = false;
            progressImport.Properties.ShowTitle = true;
            progressImport.Visible = false;
        }

        private static void ConfigureGrid(GridControl gridControl1, GridView gridView1)
        {
            gridControl1.Dock = DockStyle.Fill;
            gridControl1.MainView = gridView1;
            gridControl1.ViewCollection.AddRange(new DevExpress.XtraGrid.Views.Base.BaseView[] { gridView1 });

            gridView1.GridControl = gridControl1;
            gridView1.OptionsBehavior.Editable = false;
            gridView1.OptionsFind.AlwaysVisible = true;
            gridView1.OptionsFind.ShowFindButton = false;
            gridView1.OptionsView.EnableAppearanceEvenRow = true;
            gridView1.OptionsView.EnableAppearanceOddRow = true;
            gridView1.OptionsView.ShowGroupPanel = false;
        }


        private static void AttachProgressVisibilityHandlers(
            TableLayoutPanel rootLayout,
            LabelControl lblImportProgress,
            ProgressBarControl progressImport,
            float scale)
        {
            void ApplyProgressRows()
            {
                rootLayout.RowStyles[1].Height = lblImportProgress.Visible ? Scale(22, scale) : 0F;
                rootLayout.RowStyles[2].Height = progressImport.Visible ? Scale(18, scale) : 0F;
            }

            lblImportProgress.VisibleChanged += (_, _) => ApplyProgressRows();
            progressImport.VisibleChanged += (_, _) => ApplyProgressRows();
            ApplyProgressRows();
        }
        private static TableLayoutPanel CreateRootLayout(float scale, CommandLayoutMode commandMode, int commandControlHeight)
        {
            TableLayoutPanel rootLayout = new()
            {
                ColumnCount = 1,
                Dock = DockStyle.Fill,
                Padding = ScalePadding(8, 8, 8, 8, scale),
                RowCount = 4
            };
            rootLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            rootLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, GetCommandRowHeight(scale, commandMode, commandControlHeight)));
            rootLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 0F));
            rootLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 0F));
            rootLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));

            return rootLayout;
        }

        private static TableLayoutPanel CreateCommandLayout(float scale, CommandLayoutMode commandMode)
        {
            if (commandMode == CommandLayoutMode.Compact)
            {
                return CreateCompactCommandLayout(scale);
            }

            TableLayoutPanel commandLayout = new()
            {
                ColumnCount = 9,
                Dock = DockStyle.Fill,
                RowCount = 1
            };
            commandLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, Scale(58, scale)));
            commandLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, Scale(130, scale)));
            commandLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, Scale(82, scale)));
            commandLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, Scale(136, scale)));
            commandLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            commandLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, Scale(88, scale)));
            commandLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, Scale(48, scale)));
            commandLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, Scale(140, scale)));
            commandLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, Scale(128, scale)));
            commandLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));

            return commandLayout;
        }

        private static TableLayoutPanel CreateCompactCommandLayout(float scale)
        {
            TableLayoutPanel commandLayout = new()
            {
                ColumnCount = 6,
                Dock = DockStyle.Fill,
                RowCount = 2
            };
            commandLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, Scale(58, scale)));
            commandLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, Scale(130, scale)));
            commandLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, Scale(82, scale)));
            commandLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            commandLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, Scale(112, scale)));
            commandLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, Scale(140, scale)));
            commandLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));
            commandLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));

            return commandLayout;
        }

        private static PanelControl CreateGridPanel(GridControl gridControl1, float scale)
        {
            PanelControl gridPanel = new()
            {
                BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.Simple,
                Dock = DockStyle.Fill,
                Padding = ScalePadding(1, 1, 1, 1, scale)
            };
            gridPanel.Controls.Add(gridControl1);

            return gridPanel;
        }

        private static void AddCommandControls(
            TableLayoutPanel commandLayout,
            ComboBoxEdit cmbbulan,
            SpinEdit setahun,
            SimpleButton sbbrowse,
            LabelControl txtPath,
            LabelControl labelControl1,
            ComboBoxEdit cboSheet,
            SimpleButton SBImport,
            LabelControl lblrecord,
            float scale,
            CommandLayoutMode commandMode)
        {
            LabelControl periodLabel = CreateLabel("Periode");

            if (commandMode == CommandLayoutMode.Compact)
            {
                commandLayout.Controls.Add(periodLabel, 0, 0);
                commandLayout.Controls.Add(cmbbulan, 1, 0);
                commandLayout.Controls.Add(setahun, 2, 0);
                commandLayout.Controls.Add(sbbrowse, 3, 0);
                commandLayout.Controls.Add(SBImport, 4, 0);
                commandLayout.Controls.Add(lblrecord, 5, 0);
                commandLayout.Controls.Add(txtPath, 0, 1);
                commandLayout.SetColumnSpan(txtPath, 4);
                commandLayout.Controls.Add(labelControl1, 4, 1);
                commandLayout.Controls.Add(cboSheet, 5, 1);

                SetControlMargin(periodLabel, ScalePadding(0, 2, 8, 2, scale));
                SetControlMargin(cmbbulan, ScalePadding(0, 2, 6, 2, scale));
                SetControlMargin(setahun, ScalePadding(0, 2, 10, 2, scale));
                SetControlMargin(sbbrowse, ScalePadding(0, 2, 8, 2, scale));
                SetControlMargin(SBImport, ScalePadding(0, 2, 8, 2, scale));
                SetControlMargin(lblrecord, ScalePadding(0, 2, 0, 2, scale));
                SetControlMargin(txtPath, ScalePadding(0, 2, 10, 2, scale));
                SetControlMargin(labelControl1, ScalePadding(0, 2, 8, 2, scale));
                SetControlMargin(cboSheet, ScalePadding(0, 2, 0, 2, scale));
                return;
            }

            commandLayout.Controls.Add(periodLabel, 0, 0);
            commandLayout.Controls.Add(cmbbulan, 1, 0);
            commandLayout.Controls.Add(setahun, 2, 0);
            commandLayout.Controls.Add(sbbrowse, 3, 0);
            commandLayout.Controls.Add(txtPath, 4, 0);
            commandLayout.Controls.Add(lblrecord, 5, 0);
            commandLayout.Controls.Add(labelControl1, 6, 0);
            commandLayout.Controls.Add(cboSheet, 7, 0);
            commandLayout.Controls.Add(SBImport, 8, 0);

            SetControlMargin(periodLabel, ScalePadding(0, 2, 8, 2, scale));
            SetControlMargin(cmbbulan, ScalePadding(0, 2, 6, 2, scale));
            SetControlMargin(setahun, ScalePadding(0, 2, 10, 2, scale));
            SetControlMargin(sbbrowse, ScalePadding(0, 2, 8, 2, scale));
            SetControlMargin(txtPath, ScalePadding(0, 2, 10, 2, scale));
            SetControlMargin(lblrecord, ScalePadding(0, 2, 10, 2, scale));
            SetControlMargin(labelControl1, ScalePadding(0, 2, 8, 2, scale));
            SetControlMargin(cboSheet, ScalePadding(0, 2, 8, 2, scale));
            SetControlMargin(SBImport, ScalePadding(0, 2, 0, 2, scale));
        }

        private static TableLayoutPanel RebuildCommandLayout(
            TableLayoutPanel rootLayout,
            TableLayoutPanel currentCommandLayout,
            ComboBoxEdit cmbbulan,
            SpinEdit setahun,
            SimpleButton sbbrowse,
            LabelControl txtPath,
            LabelControl labelControl1,
            ComboBoxEdit cboSheet,
            SimpleButton SBImport,
            LabelControl lblrecord,
            float scale,
            CommandLayoutMode commandMode,
            int commandControlHeight)
        {
            rootLayout.SuspendLayout();
            TableLayoutPanel nextCommandLayout = CreateCommandLayout(scale, commandMode);

            rootLayout.Controls.Remove(currentCommandLayout);
            DetachCommandControls(currentCommandLayout, cmbbulan, setahun, sbbrowse, txtPath, labelControl1, cboSheet, SBImport, lblrecord);
            currentCommandLayout.Dispose();

            AddCommandControls(nextCommandLayout, cmbbulan, setahun, sbbrowse, txtPath, labelControl1, cboSheet, SBImport, lblrecord, scale, commandMode);
            rootLayout.RowStyles[0].Height = GetCommandRowHeight(scale, commandMode, commandControlHeight);
            rootLayout.Controls.Add(nextCommandLayout, 0, 0);
            rootLayout.ResumeLayout(true);

            return nextCommandLayout;
        }

        private static void DetachCommandControls(
            TableLayoutPanel commandLayout,
            ComboBoxEdit cmbbulan,
            SpinEdit setahun,
            SimpleButton sbbrowse,
            LabelControl txtPath,
            LabelControl labelControl1,
            ComboBoxEdit cboSheet,
            SimpleButton SBImport,
            LabelControl lblrecord)
        {
            for (int i = commandLayout.Controls.Count - 1; i >= 0; i--)
            {
                Control control = commandLayout.Controls[i];
                commandLayout.Controls.RemoveAt(i);

                if (!ReferenceEquals(control, cmbbulan)
                    && !ReferenceEquals(control, setahun)
                    && !ReferenceEquals(control, sbbrowse)
                    && !ReferenceEquals(control, txtPath)
                    && !ReferenceEquals(control, labelControl1)
                    && !ReferenceEquals(control, cboSheet)
                    && !ReferenceEquals(control, SBImport)
                    && !ReferenceEquals(control, lblrecord))
                {
                    control.Dispose();
                }
            }
        }

        private static LabelControl CreateLabel(string text)
        {
            LabelControl label = new()
            {
                AutoSizeMode = LabelAutoSizeMode.None,
                Dock = DockStyle.Fill,
                Text = text
            };
            label.Appearance.TextOptions.VAlignment = VertAlignment.Center;

            return label;
        }

        private static void SetControlMargin(Control control, Padding margin)
        {
            control.Dock = DockStyle.Fill;
            control.Margin = margin;
        }

        private static CommandLayoutMode GetCommandLayoutMode(int clientWidth, float scale)
        {
            float effectiveWidth = clientWidth / scale;
            return effectiveWidth < CompactWidthThreshold ? CommandLayoutMode.Compact : CommandLayoutMode.Normal;
        }

        private static int GetCommandRowHeight(float scale, CommandLayoutMode commandMode, int commandControlHeight)
        {
            int rowHeight = commandControlHeight + Scale(4, scale);
            return commandMode == CommandLayoutMode.Compact ? rowHeight * 2 : rowHeight;
        }

        private static int GetCommandControlHeight(Control referenceControl, float scale)
        {
            int textHeight = TextRenderer.MeasureText("Ag", referenceControl.Font).Height;
            return Math.Max(textHeight + Scale(8, scale), Scale(24, scale));
        }

        private static float GetScale(XtraForm form)
        {
            return form.DeviceDpi > 0 ? form.DeviceDpi / 96F : 1F;
        }

        private static int Scale(int value, float scale)
        {
            return (int)Math.Round(value * scale);
        }

        private static Size ScaleSize(int width, int height, float scale)
        {
            return new Size(Scale(width, scale), Scale(height, scale));
        }

        private static Padding ScalePadding(int left, int top, int right, int bottom, float scale)
        {
            return new Padding(Scale(left, scale), Scale(top, scale), Scale(right, scale), Scale(bottom, scale));
        }
    }
}

