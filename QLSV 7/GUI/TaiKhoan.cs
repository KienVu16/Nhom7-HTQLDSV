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

namespace QLSV_7.GUI
{
    public partial class TaiKhoan : Form
    {
        public TaiKhoan()
        {
            InitializeComponent();
        }

        string Nguon = @"Data Source=DESKTOP-5LSPDHV\SQLEXPRESS01;Initial Catalog=QLSV;Integrated Security=True";

        private void HienThi()
        {
            dgvTaiKhoan.DataSource = null;
            dgvTaiKhoan.Rows.Clear();

            try
            {
                using (var conn = new SqlConnection(Nguon))
                using (var cmd = new SqlCommand(@"SELECT MaTK, TenDangNhap, MatKhau, VaiTro, NgayTao FROM TaiKhoan;", conn))
                {
                    conn.Open();
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            int index = dgvTaiKhoan.Rows.Add();
                            dgvTaiKhoan.Rows[index].Cells[0].Value = reader["MaTK"].ToString();
                            dgvTaiKhoan.Rows[index].Cells[1].Value = reader["TenDangNhap"].ToString();
                            dgvTaiKhoan.Rows[index].Cells[2].Value = reader["MatKhau"].ToString();
                            dgvTaiKhoan.Rows[index].Cells[3].Value = reader["VaiTro"].ToString();
                            dgvTaiKhoan.Rows[index].Cells[4].Value = Convert.ToDateTime(reader["NgayTao"]).ToString("yyyy-MM-dd");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi hiển thị dữ liệu: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void TaiKhoan_Load(object sender, EventArgs e)
        {
            try { HienThi(); } catch { }
            dgvTaiKhoan.CellClick += dgvTaiKhoan_CellClick;
        }

        private void dgvTaiKhoan_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;
            var row = dgvTaiKhoan.Rows[e.RowIndex];

            txtMaTK.Text = row.Cells.Count > 0 ? row.Cells[0].Value?.ToString() ?? string.Empty : string.Empty;
            txtTenDangNhap.Text = row.Cells.Count > 1 ? row.Cells[1].Value?.ToString() ?? string.Empty : string.Empty;
            txtMatKhau.Text = row.Cells.Count > 2 ? row.Cells[2].Value?.ToString() ?? string.Empty : string.Empty;
            txtVaiTro.Text = row.Cells.Count > 3 ? row.Cells[3].Value?.ToString() ?? string.Empty : string.Empty;
            var ngayTao = row.Cells.Count > 4 ? row.Cells[4].Value?.ToString() ?? string.Empty : string.Empty;
            if (DateTime.TryParse(ngayTao, out DateTime d)) dtpNgayTao.Value = d; else dtpNgayTao.Value = DateTime.Today;
        }

        private void btnThem_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtMaTK.Text)) { MessageBox.Show("Vui lòng nhập Mã TK."); return; }
            if (string.IsNullOrWhiteSpace(txtTenDangNhap.Text)) { MessageBox.Show("Vui lòng nhập Tên đăng nhập."); return; }
            if (string.IsNullOrWhiteSpace(txtMatKhau.Text)) { MessageBox.Show("Vui lòng nhập Mật khẩu."); return; }
            if (string.IsNullOrWhiteSpace(txtVaiTro.Text)) { MessageBox.Show("Vui lòng nhập Vai trò."); return; }

