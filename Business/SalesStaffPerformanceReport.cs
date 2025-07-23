using LGPACKAGING_POS_SYSTEM.WstGrp21DataSetTableAdapters;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;

namespace LGPACKAGING_POS_SYSTEM.Business
{
    public partial class frmSalesStaffPerformanceReport : Form
    {
        private Dictionary<string, Color> staffColorMap = new Dictionary<string, Color>(); // Persistent colors
        private bool isInitialized = false;

        public frmSalesStaffPerformanceReport()
        {
            InitializeComponent();
            PopulateComboBoxes();
            isInitialized = true;

            label1.Parent = pictureBox1;
            label2.Parent = pictureBox1;
            label3.Parent = pictureBox1;
            radioButtonRevenue.Parent = pictureBox1;
        }

        private void PopulateComboBoxes()
        {

            // Populate Years
            DataTable yearsTable = new SalesPerformanceTableAdapter().GetYears();
            DataRow allYearsRow = yearsTable.NewRow();
            allYearsRow["SaleYear"] = 0; // "All Years" option
            yearsTable.Rows.InsertAt(allYearsRow, 0);
            comboBoxYear.DataSource = yearsTable;
            comboBoxYear.DisplayMember = "SaleYear";
            comboBoxYear.ValueMember = "SaleYear";

           
        }

        

        private void LoadChartData()
        {
            if (!isInitialized) return;
            int selectedYear = Convert.ToInt32(comboBoxYear.SelectedValue);
            bool showRevenue = radioButtonRevenue.Checked;
            bool isAllYears = selectedYear == 0;

            using (var adapter = new SalesPerformanceTableAdapter())
            {
                DataTable salesData = adapter.GetSalesData(selectedYear); // Get full dataset



                if (salesData.Rows.Count == 0)
                {
                    MessageBox.Show("No data found for the selected filters.");
                    SalesPerformance.Series.Clear();
                    SalesPerformance.ChartAreas[0].AxisX.CustomLabels.Clear();
                    return;
                }

                PopulateChart(salesData, showRevenue, isAllYears);
            }
        }

        private void PopulateChart(DataTable salesData, bool showRevenue, bool isAllYears)
        {
            SalesPerformance.Series.Clear();
            SalesPerformance.ChartAreas[0].AxisX.Title = isAllYears ? "Years" : "Sales Period";
            SalesPerformance.ChartAreas[0].AxisY.Title = showRevenue ? "Total Revenue" : "Total Sales";

            Dictionary<string, Series> staffSeriesDict = new Dictionary<string, Series>();
            Random rand = new Random();
            SalesPerformance.ChartAreas[0].AxisX.CustomLabels.Clear();

            // Use a HashSet to avoid duplicate custom labels
            HashSet<int> addedPeriods = new HashSet<int>();

            foreach (DataRow row in salesData.Rows)
            {
                string staffName = $"{row["firstName"]} {row["lastName"]}";
                int periodNumber = Convert.ToInt32(row["PeriodNumber"]);
                string periodLabel = row["SalesPeriod"].ToString();
                int value = showRevenue ? Convert.ToInt32(row["TotalRevenue"]) : Convert.ToInt32(row["TotalSales"]);

                if (!staffSeriesDict.ContainsKey(staffName))
                {
                    Series newSeries = SalesPerformance.Series.Add(staffName);
                    newSeries.ChartType = SeriesChartType.Column;
                    newSeries.IsValueShownAsLabel = true;
                    newSeries.LegendText = staffName;

                    if (!staffColorMap.ContainsKey(staffName))
                        staffColorMap[staffName] = Color.FromArgb(rand.Next(100, 255), rand.Next(100, 255), rand.Next(100, 255));

                    newSeries.Color = staffColorMap[staffName];
                    staffSeriesDict[staffName] = newSeries;
                }

                // Use periodNumber as X value for correct label alignment
                staffSeriesDict[staffName].Points.AddXY(periodNumber, value);

                // Add custom label only once per period
                if (!addedPeriods.Contains(periodNumber))
                {
                    SalesPerformance.ChartAreas[0].AxisX.CustomLabels.Add(
                        new CustomLabel(periodNumber - 0.5, periodNumber + 0.5, periodLabel, 0, LabelMarkStyle.None));
                    addedPeriods.Add(periodNumber);
                }
            }

            if (salesData.Rows.Count > 0)
            {
                double maxValue = salesData.AsEnumerable().Max(row => showRevenue ? Convert.ToDouble(row["TotalRevenue"]) : Convert.ToDouble(row["TotalSales"]));
                SalesPerformance.ChartAreas[0].AxisY.Maximum = Math.Ceiling(maxValue * 1.1);
            }
            else
            {
                SalesPerformance.ChartAreas[0].AxisY.Maximum = 1;
            }




        }

        private void comboBoxYear_SelectedIndexChanged(object sender, EventArgs e) => LoadChartData();
      
        private void radioButtonSales_CheckedChanged(object sender, EventArgs e) => LoadChartData();
        private void radioButtonRevenue_CheckedChanged(object sender, EventArgs e) => LoadChartData();

        private void frmSalesStaffPerformanceReport_Load(object sender, EventArgs e)
        {

        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void btnRefresh_Click(object sender, EventArgs e)
        {
            // Get the parent form (if this is an MDI child)
            Form parent = this.MdiParent;

            // Create a new instance of the form
            var newForm = new frmSalesStaffPerformanceReport();

            if (parent != null)
            {
                // Set as MDI child and fit to parent
                newForm.MdiParent = parent;
                newForm.WindowState = FormWindowState.Maximized;
            }
            else
            {
                // If not MDI, size to current window
                newForm.StartPosition = FormStartPosition.Manual;
                newForm.Size = this.Size;
                newForm.Location = this.Location;
            }

            newForm.Show();
            this.Close();
        }
    }
}
