using System;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.Threading.Tasks;
using System.Windows.Forms;
using Accounting.BusinessLayer;
using Accounting.FixedAssets.Domain;
using DevExpress.XtraEditors;
using WinFormsComboBox = System.Windows.Forms.ComboBox;

namespace Accounting.Form;

public sealed class FrmFixedAssetMaster : XtraForm
{
    private sealed class CodeOption
    {
        public required string Code { get; init; }
        public required string Text { get; init; }
        public override string ToString() => Text;
    }

    private static string CurrentIdData => CompanyInfo.IDDATA?.Trim() ?? string.Empty;

    private readonly TextBox txtIdData = new();
    private readonly TextBox txtSearch = new();
    private readonly WinFormsComboBox cmbStatusFilter = new();
    private readonly Label lblInfo = new();
    private readonly DataGridView grid = new();

    private readonly TextBox txtAssetId = new();
    private readonly TextBox txtAssetCode = new();
    private readonly TextBox txtAssetName = new();
    private readonly WinFormsComboBox cmbCategory = new();
    private readonly WinFormsComboBox cmbGroup = new();
    private readonly DateTimePicker dtAcquisition = new();
    private readonly DateTimePicker dtInService = new();
    private readonly DateTimePicker dtDepStart = new();
    private readonly TextBox txtAcquisitionCost = new();
    private readonly TextBox txtResidual = new();
    private readonly TextBox txtUsefulLife = new();
    private readonly WinFormsComboBox cmbMethod = new();
    private readonly WinFormsComboBox cmbStatus = new();
    private readonly TextBox txtDepartment = new();
    private readonly TextBox txtCostCenter = new();
    private readonly TextBox txtLocation = new();
    private readonly TextBox txtVendor = new();
    private readonly TextBox txtSerial = new();
    private readonly TextBox txtCurrency = new();
    private readonly TextBox txtRate = new();
    private readonly TextBox txtNotes = new();
    private readonly DataGridView gridDeprHistory = new();
    private readonly DataGridView gridTrxHistory = new();

    private bool _isLoadingSelection;
    private bool _isNewRecord;

    public FrmFixedAssetMaster()
    {
        Text = "Aset Tetap - Data Master";
        Width = 1480;
        Height = 900;
        MinimumSize = new Size(1240, 760);
        StartPosition = FormStartPosition.CenterScreen;

        BuildUi();
        txtIdData.Text = CurrentIdData;
        cmbStatusFilter.SelectedIndex = 0;
        cmbMethod.SelectedIndex = 0;
        cmbStatus.SelectedIndex = 1;
        dtAcquisition.Value = DateTime.Today;
        dtInService.Value = DateTime.Today;
        dtDepStart.Value = DateTime.Today;
        txtCurrency.Text = "IDR";
        txtRate.Text = "1";
        txtUsefulLife.Text = "60";

        _ = InitializeAsync();
    }

    private async Task InitializeAsync()
    {
        await LoadReferenceDataAsync().ConfigureAwait(true);
        await LoadDataAsync().ConfigureAwait(true);
        StartNewEntry();
    }

