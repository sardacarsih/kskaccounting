using Accounting.Form;
using DevExpress.Utils;
using DevExpress.XtraEditors;
using DevExpress.XtraLayout;
using DevExpress.XtraLayout.Utils;
using System.Drawing;
using System.Reflection;
using System.Threading;
using System.Windows.Forms;

namespace Accounting.Tests;

public sealed class AccountEditorLayoutTests
{
    [Theory]
    [InlineData(1.00F, 1366, 768, false)]
    [InlineData(1.25F, 1366, 768, false)]
    [InlineData(1.50F, 1366, 768, false)]
    [InlineData(1.75F, 1366, 768, true)]
    [InlineData(2.00F, 1366, 768, true)]
    [InlineData(1.00F, 1920, 1080, false)]
    [InlineData(1.25F, 1920, 1080, false)]
    [InlineData(1.50F, 1920, 1080, false)]
    [InlineData(1.75F, 1920, 1080, false)]
    [InlineData(2.00F, 1920, 1080, false)]
    public void AddSizing_ClampsToWorkingAreaAndReportsScrollFallback(
        float scale,
        int workingWidth,
        int workingHeight,
        bool requiresScrolling)
    {
        Size nonClientSize = Scale(new Size(16, 39), scale);
        AccountEditorLayoutMetrics metrics = AccountEditorLayoutSizing.Calculate(
            new Size(640, 440),
            new Size(520, 360),
            scale,
            new Size(workingWidth, workingHeight),
            nonClientSize);

        AssertMetrics(
            metrics,
            scale,
            new Size(640, 440),
            new Size(520, 360),
            new Size(workingWidth, workingHeight),
            nonClientSize,
            requiresScrolling);
    }

    [Theory]
    [InlineData(1.00F, 1366, 768, false)]
    [InlineData(1.25F, 1366, 768, false)]
    [InlineData(1.50F, 1366, 768, false)]
    [InlineData(1.75F, 1366, 768, true)]
    [InlineData(2.00F, 1366, 768, true)]
    [InlineData(1.00F, 1920, 1080, false)]
    [InlineData(1.25F, 1920, 1080, false)]
    [InlineData(1.50F, 1920, 1080, false)]
    [InlineData(1.75F, 1920, 1080, false)]
    [InlineData(2.00F, 1920, 1080, false)]
    public void EditSizing_ClampsToWorkingAreaAndReportsScrollFallback(
        float scale,
        int workingWidth,
        int workingHeight,
        bool requiresScrolling)
    {
        Size nonClientSize = Scale(new Size(16, 39), scale);
        AccountEditorLayoutMetrics metrics = AccountEditorLayoutSizing.Calculate(
            new Size(640, 410),
            new Size(520, 330),
            scale,
            new Size(workingWidth, workingHeight),
            nonClientSize);

        AssertMetrics(
            metrics,
            scale,
            new Size(640, 410),
            new Size(520, 330),
            new Size(workingWidth, workingHeight),
            nonClientSize,
            requiresScrolling);
    }

