using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace LGPACKAGING_POS_SYSTEM.Business
{
    public partial class frmIncentives: Form
    {
        public frmIncentives()
        {
            InitializeComponent();
        }

        private void frmIncentives_Load(object sender, EventArgs e)
        {
            dsIncentives.EnforceConstraints = false;
            taSales.FilterByCompletedSales(dsIncentives.Sale);
            taIncentiveType.Fill(dsIncentives.IncentiveType);
            taStaff.FillBySaleStaff(dsIncentives.Staff);
            taIncentive.Fill(dsIncentives.Incentive);
            groupBox1.Parent = pictureBox1;
            groupBox5.Parent = pictureBox1;
            

        }

        private void dgvSales_RowHeaderMouseClick(object sender, DataGridViewCellMouseEventArgs e)
        {
          
            
        }

        private void dgvIncentive_RowHeaderCellChanged(object sender, DataGridViewRowEventArgs e)
        {
            if (dgvIncentive.CurrentRow.Cells != null)
            {
                txtStaffIncentive.Text = dgvIncentive.CurrentRow.Cells["staff_ID"].Value.ToString();
                txtSaleNum.Text = dgvIncentive.CurrentRow.Cells["sale_No"].Value.ToString();
                txtAmountIncentive.Text = dgvIncentive.CurrentRow.Cells["amount"].Value.ToString();
                txtIncentiveDate.Text = Convert.ToDateTime(dgvIncentive.CurrentRow.Cells["date"].Value).ToString("dd/MM/yyyy");
                txtIncentiveStatus.Text = dgvIncentive.CurrentRow.Cells["status"].Value.ToString();
            }
        }

        private void dgvIncentiveType_RowHeaderMouseClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            if (dgvIncentiveType.CurrentRow.Cells != null)
            {

                cmbName.Text = dgvIncentiveType.CurrentRow.Cells[1].Value.ToString();
                txtIncentiveTypeName.Text = dgvIncentiveType.CurrentRow.Cells[1].Value.ToString();
                nudThresholdVal.Value = Convert.ToDecimal(dgvIncentiveType.CurrentRow.Cells[2].Value);
                nudRewardPercent.Value = Convert.ToDecimal(dgvIncentiveType.CurrentRow.Cells[3].Value);
                txtThresholdBasis.Text = dgvIncentiveType.CurrentRow.Cells[4].Value.ToString();
                txtRewardBasis.Text = dgvIncentiveType.CurrentRow.Cells[5].Value.ToString();
                rtxtDescription.Text = dgvIncentiveType.CurrentRow.Cells[6].Value.ToString();
            }

            


        }

        private void dgvIncentive_RowHeaderMouseClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            if (dgvIncentive.CurrentRow.Cells != null)
            {
                txtStaffIncentive.Text = dgvIncentive.CurrentRow.Cells[1].Value.ToString();
                txtSaleNum.Text = dgvIncentive.CurrentRow.Cells[2].Value.ToString();
                txtAmountIncentive.Text = dgvIncentive.CurrentRow.Cells[3].Value.ToString();
                txtIncentiveDate.Text = Convert.ToDateTime(dgvIncentive.CurrentRow.Cells[4].Value).ToString("dd/MM/yyyy");
                txtIncentiveStatus.Text = dgvIncentive.CurrentRow.Cells[5].Value.ToString();
            }
        }


        private void btnAddIncentiveT_Click(object sender, EventArgs e)
        {
            try
            {
                taIncentiveType.Insert(txtIncentiveTypeName.Text, nudThresholdVal.Value, nudRewardPercent.Value, txtThresholdBasis.Text, txtRewardBasis.Text, rtxtDescription.Text, "Active");
                taIncentiveType.Fill(dsIncentives.IncentiveType);
                MessageBox.Show("Incentive Type Inserted Successfully", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            
        }

        private void btnUpdate_Click(object sender, EventArgs e)
        {
            try {
                taIncentiveType.UpdateQuery(txtIncentiveTypeName.Text, nudThresholdVal.Value, nudRewardPercent.Value, txtThresholdBasis.Text, txtRewardBasis.Text, rtxtDescription.Text, "Active", (int)dgvIncentiveType.CurrentRow.Cells[0].Value);
                taIncentiveType.Fill(dsIncentives.IncentiveType);
                MessageBox.Show("Incentive Type Updated Successfully", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            } catch (Exception ex) {
                MessageBox.Show(ex.Message);
            }
            
        }

        private void btnArchive_Click(object sender, EventArgs e)
        {
            DialogResult res = MessageBox.Show("Are you sure you want to archive this incentive type?", "Confirm Archive", MessageBoxButtons.YesNo, MessageBoxIcon.Question);


            if (res == DialogResult.Yes) {
                taIncentiveType.UpdateStatus("Archived", (int)dgvIncentiveType.CurrentRow.Cells[0].Value);
                taIncentiveType.Fill(dsIncentives.IncentiveType);
                MessageBox.Show("Incentive Type Archived Successfully", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void btnUpdateIncentive_Click(object sender, EventArgs e)
        {
            taIncentive.UpdateQuery(Convert.ToInt32(cmbIncentiveType.Text), Convert.ToInt32(txtStaffIncentive.Text), Convert.ToInt32(txtSaleNum.Text), Convert.ToDecimal(txtAmountIncentive.Text), DateTime.Now, "Active", (int)dgvIncentive.CurrentRow.Cells[0].Value, (int)dgvIncentive.CurrentRow.Cells[1].Value);
        }

        private void btnAddIncentive_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < dgvIncentive.Rows.Count; i++)
            {
                if (Convert.ToInt32(dgvSales.CurrentRow.Cells[0].Value) == Convert.ToInt32(dgvIncentive.Rows[i].Cells[2].Value))
                {
                    MessageBox.Show("Selected Sale already has an incentive applied to it.");
                    return; // Sale already has an incentive, do not add again
                }
            }
            if (txtAmountRewarded.Text != "" || txtAmountRewarded.Text != (0).ToString("C2"))
            {
                taIncentive.InsertQuery(Convert.ToInt32(cmbIncentiveTypeSale.SelectedValue), Convert.ToInt32(dgvSales.CurrentRow.Cells[2].Value), Convert.ToInt32(dgvSales.CurrentRow.Cells[0].Value), Decimal.Parse(txtAmountRewarded.Text, NumberStyles.Currency), DateTime.Now, "Active");
                taIncentive.Fill(dsIncentives.Incentive);
                dsIncentives.EnforceConstraints = false;
                taSales.FilterByCompletedSales(dsIncentives.Sale);
            }
            else {
                MessageBox.Show("Please Select a valid Sale AND Incentive Type.");
            }
        }


        

        private void btnArchiveIncentive_Click(object sender, EventArgs e)
        {
            DialogResult res = MessageBox.Show("Are you sure you want to archive this incentive?", "Confirm Archive", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (res == DialogResult.Yes) {
                taIncentive.UpdateStatus("Archived", (int)dgvIncentive.CurrentRow.Cells[0].Value, (int)dgvIncentive.CurrentRow.Cells[1].Value);
                taIncentive.FilterByStaffID(dsIncentives.Incentive, (int)dgvIncentive.CurrentRow.Cells[1].Value);
                MessageBox.Show("Incentive Archived Successfully", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            
        }

        ////////////////////////////////////////// -AMOUNT CALCULATION- ///////////////////////////////////////
        public decimal calcIncentiveAmountSale(int incentiveID)
        {
            decimal rewardAmount = 0;
            try
            {
                if (cmbIncentiveTypeSale.Text != "")
                {
                    if (dgvSales.CurrentRow.Cells != null)
                    {
                        for (int i = 0; i < dgvIncentiveType.Rows.Count; i++)
                        {
                            int incentiveTypeID = Convert.ToInt32(dgvIncentiveType.Rows[i].Cells[0].Value);
                            string thresholdBasis = dgvIncentiveType.Rows[i].Cells[4].Value.ToString().Trim();
                            decimal thresholdAmount = Convert.ToDecimal(dgvIncentiveType.Rows[i].Cells[2].Value);
                            string rewardBasis = dgvIncentiveType.Rows[i].Cells[5].Value.ToString().Trim();
                            decimal rewardPercent = Convert.ToDecimal(dgvIncentiveType.Rows[i].Cells[3].Value);
                            
                            if (incentiveID == incentiveTypeID)
                            {
                                
                                if (Convert.ToInt32(dgvSales.CurrentRow.Cells[thresholdBasis].Value) >= Convert.ToInt32(thresholdAmount))
                                {

                                    rewardAmount = Convert.ToDecimal(dgvSales.CurrentRow.Cells[rewardBasis].Value) * (rewardPercent);
                                    
                                }
                                else
                                {
                                    MessageBox.Show($"The sale is less than the threshold amount of {thresholdAmount} for this incentive type.", "Threshold Not Met", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                                }
                            }
                        }

                    }

                    else
                    {
                        MessageBox.Show("Please Select a Sale from the table before proceeding");
                    }
                }
            }
            catch (Exception ex) {
                MessageBox.Show(ex.Message);
            }
            return rewardAmount;
        }
        public decimal calcQuantity(int incentiveID) {
            decimal rewardAmount=0;
            try
            {
                if (cmbIncentiveTypeSale.Text != "")
                {
                    if (dgvSales.CurrentRow.Cells != null)
                    {
                        int quantity = Convert.ToInt32(tasaleItem.CountSaleQuantity(Convert.ToInt32(dgvSales.CurrentRow.Cells[0].Value)));
                        for (int i = 0; i < dgvIncentiveType.Rows.Count; i++)
                        {
                            int incentiveTypeID = Convert.ToInt32(dgvIncentiveType.Rows[i].Cells[0].Value);
                            decimal thresholdAmount = Convert.ToDecimal(dgvIncentiveType.Rows[i].Cells[2].Value);
                            string rewardBasis = dgvIncentiveType.Rows[i].Cells[5].Value.ToString().Trim();
                            decimal rewardPercent = Convert.ToDecimal(dgvIncentiveType.Rows[i].Cells[3].Value);

                            if (incentiveID == incentiveTypeID)
                            {
                                if (quantity >= (int)thresholdAmount)
                                {
                                    rewardAmount = Convert.ToDecimal(dgvSales.CurrentRow.Cells[rewardBasis].Value) * (rewardPercent);
                                }
                                else
                                {
                                    MessageBox.Show($"The sale is less than the threshold amount of {thresholdAmount} for this incentive type.", "Threshold Not Met", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                                }
                            }
                        }


                    }
                    else
                    {
                        MessageBox.Show("Please Select a Sale from the table before proceeding");
                    }
                }
            }
            catch(Exception ex) {
                MessageBox.Show(ex.Message);
            }
            return rewardAmount;
        }
        public decimal calcIncentiveAmountSale2(int incentiveID, int incentiveSaleNum)
        {
            
                decimal rewardAmount = 0;
                bool salefound = false;
                bool salethreshold = false;
            try
            {
                if (cmbIncentiveType.Text != "")
                {
                    for (int k = 0; k < dgvSales.Rows.Count; k++)
                    {
                        int saleNo = Convert.ToInt32(dgvSales.Rows[k].Cells[0].Value);
                        if (incentiveSaleNum == saleNo)
                        {
                            salefound = true;
                            for (int i = 0; i < dgvIncentiveType.Rows.Count; i++)
                            {
                                int incentiveTypeID = Convert.ToInt32(dgvIncentiveType.Rows[i].Cells[0].Value);
                                string thresholdBasis = dgvIncentiveType.Rows[i].Cells[4].Value.ToString().Trim();
                                decimal thresholdAmount = Convert.ToDecimal(dgvIncentiveType.Rows[i].Cells[2].Value);
                                string rewardBasis = dgvIncentiveType.Rows[i].Cells[5].Value.ToString().Trim();
                                decimal rewardPercent = Convert.ToDecimal(dgvIncentiveType.Rows[i].Cells[3].Value);

                                if (incentiveID == incentiveTypeID)
                                {
                                    if (Convert.ToInt32(dgvSales.Rows[k].Cells[thresholdBasis].Value) >= Convert.ToInt32(thresholdAmount))
                                    {
                                        salethreshold = true;
                                        rewardAmount = Convert.ToDecimal(dgvSales.Rows[k].Cells[rewardBasis].Value) * (rewardPercent);
                                    }
                                }
                            }

                        }

                    }

                    if (salefound == false)
                    {
                        MessageBox.Show("Sale does not exist.");
                    }
                    if (salethreshold == false)
                    {
                        MessageBox.Show($"The sale is less than the threshold amount for this incentive type.", "Threshold Not Met", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                }
            }
            catch(Exception ex) {
                MessageBox.Show(ex.Message);
            }
            return rewardAmount;
        }

        public decimal calcQuantity2(int incentiveID,int incentiveSaleNum)
        {
            decimal rewardAmount = 0;
            bool salefound = false;
            bool salethreshold = false;
            try { 
            if (cmbIncentiveType.Text != "")
            {
                for (int k = 0; k < dgvSales.Rows.Count; k++)
                {
                    int saleNo = Convert.ToInt32(dgvSales.Rows[k].Cells[0].Value);
                    if (incentiveSaleNum == saleNo)
                    {
                        salefound = true;
                        int quantity = Convert.ToInt32(tasaleItem.CountSaleQuantity(incentiveSaleNum));
                        for (int i = 0; i < dgvIncentiveType.Rows.Count; i++)
                        {
                            int incentiveTypeID = Convert.ToInt32(dgvIncentiveType.Rows[i].Cells[0].Value);
                            //string thresholdBasis = dgvIncentiveType.Rows[i].Cells[4].Value.ToString().Trim();
                            decimal thresholdAmount = Convert.ToDecimal(dgvIncentiveType.Rows[i].Cells[2].Value);
                            string rewardBasis = dgvIncentiveType.Rows[i].Cells[5].Value.ToString().Trim();
                            decimal rewardPercent = Convert.ToDecimal(dgvIncentiveType.Rows[i].Cells[3].Value);

                            if (incentiveID == incentiveTypeID)
                            {
                                if (quantity >= (int)thresholdAmount)
                                {
                                    salethreshold = true;
                                    rewardAmount = Convert.ToDecimal(dgvSales.Rows[k].Cells[rewardBasis].Value) * (rewardPercent);
                                }
                            }
                        }


                    }
                }

                if (salefound == false)
                {
                    MessageBox.Show("Sale does not exist.");
                }
                if (salethreshold == false)
                {
                    MessageBox.Show($"The sale is less than the threshold amount for this incentive type.", "Threshold Not Met", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            return rewardAmount;
        }


        private void cmbIncentiveType_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (dgvIncentive.CurrentRow != null)
            {
                int saleNum = Convert.ToInt32(dgvIncentive.CurrentRow.Cells[2].Value);
                
                 if (Convert.ToInt32(cmbIncentiveType.SelectedValue) != 4)
                {
                    txtAmountIncentive.Text = calcIncentiveAmountSale2(Convert.ToInt32(cmbIncentiveType.SelectedValue),saleNum).ToString("C2");
                }
                else if (Convert.ToInt32(cmbIncentiveType.SelectedValue) == 4)
                {
                    txtAmountIncentive.Text = calcQuantity2(Convert.ToInt32(cmbIncentiveType.SelectedValue),saleNum).ToString("C2");
                }
            }
            else {

                //MessageBox.Show("Please select an Incentive from the table before proceeding.", "Selection Required", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void cmbIncentiveTypeSale_SelectedIndexChanged(object sender, EventArgs e)
        {
            
            if (Convert.ToInt32(cmbIncentiveTypeSale.SelectedValue) != 4) {
                txtAmountRewarded.Text = calcIncentiveAmountSale(Convert.ToInt32(cmbIncentiveTypeSale.SelectedValue)).ToString("C2");
            } else if (Convert.ToInt32(cmbIncentiveTypeSale.SelectedValue) == 4) {
                txtAmountRewarded.Text = calcQuantity(Convert.ToInt32(cmbIncentiveTypeSale.SelectedValue)).ToString("C2");
            }
                
        }

        private void txtStaffID_TextChanged(object sender, EventArgs e)
        {
            if (txtStaffID.Text != "")
            {
                taSales.FilterBySaleStaff(dsIncentives.Sale, Convert.ToInt32(txtStaffID.Text));
            }
            else {
                dsIncentives.EnforceConstraints = false;
                taSales.FilterByCompletedSales(dsIncentives.Sale);
            }
        }

        private void dgvSales_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }
    }
}