    private void BuildUi()
    {
        SuspendLayout();
        ConfigureGrid(grid);
        ConfigureGrid(gridDeprHistory);
        ConfigureGrid(gridTrxHistory);
        ConfigureEditorControls();

        grid.SelectionChanged += async (_, _) => await LoadSelectedAssetDetailAsync().ConfigureAwait(true);
        cmbCategory.SelectedIndexChanged += async (_, _) => await ReloadGroupLookupAsync().ConfigureAwait(true);

        TableLayoutPanel shell = new()
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 2,
            Padding = new Padding(0)
        };
        shell.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        shell.RowStyles.Add(new RowStyle());
        shell.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));

        shell.Controls.Add(BuildFilterPanel(), 0, 0);
        shell.Controls.Add(BuildContentPanel(), 0, 1);

        Controls.Add(shell);
        ResumeLayout(performLayout: true);
    }

    private void ConfigureEditorControls()
    {
        txtAssetId.ReadOnly = true;
        txtAssetCode.ReadOnly = true;
        txtIdData.ReadOnly = true;
        txtIdData.TabStop = false;
        txtCurrency.CharacterCasing = CharacterCasing.Upper;

        cmbStatusFilter.DropDownStyle = ComboBoxStyle.DropDownList;
        cmbCategory.DropDownStyle = ComboBoxStyle.DropDownList;
        cmbGroup.DropDownStyle = ComboBoxStyle.DropDownList;
        cmbMethod.DropDownStyle = ComboBoxStyle.DropDownList;
        cmbStatus.DropDownStyle = ComboBoxStyle.DropDownList;

        txtNotes.Multiline = true;
        txtNotes.ScrollBars = ScrollBars.Vertical;
        txtNotes.MinimumSize = new Size(0, 88);

        ConfigureDatePicker(dtAcquisition, useCheckBox: false);
        ConfigureDatePicker(dtInService, useCheckBox: true);
        ConfigureDatePicker(dtDepStart, useCheckBox: true);

        cmbStatusFilter.Items.AddRange(
        [
            new CodeOption { Code = "ALL", Text = "Semua Status" },
            new CodeOption { Code = "DRAFT", Text = "Draf" },
            new CodeOption { Code = "ACTIVE", Text = "Aktif" },
            new CodeOption { Code = "UNDER_CONSTRUCTION", Text = "Dalam Pembangunan" },
            new CodeOption { Code = "DISPOSED", Text = "Dihapus" },
            new CodeOption { Code = "SOLD", Text = "Terjual" },
            new CodeOption { Code = "TRANSFERRED", Text = "Transfer" },
            new CodeOption { Code = "WRITTEN_OFF", Text = "Dihapusbukukan" },
            new CodeOption { Code = "RETIRED", Text = "Pensiun Aset" }
        ]);

        cmbMethod.Items.AddRange(
        [
            new CodeOption { Code = "SL", Text = "Garis Lurus (SL)" },
            new CodeOption { Code = "DB", Text = "Saldo Menurun (DB)" },
            new CodeOption { Code = "NONE", Text = "Tanpa Penyusutan" }
        ]);

        cmbStatus.Items.AddRange(
        [
            new CodeOption { Code = "DRAFT", Text = "Draf" },
            new CodeOption { Code = "ACTIVE", Text = "Aktif" },
            new CodeOption { Code = "UNDER_CONSTRUCTION", Text = "Dalam Pembangunan" },
            new CodeOption { Code = "DISPOSED", Text = "Dihapus" },
            new CodeOption { Code = "SOLD", Text = "Terjual" },
            new CodeOption { Code = "TRANSFERRED", Text = "Transfer" },
            new CodeOption { Code = "WRITTEN_OFF", Text = "Dihapusbukukan" },
            new CodeOption { Code = "RETIRED", Text = "Pensiun Aset" }
        ]);
    }

    private static void ConfigureDatePicker(DateTimePicker picker, bool useCheckBox)
    {
        picker.Format = DateTimePickerFormat.Custom;
        picker.CustomFormat = "dd-MMM-yyyy";
        picker.ShowCheckBox = useCheckBox;
    }

    private Control BuildFilterPanel()
    {
        Panel host = new()
        {
            Dock = DockStyle.Top,
            Padding = new Padding(12, 12, 12, 8),
            BackColor = Color.WhiteSmoke,
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink
        };

        TableLayoutPanel content = new()
        {
            Dock = DockStyle.Top,
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            ColumnCount = 1,
            RowCount = 3
        };
        content.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        content.RowStyles.Add(new RowStyle());
        content.RowStyles.Add(new RowStyle());
        content.RowStyles.Add(new RowStyle());

        TableLayoutPanel filters = CreateFilterLayout();
        FlowLayoutPanel actions = CreateActionFlowPanel();
        actions.Controls.Add(CreateButton("Cari", async (_, _) => await LoadDataAsync().ConfigureAwait(true), 100));
        actions.Controls.Add(CreateButton("Muat Ulang", async (_, _) => await LoadDataAsync().ConfigureAwait(true), 110));
        actions.Controls.Add(CreateButton("Baru", (_, _) => StartNewEntry(), 100));

        Button btnImportExcel = CreateButton("Import Excel", async (_, _) =>
        {
            using FrmImportFixedAsset dlg = new();
            if (dlg.ShowDialog(this) == DialogResult.OK)
            {
                await LoadDataAsync().ConfigureAwait(true);
            }
        }, 120);
        btnImportExcel.Enabled = FixedAssetUiRoleHelper.CanMasterEdit();
        actions.Controls.Add(btnImportExcel);

        lblInfo.Dock = DockStyle.Top;
        lblInfo.AutoSize = true;
        lblInfo.Margin = new Padding(0, 4, 0, 0);
        lblInfo.Text = "Siap.";

        content.Controls.Add(filters, 0, 0);
        content.Controls.Add(actions, 0, 1);
        content.Controls.Add(lblInfo, 0, 2);

        host.Controls.Add(content);
        return host;
    }

    private TableLayoutPanel CreateFilterLayout()
    {
        TableLayoutPanel filters = new()
        {
            Dock = DockStyle.Top,
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            ColumnCount = 4,
            RowCount = 2,
            Margin = new Padding(0)
        };
        filters.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 110F));
        filters.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
        filters.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 110F));
        filters.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
        filters.RowStyles.Add(new RowStyle());
        filters.RowStyles.Add(new RowStyle());

        AddFilterField(filters, 0, 0, "Lokasi Data", txtIdData);
        AddFilterField(filters, 0, 2, "Status Aset", cmbStatusFilter);
        AddWideField(filters, 1, "Pencarian", txtSearch);
        return filters;
    }

    private Control BuildContentPanel()
    {
        SplitContainer split = new()
        {
            Dock = DockStyle.Fill,
            Orientation = Orientation.Vertical,
            SplitterWidth = 6
        };
        ConfigureSplitContainer(split, preferredDistance: 520, panel1MinSize: 420, panel2MinSize: 560);

        split.Panel1.Padding = new Padding(12, 12, 6, 12);
        split.Panel2.Padding = new Padding(6, 12, 12, 12);
        split.Panel1.Controls.Add(BuildGridPanel());
        split.Panel2.Controls.Add(BuildEditorPanel());
        return split;
    }

    private Control BuildGridPanel()
    {
        GroupBox group = CreateSectionGroup("Daftar Aset");
        group.Dock = DockStyle.Fill;
        group.AutoSize = false;
        group.Padding = new Padding(10, 26, 10, 10);
        group.Controls.Add(grid);
        return group;
    }

    private Control BuildEditorPanel()
    {
        TabControl tabs = new() { Dock = DockStyle.Fill };
        TabPage tabEditor = new("Editor Aset");
        TabPage tabCard = new("Kartu Aset");

        tabEditor.Padding = new Padding(0);
        tabCard.Padding = new Padding(0);
        tabEditor.Controls.Add(BuildEditorFieldsPanel());
        tabCard.Controls.Add(BuildAssetCardPanel());

        tabs.TabPages.Add(tabEditor);
        tabs.TabPages.Add(tabCard);
        return tabs;
    }

    private Control BuildEditorFieldsPanel()
    {
        Panel scrollHost = new()
        {
            Dock = DockStyle.Fill,
            AutoScroll = true,
            Padding = new Padding(12)
        };

        TableLayoutPanel content = new()
        {
            Dock = DockStyle.Top,
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            ColumnCount = 1,
            RowCount = 6
        };
        content.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));

        int rowIndex = 0;
        content.Controls.Add(BuildIdentitySection(), 0, rowIndex++);
        content.Controls.Add(BuildClassificationSection(), 0, rowIndex++);
        content.Controls.Add(BuildDepreciationSection(), 0, rowIndex++);
        content.Controls.Add(BuildReferenceSection(), 0, rowIndex++);
        content.Controls.Add(BuildNotesSection(), 0, rowIndex++);
        content.Controls.Add(BuildActionSection(), 0, rowIndex);

        scrollHost.Controls.Add(content);
        return scrollHost;
    }

    private Control BuildAssetCardPanel()
    {
        SplitContainer split = new()
        {
            Dock = DockStyle.Fill,
            Orientation = Orientation.Horizontal,
            SplitterWidth = 6
        };
        ConfigureSplitContainer(split, preferredDistance: 220, panel1MinSize: 160, panel2MinSize: 160);

        split.Panel1.Padding = new Padding(12, 12, 12, 6);
        split.Panel2.Padding = new Padding(12, 6, 12, 12);
        split.Panel1.Controls.Add(BuildHistoryGroup("Riwayat Penyusutan", gridDeprHistory));
        split.Panel2.Controls.Add(BuildHistoryGroup("Riwayat Transaksi Siklus", gridTrxHistory));
        return split;
    }

    private static void ConfigureSplitContainer(
        SplitContainer split,
        int preferredDistance,
        int panel1MinSize,
        int panel2MinSize)
    {
        void ApplyConstraints(object? _, EventArgs __) =>
            ApplySplitContainerConstraints(split, preferredDistance, panel1MinSize, panel2MinSize);

        split.SizeChanged += ApplyConstraints;
        split.HandleCreated += ApplyConstraints;
        ApplyConstraints(null, EventArgs.Empty);
    }

    private static void ApplySplitContainerConstraints(
        SplitContainer split,
        int preferredDistance,
        int requestedPanel1MinSize,
        int requestedPanel2MinSize)
    {
        int availableSize = split.Orientation == Orientation.Vertical
            ? split.ClientSize.Width
            : split.ClientSize.Height;

        if (availableSize <= 0)
        {
            return;
        }

        int panel1MinSize = Math.Min(requestedPanel1MinSize, availableSize);
        int panel2MinSize = Math.Min(requestedPanel2MinSize, Math.Max(0, availableSize - panel1MinSize));

        int maxDistance = Math.Max(0, availableSize - panel2MinSize);
        int minDistance = Math.Min(panel1MinSize, maxDistance);
        int safeDistance = Math.Clamp(preferredDistance, minDistance, maxDistance);

        if (split.SplitterDistance != safeDistance)
        {
            split.SplitterDistance = safeDistance;
        }

        if (split.Panel1MinSize != panel1MinSize)
        {
            split.Panel1MinSize = panel1MinSize;
        }

        if (split.Panel2MinSize != panel2MinSize)
        {
            split.Panel2MinSize = panel2MinSize;
        }
    }

    private Control BuildIdentitySection()
    {
        TableLayoutPanel gridLayout = CreateFieldGrid();
        int rowIndex = 0;
        AddFieldPair(gridLayout, rowIndex++, "ID Aset", txtAssetId, "Kode Aset", txtAssetCode);
        AddWideField(gridLayout, rowIndex, "Nama Aset", txtAssetName);
        return CreateSectionGroup("Identitas Aset", gridLayout);
    }

    private Control BuildClassificationSection()
    {
        TableLayoutPanel gridLayout = CreateFieldGrid();
        int rowIndex = 0;
        AddFieldPair(gridLayout, rowIndex++, "Kategori", cmbCategory, "Kelompok", cmbGroup);
        AddFieldPair(gridLayout, rowIndex, "Status Aset", cmbStatus, "Mata Uang", txtCurrency);
        return CreateSectionGroup("Klasifikasi", gridLayout);
    }

    private Control BuildDepreciationSection()
    {
        TableLayoutPanel gridLayout = CreateFieldGrid();
        int rowIndex = 0;
        AddFieldPair(gridLayout, rowIndex++, "Tanggal Perolehan", dtAcquisition, "Tanggal Mulai Pakai", dtInService);
        AddFieldPair(gridLayout, rowIndex++, "Mulai Penyusutan", dtDepStart, "Metode Penyusutan", cmbMethod);
        AddFieldPair(gridLayout, rowIndex++, "Nilai Perolehan", txtAcquisitionCost, "Nilai Residu", txtResidual);
        AddFieldPair(gridLayout, rowIndex, "Masa Manfaat (Bulan)", txtUsefulLife, "Kurs", txtRate);
        return CreateSectionGroup("Tanggal & Penyusutan", gridLayout);
    }

    private Control BuildReferenceSection()
    {
        TableLayoutPanel gridLayout = CreateFieldGrid();
        int rowIndex = 0;
        AddFieldPair(gridLayout, rowIndex++, "Departemen", txtDepartment, "Pusat Biaya", txtCostCenter);
        AddFieldPair(gridLayout, rowIndex++, "Lokasi", txtLocation, "Vendor", txtVendor);
        AddWideField(gridLayout, rowIndex, "Nomor Seri", txtSerial);
        return CreateSectionGroup("Organisasi & Referensi", gridLayout);
    }

    private Control BuildNotesSection()
    {
        TableLayoutPanel gridLayout = CreateSingleFieldGrid();
        AddSingleColumnField(gridLayout, 0, "Catatan", txtNotes);
        return CreateSectionGroup("Catatan", gridLayout);
    }

    private Control BuildActionSection()
    {
        TableLayoutPanel content = new()
        {
            Dock = DockStyle.Top,
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            ColumnCount = 1,
            RowCount = 4,
            Padding = new Padding(0)
        };
        content.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));

        FlowLayoutPanel primaryActions = CreateActionFlowPanel();
        Button btnSave = CreateButton("Simpan", async (_, _) => await SaveAssetAsync().ConfigureAwait(true), 120);
        Button btnDelete = CreateButton("Hapus Logis", async (_, _) => await DeleteAssetAsync().ConfigureAwait(true), 120);
        Button btnClear = CreateButton("Bersihkan", (_, _) => StartNewEntry(), 120);
        btnSave.Enabled = FixedAssetUiRoleHelper.CanMasterEdit();
        btnDelete.Enabled = FixedAssetUiRoleHelper.CanMasterEdit();
        primaryActions.Controls.Add(btnSave);
        primaryActions.Controls.Add(btnDelete);
        primaryActions.Controls.Add(btnClear);

        Label lifecycleLabel = new()
        {
            Text = "Aksi Cepat Siklus Aset",
            AutoSize = true,
            Margin = new Padding(0, 8, 0, 0)
        };

        FlowLayoutPanel lifecycleActions = CreateActionFlowPanel();
        lifecycleActions.Controls.Add(CreateLifecycleButton("Penambahan", FixedAssetTransactionType.Improvement));
        lifecycleActions.Controls.Add(CreateLifecycleButton("Revaluasi", FixedAssetTransactionType.Revaluation));
        lifecycleActions.Controls.Add(CreateLifecycleButton("Transfer", FixedAssetTransactionType.Transfer));
        lifecycleActions.Controls.Add(CreateLifecycleButton("Penghapusan", FixedAssetTransactionType.FullDisposal));
        lifecycleActions.Controls.Add(CreateLifecycleButton("Penjualan", FixedAssetTransactionType.Sale));
        lifecycleActions.Controls.Add(CreateLifecycleButton("Penghapusbukuan", FixedAssetTransactionType.WriteOff));

        content.Controls.Add(primaryActions, 0, 0);
        content.Controls.Add(lifecycleLabel, 0, 1);
        content.Controls.Add(lifecycleActions, 0, 2);
        return CreateSectionGroup("Aksi", content);
    }

    private static Control BuildHistoryGroup(string title, Control gridControl)
    {
        GroupBox group = CreateSectionGroup(title);
        group.Dock = DockStyle.Fill;
        group.AutoSize = false;
        group.Padding = new Padding(10, 26, 10, 10);
        group.Controls.Add(gridControl);
        return group;
    }

    private static GroupBox CreateSectionGroup(string title, Control? content = null)
    {
        GroupBox group = new()
        {
            Text = title,
            Dock = DockStyle.Top,
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            Padding = new Padding(12, 26, 12, 12),
            Margin = new Padding(0, 0, 0, 12)
        };

        if (content is not null)
        {
            group.Controls.Add(content);
        }

        return group;
    }

    private static TableLayoutPanel CreateFieldGrid()
    {
        TableLayoutPanel gridLayout = new()
        {
            Dock = DockStyle.Top,
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            ColumnCount = 4,
            RowCount = 0,
            Margin = new Padding(0)
        };
        gridLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 150F));
        gridLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
        gridLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 150F));
        gridLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
        return gridLayout;
    }

    private static TableLayoutPanel CreateSingleFieldGrid()
    {
        TableLayoutPanel gridLayout = new()
        {
            Dock = DockStyle.Top,
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            ColumnCount = 2,
            RowCount = 0,
            Margin = new Padding(0)
        };
        gridLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 150F));
        gridLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        return gridLayout;
    }

    private static void AddFieldPair(
        TableLayoutPanel gridLayout,
        int rowIndex,
        string leftLabel,
        Control leftInput,
        string rightLabel,
        Control rightInput)
    {
        EnsureRow(gridLayout, rowIndex);
        gridLayout.Controls.Add(CreateFieldLabel(leftLabel), 0, rowIndex);
        gridLayout.Controls.Add(PrepareInput(leftInput), 1, rowIndex);
        gridLayout.Controls.Add(CreateFieldLabel(rightLabel), 2, rowIndex);
        gridLayout.Controls.Add(PrepareInput(rightInput), 3, rowIndex);
    }

    private static void AddWideField(TableLayoutPanel gridLayout, int rowIndex, string labelText, Control input)
    {
        EnsureRow(gridLayout, rowIndex);
        gridLayout.Controls.Add(CreateFieldLabel(labelText), 0, rowIndex);

        Control prepared = PrepareInput(input);
        gridLayout.Controls.Add(prepared, 1, rowIndex);
        gridLayout.SetColumnSpan(prepared, 3);
    }

    private static void AddSingleColumnField(TableLayoutPanel gridLayout, int rowIndex, string labelText, Control input)
    {
        EnsureRow(gridLayout, rowIndex);
        gridLayout.Controls.Add(CreateFieldLabel(labelText), 0, rowIndex);
        gridLayout.Controls.Add(PrepareInput(input), 1, rowIndex);
    }

    private static void AddFilterField(TableLayoutPanel gridLayout, int rowIndex, int columnIndex, string labelText, Control input)
    {
        EnsureRow(gridLayout, rowIndex);
        gridLayout.Controls.Add(CreateFieldLabel(labelText), columnIndex, rowIndex);
        gridLayout.Controls.Add(PrepareInput(input, new Padding(0, 0, 12, 8)), columnIndex + 1, rowIndex);
    }

    private static void EnsureRow(TableLayoutPanel gridLayout, int rowIndex)
    {
        while (gridLayout.RowCount <= rowIndex)
        {
            gridLayout.RowStyles.Add(new RowStyle());
            gridLayout.RowCount++;
        }
    }

    private static Label CreateFieldLabel(string text)
    {
        return new Label
        {
            Text = text,
            AutoSize = true,
            Anchor = AnchorStyles.Left,
            Margin = new Padding(0, 6, 12, 8)
        };
    }

    private static Control PrepareInput(Control input, Padding? margin = null)
    {
        input.Dock = DockStyle.Fill;
        input.Margin = margin ?? new Padding(0, 0, 12, 8);

        if (input is not TextBox textBox || textBox.Multiline)
        {
            return input;
        }

        textBox.MinimumSize = new Size(0, 28);
        return input;
    }

    private static FlowLayoutPanel CreateActionFlowPanel()
    {
        return new FlowLayoutPanel
        {
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            Dock = DockStyle.Top,
            WrapContents = true,
            FlowDirection = FlowDirection.LeftToRight,
            Margin = new Padding(0, 8, 0, 0),
            Padding = new Padding(0)
        };
    }

    private static Button CreateButton(string text, EventHandler onClick, int width)
    {
        Button button = new()
        {
            Text = text,
            Width = width,
            Height = 32,
            Margin = new Padding(0, 0, 8, 8),
            AutoSize = false
        };
        button.Click += onClick;
        return button;
    }

    private Button CreateLifecycleButton(string text, FixedAssetTransactionType transactionType)
    {
        Button button = CreateButton(text, async (_, _) => await OpenLifecycleDialogAsync(transactionType).ConfigureAwait(true), 128);
        button.Enabled = FixedAssetUiRoleHelper.CanLifecycleCreate();
        return button;
    }

    private static void ConfigureGrid(DataGridView target)
    {
        target.Dock = DockStyle.Fill;
        target.ReadOnly = true;
        target.AllowUserToAddRows = false;
        target.AllowUserToDeleteRows = false;
        target.AllowUserToResizeRows = false;
        target.MultiSelect = false;
        target.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        target.AutoGenerateColumns = true;
        target.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.DisplayedCells;
        target.DataBindingComplete += (_, _) => FixedAssetGridLocalization.Apply(target);
    }

    private async Task LoadReferenceDataAsync()
    {
        string idData = CurrentIdData;
        DataTable categoryTable = await Task.Run(() => FixedAssetQueryServices.GetAssetCategories(idData)).ConfigureAwait(true);
        cmbCategory.DataSource = categoryTable;
        cmbCategory.DisplayMember = "DISPLAY_NAME";
        cmbCategory.ValueMember = "CATEGORY_ID";
        await ReloadGroupLookupAsync().ConfigureAwait(true);
    }

    private async Task ReloadGroupLookupAsync()
    {
        string idData = CurrentIdData;
        long? categoryId = TryGetComboLong(cmbCategory);
        DataTable groupTable = await Task.Run(() => FixedAssetQueryServices.GetAssetGroups(idData, categoryId)).ConfigureAwait(true);

        DataRow allRow = groupTable.NewRow();
        allRow["GROUP_ID"] = DBNull.Value;
        allRow["DISPLAY_NAME"] = "(Tidak Ada)";
        groupTable.Rows.InsertAt(allRow, 0);

        cmbGroup.DataSource = groupTable;
        cmbGroup.DisplayMember = "DISPLAY_NAME";
        cmbGroup.ValueMember = "GROUP_ID";
    }

    private async Task LoadDataAsync()
    {
        try
        {
            string idData = CurrentIdData;
            string statusCode = GetSelectedCode(cmbStatusFilter, "ALL");
            string status = statusCode == "ALL" ? string.Empty : statusCode;
            string search = txtSearch.Text.Trim();

            DataTable table = await Task.Run(() => FixedAssetQueryServices.GetAssetMaster(idData, search, status)).ConfigureAwait(true);
            grid.DataSource = table;
            lblInfo.Text = $"Berhasil memuat {table.Rows.Count} aset.";

            if (table.Rows.Count > 0)
            {
                grid.ClearSelection();
                grid.Rows[0].Selected = true;
                if (grid.CurrentCell is null && grid.Columns.Count > 0)
                {
                    grid.CurrentCell = grid.Rows[0].Cells[0];
                }
                await LoadSelectedAssetDetailAsync().ConfigureAwait(true);
            }
            else
            {
                StartNewEntry();
            }
        }
        catch (Exception ex)
        {
            lblInfo.Text = "Gagal.";
            XtraMessageBox.Show(ex.Message, "Aset Tetap - Master", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private async Task LoadSelectedAssetDetailAsync()
    {
        if (_isLoadingSelection)
        {
            return;
        }

        if (grid.CurrentRow is null || grid.CurrentRow.DataBoundItem is not DataRowView rowView)
        {
            return;
        }

        if (!long.TryParse(rowView["ASSET_ID"]?.ToString(), out long assetId) || assetId <= 0)
        {
            return;
        }

        _isLoadingSelection = true;
        try
        {
            string idData = CurrentIdData;
            Task<DataTable> detailTask = Task.Run(() => FixedAssetQueryServices.GetAssetDetail(idData, assetId));
            Task<DataTable> depTask = Task.Run(() => FixedAssetQueryServices.GetAssetDepreciationHistory(idData, assetId));
            Task<DataTable> trxTask = Task.Run(() => FixedAssetQueryServices.GetAssetTransactionHistory(idData, assetId));

            await Task.WhenAll(detailTask, depTask, trxTask).ConfigureAwait(true);
            BindAssetDetail(detailTask.Result);
            gridDeprHistory.DataSource = depTask.Result;
            gridTrxHistory.DataSource = trxTask.Result;
            _isNewRecord = false;
        }
        finally
        {
            _isLoadingSelection = false;
        }
    }

    private void BindAssetDetail(DataTable detail)
    {
        if (detail.Rows.Count == 0)
        {
            return;
        }

        DataRow row = detail.Rows[0];
        txtAssetId.Text = row["ASSET_ID"]?.ToString() ?? string.Empty;
        txtAssetCode.Text = row["ASSET_CODE"]?.ToString() ?? string.Empty;
        txtAssetName.Text = row["ASSET_NAME"]?.ToString() ?? string.Empty;

        if (row["CATEGORY_ID"] != DBNull.Value)
        {
            cmbCategory.SelectedValue = Convert.ToInt64(row["CATEGORY_ID"]);
        }

        if (row["GROUP_ID"] == DBNull.Value)
        {
            cmbGroup.SelectedIndex = 0;
        }
        else
        {
            cmbGroup.SelectedValue = Convert.ToInt64(row["GROUP_ID"]);
        }

        dtAcquisition.Value = Convert.ToDateTime(row["ACQUISITION_DATE"]);
        SetOptionalDate(dtInService, row["IN_SERVICE_DATE"]);
        SetOptionalDate(dtDepStart, row["DEPRECIATION_START_DATE"]);

        txtAcquisitionCost.Text = Convert.ToDecimal(row["ACQUISITION_COST"]).ToString("n2", CultureInfo.CurrentCulture);
        txtResidual.Text = Convert.ToDecimal(row["RESIDUAL_VALUE"]).ToString("n2", CultureInfo.CurrentCulture);
        txtUsefulLife.Text = row["USEFUL_LIFE_MONTHS"]?.ToString() ?? "0";
        SelectComboByCode(cmbMethod, row["DEPR_METHOD"]?.ToString(), "SL");
        SelectComboByCode(cmbStatus, row["STATUS"]?.ToString(), "ACTIVE");
        txtDepartment.Text = row["DEPARTMENT_ID"]?.ToString() ?? string.Empty;
        txtCostCenter.Text = row["COST_CENTER_ID"]?.ToString() ?? string.Empty;
        txtLocation.Text = row["LOCATION_ID"]?.ToString() ?? string.Empty;
        txtVendor.Text = row["VENDOR_ID"]?.ToString() ?? string.Empty;
        txtSerial.Text = row["SERIAL_NO"]?.ToString() ?? string.Empty;
        txtCurrency.Text = row["CURRENCY_CODE"]?.ToString() ?? "IDR";
        txtRate.Text = Convert.ToDecimal(row["EXCHANGE_RATE"]).ToString("n4", CultureInfo.CurrentCulture);
        txtNotes.Text = row["NOTES"]?.ToString() ?? string.Empty;
    }

    private void StartNewEntry()
    {
        _isNewRecord = true;
        txtAssetId.Text = "0";
        txtAssetCode.Text = "(Otomatis)";
        txtAssetName.Clear();
        cmbCategory.SelectedIndex = cmbCategory.Items.Count > 0 ? 0 : -1;
        cmbGroup.SelectedIndex = cmbGroup.Items.Count > 0 ? 0 : -1;
        dtAcquisition.Value = DateTime.Today;
        dtInService.Checked = false;
        dtDepStart.Checked = false;
        txtAcquisitionCost.Text = "0";
        txtResidual.Text = "0";
        txtUsefulLife.Text = "60";
        SelectComboByCode(cmbMethod, "SL", "SL");
        SelectComboByCode(cmbStatus, "ACTIVE", "ACTIVE");
        txtDepartment.Clear();
        txtCostCenter.Clear();
        txtLocation.Clear();
        txtVendor.Clear();
        txtSerial.Clear();
        txtCurrency.Text = "IDR";
        txtRate.Text = "1";
        txtNotes.Clear();
        gridDeprHistory.DataSource = null;
        gridTrxHistory.DataSource = null;
    }

    private async Task SaveAssetAsync()
    {
        try
        {
            if (!FixedAssetUiRoleHelper.CanMasterEdit())
            {
                throw new InvalidOperationException("Role Anda tidak memiliki izin menyimpan data master aset.");
            }

            FixedAssetMasterSaveRequest request = BuildRequestFromEditor();
            long savedId = await Task.Run(() => FixedAssetQueryServices.SaveAsset(request, LoginInfo.userID)).ConfigureAwait(true);
            lblInfo.Text = _isNewRecord
                ? $"Aset berhasil dibuat. AssetId={savedId}."
                : $"Aset berhasil diperbarui. AssetId={savedId}.";
            await LoadDataAsync().ConfigureAwait(true);
            SelectGridAssetById(savedId);
        }
        catch (Exception ex)
        {
            lblInfo.Text = "Gagal menyimpan.";
            XtraMessageBox.Show(ex.Message, "Aset Tetap - Simpan Master", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private async Task DeleteAssetAsync()
    {
        try
        {
            if (!FixedAssetUiRoleHelper.CanMasterEdit())
            {
                throw new InvalidOperationException("Role Anda tidak memiliki izin menghapus data master aset.");
            }

            if (!long.TryParse(txtAssetId.Text.Trim(), out long assetId) || assetId <= 0)
            {
                throw new InvalidOperationException("Pilih aset yang valid terlebih dahulu.");
            }

            DialogResult confirm = XtraMessageBox.Show(
                $"Hapus logis aset {txtAssetCode.Text}?",
                "Konfirmasi",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);
            if (confirm != DialogResult.Yes)
            {
                return;
            }

            await Task.Run(() => FixedAssetQueryServices.SoftDeleteAsset(CurrentIdData, assetId, LoginInfo.userID)).ConfigureAwait(true);
            lblInfo.Text = $"Aset {txtAssetCode.Text} berhasil dihapus logis.";
            await LoadDataAsync().ConfigureAwait(true);
            StartNewEntry();
        }
        catch (Exception ex)
        {
            lblInfo.Text = "Gagal menghapus.";
            XtraMessageBox.Show(ex.Message, "Aset Tetap - Hapus Master", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private FixedAssetMasterSaveRequest BuildRequestFromEditor()
    {
        if (!decimal.TryParse(txtAcquisitionCost.Text.Trim(), NumberStyles.Any, CultureInfo.CurrentCulture, out decimal acquisitionCost)
            && !decimal.TryParse(txtAcquisitionCost.Text.Trim(), NumberStyles.Any, CultureInfo.InvariantCulture, out acquisitionCost))
        {
            throw new InvalidOperationException("Nilai perolehan tidak valid.");
        }

        if (!decimal.TryParse(txtResidual.Text.Trim(), NumberStyles.Any, CultureInfo.CurrentCulture, out decimal residual)
            && !decimal.TryParse(txtResidual.Text.Trim(), NumberStyles.Any, CultureInfo.InvariantCulture, out residual))
        {
            throw new InvalidOperationException("Nilai residu tidak valid.");
        }

        if (!decimal.TryParse(txtRate.Text.Trim(), NumberStyles.Any, CultureInfo.CurrentCulture, out decimal rate)
            && !decimal.TryParse(txtRate.Text.Trim(), NumberStyles.Any, CultureInfo.InvariantCulture, out rate))
        {
            throw new InvalidOperationException("Kurs tidak valid.");
        }

        if (!int.TryParse(txtUsefulLife.Text.Trim(), out int usefulLife) || usefulLife <= 0)
        {
            throw new InvalidOperationException("Masa manfaat harus > 0.");
        }

        long categoryId = TryGetComboLong(cmbCategory) ?? 0;
        if (categoryId <= 0)
        {
            throw new InvalidOperationException("Kategori wajib dipilih.");
        }

        long? groupId = TryGetComboLong(cmbGroup);
        long.TryParse(txtAssetId.Text.Trim(), out long assetId);

        return new FixedAssetMasterSaveRequest
        {
            AssetId = assetId,
            IdData = CurrentIdData,
            AssetCode = _isNewRecord ? string.Empty : txtAssetCode.Text.Trim(),
            AssetName = txtAssetName.Text.Trim(),
            CategoryId = categoryId,
            GroupId = groupId,
            AcquisitionDate = dtAcquisition.Value.Date,
            InServiceDate = dtInService.Checked ? dtInService.Value.Date : null,
            DepreciationStartDate = dtDepStart.Checked ? dtDepStart.Value.Date : null,
            AcquisitionCost = acquisitionCost,
            ResidualValue = residual,
            UsefulLifeMonths = usefulLife,
            DepreciationMethod = GetSelectedCode(cmbMethod, "SL"),
            Status = GetSelectedCode(cmbStatus, "ACTIVE"),
            DepartmentId = txtDepartment.Text.Trim(),
            CostCenterId = txtCostCenter.Text.Trim(),
            LocationId = txtLocation.Text.Trim(),
            VendorId = txtVendor.Text.Trim(),
            SerialNo = txtSerial.Text.Trim(),
            CurrencyCode = txtCurrency.Text.Trim(),
            ExchangeRate = rate <= 0 ? 1m : rate,
            Notes = txtNotes.Text.Trim()
        };
    }

    private async Task OpenLifecycleDialogAsync(FixedAssetTransactionType transactionType)
    {
        try
        {
            if (!FixedAssetUiRoleHelper.CanLifecycleCreate())
            {
                throw new InvalidOperationException("Role Anda tidak memiliki izin membuat transaksi siklus aset.");
            }

            if (!long.TryParse(txtAssetId.Text.Trim(), out long assetId) || assetId <= 0)
            {
                throw new InvalidOperationException("Pilih aset terlebih dahulu.");
            }

            using FrmFixedAssetLifecycle dialog = new(assetId, transactionType, $"ASET:{txtAssetCode.Text.Trim()}");
            dialog.ShowDialog(this);

            await LoadDataAsync().ConfigureAwait(true);
            SelectGridAssetById(assetId);
        }
        catch (Exception ex)
        {
            XtraMessageBox.Show(ex.Message, "Aksi Siklus Aset", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void SelectGridAssetById(long assetId)
    {
        foreach (DataGridViewRow row in grid.Rows)
        {
            if (row.Cells["ASSET_ID"]?.Value is null)
            {
                continue;
            }

            if (!long.TryParse(row.Cells["ASSET_ID"].Value.ToString(), out long rowId) || rowId != assetId)
            {
                continue;
            }

            row.Selected = true;
            grid.CurrentCell = row.Cells[0];
            break;
        }
    }

    private static long? TryGetComboLong(WinFormsComboBox combo)
    {
        if (combo.SelectedValue is null || combo.SelectedValue == DBNull.Value)
        {
            return null;
        }

        return long.TryParse(combo.SelectedValue.ToString(), out long value) ? value : null;
    }

    private static string GetSelectedCode(WinFormsComboBox combo, string defaultCode)
    {
        if (combo.SelectedItem is CodeOption option)
        {
            return option.Code;
        }

        return defaultCode;
    }

    private static void SelectComboByCode(WinFormsComboBox combo, string? code, string fallbackCode)
    {
        string targetCode = string.IsNullOrWhiteSpace(code)
            ? fallbackCode
            : code.Trim().ToUpperInvariant();

        for (int i = 0; i < combo.Items.Count; i++)
        {
            if (combo.Items[i] is not CodeOption option || !string.Equals(option.Code, targetCode, StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            combo.SelectedIndex = i;
            return;
        }

        for (int i = 0; i < combo.Items.Count; i++)
        {
            if (combo.Items[i] is not CodeOption option || !string.Equals(option.Code, fallbackCode, StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            combo.SelectedIndex = i;
            return;
        }

        combo.SelectedIndex = combo.Items.Count > 0 ? 0 : -1;
    }

    private static void SetOptionalDate(DateTimePicker picker, object value)
    {
        if (value == DBNull.Value || value is null)
        {
            picker.Checked = false;
            picker.Value = DateTime.Today;
            return;
        }

        picker.Checked = true;
        picker.Value = Convert.ToDateTime(value);
    }
}
