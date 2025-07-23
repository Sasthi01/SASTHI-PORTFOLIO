using LGPACKAGING_POS_SYSTEM.Business;
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
using Microsoft.VisualBasic;
using System.Reflection.Emit;
using static LGPACKAGING_POS_SYSTEM.Interface.frmLogin;

namespace LGPACKAGING_POS_SYSTEM
{
    public partial class frmMainMenu: Form
    {
        public frmMainMenu()
        {
            InitializeComponent();
            
        }
        public void FormSetup(Form myForm) {
            if (this.ActiveMdiChild != null)
            {
                this.ActiveMdiChild.Close();
            }

            myForm.MdiParent= this;
            msMain.Parent = this;
            myForm.WindowState = FormWindowState.Maximized;
           
            
            myForm.Show();
            pictureBox1.SendToBack();

        }

        private void productsItemsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            frmInventory inventory = new frmInventory();
            FormSetup(inventory);
        }


        private void registerCustomerToolStripMenuItem_Click(object sender, EventArgs e)
        {
            frmRegisterCustomer registerCustomer = new frmRegisterCustomer();
            registerCustomer.Show();
        }

        private void newSaleToolStripMenuItem_Click(object sender, EventArgs e)
        {
            frmSale sale = new frmSale();
            FormSetup(sale);
        }

