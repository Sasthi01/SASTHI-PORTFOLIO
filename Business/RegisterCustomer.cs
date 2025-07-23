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
    public partial class frmRegisterCustomer: Form
    {
        String loyalty;
        String customerType;
        String Province;
        public frmRegisterCustomer()
        {
            InitializeComponent();
            panel1.Parent = pictureBox1;
            label13.Parent = pictureBox1;
        }

       

        
        private void backBtn_Click(object sender, EventArgs e)
        {
            this.Close();
            btnClear.PerformClick(); // Clear the form fields when closing
        }


        private void custTypeComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            customerType = custTypeComboBox.Text;
            if (customerType == "Individual")
            {
                businessNameTxt.ReadOnly = true;
                businessNameTxt.Text = "N/A"; // Set default value for Individual customers
            }
            else if (customerType == "Business")
            {
                businessNameTxt.ReadOnly = false;
            }
        }

        private void createCustBtn_Click_1(object sender, EventArgs e)
        {
            try
            {
                string error = "";
                bool er = false;
                string errorMessage = "";
                bool hasError = false;
                if (string.IsNullOrWhiteSpace(nameTxt.Text) || string.IsNullOrWhiteSpace(surnameTxt.Text) ||
                    string.IsNullOrWhiteSpace(emailTxt.Text) || string.IsNullOrWhiteSpace(telNumTxt.Text) ||
                    string.IsNullOrWhiteSpace(custTypeComboBox.Text) || string.IsNullOrWhiteSpace(streetTxt.Text) ||
                    string.IsNullOrWhiteSpace(suburbTxt.Text) || string.IsNullOrWhiteSpace(cityTxt.Text) ||
                    string.IsNullOrWhiteSpace(provinceComboBox.Text) || string.IsNullOrWhiteSpace(postalCodetxt.Text) ||
                    string.IsNullOrWhiteSpace(loyaltyComboBox.Text))
                {

                    error += "Please fill in all fields.\n";
                    er = true;
                    if (string.IsNullOrWhiteSpace(businessNameTxt.Text) && custTypeComboBox.Text == "Business")
                    {
                        error += "Please fill in the Business Name.\n";
                        er = true;
                    }
                    else if (string.IsNullOrWhiteSpace(businessNameTxt.Text) && custTypeComboBox.Text == "Individual")
                    {
                        businessNameTxt.Text = "N/A";
                    }

                }

                // Name validation
                if (!Validator.ValidateName(nameTxt.Text, out errorMessage))
                {
                    error += "First Name Error: " + errorMessage + "\n";
                    hasError = true;
                }
                if (!Validator.ValidateName(surnameTxt.Text, out errorMessage))
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

                if (er == false)
                {
                    if (loyalty == "Yes")
                    {
                        customerTableAdapter1.Insert(nameTxt.Text, surnameTxt.Text, customerType, businessNameTxt.Text, telNumTxt.Text, emailTxt.Text, streetTxt.Text, suburbTxt.Text, cityTxt.Text, Province, postalCodetxt.Text, true, 0, DateTime.Now, "Active");
                    }
                    else
                    {
                        customerTableAdapter1.Insert(nameTxt.Text, surnameTxt.Text, customerType, businessNameTxt.Text, telNumTxt.Text, emailTxt.Text, streetTxt.Text, suburbTxt.Text, cityTxt.Text, Province, postalCodetxt.Text, false, 0, DateTime.Now, "Active");
                    }
                    MessageBox.Show("Customer Account Created Successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    MessageBox.Show(error, error, MessageBoxButtons.OK, MessageBoxIcon.Error);
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show("Something Went Wrong\n" + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void provinceComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            Province = provinceComboBox.Text;
        }

        private void loyaltyComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            loyalty = loyaltyComboBox.Text;
        }

        private void btnClear_Click_1(object sender, EventArgs e)
        {
            nameTxt.Clear();
            surnameTxt.Clear();
            emailTxt.Clear();
            telNumTxt.Clear();
            custTypeComboBox.SelectedIndex = -1; // Clear the role selection
            streetTxt.Clear();
            suburbTxt.Clear();
            cityTxt.Clear();
            provinceComboBox.SelectedIndex = -1; // Clear the province selection
            postalCodetxt.Clear();
            businessNameTxt.Clear();
            loyaltyComboBox.SelectedIndex = -1;
        }
    }
}
