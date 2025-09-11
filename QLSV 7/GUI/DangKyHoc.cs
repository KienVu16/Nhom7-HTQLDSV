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
using System.Diagnostics;


namespace QLSV_7.GUI
{
    public partial class DangKyHoc : Form
    {
        public DangKyHoc()
        {
            InitializeComponent();
        }
        string Nguon = @"Data Source=DESKTOP-5LSPDHV\SQLEXPRESS01;Initial Catalog=QLSV;Integrated Security=True";
        string Lenh = @"";  
        private void HienThi()
        {
            dgvDangKyHoc.DataSource = null;
            dgvDangKyHoc.Rows.Clear();

            try
            {
                using (var conn = new SqlConnection(Nguon))
                using (var cmd = new SqlCommand(@"SELECT MaDK, MaSV, MaMH, NgayDangKy, GhiChu FROM DangKyHoc;", conn))
                {
                    conn.Open();
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            int index = dgvDangKyHoc.Rows.Add();
                            dgvDangKyHoc.Rows[index].Cells[0].Value = reader["MaDK"].ToString();
                            dgvDangKyHoc.Rows[index].Cells[1].Value = reader["MaSV"].ToString();
                            dgvDangKyHoc.Rows[index].Cells[2].Value = reader["MaMH"].ToString();
                            dgvDangKyHoc.Rows[index].Cells[3].Value = Convert.ToDateTime(reader["NgayDangKy"]).ToString("yyyy-MM-dd");
                            dgvDangKyHoc.Rows[index].Cells[4].Value = reader["GhiChu"].ToString();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ReportError("HienThi()", ex);
            }
        }
        private void DangKyHoc_Load(object sender, EventArgs e) {
            try
            {
                LoadMaSV();
                LoadMaMH();
                HienThi();

                // Ensure all events are wired even if Designer lost hookups
                btnThem.Click += btnThem_Click;
                btnSua.Click += btnSua_Click;
                btnXoa.Click += btnXoa_Click;
                btnXuatFile.Click += btnXuatFile_Click;
                btnThoat.Click += btnThoat_Click;
                dgvDangKyHoc.CellClick += dgvDangKyHoc_CellClick;
            }
            catch (Exception ex)
            {
                ReportError("DangKyHoc_Load()", ex);
            }
        }
        private void btnThem_Click(object sender, EventArgs e)
        {
            Debug.WriteLine("[UI] btnThem_Click pressed");
            string maDk = (txtMaDK.Text ?? string.Empty).Trim();
            string maSv = cbbMaSV?.SelectedValue?.ToString() ?? string.Empty;
            string maMh = cbbMaMH?.SelectedValue?.ToString() ?? string.Empty;
            if (string.IsNullOrWhiteSpace(maDk)) { MessageBox.Show("Vui lòng nhập Mã DK."); return; }
            if (string.IsNullOrWhiteSpace(maSv)) { MessageBox.Show("Vui lòng chọn Mã SV."); return; }
            if (string.IsNullOrWhiteSpace(maMh)) { MessageBox.Show("Vui lòng chọn Mã MH."); return; }

            const string insertSql = @"INSERT INTO DangKyHoc (MaDK, MaSV, MaMH, NgayDangKy, GhiChu)
                                       VALUES (@MaDK, @MaSV, @MaMH, @NgayDangKy, @GhiChu)";
            try
            {
                using (var conn = new SqlConnection(Nguon))
                {
                    conn.Open();
                    using (var check = new SqlCommand("SELECT COUNT(1) FROM DangKyHoc WHERE MaDK=@MaDK", conn))
                    {
                        check.Parameters.Add("@MaDK", SqlDbType.NVarChar, 50).Value = maDk;
                        DebugSql(check, "Check exists (insert)");
                        if (Convert.ToInt32(check.ExecuteScalar()) > 0)
                        {
                            MessageBox.Show("Mã DK đã tồn tại.");
                            return;
                        }
                    }
                    using (var cmd = new SqlCommand(insertSql, conn))
                    {
                        cmd.Parameters.Add("@MaDK", SqlDbType.NVarChar, 50).Value = maDk;
                        cmd.Parameters.Add("@MaSV", SqlDbType.NVarChar, 50).Value = maSv;
                        cmd.Parameters.Add("@MaMH", SqlDbType.NVarChar, 50).Value = maMh;
                        cmd.Parameters.Add("@NgayDangKy", SqlDbType.Date).Value = dtpNgayDangKy.Value.Date;
                        cmd.Parameters.Add("@GhiChu", SqlDbType.NVarChar, 255).Value = (txtGhiChu.Text ?? string.Empty).Trim();
                        DebugSql(cmd, "Insert DangKyHoc");
                        cmd.ExecuteNonQuery();
                    }
                }
                MessageBox.Show("Thêm đăng ký học thành công!");
                HienThi();
            }
            catch (SqlException ex) when (ex.Number == 2627 || ex.Number == 2601)
            {
                MessageBox.Show("Mã DK đã tồn tại (trùng khóa).", "Trùng khóa", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            catch (SqlException ex) when (ex.Number == 547)
            {
                MessageBox.Show("Không thể thêm do ràng buộc khóa ngoại (MaSV/MaMH không hợp lệ).", "Ràng buộc dữ liệu", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            catch (Exception ex)
            {
                ReportError("btnThem_Click", ex);
            }
        }

        private void btnSua_Click(object sender, EventArgs e)
        {
            Debug.WriteLine("[UI] btnSua_Click pressed");
            string maDk = (txtMaDK.Text ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(maDk)) { MessageBox.Show("Vui lòng nhập/chọn Mã DK."); return; }
            string maSv = cbbMaSV?.SelectedValue?.ToString() ?? string.Empty;
            string maMh = cbbMaMH?.SelectedValue?.ToString() ?? string.Empty;

            const string updateSql = @"UPDATE DangKyHoc
                                        SET MaSV=@MaSV, MaMH=@MaMH, NgayDangKy=@NgayDangKy, GhiChu=@GhiChu
                                      WHERE MaDK=@MaDK";
            try
            {
                using (var conn = new SqlConnection(Nguon))
                using (var cmd = new SqlCommand(updateSql, conn))
                {
                    // Ràng buộc nghiệp vụ: MaDK chỉ thuộc 1 MaSV; không cho đổi MaSV của MaDK đã tồn tại
                    using (var checkOwner = new SqlCommand("SELECT MaSV FROM DangKyHoc WHERE MaDK=@MaDK", conn))
                    {
                        checkOwner.Parameters.Add("@MaDK", SqlDbType.NVarChar, 50).Value = maDk;
                        conn.Open();
                        var existing = checkOwner.ExecuteScalar()?.ToString();
                        if (existing == null)
                        {
                            MessageBox.Show("Không tìm thấy MaDK để cập nhật.");
                            return;
                        }
                        if (!string.Equals(existing, maSv, StringComparison.OrdinalIgnoreCase))
                        {
                            MessageBox.Show($"MaDK '{maDk}' đã gắn với MaSV '{existing}'. Không được đổi MaSV.", "Ràng buộc nghiệp vụ", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            return;
                        }
                        conn.Close();
                    }

                    cmd.Parameters.Add("@MaDK", SqlDbType.NVarChar, 50).Value = maDk;
                    cmd.Parameters.Add("@MaSV", SqlDbType.NVarChar, 50).Value = maSv;
                    cmd.Parameters.Add("@MaMH", SqlDbType.NVarChar, 50).Value = maMh;
                    cmd.Parameters.Add("@NgayDangKy", SqlDbType.Date).Value = dtpNgayDangKy.Value.Date;
                    cmd.Parameters.Add("@GhiChu", SqlDbType.NVarChar, 255).Value = (txtGhiChu.Text ?? string.Empty).Trim();

                    conn.Open();
                    DebugSql(cmd, "Update DangKyHoc");
                    int n = cmd.ExecuteNonQuery();
                    if (n == 0) { MessageBox.Show("Không tìm thấy Mã DK để cập nhật."); return; }
                }
                MessageBox.Show("Cập nhật thành công!");
                HienThi();
            }
            catch (SqlException ex) when (ex.Number == 547)
            {
                MessageBox.Show("Ràng buộc dữ liệu vi phạm khi cập nhật (khóa ngoại).", "Ràng buộc dữ liệu", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            catch (Exception ex)
            {
                ReportError("btnSua_Click", ex);
            }
        }

        private void btnXoa_Click(object sender, EventArgs e)
        {
            Debug.WriteLine("[UI] btnXoa_Click pressed");
            string maDk = (txtMaDK.Text ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(maDk)) { MessageBox.Show("Vui lòng nhập/chọn Mã DK."); return; }
            if (MessageBox.Show($"Xóa đăng ký {maDk}?", "Xác nhận", MessageBoxButtons.YesNo) != DialogResult.Yes) return;

            try
            {
                using (var conn = new SqlConnection(Nguon))
                using (var cmd = new SqlCommand("DELETE FROM DangKyHoc WHERE MaDK=@MaDK", conn))
                {
                    cmd.Parameters.Add("@MaDK", SqlDbType.NVarChar, 50).Value = maDk;
                    conn.Open();
                    DebugSql(cmd, "Delete DangKyHoc");
                    int n = cmd.ExecuteNonQuery();
                    if (n == 0) { MessageBox.Show("Không tìm thấy Mã DK để xóa."); return; }
                }
                MessageBox.Show("Xóa thành công!");
                HienThi();
            }
            catch (SqlException ex) when (ex.Number == 547)
            {
                MessageBox.Show("Không thể xóa do ràng buộc dữ liệu (bản ghi đang được tham chiếu).", "Ràng buộc dữ liệu", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            catch (Exception ex)
            {
                ReportError("btnXoa_Click", ex);
            }
        }

        private void btnXuatFile_Click(object sender, EventArgs e)
        {
            Debug.WriteLine("[UI] btnXuatFile_Click pressed");
            // Đếm số dòng dữ liệu (bỏ qua NewRow)
            int dataRowCount = dgvDangKyHoc.Rows.Cast<DataGridViewRow>().Count(r => !r.IsNewRow);
            if (dataRowCount == 0) { MessageBox.Show("Không có dữ liệu để xuất."); return; }

            using (var sfd = new SaveFileDialog())
            {
                sfd.Title = "Xuất dữ liệu đăng ký học";
                sfd.Filter = "CSV (Excel)|*.csv";
                sfd.FileName = "DangKyHoc.csv";
                if (sfd.ShowDialog() != DialogResult.OK) return;

                try
                {
                    var sb = new StringBuilder();
                    // header
                    for (int c = 0; c < dgvDangKyHoc.Columns.Count; c++)
                    {
                        if (c > 0) sb.Append(',');
                    }
                    sb.AppendLine();

                    foreach (DataGridViewRow row in dgvDangKyHoc.Rows)
                    {
                        if (row.IsNewRow) continue;
                        for (int c = 0; c < dgvDangKyHoc.Columns.Count; c++)
                        {
                            if (c > 0) sb.Append(',');
                            var value = row.Cells[c].Value?.ToString() ?? string.Empty;
                        }
                        sb.AppendLine();
                    }

                    File.WriteAllText(sfd.FileName, sb.ToString(), new UTF8Encoding(true));
                    MessageBox.Show("Xuất file thành công!");
                }
                catch (Exception ex)
                {
                    ReportError("btnXuatFile_Click", ex);
                }
            }
        }

        private void txtGhiChu_TextChanged(object sender, EventArgs e) { }

        private void dgvDangKyHoc_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            Debug.WriteLine("[UI] dgvDangKyHoc_CellClick row=" + e.RowIndex);
            if (dgvDangKyHoc.CurrentRow == null) return;
            txtMaDK.Text = dgvDangKyHoc.CurrentRow.Cells[0].Value?.ToString() ?? string.Empty;
            var maSv = dgvDangKyHoc.CurrentRow.Cells[1].Value?.ToString() ?? string.Empty;
            var maMh = dgvDangKyHoc.CurrentRow.Cells[2].Value?.ToString() ?? string.Empty;
            if (!string.IsNullOrEmpty(maSv)) cbbMaSV.SelectedValue = maSv;
            if (!string.IsNullOrEmpty(maMh)) cbbMaMH.SelectedValue = maMh;
            var ngay = dgvDangKyHoc.CurrentRow.Cells[3].Value?.ToString() ?? string.Empty;
            if (DateTime.TryParse(ngay, out DateTime d)) dtpNgayDangKy.Value = d; else dtpNgayDangKy.Value = DateTime.Today;
            txtGhiChu.Text = dgvDangKyHoc.CurrentRow.Cells[4].Value?.ToString() ?? string.Empty;
        }

        private void btnThoat_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void tableLayoutPanel3_Paint(object sender, PaintEventArgs e)
        {

        }

        private void cbbMaSV_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                var selected = cbbMaSV.SelectedValue?.ToString();
                if (string.IsNullOrWhiteSpace(selected) || selected == "System.Data.DataRowView") return;

                using (var conn = new SqlConnection(Nguon))
                using (var cmd = new SqlCommand(@"SELECT MaDK, MaSV, MaMH, NgayDangKy, GhiChu
                                                 FROM DangKyHoc
                                                 WHERE MaSV = @MaSV
                                                 ORDER BY NgayDangKy DESC;", conn))
                {
                    cmd.Parameters.Add("@MaSV", SqlDbType.NVarChar, 50).Value = selected;
                    DisplayToGrid(cmd);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi lọc theo Mã SV: " + ex.Message);
            }
        }

        private void cbbMaMH_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                var selected = cbbMaMH.SelectedValue?.ToString();
                if (string.IsNullOrWhiteSpace(selected) || selected == "System.Data.DataRowView") return;

                using (var conn = new SqlConnection(Nguon))
                using (var cmd = new SqlCommand(@"SELECT MaDK, MaSV, MaMH, NgayDangKy, GhiChu
                                                 FROM DangKyHoc
                                                 WHERE MaMH = @MaMH
                                                 ORDER BY NgayDangKy DESC;", conn))
                {
                    cmd.Parameters.Add("@MaMH", SqlDbType.NVarChar, 50).Value = selected;
                    DisplayToGrid(cmd);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi lọc theo Mã MH: " + ex.Message);
            }
        }

        private void LoadMaSV()
        {
            using (var conn = new SqlConnection(Nguon))
            using (var cmd = new SqlCommand("SELECT MaSV FROM SinhVien ORDER BY MaSV;", conn))
            using (var da = new SqlDataAdapter(cmd))
            {
                var dt = new DataTable();
                conn.Open();
                da.Fill(dt);
                cbbMaSV.DataSource = dt;
                cbbMaSV.DisplayMember = "MaSV";
                cbbMaSV.ValueMember = "MaSV";
                if (dt.Rows.Count == 0)
                {
                    MessageBox.Show("Chưa có dữ liệu SinhVien để chọn.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
        }

        private void LoadMaMH()
        {
            using (var conn = new SqlConnection(Nguon))
            using (var cmd = new SqlCommand("SELECT MaMH FROM MonHoc ORDER BY MaMH;", conn))
            using (var da = new SqlDataAdapter(cmd))
            {
                var dt = new DataTable();
                conn.Open();
                da.Fill(dt);
                cbbMaMH.DataSource = dt;
                cbbMaMH.DisplayMember = "MaMH";
                cbbMaMH.ValueMember = "MaMH";
                if (dt.Rows.Count == 0)
                {
                    MessageBox.Show("Chưa có dữ liệu MonHoc để chọn.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
        }

        private void DisplayToGrid(SqlCommand cmd)
        {
            dgvDangKyHoc.DataSource = null;
            dgvDangKyHoc.Rows.Clear();

            using (cmd.Connection)
            {
                cmd.Connection.Open();
                using (var reader = cmd.ExecuteReader())
                {
                    int count = 0;
                    while (reader.Read())
                    {
                        int index = dgvDangKyHoc.Rows.Add();
                        dgvDangKyHoc.Rows[index].Cells[0].Value = reader["MaDK"].ToString();
                        dgvDangKyHoc.Rows[index].Cells[1].Value = reader["MaSV"].ToString();
                        dgvDangKyHoc.Rows[index].Cells[2].Value = reader["MaMH"].ToString();
                        dgvDangKyHoc.Rows[index].Cells[3].Value = Convert.ToDateTime(reader["NgayDangKy"]).ToString("yyyy-MM-dd");
                        dgvDangKyHoc.Rows[index].Cells[4].Value = reader["GhiChu"].ToString();
                        count++;
                    }
                    if (count == 0)
                    {
                        MessageBox.Show("Không có bản ghi phù hợp.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
            }
        }

        private void DebugSql(SqlCommand cmd, string context)
        {
            var sb = new StringBuilder();
            sb.AppendLine($"[{DateTime.Now:HH:mm:ss}] {context}");
            sb.AppendLine(cmd.CommandText);
            foreach (SqlParameter p in cmd.Parameters)
            {
                sb.AppendLine($"  {p.ParameterName} = {p.Value ?? "<null>"}");
            }
            Debug.WriteLine(sb.ToString());
        }

        private void ReportError(string context, Exception ex)
        {
            Debug.WriteLine($"[{DateTime.Now:HH:mm:ss}] ERROR {context}: {ex}");
            MessageBox.Show($"{context}: {ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        private void tableLayoutPanel1_Paint(object sender, PaintEventArgs e)
        {

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

        private void toolStripMenuItem6_Click(object sender, EventArgs e)
        {

        }

        private void đăngXuấtToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }
    }
}
