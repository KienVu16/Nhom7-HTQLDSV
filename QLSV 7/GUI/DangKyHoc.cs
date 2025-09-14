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
    /// <summary>
    /// Form quản lý đăng ký học
    /// Cho phép sinh viên đăng ký môn học, quản lý thông tin đăng ký
    /// </summary>
    public partial class DangKyHoc : Form
    {
        /// <summary>
        /// Khởi tạo form DangKyHoc
        /// Gọi InitializeComponent() để khởi tạo các control trên form
        /// </summary>
        public DangKyHoc()
        {
            InitializeComponent();
        }
        
        /// <summary>
        /// Chuỗi kết nối đến cơ sở dữ liệu SQL Server
        /// Sử dụng Windows Authentication để xác thực
        /// </summary>
        string Nguon = @"Data Source=DESKTOP-5LSPDHV\SQLEXPRESS01;Initial Catalog=QLSV;Integrated Security=True";
        string Lenh = @"";  
        private void HienThi()
        {
            // Xóa DataSource và xóa tất cả dòng để chuẩn bị load dữ liệu mới
            dgvDangKyHoc.DataSource = null;   
            dgvDangKyHoc.Rows.Clear();

            try
            {
                // Kết nối database và thực hiện truy vấn SELECT
                using (var conn = new SqlConnection(Nguon))
                using (var cmd = new SqlCommand(@"SELECT MaDK, MaSV, MaMH, NgayDangKy, GhiChu FROM DangKyHoc;", conn))
                {
                    conn.Open();
                    using (var reader = cmd.ExecuteReader())
                    {
                        // Đọc từng dòng dữ liệu và thêm vào DataGridView
                        while (reader.Read())
                        {
                            int index = dgvDangKyHoc.Rows.Add();
                            dgvDangKyHoc.Rows[index].Cells[0].Value = reader["MaDK"].ToString();
                            dgvDangKyHoc.Rows[index].Cells[1].Value = reader["MaSV"].ToString();
                            dgvDangKyHoc.Rows[index].Cells[2].Value = reader["MaMH"].ToString();
                            // Chuyển đổi ngày tháng thành định dạng yyyy-MM-dd
                            dgvDangKyHoc.Rows[index].Cells[3].Value = Convert.ToDateTime(reader["NgayDangKy"]).ToString("yyyy-MM-dd");
                            dgvDangKyHoc.Rows[index].Cells[4].Value = reader["GhiChu"].ToString();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Gọi hàm báo lỗi
                ReportError("HienThi()", ex);
            }
        }
        /// <summary>
        /// Sự kiện Load của form DangKyHoc
        /// Khởi tạo dữ liệu và đăng ký các event handler
        /// </summary>
        /// <param name="sender">Đối tượng gọi sự kiện</param>
        /// <param name="e">Tham số sự kiện</param>
        private void DangKyHoc_Load(object sender, EventArgs e) {
            try
            {
                LoadMaSV(); // Load danh sách mã sinh viên vào ComboBox
                LoadMaMH(); // Load danh sách mã môn học vào ComboBox
                HienThi();  // Hiển thị dữ liệu đăng ký học

                // Đảm bảo tất cả events được kết nối ngay cả khi Designer mất hookups
                btnThem.Click += btnThem_Click;
                btnSua.Click += btnSua_Click;
                btnXoa.Click += btnXoa_Click;
                btnXuatFile.Click += btnXuatFile_Click;
                btnThoat.Click += btnThoat_Click;
                dgvDangKyHoc.CellClick += dgvDangKyHoc_CellClick;
            }
            catch (Exception ex)
            {
                // Gọi hàm báo lỗi
                ReportError("DangKyHoc_Load()", ex);
            }
        }
        /// <summary>
        /// Xử lý sự kiện click nút Thêm
        /// Thêm một bản ghi đăng ký học mới vào database
        /// </summary>
        /// <param name="sender">Nút Thêm</param>
        /// <param name="e">Tham số sự kiện</param>
        private void btnThem_Click(object sender, EventArgs e)
        {
            Debug.WriteLine("[UI] btnThem_Click pressed"); // Ghi log debug
            
            // Lấy dữ liệu từ form
            string maDk = (txtMaDK.Text ?? string.Empty).Trim();
            string maSv = cbbMaSV?.SelectedValue?.ToString() ?? string.Empty;
            string maMh = cbbMaMH?.SelectedValue?.ToString() ?? string.Empty;
            
            // Kiểm tra các trường bắt buộc
            if (string.IsNullOrWhiteSpace(maDk)) { MessageBox.Show("Vui lòng nhập Mã DK."); return; }
            if (string.IsNullOrWhiteSpace(maSv)) { MessageBox.Show("Vui lòng chọn Mã SV."); return; }
            if (string.IsNullOrWhiteSpace(maMh)) { MessageBox.Show("Vui lòng chọn Mã MH."); return; }

            // Câu lệnh SQL để thêm dữ liệu đăng ký học mới
            const string insertSql = @"INSERT INTO DangKyHoc (MaDK, MaSV, MaMH, NgayDangKy, GhiChu)
                                       VALUES (@MaDK, @MaSV, @MaMH, @NgayDangKy, @GhiChu)";
            try
            {
                using (var conn = new SqlConnection(Nguon))
                {
                    conn.Open();
                    // Kiểm tra xem mã đăng ký đã tồn tại chưa
                    using (var check = new SqlCommand("SELECT COUNT(1) FROM DangKyHoc WHERE MaDK=@MaDK", conn))
                    {
                        check.Parameters.Add("@MaDK", SqlDbType.NVarChar, 50).Value = maDk;
                        DebugSql(check, "Check exists (insert)"); // Ghi log debug
                        if (Convert.ToInt32(check.ExecuteScalar()) > 0)
                        {
                            MessageBox.Show("Mã DK đã tồn tại.");
                            return;
                        }
                    }
                    // Thực hiện thêm dữ liệu đăng ký học mới
                    using (var cmd = new SqlCommand(insertSql, conn))
                    {
                        // Thêm các tham số vào câu lệnh SQL
                        cmd.Parameters.Add("@MaDK", SqlDbType.NVarChar, 50).Value = maDk;
                        cmd.Parameters.Add("@MaSV", SqlDbType.NVarChar, 50).Value = maSv;
                        cmd.Parameters.Add("@MaMH", SqlDbType.NVarChar, 50).Value = maMh;
                        cmd.Parameters.Add("@NgayDangKy", SqlDbType.Date).Value = dtpNgayDangKy.Value.Date;
                        cmd.Parameters.Add("@GhiChu", SqlDbType.NVarChar, 255).Value = (txtGhiChu.Text ?? string.Empty).Trim();
                        DebugSql(cmd, "Insert DangKyHoc"); // Ghi log debug
                        cmd.ExecuteNonQuery(); // Thực thi câu lệnh INSERT
                    }
                }
                // Thông báo thành công và cập nhật giao diện
                MessageBox.Show("Thêm đăng ký học thành công!");
                HienThi(); // Load lại dữ liệu
            }
            catch (SqlException ex) when (ex.Number == 2627 || ex.Number == 2601)
            {
                // Xử lý lỗi trùng khóa chính
                MessageBox.Show("Mã DK đã tồn tại (trùng khóa).", "Trùng khóa", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            catch (SqlException ex) when (ex.Number == 547)
            {
                // Xử lý lỗi ràng buộc khóa ngoại
                MessageBox.Show("Không thể thêm do ràng buộc khóa ngoại (MaSV/MaMH không hợp lệ).", "Ràng buộc dữ liệu", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            catch (Exception ex)
            {
                // Xử lý các lỗi khác
                ReportError("btnThem_Click", ex);
            }
        }

        /// <summary>
        /// Xử lý sự kiện click nút Sửa
        /// Cập nhật thông tin đăng ký học đã có trong database
        /// </summary>
        /// <param name="sender">Nút Sửa</param>
        /// <param name="e">Tham số sự kiện</param>
        private void btnSua_Click(object sender, EventArgs e)
        {
            Debug.WriteLine("[UI] btnSua_Click pressed"); // Ghi log debug
            
            // Lấy dữ liệu từ form
            string maDk = (txtMaDK.Text ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(maDk)) { MessageBox.Show("Vui lòng nhập/chọn Mã DK."); return; }
            string maSv = cbbMaSV?.SelectedValue?.ToString() ?? string.Empty;
            string maMh = cbbMaMH?.SelectedValue?.ToString() ?? string.Empty;

            // Câu lệnh SQL để cập nhật dữ liệu đăng ký học
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
                        // Kiểm tra ràng buộc nghiệp vụ: không cho phép đổi MaSV
                        if (!string.Equals(existing, maSv, StringComparison.OrdinalIgnoreCase))
                        {
                            MessageBox.Show($"MaDK '{maDk}' đã gắn với MaSV '{existing}'. Không được đổi MaSV.", "Ràng buộc nghiệp vụ", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            return;
                        }
                        conn.Close();
                    }

                    // Thêm các tham số vào câu lệnh SQL
                    cmd.Parameters.Add("@MaDK", SqlDbType.NVarChar, 50).Value = maDk;
                    cmd.Parameters.Add("@MaSV", SqlDbType.NVarChar, 50).Value = maSv;
                    cmd.Parameters.Add("@MaMH", SqlDbType.NVarChar, 50).Value = maMh;
                    cmd.Parameters.Add("@NgayDangKy", SqlDbType.Date).Value = dtpNgayDangKy.Value.Date;
                    cmd.Parameters.Add("@GhiChu", SqlDbType.NVarChar, 255).Value = (txtGhiChu.Text ?? string.Empty).Trim();

                    conn.Open();
                    DebugSql(cmd, "Update DangKyHoc"); // Ghi log debug
                    int n = cmd.ExecuteNonQuery(); // Thực thi câu lệnh UPDATE
                    if (n == 0) { MessageBox.Show("Không tìm thấy Mã DK để cập nhật."); return; }
                }
                // Thông báo thành công và cập nhật giao diện
                MessageBox.Show("Cập nhật thành công!");
                HienThi(); // Load lại dữ liệu
            }
            catch (SqlException ex) when (ex.Number == 547)
            {
                // Xử lý lỗi ràng buộc khóa ngoại
                MessageBox.Show("Ràng buộc dữ liệu vi phạm khi cập nhật (khóa ngoại).", "Ràng buộc dữ liệu", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            catch (Exception ex)
            {
                // Xử lý các lỗi khác
                ReportError("btnSua_Click", ex);
            }
        }

        /// <summary>
        /// Xử lý sự kiện click nút Xóa
        /// Xóa bản ghi đăng ký học khỏi database
        /// </summary>
        /// <param name="sender">Nút Xóa</param>
        /// <param name="e">Tham số sự kiện</param>
        private void btnXoa_Click(object sender, EventArgs e)
        {
            Debug.WriteLine("[UI] btnXoa_Click pressed"); // Ghi log debug
            
            // Lấy mã đăng ký cần xóa
            string maDk = (txtMaDK.Text ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(maDk)) { MessageBox.Show("Vui lòng nhập/chọn Mã DK."); return; }
            
            // Xác nhận trước khi xóa
            if (MessageBox.Show($"Xóa đăng ký {maDk}?", "Xác nhận", MessageBoxButtons.YesNo) != DialogResult.Yes) return;

            try
            {
                // Kết nối database và thực hiện câu lệnh DELETE
                using (var conn = new SqlConnection(Nguon))
                using (var cmd = new SqlCommand("DELETE FROM DangKyHoc WHERE MaDK=@MaDK", conn))
                {
                    cmd.Parameters.Add("@MaDK", SqlDbType.NVarChar, 50).Value = maDk;
                    conn.Open();
                    DebugSql(cmd, "Delete DangKyHoc"); // Ghi log debug
                    int n = cmd.ExecuteNonQuery(); // Thực thi câu lệnh DELETE
                    if (n == 0) { MessageBox.Show("Không tìm thấy Mã DK để xóa."); return; }
                }
                // Thông báo thành công và cập nhật giao diện
                MessageBox.Show("Xóa thành công!");
                HienThi(); // Load lại dữ liệu
            }
            catch (SqlException ex) when (ex.Number == 547)
            {
                // Xử lý lỗi ràng buộc dữ liệu (foreign key constraint)
                MessageBox.Show("Không thể xóa do ràng buộc dữ liệu (bản ghi đang được tham chiếu).", "Ràng buộc dữ liệu", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            catch (Exception ex)
            {
                // Xử lý các lỗi khác
                ReportError("btnXoa_Click", ex);
            }
        }



        /// <summary>
        /// Xử lý sự kiện thay đổi text trong TextBox Ghi chú
        /// Không thực hiện gì
        /// </summary>
        /// <param name="sender">TextBox Ghi chú</param>
        /// <param name="e">Tham số sự kiện</param>
        private void txtGhiChu_TextChanged(object sender, EventArgs e) { }

        /// <summary>
        /// Xử lý sự kiện click vào một dòng trong DataGridView
        /// Load dữ liệu từ dòng được chọn vào các control để chỉnh sửa
        /// </summary>
        /// <param name="sender">DataGridView</param>
        /// <param name="e">Thông tin về ô được click</param>
        private void dgvDangKyHoc_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            Debug.WriteLine("[UI] dgvDangKyHoc_CellClick row=" + e.RowIndex); // Ghi log debug
            if (dgvDangKyHoc.CurrentRow == null) return;
            
            // Load dữ liệu từ dòng được chọn vào các control
            txtMaDK.Text = dgvDangKyHoc.CurrentRow.Cells[0].Value?.ToString() ?? string.Empty;
            var maSv = dgvDangKyHoc.CurrentRow.Cells[1].Value?.ToString() ?? string.Empty;
            var maMh = dgvDangKyHoc.CurrentRow.Cells[2].Value?.ToString() ?? string.Empty;
            if (!string.IsNullOrEmpty(maSv)) cbbMaSV.SelectedValue = maSv;
            if (!string.IsNullOrEmpty(maMh)) cbbMaMH.SelectedValue = maMh;
            
            // Xử lý ngày đăng ký
            var ngay = dgvDangKyHoc.CurrentRow.Cells[3].Value?.ToString() ?? string.Empty;
            if (DateTime.TryParse(ngay, out DateTime d)) dtpNgayDangKy.Value = d; else dtpNgayDangKy.Value = DateTime.Today;
            txtGhiChu.Text = dgvDangKyHoc.CurrentRow.Cells[4].Value?.ToString() ?? string.Empty;
        }

        /// <summary>
        /// Xử lý sự kiện click nút Thoát
        /// Đóng form hiện tại
        /// </summary>
        /// <param name="sender">Nút Thoát</param>
        /// <param name="e">Tham số sự kiện</param>
        private void btnThoat_Click(object sender, EventArgs e)
        {
            this.Close(); // Đóng form
        }

        /// <summary>
        /// Xử lý sự kiện vẽ TableLayoutPanel 3
        /// Không thực hiện gì
        /// </summary>
        /// <param name="sender">TableLayoutPanel</param>
        /// <param name="e">Tham số sự kiện vẽ</param>
        private void tableLayoutPanel3_Paint(object sender, PaintEventArgs e)
        {
            // Không thực hiện gì
        }

        /// <summary>
        /// Xử lý sự kiện thay đổi lựa chọn trong ComboBox Mã SV
        /// Lọc và hiển thị dữ liệu đăng ký học theo mã sinh viên được chọn
        /// </summary>
        /// <param name="sender">ComboBox Mã SV</param>
        /// <param name="e">Tham số sự kiện</param>
        private void cbbMaSV_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                // Lấy giá trị được chọn
                var selected = cbbMaSV.SelectedValue?.ToString();
                if (string.IsNullOrWhiteSpace(selected) || selected == "System.Data.DataRowView") return;

                // Kết nối database và thực hiện truy vấn lọc theo Mã SV
                using (var conn = new SqlConnection(Nguon))
                using (var cmd = new SqlCommand(@"SELECT MaDK, MaSV, MaMH, NgayDangKy, GhiChu
                                                 FROM DangKyHoc
                                                 WHERE MaSV = @MaSV
                                                 ORDER BY NgayDangKy DESC;", conn))
                {
                    cmd.Parameters.Add("@MaSV", SqlDbType.NVarChar, 50).Value = selected;
                    DisplayToGrid(cmd); // Hiển thị kết quả lọc
                }
            }
            catch (Exception ex)
            {
                // Hiển thị thông báo lỗi nếu có lỗi xảy ra
                MessageBox.Show("Lỗi lọc theo Mã SV: " + ex.Message);
            }
        }

        /// <summary>
        /// Xử lý sự kiện thay đổi lựa chọn trong ComboBox Mã MH
        /// Lọc và hiển thị dữ liệu đăng ký học theo mã môn học được chọn
        /// </summary>
        /// <param name="sender">ComboBox Mã MH</param>
        /// <param name="e">Tham số sự kiện</param>
        private void cbbMaMH_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                // Lấy giá trị được chọn
                var selected = cbbMaMH.SelectedValue?.ToString();
                if (string.IsNullOrWhiteSpace(selected) || selected == "System.Data.DataRowView") return;

                // Kết nối database và thực hiện truy vấn lọc theo Mã MH
                using (var conn = new SqlConnection(Nguon))
                using (var cmd = new SqlCommand(@"SELECT MaDK, MaSV, MaMH, NgayDangKy, GhiChu
                                                 FROM DangKyHoc
                                                 WHERE MaMH = @MaMH
                                                 ORDER BY NgayDangKy DESC;", conn))
                {
                    cmd.Parameters.Add("@MaMH", SqlDbType.NVarChar, 50).Value = selected;
                    DisplayToGrid(cmd); // Hiển thị kết quả lọc
                }
            }
            catch (Exception ex)
            {
                // Hiển thị thông báo lỗi nếu có lỗi xảy ra
                MessageBox.Show("Lỗi lọc theo Mã MH: " + ex.Message);
            }
        }

        /// <summary>
        /// Load danh sách mã sinh viên vào ComboBox
        /// Lấy tất cả mã sinh viên từ bảng SinhVien
        /// </summary>
        private void LoadMaSV()
        {
            using (var conn = new SqlConnection(Nguon))
            using (var cmd = new SqlCommand("SELECT MaSV FROM SinhVien ORDER BY MaSV;", conn))
            using (var da = new SqlDataAdapter(cmd))
            {
                var dt = new DataTable();
                conn.Open();
                da.Fill(dt); // Đổ dữ liệu vào DataTable
                
                // Gán dữ liệu cho ComboBox
                cbbMaSV.DataSource = dt;
                cbbMaSV.DisplayMember = "MaSV"; // Cột hiển thị
                cbbMaSV.ValueMember = "MaSV";   // Cột giá trị
                
                // Kiểm tra nếu không có dữ liệu
                if (dt.Rows.Count == 0)
                {
                    MessageBox.Show("Chưa có dữ liệu SinhVien để chọn.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
        }

        /// <summary>
        /// Load danh sách mã môn học vào ComboBox
        /// Lấy tất cả mã môn học từ bảng MonHoc
        /// </summary>
        private void LoadMaMH()
        {
            using (var conn = new SqlConnection(Nguon))
            using (var cmd = new SqlCommand("SELECT MaMH FROM MonHoc ORDER BY MaMH;", conn))
            using (var da = new SqlDataAdapter(cmd))
            {
                var dt = new DataTable();
                conn.Open();
                da.Fill(dt); // Đổ dữ liệu vào DataTable
                
                // Gán dữ liệu cho ComboBox
                cbbMaMH.DataSource = dt;
                cbbMaMH.DisplayMember = "MaMH"; // Cột hiển thị
                cbbMaMH.ValueMember = "MaMH";   // Cột giá trị
                
                // Kiểm tra nếu không có dữ liệu
                if (dt.Rows.Count == 0)
                {
                    MessageBox.Show("Chưa có dữ liệu MonHoc để chọn.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
        }

        /// <summary>
        /// Hiển thị kết quả truy vấn SQL lên DataGridView
        /// </summary>
        /// <param name="cmd">SqlCommand chứa câu lệnh SQL cần thực thi</param>
        private void DisplayToGrid(SqlCommand cmd)
        {
            // Xóa dữ liệu cũ trong DataGridView
            dgvDangKyHoc.DataSource = null;
            dgvDangKyHoc.Rows.Clear();

            using (cmd.Connection)
            {
                cmd.Connection.Open();
                using (var reader = cmd.ExecuteReader())
                {
                    int count = 0;
                    // Đọc từng dòng dữ liệu và thêm vào DataGridView
                    while (reader.Read())
                    {
                        int index = dgvDangKyHoc.Rows.Add();
                        dgvDangKyHoc.Rows[index].Cells[0].Value = reader["MaDK"].ToString();
                        dgvDangKyHoc.Rows[index].Cells[1].Value = reader["MaSV"].ToString();
                        dgvDangKyHoc.Rows[index].Cells[2].Value = reader["MaMH"].ToString();
                        // Chuyển đổi ngày tháng thành định dạng yyyy-MM-dd
                        dgvDangKyHoc.Rows[index].Cells[3].Value = Convert.ToDateTime(reader["NgayDangKy"]).ToString("yyyy-MM-dd");
                        dgvDangKyHoc.Rows[index].Cells[4].Value = reader["GhiChu"].ToString();
                        count++;
                    }
                    // Thông báo nếu không có dữ liệu
                    if (count == 0)
                    {
                        MessageBox.Show("Không có bản ghi phù hợp.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
            }
        }

        /// <summary>
        /// Ghi log debug cho câu lệnh SQL
        /// Hiển thị câu lệnh SQL và các tham số
        /// </summary>
        /// <param name="cmd">SqlCommand cần ghi log</param>
        /// <param name="context">Ngữ cảnh thực thi</param>
        private void DebugSql(SqlCommand cmd, string context)
        {
            var sb = new StringBuilder();
            sb.AppendLine($"[{DateTime.Now:HH:mm:ss}] {context}"); // Thời gian và ngữ cảnh
            sb.AppendLine(cmd.CommandText); // Câu lệnh SQL
            
            // Ghi các tham số
            foreach (SqlParameter p in cmd.Parameters)
            {
                sb.AppendLine($"  {p.ParameterName} = {p.Value ?? "<null>"}");
            }
            Debug.WriteLine(sb.ToString()); // Ghi vào debug output
        }

        /// <summary>
        /// Báo cáo lỗi và hiển thị thông báo cho người dùng
        /// </summary>
        /// <param name="context">Ngữ cảnh xảy ra lỗi</param>
        /// <param name="ex">Exception xảy ra</param>
        private void ReportError(string context, Exception ex)
        {
            Debug.WriteLine($"[{DateTime.Now:HH:mm:ss}] ERROR {context}: {ex}"); // Ghi log debug
            MessageBox.Show($"{context}: {ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error); // Hiển thị thông báo lỗi
        }

        /// <summary>
        /// Xử lý sự kiện vẽ TableLayoutPanel 1
        /// Không thực hiện gì
        /// </summary>
        /// <param name="sender">TableLayoutPanel</param>
        /// <param name="e">Tham số sự kiện vẽ</param>
        private void tableLayoutPanel1_Paint(object sender, PaintEventArgs e)
        {
            // Không thực hiện gì
        }

        /// <summary>
        /// Xử lý sự kiện click menu Quản lý Sinh viên
        /// Không thực hiện gì
        /// </summary>
        /// <param name="sender">Menu item</param>
        /// <param name="e">Tham số sự kiện</param>
        private void quảnLýSinhViênToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // Không thực hiện gì
        }

        /// <summary>
        /// Xử lý sự kiện click menu Quản lý Giáo viên
        /// Không thực hiện gì
        /// </summary>
        /// <param name="sender">Menu item</param>
        /// <param name="e">Tham số sự kiện</param>
        private void quảnLýGiáoViênToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // Không thực hiện gì
        }

        /// <summary>
        /// Xử lý sự kiện click menu Quản lý Môn học
        /// Không thực hiện gì
        /// </summary>
        /// <param name="sender">Menu item</param>
        /// <param name="e">Tham số sự kiện</param>
        private void quảnLýMônHọcToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // Không thực hiện gì
        }

        /// <summary>
        /// Xử lý sự kiện click menu Quản lý Điểm
        /// Không thực hiện gì
        /// </summary>
        /// <param name="sender">Menu item</param>
        /// <param name="e">Tham số sự kiện</param>
        private void quảnLýĐiểmToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // Không thực hiện gì
        }

        /// <summary>
        /// Xử lý sự kiện click menu Quản lý Đăng ký học
        /// Không thực hiện gì
        /// </summary>
        /// <param name="sender">Menu item</param>
        /// <param name="e">Tham số sự kiện</param>
        private void quảnLýĐăngKýHọcToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // Không thực hiện gì
        }

        /// <summary>
        /// Xử lý sự kiện click menu item 6
        /// Không thực hiện gì
        /// </summary>
        /// <param name="sender">Menu item</param>
        /// <param name="e">Tham số sự kiện</param>
        private void toolStripMenuItem6_Click(object sender, EventArgs e)
        {
            // Không thực hiện gì
        }

        /// <summary>
        /// Xử lý sự kiện click menu Đăng xuất
        /// Không thực hiện gì
        /// </summary>
        /// <param name="sender">Menu item</param>
        /// <param name="e">Tham số sự kiện</param>
        private void đăngXuấtToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // Không thực hiện gì
        }

        /// <summary>
        /// Xử lý sự kiện click nút Xuất File
        /// Xuất dữ liệu đăng ký học ra file CSV
        /// </summary>
        /// <param name="sender">Nút Xuất File</param>
        /// <param name="e">Tham số sự kiện</param>
        private void btnXuatFile_Click(object sender, EventArgs e)
        {
            // Kiểm tra xem có dữ liệu để xuất không
            if (dgvDangKyHoc.Rows.Count > 0)
            {
                // Hiển thị dialog chọn file để lưu
                using (SaveFileDialog sfd = new SaveFileDialog() { Filter = "CSV file|*.csv" })
                {
                    if (sfd.ShowDialog() == DialogResult.OK)
                    {
                        try
                        {
                            // Tạo StreamWriter để ghi file CSV
                            using (StreamWriter sw = new StreamWriter(sfd.FileName, false, Encoding.UTF8))
                            {
                                // Thêm tiêu đề cho file CSV
                                sw.WriteLine("DANH SÁCH SINH VIÊN");

                                // Ghi header (tên các cột)
                                for (int i = 0; i < dgvDangKyHoc.Columns.Count; i++)
                                {
                                    sw.Write(dgvDangKyHoc.Columns[i].HeaderText);
                                    if (i <     dgvDangKyHoc.Columns.Count - 1)
                                        sw.Write(",");
                                }
                                sw.WriteLine();

                                // Ghi dữ liệu từng dòng
                                for (int i = 0; i < dgvDangKyHoc.Rows.Count; i++)
                                {
                                    for (int j = 0; j < dgvDangKyHoc.Columns.Count; j++)
                                    {
                                        var value = dgvDangKyHoc.Rows[i].Cells[j].Value;

                                        // Xử lý định dạng ngày tháng
                                        if (dgvDangKyHoc.Columns[j].Name == "NgaySinh" && value != null)
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

                                        if (j < dgvDangKyHoc.Columns.Count - 1)
                                            sw.Write(",");
                                    }
                                    sw.WriteLine(); // Xuống dòng sau mỗi dòng dữ liệu
                                }
                            }

                            // Thông báo xuất file thành công
                            MessageBox.Show("Xuất CSV thành công!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                        catch (Exception ex)
                        {
                            // Xử lý lỗi khi xuất file
                            MessageBox.Show("Lỗi: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                }
            }
            else
            {
                // Thông báo nếu không có dữ liệu để xuất
                MessageBox.Show("Không có dữ liệu để xuất!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        /// <summary>
        /// Escape chuỗi để phù hợp với định dạng CSV
        /// Thêm dấu ngoặc kép nếu chuỗi chứa dấu phẩy, ngoặc kép, hoặc ký tự xuống dòng
        /// </summary>
        /// <param name="input">Chuỗi cần escape</param>
        /// <returns>Chuỗi đã được escape</returns>
        private static string CsvEscape(string input)
        {
            if (input == null) return string.Empty;
            // Kiểm tra xem có cần thêm dấu ngoặc kép không
            bool mustQuote = input.Contains(",") || input.Contains("\"") || input.Contains("\r") || input.Contains("\n");
            // Escape dấu ngoặc kép bằng cách thay thế " thành ""
            string escaped = input.Replace("\"", "\"\"");
            return mustQuote ? "\"" + escaped + "\"" : escaped;
        }
    }
}
