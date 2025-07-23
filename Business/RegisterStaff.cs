using LGPACKAGING_POS_SYSTEM.Interface;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static LGPACKAGING_POS_SYSTEM.Interface.UserValidation;
namespace LGPACKAGING_POS_SYSTEM.Business
{
    public partial class frmRegisterStaff: Form
    {
        public frmRegisterStaff()
        {
            InitializeComponent();
            panel1.Parent = pictureBox1;
            label4.Parent = pictureBox1;
        }

        private void frmRegisterStaff_Load(object sender, EventArgs e)
        {

        }

        private void backBtn_Click(object sender, EventArgs e)
        {
            this.Close();
            btnClear.PerformClick();
        }

        private void createStaffBtn_Click_1(object sender, EventArgs e)
        {

            string errorMessage = "";
            string error = "";
            bool hasError = false;

            try
            {
                // Check required fields
                if (string.IsNullOrWhiteSpace(nameTxt.Text) || string.IsNullOrWhiteSpace(surnameTxt.Text) ||
                    string.IsNullOrWhiteSpace(emailTxt.Text) || string.IsNullOrWhiteSpace(telNumTxt.Text) ||
                    string.IsNullOrWhiteSpace(cmbRole.Text) || string.IsNullOrWhiteSpace(streetTxt.Text) ||
                    string.IsNullOrWhiteSpace(suburbTxt.Text) || string.IsNullOrWhiteSpace(cityTxt.Text) ||
                    string.IsNullOrWhiteSpace(provinceComboBox.Text) || string.IsNullOrWhiteSpace(postalCodetxt.Text) ||
                    string.IsNullOrWhiteSpace(txtPassword.Text) || string.IsNullOrWhiteSpace(txtConfirmedPassword.Text))
                {
                    error += "Please fill in all fields.\n";
                    hasError = true;
                }

                // Name validation
                if (!UserValidation.Validator.ValidateName(nameTxt.Text, out errorMessage))
                {
                    error += "First Name Error: " + errorMessage + "\n";
                    hasError = true;
                }
                if (!UserValidation.Validator.ValidateName(surnameTxt.Text, out errorMessage))
                {
                    error += "Surname Error: " + errorMessage + "\n";
                    hasError = true;
                }

                // Email validation
                if (!UserValidation.Validator.ValidateEmail(emailTxt.Text, out errorMessage))
                {
                    error += "Email Error: " + errorMessage + "\n";
                    hasError = true;
                }

                // Contact Number validation
                if (!UserValidation.Validator.ValidateContactNumber(telNumTxt.Text, out errorMessage))
                {
                    error += "Contact Number Error: " + errorMessage + "\n";
                    hasError = true;
                }

                // Address validations
                if (!UserValidation.Validator.ValidateAddress(streetTxt.Text, out errorMessage))
                {
                    error += "Street Address Error: " + errorMessage + "\n";
                    hasError = true;
                }
                if (!UserValidation.Validator.ValidateAddress(suburbTxt.Text, out errorMessage))
                {
                    error += "Suburb Error: " + errorMessage + "\n";
                    hasError = true;
                }
                if (!UserValidation.Validator.ValidateAddress(cityTxt.Text, out errorMessage))
                {
                    error += "City Error: " + errorMessage + "\n";
                    hasError = true;
                }


                // Postal Code
                if (!UserValidation.Validator.ValidatePostalCode(postalCodetxt.Text, out errorMessage))
                {
                    error += "Postal Code Error: " + errorMessage + "\n";
                    hasError = true;
                }

                // Password validation
                if (!UserValidation.Validator.ValidatePassword(txtPassword.Text, out errorMessage))
                {
                    error += "Password Error: " + errorMessage + "\n";
                    hasError = true;
                }

                // Confirm password match
                if (txtPassword.Text != txtConfirmedPassword.Text)
                {
                    error += "Confirmed password does not match the password.\n";
                    hasError = true;
                }

                // Check if date of birth is valid (e.g., must be at least 18 years old)
                if (dtpDOB.Value > DateTime.Now.AddYears(-18))
                {
                    error += "Staff member must be at least 18 years old.\n";
                    hasError = true;
                }

                // Final result
                if (!hasError)
                {
                    taStaff.Insert(
                        nameTxt.Text.Trim(),
                        surnameTxt.Text.Trim(),
                        emailTxt.Text.Trim(),
                        telNumTxt.Text.Trim(),
                        dtpDOB.Value,
                        cmbRole.Text,
                        DateTime.Now,
                        streetTxt.Text.Trim(),
                        suburbTxt.Text.Trim(),
                        cityTxt.Text.Trim(),
                        provinceComboBox.Text,
                        postalCodetxt.Text.Trim(),
                        txtPassword.Text,
                        "Active",
                        null
                    );

                    MessageBox.Show("Staff Member Created Successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    MessageBox.Show(error, "Validation Errors", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("An error occurred while creating the staff member: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

          

        }

        private void txtConfirmedPassword_TextChanged_1(object sender, EventArgs e)
        {
            if (txtConfirmedPassword.Text != txtPassword.Text)
            {
                txtPassword.ForeColor = Color.DarkRed;
                txtConfirmedPassword.ForeColor = Color.DarkRed;
            }
            else
            {
                txtPassword.ForeColor = Color.DarkGreen;
                txtConfirmedPassword.ForeColor = Color.DarkGreen;
            }
        }

        private void btnClear_Click_1(object sender, EventArgs e)
        {
            nameTxt.Clear();
            surnameTxt.Clear();
            emailTxt.Clear();
            telNumTxt.Clear();
            cmbRole.SelectedIndex = -1; // Clear the role selection
            streetTxt.Clear();
            suburbTxt.Clear();
            cityTxt.Clear();
            provinceComboBox.SelectedIndex = -1; // Clear the province selection
            postalCodetxt.Clear();
            txtPassword.Clear();
            txtConfirmedPassword.Clear();
            dtpDOB.Value = DateTime.Now; // Reset to current date
        }

        private bool passwordVisible = false;
        private void btnTogglePassword_Click(object sender, EventArgs e)
        {
            passwordVisible = !passwordVisible;
            txtPassword.UseSystemPasswordChar = !passwordVisible;
            txtConfirmedPassword.UseSystemPasswordChar = !passwordVisible;
        }
    }
}
