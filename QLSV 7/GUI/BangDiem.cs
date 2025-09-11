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
using System.Data.SqlClient;

namespace QLSV_7.GUI
{
    public partial class BangDiem : Form
    {
        public BangDiem()
        {
            InitializeComponent();
        }

        string Nguon = @"Data Source=DESKTOP-5LSPDHV\SQLEXPRESS01;Initial Catalog=QLSV;Integrated Security=True";

        private void HienThi()
        {
            dgvDiem.DataSource = null;
            dgvDiem.Rows.Clear();

            try
            {
                using (var conn = new SqlConnection(Nguon))
                using (var cmd = new SqlCommand(@"SELECT MaDiem, MaDK, DiemGiuaKy, DiemCuoiKy, DiemKhac, TongKet, GhiChu FROM BangDiem;", conn))
                {
                    conn.Open();
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            int index = dgvDiem.Rows.Add();
                            dgvDiem.Rows[index].Cells[0].Value = reader["MaDiem"].ToString();
                            dgvDiem.Rows[index].Cells[1].Value = reader["MaDK"].ToString();
                            dgvDiem.Rows[index].Cells[2].Value = reader["DiemGiuaKy"].ToString();
                            dgvDiem.Rows[index].Cells[3].Value = reader["DiemCuoiKy"].ToString();
                            dgvDiem.Rows[index].Cells[4].Value = reader["DiemKhac"].ToString();
                            dgvDiem.Rows[index].Cells[5].Value = reader["TongKet"].ToString();
                            dgvDiem.Rows[index].Cells[6].Value = reader["GhiChu"].ToString();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi hiển thị dữ liệu: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void Diem_Load(object sender, EventArgs e)
        {
            try { LoadMaDK(); HienThi(); } catch { }
            dgvDiem.CellClick += dgvDiem_CellClick;
            ccbMaDK.SelectedIndexChanged += ccbMaDK_SelectedIndexChanged;
        }

        private void dgvDiem_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;
            var row = dgvDiem.Rows[e.RowIndex];

            // Tránh dùng row.DataGridView để kiểm tra cột nhằm loại bỏ lỗi NullReference
            txtMaDiem.Text = row.Cells.Count > 0 ? row.Cells[0].Value?.ToString() ?? string.Empty : string.Empty;
            var maDk = row.Cells.Count > 1 ? row.Cells[1].Value?.ToString() ?? string.Empty : string.Empty;
            if (!string.IsNullOrEmpty(maDk)) ccbMaDK.SelectedValue = maDk;
            txtDiem.Text = row.Cells.Count > 2 ? row.Cells[2].Value?.ToString() ?? string.Empty : string.Empty;
            txtDiemCuoiKy.Text = row.Cells.Count > 3 ? row.Cells[3].Value?.ToString() ?? string.Empty : string.Empty;
            txtDiemKhac.Text = row.Cells.Count > 4 ? row.Cells[4].Value?.ToString() ?? string.Empty : string.Empty;
            txtTongKet.Text = row.Cells.Count > 5 ? row.Cells[5].Value?.ToString() ?? string.Empty : string.Empty;
            txtGhiChu.Text = row.Cells.Count > 6 ? row.Cells[6].Value?.ToString() ?? string.Empty : string.Empty;
        }

        private static string GetCellString(DataGridViewRow row, string columnName)
        {
            if (!row.DataGridView.Columns.Contains(columnName)) return string.Empty;
            return row.Cells[columnName].Value?.ToString() ?? string.Empty;
        }

        private bool TryParseScore(string input, out decimal value)
        {
            if (decimal.TryParse((input ?? string.Empty).Trim(), out value))
            {
                return true;
            }
            MessageBox.Show("Giá trị điểm không hợp lệ.", "Lỗi nhập liệu", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return false;
        }

        private void btnThem_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtMaDiem.Text)) { MessageBox.Show("Vui lòng nhập Mã điểm."); return; }
            var maDk = ccbMaDK?.SelectedValue?.ToString() ?? string.Empty;
            if (string.IsNullOrWhiteSpace(maDk)) { MessageBox.Show("Vui lòng chọn Mã DK."); return; }
            if (!TryParseScore(txtDiem.Text, out decimal diemGiuaKy)) return;
            if (!TryParseScore(txtDiemCuoiKy.Text, out decimal diemCuoiKy)) return;
            if (!TryParseScore(txtDiemKhac.Text, out decimal diemKhac)) return;
            if (!TryParseScore(txtTongKet.Text, out decimal tongKet)) return;

            const string insertSql = @"INSERT INTO BangDiem (MaDiem, MaDK, DiemGiuaKy, DiemCuoiKy, DiemKhac, TongKet, GhiChu)
                                       VALUES (@MaDiem, @MaDK, @DiemGiuaKy, @DiemCuoiKy, @DiemKhac, @TongKet, @GhiChu)";
            try
            {
                using (var conn = new SqlConnection(Nguon))
                {
                    conn.Open();
                    using (var check = new SqlCommand("SELECT COUNT(1) FROM BangDiem WHERE MaDiem = @MaDiem", conn))
                    {
                        check.Parameters.Add("@MaDiem", SqlDbType.NVarChar, 50).Value = (txtMaDiem.Text ?? string.Empty).Trim();
                        int exists = Convert.ToInt32(check.ExecuteScalar());
                        if (exists > 0)
                        {
                            MessageBox.Show("Mã điểm đã tồn tại. Vui lòng nhập mã khác.", "Trùng khóa", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            return;
                        }
                    }

                    using (var cmd = new SqlCommand(insertSql, conn))
                    {
                        cmd.Parameters.Add("@MaDiem", SqlDbType.NVarChar, 50).Value = (txtMaDiem.Text ?? string.Empty).Trim();
                        cmd.Parameters.Add("@MaDK", SqlDbType.NVarChar, 50).Value = maDk.Trim();
                        cmd.Parameters.Add("@DiemGiuaKy", SqlDbType.Decimal).Value = diemGiuaKy;
                        cmd.Parameters.Add("@DiemCuoiKy", SqlDbType.Decimal).Value = diemCuoiKy;
                        cmd.Parameters.Add("@DiemKhac", SqlDbType.Decimal).Value = diemKhac;
                        cmd.Parameters.Add("@TongKet", SqlDbType.Decimal).Value = tongKet;
                        cmd.Parameters.Add("@GhiChu", SqlDbType.NVarChar, 255).Value = (txtGhiChu.Text ?? string.Empty).Trim();
                        cmd.ExecuteNonQuery();
                    }
                }

                MessageBox.Show("Thêm điểm thành công!", "Thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);
                HienThi();
                ClearForm();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi thêm điểm: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnSua_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtMaDiem.Text)) { MessageBox.Show("Vui lòng chọn hoặc nhập Mã điểm cần sửa."); return; }
            var maDkUpdate = ccbMaDK?.SelectedValue?.ToString() ?? string.Empty;
            if (!TryParseScore(txtDiem.Text, out decimal diemGiuaKy)) return;
            if (!TryParseScore(txtDiemCuoiKy.Text, out decimal diemCuoiKy)) return;
            if (!TryParseScore(txtDiemKhac.Text, out decimal diemKhac)) return;
            if (!TryParseScore(txtTongKet.Text, out decimal tongKet)) return;

            const string updateSql = @"UPDATE BangDiem
                                        SET MaDK = @MaDK,
                                            DiemGiuaKy = @DiemGiuaKy,
                                            DiemCuoiKy = @DiemCuoiKy,
                                            DiemKhac = @DiemKhac,
                                            TongKet = @TongKet,
                                            GhiChu = @GhiChu
                                      WHERE MaDiem = @MaDiem";
            try
            {
                using (var conn = new SqlConnection(Nguon))
                using (var cmd = new SqlCommand(updateSql, conn))
                {
                    cmd.Parameters.Add("@MaDiem", SqlDbType.NVarChar, 50).Value = (txtMaDiem.Text ?? string.Empty).Trim();
                    cmd.Parameters.Add("@MaDK", SqlDbType.NVarChar, 50).Value = (maDkUpdate ?? string.Empty).Trim();
                    cmd.Parameters.Add("@DiemGiuaKy", SqlDbType.Decimal).Value = diemGiuaKy;
                    cmd.Parameters.Add("@DiemCuoiKy", SqlDbType.Decimal).Value = diemCuoiKy;
                    cmd.Parameters.Add("@DiemKhac", SqlDbType.Decimal).Value = diemKhac;
                    cmd.Parameters.Add("@TongKet", SqlDbType.Decimal).Value = tongKet;
                    cmd.Parameters.Add("@GhiChu", SqlDbType.NVarChar, 255).Value = (txtGhiChu.Text ?? string.Empty).Trim();

                    conn.Open();
                    int affected = cmd.ExecuteNonQuery();
                    if (affected == 0)
                    {
                        MessageBox.Show("Không tìm thấy Mã điểm để cập nhật.");
                        return;
                    }
                }
                MessageBox.Show("Cập nhật điểm thành công!", "Thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);
                HienThi();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi cập nhật điểm: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnXoa_Click(object sender, EventArgs e)
        {
            string maDiem = (txtMaDiem.Text ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(maDiem)) { MessageBox.Show("Vui lòng chọn Mã điểm để xóa."); return; }

            var confirm = MessageBox.Show($"Bạn có chắc muốn xóa điểm: {maDiem}?", "Xác nhận xóa", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (confirm != DialogResult.Yes) return;

            const string deleteSql = "DELETE FROM BangDiem WHERE MaDiem = @MaDiem";
            try
            {
                using (var conn = new SqlConnection(Nguon))
                using (var cmd = new SqlCommand(deleteSql, conn))
                {
                    cmd.Parameters.Add("@MaDiem", SqlDbType.NVarChar, 50).Value = maDiem;
                    conn.Open();
                    int affected = cmd.ExecuteNonQuery();
                    if (affected == 0)
                    {
                        MessageBox.Show("Không tìm thấy Mã điểm để xóa.");
                        return;
                    }
                }
                MessageBox.Show("Xóa điểm thành công!", "Thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);
                HienThi();
                ClearForm();
            }
            catch (SqlException ex)
            {
                MessageBox.Show("Không thể xóa do ràng buộc dữ liệu: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi xóa điểm: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnXuatFile_Click(object sender, EventArgs e)
        {
            if (dgvDiem.Rows.Count == 0)
            {
                MessageBox.Show("Không có dữ liệu để xuất.");
                return;
            }

            using (var sfd = new SaveFileDialog())
            {
                sfd.Title = "Xuất dữ liệu điểm";
                sfd.Filter = "CSV (Excel)|*.csv";
                sfd.FileName = "Diem.csv";
                if (sfd.ShowDialog() != DialogResult.OK) return;

                try
                {
                    var sb = new StringBuilder();
                    for (int c = 0; c < dgvDiem.Columns.Count; c++)
                    {
                        if (c > 0) sb.Append(',');
                        sb.Append(EscapeCsv(dgvDiem.Columns[c].HeaderText));
                    }
                    sb.AppendLine();

                    foreach (DataGridViewRow row in dgvDiem.Rows)
                    {
                        if (row.IsNewRow) continue;
                        for (int c = 0; c < dgvDiem.Columns.Count; c++)
                        {
                            if (c > 0) sb.Append(',');
                            var value = row.Cells[c].Value?.ToString() ?? string.Empty;
                            sb.Append(EscapeCsv(value));
                        }
                        sb.AppendLine();
                    }

                    File.WriteAllText(sfd.FileName, sb.ToString(), new UTF8Encoding(true));
                    MessageBox.Show("Xuất file thành công!");
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Lỗi khi xuất file: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
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
            txtMaDiem.Clear();
            if (ccbMaDK != null) ccbMaDK.SelectedIndex = ccbMaDK.Items.Count > 0 ? 0 : -1;
            txtDiem.Clear();
            txtDiemCuoiKy.Clear();
            txtDiemKhac.Clear();
            txtTongKet.Clear();
            txtGhiChu.Clear();
        }

        private void txtTenMonHoc_TextChanged(object sender, EventArgs e)
        {
            // not used for Diem; left to satisfy existing designer hookup
        }

        private void tableLayoutPanel4_Paint(object sender, PaintEventArgs e)
        {
            // no-op
        }

        private void label12_Click(object sender, EventArgs e)
        {

        }

        private void ccbMaDK_SelectedIndexChanged(object sender, EventArgs e)
        {
            var selected = ccbMaDK.SelectedValue?.ToString();
            if (string.IsNullOrWhiteSpace(selected) || selected == "System.Data.DataRowView") return;

            dgvDiem.DataSource = null;
            dgvDiem.Rows.Clear();

            try
            {
                using (var conn = new SqlConnection(Nguon))
                using (var cmd = new SqlCommand(@"SELECT MaDiem, MaDK, DiemGiuaKy, DiemCuoiKy, DiemKhac, TongKet, GhiChu
                                                  FROM BangDiem WHERE MaDK = @MaDK ORDER BY MaDiem;", conn))
                {
                    cmd.Parameters.Add("@MaDK", SqlDbType.NVarChar, 50).Value = selected;
                    conn.Open();
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            int index = dgvDiem.Rows.Add();
                            dgvDiem.Rows[index].Cells[0].Value = reader["MaDiem"].ToString();
                            dgvDiem.Rows[index].Cells[1].Value = reader["MaDK"].ToString();
                            dgvDiem.Rows[index].Cells[2].Value = reader["DiemGiuaKy"].ToString();
                            dgvDiem.Rows[index].Cells[3].Value = reader["DiemCuoiKy"].ToString();
                            dgvDiem.Rows[index].Cells[4].Value = reader["DiemKhac"].ToString();
                            dgvDiem.Rows[index].Cells[5].Value = reader["TongKet"].ToString();
                            dgvDiem.Rows[index].Cells[6].Value = reader["GhiChu"].ToString();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi lọc theo Mã DK: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LoadMaDK()
        {
            using (var conn = new SqlConnection(Nguon))
            using (var cmd = new SqlCommand("SELECT DISTINCT MaDK FROM DangKyHoc ORDER BY MaDK;", conn))
            using (var da = new SqlDataAdapter(cmd))
            {
                var dt = new DataTable();
                conn.Open();
                da.Fill(dt);
                ccbMaDK.DataSource = dt;
                ccbMaDK.DisplayMember = "MaDK";
                ccbMaDK.ValueMember = "MaDK";
            }
        }

        private void txtMaDiem_TextChanged(object sender, EventArgs e)
        {

        }

        private void menuStrip1_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
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

        private void toolStripMenuItem6_Click(object sender, EventArgs e)
        {

        }

        private void đăngXuấtToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        private void tableLayoutPanel1_Paint(object sender, PaintEventArgs e)
        {

        }
    }
}