        private void adjustmentsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            frmAdjustments adjustments = new frmAdjustments();
            FormSetup(adjustments);
        }

        private void productTrendsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            frmProductTrendsReport productTrendsReport = new frmProductTrendsReport();
            FormSetup(productTrendsReport);
        }

        private void salesStaffPerformanceToolStripMenuItem_Click(object sender, EventArgs e)
        {
            frmSalesStaffPerformanceReport salesStaffPerformanceReport = new frmSalesStaffPerformanceReport();
            FormSetup(salesStaffPerformanceReport);
        }

        private void incentivesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            frmIncentivesReport incentivesReport = new frmIncentivesReport();
            FormSetup(incentivesReport);
        }


        /// ////////////////////////////////////////////////////////////////////////////////////// -YUSUF
        

        public void PerformLogout()
        {
            // Disable core menus
            dashboardToolStripMenuItem.Enabled = false;
            saleToolStripMenuItem.Enabled = false;
            inventoryToolStripMenuItem.Enabled = false;
            reportsToolStripMenuItem.Enabled = false;
            incentivesToolStripMenuItem1.Enabled = false;

            registerStaffToolStripMenuItem.Visible = false;
            reviewSalesToolStripMenuItem.Visible = false;
            // Enable Login, disable Logout
            HomeToolStripMenuItem.Enabled = true;
            logOutToolStripMenuItem.Enabled = false;
            loginToolStripMenuItem.Enabled = true;
            label2.Enabled = true; // Enable the login label

        }

        public void SetPermissionsByRole(string role)
        {
            role = role.Trim().ToLower(); // Normalize before switch
            // Disable all initially
            dashboardToolStripMenuItem.Enabled = false;
            saleToolStripMenuItem.Enabled = false;
            inventoryToolStripMenuItem.Enabled = false;
            reportsToolStripMenuItem.Enabled = false;
            HomeToolStripMenuItem.Enabled = false;
            logOutToolStripMenuItem.Enabled = true;
            incentivesToolStripMenuItem1.Enabled = false;
            
          

            switch (role)
            {
                case "owner":
                    HomeToolStripMenuItem.Enabled = true;
                    dashboardToolStripMenuItem.Enabled = true;
                    saleToolStripMenuItem.Enabled = true;
                    newSaleToolStripMenuItem.Enabled = false;
                    inventoryToolStripMenuItem.Enabled = true;
                    reportsToolStripMenuItem.Enabled = true;
                    registerStaffToolStripMenuItem.Visible = true;
                    manageStaffToolStripMenuItem.Visible = true;
                    incentivesToolStripMenuItem1.Enabled = true;
                    productsItemsToolStripMenuItem.Visible = true;
                    productTrendsToolStripMenuItem.Visible = true;
                    //ownerToolStripMenuItem.Visible = true;
                    reviewSalesToolStripMenuItem.Visible = true;
                    loginToolStripMenuItem.Enabled = false;
                    label2.Enabled = false; 
                    // adjustmentLogToolStripMenuItem.Visible = false;

                    break;

                case "manager":
                    HomeToolStripMenuItem.Enabled = true;
                    dashboardToolStripMenuItem.Enabled = true;
                    saleToolStripMenuItem.Enabled = true;
                    newSaleToolStripMenuItem.Enabled = false;
                    inventoryToolStripMenuItem.Enabled = true;
                    reportsToolStripMenuItem.Enabled = true;
                    reviewSalesToolStripMenuItem.Visible = true;
                    incentivesToolStripMenuItem1.Enabled = true;
                    //managerToolStripMenuItem.Visible = true;
                    registerStaffToolStripMenuItem.Visible = true;
                    manageStaffToolStripMenuItem.Visible = true;
                    productsItemsToolStripMenuItem.Visible = true;
                    // Hide unwanted reports for manager
                    productTrendsToolStripMenuItem.Visible = false;
                    loginToolStripMenuItem.Enabled = false;
                    label2.Enabled = false;
                    // adjustmentLogToolStripMenuItem.Visible = false;
                    break;

                case "sale":
                    HomeToolStripMenuItem.Enabled = true;
                    dashboardToolStripMenuItem.Enabled = true;
                    newSaleToolStripMenuItem.Enabled = true;
                    saleToolStripMenuItem.Enabled = true;
                    reviewSalesToolStripMenuItem.Visible = true;
                    productTrendsToolStripMenuItem.Visible = false;
                    loginToolStripMenuItem.Enabled = false;
                    label2.Enabled = false;
                    //salesStaffToolStripMenuItem.Visible = true;
                    break;

                case "warehouse":
                    HomeToolStripMenuItem.Enabled = true;
                    dashboardToolStripMenuItem.Enabled = true;
                    newSaleToolStripMenuItem.Enabled = false;
                    inventoryToolStripMenuItem.Enabled = true;
                    productTrendsToolStripMenuItem.Visible = true;
                    loginToolStripMenuItem.Enabled = false;
                    label2.Enabled = false;
                    //  adjustmentLogToolStripMenuItem.Visible = true;
                    // warehouseStaffToolStripMenuItem.Visible = true;

                    // Limit inventory to adjustments only
                    productsItemsToolStripMenuItem.Visible = false;
                    break;

                default:
                    MessageBox.Show("Unknown role: " + role, "Access Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    break;
            }
        }


/// //////////////////////////////////////////////////////////////////////////////////////

        private void pictureBox1_Click(object sender, EventArgs e)
        {

        }


        private void label1_Click(object sender, EventArgs e)
        {
            frmHelp frmHelp = new frmHelp();
            frmHelp.Show();
        }

        private void frmMainMenu_Load(object sender, EventArgs e)
        {
            label1.Parent = pictureBox1;
            label2.Parent = pictureBox1;
            //label3.Parent = pictureBox1; 
           
            msMain.ForeColor = Color.White;
            msMain.Parent = pictureBox1;
        }

        private void accessControlToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //this.WindowState = FormWindowState.Maximized;
            foreach (Form child in this.MdiChildren)
            {
                child.Close();
            }
            
            pictureBox1.BringToFront();
            pictureBox1.Dock = DockStyle.Fill;
            msMain.Parent = pictureBox1;

        }


        private void label1_MouseHover(object sender, EventArgs e)
        {
            
        }

        private void label1_MouseEnter(object sender, EventArgs e)
        {
            label1.ForeColor = Color.Black;
           // label1.Font = ;
        }

        private void label1_MouseLeave(object sender, EventArgs e)
        {
            label1.ForeColor = Color.White;
            //label1.Fo = FontStyle.Underline;
        }

        private void msMain_MouseEnter(object sender, EventArgs e)
        {
           
        }

        private void msMain_MouseLeave(object sender, EventArgs e)
        {
            
        }

        private void accessControlToolStripMenuItem_MouseEnter(object sender, EventArgs e)
        {
            
        }

        private void dashboardToolStripMenuItem_Click(object sender, EventArgs e)
        {
            
            frmManagerDashboard managerDashboard = new frmManagerDashboard();
            frmSaleStaffDashboard salesStaffDashboard = new frmSaleStaffDashboard();
            frmWarehouseStaffDashboard warehouseStaffDashboard = new frmWarehouseStaffDashboard();
            switch (Globals.roleOfStaff)
            {
                case "owner":
                    FormSetup(managerDashboard);
                    break;

                case "manager":
                    FormSetup(managerDashboard);
                    break;

                case "sale":
                    FormSetup(salesStaffDashboard);
                    break;

                case "warehouse":
                    FormSetup(warehouseStaffDashboard);
                    break;

                default:
                    break;
            }
            
        }

        private void accessControlToolStripMenuItem_MouseLeave(object sender, EventArgs e)
        {
            
        }

        private void accessControlToolStripMenuItem_MouseHover(object sender, EventArgs e)
        {
            
        }

        private void accessControlToolStripMenuItem_DropDownOpened(object sender, EventArgs e)
        {
           
        }

        private void accessControlToolStripMenuItem_DropDownOpening(object sender, EventArgs e)
        {
            
        }

        private void msMain_MenuActivate(object sender, EventArgs e)
        {
            
        }

        private void msMain_MouseHover(object sender, EventArgs e)
        {
            
        }

        private void loginToolStripMenuItem_Click_1(object sender, EventArgs e)
        {
            frmLogin login = new frmLogin();
            FormSetup(login);
        }

        private void msMain_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {

        }

        private void reviewSalesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            frmSalesReview salesReview = new frmSalesReview();
            FormSetup(salesReview);
        }

        private void incentivesToolStripMenuItem1_Click_1(object sender, EventArgs e)
        {
            frmIncentives incentives = new frmIncentives();
            FormSetup(incentives);
        }

        private void manageStaffToolStripMenuItem_Click(object sender, EventArgs e)
        {
            frmManageStaff manageStaff = new frmManageStaff();
            FormSetup(manageStaff);
        }

        private void registerStaffToolStripMenuItem_Click(object sender, EventArgs e)
        {
            frmRegisterStaff registerStaff = new frmRegisterStaff();
            registerStaff.Show();
        }

        private void logOutToolStripMenuItem_Click_2(object sender, EventArgs e)
        {
            //Logout . show main menu & disable menustrip tabs, re-enable login.
            PerformLogout();
            foreach (Form child in this.MdiChildren)
            {
                child.Close();
            }

            pictureBox1.BringToFront();
            pictureBox1.Dock = DockStyle.Fill;
            msMain.Parent = pictureBox1;

            // Optionally show a message
            MessageBox.Show("You have been logged out.", "Logged Out", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void adjustmentLogToolStripMenuItem_Click(object sender, EventArgs e)
        {
            
        }

        private void msMain_ParentChanged(object sender, EventArgs e)
        {
            

        }

        private void helpToolStripMenuItem_Click(object sender, EventArgs e)
        {
            frmHelp help = new frmHelp();
            help.Show();
        }

        private void label2_Click(object sender, EventArgs e)
        {
            frmLogin frmLogin = new frmLogin();
            FormSetup(frmLogin);
        }

        private void label2_MouseEnter(object sender, EventArgs e)
        {
            label2.ForeColor = Color.Black;
        }

        private void label2_MouseHover(object sender, EventArgs e)
        {
            label2.ForeColor = Color.Black;
        }

        private void label2_MouseLeave(object sender, EventArgs e)
        {
            label2.ForeColor = Color.White;
        }
    }
}
