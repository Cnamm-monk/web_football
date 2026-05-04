-- ============================================================
--  PitchHub.vn — Tạo bảng + Seed Data hoàn chỉnh
--  Chạy 1 lần duy nhất trên SQL Server / SSMS
-- ============================================================

USE master;
GO

IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = 'SanBongBTL')
    CREATE DATABASE SanBongBTL;
GO

USE SanBongBTL;
GO

-- ============================================================
-- XÓA BẢNG CŨ (thứ tự FK: con trước, cha sau)
-- ============================================================
IF OBJECT_ID('DatSan_DichVus', 'U') IS NOT NULL DROP TABLE DatSan_DichVus;
IF OBJECT_ID('Matchmakings',   'U') IS NOT NULL DROP TABLE Matchmakings;
IF OBJECT_ID('DatSans',        'U') IS NOT NULL DROP TABLE DatSans;
IF OBJECT_ID('DanhGias',       'U') IS NOT NULL DROP TABLE DanhGias;
IF OBJECT_ID('DichVus',        'U') IS NOT NULL DROP TABLE DichVus;
IF OBJECT_ID('KhungGios',      'U') IS NOT NULL DROP TABLE KhungGios;
IF OBJECT_ID('SanBongs',       'U') IS NOT NULL DROP TABLE SanBongs;
IF OBJECT_ID('Users',          'U') IS NOT NULL DROP TABLE Users;
GO

-- ============================================================
-- TẠO BẢNG
-- ============================================================
CREATE TABLE Users (
    Id          INT           IDENTITY(1,1) PRIMARY KEY,
    HoTen       NVARCHAR(100) NOT NULL,
    Email       NVARCHAR(150) NOT NULL UNIQUE,
    MatKhau     NVARCHAR(255) NOT NULL,
    SoDienThoai NVARCHAR(20)  NULL,
    VaiTro      NVARCHAR(20)  NOT NULL DEFAULT 'User',
    NgayTao     DATETIME      NOT NULL DEFAULT GETDATE(),
    CONSTRAINT CHK_Users_VaiTro CHECK (VaiTro IN ('User','Owner','Staff','Admin'))
);
GO

CREATE TABLE SanBongs (
    Id               INT           IDENTITY(1,1) PRIMARY KEY,
    TenSan           NVARCHAR(200) NOT NULL,
    DiaChi           NVARCHAR(300) NOT NULL,
    Quan             NVARCHAR(100) NOT NULL,
    ThanhPho         NVARCHAR(100) NOT NULL,
    LoaiSan          NVARCHAR(5)   NOT NULL,
    LoaiCo           NVARCHAR(50)  NOT NULL,
    HinhAnh          NVARCHAR(500) NULL,
    MoTa             NVARCHAR(MAX) NOT NULL DEFAULT '',
    DanhGiaTrungBinh FLOAT         NOT NULL DEFAULT 0,
    Latitude         FLOAT         NOT NULL DEFAULT 0,
    Longitude        FLOAT         NOT NULL DEFAULT 0,
    TrangThaiDuyet   NVARCHAR(20)  NOT NULL DEFAULT 'ChoDuyet',
    OwnerId          INT           NOT NULL,
    CONSTRAINT FK_SanBongs_Owner    FOREIGN KEY (OwnerId) REFERENCES Users(Id),
    CONSTRAINT CHK_SanBongs_LoaiSan CHECK (LoaiSan IN ('5','7','11')),
    CONSTRAINT CHK_SanBongs_Duyet   CHECK (TrangThaiDuyet IN ('ChoDuyet','DaDuyet','TuChoi'))
);
GO

CREATE TABLE KhungGios (
    Id                INT           IDENTITY(1,1) PRIMARY KEY,
    SanBongId         INT           NOT NULL,
    GioBatDau         TIME          NOT NULL,
    GioKetThuc        TIME          NOT NULL,
    Gia               DECIMAL(18,2) NOT NULL,
    GiaGioVang        DECIMAL(18,2) NOT NULL DEFAULT 0,
    TrangThai         NVARCHAR(20)  NOT NULL DEFAULT 'Trong',
    ThoiGianHetGiuCho DATETIME      NULL,
    CONSTRAINT FK_KhungGios_SanBong   FOREIGN KEY (SanBongId) REFERENCES SanBongs(Id),
    CONSTRAINT CHK_KhungGios_TrangThai CHECK (TrangThai IN ('Trong','DaDat','DangGiu'))
);
GO

