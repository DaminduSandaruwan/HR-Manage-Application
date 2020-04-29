using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Data.SqlClient;

namespace HRManageApp
{
    public partial class Form1 : Form
    {
        int inEmpID = 0;
        bool isDefaultImage = true;
        string strConnectionString = @"Data Source=DESKTOP-M5MVHBR;Initial Catalog=HRManage;Integrated Security=True";
        string strPreviousImage = "";
        OpenFileDialog ofd = new OpenFileDialog();
        public Form1()
        {
            InitializeComponent();
        }

        private void tabPage1_Click(object sender, EventArgs e)
        {

        }

        private void btnReset_Click(object sender, EventArgs e)
        {
            Clear();
        }

        void PositionComboBoxFill()
        {
            using (SqlConnection sqlCon = new SqlConnection(strConnectionString))
            {
                sqlCon.Open();
                SqlDataAdapter sqlDa = new SqlDataAdapter("SELECT * FROM Position", sqlCon);
                DataTable dtb1 = new DataTable();
                sqlDa.Fill(dtb1);
                DataRow topItem = dtb1.NewRow();
                topItem[0] = 0;
                topItem[1] = "-Select-";
                dtb1.Rows.InsertAt(topItem, 0);
                cmbPosition.ValueMember = dgvcmbPosition.ValueMember = "PositionID";
                cmbPosition.DisplayMember = dgvcmbPosition.DisplayMember = "Position";
                cmbPosition.DataSource = dtb1;
                dgvcmbPosition.DataSource = dtb1.Copy();
            }

        }
        void Clear()
        {
            txtEmpCode.Text = txtEmpName.Text = "";
            cmbPosition.SelectedIndex = cmbGender.SelectedIndex = 0;
            dtpDOB.Value = DateTime.Now;
            rbtRegular.Checked = true;
            if (dgvEmpCompany.DataSource == null)
                dgvEmpCompany.Rows.Clear();
            else
                dgvEmpCompany.DataSource = (dgvEmpCompany.DataSource as DataTable).Clone();
             inEmpID = 0;
            btnSave.Text = "Save";
            btnDelete.Enabled = false;
            pbxPhoto.Image = Image.FromFile(Application.StartupPath + "\\Images\\defaultimage.png");
            isDefaultImage = true;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            PositionComboBoxFill();
            Clear();
        }

        private void btnImageBrowse_Click(object sender, EventArgs e)
        {
            ofd.Filter = "Images(.jpg, .png)|*.png;*.jpg";
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                pbxPhoto.Image = new Bitmap(ofd.FileName);
                isDefaultImage = false;
                strPreviousImage = "";
            }
        }

        private void btnImageClear_Click(object sender, EventArgs e)
        {
            pbxPhoto.Image = Image.FromFile(Application.StartupPath + "\\Images\\defaultimage.png");
            isDefaultImage = true;
            strPreviousImage = "";
        }
    }
}
