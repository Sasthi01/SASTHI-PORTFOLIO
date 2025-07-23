using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics.Eventing.Reader;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static LGPACKAGING_POS_SYSTEM.Interface.frmLogin;

namespace LGPACKAGING_POS_SYSTEM.Business
{
    public partial class frmAdjustments: Form
    {
        public frmAdjustments()
        {
            InitializeComponent();
            panel1.Parent = pictureBox1;
            panel2.Parent = pictureBox1;
            pnlAdjustment.Parent = pictureBox1;
            pnlItems.Parent = pictureBox1;
        }

        private void frmAdjustments_Load(object sender, EventArgs e)
        {
            // TODO: This line of code loads data into the 'wstGrp21DataSet.Product' table. You can move, or remove it, as needed.
            this.taProduct.Fill(this.dsProducts.Product);
            taItems.FilterByAvailable(dsAdjustments.Item);
            taAdjustments.Fill(dsAdjustments.Adjustment);

        }

        private void button7_Click(object sender, EventArgs e)
        {

        }

        private void txtDescriptionItems_TextChanged(object sender, EventArgs e)
        {
            
        }

        private void lstProdDescriptions_SelectedIndexChanged(object sender, EventArgs e)
        {
            //taProduct.FilterByDescription(dsProducts.Product, lstProdDescriptions.Text);

            if (lstProdDescriptions.SelectedIndex > 0)
            {

                string barcode = lstProdDescriptions.SelectedValue.ToString(); ;
                taItems.FilterByBarcode(dsAdjustments.Item, barcode);
                txtBarcode.Text = barcode;
                lblItems.Text = "PRODUCT DETAILS OF "+lstProdDescriptions.Text;
                lblAdjustment.Text = "ADJUSTMENT ON " + barcode;

            }
            
        }

        private void txtDescriptionItems_TextChanged_1(object sender, EventArgs e)
        {
            taProduct.FilterByDescription(dsProducts.Product, txtDescriptionItems.Text);
        }

        private void txtBarcode_TextChanged(object sender, EventArgs e)
        {
            int quantity = Convert.ToInt32(taItems.CountBarcodes(txtBarcode.Text));
            lblTotQuantity.Text = quantity.ToString();
            int quantityAdj = Convert.ToInt32(taItems.CountBarcodeAdjusted(txtBarcode.Text));
            lblQuantityAdjusted.Text = quantityAdj.ToString();

        }

        private void btnInsertItems_Click(object sender, EventArgs e)
        {
            taItems.Insert(txtBarcode.Text, "Available");
            taProduct.UpdateQuantityInStock_Increment(txtBarcode.Text);
            int reorderLevel = Convert.ToInt32(taProduct.GetReorderLevel(txtBarcode.Text));

            int quantity = Convert.ToInt32(taItems.CountBarcodes(txtBarcode.Text));
            lblTotQuantity.Text = quantity.ToString();

            if (quantity >= reorderLevel)
            {
                taProduct.UpdateStatus("In Stock", txtBarcode.Text);
            }
            else if (quantity< reorderLevel)
            {
                taProduct.UpdateStatus("Low Stock", txtBarcode.Text);
            }
            else if (quantity == 0)
            {
                taProduct.UpdateStatus("Out Of Stock", txtBarcode.Text);
            }
        }