CREATE TABLE DichVus (
    Id        INT           IDENTITY(1,1) PRIMARY KEY,
    TenDichVu NVARCHAR(100) NOT NULL,
    Gia       DECIMAL(18,2) NOT NULL,
    MoTa      NVARCHAR(500) NULL
);
GO

CREATE TABLE DatSans (
    Id          INT           IDENTITY(1,1) PRIMARY KEY,
    UserId      INT           NOT NULL,
    KhungGioId  INT           NOT NULL,
    NgayThiDau  DATETIME      NOT NULL,
    TienCoc     DECIMAL(18,2) NOT NULL,
    MaXacNhan   NVARCHAR(50)  NOT NULL,
    TrangThai   NVARCHAR(20)  NOT NULL DEFAULT 'ChoDuyet',
    ThoiGianTao DATETIME      NOT NULL DEFAULT GETDATE(),
    CONSTRAINT FK_DatSans_User      FOREIGN KEY (UserId)     REFERENCES Users(Id),
    CONSTRAINT FK_DatSans_KhungGio  FOREIGN KEY (KhungGioId) REFERENCES KhungGios(Id),
    CONSTRAINT CHK_DatSans_TrangThai CHECK (TrangThai IN ('ChoDuyet','DaXacNhan','DaHuy','HoanThanh'))
);
GO

CREATE TABLE DatSan_DichVus (
    Id       INT IDENTITY(1,1) PRIMARY KEY,
    DatSanId INT NOT NULL,
    DichVuId INT NOT NULL,
    SoLuong  INT NOT NULL DEFAULT 1,
    CONSTRAINT FK_DatSan_DichVus_DatSan FOREIGN KEY (DatSanId) REFERENCES DatSans(Id),
    CONSTRAINT FK_DatSan_DichVus_DichVu FOREIGN KEY (DichVuId) REFERENCES DichVus(Id)
);
GO

CREATE TABLE DanhGias (
    Id          INT            IDENTITY(1,1) PRIMARY KEY,
    SanBongId   INT            NOT NULL,
    UserId      INT            NOT NULL,
    SoSao       INT            NOT NULL DEFAULT 5,
    NhanXet     NVARCHAR(1000) NULL,
    NgayDanhGia DATETIME       NOT NULL DEFAULT GETDATE(),
    CONSTRAINT FK_DanhGias_SanBong FOREIGN KEY (SanBongId) REFERENCES SanBongs(Id),
    CONSTRAINT FK_DanhGias_User    FOREIGN KEY (UserId)    REFERENCES Users(Id),
    CONSTRAINT CHK_DanhGias_SoSao  CHECK (SoSao BETWEEN 1 AND 5)
);
GO

CREATE TABLE Matchmakings (
    Id             INT            IDENTITY(1,1) PRIMARY KEY,
    DatSanId       INT            NOT NULL UNIQUE,
    UserId         INT            NOT NULL,
    TieuDe         NVARCHAR(200)  NOT NULL,
    MoTa           NVARCHAR(1000) NULL,
    SoNguoiCanThem INT            NOT NULL DEFAULT 1,
    TrangThai      NVARCHAR(20)   NOT NULL DEFAULT 'DangTim',
    NgayDang       DATETIME       NOT NULL DEFAULT GETDATE(),
    CONSTRAINT FK_Matchmakings_DatSan   FOREIGN KEY (DatSanId) REFERENCES DatSans(Id),
    CONSTRAINT FK_Matchmakings_User     FOREIGN KEY (UserId)   REFERENCES Users(Id),
    CONSTRAINT CHK_Matchmakings_TrangThai CHECK (TrangThai IN ('DangTim','DaDu','DaDong'))
);
GO

