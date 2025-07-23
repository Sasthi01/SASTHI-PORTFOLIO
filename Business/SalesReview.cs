using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace LGPACKAGING_POS_SYSTEM.Business
{
    public partial class frmSalesReview: Form
    {
        public frmSalesReview()
        {
            InitializeComponent();
            taSale.FilterByConfirmedSales(dsSaleReview.Sale);
            groupBox1.Parent = pictureBox1;

        }

        private void frmSalesReview_Load(object sender, EventArgs e)
        {
            
        }

        private void dataGridView1_RowHeaderMouseDoubleClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            if (dgvSale.CurrentRow != null)
            {
                taSaleItem.FilterBySelectedSale(dsSaleReview.SaleItem, Convert.ToInt32(dgvSale.CurrentRow.Cells[0].Value));
            }
            
        }

        private void txtSaleNumber_TextChanged(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(txtSaleNumber.Text))
            {
                taSale.SearchBySaleNo(dsSaleReview.Sale, Convert.ToInt32(txtSaleNumber.Text));
            }
            else
            {
                taSale.FilterByConfirmedSales(dsSaleReview.Sale);
            }
        }



        private void btnCancel_Click(object sender, EventArgs e)
        {
            DialogResult res = MessageBox.Show("Are you sure you want to cancel this sale?", "Confirm Cancellation", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            Array arr = dgvSaleItem.Rows.Cast<DataGridViewRow>().Select(r => r.Cells[1].Value).ToArray();

            if (res == DialogResult.Yes)
            {
                if (dgvSale.CurrentRow != null)
                {
                    int saleId = Convert.ToInt32(dgvSale.CurrentRow.Cells[0].Value);
                    int loyaltyEarned = Convert.ToInt32(dgvSale.CurrentRow.Cells[6].Value);
                    int customerId = Convert.ToInt32(dgvSale.CurrentRow.Cells[1].Value);
                    taSale.UpdateStatus("Cancelled", saleId);
                    taCustomer.UpdateLoyaltyDecrement(customerId, loyaltyEarned);   
                    taSale.FilterByConfirmedSales(dsSaleReview.Sale);

                    for (int i = 0; i < arr.Length; i++)
                    {
                        int batchNumber = Convert.ToInt32(arr.GetValue(i));
                        taItem.UpdateStatus("Available", batchNumber);
                    }
                   

                    taSaleItem.DeleteSpecificSale(saleId);
                    taSaleItem.FilterBySelectedSale(dsSaleReview.SaleItem, saleId);
                    
                    MessageBox.Show("Sale cancelled successfully.", "Cancellation Successful", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    MessageBox.Show("Please select a sale to cancel.", "No Sale Selected", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
        }
    }
}
