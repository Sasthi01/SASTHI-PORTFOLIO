using LGPACKAGING_POS_SYSTEM.Interface;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Drawing.Printing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static LGPACKAGING_POS_SYSTEM.Business.frmIncentives;
using static LGPACKAGING_POS_SYSTEM.Interface.frmLogin;
using static LGPACKAGING_POS_SYSTEM.Interface.UserValidation;

namespace LGPACKAGING_POS_SYSTEM.Business
{
    public partial class frmSale : Form
    {
        public frmSale()
        {
            InitializeComponent();
            pictureBox1.Parent = pictureBox2;
        }

        private void label17_Click(object sender, EventArgs e)
        {

        }

        private void label2_Click(object sender, EventArgs e)
        {

        }

        private void frmSale_Load(object sender, EventArgs e)
        {
            taProduct.FillByForSale(dsSale.Product); ;
            
            taCustomer.Fill(dsSale.Customer);
            taSale.Fill(dsSale.Sale);
            
            groupBox1.Parent = pictureBox2;
            groupBox2.Parent = pictureBox2;
            groupBox4.Parent = pictureBox2;
            groupBox5.Parent = pictureBox2;
            groupBox6.Parent = pictureBox2;
            label8.Parent = pictureBox2;
            label9.Parent = pictureBox2;
            label11.Parent= pictureBox2;
      

        }

        private void dgvProducts_RowHeaderMouseDoubleClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            try
            {
                descriptionTxt.Text = dgvProducts.CurrentRow.Cells[2].Value.ToString();
                unitPriceTxt.Text = dgvProducts.CurrentRow.Cells[7].Value.ToString();
                DataRow dr;
                dr = dsSale.Invoice.NewRow();
                for (int i = 0; i < dr.ItemArray.Length; i++)
                {
                    foreach (DataColumn col in dr.Table.Columns)
                    {
                        if (col.ColumnName == "barcode_No")
                        {
                            dr[col] = dgvProducts.CurrentRow.Cells[0].Value; // Assuming column 0 is Product ID
                        }
                        else if (col.ColumnName == "description")
                        {
                            dr[col] = dgvProducts.CurrentRow.Cells[2].Value; // Assuming column 1 is Product Name
                        }
                        else if (col.ColumnName == "size")
                        {
                            dr[col] = dgvProducts.CurrentRow.Cells[3].Value; // Default quantity to 1 for simplicity
                        }
                        else if (col.ColumnName == "colour")
                        {
                            dr[col] = dgvProducts.CurrentRow.Cells[4].Value; // Assuming column 7 is Unit Price
                        }
                        else if (col.ColumnName == "unitsPerBale")
                        {
                            dr[col] = dgvProducts.CurrentRow.Cells[5].Value; // Assuming column 7 is Unit Price
                        }
                        else if (col.ColumnName == "sellingPrice")
                        {
                            dr[col] = dgvProducts.CurrentRow.Cells[7].Value; // Assuming column 7 is Unit Price
                        }
                        else if (col.ColumnName == "quantityInStock")
                        {
                            dr[col] = dgvProducts.CurrentRow.Cells[8].Value; // Assuming column 7 is Unit Price
                        }
                    }
                    //Getting text into text boxes
                    //descriptionTxt.Text = dgvProducts.CurrentRow.Cells[2].Value.ToString();
                    //unitPriceTxt.Text = dgvProducts.CurrentRow.Cells[7].Value.ToString();
                }
                dsSale.Invoice.Rows.Add(dr);
                string relativeImagePath = dgvProducts.CurrentRow.Cells[11].Value.ToString()+".png"; // e.g., "Images/product1.jpg"
                string fullImagePath = Path.Combine(Application.StartupPath, relativeImagePath);

                if (File.Exists(fullImagePath))
                {
                    pictureBox1.Image = Image.FromFile(fullImagePath);
                }
                else
                {
                    pictureBox1.Image = null;
                   // MessageBox.Show("Image not found:\n" + fullImagePath);
                }

                //vatAmountSale = ;
                //totalAmountSale = ;

            }
            catch (Exception ex)
            {
                MessageBox.Show("Error! Item not added" + ex.Message);
            }
        }


        private void prodSearch_TextChanged(object sender, EventArgs e)
        {
            taProduct.FilterByDescription(dsSale.Product, prodSearch.Text);
        }

        private void searchBtn_Click(object sender, EventArgs e)
        {

        }

        private void groupBox6_Enter(object sender, EventArgs e)
        {

        }

        private void cmbCustID_SelectedIndexChanged(object sender, EventArgs e)
        {


        }

        private void rbtnYes_CheckedChanged(object sender, EventArgs e)
        {
            if (rbtnNo.Checked == true)
            {
                rbtnYes.Checked = false;
                paymentMethodCombo.Items.Remove("Card with Loyalty Points");
                paymentMethodCombo.Items.Remove("Cash with Loyalty Points");

            }
            else if (rbtnYes.Checked == true)
            {
                rbtnNo.Checked = false;
                paymentMethodCombo.Items.Add("Card with Loyalty Points");
                paymentMethodCombo.Items.Add("Cash with Loyalty Points");
            }

        }

