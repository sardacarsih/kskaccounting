using DevExpress.Utils;
using DevExpress.XtraEditors;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace Accounting.Form
{
    internal static class ClosingFormLayout
    {
        private const int LogicalWidth = 560;
        private const int LogicalHeight = 340;
        private const int LabelColumnWidth = 138;

        public static void ApplyYear(
            XtraForm form,
            LabelControl description,
            LabelControl companyCaption,
            LabelControl companyValue,
            LabelControl dataCaption,
            LabelControl dataValue,
            LabelControl regionCaption,
            LabelControl regionValue,
            LabelControl periodCaption,
            ComboBoxEdit month,
            SpinEdit year,
            CheckEdit createClosingJournal,
            SimpleButton processButton)
        {
            processButton.ImageOptions.Image = Properties.Resources.addcalculatedfield_16x161;
            Apply(
                form,
                description,
                companyCaption,
                companyValue,
                dataCaption,
                dataValue,
                regionCaption,
                regionValue,
                periodCaption,
                month,
                year,
                createClosingJournal,
                processButton);
        }

        public static void ApplyMonth(
            XtraForm form,
            LabelControl description,
            LabelControl companyCaption,
            LabelControl companyValue,
            LabelControl dataCaption,
            LabelControl dataValue,
            LabelControl regionCaption,
            LabelControl regionValue,
            LabelControl periodCaption,
            ComboBoxEdit month,
            SpinEdit year,
            CheckEdit createClosingJournal,
            SimpleButton processButton)
        {
            Apply(
                form,
                description,
                companyCaption,
                companyValue,
                dataCaption,
                dataValue,
                regionCaption,
                regionValue,
                periodCaption,
                month,
                year,
                createClosingJournal,
                processButton);
        }

        private static void Apply(
            XtraForm form,
            LabelControl description,
            LabelControl companyCaption,
            LabelControl companyValue,
            LabelControl dataCaption,
            LabelControl dataValue,
            LabelControl regionCaption,
            LabelControl regionValue,
            LabelControl periodCaption,
            ComboBoxEdit month,
            SpinEdit year,
            CheckEdit createClosingJournal,
            SimpleButton processButton)
        {
            float scale = GetScale(form);

            form.SuspendLayout();
            form.AutoScaleDimensions = new SizeF(96F, 96F);
            form.AutoScaleMode = AutoScaleMode.Dpi;
            form.ClientSize = ScaleSize(LogicalWidth, LogicalHeight, scale);
            form.MinimumSize = ScaleSize(LogicalWidth, LogicalHeight, scale);
            form.Font = new Font("Segoe UI", 10F, FontStyle.Regular, GraphicsUnit.Point);
            form.Controls.Clear();

            ConfigureDescription(description);
            ConfigureCaption(companyCaption);
            ConfigureCaption(dataCaption);
            ConfigureCaption(regionCaption);
            ConfigureCaption(periodCaption);
            ConfigureValue(companyValue);
            ConfigureValue(dataValue);
            ConfigureValue(regionValue);
            ConfigureMonth(month, scale);
            ConfigureYear(year, scale);
            ConfigureCheckEdit(createClosingJournal);
            ConfigureProcessButton(processButton, scale);

            TableLayoutPanel rootLayout = CreateRootLayout(scale);
            TableLayoutPanel detailLayout = CreateDetailLayout(scale);
            TableLayoutPanel periodLayout = CreatePeriodLayout(scale);
            FlowLayoutPanel actionLayout = CreateActionLayout();

            AddDetailRow(detailLayout, 0, companyCaption, companyValue, scale);
            AddDetailRow(detailLayout, 1, dataCaption, dataValue, scale);
            AddDetailRow(detailLayout, 2, regionCaption, regionValue, scale);
            AddPeriodControls(periodLayout, periodCaption, month, year, scale);
            actionLayout.Controls.Add(processButton);

            rootLayout.Controls.Add(description, 0, 0);
            rootLayout.Controls.Add(detailLayout, 0, 1);
            rootLayout.Controls.Add(periodLayout, 0, 2);
            rootLayout.Controls.Add(createClosingJournal, 0, 3);
            rootLayout.Controls.Add(actionLayout, 0, 4);
            form.Controls.Add(rootLayout);
            form.ResumeLayout(false);
        }

        private static TableLayoutPanel CreateRootLayout(float scale)
        {
            TableLayoutPanel rootLayout = new()
            {
                ColumnCount = 1,
                Dock = DockStyle.Fill,
                Padding = ScalePadding(18, 14, 18, 16, scale),
                RowCount = 5
            };
            rootLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            rootLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, Scale(54, scale)));
            rootLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, Scale(130, scale)));
            rootLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, Scale(44, scale)));
            rootLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, Scale(34, scale)));
            rootLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));

            return rootLayout;
        }

        private static TableLayoutPanel CreateDetailLayout(float scale)
        {
            TableLayoutPanel detailLayout = new()
            {
                ColumnCount = 2,
                Dock = DockStyle.Fill,
                RowCount = 3
            };
            detailLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, Scale(LabelColumnWidth, scale)));
            detailLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            detailLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 33.33F));
            detailLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 33.33F));
            detailLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 33.34F));

            return detailLayout;
        }

        private static TableLayoutPanel CreatePeriodLayout(float scale)
        {
            TableLayoutPanel periodLayout = new()
            {
                ColumnCount = 4,
                Dock = DockStyle.Fill,
                RowCount = 1
            };
            periodLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, Scale(LabelColumnWidth, scale)));
            periodLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, Scale(158, scale)));
            periodLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, Scale(96, scale)));
            periodLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            periodLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));

            return periodLayout;
        }

        private static FlowLayoutPanel CreateActionLayout()
        {
            return new FlowLayoutPanel()
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = false
            };
        }

        private static void AddDetailRow(
            TableLayoutPanel detailLayout,
            int rowIndex,
            LabelControl caption,
            LabelControl value,
            float scale)
        {
            SetControlMargin(caption, ScalePadding(0, 4, 14, 4, scale));
            SetControlMargin(value, ScalePadding(0, 2, 0, 2, scale));
            detailLayout.Controls.Add(caption, 0, rowIndex);
            detailLayout.Controls.Add(value, 1, rowIndex);
        }

        private static void AddPeriodControls(
            TableLayoutPanel periodLayout,
            LabelControl periodCaption,
            ComboBoxEdit month,
            SpinEdit year,
            float scale)
        {
            SetControlMargin(periodCaption, ScalePadding(0, 6, 14, 6, scale));
            SetControlMargin(month, ScalePadding(0, 6, 8, 6, scale));
            SetControlMargin(year, ScalePadding(0, 6, 0, 6, scale));
            periodLayout.Controls.Add(periodCaption, 0, 0);
            periodLayout.Controls.Add(month, 1, 0);
            periodLayout.Controls.Add(year, 2, 0);
        }

        private static void ConfigureDescription(LabelControl label)
        {
            label.AutoSizeMode = LabelAutoSizeMode.None;
            label.Dock = DockStyle.Fill;
            label.Appearance.TextOptions.VAlignment = VertAlignment.Top;
            label.Appearance.TextOptions.WordWrap = WordWrap.Wrap;
        }

        private static void ConfigureCaption(LabelControl label)
        {
            label.AutoSizeMode = LabelAutoSizeMode.None;
            label.Dock = DockStyle.Fill;
            label.Appearance.Font = new Font("Segoe UI", 10F, FontStyle.Regular, GraphicsUnit.Point);
            label.Appearance.Options.UseFont = true;
            label.Appearance.TextOptions.VAlignment = VertAlignment.Center;
        }

        private static void ConfigureValue(LabelControl label)
        {
            label.AutoSizeMode = LabelAutoSizeMode.None;
            label.Dock = DockStyle.Fill;
            label.Appearance.Font = new Font("Segoe UI", 12F, FontStyle.Bold, GraphicsUnit.Point);
            label.Appearance.Options.UseFont = true;
            label.Appearance.TextOptions.Trimming = Trimming.EllipsisCharacter;
            label.Appearance.TextOptions.VAlignment = VertAlignment.Center;
            label.Appearance.TextOptions.WordWrap = WordWrap.NoWrap;

            ToolTip toolTip = new();
            toolTip.SetToolTip(label, label.Text);
            label.TextChanged += (_, _) => toolTip.SetToolTip(label, label.Text);
        }

        private static void ConfigureMonth(ComboBoxEdit month, float scale)
        {
            month.Dock = DockStyle.Fill;
            month.MinimumSize = new Size(Scale(140, scale), Scale(24, scale));
        }

        private static void ConfigureYear(SpinEdit year, float scale)
        {
            year.Dock = DockStyle.Fill;
            year.MinimumSize = new Size(Scale(86, scale), Scale(24, scale));
        }

        private static void ConfigureCheckEdit(CheckEdit checkEdit)
        {
            checkEdit.AutoSizeInLayoutControl = true;
            checkEdit.Dock = DockStyle.Fill;
        }

        private static void ConfigureProcessButton(SimpleButton button, float scale)
        {
            button.MinimumSize = new Size(Scale(104, scale), Scale(30, scale));
            button.Size = new Size(Scale(112, scale), Scale(32, scale));
            button.Margin = ScalePadding(LabelColumnWidth, 4, 0, 0, scale);
        }

        private static void SetControlMargin(Control control, Padding margin)
        {
            control.Dock = DockStyle.Fill;
            control.Margin = margin;
        }

        private static float GetScale(Control control)
        {
            return control.DeviceDpi > 0 ? control.DeviceDpi / 96F : 1F;
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
