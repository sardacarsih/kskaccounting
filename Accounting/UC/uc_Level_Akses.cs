using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Accounting._3.Services;
using Accounting.Models.Login;
using Accounting.Services;
using DevExpress.XtraEditors;
using DevExpress.XtraGrid;
using DevExpress.XtraGrid.Views.Base;
using DevExpress.XtraGrid.Views.Grid;
using DevExpress.XtraTab;
using Oracle.ManagedDataAccess.Client;

namespace Accounting.UC
{
    public partial class uc_Level_Akses : UserControl
    {
        private readonly BindingSource roleBindingSource = new();
        private readonly BindingSource permissionBindingSource = new();
        private readonly BindingSource availableUserBindingSource = new();
        private readonly BindingSource assignedUserBindingSource = new();

        private readonly HashSet<string> highRiskPermissions = new(StringComparer.OrdinalIgnoreCase)
        {
            "USER_AKSES",
            "SETTING_DEVELOPER",
            "AUDIT_TRAIL_VIEW"
        };

        private XtraTabControl xtraTabControl1 = null!;
        private XtraTabPage tabRoles = null!;
        private XtraTabPage tabUsers = null!;
        private GroupControl groupRoles = null!;
        private GroupControl groupPermissions = null!;
        private GroupControl groupRoleActions = null!;
        private GroupControl groupRoleInsights = null!;
        private GroupControl groupAvailableUsers = null!;
        private GroupControl groupAssignedUsers = null!;
        private GridControl gridRoles = null!;
        private GridView gridViewRoles = null!;
        private GridControl gridPermissions = null!;
        private GridView gridViewPermissions = null!;
        private GridControl gridAvailableUsers = null!;
        private GridView gridViewAvailableUsers = null!;
        private GridControl gridAssignedUsers = null!;
        private GridView gridViewAssignedUsers = null!;
        private SimpleButton btnGrantReadOnly = null!;
        private SimpleButton btnGrantFullAccess = null!;
        private SimpleButton btnClearPermissions = null!;
        private SimpleButton btnSavePermissions = null!;
        private SimpleButton btnDiscardPermissions = null!;
        private SimpleButton btnDuplicateRole = null!;
        private SimpleButton btnRenameRole = null!;
        private SimpleButton btnDeleteRole = null!;
        private SimpleButton btnRefreshRoles = null!;
        private SimpleButton btnAssignUsersToRole = null!;
        private SimpleButton btnRemoveUsersFromRole = null!;
        private TextEdit txtNewRoleName = null!;
        private TextEdit txtRenameRoleName = null!;
        private MemoEdit memoRoleImpact = null!;
        private LabelControl lblRoleNameValue = null!;
        private LabelControl lblRoleStatusValue = null!;
        private LabelControl lblRoleUserCountValue = null!;
        private LabelControl lblRolePermissionCountValue = null!;
        private LabelControl lblRoleHighRiskValue = null!;
        private LabelControl lblUserRoleHint = null!;
        private LabelControl lblRolePanelHint = null!;

        private int moduleId;
        private bool isDirty;
        private bool isLoadingRole;
        private List<RoleSummary> roleSummaries = new();
        private List<Permission> originalPermissionSnapshot = new();
        private BindingList<Permission> permissionDraft = new();
        private List<Permission_Users> allUsers = new();
        private List<Permission_Users> moduleUserAssignments = new();
        private Dictionary<int, string> roleNameLookup = new();

        public uc_Level_Akses()
        {
            InitializeComponent();
            BuildLayout();
            ConfigureGrids();
            Load += uc_Level_Akses_Load;
        }

        private void BuildLayout()
        {
            xtraTabControl1 = new XtraTabControl
            {
                Dock = DockStyle.Fill
            };

            tabRoles = new XtraTabPage { Text = "Roles" };
            tabUsers = new XtraTabPage { Text = "Assignments" };

            xtraTabControl1.TabPages.AddRange(new[] { tabRoles, tabUsers });
            Controls.Add(xtraTabControl1);

            BuildRoleTab();
            BuildAssignmentTab();
            ApplyWorkspaceStyling();
        }

