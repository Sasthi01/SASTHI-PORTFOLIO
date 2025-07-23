using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace LGPACKAGING_POS_SYSTEM.Business
{
    public partial class frmInventory : Form
    {
        public frmInventory()
        {
            InitializeComponent();
            this.taCategory.Fill(this.DSGroup.Category);
            taProduct.Fill(this.DSGroup.Product);
            panel1.Parent = pictureBox2;
            panel2.Parent = pictureBox2;
            panel3.Parent = pictureBox2;

        }
        private void SearchProducts()
        {
            if (!string.IsNullOrWhiteSpace(txtDescription.Text) && !string.IsNullOrWhiteSpace(txtColour.Text) && !string.IsNullOrWhiteSpace(txtCatID.Text))
            {
                // Search by Description, Colour & Category ID
                taProduct.FillByDescrip_Colour_ID(DSGroup.Product, txtDescription.Text, txtColour.Text, Convert.ToInt32(txtCatID.Text));
            }
            else if (!string.IsNullOrWhiteSpace(txtDescription.Text) && !string.IsNullOrWhiteSpace(txtColour.Text))
            {
                // Search by Description & Colour
                taProduct.FillByDescrip_Colour(DSGroup.Product, txtDescription.Text, txtColour.Text);
            }
            else if (!string.IsNullOrWhiteSpace(txtColour.Text) && !string.IsNullOrWhiteSpace(txtCatID.Text))
            {
                // Search by Colour & Category ID
                taProduct.FillByColour_ID(DSGroup.Product, txtColour.Text, Convert.ToInt32(txtCatID.Text));
            }
            else if (!string.IsNullOrWhiteSpace(txtDescription.Text) && !string.IsNullOrWhiteSpace(txtCatID.Text))
            {
                // Search by Description & Category ID
                taProduct.FillByDescrip_ID(DSGroup.Product, txtDescription.Text, Convert.ToInt32(txtCatID.Text));
            }
            else if (!string.IsNullOrWhiteSpace(txtDescription.Text))
            {
                // Search by Description
                taProduct.FillByProdDescription(DSGroup.Product, txtDescription.Text);
            }
            else if (!string.IsNullOrWhiteSpace(txtColour.Text))
            {
                // Search by Colour
                taProduct.FillByProdColour(DSGroup.Product, txtColour.Text);
            }
            else if (!string.IsNullOrWhiteSpace(txtCatID.Text))
            {
                // Search by Category ID
                taProduct.FillByProdCatID(DSGroup.Product, Convert.ToInt32(txtCatID.Text));
            }
            else if (string.IsNullOrWhiteSpace(txtDescription.Text) && string.IsNullOrWhiteSpace(txtCatID.Text) && string.IsNullOrWhiteSpace(txtColour.Text)) { 
                taProduct.Fill(this.DSGroup.Product);
            }
        }


        private void frmInventory_Load(object sender, EventArgs e)
        {

        }

        private void txtDescription_TextChanged_1(object sender, EventArgs e)
        {
            SearchProducts();
        }

        private void txtColour_TextChanged(object sender, EventArgs e)
        {
            SearchProducts();
        }

        private void txtCatID_TextChanged_1(object sender, EventArgs e)
        {
            SearchProducts();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            txtCatID.Clear();
            txtDescription.Clear();
            txtColour.Clear();
        }

        private void btnClearProdD_Click_1(object sender, EventArgs e)
        {
            txtBarcode.Clear();
            txtCatID.Clear();
            rtxtDescrip.Clear();
            txtProdSize.Clear();
            txtColour1.Clear();
            cmbCategoryProd.SelectedIndex = -1;
            nudUnitsPerBale.Value = 0;
            nudCostPrice.Value = 0; ;
            nudSellingPrice.Value = 0;
            nudQuantity.Value = 0;
            ReorderLevel.Value = 0;
            txtStatusProd.Clear();
            pictureBox1.ImageLocation = "";
            txtRepRequest.Clear();
        }

        private void btnAddProd_Click_1(object sender, EventArgs e)
        {
            string error = "";
            bool er = false;


            if (string.IsNullOrWhiteSpace(txtBarcode.Text) || cmbCategoryProd.SelectedValue == null ||
                string.IsNullOrWhiteSpace(rtxtDescrip.Text) || string.IsNullOrWhiteSpace(txtProdSize.Text) ||
                string.IsNullOrWhiteSpace(txtColour1.Text) || string.IsNullOrWhiteSpace(txtStatusProd.Text))
            {
                error += "Please fill in all fields.\n";
                er = true;
            }

            //description length validation
            if (rtxtDescrip.Text.Length > 50)
            {
                error += "Description cannot exceed 50 characters.\n";
                er = true;
            }

            //size length validation
            if (txtProdSize.Text.Length > 50)
            {
                error += "Product Size cannot exceed 50 characters.\n";
                er = true;
            }

            //colour length validation
            if (txtColour1.Text.Length > 10)
            {
                error += "Colour cannot exceed 10 characters.\n";
                er = true;
            }

            //unnits per bale validation
            if (nudUnitsPerBale.Value <= 0)
            {
                error += "Units per bale must be greater than 0.\n";
                er = true;
            }

            //cost price validation
            if (nudCostPrice.Value < 0 || nudCostPrice.Value > 999999.99m)
            {
                error += "Cost price must be between 0 and 999,999.99.\n";
                er = true;
            }

            //selling price validation
            if (nudSellingPrice.Value < 0 || nudSellingPrice.Value > 999999.99m)
            {
                error += "Selling price must be between 0 and 999,999.99.\n";
                er = true;
            }

            if (nudSellingPrice.Value < nudCostPrice.Value)
            {
                DialogResult result = MessageBox.Show("Selling price is less than cost price. Do you want to continue?", "Warning", MessageBoxButtons.YesNo);
                if (result == DialogResult.No)
                    return;
            }

            //quantity validation
            if (nudQuantity.Value <= 0)
            {
                error += "Quantity cannot be 0.\n";
                er = true;
            }

            //reorder level validation
            if (ReorderLevel.Value <= 0)
            {
                error += "Reorder level cannot be 0.\n";
                er = true;
            }

            //status validation
            if (txtStatusProd.Text.Length > 50)
            {
                error += "Product status cannot exceed 50 characters.\n";
                er = true;
            }

            // Final error check
            if (er)
            {
                MessageBox.Show(error, "Validation Errors", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            try
            {
                taProduct.InsertNewProd(txtBarcode.Text, Convert.ToInt32(cmbCategoryProd.SelectedValue), rtxtDescrip.Text, txtProdSize.Text, txtColour1.Text, Convert.ToInt32(nudUnitsPerBale.Value), Convert.ToDecimal(nudCostPrice.Value), Convert.ToDecimal(nudSellingPrice.Value), Convert.ToInt32(nudQuantity.Value), Convert.ToInt32(ReorderLevel.Value), txtStatusProd.Text, null);
                MessageBox.Show("Product Added Successfully");
                taProduct.Fill(this.DSGroup.Product);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error adding Product:\n" + ex.Message);
            }
        }

        private void btnUpdateProd_Click_1(object sender, EventArgs e)
        {
            string error = "";
            bool hasError = false;

            if (dataGridView1.CurrentRow == null || dataGridView1.CurrentRow.Cells[0].Value == null)
            {
                MessageBox.Show("No product selected for update.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (string.IsNullOrWhiteSpace(txtBarcode.Text) || cmbCategoryProd.SelectedValue == null ||
                string.IsNullOrWhiteSpace(rtxtDescrip.Text) || string.IsNullOrWhiteSpace(txtProdSize.Text) ||
                string.IsNullOrWhiteSpace(txtColour1.Text) || string.IsNullOrWhiteSpace(txtStatusProd.Text))
            {
                error += "Please Fill In All Fields.\n";
                hasError = true;
            }

            if (rtxtDescrip.Text.Length > 50)
            {
                error += "Description must be 50 characters or fewer.\n";
                hasError = true;
            }
            if (txtProdSize.Text.Length > 50)
            {
                error += "Size must be 50 characters or fewer.\n";
                hasError = true;
            }
            if (txtColour1.Text.Length > 10)
            {
                error += "Colour must be 10 characters or fewer.\n";
                hasError = true;
            }
            if (nudUnitsPerBale.Value <= 0)
            {
                error += "Units per bale must be greater than 0.\n";
                hasError = true;
            }
            if (nudCostPrice.Value < 0 || nudCostPrice.Value > 999999.99m)
            {
                error += "Cost price must be between 0 and 999,999.99.\n";
                hasError = true;
            }
            if (nudSellingPrice.Value <= 0 || nudSellingPrice.Value > 999999.99m)
            {
                error += "Selling price must be between 0 and 999,999.99.\n";
                hasError = true;
            }
            if (nudSellingPrice.Value < nudCostPrice.Value)
            {
                DialogResult result = MessageBox.Show("Selling price is less than cost price. Continue?", "Warning", MessageBoxButtons.YesNo);
                if (result == DialogResult.No)
                    return;
            }
            if (nudQuantity.Value <= 0)
            {
                error += "Quantity cannot be 0.\n";
                hasError = true;
            }
            if (ReorderLevel.Value <= 0)
            {
                error += "Reorder level cannot be 0.\n";
                hasError = true;
            }

            if (txtStatusProd.Text.Length > 50)
            {
                error += "Status must be 50 characters or fewer.\n";
                hasError = true;
            }

            if (txtStatusProd.Text.Trim() != "Available" && txtStatusProd.Text.Trim() != "In Stock" && txtStatusProd.Text.Trim() != "Out of Stock")
            {
                error += "Status must be either 'Available' or 'In Stock' or 'Out of Stock'.\n";
                hasError = true;
            }

            if (hasError)
            {
                MessageBox.Show(error, "Validation Errors", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            try
            {
                // Capture the barcode of the selected product
                string selectedBarcode = txtBarcode.Text;

                // Update product details
                taProduct.UpdateProd(Convert.ToInt32(cmbCategoryProd.SelectedValue), rtxtDescrip.Text, txtProdSize.Text, txtColour1.Text,
                                        Convert.ToInt32(nudUnitsPerBale.Value), Convert.ToDecimal(nudCostPrice.Value),
                                        Convert.ToDecimal(nudSellingPrice.Value), Convert.ToInt32(nudQuantity.Value),
                                        Convert.ToInt32(ReorderLevel.Value), txtStatusProd.Text, null, selectedBarcode);

                MessageBox.Show("Product Updated Successfully");

                // Refresh DataGridView and filter it to only display the updated product
                taProduct.Fill(DSGroup.Product);
                dataGridView1.DataSource = DSGroup.Product;
                // Loop through rows and find the updated product
                foreach (DataGridViewRow row in dataGridView1.Rows)
                {
                    if (row.Cells[0].Value != null && row.Cells[0].Value.ToString() == selectedBarcode)
                    {
                        // Select the row and bring it into view
                        row.Selected = true;
                        dataGridView1.FirstDisplayedScrollingRowIndex = row.Index;
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error updating Product\n" + ex.Message);
            }
        }

        private void btnDiscont_Click_1(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtBarcode.Text))
            {
                MessageBox.Show("Please select a product to discontinue.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (txtStatusProd.Text == "Discontinued")
            {
                MessageBox.Show("This product is already discontinued.");
                return;
            }

            DialogResult result = MessageBox.Show("Are you sure you want to mark this product as Discontinued?", "Confirm", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (result == DialogResult.No)
                return;


            txtStatusProd.Text = "Discontinued";
            try
            {
                taProduct.UpdateProd(Convert.ToInt32(cmbCategoryProd.SelectedValue), rtxtDescrip.Text, txtProdSize.Text, txtColour1.Text, Convert.ToInt32(nudUnitsPerBale.Value), Convert.ToDecimal(nudCostPrice.Value), Convert.ToDecimal(nudSellingPrice.Value), Convert.ToInt32(nudQuantity.Value), Convert.ToInt32(ReorderLevel.Value), txtStatusProd.Text, null, txtBarcode.Text);
                MessageBox.Show("Product Discontinued");
                taProduct.Fill(DSGroup.Product);
            }
            catch (Exception)
            {
                MessageBox.Show("Error Discontinuing Product");
            }
        }

        private void txtRepRequest_TextChanged(object sender, EventArgs e)
        {

        }

        private void txtRepRequest_MouseClick_1(object sender, MouseEventArgs e)
        {
            MessageBox.Show("This field is read-only.", "Notice", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void dataGridView1_RowHeaderMouseDoubleClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            if (dataGridView1.CurrentRow.Cells != null)
            {
                txtBarcode.Text = dataGridView1.CurrentRow.Cells[0].Value.ToString();
                cmbCategoryProd.SelectedValue = (int)dataGridView1.CurrentRow.Cells[1].Value;/////////////
                rtxtDescrip.Text = dataGridView1.CurrentRow.Cells[2].Value.ToString() != null ? dataGridView1.CurrentRow.Cells[2].Value.ToString() : "";
                txtProdSize.Text = dataGridView1.CurrentRow.Cells[3].Value.ToString();
                txtColour1.Text = dataGridView1.CurrentRow.Cells[4].Value.ToString();
                nudUnitsPerBale.Value = (int)dataGridView1.CurrentRow.Cells[5].Value;
                nudCostPrice.Value = Convert.ToDecimal(dataGridView1.CurrentRow.Cells[6].Value);
                nudSellingPrice.Value = Convert.ToDecimal(dataGridView1.CurrentRow.Cells[7].Value);
                nudQuantity.Value = (int)dataGridView1.CurrentRow.Cells[8].Value;
                ReorderLevel.Value = Convert.ToInt32(dataGridView1.CurrentRow.Cells[9].Value);
                txtStatusProd.Text = dataGridView1.CurrentRow.Cells[10].Value.ToString();
                txtRepRequest.Text = dataGridView1.CurrentRow.Cells[12].Value.ToString();

                string relativeImagePath = dataGridView1.CurrentRow.Cells[11].Value.ToString() + ".png"; // e.g., "Images/product1.jpg"
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
            }
            else
            {
                txtBarcode.Text = "";
                cmbCategoryProd.SelectedValue = -1;
                rtxtDescrip.Text = "";
                txtProdSize.Text = "";
                txtColour1.Text = "";
                nudUnitsPerBale.Value = 0;
                nudCostPrice.Value = 0;
                nudSellingPrice.Value = 0;
                nudQuantity.Value = 0;
                ReorderLevel.Value = 0;
                txtStatusProd.Text = "";
                pictureBox1.ImageLocation = "";
                txtRepRequest.Text = "";

            }
        }

        private void btnAddCat_Click(object sender, EventArgs e)
        {
            try
            {
                taCategory.InsertNewCat(txtNewCatName.Text, richTextDescription.Text, txtStatusCat.Text);
                MessageBox.Show("Category Added Successfully");
                taCategory.Fill(this.DSGroup.Category);
            }
            catch (Exception)
            {
                MessageBox.Show("Error adding Category");
            }
        }

        private void btnUpdateCat_Click(object sender, EventArgs e)
        {
            string error = "";
            bool hasError = false;
            if (dataGridView2.CurrentRow == null || dataGridView2.CurrentRow.Cells[0].Value == null)
            {
                MessageBox.Show("No category selected for update.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (string.IsNullOrWhiteSpace(txtNewCatName.Text) || string.IsNullOrWhiteSpace(richTextDescription.Text) ||
                string.IsNullOrWhiteSpace(txtStatusCat.Text))
            {
                error += "Please fill in all fields.\n";
                hasError = true;
            }

            if (comboCategoryType2.SelectedItem == null || string.IsNullOrWhiteSpace(comboCategoryType2.Text))
            {
                error += "Please select a Category Type.\n";
                hasError = true;
            }

            //status validation...COMPLETE LATER
            if (txtStatusCat.Text.Trim() != "Available" && txtStatusCat.Text.Trim() != "Unavailable")
            {
                error += "Status must be either 'Available' or 'Unavailable'.\n";
                hasError = true;
            }

            if (hasError)
            {
                MessageBox.Show(error, "Validation Errors", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            try
            {
                taCategory.UpdateCat(comboCategoryType2.Text, richTextDescription.Text, txtStatusCat.Text, Convert.ToInt32(dataGridView2.CurrentRow.Cells[0].Value));
                MessageBox.Show("Category Updated Successfully");
                taCategory.Fill(this.DSGroup.Category);
            }
            catch (Exception)
            {
                MessageBox.Show("Error updating Category");
            }
        }

        private void dataGridView2_RowHeaderMouseDoubleClick_1(object sender, DataGridViewCellMouseEventArgs e)
        {
            txtNewCatName.Text = dataGridView2.CurrentRow.Cells[1].Value.ToString();
            richTextDescription.Text = dataGridView2.CurrentRow.Cells[2].Value.ToString();
            txtStatusCat.Text = dataGridView2.CurrentRow.Cells[3].Value.ToString();
        }

        private void btnArchiveCat_Click_1(object sender, EventArgs e)
        {
            if (dataGridView2.CurrentRow == null)
            {
                MessageBox.Show("Please select a category to archive.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string currentStatus = dataGridView2.CurrentRow.Cells["status"].Value.ToString(); 
            if (currentStatus == "Archived")
            {
                MessageBox.Show("This category is already archived.");
                return;
            }

            DialogResult result = MessageBox.Show("Are you sure you want to archive this category?", "Confirm Archive", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (result == DialogResult.No)
                return;

            try
            {
                taCategory.UpdateStatusArchived("Archived", Convert.ToInt32(dataGridView2.CurrentRow.Cells[0].Value));
                MessageBox.Show("Category Archived Successfully");
                taCategory.Fill(this.DSGroup.Category);
            }
            catch (Exception)
            {
                MessageBox.Show("Error archiving Category");
            }
        }

        private void btnClearCat_Click_1(object sender, EventArgs e)
        {
            comboCategoryType2.SelectedValue = -1;
            txtNewCatName.Clear();
            richTextDescription.Clear();
            txtStatusCat.Clear();
        }
    }
}