        private void rbtnNo_CheckedChanged(object sender, EventArgs e)
        {
            if (rbtnNo.Checked == true)
            {
                rbtnYes.Checked = false;
            }
            else if (rbtnYes.Checked == true)
            {
                rbtnNo.Checked = false;
            }
        }

        private void dgvInvoice_RowsAdded(object sender, DataGridViewRowsAddedEventArgs e)
        {
            //foreach (DataGridViewRow row in dgvInvoice.Rows)
            //{
            //    row.Cells[6].Style.BackColor = Color.Pink; // Assuming column 6 is the   
            //}
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (nameTxt.Text != "")
            {
                nameTxt.ReadOnly = false;
                surnameTxt.ReadOnly = false;
                if (custTypeComboBox.Text == "Business" && surnameTxt.ReadOnly == false)
                {
                    businessNameTxt.ReadOnly = false;
                }
                else if (custTypeComboBox.Text == "Individual" && surnameTxt.ReadOnly == false)
                {
                    businessNameTxt.ReadOnly = true;
                }

                telNumTxt.ReadOnly = false;
                emailTxt.ReadOnly = false;
                streetTxt.ReadOnly = false;
                suburbTxt.ReadOnly = false;
                cityTxt.ReadOnly = false;
                postalCodetxt.ReadOnly = false;

                saveBtn.Visible = true;
                button1.Visible = false;
                custTypeComboBox.Visible = true;
                txtCustomerType.Visible = false;
                //loyaltyBoolTxt.Visible = false;
                //loyaltyComboBox.Visible = true;
                rbtnNo.Enabled = true;
                rbtnYes.Enabled = true;
                provinceComboBox.Visible = true;
                txtProvince.Visible = false;
            }
            else {
                MessageBox.Show("Please Select a Customer before trying to Edit");
            }
        }