CREATE INDEX IX_SanBongs_Quan          ON SanBongs    (Quan);
CREATE INDEX IX_SanBongs_TrangThai     ON SanBongs    (TrangThaiDuyet);
CREATE INDEX IX_KhungGios_SanBongId    ON KhungGios   (SanBongId);
CREATE INDEX IX_KhungGios_TrangThai    ON KhungGios   (TrangThai);
CREATE INDEX IX_DatSans_UserId         ON DatSans     (UserId);
CREATE INDEX IX_DatSans_TrangThai      ON DatSans     (TrangThai);
CREATE INDEX IX_Matchmakings_TrangThai ON Matchmakings(TrangThai);
GO

-- ============================================================
-- SEED DATA — dùng biến, không hardcode Id
-- ============================================================
DECLARE
    -- Users
    @idAdmin  INT, @idOwner INT, @idUser INT, @idStaff INT,
    -- SanBongs
    @idSan1   INT, @idSan2  INT, @idSan3 INT,
    @idSan4   INT, @idSan5  INT,
    -- KhungGios sẽ đặt
    @kgDat1   INT, @kgDat2  INT,
    -- DichVus
    @dvNuoc   INT, @dvBong  INT, @dvTai  INT, @dvAo INT,
    -- DatSans
    @ds1      INT, @ds2     INT;

-- ── 1. USERS ──────────────────────────────────────────────
INSERT INTO Users (HoTen, Email, MatKhau, SoDienThoai, VaiTro)
VALUES (N'Admin PitchHub', 'admin@pitchhub.vn', 'admin123', '0901000001', 'Admin');
SET @idAdmin = SCOPE_IDENTITY();

INSERT INTO Users (HoTen, Email, MatKhau, SoDienThoai, VaiTro)
VALUES (N'Vũ Nguyễn Tuấn Kiệt', 'owner1@gmail.com', 'owner123', '0901000002', 'Owner');
SET @idOwner = SCOPE_IDENTITY();

INSERT INTO Users (HoTen, Email, MatKhau, SoDienThoai, VaiTro)
VALUES (N'Nguyễn Công Nam', 'user1@gmail.com', 'user123', '0901000003', 'User');
SET @idUser = SCOPE_IDENTITY();

INSERT INTO Users (HoTen, Email, MatKhau, SoDienThoai, VaiTro)
VALUES (N'Đào Việt Toàn', 'staff@pitchhub.vn', 'staff123', '0901000004', 'Staff');
SET @idStaff = SCOPE_IDENTITY();

PRINT CONCAT('✔ Users: Admin=', @idAdmin,
             ' Owner=', @idOwner,
             ' User=',  @idUser,
             ' Staff=', @idStaff);

-- ── 2. SAN BONG ───────────────────────────────────────────
INSERT INTO SanBongs (TenSan, DiaChi, Quan, ThanhPho, LoaiSan, LoaiCo, MoTa, DanhGiaTrungBinh, Latitude, Longitude, TrangThaiDuyet, OwnerId)
VALUES (N'Sân Cầu Giấy Sport', N'12 Xuân Thủy', N'Cầu Giấy', N'Hà Nội',
        '5', 'Nhan tao', N'Sân cỏ nhân tạo chất lượng cao, đèn chiếu sáng ban đêm.',
        4.5, 21.0362, 105.7826, 'DaDuyet', @idOwner);
SET @idSan1 = SCOPE_IDENTITY();

INSERT INTO SanBongs (TenSan, DiaChi, Quan, ThanhPho, LoaiSan, LoaiCo, MoTa, DanhGiaTrungBinh, Latitude, Longitude, TrangThaiDuyet, OwnerId)
VALUES (N'Sân Đống Đa Arena', N'45 Tây Sơn', N'Đống Đa', N'Hà Nội',
        '7', 'Nhan tao', N'Sân 7 người rộng rãi, có mái che, chỗ để xe rộng.',
        4.2, 21.0198, 105.8412, 'DaDuyet', @idOwner);
