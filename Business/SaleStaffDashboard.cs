using LGPACKAGING_POS_SYSTEM.Interface;
using LGPACKAGING_POS_SYSTEM.WstGrp21DataSetTableAdapters;
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
using System.Windows.Forms.DataVisualization.Charting;
using System.Xml.Linq;
using static LGPACKAGING_POS_SYSTEM.Interface.frmLogin;
using static System.Collections.Specialized.BitVector32;


namespace LGPACKAGING_POS_SYSTEM.Business
{
    public partial class frmSaleStaffDashboard: Form
    {
        public frmSaleStaffDashboard()
        {
            InitializeComponent();

            taStaff.FillDashboard(dsSaleStaff.Staff,Globals.staffPK);
            taIncentives.FilterByStaffID(dsSaleStaff.Incentive, Globals.staffPK);
            pnl1.Parent = pictureBox1;
            pnl2.Parent = pictureBox1;
            groupBox1.Parent = pictureBox1;
            groupBox2.Parent = pictureBox1;
        }

        private void frmSaleStaffDashboard_Load(object sender, EventArgs e)
        {
            taIncentives.FilterByStaffID(dsSaleStaff.Incentive, Globals.staffPK);

            if (dsSaleStaff.Staff.Rows.Count > 0)
            {
                var row = dsSaleStaff.Staff.Rows[0];
               // pbStaffImage.Image = Image.FromFile(row["image"].ToString());
                lblFullName.Text = row["firstName"].ToString()+" "+ row["lastName"].ToString();
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
                string defaultImagePath = Path.Combine(Application.StartupPath, "user.png");
                if (File.Exists(fullImagePath))
                {
                    pbStaffImage.Image = Image.FromFile(fullImagePath);
                }
                
            }
            decimal amount = 0;
            int quantity = 0;
            decimal revenue = 0;
            if (dgvIncentives.Rows != null) {
                
                for (int i = 0; i < dgvIncentives.Rows.Count;i++) { 
                    amount += Convert.ToDecimal(dgvIncentives.Rows[i].Cells[3].Value);
                    quantity+= Convert.ToInt32(taSaleItem.CountIncentiveQuantitySold(Convert.ToInt32(dgvIncentives.Rows[i].Cells[2].Value)));
                    revenue += Convert.ToDecimal(taSale.CountIncentiveRevenueMade(Convert.ToInt32(dgvIncentives.Rows[i].Cells[2].Value)));
                }

            }
            txtTotAmount.Text = amount.ToString("C2");
            txtNumIncentives.Text = dgvIncentives.Rows.Count.ToString();
            
            txtQuantity.Text = quantity.ToString();
            txtRevenue.Text = revenue.ToString("C2");

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
 

        private void LoadChartData()
        {
            int staffID = Convert.ToInt32(txtStaffID.Text);


            string selectedFilter = cmbFilterChart.SelectedItem?.ToString() ?? "";

            if (wstGrp21DataSet.DashSaleReport == null)
            {

                // Clear any existing rows before filling to avoid constraint issues
                wstGrp21DataSet.DashSaleReport.Clear();
            }
            WstGrp21DataSet.DashSaleReportDataTable dtSalesData = wstGrp21DataSet.DashSaleReport;

            wstGrp21DataSet.EnforceConstraints = false; // Disable constraints temporarily

            switch (selectedFilter)
            {
                case "Current Year":
                    taDashSaleReport.FillByCurrentYear(dtSalesData, staffID);
                    break;
                case "Current Week":
                    taDashSaleReport.FillByCurrentWeek(dtSalesData, staffID);
                    break;
                case "Previous 6 Months":
                    taDashSaleReport.FillByPrevious6Months(dtSalesData, staffID);
                    break;
            }



            chSalesReport.Series.Clear();
            Series salesSeries = new Series("Sales Data");

            if (rbSalesRevenue.Checked)
            {
                salesSeries.ChartType = SeriesChartType.Column;
                foreach (DataRow row in dtSalesData.Rows)
                {
                    var period = row.Table.Columns.Contains("period") && row["period"] != DBNull.Value ? row["period"].ToString() : "";
                    var totalRevenue = row.Table.Columns.Contains("totalRevenue") && row["totalRevenue"] != DBNull.Value ? Convert.ToDecimal(row["totalRevenue"]) : 0m;
                    salesSeries.Points.AddXY(period, totalRevenue);
                }
            }
            else if (rbSalesNo.Checked)
            {
                salesSeries.ChartType = SeriesChartType.Column;
                foreach (DataRow row in dtSalesData.Rows)
                {
                    var period = row.Table.Columns.Contains("period") && row["period"] != DBNull.Value ? row["period"].ToString() : "";
                    var totalSales = row.Table.Columns.Contains("totalSales") && row["totalSales"] != DBNull.Value ? Convert.ToInt32(row["totalSales"]) : 0;
                    salesSeries.Points.AddXY(period, totalSales);
                }
            }


            chSalesReport.Series.Add(salesSeries);
            chSalesReport.ChartAreas[0].AxisX.Title = "Period";
            chSalesReport.ChartAreas[0].AxisX.IntervalType = DateTimeIntervalType.Auto;
            chSalesReport.ChartAreas[0].AxisX.LabelStyle.Angle = -45;
            chSalesReport.ChartAreas[0].AxisX.IsMarginVisible = true;

            chSalesReport.ChartAreas[0].AxisY.Title = rbSalesRevenue.Checked ? "Total Revenue" : "Total Sales";
            chSalesReport.ChartAreas[0].AxisY.LabelStyle.Format = rbSalesRevenue.Checked ? "C2" : "N0";
            chSalesReport.ChartAreas[0].AxisY.IntervalAutoMode = IntervalAutoMode.VariableCount;
        }
       
        private void rbSalesRevenue_CheckedChanged_1(object sender, EventArgs e)
        {
            LoadChartData();
        }

        private void rbSalesNo_CheckedChanged_1(object sender, EventArgs e)
        {
            LoadChartData();
        }

        private void cmbFilterChart_SelectedIndexChanged_1(object sender, EventArgs e)
        {
            LoadChartData();
        }
    }
}
