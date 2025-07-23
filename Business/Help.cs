using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;

namespace LGPACKAGING_POS_SYSTEM.Business
{
    public partial class frmHelp: Form
    {
        public frmHelp()
        {
            InitializeComponent();
        }

        private void frmHelp_Load(object sender, EventArgs e)
        {
            string fileName1 = "customerRegistrationHelp.txt";
            string fileName2 = "inventoryHelp.txt";
            string fileName3 = "salesFormHelp.txt";
            string fileName4 = "staffRegistrationHelp.txt";
            string filePath1 = Path.Combine(Application.StartupPath, fileName1);
            string filePath2 = Path.Combine(Application.StartupPath, fileName2);
            string filePath3 = Path.Combine(Application.StartupPath, fileName3);
            string filePath4 = Path.Combine(Application.StartupPath, fileName4);

            if (File.Exists(filePath1))
            {
                rtxtCustReg.Text = File.ReadAllText(filePath1);
            }
            else
            {
                MessageBox.Show("File not found:\n" + filePath1);
            }

            if (File.Exists(filePath2))
            {
                rtxtInventory.Text = File.ReadAllText(filePath2);
            }
            else
            {
                MessageBox.Show("File not found:\n" + filePath2);
            }
            if (File.Exists(filePath3))
            {
                rtxtNewSale.Text = File.ReadAllText(filePath3);
            }
            else
            {
                MessageBox.Show("File not found:\n" + filePath3);
            }
            if (File.Exists(filePath4))
            {
                rtxtStaffReg.Text = File.ReadAllText(filePath4);
            }
            else
            {
                MessageBox.Show("File not found:\n" + filePath4);
            }

        }
    }
}