        private void BuildRoleTab()
        {
            SidePanel leftPanel = new() { Dock = DockStyle.Left, Width = 290 };
            SidePanel rightPanel = new() { Dock = DockStyle.Right, Width = 340 };
            SidePanel centerPanel = new() { Dock = DockStyle.Fill };

            tabRoles.Controls.Add(centerPanel);
            tabRoles.Controls.Add(rightPanel);
            tabRoles.Controls.Add(leftPanel);

            groupRoles = new GroupControl { Dock = DockStyle.Fill, Text = "Role Directory" };
            gridRoles = new GridControl { Dock = DockStyle.Fill, DataSource = roleBindingSource };
            gridViewRoles = new GridView(gridRoles);
            gridRoles.MainView = gridViewRoles;
            gridRoles.ViewCollection.Add(gridViewRoles);
            groupRoles.Controls.Add(gridRoles);
            leftPanel.Controls.Add(groupRoles);

            groupPermissions = new GroupControl { Dock = DockStyle.Fill, Text = "Permission Matrix" };
            SidePanel permissionActions = new() { Dock = DockStyle.Top, Height = 72 };
            gridPermissions = new GridControl { Dock = DockStyle.Fill, DataSource = permissionBindingSource };
            gridViewPermissions = new GridView(gridPermissions);
            gridPermissions.MainView = gridViewPermissions;
            gridPermissions.ViewCollection.Add(gridViewPermissions);
            groupPermissions.Controls.Add(gridPermissions);
            groupPermissions.Controls.Add(permissionActions);
            centerPanel.Controls.Add(groupPermissions);

            lblRolePanelHint = new LabelControl
            {
                Location = new Point(14, 8),
                Size = new Size(430, 16),
                Text = "Gunakan bulk actions untuk row terpilih, atau tanpa selection untuk seluruh matrix."
            };
            btnGrantReadOnly = new SimpleButton { Text = "Read Only", Size = new Size(96, 32), Location = new Point(13, 28) };
            btnGrantFullAccess = new SimpleButton { Text = "Full Access", Size = new Size(96, 32), Location = new Point(115, 28) };
            btnClearPermissions = new SimpleButton { Text = "Clear Access", Size = new Size(96, 32), Location = new Point(217, 28) };
            btnSavePermissions = new SimpleButton { Text = "Save", Size = new Size(76, 32), Location = new Point(319, 28) };
            btnDiscardPermissions = new SimpleButton { Text = "Discard", Size = new Size(76, 32), Location = new Point(401, 28) };
            btnGrantReadOnly.Click += btnGrantReadOnly_Click;
            btnGrantFullAccess.Click += btnGrantFullAccess_Click;
            btnClearPermissions.Click += btnClearPermissions_Click;
            btnSavePermissions.Click += btnSavePermissions_Click;
            btnDiscardPermissions.Click += btnDiscardPermissions_Click;
            permissionActions.Controls.Add(lblRolePanelHint);
            permissionActions.Controls.Add(btnGrantReadOnly);
            permissionActions.Controls.Add(btnGrantFullAccess);
            permissionActions.Controls.Add(btnClearPermissions);
            permissionActions.Controls.Add(btnSavePermissions);
            permissionActions.Controls.Add(btnDiscardPermissions);

            groupRoleActions = new GroupControl { Dock = DockStyle.Top, Height = 220, Text = "Role Actions" };
            groupRoleInsights = new GroupControl { Dock = DockStyle.Fill, Text = "Impact Review" };
            rightPanel.Controls.Add(groupRoleInsights);
            rightPanel.Controls.Add(groupRoleActions);

            LabelControl lblRenameRoleName = new() { Location = new Point(16, 34), Text = "Rename selected role:" };
            txtRenameRoleName = new TextEdit { Location = new Point(16, 55), Size = new Size(308, 24) };
            txtRenameRoleName.Properties.NullValuePrompt = "Contoh: GL_CONTROLLER";
            txtRenameRoleName.Properties.NullValuePromptShowForEmptyValue = true;
            btnRenameRole = new SimpleButton { Text = "Rename", Size = new Size(96, 32), Location = new Point(16, 87) };
            btnRenameRole.Click += btnRenameRole_Click;

            LabelControl lblNewRoleName = new() { Location = new Point(16, 131), Text = "Create from selected role:" };
            txtNewRoleName = new TextEdit { Location = new Point(16, 152), Size = new Size(308, 24) };
            txtNewRoleName.Properties.NullValuePrompt = "Contoh: GL_AUDITOR";
            txtNewRoleName.Properties.NullValuePromptShowForEmptyValue = true;
            btnRefreshRoles = new SimpleButton { Text = "Refresh", Size = new Size(86, 32), Location = new Point(16, 184) };
            btnDuplicateRole = new SimpleButton { Text = "Duplicate", Size = new Size(98, 32), Location = new Point(108, 184) };
            btnDeleteRole = new SimpleButton { Text = "Delete Role", Size = new Size(118, 32), Location = new Point(212, 184) };
            btnRefreshRoles.Click += btnRefreshRoles_Click;
            btnDuplicateRole.Click += btnDuplicateRole_Click;
            btnDeleteRole.Click += btnDeleteRole_Click;
            groupRoleActions.Controls.Add(lblRenameRoleName);
            groupRoleActions.Controls.Add(txtRenameRoleName);
            groupRoleActions.Controls.Add(btnRenameRole);
            groupRoleActions.Controls.Add(lblNewRoleName);
            groupRoleActions.Controls.Add(txtNewRoleName);
            groupRoleActions.Controls.Add(btnRefreshRoles);
            groupRoleActions.Controls.Add(btnDuplicateRole);
            groupRoleActions.Controls.Add(btnDeleteRole);

            CreateInsightLabels();
        }

        private void BuildAssignmentTab()
        {
            SidePanel leftPanel = new() { Dock = DockStyle.Left, Width = 390 };
            SidePanel actionPanel = new() { Dock = DockStyle.Left, Width = 128 };
            SidePanel rightPanel = new() { Dock = DockStyle.Fill };

            tabUsers.Controls.Add(rightPanel);
            tabUsers.Controls.Add(actionPanel);
            tabUsers.Controls.Add(leftPanel);

            groupAvailableUsers = new GroupControl { Dock = DockStyle.Fill, Text = "Available Users" };
            groupAssignedUsers = new GroupControl { Dock = DockStyle.Fill, Text = "Assigned Users" };

            gridAvailableUsers = new GridControl { Dock = DockStyle.Fill, DataSource = availableUserBindingSource };
            gridAssignedUsers = new GridControl { Dock = DockStyle.Fill, DataSource = assignedUserBindingSource };
            gridViewAvailableUsers = new GridView(gridAvailableUsers);
            gridViewAssignedUsers = new GridView(gridAssignedUsers);
            gridAvailableUsers.MainView = gridViewAvailableUsers;
            gridAssignedUsers.MainView = gridViewAssignedUsers;
            gridAvailableUsers.ViewCollection.Add(gridViewAvailableUsers);
            gridAssignedUsers.ViewCollection.Add(gridViewAssignedUsers);
            groupAvailableUsers.Controls.Add(gridAvailableUsers);
            groupAssignedUsers.Controls.Add(gridAssignedUsers);
            leftPanel.Controls.Add(groupAvailableUsers);
            rightPanel.Controls.Add(groupAssignedUsers);

            lblUserRoleHint = new LabelControl
            {
                AutoSizeMode = LabelAutoSizeMode.Vertical,
                Location = new Point(14, 24),
                Size = new Size(100, 42),
                Text = "Pilih role dulu di tab Roles"
            };

            btnAssignUsersToRole = new SimpleButton { Text = "<< Assign", Size = new Size(100, 38), Location = new Point(14, 88) };
            btnRemoveUsersFromRole = new SimpleButton { Text = "Remove >>", Size = new Size(100, 38), Location = new Point(14, 138) };
            btnAssignUsersToRole.Click += btnAssignUsersToRole_Click;
            btnRemoveUsersFromRole.Click += btnRemoveUsersFromRole_Click;
            actionPanel.Controls.Add(lblUserRoleHint);
            actionPanel.Controls.Add(btnAssignUsersToRole);
            actionPanel.Controls.Add(btnRemoveUsersFromRole);
        }

