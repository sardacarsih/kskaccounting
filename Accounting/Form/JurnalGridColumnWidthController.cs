using DevExpress.XtraGrid.Columns;
using DevExpress.XtraGrid.Views.Base;
using DevExpress.XtraGrid.Views.Grid;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;

namespace Accounting.Form;

internal sealed class JurnalGridColumnWidthController : IDisposable
{
    internal const int CompactMinimumLogicalWidth = 30;
    internal const int DefaultMinimumLogicalWidth = 40;
    internal const int MaximumLogicalWidth = 10_000;
    internal const int ViewportPaddingLogicalWidth = 8;

    private readonly Func<int> deviceDpiProvider;
    private readonly Dictionary<GridView, GridRegistration> registrations = [];
    private readonly Dictionary<string, Dictionary<string, int>> storedWidths;
    private bool isApplyingLayout;
    private bool isDisposed;

    internal JurnalGridColumnWidthController(string? serializedWidths, Func<int> deviceDpiProvider)
    {
        ArgumentNullException.ThrowIfNull(deviceDpiProvider);

        this.deviceDpiProvider = deviceDpiProvider;
        storedWidths = Deserialize(serializedWidths);
    }

    internal void Register(GridView view, string layoutKey, params string[] compactColumnKeys)
    {
        RegisterCore(view, layoutKey, null, compactColumnKeys);
    }

    internal void RegisterWithFill(
        GridView view,
        string layoutKey,
        string fillRemainingColumnKey,
        params string[] compactColumnKeys)
    {
        if (string.IsNullOrWhiteSpace(fillRemainingColumnKey))
        {
            throw new ArgumentException("Fill column key cannot be empty.", nameof(fillRemainingColumnKey));
        }

        RegisterCore(view, layoutKey, fillRemainingColumnKey, compactColumnKeys);
    }

    private void RegisterCore(
        GridView view,
        string layoutKey,
        string? fillRemainingColumnKey,
        string[] compactColumnKeys)
    {
        ObjectDisposedException.ThrowIf(isDisposed, this);
        ArgumentNullException.ThrowIfNull(view);

        if (string.IsNullOrWhiteSpace(layoutKey))
        {
            throw new ArgumentException("Layout key cannot be empty.", nameof(layoutKey));
        }

        if (registrations.ContainsKey(view))
        {
            throw new InvalidOperationException($"Grid view '{view.Name}' is already registered.");
        }

        var compactColumns = new HashSet<string>(compactColumnKeys, StringComparer.OrdinalIgnoreCase);
        var registration = new GridRegistration(layoutKey, compactColumns, fillRemainingColumnKey);
        registrations.Add(view, registration);
        view.ColumnWidthChanged += ViewColumnWidthChanged;

        if (fillRemainingColumnKey != null)
        {
            view.GridControl.Resize += GridControlResize;

            if (storedWidths.TryGetValue(layoutKey, out Dictionary<string, int>? columnWidths))
            {
                columnWidths.Remove(fillRemainingColumnKey);
            }
        }
    }

    internal void Apply(GridView view)
    {
        ObjectDisposedException.ThrowIf(isDisposed, this);

        if (!registrations.TryGetValue(view, out GridRegistration? registration))
        {
            throw new InvalidOperationException($"Grid view '{view.Name}' is not registered.");
        }

        isApplyingLayout = true;
        view.BeginUpdate();
        try
        {
            view.OptionsCustomization.AllowColumnResizing = true;
            view.OptionsView.ColumnAutoWidth = false;

            foreach (GridColumn column in view.VisibleColumns)
            {
                ConfigureColumn(column, registration);
            }

            view.BestFitColumns();
            RestoreStoredWidths(view, registration);
            FillRemainingWidth(view, registration);
        }
        finally
        {
            view.EndUpdate();
            isApplyingLayout = false;
        }
    }

    internal string Serialize()
    {
        ObjectDisposedException.ThrowIf(isDisposed, this);
        return JsonSerializer.Serialize(storedWidths);
    }

