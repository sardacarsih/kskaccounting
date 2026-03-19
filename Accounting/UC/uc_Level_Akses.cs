using DevExpress.XtraEditors;
using DevExpress.XtraEditors.Controls;
using DevExpress.XtraEditors.Repository;
using DevExpress.XtraGrid.Columns;
using DevExpress.XtraGrid.Views.Base;
using DevExpress.XtraGrid.Views.Grid;
using Accounting._1.Interface;
using Accounting._2.DataAccess;
using Accounting._3.Services;
using Accounting.Models.Login;
using Oracle.ManagedDataAccess.Client;
using System.ComponentModel;
using System.Data;
using System;
using System.Linq;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Drawing;

namespace Accounting.UC
{
    public partial class uc_Level_Akses : UserControl
    {
        private object originalValue;
        private readonly IRolesAndUsers _rolesAndUsersService;
        int moduleId;
        DataTable RolesList; 
        public uc_Level_Akses() 
        {
            InitializeComponent();

            _rolesAndUsersService = new RolesAndUsersService();
            gridView2.RowUpdated += GridView_RowUpdated; 
           
        }



        private void SetupGridView()
        {
            ConfigureColumns();
            ConfigureToggleSwitch();
        }

        private void ConfigureColumns()
        {
            gridView1.Columns["DIVISIID"].OptionsColumn.ReadOnly = true;
            gridView1.Columns["DIVISI"].OptionsColumn.ReadOnly = true;

            // Set the DIVISIID column to be invisible
            gridView1.Columns["IDDATA"].Visible = false;
            gridView1.Columns["DIVISIID"].Visible = false;
        }

        private void ConfigureToggleSwitch()
        {
            RepositoryItemToggleSwitch toggleSwitch1 = CreateToggleSwitch();

            gridControl1.RepositoryItems.Add(toggleSwitch1);

            GridColumn isActiveColumn = gridView1.Columns["AKTIF"];
            GridColumn isDisplayColumn = gridView1.Columns["DISPLAY_AIS"];

            isActiveColumn.ColumnEdit = toggleSwitch1;
            isDisplayColumn.ColumnEdit = toggleSwitch1;
        }

        private RepositoryItemToggleSwitch CreateToggleSwitch()
        {
#pragma warning disable CS0618 // Type or member is obsolete
            var toggleSwitch = new RepositoryItemToggleSwitch
            {
                Name = "repositoryItemToggleSwitch",
                Properties =
                {
                    OnText = "Yes",
                    OffText = "No",
                    ShowText = true,
                    GlyphAlignment = DevExpress.Utils.HorzAlignment.Near,
                    AutoWidth = true,
                    UseParentBackground = true,
                    Appearance =
                    {
                        TextOptions =
                        {
                            HAlignment = DevExpress.Utils.HorzAlignment.Near,
                            VAlignment = DevExpress.Utils.VertAlignment.Center
                        }
                    }
                }
            };
#pragma warning restore CS0618 // Type or member is obsolete

            return toggleSwitch;
        }

       
     
      

