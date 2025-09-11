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
    public partial class GiangVien : Form
    {
        public GiangVien()
        {
            InitializeComponent();
        }
        string Nguon = @"Data Source=DESKTOP-5LSPDHV\SQLEXPRESS01;Initial Catalog=QLSV;Integrated Security=True";
        string Lenh = @"";
        SqlConnection KetNoi;
        SqlCommand ThucHien;
        SqlDataReader DocDuLieu;

        
        void HienThi()
        {
            // Gỡ DataSource để được phép dùng Rows.Add
            dgvGiangVien.DataSource = null;
            dgvGiangVien.Rows.Clear();

            try
            {
                using (var conn = new SqlConnection(Nguon))
                using (var cmd = new SqlCommand(@"SELECT 
                                                        MaGV,
                                                        Ho,
                                                        Ten,
                                                        Email,
                                                        SoDienThoai,
                                                        Khoa,
                                                        GhiChu
                                                    FROM GiangVien;", conn))
                {
                    conn.Open();
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            int index = dgvGiangVien.Rows.Add();
                            dgvGiangVien.Rows[index].Cells[0].Value = reader["MaGV"].ToString();
                            dgvGiangVien.Rows[index].Cells[1].Value = reader["Ho"].ToString();
                            dgvGiangVien.Rows[index].Cells[2].Value = reader["Ten"].ToString();
                            dgvGiangVien.Rows[index].Cells[3].Value = reader["Email"].ToString();
                            dgvGiangVien.Rows[index].Cells[4].Value = reader["SoDienThoai"].ToString();
                            dgvGiangVien.Rows[index].Cells[5].Value = reader["Khoa"].ToString();
                            dgvGiangVien.Rows[index].Cells[6].Value = reader["GhiChu"].ToString();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi hiển thị dữ liệu: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            try
            {
                HienThi();
            }
            catch { }

            btnThem.Click += btnThem_Click;
            btnSua.Click += btnSua_Click;
            btnXoa.Click += btnXoa_Click;
            btnXuatFile.Click += btnXuatFile_Click;
            btnThoat.Click += btnThoat_Click;
            dgvGiangVien.CellClick += dgvGiangVien_CellClick;
        }
        private void btnThem_Click(object sender, EventArgs e)
        {
            // Validate bắt buộc theo schema
            if (string.IsNullOrWhiteSpace(txtMaGV.Text))
            {
                MessageBox.Show("Vui lòng nhập Mã GV.", "Thiếu thông tin", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            if (string.IsNullOrWhiteSpace(txtEmail.Text))
            {
                MessageBox.Show("Vui lòng nhập Email", "Thiếu thông tin", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
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
            if (string.IsNullOrWhiteSpace(txtSoDienThoai.Text))
            {
                MessageBox.Show("Vui lòng nhập Số điện thoại.", "Thiếu thông tin", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            if (string.IsNullOrWhiteSpace(txtKhoa.Text))
            {
                MessageBox.Show("Vui lòng nhập Tên khoa .", "Thiếu thông tin", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            const string insertSql = @"INSERT INTO GiangVien
                                                (MaGV, Ho, Ten, Email, SoDienThoai, Khoa, GhiChu)
                                                VALUES (@MaGV, @Ho, @Ten, @Email, @SoDienThoai, @Khoa, @GhiChu)";

            try
            {
                using (var conn = new SqlConnection(Nguon))
                {
                    conn.Open();

                    // Kiểm tra trùng Mã GV trước khi thêm
                    using (var check = new SqlCommand("SELECT COUNT(1) FROM GiangVien WHERE MaGV = @MaGV", conn))
                    {
                        check.Parameters.Add("@MaGV", SqlDbType.NVarChar, 50).Value = (txtMaGV.Text ?? string.Empty).Trim();
                        int exists = Convert.ToInt32(check.ExecuteScalar());
                        if (exists > 0)
                        {
                            MessageBox.Show("Mã GV đã tồn tại. Vui lòng nhập mã khác.", "Trùng khóa", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            return;
                        }
                    }

                    using (var cmd = new SqlCommand(insertSql, conn))
                    {
                        cmd.Parameters.Add("@MaGV", SqlDbType.NVarChar, 50).Value = (txtMaGV.Text ?? string.Empty).Trim();
                        cmd.Parameters.Add("@Ho", SqlDbType.NVarChar, 50).Value = (txtHo.Text ?? string.Empty).Trim();
                        cmd.Parameters.Add("@Ten", SqlDbType.NVarChar, 50).Value = (txtTen.Text ?? string.Empty).Trim();
                        cmd.Parameters.Add("@Email", SqlDbType.NVarChar, 100).Value = (txtEmail.Text ?? string.Empty).Trim();
                        cmd.Parameters.Add("@SoDienThoai", SqlDbType.VarChar, 15).Value = (txtSoDienThoai.Text ?? string.Empty).Trim();
                        cmd.Parameters.Add("@Khoa", SqlDbType.NVarChar, 255).Value = (txtKhoa.Text ?? string.Empty).Trim();
                        cmd.Parameters.Add("@GhiChu", SqlDbType.NVarChar, 255).Value = (txtGhiChu.Text ?? string.Empty).Trim();

                        cmd.ExecuteNonQuery();
                    }
                }

                MessageBox.Show("Thêm giảng viên thành công!", "Thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);
                HienThi();

                // Clear form
                txtMaGV.Clear();
                txtHo.Clear();
                txtTen.Clear();
                txtEmail.Clear();
                txtSoDienThoai.Clear();
                txtKhoa.Clear();
                txtGhiChu.Clear();
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

        private void dgvGiangVien_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (dgvGiangVien.CurrentRow == null) return;

            txtMaGV.Text = dgvGiangVien.CurrentRow.Cells[0].Value?.ToString() ?? string.Empty;
            txtHo.Text = dgvGiangVien.CurrentRow.Cells[1].Value?.ToString() ?? string.Empty;
            txtTen.Text = dgvGiangVien.CurrentRow.Cells[2].Value?.ToString() ?? string.Empty;
            txtEmail.Text = dgvGiangVien.CurrentRow.Cells[3].Value?.ToString() ?? string.Empty;
            txtSoDienThoai.Text = dgvGiangVien.CurrentRow.Cells[4].Value?.ToString() ?? string.Empty;
            txtKhoa.Text = dgvGiangVien.CurrentRow.Cells[5].Value?.ToString() ?? string.Empty;
            txtGhiChu.Text = dgvGiangVien.CurrentRow.Cells[6].Value?.ToString() ?? string.Empty;

        }
        private void label2_Click(object sender, EventArgs e)
        {

        }

        private void label6_Click(object sender, EventArgs e)
        {

        }

        private void txbGhiChu_TextChanged(object sender, EventArgs e)
        {

        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {

        }

        private void label7_Click(object sender, EventArgs e)
        {

        }

        private void btnSua_Click(object sender, EventArgs e)
        {
            // Validate cơ bản
            if (string.IsNullOrWhiteSpace(txtMaGV.Text))
            {
                MessageBox.Show("Vui lòng chọn hoặc nhập Mã GV cần sửa.", "Thiếu thông tin", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(txtHo.Text) && string.IsNullOrWhiteSpace(txtTen.Text)
                && string.IsNullOrWhiteSpace(txtSoDienThoai.Text) && string.IsNullOrWhiteSpace(txtKhoa.Text)
                && string.IsNullOrWhiteSpace(txtEmail.Text))
            {
                MessageBox.Show("Không có thay đổi để cập nhật.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            const string updateSql = @"UPDATE GiangVien
                                        SET Ho = @Ho,
                                            Ten = @Ten,
                                            Email = @Email,
                                            SoDienThoai = @SoDienThoai,
                                            Khoa = @Khoa,
                                            GhiChu = @GhiChu
                                      WHERE MaGV = @MaGV";

            try
            {
                using (var conn = new SqlConnection(Nguon))
                using (var cmd = new SqlCommand(updateSql, conn))
                {
                    cmd.Parameters.Add("@MaGV", SqlDbType.NVarChar, 50).Value = (txtMaGV.Text ?? string.Empty).Trim();
                    cmd.Parameters.Add("@Ho", SqlDbType.NVarChar, 50).Value = (txtHo.Text ?? string.Empty).Trim();
                    cmd.Parameters.Add("@Ten", SqlDbType.NVarChar, 50).Value = (txtTen.Text ?? string.Empty).Trim();
                    cmd.Parameters.Add("@Email", SqlDbType.NVarChar, 100).Value = (txtEmail.Text ?? string.Empty).Trim();
                    cmd.Parameters.Add("@SoDienThoai", SqlDbType.VarChar, 15).Value = (txtSoDienThoai.Text ?? string.Empty).Trim();
                    cmd.Parameters.Add("@Khoa", SqlDbType.NVarChar, 255).Value = (txtKhoa.Text ?? string.Empty).Trim();
                    cmd.Parameters.Add("@GhiChu", SqlDbType.NVarChar, 255).Value = (txtGhiChu.Text ?? string.Empty).Trim();

                    conn.Open();
                    int affected = cmd.ExecuteNonQuery();
                    if (affected == 0)
                    {
                        MessageBox.Show("Không tìm thấy Mã GV để cập nhật.", "Không tồn tại", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }
                }

                MessageBox.Show("Cập nhật giảng viên thành công!", "Thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);
                HienThi();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi cập nhật giảng viên: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnXoa_Click(object sender, EventArgs e)
        {
            string maGv = (txtMaGV.Text ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(maGv))
            {
                MessageBox.Show("Vui lòng chọn Mã GV để xóa.", "Thiếu thông tin", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var confirm = MessageBox.Show($"Bạn có chắc muốn xóa giảng viên có Mã GV: {maGv}?", "Xác nhận xóa", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (confirm != DialogResult.Yes)
            {
                return;
            }

            const string deleteSql = "DELETE FROM GiangVien WHERE MaGV = @MaGV";

            try
            {
                using (var conn = new SqlConnection(Nguon))
                using (var cmd = new SqlCommand(deleteSql, conn))
                {
                    cmd.Parameters.Add("@MaGV", SqlDbType.NVarChar, 50).Value = maGv;
                    conn.Open();
                    int affected = cmd.ExecuteNonQuery();
                    if (affected == 0)
                    {
                        MessageBox.Show("Không tìm thấy Mã GV để xóa.", "Không tồn tại", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        return;
                    }
                }

                MessageBox.Show("Xóa giảng viên thành công!", "Thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);
                HienThi();

                // Clear form
                txtMaGV.Clear();
                txtHo.Clear();
                txtTen.Clear();
                txtEmail.Clear();
                txtSoDienThoai.Clear();
                txtKhoa.Clear();
                txtGhiChu.Clear();
            }
            catch (SqlException ex)
            {
                // Khả năng có ràng buộc khóa ngoại
                MessageBox.Show("Không thể xóa do ràng buộc dữ liệu: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi xóa giảng viên: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnXuatFile_Click(object sender, EventArgs e)
        {
            if (dgvGiangVien.Rows.Count == 0)
            {
                MessageBox.Show("Không có dữ liệu để xuất.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            using (var sfd = new SaveFileDialog())
            {
                sfd.Title = "Xuất dữ liệu giảng viên";
                sfd.Filter = "CSV (Excel)|*.csv";
                sfd.FileName = "GiangVien.csv";
                if (sfd.ShowDialog() != DialogResult.OK)
                {
                    return;
                }

                try
                {
                    var sb = new StringBuilder();

                    // Header
                    for (int c = 0; c < dgvGiangVien.Columns.Count; c++)
                    {
                        if (c > 0) sb.Append(',');
                        sb.Append(EscapeCsv(dgvGiangVien.Columns[c].HeaderText));
                    }
                    sb.AppendLine();

                    // Rows
                    foreach (DataGridViewRow row in dgvGiangVien.Rows)
                    {
                        if (row.IsNewRow) continue;
                        for (int c = 0; c < dgvGiangVien.Columns.Count; c++)
                        {
                            if (c > 0) sb.Append(',');
                            var value = row.Cells[c].Value?.ToString() ?? string.Empty;
                            sb.Append(EscapeCsv(value));
                        }
                        sb.AppendLine();
                    }

                    // Write UTF-8 with BOM so Excel reads Vietnamese correctly
                    var utf8WithBom = new UTF8Encoding(true);
                    File.WriteAllText(sfd.FileName, sb.ToString(), utf8WithBom);

                    MessageBox.Show("Xuất file thành công!", "Thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Lỗi khi xuất file: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private static string EscapeCsv(string input)
        {
            if (input.IndexOfAny(new[] { ',', '"', '\n', '\r' }) >= 0)
            {
                return '"' + input.Replace("\"", "\"\"") + '"';
            }
            return input;
        }

        private void btnThoat_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void btnThoat_Click_1(object sender, EventArgs e)
        {

        }

        private void GiangVien_Load(object sender, EventArgs e)
        {
            
        }

        private void quảnLýMônHọcToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MonHoc mh = new MonHoc();
            this.Hide();
            mh.ShowDialog();
            this.Show();
        }

        private void quảnLýSinhViênToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        private void quảnLýGiáoViênToolStripMenuItem_Click(object sender, EventArgs e)
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
