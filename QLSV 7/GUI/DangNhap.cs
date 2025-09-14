using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace QLSV_7.GUI
{
    /// <summary>
    /// Form đăng nhập hệ thống
    /// Xác thực người dùng và chuyển hướng đến form chính
    /// </summary>
    public partial class DangNhap : Form
    {
        /// <summary>
        /// Chuỗi kết nối database SQL Server
        /// Sử dụng Windows Authentication và mã hóa kết nối
        /// </summary>
        private string Nguon = @"Data Source=DESKTOP-5LSPDHV\SQLEXPRESS01;Initial Catalog=QLSV;Integrated Security=True;Encrypt=True;TrustServerCertificate=True";
        
        /// <summary>
        /// Khởi tạo form DangNhap
        /// Thiết lập các thuộc tính mặc định cho các control
        /// </summary>
        public DangNhap()
        {
            InitializeComponent();
            // Đảm bảo mật khẩu được ẩn mặc định
            txtMatKhau.UseSystemPasswordChar = true;
            txtMatKhau.PasswordChar = '*'; // Sử dụng dấu * để ẩn mật khẩu
            cbHienThiMatKhau.Checked = false; // Đảm bảo checkbox không được tick
        }

        /// <summary>
        /// Kiểm tra thông tin đăng nhập của người dùng
        /// Truy vấn database để xác thực tên đăng nhập và mật khẩu
        /// </summary>
        /// <param name="tenDangNhap">Tên đăng nhập</param>
        /// <param name="matKhau">Mật khẩu</param>
        /// <returns>True nếu đăng nhập thành công, False nếu thất bại</returns>
        private bool KiemTraDangNhap(string tenDangNhap, string matKhau)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(Nguon))
                {
                    connection.Open();
                    // Câu lệnh SQL kiểm tra tài khoản
                    string query = "SELECT COUNT(*) FROM TaiKhoan WHERE TenDangNhap = @TenDangNhap AND MatKhau = @MatKhau";
                    
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        // Thêm tham số để tránh SQL Injection
                        command.Parameters.AddWithValue("@TenDangNhap", tenDangNhap);
                        command.Parameters.AddWithValue("@MatKhau", matKhau);
                        
                        int count = (int)command.ExecuteScalar();
                        return count > 0; // Trả về true nếu tìm thấy tài khoản
                    }
                }
            }
            catch (Exception ex)
            {
                // Hiển thị thông báo lỗi nếu có lỗi kết nối database
                MessageBox.Show("Lỗi kết nối database: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }

      
      
        /// <summary>
        /// Lấy vai trò của người dùng từ database
        /// </summary>
        /// <param name="tenDangNhap">Tên đăng nhập</param>
        /// <returns>Vai trò của người dùng, trả về chuỗi rỗng nếu không tìm thấy</returns>
        private string LayVaiTro(string tenDangNhap)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(Nguon))
                {
                    connection.Open();
                    // Câu lệnh SQL lấy vai trò
                    string query = "SELECT VaiTro FROM TaiKhoan WHERE TenDangNhap = @TenDangNhap";
                    
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@TenDangNhap", tenDangNhap);
                        object result = command.ExecuteScalar();
                        return result != null ? result.ToString() : ""; // Trả về vai trò hoặc chuỗi rỗng
                    }
                }
            }
            catch (Exception ex)
            {
                // Hiển thị thông báo lỗi nếu có lỗi xảy ra
                MessageBox.Show("Lỗi lấy thông tin vai trò: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return "";
            }
        }

        /// <summary>
        /// Xử lý sự kiện thay đổi trạng thái checkbox hiển thị mật khẩu
        /// Cho phép người dùng hiển thị hoặc ẩn mật khẩu
        /// </summary>
        /// <param name="sender">CheckBox hiển thị mật khẩu</param>
        /// <param name="e">Tham số sự kiện</param>
        private void cbHienThiMatKhau_CheckedChanged(object sender, EventArgs e)
        {
            // Hiển thị hoặc ẩn mật khẩu dựa trên trạng thái checkbox
            if (cbHienThiMatKhau.Checked)
            {
                txtMatKhau.UseSystemPasswordChar = false; // Hiển thị mật khẩu
                txtMatKhau.PasswordChar = '\0'; // Xóa ký tự ẩn
            }
            else
            {
                txtMatKhau.UseSystemPasswordChar = true; // Ẩn mật khẩu
                txtMatKhau.PasswordChar = '*'; // Sử dụng dấu * để ẩn
            }
        }

        /// <summary>
        /// Xử lý sự kiện click nút Đăng nhập
        /// Thực hiện quá trình xác thực và đăng nhập vào hệ thống
        /// </summary>
        /// <param name="sender">Nút Đăng nhập</param>
        /// <param name="e">Tham số sự kiện</param>
        private void btnDangNhap_Click(object sender, EventArgs e)
        {
            try
            {
                // Kiểm tra dữ liệu đầu vào - tên đăng nhập
                if (string.IsNullOrWhiteSpace(txtTenDangNhap.Text))
                {
                    MessageBox.Show("Vui lòng nhập tên đăng nhập!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    txtTenDangNhap.Focus();
                    return;
                }

                // Kiểm tra dữ liệu đầu vào - mật khẩu
                if (string.IsNullOrWhiteSpace(txtMatKhau.Text))
                {
                    MessageBox.Show("Vui lòng nhập mật khẩu!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    txtMatKhau.Focus();
                    return;
                }

                // Lấy thông tin đăng nhập và loại bỏ khoảng trắng
                string tenDangNhap = txtTenDangNhap.Text.Trim();
                string matKhau = txtMatKhau.Text.Trim();

                // Kiểm tra thông tin đăng nhập
                if (KiemTraDangNhap(tenDangNhap, matKhau))
                {
                    // Lấy vai trò của người dùng
                    string vaiTro = LayVaiTro(tenDangNhap);
                    
                    // Thông báo đăng nhập thành công
                    MessageBox.Show($"Đăng nhập thành công!\nVai trò: {vaiTro}", "Thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    
                    // Mở form chính (TrangChu) và ẩn form đăng nhập
                    try
                    {
                        TrangChu trangChu = new TrangChu();
                        trangChu.Show();
                        this.Hide();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Lỗi khi mở trang chủ: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
                else
                {
                    // Thông báo lỗi đăng nhập và reset form
                    MessageBox.Show("Tên đăng nhập hoặc mật khẩu không đúng!", "Lỗi đăng nhập", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    txtMatKhau.Clear();
                    txtTenDangNhap.Focus();
                }
            }
            catch (Exception ex)
            {
                // Xử lý các lỗi không mong muốn
                MessageBox.Show("Lỗi không mong muốn: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Xử lý sự kiện thay đổi text trong TextBox tên đăng nhập
        /// Có thể thêm validation real-time ở đây nếu cần
        /// </summary>
        /// <param name="sender">TextBox tên đăng nhập</param>
        /// <param name="e">Tham số sự kiện</param>
        private void txtTenDangNhap_TextChanged(object sender, EventArgs e)
        {
            // Có thể thêm validation real-time ở đây nếu cần
        }

        /// <summary>
        /// Xử lý sự kiện thay đổi text trong TextBox mật khẩu
        /// Có thể thêm validation real-time ở đây nếu cần
        /// </summary>
        /// <param name="sender">TextBox mật khẩu</param>
        /// <param name="e">Tham số sự kiện</param>
        private void txtMatKhau_TextChanged(object sender, EventArgs e)
        {
            // Có thể thêm validation real-time ở đây nếu cần
        }

        /// <summary>
        /// Xử lý sự kiện nhấn phím trong TextBox mật khẩu
        /// Cho phép đăng nhập bằng phím Enter
        /// </summary>
        /// <param name="sender">TextBox mật khẩu</param>
        /// <param name="e">Tham số sự kiện phím</param>
        private void txtMatKhau_KeyPress(object sender, KeyPressEventArgs e)
        {
            // Nếu nhấn phím Enter thì thực hiện đăng nhập
            if (e.KeyChar == (char)Keys.Enter)
            {
                btnDangNhap_Click(sender, e);
            }
        }

        /// <summary>
        /// Xử lý sự kiện nhấn phím trong TextBox tên đăng nhập
        /// Chuyển focus đến TextBox mật khẩu khi nhấn Enter
        /// </summary>
        /// <param name="sender">TextBox tên đăng nhập</param>
        /// <param name="e">Tham số sự kiện phím</param>
        private void txtTenDangNhap_KeyPress(object sender, KeyPressEventArgs e)
        {
            // Nếu nhấn phím Enter thì chuyển focus đến TextBox mật khẩu
            if (e.KeyChar == (char)Keys.Enter)
            {
                txtMatKhau.Focus();
            }
        }
    }
}
