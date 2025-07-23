using LGPACKAGING_POS_SYSTEM.Business;
using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace LGPACKAGING_POS_SYSTEM.Interface
{
    public partial class frmLogin: Form
    {
        frmMainMenu mainMenuReal= new frmMainMenu();
        public frmLogin()
        {
            InitializeComponent();
            panel1.Parent = pictureBox1;
            mainMenuReal.msMain.BackColor = Color.Black;

        }

        private void button2_Click(object sender, EventArgs e)
        {
            txtUsername.Clear();
            txtPassword.Clear();

        }

        private void btnLogin_Click(object sender, EventArgs e)
        {

            dsStaff.EnforceConstraints = false;
            if (!int.TryParse(txtUsername.Text.Trim(), out int staffId))
            {
                MessageBox.Show("Please enter a valid numeric Staff ID.");
                return;
            }

           taStaff.CheckLogin(dsStaff.Staff, staffId, txtPassword.Text);
            if (dsStaff.Staff.Rows.Count > 0)
            {
                string status = dsStaff.Staff.Rows[0]["status"].ToString().Trim().ToLower(); // assuming column name is 'Status'
                if (status == "inactive")
                {
                    MessageBox.Show("Access Denied.");
                    return;
                }
                else
                {
                    string Fname = dsStaff.Staff.Rows[0]["firstName"].ToString(); // assuming column 1 is Fname
                    string Lname = dsStaff.Staff.Rows[0]["lastName"].ToString();
                    string role = dsStaff.Staff.Rows[0]["role"].ToString().Trim().ToLower();   // 👈 assuming column name is 'Role'
                    string imageLoc = dsStaff.Staff.Rows[0]["image"].ToString(); // assuming column name is image
                    Globals.imageOfStaff = imageLoc; // Store the image path in a static variable
                    MessageBox.Show("Login Successful...! Welcome " + Fname);
                    int tempStaffPk = Convert.ToInt32(dsStaff.Staff.Rows[0]["staff_ID"]); // assuming column 0 is StaffID

                    Globals.staffPK = tempStaffPk; // Store the staff ID in a static variable
                    Globals.nameOfStaff = Fname + " " + Lname;
                    frmLogin loginForm = new frmLogin();
                    //frmMainMenu main = new frmMainMenu();
                    //loginForm.MdiParent = main; // Ensure mainMenuInstance is initialized  
                    //loginForm.Show();
                    Globals.roleOfStaff = role; // Store the role in a static variable

                    // Pass the role to the MainMenu
                    frmMainMenu mainMenu = (frmMainMenu)this.MdiParent;
                    mainMenu.SetPermissionsByRole(role);
                    //this.Close();
                    //// Open the main menu form  
                    if (role == "manager" || role == "owner")
                    {
                        frmManagerDashboard managerDashboard = new frmManagerDashboard();
                        managerDashboard.MdiParent = this.MdiParent; // Set the MDI parent
                        managerDashboard.Show();
                    }
                    else if (role == "warehouse")
                    {
                        frmWarehouseStaffDashboard warehouseDashboard = new frmWarehouseStaffDashboard();
                        warehouseDashboard.MdiParent = this.MdiParent; // Set the MDI parent
                        warehouseDashboard.Show();
                    }
                    else if (role == "sale")
                    {
                        frmSaleStaffDashboard saleStaffDashboard = new frmSaleStaffDashboard();
                        saleStaffDashboard.MdiParent = this.MdiParent; // Set the MDI parent
                        saleStaffDashboard.Show();
                    }

                    mainMenuReal.loggedInAsToolStripMenuItem.Text += Fname + " " + Lname;

                } 

            }
            else
            {
                MessageBox.Show("Access Denied. Try Again");
            }
        }

        private bool passwordVisible = false;
        private void btnTogglePassword_Click(object sender, EventArgs e)
        {
            passwordVisible = !passwordVisible;
            txtPassword.UseSystemPasswordChar = !passwordVisible;
        }

        public static class Globals
        {
            public static int staffPK { get; set; } // Static variable to hold the staff ID  
            public static string roleOfStaff { get; set; }
            public static string nameOfStaff { get; set; }
            public static string imageOfStaff { get; set; } // Static variable to hold the staff image path
        }

        private void frmLogin_Load(object sender, EventArgs e)
        {
           
        }

        private void txtUsername_TextChanged(object sender, EventArgs e)
        {

        }
    }
}
