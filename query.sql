-- Bảng tài khoản
CREATE TABLE TaiKhoan (
    MaTK INT IDENTITY(1,1) PRIMARY KEY,
    TenDangNhap VARCHAR(50) UNIQUE NOT NULL,
    MatKhau VARCHAR(255) NOT NULL,
    VaiTro VARCHAR(20) NOT NULL,
    NgayTao DATETIME2 DEFAULT SYSDATETIME(),
    CONSTRAINT ck_VaiTro CHECK (VaiTro IN ('quan_tri','sinh_vien','giang_vien'))
);

-- Bảng sinh viên
CREATE TABLE SinhVien (
    MaSV INT IDENTITY(1,1) PRIMARY KEY,
    MaTK INT,
    Ho NVARCHAR(50) NOT NULL,    -- Cột lưu Họ
    Ten NVARCHAR(50) NOT NULL,   -- Cột lưu Tên
    NgaySinh DATE,
    GioiTinh NVARCHAR(10),
    Email VARCHAR(100) UNIQUE,
    SoDienThoai VARCHAR(15),
    DiaChi NVARCHAR(255),
    GhiChu NVARCHAR(255),
    CONSTRAINT ck_GioiTinh CHECK (GioiTinh IN ('Nam', 'Nu', 'Khac')),
    CONSTRAINT fk_SinhVien_TaiKhoan FOREIGN KEY (MaTK) REFERENCES TaiKhoan(MaTK) ON DELETE CASCADE
);
-- Bảng giảng viên
CREATE TABLE GiangVien (
    MaGV INT IDENTITY(1,1) PRIMARY KEY,
    HoTen NVARCHAR(100) NOT NULL,
    Email VARCHAR(100) UNIQUE,
    SoDienThoai VARCHAR(15),
    Khoa NVARCHAR(100),
    GhiChu NVARCHAR(255)
);
ALTER TABLE GiangVien
DROP COLUMN HoTen;
ALTER TABLE GiangVien
ADD Ho NVARCHAR(50) NOT NULL DEFAULT '',
    Ten NVARCHAR(50) NOT NULL DEFAULT '';
-- Bảng môn học
CREATE TABLE MonHoc (
    MaMH INT IDENTITY(1,1) PRIMARY KEY,
    TenMonHoc NVARCHAR(100) NOT NULL,
    MoTa NVARCHAR(MAX),
    SoTinChi INT NOT NULL,
    MaGV INT,
    GhiChu NVARCHAR(255),
    CONSTRAINT fk_MonHoc_GiangVien FOREIGN KEY (MaGV) REFERENCES GiangVien(MaGV)
);

-- Bảng đăng ký học
CREATE TABLE DangKyHoc (
    MaDK INT IDENTITY(1,1) PRIMARY KEY,
    MaSV INT,
    MaMH INT,
    NgayDangKy DATE DEFAULT CAST(GETDATE() AS DATE),
    GhiChu NVARCHAR(255),
    CONSTRAINT fk_DangKyHoc_SinhVien FOREIGN KEY (MaSV) REFERENCES SinhVien(MaSV),
    CONSTRAINT fk_DangKyHoc_MonHoc FOREIGN KEY (MaMH) REFERENCES MonHoc(MaMH),
    CONSTRAINT uq_DangKyHoc UNIQUE (MaSV, MaMH)
);

-- Bảng bảng điểm
CREATE TABLE BangDiem (
    MaDiem INT IDENTITY(1,1) PRIMARY KEY,
    MaDK INT,
    DiemGiuaKy FLOAT,
    DiemCuoiKy FLOAT,
    DiemKhac FLOAT,
    TongKet FLOAT,
    GhiChu NVARCHAR(255),
    CONSTRAINT fk_BangDiem_DangKyHoc FOREIGN KEY (MaDK) REFERENCES DangKyHoc(MaDK)
);
