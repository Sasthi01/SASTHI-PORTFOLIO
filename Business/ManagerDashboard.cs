using LGPACKAGING_POS_SYSTEM.Interface;
using LGPACKAGING_POS_SYSTEM.WstGrp21DataSetTableAdapters;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static LGPACKAGING_POS_SYSTEM.Interface.frmLogin;

namespace LGPACKAGING_POS_SYSTEM.Business
{
    public partial class frmManagerDashboard: Form
    {
        public frmManagerDashboard()
        {
            InitializeComponent();
            taStaff.FillDashboard(dsDashboard.Staff, Globals.staffPK);
            taProduct.FilterByStockLevel(dsDashboard.Product);
            txtNumLowStock.Text = taProduct.CountLowStock().ToString();
            txtNumOutOfStock.Text = taProduct.CountOutOfStock().ToString();
            panel1.Parent = pictureBox2;
            panel2.Parent = pictureBox2;
            panel3.Parent = pictureBox2;
        }

        private void frmManagerDashboard_Load(object sender, EventArgs e)
        {
            if (dsDashboard.Staff.Rows.Count > 0)
            {
                var row = dsDashboard.Staff.Rows[0];
                
                lblFullName.Text = row["firstName"].ToString() + " " + row["lastName"].ToString();
                txtRole.Text = row["role"].ToString();
                txtEmail.Text = row["emailAddress"].ToString();
                txtCellNum.Text = row["contactNumber"].ToString();
                lblStatus.Text = row["status"].ToString();

                txtStaffID.Text = row["staff_ID"].ToString();
                txtHireDate.Text = Convert.ToDateTime(row["employmentDate"]).ToString("dd/MM/yyyy");

                DateTime hireDate = Convert.ToDateTime(row["employmentDate"]);
                DateTime today = DateTime.Today;

                int years = today.Year - hireDate.Year;
                int months = today.Month - hireDate.Month;
                int days = today.Day - hireDate.Day;

                // Adjust if the hire day/month hasn't occurred yet this year
                if (days < 0)
                {
                    months--;
                    days += DateTime.DaysInMonth(today.Year, (today.Month == 1 ? 12 : today.Month - 1));
                }
                if (months < 0)
                {
                    years--;
                    months += 12;
                }

                txtWorkedFor.Text = $"Hired for {years} year(s), {months} month(s)";


                txtBirthDate.Text = row["dateOfBirth"].ToString();
                txtStreetAddress.Text = row["streetAddress"].ToString();
                txtCity.Text = row["city"].ToString();
                txtSuburb.Text = row["suburb"].ToString();
                cmbProvince.Text = row["province"].ToString();
                txtPostalCode.Text = row["postalCode"].ToString();
                string relativeImagePath = row["image"] + ".jpeg".ToString();
                string fullImagePath = Path.Combine(Application.StartupPath, relativeImagePath);
                string defaultImagePath = Path.Combine(Application.StartupPath,"user.png");
                if (File.Exists(fullImagePath))
                {
                    pbStaffImage.Image = Image.FromFile(fullImagePath);
                }
               
            }
        }

