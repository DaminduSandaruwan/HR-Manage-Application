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
using System.IO;

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
            FillEmployeeDataGridView();
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

        private void btnSave_Click(object sender, EventArgs e)
        {
            if (ValidateMasterDetailForm())
            {
                int _EmpID = 0;
                using(SqlConnection sqlCon = new SqlConnection(strConnectionString))
                {
                    sqlCon.Open();
                    SqlCommand sqlCmd = new SqlCommand("EmployeeAddOrEdit", sqlCon);
                    sqlCmd.CommandType = CommandType.StoredProcedure;
                    sqlCmd.Parameters.AddWithValue("@EmpID", inEmpID);
                    sqlCmd.Parameters.AddWithValue("@EmpCode", txtEmpCode.Text.Trim());
                    sqlCmd.Parameters.AddWithValue("EmpName", txtEmpName.Text.Trim());
                    sqlCmd.Parameters.AddWithValue("@PositionID", Convert.ToInt32(cmbPosition.SelectedValue));
                    sqlCmd.Parameters.AddWithValue("@DOB", dtpDOB.Value);
                    sqlCmd.Parameters.AddWithValue("@Gender", cmbGender.Text);
                    sqlCmd.Parameters.AddWithValue("@State", rbtRegular.Checked ? "Regular" : "Contractual");
                    if (isDefaultImage)
                        sqlCmd.Parameters.AddWithValue("@ImagePath", DBNull.Value);
                    else if (inEmpID > 0 && strPreviousImage != "")
                        sqlCmd.Parameters.AddWithValue("@ImagePath", strPreviousImage);
                    else
                        sqlCmd.Parameters.AddWithValue("@ImagePath", SaveImage(ofd.FileName));
                    _EmpID = Convert.ToInt32(sqlCmd.ExecuteScalar());                    
                } 

                using (SqlConnection sqlCon = new SqlConnection(strConnectionString))
                {
                    sqlCon.Open();
                    foreach(DataGridViewRow dgvRow in dgvEmpCompany.Rows)
                    {
                        if (dgvRow.IsNewRow) break;
                        else
                        {
                            SqlCommand sqlCmd = new SqlCommand("EmpCompanyAddOrEdit", sqlCon);
                            sqlCmd.CommandType = CommandType.StoredProcedure;
                            sqlCmd.Parameters.AddWithValue("@EmpCmpID", Convert.ToInt32(dgvRow.Cells["dgvtxtEmpCompID"].Value==DBNull.Value ? "0" :dgvRow.Cells["dgvtxtEmpCompID"].Value));
                            sqlCmd.Parameters.AddWithValue("@EmpID", _EmpID);
                            sqlCmd.Parameters.AddWithValue("@CompanyName", dgvRow.Cells["dgvtxtCompanyName"].Value == DBNull.Value ? "" : dgvRow.Cells["dgvtxtCompanyName"].Value);
                            sqlCmd.Parameters.AddWithValue("@PositionID", Convert.ToInt32(dgvRow.Cells["dgvcmbPosition"].Value == DBNull.Value ? "0" : dgvRow.Cells["dgvcmbPosition"].Value));
                            sqlCmd.Parameters.AddWithValue("@ExpYear", Convert.ToInt32(dgvRow.Cells["dgvtxtYear"].Value == DBNull.Value ? "0" : dgvRow.Cells["dgvtxtYear"].Value));
                            sqlCmd.ExecuteNonQuery();
                        }

                    }
                }
                FillEmployeeDataGridView();
                Clear();
                MessageBox.Show("Submitted Sucessfully");
            }
        }
        
        bool ValidateMasterDetailForm()
        {
            bool _isValid = true;
            if (txtEmpName.Text.Trim() == "")
            {
                MessageBox.Show("Employee Name is required");
                _isValid = false;
            }
            return _isValid;
        }

        string SaveImage(string _imagePath)
        {
            string _fileName = Path.GetFileNameWithoutExtension(_imagePath);
            string _extention = Path.GetExtension(_imagePath);

            _fileName = _fileName.Length <= 15 ? _fileName : _fileName.Substring(0, 15);
            _fileName = _fileName + DateTime.Now.ToString("yymmssfff") + _extention;
            pbxPhoto.Image.Save(Application.StartupPath + "\\Images\\" + _fileName);
            return _fileName;
        }

        void FillEmployeeDataGridView()
        {
            using(SqlConnection sqlCon = new SqlConnection(strConnectionString))
            {
                sqlCon.Open();
                SqlDataAdapter sqlDa = new SqlDataAdapter("EmployeeViewAll", sqlCon);
                sqlDa.SelectCommand.CommandType = CommandType.StoredProcedure;
                DataTable dtb1 = new DataTable();
                sqlDa.Fill(dtb1);
                dgvEmployee.DataSource = dtb1;
                dgvEmployee.Columns[2].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
                dgvEmployee.Columns[3].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
                dgvEmployee.Columns[0].Visible = false;
            }
        }

        private void dgvEmployee_DoubleClick(object sender, EventArgs e)
        {
            if(dgvEmployee.CurrentRow.Index != -1)
            {
                DataGridViewRow _dgvCurrentRow = dgvEmployee.CurrentRow;
                inEmpID = Convert.ToInt32(_dgvCurrentRow.Cells[0].Value);
                using (SqlConnection sqlCon = new SqlConnection(strConnectionString))
                {
                    sqlCon.Open();
                    SqlDataAdapter sqlDa = new SqlDataAdapter("EmployeeViewByID", sqlCon);
                    sqlDa.SelectCommand.CommandType = CommandType.StoredProcedure;
                    sqlDa.SelectCommand.Parameters.AddWithValue("@EmpID", inEmpID);
                    DataSet ds = new DataSet();
                    sqlDa.Fill(ds);

                    //Master Fill
                    DataRow dr = ds.Tables[0].Rows[0];
                    txtEmpCode.Text = dr["EmpCode"].ToString();
                    txtEmpName.Text = dr["EmpName"].ToString();
                    cmbPosition.SelectedValue = Convert.ToInt32(dr["PositionID"].ToString());
                    dtpDOB.Value = Convert.ToDateTime(dr["DOB"].ToString());
                    cmbGender.Text = dr["Gender"].ToString();
                    if (dr["State"].ToString() == "Regular")
                        rbtRegular.Checked = true;
                    else
                        rbtContractual.Checked = true;
                    if (dr["ImagePath"] == DBNull.Value)
                    {
                        pbxPhoto.Image = new Bitmap(Application.StartupPath + "\\Images\\defaultimage.png");
                        isDefaultImage = true;
                    }
                    else
                    {
                        pbxPhoto.Image = new Bitmap(Application.StartupPath + "\\Images\\" + dr["ImagePath"].ToString());
                        strPreviousImage = dr["ImagePath"].ToString();
                        isDefaultImage = false;
                    }
                    dgvEmpCompany.AutoGenerateColumns = false;
                    dgvEmpCompany.DataSource = ds.Tables[1];
                    btnDelete.Enabled = true;
                    btnSave.Text = "Update";
                    tabControl.SelectedIndex = 0;
                }
            }
        }

        private void dgvEmpCompany_UserDeletingRow(object sender, DataGridViewRowCancelEventArgs e)
        {
            DataGridViewRow dgvRow = dgvEmpCompany.CurrentRow;
            if(dgvRow.Cells["dgvtxtEmpCompID"].Value != DBNull.Value)
            {
                if (MessageBox.Show("Are You Sure to Delete this Record ?", "HR Manage", MessageBoxButtons.YesNo) == DialogResult.Yes)
                {
                    using (SqlConnection sqlCon = new SqlConnection(strConnectionString))
                    {
                        sqlCon.Open();
                        SqlCommand sqlCmd = new SqlCommand("EmpCompanyDelete", sqlCon);
                        sqlCmd.CommandType = CommandType.StoredProcedure;
                        sqlCmd.Parameters.AddWithValue("@EmpCmpID", Convert.ToInt32(dgvRow.Cells["dgvtxtEmpCompID"].Value));
                        sqlCmd.ExecuteNonQuery();
                    }
                }
                else
                    e.Cancel = true;
            }
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            if(MessageBox.Show("Are You Sure to Delete this Record ? ","HR Manage", MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                using(SqlConnection sqlCon = new SqlConnection(strConnectionString))
                {
                    sqlCon.Open();
                    SqlCommand sqlCmd = new SqlCommand("EmployeeDelete", sqlCon);
                    sqlCmd.CommandType = CommandType.StoredProcedure;
                    sqlCmd.Parameters.AddWithValue("@EmpID", inEmpID);
                    sqlCmd.ExecuteNonQuery();
                    Clear();
                    FillEmployeeDataGridView();
                    MessageBox.Show("Deleted Successfully");
                };
            }
        }

        private void Form1_MaximumSizeChanged(object sender, EventArgs e)
        {

        }
    }
}