        private void CreateInsightLabels()
        {
            LabelControl lblRoleName = new() { Location = new Point(16, 20), Text = "Selected role:" };
            LabelControl lblRoleStatus = new() { Location = new Point(16, 47), Text = "Status:" };
            LabelControl lblRoleUserCount = new() { Location = new Point(16, 74), Text = "Assigned users:" };
            LabelControl lblRolePermissionCount = new() { Location = new Point(16, 101), Text = "Granted entries:" };
            LabelControl lblRoleHighRisk = new() { Location = new Point(16, 128), Text = "High-risk changes:" };

            lblRoleNameValue = CreateInsightValue(new Point(124, 20));
            lblRoleStatusValue = CreateInsightValue(new Point(124, 47));
            lblRoleUserCountValue = CreateInsightValue(new Point(124, 74));
            lblRolePermissionCountValue = CreateInsightValue(new Point(124, 101));
            lblRoleHighRiskValue = CreateInsightValue(new Point(124, 128));

            memoRoleImpact = new MemoEdit
            {
                Dock = DockStyle.Bottom,
                Height = 300
            };
            memoRoleImpact.Properties.ReadOnly = true;

            groupRoleInsights.Controls.Add(lblRoleName);
            groupRoleInsights.Controls.Add(lblRoleStatus);
            groupRoleInsights.Controls.Add(lblRoleUserCount);
            groupRoleInsights.Controls.Add(lblRolePermissionCount);
            groupRoleInsights.Controls.Add(lblRoleHighRisk);
            groupRoleInsights.Controls.Add(lblRoleNameValue);
            groupRoleInsights.Controls.Add(lblRoleStatusValue);
            groupRoleInsights.Controls.Add(lblRoleUserCountValue);
            groupRoleInsights.Controls.Add(lblRolePermissionCountValue);
            groupRoleInsights.Controls.Add(lblRoleHighRiskValue);
            groupRoleInsights.Controls.Add(memoRoleImpact);
        }

        private static LabelControl CreateInsightValue(Point location)
        {
            return new LabelControl
            {
                Location = location,
                Text = "-",
                Appearance =
                {
                    Font = new Font("Tahoma", 8.25F, FontStyle.Bold)
                }
            };
        }

        private void ApplyWorkspaceStyling()
        {
            BackColor = Color.FromArgb(244, 246, 248);

            xtraTabControl1.Appearance.BackColor = BackColor;
            xtraTabControl1.Appearance.Options.UseBackColor = true;
            xtraTabControl1.AppearancePage.Header.Font = new Font("Segoe UI Semibold", 9.75F, FontStyle.Bold);
            xtraTabControl1.AppearancePage.Header.Options.UseFont = true;
            xtraTabControl1.AppearancePage.PageClient.BackColor = BackColor;
            xtraTabControl1.AppearancePage.PageClient.Options.UseBackColor = true;

            StyleGroup(groupRoles);
            StyleGroup(groupPermissions);
            StyleGroup(groupRoleActions);
            StyleGroup(groupRoleInsights);
            StyleGroup(groupAvailableUsers);
            StyleGroup(groupAssignedUsers);

            StyleActionButton(btnGrantReadOnly, false);
            StyleActionButton(btnGrantFullAccess, false);
            StyleActionButton(btnClearPermissions, false);
            StyleActionButton(btnDiscardPermissions, false);
            StyleActionButton(btnRefreshRoles, false);
            StyleActionButton(btnDuplicateRole, true);
            StyleActionButton(btnRenameRole, true);
            StyleActionButton(btnDeleteRole, false, Color.FromArgb(135, 47, 58), Color.White);
            StyleActionButton(btnSavePermissions, true);
            StyleActionButton(btnAssignUsersToRole, true);
            StyleActionButton(btnRemoveUsersFromRole, false);

            StyleGrid(gridViewRoles);
            StyleGrid(gridViewPermissions);
            StyleGrid(gridViewAvailableUsers);
            StyleGrid(gridViewAssignedUsers);

            lblRolePanelHint.Appearance.ForeColor = Color.FromArgb(90, 96, 110);
            lblRolePanelHint.Appearance.Options.UseForeColor = true;
            lblUserRoleHint.Appearance.Font = new Font("Segoe UI", 9F, FontStyle.Regular);
            lblUserRoleHint.Appearance.Options.UseFont = true;
            memoRoleImpact.Properties.Appearance.BackColor = Color.FromArgb(252, 252, 252);
            memoRoleImpact.Properties.Appearance.Options.UseBackColor = true;
            memoRoleImpact.Properties.Appearance.Font = new Font("Consolas", 9F, FontStyle.Regular);
            memoRoleImpact.Properties.Appearance.Options.UseFont = true;
        }

        private static void StyleGroup(GroupControl groupControl)
        {
            groupControl.AppearanceCaption.Font = new Font("Segoe UI Semibold", 10F, FontStyle.Bold);
            groupControl.AppearanceCaption.ForeColor = Color.FromArgb(29, 42, 58);
            groupControl.AppearanceCaption.Options.UseFont = true;
            groupControl.AppearanceCaption.Options.UseForeColor = true;
            groupControl.BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.Simple;
        }

        private static void StyleActionButton(SimpleButton button, bool primary)
        {
            Color backColor = primary ? Color.FromArgb(28, 91, 168) : Color.FromArgb(232, 236, 240);
            Color foreColor = primary ? Color.White : Color.FromArgb(38, 50, 56);
            StyleActionButton(button, primary, backColor, foreColor);
        }

        private static void StyleActionButton(SimpleButton button, bool primary, Color backColor, Color foreColor)
        {
            button.Appearance.BackColor = backColor;
            button.Appearance.ForeColor = foreColor;
            button.Appearance.Font = new Font("Segoe UI Semibold", primary ? 9F : 8.75F, FontStyle.Bold);
            button.Appearance.Options.UseBackColor = true;
            button.Appearance.Options.UseForeColor = true;
            button.Appearance.Options.UseFont = true;
            button.LookAndFeel.UseDefaultLookAndFeel = false;
            button.LookAndFeel.Style = DevExpress.LookAndFeel.LookAndFeelStyle.Flat;
        }

