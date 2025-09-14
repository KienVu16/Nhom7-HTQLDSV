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
    public partial class BangDiem : Form
    {
        public BangDiem()
        {
            InitializeComponent();
            LoadMaSV();
            HienThi();
        }

        string Nguon = @"Data Source=DESKTOP-5LSPDHV\SQLEXPRESS01;Initial Catalog=QLSV;Integrated Security=True";

        private void LoadMaSV()
        {
            using (var conn = new SqlConnection(Nguon))
            {
                conn.Open();
                var cmd = new SqlCommand("SELECT MaSV FROM SinhVien", conn);
                var reader = cmd.ExecuteReader();
                while (reader.Read())
                    cbbMaSV.Items.Add(reader["MaSV"].ToString());
            }
        }

        private void HienThi()
        {
            dgvBangDiem.Rows.Clear();
            using (var conn = new SqlConnection(Nguon))
            {
                conn.Open();
                string sql = @"
                    SELECT BD.MaDiem, BD.MaSV, SV.Ho, SV.Ten,
                           BD.DiemGiuaKy, BD.DiemCuoiKy, BD.DiemKhac, BD.TongKet, BD.GhiChu
                    FROM BangDiem BD
                    INNER JOIN SinhVien SV ON BD.MaSV = SV.MaSV";

                using (var cmd = new SqlCommand(sql, conn))
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        int index = dgvBangDiem.Rows.Add();
                        dgvBangDiem.Rows[index].Cells[0].Value = reader["MaDiem"];
                        dgvBangDiem.Rows[index].Cells[1].Value = reader["MaSV"];
                        dgvBangDiem.Rows[index].Cells[2].Value = reader["Ho"];
                        dgvBangDiem.Rows[index].Cells[3].Value = reader["Ten"];
                        dgvBangDiem.Rows[index].Cells[4].Value = reader["DiemGiuaKy"];
                        dgvBangDiem.Rows[index].Cells[5].Value = reader["DiemCuoiKy"];
                        dgvBangDiem.Rows[index].Cells[6].Value = reader["DiemKhac"];
                        dgvBangDiem.Rows[index].Cells[7].Value = reader["TongKet"];
                        dgvBangDiem.Rows[index].Cells[8].Value = reader["GhiChu"];
                    }
                }
            }
        }

        private void btnThem_Click(object sender, EventArgs e)
        {
            if (cbbMaSV.SelectedItem == null)
            {
                MessageBox.Show("Vui lòng chọn Mã SV trước khi thêm.");
                return;
            }

            using (SqlConnection conn = new SqlConnection(Nguon))
            {
                conn.Open();
                string sql = @"INSERT INTO BangDiem 
                       (MaSV, DiemGiuaKy, DiemCuoiKy, DiemKhac, TongKet, GhiChu)
                       VALUES (@MaSV, @DiemGiuaKy, @DiemCuoiKy, @DiemKhac, @TongKet, @GhiChu)";
                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@MaSV", cbbMaSV.SelectedItem.ToString());

                    decimal diemGiuaKy, diemCuoiKy, diemKhac, tongKet;
                    cmd.Parameters.AddWithValue("@DiemGiuaKy", decimal.TryParse(txtDiemGiuaKy.Text, out diemGiuaKy) ? diemGiuaKy : (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@DiemCuoiKy", decimal.TryParse(txtDiemCuoiKy.Text, out diemCuoiKy) ? diemCuoiKy : (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@DiemKhac", decimal.TryParse(txtDiemKhac.Text, out diemKhac) ? diemKhac : (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@TongKet", decimal.TryParse(txtTongKet.Text, out tongKet) ? tongKet : (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@GhiChu", string.IsNullOrWhiteSpace(txtGhiChu.Text) ? (object)DBNull.Value : txtGhiChu.Text);

                    cmd.ExecuteNonQuery();
                }
            }

            HienThi();
        }

        private void btnSua_Click(object sender, EventArgs e)
        {
            if (dgvBangDiem.CurrentRow == null)
            {
                MessageBox.Show("Vui lòng chọn dòng cần sửa.");
                return;
            }

            string maDiem = dgvBangDiem.CurrentRow.Cells[0].Value.ToString();

            using (SqlConnection conn = new SqlConnection(Nguon))
            {
                conn.Open();
                string sql = @"UPDATE BangDiem
                       SET MaSV=@MaSV, DiemGiuaKy=@DiemGiuaKy, 
                           DiemCuoiKy=@DiemCuoiKy, DiemKhac=@DiemKhac, 
                           TongKet=@TongKet, GhiChu=@GhiChu
                       WHERE MaDiem=@MaDiem";
                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@MaSV", cbbMaSV.SelectedItem == null ? "" : cbbMaSV.SelectedItem.ToString());

                    decimal diemGiuaKy, diemCuoiKy, diemKhac, tongKet;
                    cmd.Parameters.AddWithValue("@DiemGiuaKy", decimal.TryParse(txtDiemGiuaKy.Text, out diemGiuaKy) ? diemGiuaKy : (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@DiemCuoiKy", decimal.TryParse(txtDiemCuoiKy.Text, out diemCuoiKy) ? diemCuoiKy : (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@DiemKhac", decimal.TryParse(txtDiemKhac.Text, out diemKhac) ? diemKhac : (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@TongKet", decimal.TryParse(txtTongKet.Text, out tongKet) ? tongKet : (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@GhiChu", string.IsNullOrWhiteSpace(txtGhiChu.Text) ? (object)DBNull.Value : txtGhiChu.Text);
                    cmd.Parameters.AddWithValue("@MaDiem", maDiem);

                    cmd.ExecuteNonQuery();
                }
            }

            HienThi();
        }

        private void btnXoa_Click(object sender, EventArgs e)
        {
            if (dgvBangDiem.CurrentRow == null) return;
            string maDiem = dgvBangDiem.CurrentRow.Cells[0].Value.ToString();

            using (var conn = new SqlConnection(Nguon))
            {
                conn.Open();
                var cmd = new SqlCommand("DELETE FROM BangDiem WHERE MaDiem=@MaDiem", conn);
                cmd.Parameters.AddWithValue("@MaDiem", maDiem);
                cmd.ExecuteNonQuery();
            }
            HienThi();
        }
    }
}