            const string insertSql = @"INSERT INTO TaiKhoan (MaTK, TenDangNhap, MatKhau, VaiTro, NgayTao)
                                       VALUES (@MaTK, @TenDangNhap, @MatKhau, @VaiTro, @NgayTao)";
            try
            {
                using (var conn = new SqlConnection(Nguon))
                {
                    conn.Open();
                    using (var check = new SqlCommand("SELECT COUNT(1) FROM TaiKhoan WHERE MaTK = @MaTK", conn))
                    {
                        check.Parameters.Add("@MaTK", SqlDbType.NVarChar, 50).Value = (txtMaTK.Text ?? string.Empty).Trim();
                        int exists = Convert.ToInt32(check.ExecuteScalar());
                        if (exists > 0)
                        {
                            MessageBox.Show("Mã TK đã tồn tại. Vui lòng nhập mã khác.", "Trùng khóa", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            return;
                        }
                    }

                    using (var cmd = new SqlCommand(insertSql, conn))
                    {
                        cmd.Parameters.Add("@MaTK", SqlDbType.NVarChar, 50).Value = (txtMaTK.Text ?? string.Empty).Trim();
                        cmd.Parameters.Add("@TenDangNhap", SqlDbType.NVarChar, 100).Value = (txtTenDangNhap.Text ?? string.Empty).Trim();
                        cmd.Parameters.Add("@MatKhau", SqlDbType.NVarChar, 100).Value = (txtMatKhau.Text ?? string.Empty).Trim();
                        cmd.Parameters.Add("@VaiTro", SqlDbType.NVarChar, 50).Value = (txtVaiTro.Text ?? string.Empty).Trim();
                        cmd.Parameters.Add("@NgayTao", SqlDbType.Date).Value = dtpNgayTao.Value.Date;
                        cmd.ExecuteNonQuery();
                    }
                }

                MessageBox.Show("Thêm tài khoản thành công!", "Thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);
                HienThi();
                ClearForm();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi thêm tài khoản: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnSua_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtMaTK.Text))
            {
                MessageBox.Show("Vui lòng chọn hoặc nhập Mã TK cần sửa.");
                return;
            }
            if (string.IsNullOrWhiteSpace(txtTenDangNhap.Text)) { MessageBox.Show("Vui lòng nhập Tên đăng nhập."); return; }
            if (string.IsNullOrWhiteSpace(txtMatKhau.Text)) { MessageBox.Show("Vui lòng nhập Mật khẩu."); return; }
            if (string.IsNullOrWhiteSpace(txtVaiTro.Text)) { MessageBox.Show("Vui lòng nhập Vai trò."); return; }

            const string updateSql = @"UPDATE TaiKhoan
                                        SET TenDangNhap = @TenDangNhap,
                                            MatKhau = @MatKhau,
                                            VaiTro = @VaiTro,
                                            NgayTao = @NgayTao
                                      WHERE MaTK = @MaTK";
            try
            {
                using (var conn = new SqlConnection(Nguon))
                using (var cmd = new SqlCommand(updateSql, conn))
                {
                    cmd.Parameters.Add("@MaTK", SqlDbType.NVarChar, 50).Value = (txtMaTK.Text ?? string.Empty).Trim();
                    cmd.Parameters.Add("@TenDangNhap", SqlDbType.NVarChar, 100).Value = (txtTenDangNhap.Text ?? string.Empty).Trim();
                    cmd.Parameters.Add("@MatKhau", SqlDbType.NVarChar, 100).Value = (txtMatKhau.Text ?? string.Empty).Trim();
                    cmd.Parameters.Add("@VaiTro", SqlDbType.NVarChar, 50).Value = (txtVaiTro.Text ?? string.Empty).Trim();
                    cmd.Parameters.Add("@NgayTao", SqlDbType.Date).Value = dtpNgayTao.Value.Date;

                    conn.Open();
                    int affected = cmd.ExecuteNonQuery();
                    if (affected == 0)
                    {
                        MessageBox.Show("Không tìm thấy Mã TK để cập nhật.");
                        return;
                    }
                }
                MessageBox.Show("Cập nhật tài khoản thành công!", "Thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);
                HienThi();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi cập nhật tài khoản: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnXoa_Click(object sender, EventArgs e)
        {
            string maTk = (txtMaTK.Text ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(maTk))
            {
                MessageBox.Show("Vui lòng chọn Mã TK để xóa.");
                return;
            }
            var confirm = MessageBox.Show($"Bạn có chắc muốn xóa tài khoản: {maTk}?", "Xác nhận xóa", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (confirm != DialogResult.Yes) return;

            const string deleteSql = "DELETE FROM TaiKhoan WHERE MaTK = @MaTK";
            try
            {
                using (var conn = new SqlConnection(Nguon))
                using (var cmd = new SqlCommand(deleteSql, conn))
                {
                    cmd.Parameters.Add("@MaTK", SqlDbType.NVarChar, 50).Value = maTk;
                    conn.Open();
                    int affected = cmd.ExecuteNonQuery();
                    if (affected == 0)
                    {
                        MessageBox.Show("Không tìm thấy Mã TK để xóa.");
                        return;
                    }
                }
                MessageBox.Show("Xóa tài khoản thành công!", "Thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);
                HienThi();
                ClearForm();
            }
            catch (SqlException ex)
            {
                MessageBox.Show("Không thể xóa do ràng buộc dữ liệu: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi xóa tài khoản: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }


        private void btnThoat_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private static string EscapeCsv(string input)
        {
            if (input.IndexOfAny(new[] { ',', '"', '\n', '\r' }) >= 0)
            {
                return '"' + input.Replace("\"", "\"\"") + '"';
            }
            return input;
        }

        private void ClearForm()
        {
            txtMaTK.Clear();
            txtTenDangNhap.Clear();
            txtMatKhau.Clear();
            txtVaiTro.Clear();
            dtpNgayTao.Value = DateTime.Today;
        }

        private void toolStripMenuItem6_Click(object sender, EventArgs e)
        {
            TaiKhoan tk = new TaiKhoan();
            this.Hide();
            tk.ShowDialog();
            this.Show();
        }

        private void quảnLýSinhViênToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        private void quảnLýGiáoViênToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        private void quảnLýMônHọcToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        private void quảnLýĐiểmToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        private void quảnLýĐăngKýHọcToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        private void quảnLýTàiKhoảnToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        private void toolStripMenuItem6_Click_1(object sender, EventArgs e)
        {

        }

        private void đăngXuấtToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        private void btnXuatFile_Click(object sender, EventArgs e)
        {
            if (dgvTaiKhoan.Rows.Count > 0)
            {
                using (SaveFileDialog sfd = new SaveFileDialog() { Filter = "CSV file|*.csv" })
                {
                    if (sfd.ShowDialog() == DialogResult.OK)
                    {
                        try
                        {
                            using (StreamWriter sw = new StreamWriter(sfd.FileName, false, Encoding.UTF8))
                            {
                                // ===== Thêm tiêu đề =====
                                sw.WriteLine("DANH SÁCH SINH VIÊN");

                                // Ghi header
                                for (int i = 0; i < dgvTaiKhoan.Columns.Count; i++)
                                {
                                    sw.Write(dgvTaiKhoan.Columns[i].HeaderText);
                                    if (i < dgvTaiKhoan.Columns.Count - 1)
                                        sw.Write(",");
                                }
                                sw.WriteLine();

                                // Ghi dữ liệu
                                for (int i = 0; i < dgvTaiKhoan.Rows.Count; i++)
                                {
                                    for (int j = 0; j < dgvTaiKhoan.Columns.Count; j++)
                                    {
                                        var value = dgvTaiKhoan.Rows[i].Cells[j].Value;

                                        if (dgvTaiKhoan.Columns[j].Name == "NgaySinh" && value != null)
                                        {
                                            if (DateTime.TryParse(value.ToString(), out DateTime dateValue))
                                                value = dateValue.ToString("dd/MM/yyyy");
                                        }

                                        string text = value?.ToString();

                                        // Escape CSV: nếu có dấu phẩy hoặc xuống dòng thì bọc trong ""
                                        if (!string.IsNullOrEmpty(text) && (text.Contains(",") || text.Contains("\n") || text.Contains("\"")))
                                        {
                                            text = "\"" + text.Replace("\"", "\"\"") + "\"";
                                        }

                                        sw.Write(text);

                                        if (j < dgvTaiKhoan.Columns.Count - 1)
                                            sw.Write(",");
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
            else
            {
                MessageBox.Show("Không có dữ liệu để xuất!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private static string CsvEscape(string input)
        {
            if (input == null) return string.Empty;
            bool mustQuote = input.Contains(",") || input.Contains("\"") || input.Contains("\r") || input.Contains("\n");
            string escaped = input.Replace("\"", "\"\"");
            return mustQuote ? "\"" + escaped + "\"" : escaped;
        }
    }
}