        private void btnArchiveItems_Click(object sender, EventArgs e)
        {
            var cellValue = dgvItems.CurrentRow.Cells[2].Value;
            //CMBSTATUS = ARCHIVED:
            if(cellValue != null && cellValue.ToString().Trim().Equals("Available", StringComparison.OrdinalIgnoreCase) && cmbStatus.Text == "Archived")
            {
                taItems.UpdateStatus("Archived", (int)dgvItems.CurrentRow.Cells[0].Value);
                taProduct.UpdateQuantityInStock_Decrement(txtBarcode.Text);
                int reorderLevel = Convert.ToInt32(taProduct.GetReorderLevel(txtBarcode.Text));

                int quantity = Convert.ToInt32(taItems.CountBarcodes(txtBarcode.Text));
                if (quantity < reorderLevel)
                {
                    taProduct.UpdateStatus("Low Stock", txtBarcode.Text);
                }
                else if (quantity == 0)
                {
                    taProduct.UpdateStatus("Out Of Stock", txtBarcode.Text);
                }

                taItems.FilterByAvailable(dsAdjustments.Item);
                MessageBox.Show("This item(" + txtBarcode.Text + " - " + lstProdDescriptions.Text + ") Status is set to Archived.");
            }
            else if (cellValue != null && cellValue.ToString().Trim().Equals("Archived", StringComparison.OrdinalIgnoreCase) && cmbStatus.Text == "Archived")
            {
                MessageBox.Show("This item(" + txtBarcode.Text + " - " + lstProdDescriptions.Text + ") is already Archived.");
            }
            else if (cellValue != null && cellValue.ToString().Trim().Equals("Adjusted", StringComparison.OrdinalIgnoreCase) && cmbStatus.Text == "Archived")
            {
                MessageBox.Show("This item(" + txtBarcode.Text + " - " + lstProdDescriptions.Text + ") is Adjusted.\nArchive the Item within the Adjustments Table instead.");


            }
            // CMBSTATUS = AVAILABLE:
            else if (cellValue != null && cellValue.ToString().Trim().Equals("Archived", StringComparison.OrdinalIgnoreCase) && cmbStatus.Text == "Available")
            {
                taItems.UpdateStatus("Available", (int)dgvItems.CurrentRow.Cells[0].Value);
                taProduct.UpdateQuantityInStock_Increment(txtBarcode.Text);

                int reorderLevel = Convert.ToInt32(taProduct.GetReorderLevel(txtBarcode.Text));

                int quantity = Convert.ToInt32(taItems.CountBarcodes(txtBarcode.Text));
                if (quantity >= reorderLevel) {
                    taProduct.UpdateStatus("In Stock", txtBarcode.Text);
                }
                else if (quantity < reorderLevel)
                {
                    taProduct.UpdateStatus("Low Stock", txtBarcode.Text);
                }
                else if (quantity == 0)
                {
                    taProduct.UpdateStatus("Out Of Stock", txtBarcode.Text);
                }

                taItems.FilterByAvailable(dsAdjustments.Item);
                MessageBox.Show("This item(" + txtBarcode.Text + " - " + lstProdDescriptions.Text + ") Status is now set to Available.");
            }
            else if (cellValue != null && cellValue.ToString().Trim().Equals("Available", StringComparison.OrdinalIgnoreCase) && cmbStatus.Text == "Available")
            {
                MessageBox.Show("This item(" + txtBarcode.Text + " - " + lstProdDescriptions.Text + ") is already Available.");
            }
            else if (cellValue != null && cellValue.ToString().Trim().Equals("Adjusted", StringComparison.OrdinalIgnoreCase) && cmbStatus.Text == "Available")
            {
                MessageBox.Show("This item(" + txtBarcode.Text + " - " + lstProdDescriptions.Text + ") is Adjusted.\nTo make the item Available again you need to delete its Adjustment record");


            }


        }

        private void txtBatchNumber_TextChanged(object sender, EventArgs e)
        {
            if (txtBatchNumber.Text != "")
            {
                taItems.FilterByBatchNumber(dsAdjustments.Item, Convert.ToInt32(txtBatchNumber.Text));
            }
            else {
                taItems.FilterByAvailable(dsAdjustments.Item);
            }
           
        }

        private void dgvItems_RowHeaderMouseClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            string barcode = dgvItems.CurrentRow.Cells[1].Value.ToString();
            string itemDescription="";
            txtBarcode.Text = barcode;
            foreach (DataRowView row in lstProdDescriptions.Items){
                string lstBarcode = row["barcode_No"].ToString();
                if (barcode == lstBarcode) {
                    itemDescription = row["description"].ToString();
                }
            }
            lblItems.Text ="PRODUCT DETAILS OF "+itemDescription;
            lblAdjustment.Text = "ADJUSTMENT ON " + barcode;//itemDescription + " (" + barcode + ")";
        }