SET @idSan2 = SCOPE_IDENTITY();

INSERT INTO SanBongs (TenSan, DiaChi, Quan, ThanhPho, LoaiSan, LoaiCo, MoTa, DanhGiaTrungBinh, Latitude, Longitude, TrangThaiDuyet, OwnerId)
VALUES (N'Sân Hoàng Mai FC', N'78 Giải Phóng', N'Hoàng Mai', N'Hà Nội',
        '5', 'Tu nhien', N'Sân cỏ tự nhiên thoáng mát, phù hợp thi đấu buổi sáng.',
        3.8, 20.9876, 105.8543, 'DaDuyet', @idOwner);
SET @idSan3 = SCOPE_IDENTITY();

INSERT INTO SanBongs (TenSan, DiaChi, Quan, ThanhPho, LoaiSan, LoaiCo, MoTa, DanhGiaTrungBinh, Latitude, Longitude, TrangThaiDuyet, OwnerId)
VALUES (N'Sân Long Biên Star', N'23 Nguyễn Văn Cừ', N'Long Biên', N'Hà Nội',
        '11', 'Nhan tao', N'Sân 11 người tiêu chuẩn FIFA, có phòng thay đồ.',
        4.7, 21.0465, 105.8923, 'DaDuyet', @idOwner);
SET @idSan4 = SCOPE_IDENTITY();

INSERT INTO SanBongs (TenSan, DiaChi, Quan, ThanhPho, LoaiSan, LoaiCo, MoTa, DanhGiaTrungBinh, Latitude, Longitude, TrangThaiDuyet, OwnerId)
VALUES (N'Sân Chờ Duyệt Test', N'99 Test Street', N'Nam Từ Liêm', N'Hà Nội',
        '5', 'Nhan tao', N'Sân đang chờ admin phê duyệt.',
        0, 21.0100, 105.7500, 'ChoDuyet', @idOwner);
SET @idSan5 = SCOPE_IDENTITY();

PRINT CONCAT('✔ SanBongs: ', @idSan1, ' ', @idSan2, ' ', @idSan3, ' ', @idSan4, ' ', @idSan5);

-- ── 3. KHUNG GIO ──────────────────────────────────────────
-- Sân 1
INSERT INTO KhungGios (SanBongId, GioBatDau, GioKetThuc, Gia, GiaGioVang, TrangThai) VALUES (@idSan1,'06:00','07:30',180000,220000,'Trong');
INSERT INTO KhungGios (SanBongId, GioBatDau, GioKetThuc, Gia, GiaGioVang, TrangThai) VALUES (@idSan1,'07:30','09:00',180000,220000,'Trong');
INSERT INTO KhungGios (SanBongId, GioBatDau, GioKetThuc, Gia, GiaGioVang, TrangThai) VALUES (@idSan1,'09:00','10:30',160000,200000,'Trong');
INSERT INTO KhungGios (SanBongId, GioBatDau, GioKetThuc, Gia, GiaGioVang, TrangThai) VALUES (@idSan1,'15:00','16:30',160000,200000,'Trong');
INSERT INTO KhungGios (SanBongId, GioBatDau, GioKetThuc, Gia, GiaGioVang, TrangThai) VALUES (@idSan1,'16:30','18:00',200000,260000,'Trong');
INSERT INTO KhungGios (SanBongId, GioBatDau, GioKetThuc, Gia, GiaGioVang, TrangThai) VALUES (@idSan1,'18:00','19:30',250000,300000,'DaDat');
SET @kgDat1 = SCOPE_IDENTITY();  -- khung đã đặt → dùng cho DatSan 1
INSERT INTO KhungGios (SanBongId, GioBatDau, GioKetThuc, Gia, GiaGioVang, TrangThai) VALUES (@idSan1,'19:30','21:00',250000,300000,'Trong');