        private void GridView_RowUpdated(object sender, RowObjectEventArgs e)
        {
            if (e.Row is Permission updatedRow)
            {
                try
                {
                    _rolesAndUsersService.UpdatePermissionInDatabase(updatedRow);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error updating data: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }


        private void uc_Level_Akses_Load(object sender, EventArgs e)
        {
            RolesList = Permission_Services.GetRoleList();
            moduleId = Permission_Services.GetModuleId(LoginInfo.MODULE);

            SetSidePanelWidth();

            ConfigureLookUpEdit();
            ConfigureGridControl1();
            ConfigureGridControl3();
            LoadUsersLevel();
        }

        private void SetSidePanelWidth()
        {
            int screenWidth = Screen.PrimaryScreen.Bounds.Width;
            sidePanel5.Width = screenWidth / 2;
        }

        private void ConfigureLookUpEdit()
        {
            if (RolesList != null && RolesList.Rows.Count > 0)
            {
                lookUpEdit1.Properties.DataSource = RolesList;
                lookUpEdit1.Properties.ValueMember = "ROLE_ID";
                lookUpEdit1.Properties.DisplayMember = "ROLE_NAME";
                lookUpEdit1.Properties.PopulateColumns();
                lookUpEdit1.Properties.Columns["ROLE_ID"].Visible = false;
                lookUpEdit1.EditValue = RolesList.Rows[0]["ROLE_ID"];
            }
        }

        private void ConfigureGridControl1()
        {
            var masterakses = Permission_Services.GetMasterAkses(LoginInfo.MODULE);
            gridControl1.DataSource = masterakses;
            gridView1.OptionsSelection.MultiSelect = true;
            gridView1.OptionsSelection.MultiSelectMode = GridMultiSelectMode.CheckBoxRowSelect;

            HideGridControl1Columns();
            gridView1.BestFitColumns();
            gridView1.OptionsBehavior.Editable = false;
            gridView1.CustomDrawCell += gridView1_CustomDrawCell;
        }

        private void HideGridControl1Columns()
        {
            gridView1.Columns["RoleId"].Visible = false;
            gridView1.Columns["PermissionId"].Visible = false;
            gridView1.Columns["PermissionName"].Visible = false;
            gridView1.Columns["CanCreate"].Visible = false;
            gridView1.Columns["CanRead"].Visible = false;
            gridView1.Columns["CanUpdate"].Visible = false;
            gridView1.Columns["CanDelete"].Visible = false;
        }

        private void ConfigureGridControl3()
        {
            var users = Permission_Services.GetUserList();
            gridControl3.DataSource = users;
            gridView3.Columns["ROLE_ID"].Visible = false;
            gridView3.BestFitColumns();
            gridView3.OptionsBehavior.Editable = false;
        }

        private void LoadUsersLevel()
        {

            var userslevelList = Permission_Services.GetUserLevelList(moduleId).ToList();
            gridControl4.DataSource = userslevelList;
            gridView4.Columns["AKTIF"].Visible = false;
            gridView4.Columns["ROLE_ID"].Caption = "AKSES";
            gridView4.BestFitColumns();

            ConfigureRepositoryItemLookUpEdit();

            // Subscribe to the ShowingEditor event to control cell editing
            gridView4.ShowingEditor += GridView4_ShowingEditor;
        }

        private void GridView4_ShowingEditor(object? sender, CancelEventArgs e)
        {
            if (sender is GridView view)
            {
                // Check if the focused column is ROLE_ID
                if (view.FocusedColumn.FieldName == "ROLE_ID")
                {
                    // Get the value of the USERID column for the focused row
#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
                    string userId = view.GetRowCellValue(view.FocusedRowHandle, "USERID")?.ToString();
#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.

                    // If USERID is "Administrator", cancel the editing
                    if (userId == "Administrator")
                    {
                        e.Cancel = true;
                        MessageBox.Show("Editing is not allowed for the Administrator role.", "Edit Restricted", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                }
                else
                {
                    // Prevent editing for all columns except ROLE_ID
                    e.Cancel = true;
                }
            }
        }


        private void ConfigureRepositoryItemLookUpEdit()
        {
            RepositoryItemLookUpEdit lookUpEdit = new()
            {
                DataSource = RolesList,
                DisplayMember = "ROLE_NAME",
                ValueMember = "ROLE_ID",
                NullText = "Select a role"
            };

            lookUpEdit.PopulateColumns();
            lookUpEdit.Columns["ROLE_ID"].Visible = false;

            gridControl4.RepositoryItems.Add(lookUpEdit);

            if (gridControl4.MainView is GridView gridView)
            {
                ConfigureGridView(gridView, lookUpEdit);
            } 
            lookUpEdit.EditValueChanged += LookUpEdit_EditValueChanged; 
            lookUpEdit.EditValueChanging += LookUpEdit_EditValueChanging;

            gridView3.CustomDrawCell += gridView3_CustomDrawCell;
        }

        private void LookUpEdit_EditValueChanging(object sender, ChangingEventArgs e)
        {
            if (sender is LookUpEdit edit)
            {
                // Store the original value before it changes
                originalValue = edit.EditValue;
            }
        }

        private void ConfigureGridView(GridView gridView, RepositoryItemLookUpEdit lookUpEdit)
        {
            gridView.Columns["ROLE_ID"].ColumnEdit = lookUpEdit;
            gridView.BestFitColumns();
        }



        private void gridView3_CustomDrawCell(object sender, RowCellCustomDrawEventArgs e)
        {
            // Ensure view is not null
            if (sender is not GridView view) return;

            // Retrieve the data sources

            // Ensure data sources are not null
            if (gridControl3.DataSource is not List<Permission_Users> || gridControl4.DataSource is not List<Permission_Users> list2) return;

            // Get the current item

            // Ensure the current item is not null and not in the second list
            if (view.GetRow(e.RowHandle) is Permission_Users currentItem && !IsInListUsers(currentItem, list2))
            {
                e.Appearance.BackColor = Color.Red;
            }
        }

        private bool IsInListUsers(Permission_Users currentItem, List<Permission_Users> list2)
        {
            // Convert list2 to a HashSet for faster lookups
            HashSet<string> list2Ids = new(list2.Select(p => p.USERID));
            return list2Ids.Contains(currentItem.USERID);
        }





        // Event handler to update the database when the role is changed
        private void LookUpEdit_EditValueChanged(object sender, EventArgs e)
        {
            try
            {
                if (sender is LookUpEdit edit)
                {
                    // Try to cast the new value to an integer safely
                    if (int.TryParse(edit.EditValue.ToString(), out int newRoleId))
                    {
                        // Get the focused row handle
                        if (gridControl4.FocusedView is GridView gridView)
                        {
                            int rowHandle = gridView.FocusedRowHandle;

                            // Get the other necessary values to update the database
#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
                            string userId = gridView.GetRowCellValue(rowHandle, "USERID")?.ToString();
#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.


                            if (!string.IsNullOrEmpty(userId))
                            {
                                // Call your database update method here
                                UpdateUserRoleInDatabase(newRoleId, userId, moduleId);

                                // Update the list item in the grid's data source
                                if (gridControl4.DataSource is List<Permission_Users> userslevelList)
                                {
                                    var user = userslevelList.FirstOrDefault(u => u.USERID == userId);
                                    if (user != null)
                                    {
                                        user.ROLE_ID = newRoleId;
                                    }
                                }
                            }
                            else
                            {
                                // Log or handle the case where userId is null or empty
                                MessageBox.Show("Error: USERID is null or empty.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            }
                        }
                        else
                        {
                            // Log or handle the case where gridView is null
                            MessageBox.Show("Error: GridView is null.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                    else
                    {
                        // Log or handle the case where EditValue is not a valid integer
                        MessageBox.Show("Error: Invalid ROLE_ID value.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
                else
                {
                    // Log or handle the case where sender is not a LookUpEdit
                    MessageBox.Show("Error: Sender is not a LookUpEdit.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                // Log the exception
                MessageBox.Show($"Exception: {ex.Message}", "Exception", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }


        private void UpdateUserRoleInDatabase(int newRoleId, string? userId, int moduleId)
        {
#pragma warning disable CS8604 // Possible null reference argument.
            Permission_Services.UpdateUserRole(newRoleId, userId, moduleId);
#pragma warning restore CS8604 // Possible null reference argument.
        }

        private void lookUpEdit1_EditValueChanged(object sender, EventArgs e)
        {
            LoadDaftarAksesbyRole();
            gridView1.RefreshData(); // Refresh gridView1 to trigger CustomDrawCell

        }

        private void LoadDaftarAksesbyRole()
        {
            try
            {
                // Fetch permissions for the selected role and module
                var akses = Permission_Services.GetRolePermissions(lookUpEdit1.Text, LoginInfo.MODULE);

                // Check if the result is not null
                if (akses != null)
                {
                    // Set the fetched permissions as the data source for the grid control
                    gridControl2.DataSource = akses;

                    // Make specific columns invisible in the grid view
                    gridView2.Columns["RoleId"].Visible = false;
                    gridView2.Columns["PermissionId"].Visible = false;
                    gridView2.Columns["PermissionName"].Visible = false;

                    // Adjust column widths to fit the content
                    gridView2.BestFitColumns();

                    // Subscribe to the ShowingEditor event to disable editing for the Admin role
#pragma warning disable CS8622 // Nullability of reference types in type of parameter doesn't match the target delegate (possibly because of nullability attributes).
                    gridView2.ShowingEditor -= gridView2_ShowingEditor; // Unsubscribe first to avoid multiple subscriptions
#pragma warning restore CS8622 // Nullability of reference types in type of parameter doesn't match the target delegate (possibly because of nullability attributes).
#pragma warning disable CS8622 // Nullability of reference types in type of parameter doesn't match the target delegate (possibly because of nullability attributes).
                    gridView2.ShowingEditor += gridView2_ShowingEditor;
#pragma warning restore CS8622 // Nullability of reference types in type of parameter doesn't match the target delegate (possibly because of nullability attributes).
                }
                else
                {
                    // Handle the case where no permissions are returned
                    MessageBox.Show("No permissions found for the selected role and module.", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                // Handle any exceptions that may occur
                MessageBox.Show($"An error occurred while loading permissions: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void gridView2_ShowingEditor(object sender, CancelEventArgs e)
        {
            // Disable editing if the role is "Admin"
            if (lookUpEdit1.Text == "Admin")
            {
                e.Cancel = true;
                // MessageBox.Show("Akses Role Admin tidak dapat diubah .", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                // Allow editing only for columns a, b, c, d, e
                if (sender is GridView view)
                {
                    string fieldName = view.FocusedColumn.FieldName;
                    if (fieldName != "CanCreate" && fieldName != "CanRead" && fieldName != "CanUpdate" && fieldName != "CanDelete")
                    {
                        e.Cancel = true;
                    }
                }
            }
        }


        private void gridView1_CustomDrawCell(object sender, RowCellCustomDrawEventArgs e)
        {
#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
            GridView view = sender as GridView;
#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.

            // Ensure view is not null
            if (view == null) return;

            // Retrieve the data sources
#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
            List<Permission> list1 = gridControl1.DataSource as List<Permission>;
#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.
#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
            List<Permission> list2 = gridControl2.DataSource as List<Permission>;
#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.

            // Ensure data sources are not null
            if (list1 == null || list2 == null) return;

            // Get the current item
#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
            Permission currentItem = view.GetRow(e.RowHandle) as Permission;
#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.

            // Ensure the current item is not null and not in the second list
            if (currentItem != null && !IsInList(currentItem, list2))
            {
                e.Appearance.BackColor = Color.Red;
            }
        }

        private bool IsInList(Permission currentItem, List<Permission> list2)
        {
            // Convert list2 to a HashSet for faster lookups
            HashSet<int> list2Ids = new HashSet<int>(list2.Select(p => p.PermissionId));
            return list2Ids.Contains(currentItem.PermissionId);
        }


        private void btncopycheckrow_Click(object sender, EventArgs e)
        {
            var list1 = _rolesAndUsersService.GetPermissionsFromGridControl(gridControl1);
            var list2 = _rolesAndUsersService.GetPermissionsFromGridControl(gridControl2);

            if (list1 == null || list2 == null)
            {
                _rolesAndUsersService.ShowMessage("Failed to retrieve permissions from grid controls.");
                return;
            }

            if (!_rolesAndUsersService.TryGetRoleId(lookUpEdit1.EditValue, out int roleId))
            {
                _rolesAndUsersService.ShowMessage("Invalid Role ID. Please select a valid role.");
                return;
            }

            using var conn = new OracleConnection(LoginInfo.OracleConnString);
            conn.Open();

            using var transaction = conn.BeginTransaction();
            using var cmd = conn.CreateCommand();
            cmd.Transaction = transaction;

            try
            {
                foreach (var rowHandle in gridView1.GetSelectedRows())
                {
                    var item = (Permission)gridView1.GetRow(rowHandle);

                    if (!list2.Any(p => p.PermissionId == item.PermissionId))
                    {
                        _rolesAndUsersService.AddPermission(list2, item);
                        _rolesAndUsersService.InsertPermissionToDatabase(roleId, item, cmd);
                    }
                }

                transaction.Commit();
                _rolesAndUsersService.RefreshGridData(gridView1);
                _rolesAndUsersService.ClearSelection(gridView1);
            }
            catch (Exception ex)
            {
                transaction.Rollback();
                _rolesAndUsersService.ShowMessage($"An error occurred: {ex.Message}");
            }

            _rolesAndUsersService.RefreshGridData(gridView2);

        }

        private void btnremoveakses_Click(object sender, EventArgs e)
        {
            if (!_rolesAndUsersService.TryGetRoleId(lookUpEdit1.EditValue, out int roleId))
            {
                _rolesAndUsersService.ShowMessage("Invalid Role ID. Please select a valid role.");
                return;
            }

            if (roleId == 1)
            {
                _rolesAndUsersService.ShowMessage("Akses Level ini tidak dapat dihapus.");
                return;
            }

            var permissionsToDelete = new List<Permission>();
            foreach (var rowHandle in gridView2.GetSelectedRows())
            {
                var item = (Permission)gridView2.GetRow(rowHandle);
                if (item != null)
                {
                    permissionsToDelete.Add(item);
                }
            }

            if (permissionsToDelete.Count == 0)
            {
                _rolesAndUsersService.ShowMessage("No permissions selected to delete.");
                return;
            }

            var permissionList = string.Join("\n", permissionsToDelete.Select(p => $"Menu: {p.Menu}, Keterangan: {p.Description}"));
            var result = _rolesAndUsersService.ShowConfirmationDialog($"The following permissions will be deleted:\n\n{permissionList}\n\nDo you want to proceed?");

            if (result == DialogResult.No)
            {
                return;
            }

            try
            {
                using var conn = new OracleConnection(LoginInfo.OracleConnString);
                conn.Open();
                using var cmd = conn.CreateCommand();
                _rolesAndUsersService.DeletePermissionsFromDatabase(roleId, permissionsToDelete, cmd);

                LoadDaftarAksesbyRole();
                _rolesAndUsersService.RefreshGridData(gridView1);

                _rolesAndUsersService.ShowMessage("Selected permissions have been deleted successfully.");
            }
            catch (OracleException ex)
            {
                _rolesAndUsersService.ShowMessage($"Database error: {ex.Message}");
            }
            catch (Exception ex)
            {
                _rolesAndUsersService.ShowMessage($"An error occurred: {ex.Message}");
            }
        }

        private void BtnAddUser_Click(object sender, EventArgs e)
        {
            // Get the data sources of gridView3 and gridView4
            List<Permission_Users> list1 = (List<Permission_Users>)gridControl3.DataSource;
            List<Permission_Users> list2 = (List<Permission_Users>)gridControl4.DataSource;

            // Get selected rows in gridView3
            int[] selectedRows = gridView3.GetSelectedRows();

            // Check if exactly one row is selected
            if (selectedRows.Length != 1)
            {
                MessageBox.Show("Please select exactly one user to add.");
                return;
            }

            // Get the data item from the selected row
            Permission_Users selectedItem = (Permission_Users)gridView3.GetRow(selectedRows[0]);

            // Set default role to User
            selectedItem.ROLE_ID = 6;

            try
            {
                // Initialize Oracle connection
                using OracleConnection conn = new OracleConnection(LoginInfo.OracleConnString);
                conn.Open();

                // Start a transaction
                using OracleTransaction transaction = conn.BeginTransaction();
                using OracleCommand cmd = conn.CreateCommand();
                cmd.Transaction = transaction;

                // Check if the item already exists in list2
                if (!list2.Any(p => p.USERID == selectedItem.USERID))
                {
                    // Add the item to the data source of gridView4 if it doesn't exist
                    list2.Add(selectedItem);

                    // Prepare the insert command
                    cmd.CommandText = @"
                                INSERT INTO MASTER_USER_ROLES (User_Id, Role_Id, Module_Id)
                                VALUES (:UserId, :RoleId, :ModuleId)";
                    cmd.Parameters.Clear();
                    cmd.Parameters.Add(new OracleParameter("UserId", selectedItem.USERID));
                    cmd.Parameters.Add(new OracleParameter("RoleId", selectedItem.ROLE_ID)); //default as User
                    cmd.Parameters.Add(new OracleParameter("ModuleId", moduleId));

                    // Execute the command
                    cmd.ExecuteNonQuery();
                }

                // Commit the transaction
                transaction.Commit();

                // Refresh gridView3 and clear selection
                gridView3.RefreshData();
                gridView3.ClearSelection();

                // Refresh gridView4 to show the new rows
                gridView4.RefreshData();
            }
            catch (Exception ex)
            {
                // Rollback the transaction on error
                MessageBox.Show($"An error occurred: {ex.Message}");
            }
        }

        private void BtnRemoveUser_Click(object sender, EventArgs e)
        {
            // Get the selected row handle
            var rowHandle = gridView4.GetSelectedRows().FirstOrDefault();

            // Retrieve the user from the selected row
            var userToDelete = (Permission_Users)gridView4.GetRow(rowHandle);

            // Check if no user is selected
            if (userToDelete == null)
            {
                _rolesAndUsersService.ShowMessage("No User selected to delete.");
                return;
            }

            // Prevent deletion of the Administrator user
            if (userToDelete.USERID == "Administrator")
            {
                _rolesAndUsersService.ShowMessage("The Administrator user cannot be deleted.");
                return;
            }

            // Confirm deletion
            var result = _rolesAndUsersService.ShowConfirmationDialog(
                $"The following User will be deleted:\n\nUserID: {userToDelete.USERID}, Name: {userToDelete.NAMA}\n\nDo you want to proceed?"
            );

            if (result == DialogResult.No)
            {
                return;
            }

            // Attempt to delete the user from the database
            try
            {
                using var conn = new OracleConnection(LoginInfo.OracleConnString);
                conn.Open();
                using (var cmd = conn.CreateCommand())
                {
                    // Ensure that the required parameters are correctly assigned
                    var userid = userToDelete.USERID;

                    _rolesAndUsersService.DeleteUser(userid, moduleId);

                    LoadUsersLevel();
                    _rolesAndUsersService.RefreshGridData(gridView3);

                    //_rolesAndUsersService.ShowMessage("Selected User has been deleted successfully.");
                }
            }
            catch (OracleException ex)
            {
                _rolesAndUsersService.ShowMessage($"Database error: {ex.Message}");
            }
            catch (Exception ex)
            {
                _rolesAndUsersService.ShowMessage($"An error occurred: {ex.Message}");
            }
        }



    }

    internal class PermissionIdComparer : IEqualityComparer<Permission>
    {
#pragma warning disable CS8767 // Nullability of reference types in type of parameter doesn't match implicitly implemented member (possibly because of nullability attributes).
#pragma warning disable CS8767 // Nullability of reference types in type of parameter doesn't match implicitly implemented member (possibly because of nullability attributes).
        public bool Equals(Permission x, Permission y)
#pragma warning restore CS8767 // Nullability of reference types in type of parameter doesn't match implicitly implemented member (possibly because of nullability attributes).
#pragma warning restore CS8767 // Nullability of reference types in type of parameter doesn't match implicitly implemented member (possibly because of nullability attributes).
        {
            // Check if PermissionIds are equal
            return x.PermissionId == y.PermissionId;
        }

        public int GetHashCode(Permission obj)
        {
            // Return hash code based on PermissionId
            return obj.PermissionId.GetHashCode();
        }
    }
}