        private void dgvAdjustments_RowHeaderMouseClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            if (dgvAdjustments.CurrentRow != null) {
                nudQuantity.Value = (int)dgvAdjustments.CurrentRow.Cells[3].Value;
                cmbAdjType.Text = dgvAdjustments.CurrentRow.Cells[4].Value.ToString();
                rtxtReason.Text = dgvAdjustments.CurrentRow.Cells[5].Value.ToString();
            }
        }

        private void panel2_Paint(object sender, PaintEventArgs e)
        {

        }

        private void panel3_Paint(object sender, PaintEventArgs e)
        {

        }

        private void btnInsertAdj_Click_1(object sender, EventArgs e)
        {
            var batchNo = dgvItems.CurrentRow.Cells[0].Value;
            string adjType = cmbAdjType.Text;
            string barcode = dgvItems.CurrentRow.Cells[1].Value.ToString();
            
            if (batchNo != null || adjType != "" || nudQuantity.Value != 0)
            {
                if (adjType == "Other")
                {
                    if (rtxtReason != null)
                    {
                        taAdjustments.InsertQuery(Convert.ToInt32(batchNo), Globals.staffPK, rtxtReason.Text, DateTime.Now, "Active", Convert.ToInt32(nudQuantity.Value), cmbAdjType.Text);
                        taAdjustments.Fill(dsAdjustments.Adjustment);

                        taItems.UpdateStatus("Adjusted", Convert.ToInt32(batchNo));
                        taItems.FilterByAvailable(dsAdjustments.Item);

                        taProduct.UpdateQuantityInStock_Decrement(dgvItems.CurrentRow.Cells[1].Value.ToString());
                        int quantity = Convert.ToInt32(taProduct.GetQuantity(barcode));
                        
                        int reorderLevel = Convert.ToInt32(taProduct.GetReorderLevel(barcode));
                        if (quantity < reorderLevel)
                        {
                            taProduct.UpdateStatus("Low Stock", barcode);
                        }
                        else if (quantity == 0)
                        {
                            taProduct.UpdateStatus("Out Of Stock", barcode);
                        }
                        else if (quantity >= reorderLevel)
                        {
                            taProduct.UpdateStatus("In Stock", barcode);
                        }
                        MessageBox.Show("Adjustment Added.");
                        lblTotQuantity.Text = quantity.ToString();
                        int quantityAdj = Convert.ToInt32(taItems.CountBarcodeAdjusted(txtBarcode.Text));
                        lblQuantityAdjusted.Text = quantityAdj.ToString();
                    }
                    else
                    {
                        MessageBox.Show("Please enter a Reason for Adjustment.");
                    }

                }
                else
                {
                    taAdjustments.InsertQuery(Convert.ToInt32(batchNo), Globals.staffPK, rtxtReason.Text, DateTime.Now, "Active", Convert.ToInt32(nudQuantity.Value), cmbAdjType.Text);
                    taAdjustments.Fill(dsAdjustments.Adjustment);

                    taItems.UpdateStatus("Adjusted", Convert.ToInt32(batchNo));
                    taItems.FilterByAvailable(dsAdjustments.Item);

                    taProduct.UpdateQuantityInStock_Decrement(dgvItems.CurrentRow.Cells[1].Value.ToString());
                    int quantity = Convert.ToInt32(taProduct.GetQuantity(barcode));
                    int reorderLevel = Convert.ToInt32(taProduct.GetReorderLevel(barcode));
                    if (quantity < reorderLevel)
                    {
                        taProduct.UpdateStatus("Low Stock", barcode);
                    }
                    else if (quantity == 0)
                    {
                        taProduct.UpdateStatus("Out Of Stock", barcode);
                    }
                    else if (quantity >= reorderLevel)
                    {
                        taProduct.UpdateStatus("In Stock", barcode);
                    }
                    
                    MessageBox.Show("Adjustment Added.");

                }

            }
            else
            {
                if (batchNo == null)
                    MessageBox.Show("Please select an Item from the Items Table to make an Adjustment on.");
                if (adjType == "")
                    MessageBox.Show("Please select an Adjustment Type.");
                if (nudQuantity.Value == 0)
                    MessageBox.Show("Please enter a Quantity.");
            }
        }

        private void btnUpdateAdj_Click_1(object sender, EventArgs e)
        {
            var CurrentAdjCell = dgvAdjustments.CurrentRow.Cells;
            int adjID = Convert.ToInt32(dgvAdjustments.CurrentRow.Cells[0].Value);
            string adjType = cmbAdjType.Text;

            if (CurrentAdjCell != null || nudQuantity.Value != 0)
            {
                if (adjType == "Other")
                {
                    if (rtxtReason != null)
                    {

                        taAdjustments.UpdateQuery(rtxtReason.Text, Convert.ToInt32(nudQuantity.Value), cmbAdjType.Text, adjID);
                        MessageBox.Show("Adjustment " + dgvAdjustments.CurrentRow.Cells[0].Value.ToString() + " is updated.");
                        taAdjustments.Fill(dsAdjustments.Adjustment);
                    }
                    else
                    {
                        MessageBox.Show("Please enter a Reason for Adjustment.");
                    }

                }
                else
                {
                    taAdjustments.UpdateQuery(rtxtReason.Text, Convert.ToInt32(nudQuantity.Value), cmbAdjType.Text, adjID);
                    MessageBox.Show("Adjustment " + dgvAdjustments.CurrentRow.Cells[0].Value.ToString() + " is updated.");
                    taAdjustments.Fill(dsAdjustments.Adjustment);

                }

            }
            else
            {
                if (CurrentAdjCell == null)
                    MessageBox.Show("Please select an Item from the Items Table to make an Adjustment on.");
                //if (adjType == "")
                //    MessageBox.Show("Please select an Adjustment Type.");
                if (nudQuantity.Value == 0)
                    MessageBox.Show("Please enter a Quantity.");
            }


        }

        private void btnDelete_Click_1(object sender, EventArgs e)
        {
            if (dgvAdjustments.CurrentRow != null)
            {
                taAdjustments.DeleteQuery((int)dgvAdjustments.CurrentRow.Cells[0].Value);
                taItems.UpdateStatus("Available", (int)dgvAdjustments.CurrentRow.Cells[1].Value);
                string barcode = "";
                for (int i = 0; i < dgvItems.Rows.Count; i++)
                {
                    if (dgvItems.Rows[i].Cells[0] == dgvAdjustments.CurrentRow.Cells[1].Value)
                        barcode = dgvItems.Rows[i].Cells[1].Value.ToString();
                }
                taProduct.UpdateQuantityInStock_Increment(barcode);
                int quantity = Convert.ToInt32(taProduct.GetQuantity(barcode));
                int reorderLevel = Convert.ToInt32(taProduct.GetReorderLevel(barcode));
                if (quantity < reorderLevel)
                {
                    taProduct.UpdateStatus("Low Stock", barcode);
                }
                else if (quantity == 0)
                {
                    taProduct.UpdateStatus("Out Of Stock", barcode);
                }
                else if (quantity >= reorderLevel)
                {
                    taProduct.UpdateStatus("In Stock", barcode);
                }
                taAdjustments.Fill(dsAdjustments.Adjustment);
                taItems.FilterByAvailable(dsAdjustments.Item);
                MessageBox.Show("Adjustment Record Deleted.");
                lblTotQuantity.Text = quantity.ToString();
                int quantityAdj = Convert.ToInt32(taItems.CountBarcodeAdjusted(txtBarcode.Text));
                lblQuantityAdjusted.Text = quantityAdj.ToString();
                nudQuantity.Value = 0;
                rtxtReason.Text = "";
                cmbAdjType.Text = "";
            }
        }
    }
}