        private void saveBtn_Click(object sender, EventArgs e)
        {
            bool val = false;
            if (rbtnNo.Checked == true) {
                val = false;
            }
            else if (rbtnYes.Checked == true) {
                val = true;
            }

            string error = "";
            string errorMessage = "";
            bool hasError = false;

            // Check for empty required fields
            if (string.IsNullOrWhiteSpace(nameTxt.Text) ||
                string.IsNullOrWhiteSpace(surnameTxt.Text) ||
                string.IsNullOrWhiteSpace(emailTxt.Text) ||
                string.IsNullOrWhiteSpace(telNumTxt.Text) ||
                string.IsNullOrWhiteSpace(custTypeComboBox.Text) ||
                string.IsNullOrWhiteSpace(streetTxt.Text) ||
                string.IsNullOrWhiteSpace(suburbTxt.Text) ||
                string.IsNullOrWhiteSpace(cityTxt.Text) ||
                string.IsNullOrWhiteSpace(provinceComboBox.Text) ||
                string.IsNullOrWhiteSpace(postalCodetxt.Text))
            {
                error += "Please fill in all required fields.\n";
                hasError = true;
            }

            // Conditional check for business name
            if (string.IsNullOrWhiteSpace(businessNameTxt.Text) && custTypeComboBox.Text == "Business")
            {
                error += "Please fill in the Business Name.\n";
                hasError = true;
            }
            else if (string.IsNullOrWhiteSpace(businessNameTxt.Text) && custTypeComboBox.Text == "Individual")
            {
                businessNameTxt.Text = "N/A";
            }

            // Name Validation
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

            // Email Validation
            if (!UserValidation.Validator.ValidateEmail(emailTxt.Text, out errorMessage))
            {
                error += "Email Error: " + errorMessage + "\n";
                hasError = true;
            }

            // Contact Number
            if (!UserValidation.Validator.ValidateContactNumber(telNumTxt.Text, out errorMessage))
            {
                error += "Contact Number Error: " + errorMessage + "\n";
                hasError = true;
            }

            // Address Fields
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

            // Postal Code Validation
            if (!UserValidation.Validator.ValidatePostalCode(postalCodetxt.Text, out errorMessage))
            {
                error += "Postal Code Error: " + errorMessage + "\n";
                hasError = true;
            }

            // Loyalty option check
            if (!rbtnYes.Checked && !rbtnNo.Checked)
            {
                error += "Please select if loyalty is enabled.\n";
                hasError = true;
            }

            // ⛔ Stop and show errors if any validation failed
            if (hasError)
            {
                MessageBox.Show(error, "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try {
                taCustomer.UpdateQuery(nameTxt.Text, surnameTxt.Text, custTypeComboBox.Text, businessNameTxt.Text, telNumTxt.Text, emailTxt.Text, streetTxt.Text, suburbTxt.Text, cityTxt.Text, provinceComboBox.Text, postalCodetxt.Text, val, Convert.ToInt32(pointsAmtTxt.Text), custRegDate, "Active", custID);
                MessageBox.Show("Customer details updated successfully!");
                saveBtn.Hide();
                button1.Visible = true;
                nameTxt.ReadOnly = true;
                surnameTxt.ReadOnly = true;
                businessNameTxt.ReadOnly = true;
                telNumTxt.ReadOnly = true;
                emailTxt.ReadOnly = true;
                streetTxt.ReadOnly = true;
                suburbTxt.ReadOnly = true;
                cityTxt.ReadOnly = true;
                postalCodetxt.ReadOnly = true;

                saveBtn.Visible = false;
                button1.Visible = true;
                custTypeComboBox.Visible = false;
                txtCustomerType.Visible = true;
                rbtnNo.Enabled = false;
                rbtnYes.Enabled = false;
                //loyaltyBoolTxt.Visible = true;
                //loyaltyComboBox.Visible = false;
                provinceComboBox.Visible = false;
                txtProvince.Visible = true;
            }
            catch {
                MessageBox.Show("Failed to update customer Details. Try Again");
            }


        }

        private void RegCustBtn_Click(object sender, EventArgs e)
        {
            frmRegisterCustomer cust = new frmRegisterCustomer();
            cust.Show();

        }

        //UPDATE TOP(QuantityRequested) Item SET status = 'Sold' WHERE barcode_No = @barcode_No AND status='Available'


        public bool quantitiesValid = false;
        public int sale_No = 0;
        private void button2_Click(object sender, EventArgs e)
        {
            bool saleValid = false;
            if (quantitiesValid == true && custID != 0 && dgvInvoice.Rows.Count != 0 && paymentMethodCombo.Text != "")
            {
                DateTime today = DateTime.Now;
                DateTime sixMonthsLater = today.AddMonths(6);

                // Assuming sale_No is generated by the InsertQuery method
                sale_No = (int)taSale.InsertQuery(
                    custID,
                    Globals.staffPK,
                    Decimal.Parse(totalTxt.Text, NumberStyles.Currency),
                    paymentMethodCombo.Text,
                    Convert.ToDecimal(15),
                    Convert.ToInt32(pointsEarnedTxt.Text),
                    sixMonthsLater,
                    Convert.ToInt32(pointsUsedTxt.Text),
                    Decimal.Parse(DiscountTxt.Text, NumberStyles.Currency),
                    DateTime.Now,
                    "Confirmed"
                );

                try
                {
                    if (rbtnYes.Checked && (paymentMethodCombo.Text== "Card with Loyalty Points"|| paymentMethodCombo.Text == "Cash with Loyalty Points"))
                    {
                        taCustomer.UpdateLoyaltyDecrement(Convert.ToInt32(pointsUsedTxt.Text), custID);
                        taCustomer.UpdateLoyaltyPoints(Convert.ToInt32(pointsEarnedTxt.Text), custID);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error updating loyalty points: " + ex.Message);
                }

                for (int i = 0; i < dgvInvoice.Rows.Count; i++)
                {
                    int quantityReq = (int)dgvInvoice.Rows[i].Cells[7].Value;
                    string barcode = dgvInvoice.Rows[i].Cells[0].Value.ToString();

                    
                    dsSale.EnforceConstraints = false; // Disable constraints temporarily to allow updates
                    taItem.FillBy(dsSale.Item, quantityReq, barcode);

                    foreach (DataRow row in dsSale.Item.Rows)
                    {
                        taSaleItem.InsertQuery(
                            sale_No,
                            Convert.ToInt32(row["batch_No"]),
                            Convert.ToDecimal(dgvInvoice.Rows[i].Cells[5].Value),
                            1
                        );
                    }

                    int remQuantity = (int)dgvInvoice.Rows[i].Cells[6].Value - quantityReq;
                    taProduct.UpdateQuantity(remQuantity, barcode);
                    taProduct.FillByForSale(dsSale.Product);

                    saleValid = true;
                }

             
                if (saleValid == true) {
                    MessageBox.Show("Sale Completed Successfully!\nSale No: " + sale_No, "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    //saleMsg = true;
                    DialogResult res = MessageBox.Show("Do you want to print the Sale Details?", "Print Sale", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                        for (int j = 0; j < dgvProducts.Rows.Count; j++)
                        {
                            int quantityInStock = Convert.ToInt32(dgvProducts.Rows[j].Cells[8].Value);
                            int reorderLevel = Convert.ToInt32(dgvProducts.Rows[j].Cells[9].Value);
                            if (quantityInStock < reorderLevel)
                            {
                                taProduct.UpdateStatus("Low Stock", dgvProducts.Rows[j].Cells[0].Value.ToString());
                                taProduct.FillByForSale(dsSale.Product);
                            }
                            else if (quantityInStock == 0)
                            {
                                taProduct.UpdateStatus("Out Of Stock", dgvProducts.Rows[j].Cells[0].Value.ToString());
                                taProduct.FillByForSale(dsSale.Product);
                            }
                        }
                    if (res == DialogResult.Yes)
                    {
                        //saleMsg = false;
                        btnPrint.Visible = true;
                    }
                    else if (res == DialogResult.No)
                    {
                        clearInvoiceBtn.PerformClick(); // Clear the invoice after successful sale
                    }
                }
            }
            else
            {
                string errormsg = "";
                if (custID == 0)
                {
                    errormsg += "- Please ensure that you have selected a Customer.\n";
                }
                if (dgvInvoice.Rows.Count == 0)
                {
                    errormsg += "- Please ensure that you have selected Products for the Sale.\n";
                }
                if (quantitiesValid == false)
                {
                    errormsg += "- Please ensure that you have entered Quantities for all Invoice Items.\n";
                }
                if (paymentMethodCombo.Text == "")
                {
                    errormsg += "- Please ensure that you have selected a Payment Method.\n";
                }
                MessageBox.Show("Error! Sale not completed.\n" + errormsg, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }


        private void dgvInvoice_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {

        }

        private void dgvInvoice_CellValueChanged_1(object sender, DataGridViewCellEventArgs e)
        {


        }

        /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        private decimal getSubtotal()
        {
            decimal subTotal = 0;
            decimal itemPrice = 0;
            for (int i = 0; i < dgvInvoice.Rows.Count; i++)
            {
                itemPrice = Convert.ToDecimal(dgvInvoice.Rows[i].Cells[5].Value) * Convert.ToInt32(dgvInvoice.Rows[i].Cells[7].Value);
                subTotal += itemPrice;
            }

            //quantityUpDown.Value = 1; //reset quantity to 1 after calculating subtotal
            return subTotal;
        }

        //public decimal vatInclusiveAmt = 0;
        private decimal calcVat(decimal subTotal)
        {
            decimal vatAmount = 0;
            vatAmount = subTotal + (subTotal * (decimal)0.15);
            //vatAmount += subTotal;
            // vatInclusiveAmt = vatAmount; // Store the VAT amount for later use
            return vatAmount;
        }

        private int CalcPointsEarned(decimal subtotal)
        {
            int pointsEarned = 0;
            if (rbtnYes.Checked == true)
            {
                pointsEarned = (int)(subtotal / 10);
                return pointsEarned;
            }
            else
            {
                return 0;
            }

        }

        //public decimal discountAmountSale = 0;
        private decimal discountWithLoyaltyPoints()
        {
            decimal loyaltyDiscount = 0;


            if (paymentMethodCombo.Text == "Card with Loyalty Points" || paymentMethodCombo.Text == "Cash with Loyalty Points")
            {
                if (pointsAmtTxt.Text == "0")
                {
                    //MessageBox.Show("You have no loyalty points to use!");
                    return 0;
                }
                else
                {
                    loyaltyDiscount = Convert.ToDecimal(pointsAmtTxt.Text) / 10; //1 point = 10 cents
                                                                                 //  discountAmountSale = loyaltyDiscount; // Store the discount amount for later use
                    return loyaltyDiscount;
                }
            }
            else
            {
                // loyaltyDiscount = 0;
                return 0;
            }
        }

        private int pointsUsed()
        {
            if (paymentMethodCombo.Text == "Card with Loyalty Points" || paymentMethodCombo.Text == "Cash with Loyalty Points")
            {
                return Convert.ToInt32(pointsAmtTxt.Text);
            }
            else
            {
                return 0;
            }
        }

        //public decimal totalAmount = 0;
        private decimal calcTotal(decimal vatAmount, decimal loyaltyDiscount)
        {

            decimal finalTotal = 0;
            if (dgvInvoice.Rows != null) {

                if (paymentMethodCombo.Text == "Cash" || paymentMethodCombo.Text == "Card")
                {
                    finalTotal = vatAmount;
                }
                else if ((paymentMethodCombo.Text == "Cash with Loyalty Points" || paymentMethodCombo.Text == "Card with Loyalty Points") && loyaltyDiscount == 0)
                {
                    finalTotal = vatAmount;
                }
                else if ((paymentMethodCombo.Text == "Cash with Loyalty Points" || paymentMethodCombo.Text == "Card with Loyalty Points") && loyaltyDiscount > 0)
                {
                    finalTotal = vatAmount - loyaltyDiscount;
                }
                else if (paymentMethodCombo.Text == "")
                {

                    finalTotal = vatAmount;
                }
            } else {

                return 0;
            }
                // totalAmount = finalTotal; // Store the final total for later use
                return finalTotal;
        }

        /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////


        private void dgvInvoice_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (dgvInvoice.Rows.Count > 0)
            {
                descriptionTxt.Text = dgvInvoice.CurrentRow.Cells[1].Value.ToString();
                unitPriceTxt.Text = dgvInvoice.CurrentRow.Cells[5].Value.ToString();
                if (e.ColumnIndex == 8)
                {
                    decimal deletedAmount = Convert.ToDecimal(dgvInvoice.Rows[e.RowIndex].Cells[5].Value) * Convert.ToInt32(dgvInvoice.Rows[e.RowIndex].Cells[7].Value);
                    decimal deletedAmountVat = deletedAmount + (deletedAmount * 15 / 100);
                    subtotalTxt.Text = (getSubtotal() - deletedAmount).ToString("C2");
                    vatTxt.Text = (calcVat(getSubtotal()) - deletedAmountVat).ToString("C2");
                    totalTxt.Text = (calcTotal(calcVat(getSubtotal()), Decimal.Parse(DiscountTxt.Text, NumberStyles.Currency)) - deletedAmountVat).ToString("C2");
                    pointsEarnedTxt.Text = CalcPointsEarned(getSubtotal() - deletedAmount).ToString();
                    dgvInvoice.Rows.RemoveAt(e.RowIndex);

                }

            }
            if ((rbtnNo.Checked && dgvInvoice.Rows.Count > 0) || (rbtnYes.Checked && dgvInvoice.Rows.Count == 0) || (rbtnNo.Checked && dgvInvoice.Rows.Count == 0))
            {
                pointsEarnedTxt.Text = "0";
                DiscountTxt.Text = (0.00).ToString("C2");
                pointsUsedTxt.Text = "0";
                totalTxt.Text = (0.00).ToString("C2");
            }
        }

        private void dgvInvoice_CellEnter(object sender, DataGridViewCellEventArgs e)
        {

            if (dgvInvoice.CurrentRow != null)
            {
                descriptionTxt.Text = dgvInvoice.CurrentRow.Cells[1].Value.ToString();
                unitPriceTxt.Text = dgvInvoice.CurrentRow.Cells[5].Value.ToString();

                int quantityInStock = Convert.ToInt32(dgvInvoice.CurrentRow.Cells[6].Value);
                int quantityRequested = Convert.ToInt32(dgvInvoice.CurrentRow.Cells[7].Value);

                if (rbtnYes.Checked)
                {
                    DiscountTxt.Text = discountWithLoyaltyPoints().ToString("C2");
                    pointsUsedTxt.Text = pointsUsed().ToString();
                    pointsEarnedTxt.Text = CalcPointsEarned(Decimal.Parse(subtotalTxt.Text, NumberStyles.Currency)).ToString();
                }
                else
                {
                    pointsEarnedTxt.Text = "0";
                    DiscountTxt.Text = (0.00).ToString("C2");
                    pointsUsedTxt.Text = "0";
                }

                if (quantityRequested > quantityInStock)
                {
                    dgvInvoice.CurrentRow.Cells[7].Style.BackColor = Color.LightSalmon;
                    quantitiesValid = false;
                    decimal deletedAmount = Convert.ToDecimal(dgvInvoice.Rows[e.RowIndex].Cells[5].Value) * Convert.ToInt32(dgvInvoice.Rows[e.RowIndex].Cells[7].Value);
                    decimal deletedAmountVat = deletedAmount + (deletedAmount * 15 / 100);
                    subtotalTxt.Text = (getSubtotal() - deletedAmount).ToString("C2");
                    vatTxt.Text = (calcVat(getSubtotal()) - deletedAmountVat).ToString("C2");
                    totalTxt.Text = (calcTotal(calcVat(getSubtotal()), Decimal.Parse(DiscountTxt.Text, NumberStyles.Currency)) - deletedAmountVat).ToString("C2");
                    pointsEarnedTxt.Text = CalcPointsEarned(getSubtotal() - deletedAmount).ToString();
                }
                else if (quantityRequested == 0)
                {
                    dgvInvoice.CurrentRow.Cells[7].Style.BackColor = Color.LightSalmon;
                    quantitiesValid = false;
                    decimal deletedAmount = Convert.ToDecimal(dgvInvoice.Rows[e.RowIndex].Cells[5].Value) * Convert.ToInt32(dgvInvoice.Rows[e.RowIndex].Cells[7].Value);
                    decimal deletedAmountVat = deletedAmount + (deletedAmount * 15 / 100);
                    subtotalTxt.Text = (getSubtotal() - deletedAmount).ToString("C2");
                    vatTxt.Text = (calcVat(getSubtotal()) - deletedAmountVat).ToString("C2");
                    totalTxt.Text = (calcTotal(calcVat(getSubtotal()), Decimal.Parse(DiscountTxt.Text, NumberStyles.Currency)) - deletedAmountVat).ToString("C2");
                    pointsEarnedTxt.Text = CalcPointsEarned(getSubtotal() - deletedAmount).ToString();

                }
                else if (quantityRequested <= quantityInStock)
                {
                    dgvInvoice.CurrentRow.Cells[7].Style.BackColor = Color.LightGreen;
                    quantitiesValid = true;
                    subtotalTxt.Text = getSubtotal().ToString("C2");
                    vatTxt.Text = calcVat(Decimal.Parse(subtotalTxt.Text, NumberStyles.Currency)).ToString("C2");
                    totalTxt.Text = calcTotal(Decimal.Parse(vatTxt.Text, NumberStyles.Currency), Decimal.Parse(DiscountTxt.Text, NumberStyles.Currency)).ToString("C2");

                }



            }
        }

        private void dgvInvoice_CellValueChanged_2(object sender, DataGridViewCellEventArgs e)
        {
            if (dgvInvoice.CurrentRow != null)
            {
                descriptionTxt.Text = dgvInvoice.CurrentRow.Cells[1].Value.ToString();
                unitPriceTxt.Text = dgvInvoice.CurrentRow.Cells[5].Value.ToString();

                int quantityInStock = Convert.ToInt32(dgvInvoice.CurrentRow.Cells[6].Value);
                int quantityRequested = Convert.ToInt32(dgvInvoice.CurrentRow.Cells[7].Value);

                if (rbtnYes.Checked)
                {
                    DiscountTxt.Text = discountWithLoyaltyPoints().ToString("C2");
                    pointsUsedTxt.Text = pointsUsed().ToString();
                    pointsEarnedTxt.Text = CalcPointsEarned(Decimal.Parse(subtotalTxt.Text, NumberStyles.Currency)).ToString();
                }
                else
                {
                    pointsEarnedTxt.Text = "0";
                    DiscountTxt.Text = (0.00).ToString("C2");
                    pointsUsedTxt.Text = "0";
                }

                if (quantityRequested > quantityInStock)
                {
                    dgvInvoice.CurrentRow.Cells[7].Style.BackColor = Color.LightSalmon;
                    quantitiesValid = false;
                    decimal deletedAmount = Convert.ToDecimal(dgvInvoice.Rows[e.RowIndex].Cells[5].Value) * Convert.ToInt32(dgvInvoice.Rows[e.RowIndex].Cells[7].Value);
                    decimal deletedAmountVat = deletedAmount + (deletedAmount * 15/100);
                    subtotalTxt.Text = (getSubtotal() - deletedAmount).ToString("C2");
                    vatTxt.Text = (calcVat(getSubtotal()) - deletedAmountVat).ToString("C2");
                    totalTxt.Text = (calcTotal(calcVat(getSubtotal()), Decimal.Parse(DiscountTxt.Text, NumberStyles.Currency)) - deletedAmountVat).ToString("C2");
                    pointsEarnedTxt.Text = CalcPointsEarned(getSubtotal()-deletedAmount).ToString();
                }
                else if (quantityRequested == 0)
                {
                    dgvInvoice.CurrentRow.Cells[7].Style.BackColor = Color.LightSalmon;
                    quantitiesValid = false;
                    decimal deletedAmount = Convert.ToDecimal(dgvInvoice.Rows[e.RowIndex].Cells[5].Value) * Convert.ToInt32(dgvInvoice.Rows[e.RowIndex].Cells[7].Value);
                    subtotalTxt.Text = (getSubtotal() - deletedAmount).ToString("C2");
                    vatTxt.Text = (calcVat(getSubtotal()) - deletedAmount).ToString("C2");
                    totalTxt.Text = (calcTotal(calcVat(getSubtotal()), Decimal.Parse(DiscountTxt.Text, NumberStyles.Currency)) - deletedAmount).ToString("C2");
                    pointsEarnedTxt.Text = CalcPointsEarned(getSubtotal()).ToString();
                }
                else if (quantityRequested <= quantityInStock)
                {
                    dgvInvoice.CurrentRow.Cells[7].Style.BackColor = Color.LightGreen;
                    quantitiesValid = true;
                    subtotalTxt.Text = getSubtotal().ToString("C2");
                    vatTxt.Text = calcVat(Decimal.Parse(subtotalTxt.Text, NumberStyles.Currency)).ToString("C2");
                    totalTxt.Text = calcTotal(Decimal.Parse(vatTxt.Text, NumberStyles.Currency), Decimal.Parse(DiscountTxt.Text, NumberStyles.Currency)).ToString("C2");
                  
                }


            }
            if (dgvInvoice.Rows == null) {
                quantitiesValid = false;
            }
        }

        private void dgvInvoice_ColumnSortModeChanged(object sender, DataGridViewColumnEventArgs e)
        {

            if (dgvInvoice.CurrentRow != null)
            {

                int quantityInStock = Convert.ToInt32(dgvInvoice.CurrentRow.Cells[6].Value);
                int quantityRequested = Convert.ToInt32(dgvInvoice.CurrentRow.Cells[7].Value);

                if (quantityRequested > quantityInStock)
                {
                    dgvInvoice.CurrentRow.Cells[7].Style.BackColor = Color.LightSalmon;
                    quantitiesValid = false;
                }
                else if (quantityRequested == 0)
                {
                    dgvInvoice.CurrentRow.Cells[7].Style.BackColor = Color.LightSalmon;
                    quantitiesValid = false;
                }
                else if (quantityRequested <= quantityInStock)
                {
                    dgvInvoice.CurrentRow.Cells[7].Style.BackColor = Color.LightGreen;
                    quantitiesValid = true;
                }
            }
        }

        private void updateItemBtn_Click(object sender, EventArgs e)
        {

        }

        private void clearInvoiceBtn_Click(object sender, EventArgs e)
        {
            int i=dgvInvoice.Rows.Count - 1;
            while (dgvInvoice.Rows.Count > 0)
            {
                dgvInvoice.Rows.RemoveAt(i);
                i--;
            }
            descriptionTxt.Text = "";
            unitPriceTxt.Text = "";
            paymentMethodCombo.SelectedIndex = -1;
            taProduct.FillByForSale(dsSale.Product);
            taCustomer.Fill(dsSale.Customer);
            dgvProducts.ClearSelection();
        }

        private void dgvInvoice_DataError(object sender, DataGridViewDataErrorEventArgs e)
        {
            if (e.Exception is FormatException)
            {
                MessageBox.Show("Invalid Quantity entered. Please enter a valid number.", "Input Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                e.ThrowException = false; // Prevent the exception from propagating
            }
            else
            {
                MessageBox.Show("An unexpected error occurred: " + e.Exception.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                e.ThrowException = true; // Allow other exceptions to propagate
            }
        }

        private void backBtn_Click(object sender, EventArgs e)
        {

        }

        private void txtCustIDorName_TextChanged(object sender, EventArgs e)
        {
            try
            {
                if (txtCustomerIDSearch.Text != "")
                {
                    taCustomer.FilterByID(dsSale.Customer, Convert.ToInt32(txtCustomerIDSearch.Text));
                }
                else
                {
                    taCustomer.Fill(dsSale.Customer);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error searching for customer: " + ex.Message, "Search Error", MessageBoxButtons.OK, MessageBoxIcon.Error);

            }
        }

        public int custID = 0;
        public DateTime custRegDate;
        private void dgvCustomer_RowHeaderMouseDoubleClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            if (dgvCustomer.Rows == null)
            {
                groupBox1.Text = "Customer Details";
                nameTxt.Text = "";
                surnameTxt.Text = "";
                txtCustomerType.Text = "";
                custTypeComboBox.Text = "";
                businessNameTxt.Text = "";
                telNumTxt.Text = "";
                emailTxt.Text = "";
                streetTxt.Text = "";
                suburbTxt.Text = "";
                cityTxt.Text = "";
                txtProvince.Text = "";
                provinceComboBox.Text = "";
                postalCodetxt.Text = "";
                pointsAmtTxt.Text = "0";
                rbtnNo.Checked = true;
                rbtnYes.Checked = false;
                //  groupBox3.Enabled = false;
                groupBox4.Enabled = false;
                groupBox5.Enabled = false;
                descriptionTxt.Enabled = false;
                unitPriceTxt.Enabled = false;
                clearInvoiceBtn.Enabled = false;
                button2.Enabled = false;
                paymentMethodCombo.Enabled = false;
                groupBox2.Enabled = false;
            }
            else if (dgvCustomer.Rows != null)
            {
                var row = dgvCustomer.CurrentRow.Cells;
                groupBox1.Text = "Customer Details [" + row[0].Value.ToString() + "]";
                custID = Convert.ToInt32(row[0].Value);
                nameTxt.Text = row[1].Value.ToString();
                surnameTxt.Text = row[2].Value.ToString();
                txtCustomerType.Text = row[3].Value.ToString();
                custTypeComboBox.Text = row[3].Value.ToString(); // Set the combobox to the current customer type
                businessNameTxt.Text = row[4].Value.ToString();
                telNumTxt.Text = row[5].Value.ToString();
                emailTxt.Text = row[6].Value.ToString();
                streetTxt.Text = row[7].Value.ToString();
                suburbTxt.Text = row[8].Value.ToString();
                cityTxt.Text = row[9].Value.ToString();
                txtProvince.Text = row[10].Value.ToString();
                provinceComboBox.Text = row[10].Value.ToString(); // Set the combobox to the current province
                postalCodetxt.Text = row[11].Value.ToString();
                custRegDate = Convert.ToDateTime(row[14].Value);
                if (row[12].Value.ToString() == "True")
                {
                    rbtnYes.Checked = true;

                }
                else
                {
                    rbtnNo.Checked = true;

                }
                //ONLY DISPLAY POINTS THAT ARENT EXPIRED:
                DateTime currentTime = DateTime.Now;

                int year = currentTime.Year;
                int month = currentTime.Month;
                int pointsAmt = 0;


                foreach (DataRow row1 in dsSale.Sale.Rows)
                {
                    //MessageBox.Show(row1[0].ToString());
                    string customerID = row1[1].ToString();

                   // MessageBox.Show("in Loop");
                    if (customerID == custID.ToString())
                    {
                       // MessageBox.Show(row1[1].ToString() + "==" + custID.ToString());
                        DateTime saleDate = Convert.ToDateTime(row1[10]);

                        int yearSale = saleDate.Year;
                        int monthSale = saleDate.Month;
                       // MessageBox.Show(yearSale.ToString() + " " + monthSale.ToString());
                        if ((year == yearSale) && ((month - monthSale) < 6))
                        {
                            pointsAmt += Convert.ToInt32(row1[6]);
                          //  MessageBox.Show(pointsAmt.ToString());
                        }


                    }
                }

                paymentMethodCombo.SelectedIndex = -1;

                pointsAmtTxt.Text = pointsAmt.ToString();
                groupBox4.Enabled = true;
                groupBox5.Enabled = true;
                descriptionTxt.Enabled = true;
                unitPriceTxt.Enabled = true;
                clearInvoiceBtn.Enabled = true;
                button2.Enabled = true;
                paymentMethodCombo.Enabled = true;
                groupBox2.Enabled = true;

            }
        }

        private void txtCustomerNameSearch_TextChanged(object sender, EventArgs e)
        {
            if (txtCustomerNameSearch.Text != "")
            {
                taCustomer.FilterByFullName(dsSale.Customer, txtCustomerNameSearch.Text);
            }
            else
            {
                taCustomer.Fill(dsSale.Customer);
            }
        }

        private void dgvInvoice_RowHeaderMouseDoubleClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            descriptionTxt.Text = dgvInvoice.CurrentRow.Cells[1].Value.ToString();
            unitPriceTxt.Text = dgvInvoice.CurrentRow.Cells[5].Value.ToString();
        }

        private void paymentMethodCombo_SelectedIndexChanged(object sender, EventArgs e)
        {

            if (rbtnYes.Checked && dgvInvoice.Rows.Count > 0)
            {
                DiscountTxt.Text = discountWithLoyaltyPoints().ToString("C2");
                pointsUsedTxt.Text = pointsUsed().ToString();
                pointsEarnedTxt.Text = CalcPointsEarned(Decimal.Parse(subtotalTxt.Text, NumberStyles.Currency)).ToString();

            }
            else if ((rbtnNo.Checked && dgvInvoice.Rows.Count> 0) || (rbtnYes.Checked && dgvInvoice.Rows.Count == 0) || (rbtnNo.Checked && dgvInvoice.Rows.Count == 0))
            {
                pointsEarnedTxt.Text = "0";
                DiscountTxt.Text = (0.00).ToString("C2");
                pointsUsedTxt.Text = "0";
            } 

            if (quantitiesValid)
            {
                subtotalTxt.Text = getSubtotal().ToString("C2");
                vatTxt.Text = calcVat(Decimal.Parse(subtotalTxt.Text, NumberStyles.Currency)).ToString("C2");

                totalTxt.Text = calcTotal(Decimal.Parse(vatTxt.Text, NumberStyles.Currency), Decimal.Parse(DiscountTxt.Text, NumberStyles.Currency)).ToString("C2");
            }
            
            }

        private void dgvInvoice_RowsRemoved(object sender, DataGridViewRowsRemovedEventArgs e)
        {
            
        }

        private void dgvProducts_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }

        private void emailTxt_TextChanged(object sender, EventArgs e)
        {

        }

        private void suburbTxt_TextChanged(object sender, EventArgs e)
        {

        }

        private void dgvCustomer_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }

        private int rowIndex = 0;
        private List<int> scaledColumnWidths = new List<int>();
        private void btnPrint_Click(object sender, EventArgs e)
        {
            rowIndex = 0;
            scaledColumnWidths.Clear();
            printPreviewDialog1.Document = printDocument1;
            printPreviewDialog1.ShowDialog();
        }

        private void printDocument1_PrintPage(object sender, PrintPageEventArgs e)
        {
            int leftMargin = e.MarginBounds.Left;
            int topMargin = e.MarginBounds.Top;
            int rightMargin = e.MarginBounds.Right;
            int printableWidth = e.MarginBounds.Width;
            int bottomMargin = e.MarginBounds.Bottom;

            Font headerFont = new Font("Arial", 16, FontStyle.Bold);
            Font cellFont = new Font("Arial", 9);
            Font footerFont = new Font("Arial", 14, FontStyle.Bold);
            Brush brush = Brushes.Black;

            int y = topMargin;


           

            // Print Header
            string headerText = "Sales Report - " + DateTime.Now.ToString("MMMM yyyy")+"\nCustomer: "+nameTxt.Text+" "+surnameTxt.Text+" ["+custID.ToString()+"]"+"\nSales Staff: "+Globals.staffPK.ToString()+"\nSale Number: "+sale_No.ToString()+"\n\n Products:";
            e.Graphics.DrawString(headerText, headerFont, brush, leftMargin, y);
            y += (int)e.Graphics.MeasureString(headerText, headerFont).Height + 20;

            int rowHeight = 30;

            // Calculate total DataGridView width
            int totalColumnWidth = 0;
            foreach (DataGridViewColumn column in dgvInvoice.Columns)
            {
                if (column.Visible)
                    totalColumnWidth += column.Width;
            }

            // Scale widths to fit page
            if (scaledColumnWidths.Count == 0)
            {
                foreach (DataGridViewColumn column in dgvInvoice.Columns)
                {
                    if (column.Visible)
                    {
                        float widthRatio = (float)column.Width / totalColumnWidth;
                        int scaledWidth = (int)(widthRatio * printableWidth);
                        scaledColumnWidths.Add(scaledWidth);
                    }
                }
            }

            // Draw Column Headers
            int colX = leftMargin;
            int colIndex = 0;
            foreach (DataGridViewColumn column in dgvInvoice.Columns)
            {
                if (!column.Visible) continue;

                int colWidth = scaledColumnWidths[colIndex];

                e.Graphics.FillRectangle(Brushes.LightGray, new Rectangle(colX, y, colWidth, rowHeight));
                e.Graphics.DrawRectangle(Pens.Black, new Rectangle(colX, y, colWidth, rowHeight));
                e.Graphics.DrawString(column.HeaderText, cellFont, brush, new RectangleF(colX, y, colWidth, rowHeight));

                colX += colWidth;
                colIndex++;
            }

            y += rowHeight;

            // Draw DataGridView Rows
            while (rowIndex < dgvInvoice.Rows.Count)
            {
                DataGridViewRow row = dgvInvoice.Rows[rowIndex];
                if (row.IsNewRow) break;

                if (y + rowHeight > bottomMargin)
                {
                    e.HasMorePages = true;
                    return;
                }

                colX = leftMargin;
                colIndex = 0;
                foreach (DataGridViewCell cell in row.Cells)
                {
                    if (!cell.OwningColumn.Visible) continue;

                    int colWidth = scaledColumnWidths[colIndex];
                    e.Graphics.DrawRectangle(Pens.Black, new Rectangle(colX, y, colWidth, rowHeight));
                    e.Graphics.DrawString(Convert.ToString(cell.Value), cellFont, brush, new RectangleF(colX, y, colWidth, rowHeight));

                    colX += colWidth;
                    colIndex++;
                }

                y += rowHeight;
                rowIndex++;
            }

            // Footer
            string footerText = "Payment Method: "+paymentMethodCombo.Text + "\tTotalAmount ="+totalTxt.Text+"\n\nPrinted on: " + DateTime.Now.ToString("dd MMM yyyy hh:mm tt");
            e.Graphics.DrawString(footerText, footerFont, brush, leftMargin, bottomMargin + 20);

            e.HasMorePages = false;

            clearInvoiceBtn.PerformClick();
            btnPrint.Visible = false;
        }
    }
}
