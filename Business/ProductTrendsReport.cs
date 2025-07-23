using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;

namespace LGPACKAGING_POS_SYSTEM.Business
{
    
    public partial class frmProductTrendsReport: Form
    {
        public frmProductTrendsReport()
        {
            InitializeComponent();
            label2.Parent = pictureBox1;
            label3.Parent = pictureBox1;
            label4.Parent = pictureBox1;

            // Populate the Years ComboBox from Database
            DataTable yearsTable = taDataTableYear.GetData(); // Get years from database
            DataRow allYearsRow = yearsTable.NewRow();
            allYearsRow["SaleYear"] = 0; // "All Years" option
            yearsTable.Rows.InsertAt(allYearsRow, 0);

            comboBoxYear1.DataSource = yearsTable;
            comboBoxYear1.DisplayMember = "SaleYear";
            comboBoxYear1.ValueMember = "SaleYear";

            // Populate the Months ComboBox with predefined values
            var monthGroups = new List<KeyValuePair<int, string>>()
            {
               new KeyValuePair<int, string>(3, "Jan-Mar"),
               new KeyValuePair<int, string>(6, "Apr-Jun"),
               new KeyValuePair<int, string>(9, "Jul-Sep"),
               new KeyValuePair<int, string>(12, "Oct-Dec")
            };

            comboBoxMonths1.DataSource = monthGroups;
            comboBoxMonths1.DisplayMember = "Value";  // Show text in dropdown
            comboBoxMonths1.ValueMember = "Key";      // Use numbers as actual values
        }

        private void frmProductTrendsReport_Load_1(object sender, EventArgs e)
        {
            // Initial loading of dataset for year and month filtering
            int selectedYear = Convert.ToInt32(comboBoxYear1.SelectedValue);
            int selectedMonths = Convert.ToInt32(comboBoxMonths1.SelectedValue);

            taProdDemand.Fill(DSGroup.ProdDemand, selectedYear);
            taProdDemand.FillByMonthAndYear(DSGroup.ProdDemand, selectedYear, selectedMonths);

            BindYearlyChart();
            BindMonthlyChart();

        }

        private void comboBoxYear1_SelectedIndexChanged_1(object sender, EventArgs e)
        {
            int selectedYear = Convert.ToInt32(comboBoxYear1.SelectedValue);

            // Ensure dataset is cleared before refilling
            DSGroup.ProdDemand.Clear();

            if (selectedYear == 0) // "All Years" selected
            {
                DSGroup.EnforceConstraints = false;
                taProdDemand.FillByAllYear(DSGroup.ProdDemand);
            }
            else
            {
                DSGroup.EnforceConstraints = false;
                taProdDemand.Fill(DSGroup.ProdDemand, selectedYear);
            }

            BindYearlyChart();
        }

        private void comboBoxMonths1_SelectedIndexChanged(object sender, EventArgs e)
        {
            int selectedYear = Convert.ToInt32(comboBoxYear1.SelectedValue);
            int selectedMonths = Convert.ToInt32(comboBoxMonths1.SelectedValue);

            // Ensure dataset is cleared before refilling
            DSGroup.ProdDemand.Clear();

            // Load filtered data based on year & month grouping
            taProdDemand.FillByMonthAndYear(DSGroup.ProdDemand, selectedYear, selectedMonths);

            BindMonthlyChart();
        }


        private void BindYearlyChart()
        {
            // Bind yearly data to first chart (ProdDemand)
            YearlyChart.DataSource = DSGroup.ProdDemand;
            YearlyChart.Series["Series1"].XValueMember = "description";  // Products on X-axis
            YearlyChart.Series["Series1"].YValueMembers = "TotalQuantity";  // Sales quantities on Y-axis
            YearlyChart.DataBind();
        }
        private Dictionary<string, Color> productColorMap = new Dictionary<string, Color>(); // Global variable
        private void BindMonthlyChart()
        {
            MonthlyChart1.Series.Clear(); // Remove previous series

            Dictionary<string, Series> productSeriesDict = new Dictionary<string, Series>();
            Random rand = new Random(); // Declare random generator once
            foreach (DataRow row in DSGroup.ProdDemand.Rows)
            {
                string productName = row["description"].ToString();
                string saleMonth = row["SaleMonth"].ToString(); // Month name
                int monthNumber = Convert.ToInt32(row["MonthNumber"]); // Numeric month order
                int totalQuantity = Convert.ToInt32(row["TotalQuantity"]);

                // Ensure each product has a unique series
                if (!productSeriesDict.ContainsKey(productName))
                {
                    Series newSeries = MonthlyChart1.Series.Add(productName);
                    newSeries.ChartType = SeriesChartType.Column; // Grouped bars
                    newSeries.IsValueShownAsLabel = true;
                    newSeries.LegendText = productName;

                // Assign color persistently for each product
                    if (!productColorMap.ContainsKey(productName))
                    {
                        productColorMap[productName] = Color.FromArgb(rand.Next(100, 255), rand.Next(100, 255), rand.Next(100, 255));
                    }

                    newSeries.Color = productColorMap[productName]; // Set persistent color
                    productSeriesDict[productName] = newSeries;
            }

            // Correctly bind month values to avoid collapsing data into July
            productSeriesDict[productName].Points.AddXY(monthNumber, totalQuantity); // Bind using numeric month order
            }

            // Ensure proper spacing for months on the X-axis
            MonthlyChart1.ChartAreas[0].AxisX.Interval = 1;
            MonthlyChart1.ChartAreas[0].AxisX.Title = "Month";
            MonthlyChart1.ChartAreas[0].AxisY.Title = "Total Sales Quantity";

            // Format the X-axis to display proper month names instead of numbers
            MonthlyChart1.ChartAreas[0].AxisX.CustomLabels.Clear();
            Dictionary<int, string> monthLabels = new Dictionary<int, string>
            {
                 { 1, "Jan" }, { 2, "Feb" }, { 3, "Mar" },
                 { 4, "Apr" }, { 5, "May" }, { 6, "Jun" },
                 { 7, "Jul" }, { 8, "Aug" }, { 9, "Sep" },
                 { 10, "Oct" }, { 11, "Nov" }, { 12, "Dec" }
            };

            foreach (var entry in monthLabels)
            {
                CustomLabel label = new CustomLabel(entry.Key - 0.5, entry.Key + 0.5, entry.Value, 0, LabelMarkStyle.None);
                MonthlyChart1.ChartAreas[0].AxisX.CustomLabels.Add(label);
            }
        }

        private void YearlyChart_Click(object sender, EventArgs e)
        {

        }

       
    }
}