        private static void StyleGrid(GridView gridView)
        {
            gridView.Appearance.HeaderPanel.Font = new Font("Segoe UI Semibold", 9F, FontStyle.Bold);
            gridView.Appearance.HeaderPanel.Options.UseFont = true;
            gridView.Appearance.Row.Font = new Font("Segoe UI", 9F, FontStyle.Regular);
            gridView.Appearance.Row.Options.UseFont = true;
            gridView.Appearance.FocusedRow.BackColor = Color.FromArgb(222, 235, 252);
            gridView.Appearance.FocusedRow.Options.UseBackColor = true;
            gridView.Appearance.HideSelectionRow.BackColor = Color.FromArgb(238, 242, 246);
            gridView.Appearance.HideSelectionRow.Options.UseBackColor = true;
            gridView.OptionsView.EnableAppearanceEvenRow = true;
            gridView.Appearance.EvenRow.BackColor = Color.FromArgb(249, 250, 251);
            gridView.Appearance.EvenRow.Options.UseBackColor = true;
            gridView.OptionsView.ShowIndicator = false;
        }

        private void ConfigureGrids()
        {
            ConfigureRoleGrid();
            ConfigurePermissionGrid();
            ConfigureUserGrid(gridViewAvailableUsers);
            ConfigureUserGrid(gridViewAssignedUsers);
        }

        private void ConfigureRoleGrid()
        {
            gridViewRoles.OptionsBehavior.Editable = false;
            gridViewRoles.OptionsBehavior.ReadOnly = true;
            gridViewRoles.OptionsFind.AlwaysVisible = true;
            gridViewRoles.OptionsFind.FindNullPrompt = "Cari role";
            gridViewRoles.OptionsView.ShowGroupPanel = false;
            gridViewRoles.FocusedRowChanged += gridViewRoles_FocusedRowChanged;
        }

        private void ConfigurePermissionGrid()
        {
            gridViewPermissions.OptionsFind.AlwaysVisible = true;
            gridViewPermissions.OptionsFind.FindNullPrompt = "Cari permission";
            gridViewPermissions.OptionsSelection.MultiSelect = true;
            gridViewPermissions.OptionsSelection.MultiSelectMode = GridMultiSelectMode.CheckBoxRowSelect;
            gridViewPermissions.OptionsView.ShowAutoFilterRow = true;
            gridViewPermissions.OptionsView.ShowGroupPanel = false;
            gridViewPermissions.CellValueChanged += gridViewPermissions_CellValueChanged;
            gridViewPermissions.ShowingEditor += gridViewPermissions_ShowingEditor;
            gridViewPermissions.RowStyle += gridViewPermissions_RowStyle;
        }

        private static void ConfigureUserGrid(GridView view)
        {
            view.OptionsFind.AlwaysVisible = true;
            view.OptionsSelection.MultiSelect = true;
            view.OptionsSelection.MultiSelectMode = GridMultiSelectMode.CheckBoxRowSelect;
            view.OptionsView.ShowAutoFilterRow = true;
            view.OptionsView.ShowGroupPanel = false;
            view.OptionsBehavior.Editable = false;
            view.OptionsBehavior.ReadOnly = true;
        }

        private RoleSummary? SelectedRole => gridViewRoles.GetFocusedRow() as RoleSummary;

