USE DBWebBanSach
GO
CREATE TABLE [dbo].[Category] (
    MaTheLoai INT IDENTITY(1,1) PRIMARY KEY,
    TenTheLoai NVARCHAR(100) NOT NULL
);
GO

CREATE TABLE [dbo].[Author] (
    MaTacGia INT IDENTITY(1,1) PRIMARY KEY,
    TenTacGia NVARCHAR(100) NOT NULL
);
GO

CREATE TABLE [dbo].[Book](
    MaSach INT IDENTITY(1,1) PRIMARY KEY,
    TenSach NVARCHAR(250) NOT NULL,
    GiaBan DECIMAL(12, 0) NOT NULL DEFAULT 0,
    MoTa NVARCHAR(MAX) NULL,
    HinhAnh VARCHAR(255) NULL,
    SoLuongTon INT NOT NULL DEFAULT 0,
    MaTheLoai INT NOT NULL,
    MaTacGia INT NOT NULL,
    CONSTRAINT FK_Book_Category FOREIGN KEY (MaTheLoai) REFERENCES [dbo].[Category](MaTheLoai),
    CONSTRAINT FK_Book_Author FOREIGN KEY (MaTacGia) REFERENCES [dbo].[Author](MaTacGia)
);

GO
CREATE TABLE [dbo].[Customer](
    MaKH INT IDENTITY(1,1) PRIMARY KEY,
    HoTen NVARCHAR(100) NOT NULL,
    TenTaiKhoan VARCHAR(50) NOT NULL UNIQUE,
    MatKhau VARCHAR(100) NOT NULL,
    Email VARCHAR(100) NULL,
    DiaChi NVARCHAR(255) NULL,
    DienThoai VARCHAR(20) NULL
);
GO
CREATE TABLE [dbo].[Order](
    MaDonHang INT IDENTITY(1,1) PRIMARY KEY,
    MaKH INT NOT NULL,
    NgayDat DATETIME DEFAULT GETDATE(),
    NgayGiao DATETIME NULL,
    DaThanhToan BIT DEFAULT 0,
    TinhTrangGiaoHang INT DEFAULT 1,
    DiaChiGiao NVARCHAR(255) NULL,
    TongTien DECIMAL(12, 0) DEFAULT 0,
    CONSTRAINT FK_Order_Customer FOREIGN KEY (MaKH) REFERENCES [dbo].[Customer](MaKH)
);
GO


CREATE TABLE [dbo].[OrderDetail](
    MaDonHang INT NOT NULL,
    MaSach INT NOT NULL,
    SoLuong INT NOT NULL CHECK (SoLuong > 0),
    DonGia DECIMAL(12, 0) NOT NULL,
    PRIMARY KEY (MaDonHang, MaSach),
    CONSTRAINT FK_OrderDetail_Order FOREIGN KEY (MaDonHang) REFERENCES [dbo].[Order](MaDonHang) ON DELETE CASCADE,
    CONSTRAINT FK_OrderDetail_Book FOREIGN KEY (MaSach) REFERENCES [dbo].[Book](MaSach)
);
GO
CREATE TABLE [dbo].[Cart] (
    MaGioHang INT IDENTITY(1,1) PRIMARY KEY,
    MaKH INT NULL,
    NgayTao DATETIME DEFAULT GETDATE(),
    CONSTRAINT FK_Cart_Customer FOREIGN KEY (MaKH) REFERENCES [dbo].[Customer](MaKH) ON DELETE SET NULL
);
GO

CREATE TABLE [dbo].[CartItem] (
    MaCartItem INT IDENTITY(1,1) PRIMARY KEY,
    MaGioHang INT NOT NULL,
    MaSach INT NOT NULL,
    SoLuong INT NOT NULL DEFAULT 1 CHECK (SoLuong > 0),
    CONSTRAINT FK_CartItem_Cart FOREIGN KEY (MaGioHang) REFERENCES [dbo].[Cart](MaGioHang) ON DELETE CASCADE,
    CONSTRAINT FK_CartItem_Book FOREIGN KEY (MaSach) REFERENCES [dbo].[Book](MaSach),
    CONSTRAINT UQ_CartItem_Cart_Book UNIQUE (MaGioHang, MaSach),
);
GO

USE WebBanSach; -- Hoặc tên database của bạn
GO

-- 1. Thêm thể loại mẫu (nếu chưa có)
INSERT INTO Category (TenTheLoai) VALUES (N'Công Nghệ Thông Tin'), (N'Kinh Tế - Kỹ Năng');

-- 2. Thêm tác giả mẫu (nếu chưa có)
INSERT INTO Author (TenTacGia) VALUES (N'Nguyễn Nhật Ánh'), (N'Robert C. Martin');

-- 3. Thêm sách mẫu
INSERT INTO Book (TenSach, GiaBan, MoTa, SoLuongTon, MaTheLoai, MaTacGia)
VALUES 
(N'Lập Trình C# Cơ Bản Đến Nâng Cao', 125000, N'Sách hướng dẫn lập trình C# cho người mới bắt đầu', 50, 1, 2),
(N'Clean Code - Nghệ Thuật Viết Code Sạch', 210000, N'Cẩm nang tối ưu hóa mã nguồn chuyên nghiệp', 30, 1, 2),
(N'Tôi Thấy Hoa Vàng Trên Cỏ Xanh', 85000, N'Tiểu thuyết tuổi học trò đầy cảm xúc', 100, 2, 1);
GO