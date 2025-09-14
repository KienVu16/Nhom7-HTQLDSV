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
    public partial class MonHoc : Form
    {
        public MonHoc()
        {
            InitializeComponent();
        }
        string Nguon = @"Data Source=DESKTOP-5LSPDHV\SQLEXPRESS01;Initial Catalog=QLSV;Integrated Security=True";

        private void HienThi()
        {
            dgvMonHoc.DataSource = null;
            dgvMonHoc.Rows.Clear();

            try
            {
                using (var conn = new SqlConnection(Nguon))
                using (var cmd = new SqlCommand(@"SELECT MaMH, TenMonHoc, SoTinChi, MaGV, MoTa, GhiChu FROM MonHoc;", conn))
                {
                    conn.Open();
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            int index = dgvMonHoc.Rows.Add();
                            dgvMonHoc.Rows[index].Cells[0].Value = reader["MaMH"].ToString();
                            dgvMonHoc.Rows[index].Cells[1].Value = reader["TenMonHoc"].ToString();
                            dgvMonHoc.Rows[index].Cells[2].Value = reader["SoTinChi"].ToString();
                            dgvMonHoc.Rows[index].Cells[3].Value = reader["MaGV"].ToString();
                            dgvMonHoc.Rows[index].Cells[4].Value = reader["MoTa"].ToString();
                            dgvMonHoc.Rows[index].Cells[5].Value = reader["GhiChu"].ToString();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi hiển thị dữ liệu: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnThem_Click(object sender, EventArgs e)
        {

        }

        private void btnThem_Click_1(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtMaMH.Text)) { MessageBox.Show("Vui lòng nhập Mã MH."); return; }
            if (string.IsNullOrWhiteSpace(txtTenMonHoc.Text)) { MessageBox.Show("Vui lòng nhập Tên môn học."); return; }
            if (string.IsNullOrWhiteSpace(txtSoTinChi.Text)) { MessageBox.Show("Vui lòng nhập Số tín chỉ."); return; }
            if (!int.TryParse(txtSoTinChi.Text.Trim(), out int soTinChi) || soTinChi < 0)
            {
                MessageBox.Show("Số tín chỉ không hợp lệ.");
                return;
            }
            var maGv = ccbMaGV?.SelectedValue?.ToString() ?? string.Empty;

            const string insertSql = @"INSERT INTO MonHoc (MaMH, TenMonHoc, SoTinChi, MaGV, MoTa, GhiChu)
                                       VALUES (@MaMH, @TenMonHoc, @SoTinChi, @MaGV, @MoTa, @GhiChu)";
            try
            {
                using (var conn = new SqlConnection(Nguon))
                {
                    conn.Open();
                    using (var check = new SqlCommand("SELECT COUNT(1) FROM MonHoc WHERE MaMH = @MaMH", conn))
                    {
                        check.Parameters.Add("@MaMH", SqlDbType.NVarChar, 50).Value = (txtMaMH.Text ?? string.Empty).Trim();
                        int exists = Convert.ToInt32(check.ExecuteScalar());
                        if (exists > 0)
                        {
                            MessageBox.Show("Mã MH đã tồn tại. Vui lòng nhập mã khác.", "Trùng khóa", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            return;
                        }
                    }

                    using (var cmd = new SqlCommand(insertSql, conn))
                    {
                        cmd.Parameters.Add("@MaMH", SqlDbType.NVarChar, 50).Value = (txtMaMH.Text ?? string.Empty).Trim();
                        cmd.Parameters.Add("@TenMonHoc", SqlDbType.NVarChar, 255).Value = (txtTenMonHoc.Text ?? string.Empty).Trim();
                        cmd.Parameters.Add("@SoTinChi", SqlDbType.Int).Value = soTinChi;
                        cmd.Parameters.Add("@MaGV", SqlDbType.NVarChar, 50).Value = (maGv ?? string.Empty).Trim();
                        cmd.Parameters.Add("@MoTa", SqlDbType.NVarChar, 1000).Value = (txtMoTa.Text ?? string.Empty).Trim();
                        cmd.Parameters.Add("@GhiChu", SqlDbType.NVarChar, 255).Value = (txtGhiChu.Text ?? string.Empty).Trim();
                        cmd.ExecuteNonQuery();
                    }
                }

                MessageBox.Show("Thêm môn học thành công!", "Thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);
                HienThi();
                ClearForm();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi thêm môn học: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void MonHoc_Load(object sender, EventArgs e)
        {
            try { LoadMaGV(); HienThi(); } catch { }

            dgvMonHoc.CellClick += dgvMonHoc_CellClick;
            ccbMaGV.SelectedIndexChanged += ccbMaGV_SelectedIndexChanged;
        }

        private void btnSua_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtMaMH.Text))
            {
                MessageBox.Show("Vui lòng chọn hoặc nhập Mã MH cần sửa.");
                return;
            }
            if (!int.TryParse(txtSoTinChi.Text.Trim(), out int soTinChi) || soTinChi < 0)
            {
                MessageBox.Show("Số tín chỉ không hợp lệ.");
                return;
            }

            const string updateSql = @"UPDATE MonHoc
                                        SET TenMonHoc = @TenMonHoc,
                                            SoTinChi = @SoTinChi,
                                            MaGV = @MaGV,
                                            MoTa = @MoTa,
                                            GhiChu = @GhiChu
                                      WHERE MaMH = @MaMH";
            try
            {
                using (var conn = new SqlConnection(Nguon))
                using (var cmd = new SqlCommand(updateSql, conn))
                {
                    cmd.Parameters.Add("@MaMH", SqlDbType.NVarChar, 50).Value = (txtMaMH.Text ?? string.Empty).Trim();
                    cmd.Parameters.Add("@TenMonHoc", SqlDbType.NVarChar, 255).Value = (txtTenMonHoc.Text ?? string.Empty).Trim();
                    cmd.Parameters.Add("@SoTinChi", SqlDbType.Int).Value = soTinChi;
                    cmd.Parameters.Add("@MaGV", SqlDbType.NVarChar, 50).Value = (ccbMaGV?.SelectedValue?.ToString() ?? string.Empty).Trim();
                    cmd.Parameters.Add("@MoTa", SqlDbType.NVarChar, 1000).Value = (txtMoTa.Text ?? string.Empty).Trim();
                    cmd.Parameters.Add("@GhiChu", SqlDbType.NVarChar, 255).Value = (txtGhiChu.Text ?? string.Empty).Trim();

                    conn.Open();
                    int affected = cmd.ExecuteNonQuery();
                    if (affected == 0)
                    {
                        MessageBox.Show("Không tìm thấy Mã MH để cập nhật.");
                        return;
                    }
                }
                MessageBox.Show("Cập nhật môn học thành công!", "Thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);
                HienThi();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi cập nhật môn học: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnXoa_Click(object sender, EventArgs e)
        {
            string maMh = (txtMaMH.Text ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(maMh))
            {
                MessageBox.Show("Vui lòng chọn Mã MH để xóa.");
                return;
            }
            var confirm = MessageBox.Show($"Bạn có chắc muốn xóa môn học: {maMh}?", "Xác nhận xóa", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (confirm != DialogResult.Yes) return;

            const string deleteSql = "DELETE FROM MonHoc WHERE MaMH = @MaMH";
            try
            {
                using (var conn = new SqlConnection(Nguon))
                using (var cmd = new SqlCommand(deleteSql, conn))
                {
                    cmd.Parameters.Add("@MaMH", SqlDbType.NVarChar, 50).Value = maMh;
                    conn.Open();
                    int affected = cmd.ExecuteNonQuery();
                    if (affected == 0)
                    {
                        MessageBox.Show("Không tìm thấy Mã MH để xóa.");
                        return;
                    }
                }
                MessageBox.Show("Xóa môn học thành công!", "Thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);
                HienThi();
                ClearForm();
            }
            catch (SqlException ex)
            {
                MessageBox.Show("Không thể xóa do ràng buộc dữ liệu: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi xóa môn học: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
            txtMaMH.Clear();
            txtTenMonHoc.Clear();
            txtSoTinChi.Clear();
            if (ccbMaGV != null) ccbMaGV.SelectedIndex = ccbMaGV.Items.Count > 0 ? 0 : -1;
            txtMoTa.Clear();
            txtGhiChu.Clear();
        }

        private void dgvMonHoc_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (dgvMonHoc.CurrentRow == null) return;

            txtMaMH.Text = dgvMonHoc.CurrentRow.Cells[0].Value?.ToString() ?? string.Empty;
            txtTenMonHoc.Text = dgvMonHoc.CurrentRow.Cells[1].Value?.ToString() ?? string.Empty;
            txtSoTinChi.Text = dgvMonHoc.CurrentRow.Cells[2].Value?.ToString() ?? string.Empty;
            var maGv = dgvMonHoc.CurrentRow.Cells[3].Value?.ToString() ?? string.Empty;
            if (!string.IsNullOrEmpty(maGv)) ccbMaGV.SelectedValue = maGv;
            txtMoTa.Text = dgvMonHoc.CurrentRow.Cells[4].Value?.ToString() ?? string.Empty;
            txtGhiChu.Text = dgvMonHoc.CurrentRow.Cells[5].Value?.ToString() ?? string.Empty;
        }

        private void quảnLýSinhViênToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SinhVien sv = new SinhVien();
            this.Hide();
            sv.ShowDialog();
            this.Show();
        }

        private void quảnLýGiáoViênToolStripMenuItem_Click(object sender, EventArgs e)
        {
            GiangVien gv = new GiangVien();
            this.Hide();
            gv.ShowDialog();
            this.Show();
        }

        private void label2_Click(object sender, EventArgs e)
        {

        }

        private void quảnLýĐiểmToolStripMenuItem_Click(object sender, EventArgs e)
        {
            BangDiem bd = new BangDiem();
            this.Hide();
            bd.ShowDialog(); 
            this.Show();
        }

        private void ccbMaGV_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                var selected = ccbMaGV.SelectedValue?.ToString();
                if (string.IsNullOrWhiteSpace(selected) || selected == "System.Data.DataRowView") return;

                dgvMonHoc.DataSource = null;
                dgvMonHoc.Rows.Clear();

                using (var conn = new SqlConnection(Nguon))
                using (var cmd = new SqlCommand(@"SELECT MaMH, TenMonHoc, SoTinChi, MaGV, MoTa, GhiChu
                                                 FROM MonHoc
                                                 WHERE MaGV = @MaGV
                                                 ORDER BY MaMH;", conn))
                {
                    cmd.Parameters.Add("@MaGV", SqlDbType.NVarChar, 50).Value = selected;
                    conn.Open();
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            int index = dgvMonHoc.Rows.Add();
                            dgvMonHoc.Rows[index].Cells[0].Value = reader["MaMH"].ToString();
                            dgvMonHoc.Rows[index].Cells[1].Value = reader["TenMonHoc"].ToString();
                            dgvMonHoc.Rows[index].Cells[2].Value = reader["SoTinChi"].ToString();
                            dgvMonHoc.Rows[index].Cells[3].Value = reader["MaGV"].ToString();
                            dgvMonHoc.Rows[index].Cells[4].Value = reader["MoTa"].ToString();
                            dgvMonHoc.Rows[index].Cells[5].Value = reader["GhiChu"].ToString();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi lọc theo Mã GV: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LoadMaGV()
        {
            using (var conn = new SqlConnection(Nguon))
            using (var cmd = new SqlCommand("SELECT MaGV FROM GiangVien ORDER BY MaGV;", conn))
            using (var da = new SqlDataAdapter(cmd))
            {
                var dt = new DataTable();
                conn.Open();
                da.Fill(dt);
                ccbMaGV.DataSource = dt;
                ccbMaGV.DisplayMember = "MaGV";
                ccbMaGV.ValueMember = "MaGV";
            }
        }

        private void quảnLýMônHọcToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        private void quảnLýĐăngKýHọcToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        private void toolStripMenuItem6_Click(object sender, EventArgs e)
        {

        }

        private void đăngXuấtToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        private void btnXuatFile_Click(object sender, EventArgs e)
        {
            if (dgvMonHoc.Rows.Count > 0)
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
                                for (int i = 0; i < dgvMonHoc.Columns.Count; i++)
                                {
                                    sw.Write(dgvMonHoc.Columns[i].HeaderText);
                                    if (i < dgvMonHoc.Columns.Count - 1)
                                        sw.Write(",");
                                }
                                sw.WriteLine();

                                // Ghi dữ liệu
                                for (int i = 0; i < dgvMonHoc.Rows.Count; i++)
                                {
                                    for (int j = 0; j < dgvMonHoc.Columns.Count; j++)
                                    {
                                        var value = dgvMonHoc.Rows[i].Cells[j].Value;

                                        if (dgvMonHoc.Columns[j].Name == "NgaySinh" && value != null)
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

                                        if (j < dgvMonHoc.Columns.Count - 1)
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
