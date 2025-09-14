using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace QLSV_7.GUI
{
    public partial class TrangChu : Form
    {
        public TrangChu()
        {
            InitializeComponent();
        }

        private void OpenChild<T>() where T : Form, new()
        {
            var existing = this.MdiChildren.OfType<T>().FirstOrDefault();
            if (existing != null)
            {
                existing.WindowState = FormWindowState.Maximized;
                existing.Activate();
                existing.BringToFront();
                return;
            }
            var child = new T
            {
                MdiParent = this,
                WindowState = FormWindowState.Maximized
            };
            child.Show();
        }
        private void quảnLýSinhViênToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenChild<SinhVien>();
        }

        private void quảnLýGiáoViênToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenChild<GiangVien>();
        }

        private void quảnLýĐiểmToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenChild<BangDiem>();
        }

        private void quảnLýĐăngKýHọcToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenChild<DangKyHoc>();
        }

        private void tableLayoutPanel1_Paint(object sender, PaintEventArgs e)
        {

        }

        private void thêmTàiKhoảnToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            OpenChild<TaiKhoan>();
        }

        private void quảnLýToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        private void menuStrip1_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {

        }

        private void quảnLýMônHọcToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenChild<MonHoc>();
        }

        private void menuStrip2_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {

        }

        private void thêmTàiKhoảnToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenChild<TaiKhoan>();
        }

        private void tìmKiếmToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenChild<TimKiem>();
        }

        private void quảnLýTàiKhoảnToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // Có thể thêm logic xử lý khi click vào menu quản lý tài khoản
            // Hiện tại chỉ mở form quản lý tài khoản
            OpenChild<TaiKhoan>();
        }

        


        private void quảnLýTàiKhoảnToolStripMenuItem1_Click(object sender, EventArgs e)
        {

        }

        private void thêmTàiKhoảnToolStripMenuItem_Click_1(object sender, EventArgs e)
        {
            OpenChild<TaiKhoan>();
        }

        private void đăngXuấtToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DangNhap dn = new DangNhap();
            dn.Show();
            this.Close();
        }

        private void TrangChu_Load(object sender, EventArgs e)
        {
            this.WindowState = FormWindowState.Maximized;
        }

    }
}
