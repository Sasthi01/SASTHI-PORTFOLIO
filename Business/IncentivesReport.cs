using LGPACKAGING_POS_SYSTEM.WstGrp21DataSetTableAdapters;
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
    public partial class frmIncentivesReport : Form
    {
        public frmIncentivesReport()
        {
            InitializeComponent();
            label1.Parent = pictureBox1;
            label2.Parent = pictureBox1;
       
            // Load data into tables
            this.incentiveTypeTableAdapter1.Fill(this.wstGrp21DataSet1.IncentiveType);
            this.incentivesReportTableAdapter1.Fill(this.wstGrp21DataSet1.IncentivesReport);

            PopulateComboBoxes();
        }



        private void PopulateComboBoxes()
        {
            var incentivesData = incentivesReportTableAdapter1.GetDataBy2();

            // Populate Year ComboBox
            comboBoxYear.Items.Clear();
            comboBoxYear.Items.Add("0"); // Add "All Years" option
            var availableYears = incentivesData.Select(row => row.Field<int>("Year")).Distinct().OrderByDescending(y => y);
            foreach (var year in availableYears)
            {
                comboBoxYear.Items.Add(year.ToString());
            }

            comboBoxYear.SelectedIndex = 0;

        }

        private void comboBoxYear_SelectedIndexChanged(object sender, EventArgs e)
        {
            LoadChartData();
        }
       
        private void LoadChartData()
        {
            int selectedYear = Convert.ToInt32(comboBoxYear.SelectedItem);

            DataTable data = selectedYear == 0 ? GetDataByAllYears() : GetDataByYear(selectedYear);

            if (data.Rows.Count == 0)
            {
                MessageBox.Show("No data found for the selected filters.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                IncentivesChart.Series.Clear();
                return;
            }

            PopulateChart(data, selectedYear);
        }

        private DataTable GetDataByYear(int selectedYear)
        {
            using (var adapter = new IncentivesReportTableAdapter())
            {
                return adapter.GetDataBy1(selectedYear);
            }
        }

        private DataTable GetDataByAllYears()
        {
            using (var adapter = new IncentivesReportTableAdapter())
            {
                return adapter.GetDataBy2();
            }
        }


        private void PopulateChart(DataTable filteredData, int selectedYear)
        {
            IncentivesChart.Series.Clear();
            IncentivesChart.Legends.Clear();
            IncentivesChart.Legends.Add(new Legend("SalesStaffLegend"));

            Dictionary<string, Color> staffColors = new Dictionary<string, Color>();
            Random rand = new Random();

            IncentivesChart.ChartAreas[0].AxisX.CustomLabels.Clear();
            IncentivesChart.ChartAreas[0].AxisX.Interval = 1;
            IncentivesChart.ChartAreas[0].AxisX.Title = selectedYear == 0 ? "Year" : "Month";
            IncentivesChart.ChartAreas[0].AxisY.Title = "Total Incentives Amount";

            // Use month number as X value for correct alignment
            bool isAllYears = selectedYear == 0;
            var staffNames = filteredData.AsEnumerable()
                .Select(row => row.Field<string>("SalesStaff") ?? "Unknown")
                .Distinct()
                .ToList();

            foreach (var staffName in staffNames)
            {
                if (!staffColors.ContainsKey(staffName))
                    staffColors[staffName] = Color.FromArgb(rand.Next(150, 255), rand.Next(150, 255), rand.Next(150, 255));

                Series series = new Series(staffName)
                {
                    ChartType = SeriesChartType.Column,
                    Color = staffColors[staffName],
                    LegendText = staffName
                };
                IncentivesChart.Series.Add(series);

                if (isAllYears)
                {
                    // Group by year
                    var yearGroups = filteredData.AsEnumerable()
                        .Where(row => (row.Field<string>("SalesStaff") ?? "Unknown") == staffName)
                        .GroupBy(row => row.Field<int>("Year"))
                        .OrderBy(g => g.Key);

                    foreach (var group in yearGroups)
                    {
                        double total = group.Sum(r => (double)(r.Field<decimal?>("TotalAmount") ?? 0));
                        series.Points.AddXY(group.Key, total);
                    }
                }
                else
                {
                    // Group by month number
                    var monthGroups = filteredData.AsEnumerable()
                        .Where(row => (row.Field<string>("SalesStaff") ?? "Unknown") == staffName)
                        .GroupBy(row => row.Field<int>("MonthNumber"))
                        .OrderBy(g => g.Key);

                    foreach (var group in monthGroups)
                    {
                        int monthNum = group.Key;
                        string monthLabel = new DateTime(1, monthNum, 1).ToString("MMM");
                        double total = group.Sum(r => (double)(r.Field<decimal?>("TotalAmount") ?? 0));
                        series.Points.AddXY(monthNum, total);

                        // Add custom label only once per month
                        if (IncentivesChart.ChartAreas[0].AxisX.CustomLabels.All(cl => cl.FromPosition != monthNum - 0.5))
                        {
                            IncentivesChart.ChartAreas[0].AxisX.CustomLabels.Add(
                                new CustomLabel(monthNum - 0.5, monthNum + 0.5, monthLabel, 0, LabelMarkStyle.None));
                        }
                    }
                    // Set axis min/max to show all months
                    IncentivesChart.ChartAreas[0].AxisX.Minimum = 0.5;
                    IncentivesChart.ChartAreas[0].AxisX.Maximum = 12.5;
                }
            }

            // Ensure Y-Axis scales dynamically
            double maxValue = IncentivesChart.Series.SelectMany(s => s.Points.Select(p => p.YValues[0])).DefaultIfEmpty(0).Max();
            IncentivesChart.ChartAreas[0].AxisY.Maximum = Math.Ceiling(maxValue * 1.1);
            IncentivesChart.ChartAreas[0].AxisY.LabelStyle.Format = "#,##0";
        }
        

        private void IncentivesChart_Click(object sender, EventArgs e)
        {

        }

        private void frmIncentivesReport_Load(object sender, EventArgs e)
        {

        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {

        }

        private void btnRefresh_Click(object sender, EventArgs e)
        {
            // Get the parent form (if this is an MDI child)
            Form parent = this.MdiParent;

            // Create a new instance of the form
            var newForm = new frmIncentivesReport();

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