        private void uc_Level_Akses_Load(object? sender, EventArgs e)
        {
            try
            {
                AuthorizationService.EnsureCanViewRoleManagement();
                moduleId = Permission_Services.GetModuleId(LoginInfo.MODULE);
                ReloadWorkspace();
                ApplyAuthorizationState();
            }
            catch (Exception ex)
            {
                XtraMessageBox.Show(ex.Message, "Akses Ditolak", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                Enabled = false;
            }
        }

        private void ReloadWorkspace(int? selectedRoleId = null)
        {
            LoadRoles(selectedRoleId);
            LoadUsers();
            RefreshUserAssignmentViews();
        }

        private void LoadRoles(int? selectedRoleId = null)
        {
            int? currentRoleId = selectedRoleId ?? SelectedRole?.RoleId;

            roleSummaries = Permission_Services.GetRoleSummaries(moduleId).ToList();
            roleNameLookup = roleSummaries.ToDictionary(role => role.RoleId, role => role.RoleName);
            roleBindingSource.DataSource = roleSummaries;
            ConfigureRoleColumns();

            if (roleSummaries.Count == 0)
            {
                originalPermissionSnapshot = new List<Permission>();
                permissionDraft = new BindingList<Permission>();
                permissionBindingSource.DataSource = permissionDraft;
                txtRenameRoleName.Text = string.Empty;
                txtNewRoleName.Text = string.Empty;
                UpdateRoleDetails();
                ApplyAuthorizationState();
                return;
            }

            int roleIndex = currentRoleId.HasValue
                ? roleSummaries.FindIndex(role => role.RoleId == currentRoleId.Value)
                : 0;

            isLoadingRole = true;
            gridViewRoles.FocusedRowHandle = roleIndex >= 0 ? roleIndex : 0;
            isLoadingRole = false;
            LoadSelectedRole();
        }

        private void LoadUsers()
        {
            allUsers = Permission_Services.GetUserList().Select(CloneUser).ToList();
            moduleUserAssignments = Permission_Services.GetUserLevelList(moduleId).Select(CloneUser).ToList();

            foreach (Permission_Users assignment in moduleUserAssignments)
            {
                assignment.ROLE_NAME = ResolveRoleName(assignment.ROLE_ID, assignment.ROLE_NAME);
            }
        }

        private void ConfigureRoleColumns()
        {
            if (gridViewRoles.Columns.Count == 0)
            {
                return;
            }

            gridViewRoles.Columns[nameof(RoleSummary.RoleId)].Visible = false;
            gridViewRoles.Columns[nameof(RoleSummary.IsProtected)].Visible = false;
            gridViewRoles.Columns[nameof(RoleSummary.IsSystemRole)].Visible = false;
            gridViewRoles.Columns[nameof(RoleSummary.RoleName)].Caption = "Role";
            gridViewRoles.Columns[nameof(RoleSummary.UserCount)].Caption = "Users";
            gridViewRoles.Columns[nameof(RoleSummary.Status)].Caption = "Status";
            gridViewRoles.BestFitColumns();
        }

        private void LoadSelectedRole()
        {
            RoleSummary? selectedRole = SelectedRole;
            if (selectedRole == null)
            {
                return;
            }

            isLoadingRole = true;
            try
            {
                originalPermissionSnapshot = Permission_Services.GetRolePermissionMatrix(selectedRole.RoleId, LoginInfo.MODULE)
                    .Select(ClonePermission)
                    .ToList();

                permissionDraft = new BindingList<Permission>(originalPermissionSnapshot.Select(ClonePermission).ToList());
                permissionBindingSource.DataSource = permissionDraft;
                ConfigurePermissionColumns();
                txtRenameRoleName.Text = selectedRole.RoleName;
                isDirty = false;
                UpdateRoleDetails();
                RefreshUserAssignmentViews();
            }
            finally
            {
                isLoadingRole = false;
            }
        }

        private void ConfigurePermissionColumns()
        {
            if (gridViewPermissions.Columns.Count == 0)
            {
                return;
            }

            gridViewPermissions.Columns[nameof(Permission.RoleId)].Visible = false;
            gridViewPermissions.Columns[nameof(Permission.PermissionId)].Visible = false;
            gridViewPermissions.Columns[nameof(Permission.PermissionName)].Visible = false;

            gridViewPermissions.Columns[nameof(Permission.Category)].Caption = "Area";
            gridViewPermissions.Columns[nameof(Permission.Menu)].Caption = "Menu";
            gridViewPermissions.Columns[nameof(Permission.Description)].Caption = "Description";
            gridViewPermissions.Columns[nameof(Permission.CanCreate)].Caption = "Create";
            gridViewPermissions.Columns[nameof(Permission.CanRead)].Caption = "Read";
            gridViewPermissions.Columns[nameof(Permission.CanUpdate)].Caption = "Update";
            gridViewPermissions.Columns[nameof(Permission.CanDelete)].Caption = "Delete";

            gridViewPermissions.Columns[nameof(Permission.Category)].GroupIndex = 0;
            gridViewPermissions.Columns[nameof(Permission.Category)].SortMode = DevExpress.XtraGrid.ColumnSortMode.DisplayText;
            gridViewPermissions.ExpandAllGroups();
            gridViewPermissions.BestFitColumns();
        }

        private void RefreshUserAssignmentViews()
        {
            RoleSummary? selectedRole = SelectedRole;
            if (selectedRole == null)
            {
                availableUserBindingSource.DataSource = new List<Permission_Users>();
                assignedUserBindingSource.DataSource = new List<Permission_Users>();
                groupAssignedUsers.Text = "Assigned Users";
                lblUserRoleHint.Text = "Pilih role dulu di tab Roles";
                return;
            }

            lblUserRoleHint.Text = $"Assign ke role:\n{selectedRole.RoleName}";
            groupAssignedUsers.Text = $"Assigned Users - {selectedRole.RoleName}";

            List<Permission_Users> assignedUsers = moduleUserAssignments
                .Where(user => user.ROLE_ID == selectedRole.RoleId)
                .OrderBy(user => user.NAMA)
                .Select(CloneUser)
                .ToList();

            Dictionary<string, Permission_Users> assignmentsByUser = moduleUserAssignments
                .GroupBy(user => user.USERID, StringComparer.OrdinalIgnoreCase)
                .Select(group => group.First())
                .ToDictionary(user => user.USERID, CloneUser, StringComparer.OrdinalIgnoreCase);

            List<Permission_Users> availableUsers = allUsers
                .Where(user => !assignedUsers.Any(assigned =>
                    string.Equals(assigned.USERID, user.USERID, StringComparison.OrdinalIgnoreCase)))
                .Select(user =>
                {
                    Permission_Users clone = CloneUser(user);
                    if (assignmentsByUser.TryGetValue(user.USERID, out Permission_Users? currentAssignment))
                    {
                        clone.ROLE_ID = currentAssignment.ROLE_ID;
                        clone.ROLE_NAME = ResolveRoleName(currentAssignment.ROLE_ID, currentAssignment.ROLE_NAME);
                    }

                    return clone;
                })
                .OrderBy(user => user.NAMA)
                .ToList();

            availableUserBindingSource.DataSource = availableUsers;
            assignedUserBindingSource.DataSource = assignedUsers;
            ConfigureUserColumns(gridViewAvailableUsers, true);
            ConfigureUserColumns(gridViewAssignedUsers, false);
        }

        private void ConfigureUserColumns(GridView view, bool showCurrentRole)
        {
            if (view.Columns.Count == 0)
            {
                return;
            }

            view.Columns[nameof(Permission_Users.AKTIF)].Visible = false;
            view.Columns[nameof(Permission_Users.ROLE_ID)].Visible = false;
            view.Columns[nameof(Permission_Users.USERID)].Caption = "User ID";
            view.Columns[nameof(Permission_Users.NAMA)].Caption = "Nama";
            view.Columns[nameof(Permission_Users.DEPT)].Caption = "Dept";
            view.Columns[nameof(Permission_Users.JABATAN)].Caption = "Jabatan";
            view.Columns[nameof(Permission_Users.ROLE_NAME)].Caption = showCurrentRole ? "Current Role" : "Role";
            view.Columns[nameof(Permission_Users.ROLE_NAME)].Visible = true;
            view.BestFitColumns();
        }

        private void UpdateRoleDetails()
        {
            RoleSummary? selectedRole = SelectedRole;
            if (selectedRole == null)
            {
                lblRoleNameValue.Text = "-";
                lblRoleStatusValue.Text = "-";
                lblRoleUserCountValue.Text = "-";
                lblRolePermissionCountValue.Text = "-";
                lblRoleHighRiskValue.Text = "-";
                memoRoleImpact.Text = "Pilih role untuk melihat detail.";
                return;
            }

            int grantedEntries = permissionDraft.Count(permission => HasAnyAccess(permission));
            int highRiskChangesCount = GetPermissionDiffs().Count(diff => highRiskPermissions.Contains(diff.PermissionName));
            List<Permission_Users> impactedUsers = moduleUserAssignments
                .Where(user => user.ROLE_ID == selectedRole.RoleId)
                .OrderBy(user => user.NAMA)
                .ToList();

            lblRoleNameValue.Text = selectedRole.RoleName;
            lblRoleStatusValue.Text = selectedRole.Status;
            lblRoleUserCountValue.Text = impactedUsers.Count.ToString();
            lblRolePermissionCountValue.Text = grantedEntries.ToString();
            lblRoleHighRiskValue.Text = highRiskChangesCount.ToString();

            string[] affectedUsers = impactedUsers.Select(user => user.NAMA).Take(8).ToArray();
            string previewUsers = affectedUsers.Length == 0
                ? "Belum ada user yang memakai role ini."
                : string.Join(", ", affectedUsers);

            List<string> diffLines = GetPermissionDiffs()
                .Take(10)
                .Select(diff => $"{diff.PermissionName}: {diff.ChangeSummary}")
                .ToList();

            memoRoleImpact.Text = string.Join(Environment.NewLine, new[]
            {
                $"Module: {LoginInfo.MODULE}",
                $"Users impacted: {impactedUsers.Count}",
                $"Role status: {selectedRole.Status}",
                string.Empty,
                "Sample users:",
                previewUsers,
                string.Empty,
                "Pending changes:",
                diffLines.Count == 0 ? "Tidak ada perubahan draft." : string.Join(Environment.NewLine, diffLines)
            });

            ApplyAuthorizationState();
        }

        private void ApplyAuthorizationState()
        {
            RoleSummary? selectedRole = SelectedRole;
            bool canManageRoleAssignments = AuthorizationService.CanManageRoleAssignments();
            bool canManageRolePermissions = AuthorizationService.CanManageRolePermissions();
            bool canEditSelectedRole = canManageRolePermissions
                && selectedRole != null
                && !AuthorizationService.IsProtectedRoleId(selectedRole.RoleId);

            btnGrantReadOnly.Enabled = canEditSelectedRole;
            btnGrantFullAccess.Enabled = canEditSelectedRole;
            btnClearPermissions.Enabled = canEditSelectedRole;
            btnSavePermissions.Enabled = canEditSelectedRole && isDirty;
            btnDiscardPermissions.Enabled = canEditSelectedRole && isDirty;
            txtRenameRoleName.Enabled = canEditSelectedRole;
            btnRenameRole.Enabled = canEditSelectedRole;
            btnDuplicateRole.Enabled = canManageRolePermissions && selectedRole != null;
            btnDeleteRole.Enabled = canEditSelectedRole;
            btnAssignUsersToRole.Enabled = canManageRoleAssignments && selectedRole != null;
            btnRemoveUsersFromRole.Enabled = canManageRoleAssignments && selectedRole != null;
        }

        private void gridViewRoles_FocusedRowChanged(object sender, FocusedRowChangedEventArgs e)
        {
            if (isLoadingRole)
            {
                return;
            }

            if (!ConfirmDiscardIfNeeded())
            {
                isLoadingRole = true;
                try
                {
                    gridViewRoles.FocusedRowHandle = e.PrevFocusedRowHandle;
                }
                finally
                {
                    isLoadingRole = false;
                }

                return;
            }

            LoadSelectedRole();
        }

        private bool ConfirmDiscardIfNeeded()
        {
            if (!isDirty)
            {
                return true;
            }

            DialogResult result = XtraMessageBox.Show(
                "Ada perubahan permission yang belum disimpan. Simpan sekarang?",
                "Unsaved Changes",
                MessageBoxButtons.YesNoCancel,
                MessageBoxIcon.Question);

            if (result == DialogResult.Cancel)
            {
                return false;
            }

            if (result == DialogResult.Yes)
            {
                return SavePermissionDraft();
            }

            ReloadPermissionDraft();
            return true;
        }

        private void ReloadPermissionDraft()
        {
            RoleSummary? selectedRole = SelectedRole;
            if (selectedRole == null)
            {
                return;
            }

            permissionDraft = new BindingList<Permission>(originalPermissionSnapshot.Select(ClonePermission).ToList());
            permissionBindingSource.DataSource = permissionDraft;
            ConfigurePermissionColumns();
            isDirty = false;
            UpdateRoleDetails();
        }

        private void gridViewPermissions_ShowingEditor(object? sender, CancelEventArgs e)
        {
            RoleSummary? selectedRole = SelectedRole;
            if (selectedRole == null)
            {
                e.Cancel = true;
                return;
            }

            if (!AuthorizationService.CanManageRolePermissions() || AuthorizationService.IsProtectedRoleId(selectedRole.RoleId))
            {
                e.Cancel = true;
                return;
            }

            if (sender is not GridView view || view.FocusedColumn == null)
            {
                e.Cancel = true;
                return;
            }

            string fieldName = view.FocusedColumn.FieldName;
            e.Cancel = fieldName != nameof(Permission.CanCreate)
                && fieldName != nameof(Permission.CanRead)
                && fieldName != nameof(Permission.CanUpdate)
                && fieldName != nameof(Permission.CanDelete);
        }

        private void gridViewPermissions_CellValueChanged(object sender, CellValueChangedEventArgs e)
        {
            if (isLoadingRole)
            {
                return;
            }

            if (e.Column.FieldName != nameof(Permission.CanCreate)
                && e.Column.FieldName != nameof(Permission.CanRead)
                && e.Column.FieldName != nameof(Permission.CanUpdate)
                && e.Column.FieldName != nameof(Permission.CanDelete))
            {
                return;
            }

            isDirty = true;
            UpdateRoleDetails();
        }

        private void gridViewPermissions_RowStyle(object sender, RowStyleEventArgs e)
        {
            if (gridViewPermissions.GetRow(e.RowHandle) is not Permission permission)
            {
                return;
            }

            if (highRiskPermissions.Contains(permission.PermissionName))
            {
                e.Appearance.BackColor = Color.FromArgb(255, 244, 230);
            }
        }

        private void btnGrantReadOnly_Click(object? sender, EventArgs e)
        {
            ApplyPermissionPreset(permission =>
            {
                permission.CanCreate = false;
                permission.CanRead = true;
                permission.CanUpdate = false;
                permission.CanDelete = false;
            });
        }

        private void btnGrantFullAccess_Click(object? sender, EventArgs e)
        {
            ApplyPermissionPreset(permission =>
            {
                permission.CanCreate = true;
                permission.CanRead = true;
                permission.CanUpdate = true;
                permission.CanDelete = true;
            });
        }

        private void btnClearPermissions_Click(object? sender, EventArgs e)
        {
            ApplyPermissionPreset(permission =>
            {
                permission.CanCreate = false;
                permission.CanRead = false;
                permission.CanUpdate = false;
                permission.CanDelete = false;
            });
        }

        private void ApplyPermissionPreset(Action<Permission> applyPreset)
        {
            if (!AuthorizationService.CanManageRolePermissions())
            {
                XtraMessageBox.Show("Anda tidak memiliki izin mengubah permission role.", "Akses Ditolak", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            int[] selectedRows = gridViewPermissions.GetSelectedRows().Where(rowHandle => rowHandle >= 0).ToArray();
            List<Permission> targets = selectedRows.Length > 0
                ? selectedRows.Select(rowHandle => gridViewPermissions.GetRow(rowHandle) as Permission).Where(permission => permission != null).Cast<Permission>().ToList()
                : permissionDraft.ToList();

            foreach (Permission permission in targets)
            {
                applyPreset(permission);
            }

            gridViewPermissions.RefreshData();
            isDirty = true;
            UpdateRoleDetails();
        }

        private void btnSavePermissions_Click(object? sender, EventArgs e)
        {
            _ = SavePermissionDraft();
        }

        private bool SavePermissionDraft()
        {
            RoleSummary? selectedRole = SelectedRole;
            if (selectedRole == null)
            {
                return false;
            }

            List<PermissionDiff> diffs = GetPermissionDiffs();
            if (diffs.Count == 0)
            {
                isDirty = false;
                UpdateRoleDetails();
                return true;
            }

            List<Permission_Users> impactedUsers = moduleUserAssignments
                .Where(user => user.ROLE_ID == selectedRole.RoleId)
                .OrderBy(user => user.NAMA)
                .ToList();

            string diffSummary = string.Join(Environment.NewLine, diffs.Take(10).Select(diff => $"- {diff.PermissionName}: {diff.ChangeSummary}"));
            string riskWarning = diffs.Any(diff => highRiskPermissions.Contains(diff.PermissionName))
                ? $"{Environment.NewLine}{Environment.NewLine}Perhatian: perubahan menyentuh permission high-risk."
                : string.Empty;

            DialogResult result = XtraMessageBox.Show(
                $"Role: {selectedRole.RoleName}{Environment.NewLine}" +
                $"Users impacted: {impactedUsers.Count}{Environment.NewLine}{Environment.NewLine}" +
                $"Perubahan:{Environment.NewLine}{diffSummary}{riskWarning}{Environment.NewLine}{Environment.NewLine}" +
                "Simpan perubahan ini?",
                "Review Impact",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (result != DialogResult.Yes)
            {
                return false;
            }

            try
            {
                Permission_Services.SaveRolePermissions(selectedRole.RoleId, permissionDraft.ToList(), LoginInfo.MODULE);
                originalPermissionSnapshot = permissionDraft.Select(ClonePermission).ToList();
                isDirty = false;
                UpdateRoleDetails();
                XtraMessageBox.Show("Permission role berhasil disimpan.", "Sukses", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return true;
            }
            catch (Exception ex)
            {
                XtraMessageBox.Show(ex.Message, "Gagal Menyimpan", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }

        private void btnDiscardPermissions_Click(object? sender, EventArgs e)
        {
            ReloadPermissionDraft();
        }

        private void btnDuplicateRole_Click(object? sender, EventArgs e)
        {
            RoleSummary? selectedRole = SelectedRole;
            if (selectedRole == null)
            {
                return;
            }

            string newRoleName = txtNewRoleName.Text?.Trim() ?? string.Empty;
            if (newRoleName.Length == 0)
            {
                XtraMessageBox.Show("Isi nama role baru terlebih dahulu.", "Validasi", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtNewRoleName.Focus();
                return;
            }

            try
            {
                int newRoleId = Permission_Services.DuplicateRole(newRoleName, selectedRole.RoleId, LoginInfo.MODULE);
                txtNewRoleName.Text = string.Empty;
                ReloadWorkspace(newRoleId);
                XtraMessageBox.Show("Role baru berhasil dibuat dari role terpilih.", "Sukses", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (OracleException ex) when (ex.Message.Contains("ORA-00001", StringComparison.OrdinalIgnoreCase))
            {
                XtraMessageBox.Show("Nama role sudah ada. Gunakan nama lain.", "Validasi", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            catch (Exception ex)
            {
                XtraMessageBox.Show(ex.Message, "Gagal Membuat Role", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnRenameRole_Click(object? sender, EventArgs e)
        {
            RoleSummary? selectedRole = SelectedRole;
            if (selectedRole == null)
            {
                return;
            }

            string newRoleName = txtRenameRoleName.Text?.Trim() ?? string.Empty;
            if (newRoleName.Length == 0)
            {
                XtraMessageBox.Show("Isi nama role baru untuk rename.", "Validasi", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtRenameRoleName.Focus();
                return;
            }

            if (string.Equals(newRoleName, selectedRole.RoleName, StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            DialogResult result = XtraMessageBox.Show(
                $"Ubah nama role {selectedRole.RoleName} menjadi {newRoleName}?",
                "Konfirmasi Rename",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (result != DialogResult.Yes)
            {
                txtRenameRoleName.Text = selectedRole.RoleName;
                return;
            }

            try
            {
                Permission_Services.RenameRole(selectedRole.RoleId, newRoleName);
                ReloadWorkspace(selectedRole.RoleId);
                XtraMessageBox.Show("Nama role berhasil diperbarui.", "Sukses", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (OracleException ex) when (ex.Message.Contains("ORA-00001", StringComparison.OrdinalIgnoreCase))
            {
                XtraMessageBox.Show("Nama role sudah digunakan. Pilih nama lain.", "Validasi", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            catch (Exception ex)
            {
                XtraMessageBox.Show(ex.Message, "Gagal Rename Role", MessageBoxButtons.OK, MessageBoxIcon.Error);
                txtRenameRoleName.Text = selectedRole.RoleName;
            }
        }

        private void btnDeleteRole_Click(object? sender, EventArgs e)
        {
            RoleSummary? selectedRole = SelectedRole;
            if (selectedRole == null)
            {
                return;
            }

            try
            {
                AuthorizationService.EnsureCanManageRolePermissions(selectedRole.RoleId);
            }
            catch (Exception ex)
            {
                XtraMessageBox.Show(ex.Message, "Akses Ditolak", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            List<Permission_Users> assignedUsers = Permission_Services.GetUsersByRole(selectedRole.RoleId, null).ToList();
            if (assignedUsers.Count > 0)
            {
                string userPreview = string.Join(Environment.NewLine, assignedUsers.Take(10).Select(user => $"{user.USERID} - {user.NAMA}"));
                XtraMessageBox.Show(
                    $"Role tidak bisa dihapus karena masih dipakai {assignedUsers.Count} user.{Environment.NewLine}{Environment.NewLine}{userPreview}",
                    "Delete Blocked",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                return;
            }

            DialogResult result = XtraMessageBox.Show(
                $"Hapus role {selectedRole.RoleName}?",
                "Konfirmasi Hapus",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning);

            if (result != DialogResult.Yes)
            {
                return;
            }

            try
            {
                Permission_Services.DeleteRole(selectedRole.RoleId);
                ReloadWorkspace();
            }
            catch (Exception ex)
            {
                XtraMessageBox.Show(ex.Message, "Gagal Menghapus Role", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnRefreshRoles_Click(object? sender, EventArgs e)
        {
            ReloadWorkspace(SelectedRole?.RoleId);
        }

        private void btnAssignUsersToRole_Click(object? sender, EventArgs e)
        {
            RoleSummary? selectedRole = SelectedRole;
            if (selectedRole == null)
            {
                return;
            }

            List<Permission_Users> selectedUsers = GetSelectedUsers(gridViewAvailableUsers);
            if (selectedUsers.Count == 0)
            {
                XtraMessageBox.Show("Pilih user yang ingin di-assign.", "Validasi", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                Permission_Services.AssignUsersToRole(selectedUsers.Select(user => user.USERID), selectedRole.RoleId, moduleId);
                LoadUsers();
                RefreshUserAssignmentViews();
            }
            catch (Exception ex)
            {
                XtraMessageBox.Show(ex.Message, "Gagal Assign User", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnRemoveUsersFromRole_Click(object? sender, EventArgs e)
        {
            List<Permission_Users> selectedUsers = GetSelectedUsers(gridViewAssignedUsers);
            if (selectedUsers.Count == 0)
            {
                XtraMessageBox.Show("Pilih user yang ingin dilepas dari role.", "Validasi", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                Permission_Services.RemoveUsersFromRole(selectedUsers.Select(user => user.USERID), moduleId);
                LoadUsers();
                RefreshUserAssignmentViews();
            }
            catch (Exception ex)
            {
                XtraMessageBox.Show(ex.Message, "Gagal Remove User", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private static List<Permission_Users> GetSelectedUsers(GridView view)
        {
            return view.GetSelectedRows()
                .Where(rowHandle => rowHandle >= 0)
                .Select(rowHandle => view.GetRow(rowHandle) as Permission_Users)
                .Where(user => user != null)
                .Cast<Permission_Users>()
                .Select(CloneUser)
                .ToList();
        }

        private List<PermissionDiff> GetPermissionDiffs()
        {
            Dictionary<int, Permission> originalById = originalPermissionSnapshot
                .ToDictionary(permission => permission.PermissionId, ClonePermission);

            return permissionDraft
                .Select(permission =>
                {
                    Permission original = originalById[permission.PermissionId];
                    string before = AccessSummary(original);
                    string after = AccessSummary(permission);
                    return new PermissionDiff(permission.PermissionName, before, after);
                })
                .Where(diff => !string.Equals(diff.Before, diff.After, StringComparison.Ordinal))
                .ToList();
        }

        private static string AccessSummary(Permission permission)
        {
            List<string> flags = new();
            if (permission.CanCreate)
            {
                flags.Add("C");
            }

            if (permission.CanRead)
            {
                flags.Add("R");
            }

            if (permission.CanUpdate)
            {
                flags.Add("U");
            }

            if (permission.CanDelete)
            {
                flags.Add("D");
            }

            return flags.Count == 0 ? "No access" : string.Join("/", flags);
        }

        private static bool HasAnyAccess(Permission permission)
        {
            return permission.CanCreate || permission.CanRead || permission.CanUpdate || permission.CanDelete;
        }

        private string ResolveRoleName(int roleId, string? fallback)
        {
            if (roleNameLookup.TryGetValue(roleId, out string? roleName))
            {
                return roleName;
            }

            return string.IsNullOrWhiteSpace(fallback) ? "-" : fallback.Trim();
        }

        private static Permission ClonePermission(Permission permission)
        {
            return new Permission
            {
                RoleId = permission.RoleId,
                PermissionId = permission.PermissionId,
                PermissionName = permission.PermissionName,
                Menu = permission.Menu,
                Description = permission.Description,
                Category = permission.Category,
                CanCreate = permission.CanCreate,
                CanRead = permission.CanRead,
                CanUpdate = permission.CanUpdate,
                CanDelete = permission.CanDelete
            };
        }

        private static Permission_Users CloneUser(Permission_Users user)
        {
            return new Permission_Users
            {
                USERID = user.USERID,
                ROLE_ID = user.ROLE_ID,
                NAMA = user.NAMA,
                DEPT = user.DEPT,
                JABATAN = user.JABATAN,
                ROLE_NAME = user.ROLE_NAME,
                AKTIF = user.AKTIF
            };
        }

        private sealed record PermissionDiff(string PermissionName, string Before, string After)
        {
            public string ChangeSummary => $"{Before} -> {After}";
        }
    }
}
