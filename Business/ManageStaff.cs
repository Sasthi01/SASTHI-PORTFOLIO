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
    public partial class frmManageStaff: Form
    {
        public frmManageStaff()
        {
            InitializeComponent();
            taStaff.Fill(dsStaff.Staff);
            groupBox1.Parent = pictureBox1;
        }

        private void txtStaffID_TextChanged(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(txtStaffID.Text))
            {
                taStaff.FillDashboard(dsStaff.Staff, Convert.ToInt32(txtStaffID.Text));
            }
            else
            {
                taStaff.Fill(dsStaff.Staff);
            }
        }

        private void txtStaffName_TextChanged(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(txtStaffName.Text))
            {
                taStaff.FilterByName(dsStaff.Staff, txtStaffName.Text);
            }
            else
            {
                taStaff.Fill(dsStaff.Staff);
            }
        }

        private void btnUpdate_Click(object sender, EventArgs e)
        {
            try
            {
                // Check if a staff member is selected
                if (txtStaffID.Text == "" && txtStaffName.Text == "" && cmbStatus.Text == "" && cmbRole.Text == "")
                {
                    MessageBox.Show("Please select a staff member to update.", "No Staff Selected", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                else
                {
                    if (cmbStatus.Text != "" && cmbRole.Text != "")
                    {
                        int staffId = Convert.ToInt32(dgvStaff.CurrentRow.Cells[0].Value);

                        if (!string.IsNullOrWhiteSpace(cmbStatus.Text))
                            taStaff.UpdateStatus(cmbStatus.Text, staffId);

                        if (!string.IsNullOrWhiteSpace(cmbRole.Text))
                            taStaff.UpdateRole(cmbRole.Text, staffId);

                        MessageBox.Show("Staff member updated successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);

                        //  Clear fields after successful update
                        txtStaffID.Clear();
                        txtStaffName.Clear();
                        cmbStatus.Text = string.Empty;
                        cmbRole.Text = string.Empty;
                        dgvStaff.ClearSelection();
                    }
                    else
                    {
                        MessageBox.Show("Please select a status OR role to update.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("An error occurred while updating the staff member: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            
        }

        private void dgvStaff_RowHeaderMouseDoubleClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            if (dgvStaff.CurrentRow != null)
            {
                
              cmbStatus.Text = dgvStaff.CurrentRow.Cells[14].Value.ToString();
              cmbRole.Text = dgvStaff.CurrentRow.Cells[6].Value.ToString();

            }
        }

        private void frmManageStaff_Load(object sender, EventArgs e)
        {

        }
    }
}