    [Fact]
    public void Sizing_RejectsNonPositiveScale()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => AccountEditorLayoutSizing.Calculate(
            new Size(640, 440),
            new Size(520, 360),
            0F,
            new Size(1366, 768),
            new Size(16, 39)));
    }

    [Fact]
    public void AddAndEditForms_UseDpiAwareTwoRowLayouts()
    {
        RunSta(() =>
        {
            using FrmAkunAdd addForm = new(null!);
            using FrmAkunEdit editForm = new(null!);

            AssertResponsiveForm(addForm, expectedRowCount: 2);
            AssertResponsiveForm(editForm, expectedRowCount: 2);

            LayoutControlGroup addCodeGroup = FindCodeGroup(addForm);
            AssertCell(addCodeGroup, "liKepala", column: 0, row: 0, columnSpan: 1);
            AssertCell(addCodeGroup, "liGroupKode", column: 1, row: 0, columnSpan: 1);
            AssertCell(addCodeGroup, "liDetailKode", column: 2, row: 0, columnSpan: 1);
            AssertCell(addCodeGroup, "liLevel", column: 0, row: 1, columnSpan: 2);
            AssertCell(addCodeGroup, "liSisi", column: 2, row: 1, columnSpan: 2);

            LayoutControlGroup editCodeGroup = FindCodeGroup(editForm);
            AssertCell(editCodeGroup, "liDetailKode", column: 0, row: 0, columnSpan: 2);
            AssertCell(editCodeGroup, "liLevel", column: 0, row: 1, columnSpan: 1);
            AssertCell(editCodeGroup, "liSisi", column: 1, row: 1, columnSpan: 1);

            Assert.Equal(2, FindControl<RadioGroup>(addForm, "gd").Properties.Columns);
            Assert.Equal(2, FindControl<RadioGroup>(addForm, "rgsisi").Properties.Columns);
            Assert.Equal(2, FindControl<RadioGroup>(editForm, "gd").Properties.Columns);
            Assert.Equal(2, FindControl<RadioGroup>(editForm, "rgsisi").Properties.Columns);

            LabelControl parentName = FindControl<LabelControl>(addForm, "lblinduk");
            parentName.Text = "Nama induk yang sangat panjang dan harus dipotong secara aman";
            Assert.Equal(LabelAutoSizeMode.None, parentName.AutoSizeMode);
            Assert.Equal(Trimming.EllipsisCharacter, parentName.Appearance.TextOptions.Trimming);
            Assert.Equal(WordWrap.NoWrap, parentName.Appearance.TextOptions.WordWrap);
        });
    }

    [Fact]
    public void Relayout_PreservesEnteredValuesSelectionsAndTabOrder()
    {
        RunSta(() =>
        {
            using FrmAkunAdd addForm = new(null!);
            using FrmAkunEdit editForm = new(null!);

            AssertRelayoutPreservesState(addForm, "Akun add yang belum disimpan");
            AssertRelayoutPreservesState(editForm, "Akun edit yang belum disimpan");
        });
    }

    private static void AssertMetrics(
        AccountEditorLayoutMetrics metrics,
        float scale,
        Size preferredLogicalSize,
        Size minimumLogicalSize,
        Size workingAreaSize,
        Size nonClientSize,
        bool requiresScrolling)
    {
        int margin = AccountEditorLayoutSizing.Scale(
            AccountEditorLayoutSizing.SafeMarginLogical,
            scale);
        Size availableClientSize = new(
            workingAreaSize.Width - nonClientSize.Width - (margin * 2),
            workingAreaSize.Height - nonClientSize.Height - (margin * 2));
        Size scaledPreferredSize = Scale(preferredLogicalSize, scale);
        Size scaledMinimumSize = Scale(minimumLogicalSize, scale);

        Assert.Equal(scaledPreferredSize, metrics.ScrollContentSize);
        Assert.Equal(
            new Size(
                Math.Min(scaledPreferredSize.Width, availableClientSize.Width),
                Math.Min(scaledPreferredSize.Height, availableClientSize.Height)),
            metrics.InitialClientSize);
        Assert.Equal(
            new Size(
                Math.Min(scaledMinimumSize.Width, availableClientSize.Width),
                Math.Min(scaledMinimumSize.Height, availableClientSize.Height)),
            metrics.MinimumClientSize);
        Assert.True(metrics.MinimumClientSize.Width <= metrics.InitialClientSize.Width);
        Assert.True(metrics.MinimumClientSize.Height <= metrics.InitialClientSize.Height);
        Assert.Equal(requiresScrolling, metrics.RequiresScrolling);
    }

    private static void AssertResponsiveForm(System.Windows.Forms.Form form, int expectedRowCount)
    {
        Assert.Equal(AutoScaleMode.Dpi, form.AutoScaleMode);
        Assert.Equal(FormBorderStyle.Sizable, form.FormBorderStyle);
        Assert.False(form.MaximizeBox);
        Assert.False(form.MinimizeBox);
        Assert.True(form.MinimumSize.Width > 0);
        Assert.True(form.MinimumSize.Height > 0);

        LayoutControl layout = form.Controls.OfType<LayoutControl>().Single();
        Assert.True(layout.AutoScroll);
        Assert.True(((ScrollableControl)layout).AutoScrollMinSize.Width > 0);
        Assert.True(((ScrollableControl)layout).AutoScrollMinSize.Height > 0);

        LayoutControlGroup codeGroup = FindCodeGroup(form);
        Assert.Equal(LayoutMode.Table, codeGroup.LayoutMode);
        Assert.Equal(expectedRowCount, codeGroup.OptionsTableLayoutGroup.RowCount);

        AssertControlsDoNotOverlap(layout);
        form.Size = form.MinimumSize;
        layout.PerformLayout();
        AssertControlsDoNotOverlap(layout);

        foreach (string itemName in new[] { "liTutup", form is FrmAkunAdd ? "liSimpan" : "liUpdate" })
        {
            LayoutControlItem buttonItem = layout.Root.Items
                .OfType<LayoutControlItem>()
                .Single(item => item.Name == itemName);
            Assert.Equal(Size.Empty, buttonItem.MaxSize);
            Assert.True(buttonItem.MinSize.Width > 0);
            Assert.True(buttonItem.MinSize.Height > 0);

            SimpleButton button = Assert.IsType<SimpleButton>(buttonItem.Control);
            Assert.Equal(0, button.MaximumSize.Width);
            Assert.Equal(button.MinimumSize.Height, button.MaximumSize.Height);
        }
    }

    private static void AssertControlsDoNotOverlap(LayoutControl layout)
    {
        layout.PerformLayout();
        Control[] visibleControls = layout.Controls
            .Cast<Control>()
            .Where(control => control.Visible && control.Width > 0 && control.Height > 0)
            .ToArray();

        for (int firstIndex = 0; firstIndex < visibleControls.Length; firstIndex++)
        {
            for (int secondIndex = firstIndex + 1; secondIndex < visibleControls.Length; secondIndex++)
            {
                Control first = visibleControls[firstIndex];
                Control second = visibleControls[secondIndex];
                Assert.False(
                    first.Bounds.IntersectsWith(second.Bounds),
                    $"Controls '{first.Name}' and '{second.Name}' overlap: " +
                    $"{first.Bounds} / {second.Bounds}.");
            }
        }
    }

    private static void AssertRelayoutPreservesState(
        System.Windows.Forms.Form form,
        string accountName)
    {
        TextEdit nameEditor = FindControl<TextEdit>(form, "txtnamaakun");
        RadioGroup accountKind = FindControl<RadioGroup>(form, "gd");
        RadioGroup normalBalance = FindControl<RadioGroup>(form, "rgsisi");
        ToggleSwitch activeState = FindControl<ToggleSwitch>(form, "checkEditnonaktif");
        List<(Control Control, int TabIndex)> tabOrder = form.Controls[0].Controls
            .Cast<Control>()
            .Select(control => (control, control.TabIndex))
            .ToList();

        nameEditor.Text = accountName;
        accountKind.SelectedIndex = 1;
        normalBalance.SelectedIndex = 1;
        activeState.IsOn = true;

        FieldInfo layoutField = form.GetType().GetField(
            "accountEditorLayout",
            BindingFlags.Instance | BindingFlags.NonPublic)!;
        AccountEditorLayoutHandle handle = Assert.IsType<AccountEditorLayoutHandle>(
            layoutField.GetValue(form));

        handle.Relayout();

        Assert.Equal(accountName, nameEditor.Text);
        Assert.Equal(1, accountKind.SelectedIndex);
        Assert.Equal(1, normalBalance.SelectedIndex);
        Assert.True(activeState.IsOn);
        Assert.All(tabOrder, entry =>
            Assert.Equal(entry.TabIndex, entry.Control.TabIndex));
    }

    private static LayoutControlGroup FindCodeGroup(System.Windows.Forms.Form form)
    {
        LayoutControl layout = form.Controls.OfType<LayoutControl>().Single();
        return layout.Root.Items
            .OfType<LayoutControlGroup>()
            .Single(group => group.Name == "grpKodeAkun");
    }

    private static void AssertCell(
        LayoutControlGroup group,
        string itemName,
        int column,
        int row,
        int columnSpan)
    {
        LayoutControlItem item = group.Items
            .OfType<LayoutControlItem>()
            .Single(candidate => candidate.Name == itemName);

        Assert.Equal(column, item.OptionsTableLayoutItem.ColumnIndex);
        Assert.Equal(row, item.OptionsTableLayoutItem.RowIndex);
        Assert.Equal(columnSpan, item.OptionsTableLayoutItem.ColumnSpan);
    }

    private static T FindControl<T>(Control root, string name)
        where T : Control
    {
        return root.Controls.Find(name, true).OfType<T>().Single();
    }

    private static Size Scale(Size value, float scale)
    {
        return new Size(
            AccountEditorLayoutSizing.Scale(value.Width, scale),
            AccountEditorLayoutSizing.Scale(value.Height, scale));
    }

    private static void RunSta(Action action)
    {
        Exception? failure = null;
        Thread thread = new(() =>
        {
            try
            {
                action();
            }
            catch (Exception exception)
            {
                failure = exception;
            }
        });
        thread.SetApartmentState(ApartmentState.STA);
        thread.Start();
        Assert.True(thread.Join(TimeSpan.FromSeconds(30)), "STA form smoke test timed out.");

        if (failure != null)
        {
            throw new Xunit.Sdk.XunitException(failure.ToString());
        }
    }
}