-- Sân 2
INSERT INTO KhungGios (SanBongId, GioBatDau, GioKetThuc, Gia, GiaGioVang, TrangThai) VALUES (@idSan2,'06:00','07:30',250000,300000,'Trong');
INSERT INTO KhungGios (SanBongId, GioBatDau, GioKetThuc, Gia, GiaGioVang, TrangThai) VALUES (@idSan2,'07:30','09:00',250000,300000,'Trong');
INSERT INTO KhungGios (SanBongId, GioBatDau, GioKetThuc, Gia, GiaGioVang, TrangThai) VALUES (@idSan2,'17:00','18:30',320000,380000,'DaDat');
SET @kgDat2 = SCOPE_IDENTITY();  -- khung đã đặt → dùng cho DatSan 2
INSERT INTO KhungGios (SanBongId, GioBatDau, GioKetThuc, Gia, GiaGioVang, TrangThai) VALUES (@idSan2,'18:30','20:00',350000,420000,'Trong');
INSERT INTO KhungGios (SanBongId, GioBatDau, GioKetThuc, Gia, GiaGioVang, TrangThai) VALUES (@idSan2,'20:00','21:30',350000,420000,'Trong');

-- Sân 3
INSERT INTO KhungGios (SanBongId, GioBatDau, GioKetThuc, Gia, GiaGioVang, TrangThai) VALUES (@idSan3,'05:30','07:00',150000,180000,'Trong');
INSERT INTO KhungGios (SanBongId, GioBatDau, GioKetThuc, Gia, GiaGioVang, TrangThai) VALUES (@idSan3,'07:00','08:30',150000,180000,'Trong');
INSERT INTO KhungGios (SanBongId, GioBatDau, GioKetThuc, Gia, GiaGioVang, TrangThai) VALUES (@idSan3,'16:00','17:30',180000,220000,'Trong');
INSERT INTO KhungGios (SanBongId, GioBatDau, GioKetThuc, Gia, GiaGioVang, TrangThai) VALUES (@idSan3,'17:30','19:00',200000,250000,'Trong');

-- Sân 4
INSERT INTO KhungGios (SanBongId, GioBatDau, GioKetThuc, Gia, GiaGioVang, TrangThai) VALUES (@idSan4,'06:00','08:00',500000,600000,'Trong');
INSERT INTO KhungGios (SanBongId, GioBatDau, GioKetThuc, Gia, GiaGioVang, TrangThai) VALUES (@idSan4,'08:00','10:00',500000,600000,'Trong');
INSERT INTO KhungGios (SanBongId, GioBatDau, GioKetThuc, Gia, GiaGioVang, TrangThai) VALUES (@idSan4,'15:00','17:00',550000,650000,'Trong');
INSERT INTO KhungGios (SanBongId, GioBatDau, GioKetThuc, Gia, GiaGioVang, TrangThai) VALUES (@idSan4,'17:00','19:00',650000,780000,'Trong');
INSERT INTO KhungGios (SanBongId, GioBatDau, GioKetThuc, Gia, GiaGioVang, TrangThai) VALUES (@idSan4,'19:00','21:00',650000,780000,'Trong');

PRINT CONCAT('✔ KhungGios xong. KgDat1=', @kgDat1, ' KgDat2=', @kgDat2);

-- ── 4. DICH VU ────────────────────────────────────────────
INSERT INTO DichVus (TenDichVu, Gia, MoTa) VALUES ('Nuoc uong',     15000,  N'Nước lọc / nước ngọt / tăng lực');
SET @dvNuoc = SCOPE_IDENTITY();
INSERT INTO DichVus (TenDichVu, Gia, MoTa) VALUES ('Thue bong',     30000,  N'Bóng thi đấu tiêu chuẩn Size 4/5');
SET @dvBong = SCOPE_IDENTITY();
INSERT INTO DichVus (TenDichVu, Gia, MoTa) VALUES ('Thue trong tai',100000, N'Trọng tài có kinh nghiệm');
SET @dvTai  = SCOPE_IDENTITY();
INSERT INTO DichVus (TenDichVu, Gia, MoTa) VALUES ('Thue ao',       20000,  N'Áo thi đấu có số, 2 màu');
SET @dvAo   = SCOPE_IDENTITY();

