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

namespace QLSV_7.GUI
{
    public partial class DangNhap : Form
    {
        private int loginAttempts = 0;
        private const int maxLoginAttempts = 3;
        private DateTime lastLoginAttempt = DateTime.MinValue;
        
        public DangNhap()
        {
            InitializeComponent();
            SetupForm();
        }

        string Nguon = @"Data Source=DESKTOP-5LSPDHV\SQLEXPRESS01;Initial Catalog=QLSV;Integrated Security=True";

        private void SetupForm()
        {
            // Thiết lập form
            this.FormBorderStyle = FormBorderStyle.None;
            this.StartPosition = FormStartPosition.CenterScreen;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            
            // Thiết lập focus cho textbox đầu tiên
            txtTenDangNhap.Focus();
            
            // Thiết lập placeholder text
            SetPlaceholderText();
        }

        private void SetPlaceholderText()
        {
            // Thêm placeholder text cho các textbox
            txtTenDangNhap.GotFocus += (s, e) => {
                if (txtTenDangNhap.Text == "Nhập tên đăng nhập")
                {
                    txtTenDangNhap.Text = "";
                    txtTenDangNhap.ForeColor = Color.Black;
                }
            };
            
            txtTenDangNhap.LostFocus += (s, e) => {
                if (string.IsNullOrWhiteSpace(txtTenDangNhap.Text))
                {
                    txtTenDangNhap.Text = "Nhập tên đăng nhập";
                    txtTenDangNhap.ForeColor = Color.Gray;
                }
            };
            
            txtMatKhau.GotFocus += (s, e) => {
                if (txtMatKhau.Text == "Nhập mật khẩu")
                {
                    txtMatKhau.Text = "";
                    txtMatKhau.ForeColor = Color.Black;
                    txtMatKhau.PasswordChar = '●';
                }
            };
            
            txtMatKhau.LostFocus += (s, e) => {
                if (string.IsNullOrWhiteSpace(txtMatKhau.Text))
                {
                    txtMatKhau.Text = "Nhập mật khẩu";
                    txtMatKhau.ForeColor = Color.Gray;
                    txtMatKhau.PasswordChar = '\0';
                }
            };
            
            // Thiết lập placeholder ban đầu
            txtTenDangNhap.Text = "Nhập tên đăng nhập";
            txtTenDangNhap.ForeColor = Color.Gray;
            txtMatKhau.Text = "Nhập mật khẩu";
            txtMatKhau.ForeColor = Color.Gray;
            txtMatKhau.PasswordChar = '\0';
        }