    internal static Dictionary<string, Dictionary<string, int>> Deserialize(string? serializedWidths)
    {
        var normalizedWidths = new Dictionary<string, Dictionary<string, int>>(StringComparer.OrdinalIgnoreCase);
        if (string.IsNullOrWhiteSpace(serializedWidths))
        {
            return normalizedWidths;
        }

        try
        {
            Dictionary<string, Dictionary<string, int>>? parsedWidths =
                JsonSerializer.Deserialize<Dictionary<string, Dictionary<string, int>>>(serializedWidths);

            if (parsedWidths == null)
            {
                return normalizedWidths;
            }

            foreach ((string layoutKey, Dictionary<string, int> columnWidths) in parsedWidths)
            {
                if (string.IsNullOrWhiteSpace(layoutKey) || columnWidths == null)
                {
                    continue;
                }

                normalizedWidths[layoutKey] = new Dictionary<string, int>(
                    columnWidths,
                    StringComparer.OrdinalIgnoreCase);
            }
        }
        catch (JsonException)
        {
            return normalizedWidths;
        }
        catch (NotSupportedException)
        {
            return normalizedWidths;
        }

        return normalizedWidths;
    }

    internal static int ScaleToDevicePixels(int logicalWidth, int deviceDpi)
    {
        int normalizedDpi = NormalizeDpi(deviceDpi);
        return (int)Math.Round(logicalWidth * normalizedDpi / 96D, MidpointRounding.AwayFromZero);
    }

    internal static int ScaleToLogicalPixels(int deviceWidth, int deviceDpi)
    {
        int normalizedDpi = NormalizeDpi(deviceDpi);
        return (int)Math.Round(deviceWidth * 96D / normalizedDpi, MidpointRounding.AwayFromZero);
    }

    public void Dispose()
    {
        if (isDisposed)
        {
            return;
        }

        foreach ((GridView view, GridRegistration registration) in registrations)
        {
            view.ColumnWidthChanged -= ViewColumnWidthChanged;

            if (registration.FillRemainingColumnKey != null)
            {
                view.GridControl.Resize -= GridControlResize;
            }
        }

        registrations.Clear();
        isDisposed = true;
    }

    private void ViewColumnWidthChanged(object sender, ColumnEventArgs e)
    {
        if (isApplyingLayout || sender is not GridView view || !e.Column.Visible)
        {
            return;
        }

        if (!registrations.TryGetValue(view, out GridRegistration? registration))
        {
            return;
        }

        isApplyingLayout = true;
        view.BeginUpdate();
        try
        {
            FillRemainingWidth(view, registration);
            CaptureVisibleWidths(view, registration);
        }
        finally
        {
            view.EndUpdate();
            isApplyingLayout = false;
        }
    }

    private void GridControlResize(object? sender, EventArgs e)
    {
        if (isApplyingLayout)
        {
            return;
        }

        foreach ((GridView view, GridRegistration registration) in registrations)
        {
            if (registration.FillRemainingColumnKey == null || !ReferenceEquals(view.GridControl, sender))
            {
                continue;
            }

            isApplyingLayout = true;
            view.BeginUpdate();
            try
            {
                FillRemainingWidth(view, registration);
            }
            finally
            {
                view.EndUpdate();
                isApplyingLayout = false;
            }
        }
    }

    private void CaptureVisibleWidths(GridView view, GridRegistration registration)
    {
        if (!storedWidths.TryGetValue(registration.LayoutKey, out Dictionary<string, int>? columnWidths))
        {
            columnWidths = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            storedWidths[registration.LayoutKey] = columnWidths;
        }

        int deviceDpi = GetDeviceDpi();
        foreach (GridColumn column in view.VisibleColumns)
        {
            string columnKey = GetColumnKey(column);
            if (string.IsNullOrWhiteSpace(columnKey))
            {
                continue;
            }

            if (IsFillRemainingColumn(columnKey, registration))
            {
                continue;
            }

            int minimumLogicalWidth = GetMinimumLogicalWidth(columnKey, registration);
            int logicalWidth = ScaleToLogicalPixels(column.Width, deviceDpi);
            columnWidths[columnKey] = Math.Clamp(
                logicalWidth,
                minimumLogicalWidth,
                MaximumLogicalWidth);
        }
    }

