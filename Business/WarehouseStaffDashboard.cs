using LGPACKAGING_POS_SYSTEM.Interface;
using System;
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
    public partial class frmWarehouseStaffDashboard: Form
    {
        public frmWarehouseStaffDashboard()
        {
            InitializeComponent();
            taStaff.FillDashboard(dsWarehouseStaff.Staff, Globals.staffPK);
            taProduct.FilterByRequestTrue(dsWarehouseStaff.Product);
            taSale.FilterByConfirmedSales(dsWarehouseStaff.Sale);
            panel1.Parent = pictureBox1;
            panel2.Parent = pictureBox1;
            groupBox4.Parent = pictureBox1;
            groupBox5.Parent = pictureBox1;
        }

        private void frmWarehouseStaffDashboard_Load(object sender, EventArgs e)
        {
            if (dsWarehouseStaff.Staff.Rows.Count > 0)
            {
                var row = dsWarehouseStaff.Staff.Rows[0];
                // pbStaffImage.Image = Image.FromFile(row["image"].ToString());
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

                // Ensure the image path is valid and not null
                string relativeImagePath = row["image"] + ".jpeg".ToString();
                string fullImagePath = Path.Combine(Application.StartupPath, relativeImagePath);
               // string defaultImagePath = Path.Combine(Application.StartupPath, "user.png");
                if (File.Exists(fullImagePath))
                {
                    pbStaffImage.Image = Image.FromFile(fullImagePath);
                }
               
            }
  
        }

        private void btnEditDetails_Click(object sender, EventArgs e)
        {
            btnEditDetails.Visible = false;
            btnUpdateDetails.Visible = true;
            txtCellNum.ReadOnly = false;
            txtEmail.ReadOnly = false;
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

        private void btnEditAddress_Click(object sender, EventArgs e)
        {
            btnEditAddress.Visible = false;
            btnUpdateAddress.Visible = true;
            txtStreetAddress.ReadOnly = false;
            txtCity.ReadOnly = false;
            txtSuburb.ReadOnly = false;
            cmbProvince.Enabled = true;
            txtPostalCode.ReadOnly = false;
        }

        private void btnUpdateAddress_Click(object sender, EventArgs e)
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

        private void dgvSale_RowHeaderMouseDoubleClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            if (dgvSale.CurrentRow != null)
            {
                taSaleItem.FilterBySelectedSale(dsWarehouseStaff.SaleItem, Convert.ToInt32(dgvSale.CurrentRow.Cells[0].Value));
            }

        }

        private void txtSaleNumber_TextChanged(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(txtSaleNumber.Text))
            {
                taSale.SearchBySaleNo(dsWarehouseStaff.Sale, Convert.ToInt32(txtSaleNumber.Text));
            }
            else {
                taSale.FilterByConfirmedSales(dsWarehouseStaff.Sale);
            }
        }

        private void btnCompleted_Click(object sender, EventArgs e)
        {
            if (dgvSale.CurrentRow != null)
            {
                taSale.UpdateStatus("Completed", Convert.ToInt32(dgvSale.CurrentRow.Cells[0].Value));
                taSale.FilterByConfirmedSales(dsWarehouseStaff.Sale);
            }
            else { 
                MessageBox.Show("Please select a sale to mark as completed.", "No Sale Selected", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void label6_Click(object sender, EventArgs e)
        {

        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {

        }
    }
}
