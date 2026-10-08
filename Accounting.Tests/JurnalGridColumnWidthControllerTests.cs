using Accounting.Form;
using DevExpress.XtraGrid;
using DevExpress.XtraGrid.Columns;
using DevExpress.XtraGrid.Views.Base;
using DevExpress.XtraGrid.Views.Grid;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;

namespace Accounting.Tests;

public sealed class JurnalGridColumnWidthControllerTests
{
    [Fact]
    public void Apply_UnlocksOnlyVisibleDataColumnsAndCapturesResize()
    {
        RunSta(() =>
        {
            using var fixture = new GridFixture();
            GridColumn numberColumn = fixture.AddVisibleColumn("BARIS", 0, 30);
            GridColumn codeColumn = fixture.AddVisibleColumn("Kode", 1, 120);
            GridColumn hiddenColumn = fixture.AddHiddenColumn("HIDREFF", 17);
            using var controller = new JurnalGridColumnWidthController(string.Empty, () => 96);

            controller.Register(fixture.View, "Detail", "BARIS");
            controller.Apply(fixture.View);

            Assert.True(fixture.View.OptionsCustomization.AllowColumnResizing);
            Assert.False(fixture.View.OptionsView.ColumnAutoWidth);
            Assert.True(numberColumn.OptionsColumn.AllowSize);
            Assert.False(numberColumn.OptionsColumn.FixedWidth);
            Assert.Equal(0, numberColumn.MaxWidth);
            Assert.Equal(30, numberColumn.MinWidth);
            Assert.True(codeColumn.OptionsColumn.AllowSize);
            Assert.False(codeColumn.OptionsColumn.FixedWidth);
            Assert.Equal(0, codeColumn.MaxWidth);
            Assert.Equal(40, codeColumn.MinWidth);
            Assert.False(hiddenColumn.OptionsColumn.AllowSize);
            Assert.True(hiddenColumn.OptionsColumn.FixedWidth);
            Assert.Equal(17, hiddenColumn.MaxWidth);

            codeColumn.Width = 225;
            fixture.View.NotifyColumnWidthChanged(codeColumn);

            Dictionary<string, Dictionary<string, int>> storedWidths =
                JurnalGridColumnWidthController.Deserialize(controller.Serialize());
            Assert.Equal(225, storedWidths["Detail"]["Kode"]);
            Assert.DoesNotContain("HIDREFF", storedWidths["Detail"].Keys);
        });
    }

    [Fact]
    public void Apply_RestoresIndependentLayoutsAndScalesForDpi()
    {
        const string serializedWidths =
            """
            {
              "Header": { "NoJurnal": 210, "Tanggal": 100 },
              "Detail": { "Kode": 220, "Keterangan": 360 }
            }
            """;

        RunSta(() =>
        {
            using var headerFixture = new GridFixture();
            GridColumn numberColumn = headerFixture.AddVisibleColumn("NoJurnal", 0, 100);
            GridColumn dateColumn = headerFixture.AddVisibleColumn("Tanggal", 1, 100);
            using var detailFixture = new GridFixture();
            GridColumn codeColumn = detailFixture.AddVisibleColumn("Kode", 0, 120);
            GridColumn descriptionColumn = detailFixture.AddVisibleColumn("Keterangan", 1, 200);
            using var controller = new JurnalGridColumnWidthController(serializedWidths, () => 144);

            controller.Register(headerFixture.View, "Header");
            controller.Register(detailFixture.View, "Detail", "BARIS");
            controller.Apply(headerFixture.View);
            controller.Apply(detailFixture.View);

            Assert.Equal(315, numberColumn.Width);
            Assert.Equal(150, dateColumn.Width);
            Assert.Equal(330, codeColumn.Width);
            Assert.Equal(540, descriptionColumn.Width);
        });
    }

    [Fact]
    public void Apply_IgnoresUnknownColumnsAndClampsInvalidWidths()
    {
        const string serializedWidths =
            """
            {
              "Detail": {
                "Kode": 1,
                "Keterangan": 2147483647,
                "RemovedColumn": 500
              }
            }
            """;

        RunSta(() =>
        {
            using var fixture = new GridFixture();
            GridColumn codeColumn = fixture.AddVisibleColumn("Kode", 0, 120);
            GridColumn descriptionColumn = fixture.AddVisibleColumn("Keterangan", 1, 200);
            using var controller = new JurnalGridColumnWidthController(serializedWidths, () => 96);

            controller.Register(fixture.View, "Detail", "BARIS");
            controller.Apply(fixture.View);

            Assert.Equal(40, codeColumn.Width);
            Assert.Equal(JurnalGridColumnWidthController.MaximumLogicalWidth, descriptionColumn.Width);
            Assert.Null(fixture.View.Columns["RemovedColumn"]);
        });
    }

