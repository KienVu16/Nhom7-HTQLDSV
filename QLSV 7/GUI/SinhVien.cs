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
using ClosedXML.Excel;
using System.IO;

namespace QLSV_7.GUI
{
    public partial class SinhVien : Form
    {
        public SinhVien()
        {
            InitializeComponent();
        }
        string Nguon = @"Data Source=DESKTOP-5LSPDHV\SQLEXPRESS01;Initial Catalog=QLSV;Integrated Security=True";
        string Lenh = @"";
        SqlConnection KetNoi;
        SqlCommand ThucHien;
        SqlDataReader DocDuLieu;
        private void tableLayoutPanel1_Paint(object sender, PaintEventArgs e)
        {

        }


        private void SinhVien_Load(object sender, EventArgs e)
        {
            try { LoadMaTK(); HienThi(); } catch { }
            ccbMaTK.SelectedIndexChanged += ccbMaTK_SelectedIndexChanged;
        }
        void HienThi()
        {
            // Gỡ DataSource để được phép dùng Rows.Add
            dgvSinhVien.DataSource = null;
            dgvSinhVien.Rows.Clear();

            try
            {
                using (var conn = new SqlConnection(Nguon))
                using (var cmd = new SqlCommand(@"SELECT 
                                                        MaSV,
                                                        MaTK,
                                                        NgaySinh,
                                                        Ho,
                                                        Ten,
                                                        GioiTinh,
                                                        Email,
                                                        SoDienThoai,
                                                        DiaChi,
                                                        GhiChu
                                                    FROM SinhVien;", conn))
                {
                    conn.Open();
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            int index = dgvSinhVien.Rows.Add();
                            dgvSinhVien.Rows[index].Cells[0].Value = reader["MaSV"].ToString();
                            dgvSinhVien.Rows[index].Cells[1].Value = reader["MaTK"].ToString();
                            dgvSinhVien.Rows[index].Cells[2].Value = reader["Ho"].ToString();
                            dgvSinhVien.Rows[index].Cells[3].Value = reader["Ten"].ToString();
                            dgvSinhVien.Rows[index].Cells[4].Value = Convert.ToDateTime(reader["NgaySinh"]).ToString("yyyy-MM-dd");
                            int gioiTinh = Convert.ToInt32(reader["GioiTinh"]);
                            dgvSinhVien.Rows[index].Cells[5].Value = (gioiTinh == 0) ? "Nam" : "Nữ";
                            dgvSinhVien.Rows[index].Cells[6].Value = reader["Email"].ToString();
                            dgvSinhVien.Rows[index].Cells[7].Value = reader["SoDienThoai"].ToString();
                            dgvSinhVien.Rows[index].Cells[8].Value = reader["DiaChi"].ToString();
                            dgvSinhVien.Rows[index].Cells[9].Value = reader["GhiChu"].ToString();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi hiển thị dữ liệu: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        // Event handler cho checkbox Nam
        private void cbNam_CheckedChanged(object sender, EventArgs e)
        {
            if (cbNam.Checked)
            {
                cbNu.Checked = false; // Bỏ chọn checkbox Nữ
            }
        }

        // Event handler cho checkbox Nữ
        private void cbNu_CheckedChanged(object sender, EventArgs e)
        {
            if (cbNu.Checked)
            {
                cbNam.Checked = false; // Bỏ chọn checkbox Nam
            }
        }
     
        private void btnThem_Click(object sender, EventArgs e)
        {
            // Validate bắt buộc theo schema
            if (string.IsNullOrWhiteSpace(txtMaSV.Text))
            {
                MessageBox.Show("Vui lòng nhập Mã SV.", "Thiếu thông tin", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            var maTkValue = ccbMaTK?.SelectedValue?.ToString() ?? string.Empty;
            if (string.IsNullOrWhiteSpace(maTkValue)) { MessageBox.Show("Vui lòng chọn Mã TK.", "Thiếu thông tin", MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }
            if (string.IsNullOrWhiteSpace(txtHo.Text))
            {
                MessageBox.Show("Vui lòng nhập Họ.", "Thiếu thông tin", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            if (string.IsNullOrWhiteSpace(txtTen.Text))
            {
                MessageBox.Show("Vui lòng nhập Tên.", "Thiếu thông tin", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            if (!cbNam.Checked && !cbNu.Checked)
            {
                MessageBox.Show("Vui lòng chọn Giới tính.", "Thiếu thông tin", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            if (string.IsNullOrWhiteSpace(txbEmail.Text))
            {
                MessageBox.Show("Vui lòng nhập Email.", "Thiếu thông tin", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            if (string.IsNullOrWhiteSpace(txbSoDienThoai.Text))
            {
                MessageBox.Show("Vui lòng nhập Số điện thoại.", "Thiếu thông tin", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            const string insertSql = @"INSERT INTO SinhVien
                                                (MaSV, MaTK, NgaySinh, Ho, Ten, GioiTinh, Email, SoDienThoai, DiaChi, GhiChu)
                                                VALUES
                                                (@MaSV, @MaTK, @NgaySinh, @Ho, @Ten, @GioiTinh, @Email, @SoDienThoai, @DiaChi, @GhiChu)";

            try
            {
                using (var conn = new SqlConnection(Nguon))
                {
                    conn.Open();

                    // Kiểm tra trùng Mã SV trước khi thêm
                    using (var check = new SqlCommand("SELECT COUNT(1) FROM SinhVien WHERE MaSV = @MaSV", conn))
                    {
                        check.Parameters.Add("@MaSV", SqlDbType.NVarChar, 50).Value = (txtMaSV.Text ?? string.Empty).Trim();
                        int exists = Convert.ToInt32(check.ExecuteScalar());
                        if (exists > 0)
                        {
                            MessageBox.Show("Mã SV đã tồn tại. Vui lòng nhập mã khác.", "Trùng khóa", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            return;
                        }
                    }

                    using (var cmd = new SqlCommand(insertSql, conn))
                    {
                        cmd.Parameters.Add("@MaSV", SqlDbType.NVarChar, 50).Value = (txtMaSV.Text ?? string.Empty).Trim();
                        cmd.Parameters.Add("@MaTK", SqlDbType.NVarChar, 20).Value = (maTkValue ?? string.Empty).Trim();
                        cmd.Parameters.Add("@NgaySinh", SqlDbType.Date).Value = dtpNgaySinh.Value.Date;
                        cmd.Parameters.Add("@Ho", SqlDbType.NVarChar, 50).Value = (txtHo.Text ?? string.Empty).Trim();
                        cmd.Parameters.Add("@Ten", SqlDbType.NVarChar, 50).Value = (txtTen.Text ?? string.Empty).Trim();
                        cmd.Parameters.Add("@GioiTinh", SqlDbType.Bit).Value = cbNu.Checked ? 1 : 0; // 0=Nam,1=Nữ
                        cmd.Parameters.Add("@Email", SqlDbType.NVarChar, 100).Value = (txbEmail.Text ?? string.Empty).Trim();
                        cmd.Parameters.Add("@SoDienThoai", SqlDbType.VarChar, 15).Value = (txbSoDienThoai.Text ?? string.Empty).Trim();
                        cmd.Parameters.Add("@DiaChi", SqlDbType.NVarChar, 255).Value = (txbDiaChi.Text ?? string.Empty).Trim();
                        cmd.Parameters.Add("@GhiChu", SqlDbType.NVarChar, 255).Value = (txbGhiChu.Text ?? string.Empty).Trim();

                        cmd.ExecuteNonQuery();
                    }
                }

                MessageBox.Show("Thêm sinh viên thành công!", "Thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);
                HienThi();

                // Clear form
                txtMaSV.Clear();
                if (ccbMaTK != null) ccbMaTK.SelectedIndex = ccbMaTK.Items.Count > 0 ? 0 : -1;
                dtpNgaySinh.Value = DateTime.Now;
                txtHo.Clear();
                txtTen.Clear();
                cbNam.Checked = false;
                cbNu.Checked = false;
                txbEmail.Clear();
                txbSoDienThoai.Clear();
                txbDiaChi.Clear();
                txbGhiChu.Clear();
            }
            catch (SqlException ex) when (ex.Number == 2627 || ex.Number == 2601)
            {
                MessageBox.Show("Mã SV đã tồn tại. Vui lòng nhập mã khác.", "Trùng khóa", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi thêm sinh viên: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void dgvSinhVien_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (dgvSinhVien.CurrentRow == null) return;

            // Lấy MaSV từ cột 0
            if (dgvSinhVien.CurrentRow.Cells[0].Value != null)
                txtMaSV.Text = dgvSinhVien.CurrentRow.Cells[0].Value.ToString();

            var maTk = dgvSinhVien.CurrentRow.Cells[1].Value?.ToString() ?? string.Empty;
            if (!string.IsNullOrEmpty(maTk)) ccbMaTK.SelectedValue = maTk;
            txtHo.Text = dgvSinhVien.CurrentRow.Cells[2].Value.ToString();
            txtTen.Text = dgvSinhVien.CurrentRow.Cells[3].Value.ToString();
            dtpNgaySinh.Value = Convert.ToDateTime(dgvSinhVien.CurrentRow.Cells[4].Value);
            string gioiTinh = dgvSinhVien.CurrentRow.Cells[5].Value.ToString();
            if (gioiTinh == "Nam")
            {
                cbNam.Checked = true;
                cbNu.Checked = false;
            }
            else
            {
                cbNam.Checked = false;
                cbNu.Checked = true;
            }
            txbEmail.Text = dgvSinhVien.CurrentRow.Cells[6].Value.ToString();
            txbSoDienThoai.Text = dgvSinhVien.CurrentRow.Cells[7].Value.ToString();
            txbDiaChi.Text = dgvSinhVien.CurrentRow.Cells[8].Value.ToString();
            txbGhiChu.Text = dgvSinhVien.CurrentRow.Cells[9].Value.ToString();

        }

        private void btnSua_Click(object sender, EventArgs e)
        {
            string maSvInput = (txtMaSV.Text ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(maSvInput))
            {
                if (dgvSinhVien.CurrentRow != null && dgvSinhVien.CurrentRow.Cells[0].Value != null)
                {
                    maSvInput = dgvSinhVien.CurrentRow.Cells[0].Value.ToString().Trim();
                }
            }

            if (string.IsNullOrWhiteSpace(maSvInput))
            {
                MessageBox.Show("Vui lòng chọn hoặc nhập Mã SV cần sửa.", "Thiếu thông tin", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            const string updateSql = @"UPDATE SinhVien
                                                    SET
                                                        MaSV = @MaSV,
                                                        MaTK = @MaTK,
                                                        Ho = @Ho,
                                                        Ten = @Ten,
                                                        NgaySinh = @NgaySinh,
                                                        GioiTinh = @GioiTinh,
                                                        Email = @Email,
                                                        SoDienThoai = @SoDienThoai,
                                                        DiaChi = @DiaChi,
                                                        GhiChu = @GhiChu
                                                    WHERE MaSV = @MaSV";

            try
            {
                using (var conn = new SqlConnection(Nguon))
                {
                    conn.Open();

                    // Kiểm tra tồn tại MaSV trước khi cập nhật
                    using (var check = new SqlCommand("SELECT COUNT(1) FROM SinhVien WHERE MaSV = @MaSV", conn))
                    {
                        check.Parameters.Add("@MaSV", SqlDbType.NVarChar, 50).Value = maSvInput;
                        int exists = Convert.ToInt32(check.ExecuteScalar());
                        if (exists == 0)
                        {
                            MessageBox.Show($"Không tìm thấy Mã SV: '{maSvInput}' trong cơ sở dữ liệu.", "Không tồn tại", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            return;
                        }
                    }

                    using (var cmd = new SqlCommand(updateSql, conn))
                    {
                        cmd.Parameters.Add("@MaSV", SqlDbType.NVarChar, 50).Value = maSvInput;
                        cmd.Parameters.Add("@MaTK", SqlDbType.NVarChar, 20).Value = (ccbMaTK?.SelectedValue?.ToString() ?? string.Empty).Trim();
                        cmd.Parameters.Add("@NgaySinh", SqlDbType.Date).Value = dtpNgaySinh.Value.Date;
                        cmd.Parameters.Add("@Ho", SqlDbType.NVarChar, 50).Value = (txtHo.Text ?? string.Empty).Trim();
                        cmd.Parameters.Add("@Ten", SqlDbType.NVarChar, 50).Value = (txtTen.Text ?? string.Empty).Trim();
                        cmd.Parameters.Add("@GioiTinh", SqlDbType.Bit).Value = cbNu.Checked ? 1 : 0; // 0=Nam,1=Nữ
                        cmd.Parameters.Add("@Email", SqlDbType.NVarChar, 100).Value = (txbEmail.Text ?? string.Empty).Trim();
                        cmd.Parameters.Add("@SoDienThoai", SqlDbType.VarChar, 15).Value = (txbSoDienThoai.Text ?? string.Empty).Trim();
                        cmd.Parameters.Add("@DiaChi", SqlDbType.NVarChar, 255).Value = (txbDiaChi.Text ?? string.Empty).Trim();
                        cmd.Parameters.Add("@GhiChu", SqlDbType.NVarChar, 255).Value = (txbGhiChu.Text ?? string.Empty).Trim();

                        int affected = cmd.ExecuteNonQuery();
                        if (affected > 0)
                        {
                            MessageBox.Show("Cập nhật sinh viên thành công!", "Thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            HienThi();
                        }
                        else
                        {
                            MessageBox.Show("Không tìm thấy Mã SV để cập nhật (0 dòng bị ảnh hưởng).", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi cập nhật sinh viên: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnXoa_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtMaSV.Text))
            {
                MessageBox.Show("Vui lòng chọn hoặc nhập Mã SV cần xóa.", "Thiếu thông tin", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var xacNhan = MessageBox.Show($"Bạn có chắc muốn xóa sinh viên có Mã SV: {txtMaSV.Text}?", "Xác nhận xóa", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (xacNhan != DialogResult.Yes)
            {
                return;
            }

            KetNoi = new SqlConnection(Nguon);
            Lenh = "DELETE FROM SinhVien WHERE MaSV = @MaSV";

            ThucHien = new SqlCommand(Lenh, KetNoi);
            // MaSV là NVARCHAR(50)
            var pMaSv = ThucHien.Parameters.Add("@MaSV", SqlDbType.NVarChar, 50);
            pMaSv.Value = (txtMaSV.Text ?? string.Empty).Trim();

            try
            {
                KetNoi.Open();
                int soDong = ThucHien.ExecuteNonQuery();
                if (soDong > 0)
                {
                    MessageBox.Show("Xóa sinh viên thành công!", "Thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    HienThi();
                    // Xóa lựa chọn hiện tại và làm sạch form
                    dgvSinhVien.ClearSelection();
                    txtMaSV.Clear();
                }
                else
                {
                    MessageBox.Show("Không tìm thấy Mã SV để xóa.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi xóa sinh viên: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                if (KetNoi.State == ConnectionState.Open)
                    KetNoi.Close();
            }
        }

        private void btnXuatFile_Click(object sender, EventArgs e)
        {
            if (dgvSinhVien.Rows.Count == 0)
            {
                MessageBox.Show("Không có dữ liệu để xuất!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            using (SaveFileDialog sfd = new SaveFileDialog() { Filter = "CSV file|*.csv" })
            {
                if (sfd.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        using (StreamWriter sw = new StreamWriter(sfd.FileName, false, new UTF8Encoding(true)))
                        {
                            // ===== Tiêu đề =====
                            sw.WriteLine("DANH SÁCH SINH VIÊN");
                            sw.WriteLine();

                            // ===== Header =====
                            for (int i = 0; i < dgvSinhVien.Columns.Count; i++)
                            {
                                sw.Write(EscapeCsv(dgvSinhVien.Columns[i].HeaderText));
                                if (i < dgvSinhVien.Columns.Count - 1) sw.Write(",");
                            }
                            sw.WriteLine();

                            // ===== Dữ liệu =====
                            foreach (DataGridViewRow row in dgvSinhVien.Rows)
                            {
                                if (row.IsNewRow) continue; // bỏ qua dòng trống cuối

                                for (int j = 0; j < dgvSinhVien.Columns.Count; j++)
                                {
                                    var value = row.Cells[j].Value;

                                    // Xử lý ngày sinh
                                    if (dgvSinhVien.Columns[j].Name == "NgaySinh" && value != null &&
                                        DateTime.TryParse(value.ToString(), out DateTime dateValue))
                                    {
                                        value = dateValue.ToString("dd/MM/yyyy");
                                    }

                                    // Xử lý số điện thoại: ép sang chuỗi, giữ nguyên 0 ở đầu
                                    if (dgvSinhVien.Columns[j].Name == "SoDienThoai" && value != null)
                                    {
                                        value = "'" + value.ToString();
                                        // thêm dấu nháy đơn để Excel hiểu là chuỗi, không cắt số 0
                                    }

                                    string text = value?.ToString() ?? "";
                                    sw.Write(EscapeCsv(text));
                                    if (j < dgvSinhVien.Columns.Count - 1) sw.Write(",");
                                }
                                sw.WriteLine();
                            }
                        }

                        MessageBox.Show("Xuất CSV thành công!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Lỗi: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        /// <summary>
        /// Escape dữ liệu CSV theo chuẩn RFC4180
        /// </summary>
        private string EscapeCsv(string text)
        {
            if (string.IsNullOrEmpty(text)) return "";
            if (text.Contains(",") || text.Contains("\n") || text.Contains("\""))
                return "\"" + text.Replace("\"", "\"\"") + "\"";
            return text;
        }

        private void btnThoat_Click(object sender, EventArgs e)
        {
            
            this.Close();
        }
        private void ccbMaTK_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                var selected = ccbMaTK.SelectedValue?.ToString();
                if (string.IsNullOrWhiteSpace(selected) || selected == "System.Data.DataRowView") return;

                dgvSinhVien.DataSource = null;
                dgvSinhVien.Rows.Clear();

                using (var conn = new SqlConnection(Nguon))
                using (var cmd = new SqlCommand(@"SELECT 
                                                        SV.MaSV,
                                                        SV.MaTK,
                                                        SV.Ho,
                                                        SV.Ten,
                                                        SV.NgaySinh,
                                                        SV.GioiTinh,
                                                        SV.Email,
                                                        SV.SoDienThoai,
                                                        SV.DiaChi,
                                                        SV.GhiChu
                                                    FROM SinhVien SV
                                                    WHERE SV.MaTK = @MaTK
                                                    ORDER BY SV.MaSV;", conn))
                {
                    cmd.Parameters.Add("@MaTK", SqlDbType.NVarChar, 20).Value = selected;
                    conn.Open();
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            int index = dgvSinhVien.Rows.Add();
                            dgvSinhVien.Rows[index].Cells[0].Value = reader["MaSV"].ToString();
                            dgvSinhVien.Rows[index].Cells[1].Value = reader["MaTK"].ToString();
                            dgvSinhVien.Rows[index].Cells[2].Value = reader["Ho"].ToString();
                            dgvSinhVien.Rows[index].Cells[3].Value = reader["Ten"].ToString();
                            dgvSinhVien.Rows[index].Cells[4].Value = Convert.ToDateTime(reader["NgaySinh"]).ToString("yyyy-MM-dd");
                            int gioiTinh = Convert.ToInt32(reader["GioiTinh"]);
                            dgvSinhVien.Rows[index].Cells[5].Value = (gioiTinh == 0) ? "Nam" : "Nữ";
                            dgvSinhVien.Rows[index].Cells[6].Value = reader["Email"].ToString();
                            dgvSinhVien.Rows[index].Cells[7].Value = reader["SoDienThoai"].ToString();
                            dgvSinhVien.Rows[index].Cells[8].Value = reader["DiaChi"].ToString();
                            dgvSinhVien.Rows[index].Cells[9].Value = reader["GhiChu"].ToString();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi lọc theo Mã TK: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LoadMaTK()
        {
            using (var conn = new SqlConnection(Nguon))
            using (var cmd = new SqlCommand("SELECT MaTK FROM TaiKhoan ORDER BY MaTK;", conn))
            using (var da = new SqlDataAdapter(cmd))
            {
                var dt = new DataTable();
                conn.Open();
                da.Fill(dt);
                ccbMaTK.DataSource = dt;
                ccbMaTK.DisplayMember = "MaTK";
                ccbMaTK.ValueMember = "MaTK";
            }
        }


    }
}