        private void btnUpdateDetails_Click(object sender, EventArgs e)
        {
            string error = "";
            bool er = false;
            string errorMessage = "";
            bool hasError = false;

            if (string.IsNullOrWhiteSpace(txtEmail.Text) || string.IsNullOrWhiteSpace(txtCellNum.Text))
            {
                errorMessage += "Please fill in all fields.\n";
                hasError = true;
            }
            // Validate Email
            if (!UserValidation.Validator.ValidateEmail(txtEmail.Text.Trim(), out errorMessage))
            {
                error += "Email Error: " + errorMessage + "\n";
                hasError = true;
            }

            // Validate Contact Number
            if (!UserValidation.Validator.ValidateContactNumber(txtCellNum.Text.Trim(), out errorMessage))
            {
                error += "Contact Number Error: " + errorMessage + "\n";
                hasError = true;
            }

            if (hasError)
            {
                MessageBox.Show(errorMessage, "Validation Errors", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            try
            {
                // Update if all validations pass
                taStaff.UpdateEmailContact(txtEmail.Text.Trim(), txtCellNum.Text.Trim(), Globals.staffPK);
                MessageBox.Show("Details updated successfully!");

                btnUpdateDetails.Visible = false;
                btnEditDetails.Visible = true;
                txtCellNum.ReadOnly = true;
                txtEmail.ReadOnly = true;
            }
            catch
            {
                MessageBox.Show("Failed to update details. Try again.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

         
        }


        

        private void btnEditDetails_Click_1(object sender, EventArgs e)
        {
            btnEditDetails.Visible = false;
            btnUpdateDetails.Visible = true;
            txtCellNum.ReadOnly = false;
            txtEmail.ReadOnly = false;

        }

        private void btnEditAddress_Click_1(object sender, EventArgs e)
        {
            btnEditAddress.Visible = false;
            btnUpdateAddress.Visible = true;
            txtStreetAddress.ReadOnly = false;
            txtCity.ReadOnly = false;
            txtSuburb.ReadOnly = false;
            cmbProvince.Enabled = true;
            txtPostalCode.ReadOnly = false;

        }

        private void btnUpdateAddress_Click_1(object sender, EventArgs e)
        {
            string error = "";
            bool er = false;
            string errorMessage = "";
            bool hasError = false;

            if (string.IsNullOrWhiteSpace(txtStreetAddress.Text) || string.IsNullOrWhiteSpace(txtSuburb.Text) ||
                string.IsNullOrWhiteSpace(txtCity.Text) || string.IsNullOrWhiteSpace(txtPostalCode.Text))
            {
                errorMessage += "Please fill in all fields.\n";
                hasError = true;
            }

            // Address validations
            if (!UserValidation.Validator.ValidateAddress(txtStreetAddress.Text, out errorMessage))
            {
                error += "Street Address Error: " + errorMessage + "\n";
                hasError = true;
            }
            if (!UserValidation.Validator.ValidateAddress(txtSuburb.Text, out errorMessage))
            {
                error += "Suburb Error: " + errorMessage + "\n";
                hasError = true;
            }
            if (!UserValidation.Validator.ValidateAddress(txtCity.Text, out errorMessage))
            {
                error += "City Error: " + errorMessage + "\n";
                hasError = true;
            }

            // Postal Code
            if (!UserValidation.Validator.ValidatePostalCode(txtPostalCode.Text, out errorMessage))
            {
                error += "Postal Code Error: " + errorMessage + "\n";
                hasError = true;
            }
            if (hasError)
            {
                MessageBox.Show(errorMessage, "Validation Errors", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            try
            {
                taStaff.UpdateAddress(txtStreetAddress.Text, txtSuburb.Text, txtCity.Text, cmbProvince.Text, txtPostalCode.Text, Globals.staffPK);
                MessageBox.Show("Address updated successfully!");
                btnUpdateAddress.Visible = false;
                btnEditAddress.Visible = true;
                txtStreetAddress.ReadOnly = true;
                txtCity.ReadOnly = true;
                txtSuburb.ReadOnly = true;
                cmbProvince.Enabled = false;
                txtPostalCode.ReadOnly = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to update address. Try again.\n" + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

           
        }
        

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cmbFilter.SelectedIndex != -1)
            {

                switch (cmbFilter.SelectedItem.ToString())
                {
                    case "No Filter":
                        taProduct.FilterByStockLevel(dsDashboard.Product);
                        break;
                    case "Low Stock":
                        taProduct.FilterByLowStock(dsDashboard.Product);
                        break;
                    case "Out Of Stock":
                        taProduct.FilterByOutOfStock(dsDashboard.Product);
                        break;
                    case "No Requests Sent":
                        taProduct.FilterByNoRequest(dsDashboard.Product);
                        break;
                }
            }
        }

        private void btnRequestReplenishment_Click(object sender, EventArgs e)
        {
            if (dgvProducts.CurrentRow != null)
            {
                if (chkAll.Checked == false)
                {
                    taProduct.UpdateRequestTrue(dgvProducts.CurrentRow.Cells[0].Value.ToString());
                    taProduct.FilterByStockLevel(dsDashboard.Product);
                }
                else {
                    taProduct.UpdateAllRequestTrue();
                    taProduct.FilterByStockLevel(dsDashboard.Product);
                }
            }
            else
            {
                MessageBox.Show("Please select a product to request replenishment on.");
                return;

            }
        }

        private void btnUnrequest_Click(object sender, EventArgs e)
        {
            if (dgvProducts.CurrentRow != null)
            {
                taProduct.UpdateRequestFalse(dgvProducts.CurrentRow.Cells[0].Value.ToString());
                taProduct.FilterByStockLevel(dsDashboard.Product);
            }
            else
            {
                MessageBox.Show("Please select a product to request replenishment on.");
                return;

            }
        }

        private void dgvProducts_CellValuePushed(object sender, DataGridViewCellValueEventArgs e)
        {
            
        }

        private void dgvProducts_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            
        }

        private void dgvProducts_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            
        }

        private void dgvProducts_ColumnStateChanged(object sender, DataGridViewColumnStateChangedEventArgs e)
        {
           
        }

        private void dgvProducts_MouseHover(object sender, EventArgs e)
        {
            
        }

        private void dgvProducts_DataBindingComplete(object sender, DataGridViewBindingCompleteEventArgs e)
        {
            for (int i = 0; i < dgvProducts.Rows.Count; i++)
            {
                if (dgvProducts.Rows[i] != null)
                {
                    if (dgvProducts.Rows[i].Cells[4].Value.ToString().Trim() == "Low Stock")
                    {
                        dgvProducts.Rows[i].Cells[4].Style.BackColor = Color.Orange;
                    }
                    else if (dgvProducts.Rows[i].Cells[4].Value.ToString().Trim() == "Out Of Stock")
                    {
                        dgvProducts.Rows[i].Cells[4].Style.BackColor = Color.Red;
                    }

                    if (dgvProducts.Rows[i].Cells[5].Value.ToString().Trim() == "True")
                    {
                        dgvProducts.Rows[i].Cells[5].Style.BackColor = Color.Green;
                    }
                    else
                    {
                        dgvProducts.Rows[i].Cells[5].Style.BackColor = Color.White;
                    }
                }
            }

            
        }

        private void dgvProducts_RowsAdded(object sender, DataGridViewRowsAddedEventArgs e)
        {
            
        }

        private void txtRole_TextChanged(object sender, EventArgs e)
        {

        }
    }
}