        private void btnDangNhap_Click(object sender, EventArgs e)
        {
            // Kiểm tra thời gian chờ giữa các lần đăng nhập
            if (loginAttempts >= maxLoginAttempts)
            {
                var timeSinceLastAttempt = DateTime.Now - lastLoginAttempt;
                if (timeSinceLastAttempt.TotalMinutes < 5)
                {
                    var remainingTime = TimeSpan.FromMinutes(5) - timeSinceLastAttempt;
                    MessageBox.Show($"Bạn đã đăng nhập sai quá nhiều lần. Vui lòng thử lại sau {remainingTime.Minutes} phút {remainingTime.Seconds} giây.", 
                        "Tài khoản tạm khóa", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                else
                {
                    loginAttempts = 0; // Reset sau 5 phút
                }
            }

            string tenDangNhap = txtTenDangNhap.Text.Trim();
            string matKhau = txtMatKhau.Text.Trim();

            // Kiểm tra placeholder text
            if (tenDangNhap == "Nhập tên đăng nhập" || string.IsNullOrWhiteSpace(tenDangNhap))
            {
                ShowErrorMessage("Vui lòng nhập tên đăng nhập!", txtTenDangNhap);
                return;
            }

            if (matKhau == "Nhập mật khẩu" || string.IsNullOrWhiteSpace(matKhau))
            {
                ShowErrorMessage("Vui lòng nhập mật khẩu!", txtMatKhau);
                return;
            }

            // Kiểm tra độ dài tên đăng nhập
            if (tenDangNhap.Length < 3)
            {
                ShowErrorMessage("Tên đăng nhập phải có ít nhất 3 ký tự!", txtTenDangNhap);
                return;
            }

            // Kiểm tra độ dài mật khẩu
            if (matKhau.Length < 6)
            {
                ShowErrorMessage("Mật khẩu phải có ít nhất 6 ký tự!", txtMatKhau);
                return;
            }

            try
            {
                using (var conn = new SqlConnection(Nguon))
                using (var cmd = new SqlCommand("SELECT MaTK, TenDangNhap, MatKhau, VaiTro FROM TaiKhoan WHERE TenDangNhap = @TenDangNhap AND MatKhau = @MatKhau", conn))
                {
                    cmd.Parameters.Add("@TenDangNhap", SqlDbType.NVarChar, 100).Value = tenDangNhap;
                    cmd.Parameters.Add("@MatKhau", SqlDbType.NVarChar, 100).Value = matKhau;
                    
                    conn.Open();
                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            string maTK = reader["MaTK"].ToString();
                            string vaiTro = reader["VaiTro"].ToString();
                            
                            // Reset số lần đăng nhập sai
                            loginAttempts = 0;
                            
                            // Hiển thị thông báo thành công
                            ShowSuccessMessage($"Đăng nhập thành công!\nMã TK: {maTK}\nVai trò: {vaiTro}");
                            
                            // Mở form chính
                            var trangChu = new TrangChu();
                            this.Hide();
                            trangChu.ShowDialog();
                            this.Close();
                        }
                        else
                        {
                            HandleLoginFailure();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi kết nối cơ sở dữ liệu: " + ex.Message, 
                    "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ShowErrorMessage(string message, Control focusControl)
        {
            MessageBox.Show(message, "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            focusControl.Focus();
            focusControl.SelectAll();
        }

        private void ShowSuccessMessage(string message)
        {
            MessageBox.Show(message, "Thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void HandleLoginFailure()
        {
            loginAttempts++;
            lastLoginAttempt = DateTime.Now;
            
            string message = "Tên đăng nhập hoặc mật khẩu không đúng!";
            
            if (loginAttempts >= maxLoginAttempts)
            {
                message += $"\n\nBạn đã đăng nhập sai {maxLoginAttempts} lần. Tài khoản sẽ bị khóa tạm thời trong 5 phút.";
            }
            else
            {
                message += $"\n\nCòn lại {maxLoginAttempts - loginAttempts} lần thử.";
            }
            
            MessageBox.Show(message, "Lỗi đăng nhập", MessageBoxButtons.OK, MessageBoxIcon.Error);
            
            txtMatKhau.Clear();
            txtMatKhau.Text = "Nhập mật khẩu";
            txtMatKhau.ForeColor = Color.Gray;
            txtMatKhau.PasswordChar = '\0';
            txtMatKhau.Focus();
        }

        private void btnThoat_Click(object sender, EventArgs e)
        {
            var result = MessageBox.Show("Bạn có chắc muốn thoát chương trình?", 
                "Xác nhận", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            
            if (result == DialogResult.Yes)
            {
                Application.Exit();
            }
        }

        private void txtTenDangNhap_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)Keys.Enter)
            {
                // Xử lý placeholder text khi nhấn Enter
                if (txtTenDangNhap.Text == "Nhập tên đăng nhập")
                {
                    txtTenDangNhap.Text = "";
                    txtTenDangNhap.ForeColor = Color.Black;
                }
                txtMatKhau.Focus();
            }
        }

        private void txtMatKhau_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)Keys.Enter)
            {
                // Xử lý placeholder text khi nhấn Enter
                if (txtMatKhau.Text == "Nhập mật khẩu")
                {
                    txtMatKhau.Text = "";
                    txtMatKhau.ForeColor = Color.Black;
                    txtMatKhau.PasswordChar = '●';
                }
                btnDangNhap_Click(sender, e);
            }
        }

        // Xử lý sự kiện quên mật khẩu
        private void lblForgotPassword_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            MessageBox.Show("Vui lòng liên hệ quản trị viên để được hỗ trợ đặt lại mật khẩu.\n\nEmail: admin@qlsv.edu.vn\nĐiện thoại: 0123-456-789", 
                "Quên mật khẩu", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void lblWelcome_Click(object sender, EventArgs e)
        {

        }
    }
}