    private void RestoreStoredWidths(GridView view, GridRegistration registration)
    {
        if (!storedWidths.TryGetValue(registration.LayoutKey, out Dictionary<string, int>? columnWidths))
        {
            return;
        }

        int deviceDpi = GetDeviceDpi();
        foreach (GridColumn column in view.VisibleColumns)
        {
            string columnKey = GetColumnKey(column);
            if (IsFillRemainingColumn(columnKey, registration))
            {
                continue;
            }

            if (!columnWidths.TryGetValue(columnKey, out int storedLogicalWidth))
            {
                continue;
            }

            int minimumLogicalWidth = GetMinimumLogicalWidth(columnKey, registration);
            int logicalWidth = Math.Clamp(
                storedLogicalWidth,
                minimumLogicalWidth,
                MaximumLogicalWidth);
            column.Width = ScaleToDevicePixels(logicalWidth, deviceDpi);
        }
    }

    private void ConfigureColumn(GridColumn column, GridRegistration registration)
    {
        string columnKey = GetColumnKey(column);
        if (string.IsNullOrWhiteSpace(columnKey))
        {
            return;
        }

        int minimumLogicalWidth = GetMinimumLogicalWidth(columnKey, registration);
        bool isFillRemainingColumn = IsFillRemainingColumn(columnKey, registration);
        column.OptionsColumn.AllowSize = !isFillRemainingColumn;
        column.OptionsColumn.FixedWidth = isFillRemainingColumn;
        column.MaxWidth = 0;
        column.MinWidth = ScaleToDevicePixels(minimumLogicalWidth, GetDeviceDpi());
    }

    private void FillRemainingWidth(GridView view, GridRegistration registration)
    {
        if (registration.FillRemainingColumnKey == null)
        {
            return;
        }

        GridColumn? fillColumn = view.Columns
            .FirstOrDefault(column =>
                column.Visible
                && IsFillRemainingColumn(GetColumnKey(column), registration));
        if (fillColumn == null)
        {
            return;
        }

        int viewportWidth = view.ViewRect.Width;
        if (viewportWidth <= 0)
        {
            viewportWidth = view.GridControl.ClientSize.Width;
        }

        if (viewportWidth <= 0)
        {
            return;
        }

        int occupiedWidth = 0;
        foreach (GridColumn column in view.VisibleColumns)
        {
            if (column != fillColumn)
            {
                occupiedWidth += column.Width;
            }
        }

        int minimumWidth = ScaleToDevicePixels(DefaultMinimumLogicalWidth, GetDeviceDpi());
        int paddingWidth = ScaleToDevicePixels(ViewportPaddingLogicalWidth, GetDeviceDpi());
        fillColumn.Width = Math.Max(minimumWidth, viewportWidth - occupiedWidth - paddingWidth);
    }

    private int GetDeviceDpi()
    {
        return NormalizeDpi(deviceDpiProvider());
    }

    private static int GetMinimumLogicalWidth(string columnKey, GridRegistration registration)
    {
        return registration.CompactColumnKeys.Contains(columnKey)
            ? CompactMinimumLogicalWidth
            : DefaultMinimumLogicalWidth;
    }

    private static bool IsFillRemainingColumn(string columnKey, GridRegistration registration)
    {
        return registration.FillRemainingColumnKey != null
            && string.Equals(
                columnKey,
                registration.FillRemainingColumnKey,
                StringComparison.OrdinalIgnoreCase);
    }

    private static string GetColumnKey(GridColumn column)
    {
        return string.IsNullOrWhiteSpace(column.FieldName)
            ? column.Name
            : column.FieldName;
    }

    private static int NormalizeDpi(int deviceDpi)
    {
        return deviceDpi > 0 ? deviceDpi : 96;
    }

    private sealed record GridRegistration(
        string LayoutKey,
        HashSet<string> CompactColumnKeys,
        string? FillRemainingColumnKey);
}
