using Accounting.Form;
using DevExpress.XtraEditors;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;

namespace Accounting.Tests;

[Collection(WinFormsUiCollection.Name)]
public sealed class FrmReportParamLayoutTests
{
    [Theory]
    [InlineData(1.00F)]
    [InlineData(1.25F)]
    [InlineData(1.50F)]
    public void MonthBasedReports_KeepYearAlignedAfterSelectionChanges(float scale)
    {
        RunSta(() =>
        {
            using FrmReportParam form = new();
            RadioGroup reportSelector = FindControl<RadioGroup>(form, "radioGroup1");
            ComboBoxEdit fromMonth = FindControl<ComboBoxEdit>(form, "cmbbulan");
            SpinEdit fromYear = FindControl<SpinEdit>(form, "daritahun");
            SpinEdit toYear = FindControl<SpinEdit>(form, "sampaitahun");
            LabelControl fromLabel = FindControl<LabelControl>(form, "labelControl3");

            fromMonth.Location = Scale(new Point(285, 174), scale);
            toYear.Location = Scale(new Point(402, 203), scale);
            Point expectedYearLocation = new(toYear.Left, fromMonth.Top);

            foreach (int reportIndex in new[] { 0, 1, 2, 4 })
            {
                reportSelector.SelectedIndex = 3;
                reportSelector.SelectedIndex = reportIndex;

                Assert.Equal(expectedYearLocation, fromYear.Location);
                Assert.Equal("Dari", fromLabel.Text);
            }
        });
    }

    [Theory]
    [InlineData(1.00F)]
    [InlineData(1.25F)]
    [InlineData(1.50F)]
    public void NeracaSaldo_UsesMonthSlotAndRestoresMonthYearLayout(float scale)
    {
        RunSta(() =>
        {
            using FrmReportParam form = new();
            RadioGroup reportSelector = FindControl<RadioGroup>(form, "radioGroup1");
            ComboBoxEdit fromMonth = FindControl<ComboBoxEdit>(form, "cmbbulan");
            SpinEdit fromYear = FindControl<SpinEdit>(form, "daritahun");
            SpinEdit toYear = FindControl<SpinEdit>(form, "sampaitahun");
            LabelControl fromLabel = FindControl<LabelControl>(form, "labelControl3");

            fromMonth.Location = Scale(new Point(285, 174), scale);
            toYear.Location = Scale(new Point(402, 203), scale);

            reportSelector.SelectedIndex = 3;

            Assert.Equal(fromMonth.Location, fromYear.Location);
            Assert.Equal("Tahun", fromLabel.Text);

            reportSelector.SelectedIndex = 1;

            Assert.Equal(new Point(toYear.Left, fromMonth.Top), fromYear.Location);
            Assert.Equal("Dari", fromLabel.Text);
        });
    }

    private static T FindControl<T>(Control root, string name)
        where T : Control
    {
        return root.Controls.Find(name, true).OfType<T>().Single();
    }

    private static Point Scale(Point point, float scale)
    {
        return new Point(
            (int)Math.Round(point.X * scale, MidpointRounding.AwayFromZero),
            (int)Math.Round(point.Y * scale, MidpointRounding.AwayFromZero));
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
        Assert.True(thread.Join(TimeSpan.FromSeconds(120)), "STA form smoke test timed out.");

        if (failure != null)
        {
            throw new Xunit.Sdk.XunitException(failure.ToString());
        }
    }
}
