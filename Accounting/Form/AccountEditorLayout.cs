using DevExpress.Utils;
using DevExpress.XtraEditors;
using DevExpress.XtraLayout;
using DevExpress.XtraLayout.Utils;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace Accounting.Form
{
    internal readonly record struct AccountEditorLayoutMetrics(
        Size InitialClientSize,
        Size MinimumClientSize,
        Size ScrollContentSize,
        bool RequiresScrolling);

    internal static class AccountEditorLayoutSizing
    {
        internal const int SafeMarginLogical = 12;

        internal static AccountEditorLayoutMetrics Calculate(
            Size preferredLogicalSize,
            Size minimumLogicalSize,
            float scale,
            Size workingAreaSize,
            Size nonClientSize)
        {
            if (scale <= 0F)
            {
                throw new ArgumentOutOfRangeException(nameof(scale));
            }

            Size preferredSize = Scale(preferredLogicalSize, scale);
            Size minimumSize = Scale(minimumLogicalSize, scale);
            int safeMargin = Scale(SafeMarginLogical, scale);
            Size availableClientSize = new(
                Math.Max(1, workingAreaSize.Width - nonClientSize.Width - (safeMargin * 2)),
                Math.Max(1, workingAreaSize.Height - nonClientSize.Height - (safeMargin * 2)));

            Size initialClientSize = Clamp(preferredSize, availableClientSize);
            Size minimumClientSize = Clamp(minimumSize, availableClientSize);

            return new AccountEditorLayoutMetrics(
                initialClientSize,
                minimumClientSize,
                preferredSize,
                preferredSize.Width > availableClientSize.Width ||
                preferredSize.Height > availableClientSize.Height);
        }

        internal static int Scale(int value, float scale)
        {
            return (int)Math.Round(value * scale, MidpointRounding.AwayFromZero);
        }

        private static Size Scale(Size value, float scale)
        {
            return new Size(Scale(value.Width, scale), Scale(value.Height, scale));
        }

        private static Size Clamp(Size value, Size maximum)
        {
            return new Size(
                Math.Min(value.Width, maximum.Width),
                Math.Min(value.Height, maximum.Height));
        }
    }

    internal static class AccountEditorLayout
    {
        private static readonly Size AddPreferredLogicalSize = new(640, 440);
        private static readonly Size AddMinimumLogicalSize = new(520, 360);
        private static readonly Size EditPreferredLogicalSize = new(640, 410);
        private static readonly Size EditMinimumLogicalSize = new(520, 330);

        internal static AccountEditorLayoutHandle ApplyAdd(
            XtraForm form,
            LayoutControl layout,
            LayoutControlGroup codeGroup,
            LayoutControlItem headCodeItem,
            LayoutControlItem groupCodeItem,
            LayoutControlItem detailCodeItem,
            LayoutControlItem levelItem,
            LayoutControlItem normalBalanceItem,
            RadioGroup accountKind,
            RadioGroup normalBalance,
            LabelControl parentName,
            LayoutControlItem saveItem,
            LayoutControlItem closeItem,
            SimpleButton saveButton,
            SimpleButton closeButton)
        {
            ConfigureAddCodeGroup(
                codeGroup,
                headCodeItem,
                groupCodeItem,
                detailCodeItem,
                levelItem,
                normalBalanceItem);

            return new AccountEditorLayoutHandle(
                form,
                layout,
                codeGroup,
                accountKind,
                normalBalance,
                parentName,
                new[] { saveItem, closeItem },
                new[] { saveButton, closeButton },
                AddPreferredLogicalSize,
                AddMinimumLogicalSize);
        }

        internal static AccountEditorLayoutHandle ApplyEdit(
            XtraForm form,
            LayoutControl layout,
            LayoutControlGroup codeGroup,
            LayoutControlItem detailCodeItem,
            LayoutControlItem levelItem,
            LayoutControlItem normalBalanceItem,
            RadioGroup accountKind,
            RadioGroup normalBalance,
            LayoutControlItem updateItem,
            LayoutControlItem closeItem,
            SimpleButton updateButton,
            SimpleButton closeButton)
        {
            ConfigureEditCodeGroup(codeGroup, detailCodeItem, levelItem, normalBalanceItem);

            return new AccountEditorLayoutHandle(
                form,
                layout,
                codeGroup,
                accountKind,
                normalBalance,
                null,
                new[] { updateItem, closeItem },
                new[] { updateButton, closeButton },
                EditPreferredLogicalSize,
                EditMinimumLogicalSize);
        }

        private static void ConfigureAddCodeGroup(
            LayoutControlGroup codeGroup,
            LayoutControlItem headCodeItem,
            LayoutControlItem groupCodeItem,
            LayoutControlItem detailCodeItem,
            LayoutControlItem levelItem,
            LayoutControlItem normalBalanceItem)
        {
            ConfigureTable(codeGroup, new[] { 15D, 20D, 15D, 50D });
            Place(headCodeItem, column: 0, row: 0);
            Place(groupCodeItem, column: 1, row: 0);
            Place(detailCodeItem, column: 2, row: 0);
            Place(levelItem, column: 0, row: 1, columnSpan: 2);
            Place(normalBalanceItem, column: 2, row: 1, columnSpan: 2);
        }

        private static void ConfigureEditCodeGroup(
            LayoutControlGroup codeGroup,
            LayoutControlItem detailCodeItem,
            LayoutControlItem levelItem,
            LayoutControlItem normalBalanceItem)
        {
            ConfigureTable(codeGroup, new[] { 35D, 65D });
            Place(detailCodeItem, column: 0, row: 0, columnSpan: 2);
            Place(levelItem, column: 0, row: 1);
            Place(normalBalanceItem, column: 1, row: 1);
        }

        private static void ConfigureTable(LayoutControlGroup group, double[] columnWidths)
        {
            group.LayoutMode = LayoutMode.Table;
            group.OptionsTableLayoutGroup.ColumnDefinitions.Clear();
            group.OptionsTableLayoutGroup.RowDefinitions.Clear();

            foreach (double width in columnWidths)
            {
                group.OptionsTableLayoutGroup.ColumnDefinitions.Add(
                    new ColumnDefinition(group, width, SizeType.Percent));
            }

            group.OptionsTableLayoutGroup.RowDefinitions.Add(
                new RowDefinition(group, 50D, SizeType.Percent));
            group.OptionsTableLayoutGroup.RowDefinitions.Add(
                new RowDefinition(group, 50D, SizeType.Percent));
        }

        private static void Place(
            LayoutControlItem item,
            int column,
            int row,
            int columnSpan = 1)
        {
            item.OptionsTableLayoutItem.ColumnIndex = column;
            item.OptionsTableLayoutItem.RowIndex = row;
            item.OptionsTableLayoutItem.ColumnSpan = columnSpan;
            item.OptionsTableLayoutItem.RowSpan = 1;
        }
    }

    internal sealed class AccountEditorLayoutHandle : IDisposable
    {
        private const int LogicalCodeGroupHeight = 92;
        private const int LogicalEditorHeight = 26;
        private const int LogicalToggleHeight = 28;
        private const int LogicalRadioHeight = 32;
        private const int LogicalButtonWidth = 104;
        private const int LogicalButtonHeight = 34;
        private const int LogicalRootPadding = 12;
        private const int LogicalCodeGroupPadding = 8;

        private readonly XtraForm form;
        private readonly LayoutControl layout;
        private readonly LayoutControlGroup codeGroup;
        private readonly RadioGroup accountKind;
        private readonly RadioGroup normalBalance;
        private readonly LabelControl? parentName;
        private readonly LayoutControlItem[] buttonItems;
        private readonly SimpleButton[] buttons;
        private readonly Size preferredLogicalSize;
        private readonly Size minimumLogicalSize;
        private readonly ToolTip? parentNameToolTip;
        private readonly EventHandler? parentNameTextChangedHandler;
        private Rectangle lastWorkingArea;
        private float lastScale;
        private bool disposed;

        internal AccountEditorLayoutHandle(
            XtraForm form,
            LayoutControl layout,
            LayoutControlGroup codeGroup,
            RadioGroup accountKind,
            RadioGroup normalBalance,
            LabelControl? parentName,
            LayoutControlItem[] buttonItems,
            SimpleButton[] buttons,
            Size preferredLogicalSize,
            Size minimumLogicalSize)
        {
            this.form = form;
            this.layout = layout;
            this.codeGroup = codeGroup;
            this.accountKind = accountKind;
            this.normalBalance = normalBalance;
            this.parentName = parentName;
            this.buttonItems = buttonItems;
            this.buttons = buttons;
            this.preferredLogicalSize = preferredLogicalSize;
            this.minimumLogicalSize = minimumLogicalSize;

            ConfigureForm();
            ConfigureControls();

            if (parentName != null)
            {
                parentNameToolTip = new ToolTip();
                parentNameTextChangedHandler = (_, _) =>
                    parentNameToolTip.SetToolTip(parentName, parentName.Text);
                ConfigureParentName(parentName, parentNameToolTip);
                parentName.TextChanged += parentNameTextChangedHandler;
            }

            form.DpiChanged += Form_DpiChanged;
            form.Shown += Form_Shown;
            form.LocationChanged += Form_LocationChanged;
            form.FormClosed += Form_FormClosed;
            form.Disposed += Form_Disposed;

            Relayout(usePreferredSize: true);
        }

        internal void Relayout(bool usePreferredSize = false)
        {
            if (disposed)
            {
                return;
            }

            float scale = GetScale(form);
            Rectangle workingArea = Screen.FromControl(form).WorkingArea;
            Size nonClientSize = new(
                Math.Max(0, form.Width - form.ClientSize.Width),
                Math.Max(0, form.Height - form.ClientSize.Height));
            AccountEditorLayoutMetrics metrics = AccountEditorLayoutSizing.Calculate(
                preferredLogicalSize,
                minimumLogicalSize,
                scale,
                workingArea.Size,
                nonClientSize);

            form.SuspendLayout();
            layout.SuspendLayout();

            layout.AutoScroll = true;
            ((ScrollableControl)layout).AutoScrollMinSize = metrics.ScrollContentSize;
            layout.Root.Padding = CreateLayoutPadding(LogicalRootPadding, scale);
            codeGroup.Padding = CreateLayoutPadding(LogicalCodeGroupPadding, scale);
            codeGroup.MinSize = new Size(0, Scale(LogicalCodeGroupHeight, scale));

            ConfigureEditorHeights(scale);
            ConfigureRadio(accountKind, scale);
            ConfigureRadio(normalBalance, scale);
            ConfigureButtons(scale);

            form.MinimumSize = new Size(
                metrics.MinimumClientSize.Width + nonClientSize.Width,
                metrics.MinimumClientSize.Height + nonClientSize.Height);
            if (usePreferredSize)
            {
                form.ClientSize = metrics.InitialClientSize;
            }
            else
            {
                Size constrainedClientSize = new(
                    Math.Min(form.ClientSize.Width, metrics.InitialClientSize.Width),
                    Math.Min(form.ClientSize.Height, metrics.InitialClientSize.Height));

                if (constrainedClientSize != form.ClientSize)
                {
                    form.ClientSize = constrainedClientSize;
                }
            }

            layout.ResumeLayout(true);
            form.ResumeLayout(true);

            lastScale = scale;
            lastWorkingArea = workingArea;
        }

        public void Dispose()
        {
            if (disposed)
            {
                return;
            }

            disposed = true;
            form.DpiChanged -= Form_DpiChanged;
            form.Shown -= Form_Shown;
            form.LocationChanged -= Form_LocationChanged;
            form.FormClosed -= Form_FormClosed;
            form.Disposed -= Form_Disposed;
            if (parentName != null && parentNameTextChangedHandler != null)
            {
                parentName.TextChanged -= parentNameTextChangedHandler;
            }
            parentNameToolTip?.Dispose();
        }

        private void ConfigureForm()
        {
            form.AutoScaleDimensions = new SizeF(96F, 96F);
            form.AutoScaleMode = AutoScaleMode.Dpi;
            form.FormBorderStyle = FormBorderStyle.Sizable;
            form.MaximizeBox = false;
            form.MinimizeBox = false;
        }

        private void ConfigureControls()
        {
            accountKind.Properties.Columns = 2;
            normalBalance.Properties.Columns = 2;

            foreach (LayoutControlItem buttonItem in buttonItems)
            {
                buttonItem.MaxSize = Size.Empty;
                buttonItem.SizeConstraintsType = SizeConstraintsType.Custom;
            }
        }

        private static void ConfigureParentName(LabelControl label, ToolTip toolTip)
        {
            label.AutoSizeMode = LabelAutoSizeMode.None;
            label.Appearance.TextOptions.Trimming = Trimming.EllipsisCharacter;
            label.Appearance.TextOptions.WordWrap = WordWrap.NoWrap;
            toolTip.SetToolTip(label, label.Text);
        }

        private void ConfigureRadio(RadioGroup radioGroup, float scale)
        {
            radioGroup.MinimumSize = new Size(0, Scale(LogicalRadioHeight, scale));
        }

        private void ConfigureEditorHeights(float scale)
        {
            foreach (Control control in layout.Controls)
            {
                if (control is not BaseEdit || control is RadioGroup)
                {
                    continue;
                }

                int logicalHeight = control is ToggleSwitch
                    ? LogicalToggleHeight
                    : LogicalEditorHeight;
                control.MinimumSize = new Size(0, Scale(logicalHeight, scale));
            }
        }

        private void ConfigureButtons(float scale)
        {
            Size buttonMinimumSize = new(
                Scale(LogicalButtonWidth, scale),
                Scale(LogicalButtonHeight, scale));

            for (int index = 0; index < buttonItems.Length; index++)
            {
                buttonItems[index].MinSize = buttonMinimumSize;
                buttonItems[index].MaxSize = Size.Empty;
                buttons[index].MinimumSize = buttonMinimumSize;
                buttons[index].MaximumSize = new Size(0, buttonMinimumSize.Height);
            }
        }

        private void Form_DpiChanged(object? sender, DpiChangedEventArgs e)
        {
            Relayout();
        }

        private void Form_Shown(object? sender, EventArgs e)
        {
            float scale = GetScale(form);
            Relayout(usePreferredSize: Math.Abs(scale - lastScale) > 0.01F);
        }

        private void Form_LocationChanged(object? sender, EventArgs e)
        {
            Rectangle workingArea = Screen.FromControl(form).WorkingArea;
            if (workingArea != lastWorkingArea)
            {
                Relayout();
            }
        }

        private void Form_FormClosed(object? sender, FormClosedEventArgs e)
        {
            Dispose();
        }

        private void Form_Disposed(object? sender, EventArgs e)
        {
            Dispose();
        }

        private static float GetScale(Control control)
        {
            return control.DeviceDpi > 0 ? control.DeviceDpi / 96F : 1F;
        }

        private static int Scale(int value, float scale)
        {
            return AccountEditorLayoutSizing.Scale(value, scale);
        }

        private static DevExpress.XtraLayout.Utils.Padding CreateLayoutPadding(
            int logicalPadding,
            float scale)
        {
            return new DevExpress.XtraLayout.Utils.Padding(Scale(logicalPadding, scale));
        }
    }
}