    [Fact]
    public void Apply_FillColumnAlwaysUsesRemainingViewportWidth()
    {
        const string serializedWidths =
            """
            {
              "Detail": { "Kode": 120, "Keterangan": 100 }
            }
            """;

        RunSta(() =>
        {
            using var fixture = new GridFixture();
            GridColumn numberColumn = fixture.AddVisibleColumn("BARIS", 0, 30);
            GridColumn codeColumn = fixture.AddVisibleColumn("Kode", 1, 120);
            GridColumn descriptionColumn = fixture.AddVisibleColumn("Keterangan", 2, 200);
            using var controller = new JurnalGridColumnWidthController(serializedWidths, () => 96);

            controller.RegisterWithFill(
                fixture.View,
                "Detail",
                "Keterangan",
                "BARIS");
            controller.Apply(fixture.View);

            Assert.False(descriptionColumn.OptionsColumn.AllowSize);
            Assert.True(descriptionColumn.OptionsColumn.FixedWidth);
            Assert.Equal(
                CalculateFillWidth(fixture, descriptionColumn),
                descriptionColumn.Width);

            codeColumn.Width += 60;
            fixture.View.NotifyColumnWidthChanged(codeColumn);

            Assert.Equal(
                CalculateFillWidth(fixture, descriptionColumn),
                descriptionColumn.Width);

            fixture.Resize(1_000);

            Assert.Equal(
                CalculateFillWidth(fixture, descriptionColumn),
                descriptionColumn.Width);

            Dictionary<string, Dictionary<string, int>> storedWidths =
                JurnalGridColumnWidthController.Deserialize(controller.Serialize());
            Assert.Equal(codeColumn.Width, storedWidths["Detail"]["Kode"]);
            Assert.DoesNotContain("Keterangan", storedWidths["Detail"].Keys);
            Assert.Equal(30, numberColumn.MinWidth);
        });
    }

    [Fact]
    public void Deserialize_ReturnsEmptyStateForInvalidJson()
    {
        Dictionary<string, Dictionary<string, int>> storedWidths =
            JurnalGridColumnWidthController.Deserialize("{not-valid-json");

        Assert.Empty(storedWidths);
    }

    [Theory]
    [InlineData(100, 96, 100)]
    [InlineData(100, 120, 125)]
    [InlineData(100, 144, 150)]
    [InlineData(100, 192, 200)]
    [InlineData(100, 0, 100)]
    public void DpiConversion_RoundTripsLogicalWidths(int logicalWidth, int deviceDpi, int deviceWidth)
    {
        Assert.Equal(
            deviceWidth,
            JurnalGridColumnWidthController.ScaleToDevicePixels(logicalWidth, deviceDpi));
        Assert.Equal(
            logicalWidth,
            JurnalGridColumnWidthController.ScaleToLogicalPixels(deviceWidth, deviceDpi));
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
        Assert.True(thread.Join(TimeSpan.FromSeconds(30)), "STA grid test timed out.");

        if (failure != null)
        {
            throw new Xunit.Sdk.XunitException(failure.ToString());
        }
    }

    private static int CalculateFillWidth(GridFixture fixture, GridColumn fillColumn)
    {
        int viewportWidth = fixture.View.ViewRect.Width;
        if (viewportWidth <= 0)
        {
            viewportWidth = fixture.Control.ClientSize.Width;
        }

        int occupiedWidth = fixture.View.VisibleColumns
            .Where(column => column != fillColumn)
            .Sum(column => column.Width);
        return Math.Max(
            JurnalGridColumnWidthController.DefaultMinimumLogicalWidth,
            viewportWidth
                - occupiedWidth
                - JurnalGridColumnWidthController.ViewportPaddingLogicalWidth);
    }

    private sealed class GridFixture : IDisposable
    {
        private readonly System.Windows.Forms.Form hostForm;

        internal GridFixture()
        {
            hostForm = new System.Windows.Forms.Form
            {
                ClientSize = new Size(800, 400)
            };
            Control = new GridControl
            {
                Dock = DockStyle.Fill
            };
            View = new TestGridView
            {
                GridControl = Control
            };
            Control.MainView = View;
            Control.ViewCollection.Add(View);
            hostForm.Controls.Add(Control);
            hostForm.CreateControl();
            Control.ForceInitialize();
        }

        internal GridControl Control { get; }

        internal TestGridView View { get; }

        internal GridColumn AddVisibleColumn(string fieldName, int visibleIndex, int width)
        {
            var column = CreateLockedColumn(fieldName, width);
            View.Columns.Add(column);
            column.Visible = true;
            column.VisibleIndex = visibleIndex;
            return column;
        }

        internal GridColumn AddHiddenColumn(string fieldName, int width)
        {
            var column = CreateLockedColumn(fieldName, width);
            View.Columns.Add(column);
            return column;
        }

        internal void Resize(int width)
        {
            hostForm.ClientSize = new Size(width, hostForm.ClientSize.Height);
            hostForm.PerformLayout();
            Control.PerformLayout();
        }

        public void Dispose()
        {
            hostForm.Dispose();
        }

        private static GridColumn CreateLockedColumn(string fieldName, int width)
        {
            return new GridColumn
            {
                Caption = fieldName,
                FieldName = fieldName,
                Name = $"column{fieldName}",
                MinWidth = width,
                MaxWidth = width,
                Width = width,
                OptionsColumn =
                {
                    AllowSize = false,
                    FixedWidth = true
                }
            };
        }
    }

    private sealed class TestGridView : GridView
    {
        internal void NotifyColumnWidthChanged(GridColumn column)
        {
            RaiseColumnWidthChanged(new ColumnEventArgs(column));
        }
    }
}
