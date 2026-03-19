using DevExpress.XtraEditors;
using DevExpress.XtraEditors.Controls;
using System;
using System.Data;

namespace Accounting.Utilities
{
    public static class PeriodeHelper
    {
        public static readonly string[] months = { "Januari", "Februari", "Maret", "April", "Mei", "Juni", "Juli", "Agustus", "September", "Oktober", "November", "Desember" };

        public static string GetMonthName(int monthNumber)
        {
            if (IsValidMonthNumber(monthNumber))
            {
                return months[monthNumber - 1];
            }
            else
            {
                return "Invalid month number";
            }
        }

        public static void LoadMonthNamesIntoComboBox(ComboBoxEdit comboBox)
        {
            comboBox.Properties.Items.Clear();
            comboBox.Properties.Items.AddRange(months);
            comboBox.Properties.TextEditStyle = TextEditStyles.DisableTextEditor;

            // Set default month based on the current month
            int defaultMonthNumber = DateTime.Now.Month;

            if (IsValidMonthNumber(defaultMonthNumber))
            {
                // Select the default month in the ComboBox
                comboBox.SelectedIndex = defaultMonthNumber - 1;
            }
            else
            {
                // Handle the case where the current month is not valid (unlikely)
                comboBox.SelectedIndex = 0; // Set a default index (e.g., January)
            }
        }

        private static bool IsValidMonthNumber(int monthNumber)
        {
            return monthNumber >= 1 && monthNumber <= 12;
        }

        public static void LoadTahunintoSpinEdit(SpinEdit spinedit)
        {
            // Set default year based on the current year
            int defaultTahunNumber = DateTime.Now.Year;

            spinedit.Properties.DisplayFormat.FormatType = DevExpress.Utils.FormatType.Numeric;
            spinedit.Properties.Mask.EditMask = "F0";
            spinedit.Properties.DisplayFormat.FormatString = "F0";


            spinedit.Value = defaultTahunNumber;

        }

        public static void LoadRemiseToLookupedit(LookUpEdit remiselookUpEdit)
        {
            // Create a DataTable and define its columns
            DataTable remiseTable = new();
            remiseTable.Columns.Add("Key", typeof(int));
            remiseTable.Columns.Add("Value", typeof(string));

            // Add rows to the DataTable
            remiseTable.Rows.Add(1, "Remise 1");
            remiseTable.Rows.Add(2, "Remise 2");

            // Bind the DataTable to the LookUpEdit control
            remiselookUpEdit.Properties.DataSource = remiseTable;
            remiselookUpEdit.Properties.DisplayMember = "Value";
            remiselookUpEdit.Properties.ValueMember = "Key";

            // Populate the columns of the LookUpEdit control

            // Hide the Key column
            remiselookUpEdit.Properties.ForceInitialize();
            remiselookUpEdit.Properties.PopulateColumns();


            remiselookUpEdit.Properties.Columns[0].Visible = false;

            // Set a default value (e.g., the first item in the DataTable)
            if (remiseTable.Rows.Count > 0)
            {
                remiselookUpEdit.EditValue = remiseTable.Rows[0]["Key"];
            }

        }


    }


}