-- ── 5. DAT SAN ────────────────────────────────────────────
INSERT INTO DatSans (UserId, KhungGioId, NgayThiDau, TienCoc, MaXacNhan, TrangThai)
VALUES (@idUser, @kgDat1, DATEADD(DAY,2,GETDATE()), 75000, 'ABC12345', 'DaXacNhan');
SET @ds1 = SCOPE_IDENTITY();

INSERT INTO DatSans (UserId, KhungGioId, NgayThiDau, TienCoc, MaXacNhan, TrangThai)
VALUES (@idUser, @kgDat2, DATEADD(DAY,3,GETDATE()), 105000, 'DEF67890', 'ChoDuyet');
SET @ds2 = SCOPE_IDENTITY();

PRINT CONCAT('✔ DatSans: ds1=', @ds1, ' ds2=', @ds2);

-- ── 6. DICH VU KEM THEO ───────────────────────────────────
INSERT INTO DatSan_DichVus (DatSanId, DichVuId, SoLuong) VALUES (@ds1, @dvNuoc, 10);
INSERT INTO DatSan_DichVus (DatSanId, DichVuId, SoLuong) VALUES (@ds1, @dvBong,  1);
INSERT INTO DatSan_DichVus (DatSanId, DichVuId, SoLuong) VALUES (@ds1, @dvTai,   1);

-- ── 7. DANH GIA ───────────────────────────────────────────
INSERT INTO DanhGias (SanBongId, UserId, SoSao, NhanXet)
VALUES (@idSan1, @idUser,  5, N'Sân rất đẹp, cỏ mới, nhân viên nhiệt tình!');
INSERT INTO DanhGias (SanBongId, UserId, SoSao, NhanXet)
VALUES (@idSan2, @idUser,  4, N'Sân rộng, phù hợp đá 7. Bãi xe tiện lợi.');
INSERT INTO DanhGias (SanBongId, UserId, SoSao, NhanXet)
VALUES (@idSan3, @idAdmin, 3, N'Cỏ tự nhiên nhưng cần chăm sóc thêm.');

UPDATE SanBongs SET DanhGiaTrungBinh = 5.0 WHERE Id = @idSan1;
UPDATE SanBongs SET DanhGiaTrungBinh = 4.0 WHERE Id = @idSan2;
UPDATE SanBongs SET DanhGiaTrungBinh = 3.0 WHERE Id = @idSan3;

-- ── 8. MATCHMAKING ────────────────────────────────────────
INSERT INTO Matchmakings (DatSanId, UserId, TieuDe, MoTa, SoNguoiCanThem, TrangThai)
VALUES (@ds1, @idUser,
        N'Cần thêm 2 tiền đạo — Sân Cầu Giấy 18h tối mai',
        N'Team trình độ trung bình, chơi vui là chính. LH ngay!',
        2, 'DangTim');

-- ── KIỂM TRA ──────────────────────────────────────────────
SELECT [Bang] = v.n, [So ban ghi] = v.c FROM (VALUES
    ('Users',          (SELECT COUNT(*) FROM Users)),
    ('SanBongs',       (SELECT COUNT(*) FROM SanBongs)),
    ('KhungGios',      (SELECT COUNT(*) FROM KhungGios)),
    ('DichVus',        (SELECT COUNT(*) FROM DichVus)),
    ('DatSans',        (SELECT COUNT(*) FROM DatSans)),
    ('DatSan_DichVus', (SELECT COUNT(*) FROM DatSan_DichVus)),
    ('DanhGias',       (SELECT COUNT(*) FROM DanhGias)),
    ('Matchmakings',   (SELECT COUNT(*) FROM Matchmakings))
) v(n,c);

PRINT '======================================';
PRINT '✅ Database SanBongBTL tao thanh cong!';
PRINT 'Dang nhap test:';
PRINT '  Admin : admin@pitchhub.vn  / admin123';
PRINT '  Owner : owner1@gmail.com   / owner123';
PRINT '  User  : user1@gmail.com    / user123';
PRINT '  Staff : staff@pitchhub.vn  / staff123';
PRINT '======================================';
GO