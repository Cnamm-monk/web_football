-- ============================================================
--  PitchHub.vn — Database v3 HOÀN CHỈNH
--  Bổ sung so với v2:
--    1. SanBongs        : thêm IsHidden
--    2. KhungGios       : thêm LoaiNgay (ThuThuong/CuoiTuan)
--    3. DatSans         : thêm StaffCheckInId, StaffCheckOutId,
--                         GhiChuSuCo, LoaiSuCo
--    4. StaffSanPhanCong: BẢNG MỚI — Staff được gán sân nào
--    5. DanhMucLoaiSan  : BẢNG MỚI — thay thế CHECK constraint
--    6. DanhMucLoaiCo   : BẢNG MỚI — thay thế CHECK constraint
--    7. Seed Data       : hướng dẫn hash BCrypt đúng cách
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
IF OBJECT_ID('StaffSanPhanCong','U') IS NOT NULL DROP TABLE StaffSanPhanCong;
IF OBJECT_ID('AuditLogs',       'U') IS NOT NULL DROP TABLE AuditLogs;
IF OBJECT_ID('KhieuNais',       'U') IS NOT NULL DROP TABLE KhieuNais;
IF OBJECT_ID('DatSan_DichVus',  'U') IS NOT NULL DROP TABLE DatSan_DichVus;
IF OBJECT_ID('Matchmakings',    'U') IS NOT NULL DROP TABLE Matchmakings;
IF OBJECT_ID('DatSans',         'U') IS NOT NULL DROP TABLE DatSans;
IF OBJECT_ID('DanhGias',        'U') IS NOT NULL DROP TABLE DanhGias;
IF OBJECT_ID('DichVus',         'U') IS NOT NULL DROP TABLE DichVus;
IF OBJECT_ID('KhungGios',       'U') IS NOT NULL DROP TABLE KhungGios;
IF OBJECT_ID('SanBongs',        'U') IS NOT NULL DROP TABLE SanBongs;
IF OBJECT_ID('Users',           'U') IS NOT NULL DROP TABLE Users;
IF OBJECT_ID('DanhMucDichVu',   'U') IS NOT NULL DROP TABLE DanhMucDichVu;
IF OBJECT_ID('DanhMucLoaiCo',   'U') IS NOT NULL DROP TABLE DanhMucLoaiCo;
IF OBJECT_ID('DanhMucLoaiSan',  'U') IS NOT NULL DROP TABLE DanhMucLoaiSan;
IF OBJECT_ID('DanhMucQuan',     'U') IS NOT NULL DROP TABLE DanhMucQuan;
GO

-- ============================================================
-- TẦNG 1: MASTER DATA (Admin quản lý, không phụ thuộc bảng nào)
-- ============================================================

-- Danh mục quận/huyện
CREATE TABLE DanhMucQuan (
    Id       INT           IDENTITY(1,1) PRIMARY KEY,
    TenQuan  NVARCHAR(100) NOT NULL UNIQUE,
    ThanhPho NVARCHAR(100) NOT NULL DEFAULT N'Hà Nội',
    ThuTu    INT           NOT NULL DEFAULT 0,
    IsActive BIT           NOT NULL DEFAULT 1
);
GO

-- Danh mục loại sân — Admin thêm được loại mới mà không cần sửa DB
CREATE TABLE DanhMucLoaiSan (
    Id       INT          IDENTITY(1,1) PRIMARY KEY,
    MaLoai   NVARCHAR(5)  NOT NULL UNIQUE,   -- '5' | '7' | '11'
    TenLoai  NVARCHAR(50) NOT NULL,          -- 'Sân 5 người' | ...
    IsActive BIT          NOT NULL DEFAULT 1
);
GO

-- Danh mục loại cỏ — Admin thêm được loại mới
CREATE TABLE DanhMucLoaiCo (
    Id      INT           IDENTITY(1,1) PRIMARY KEY,
    MaLoai  NVARCHAR(20)  NOT NULL UNIQUE,   -- 'Nhan tao' | 'Tu nhien'
    TenLoai NVARCHAR(100) NOT NULL,
    IsActive BIT          NOT NULL DEFAULT 1
);
GO

-- Danh mục dịch vụ gốc — Admin tạo, Owner bật/tắt cho sân của mình
CREATE TABLE DanhMucDichVu (
    Id        INT           IDENTITY(1,1) PRIMARY KEY,
    TenDichVu NVARCHAR(100) NOT NULL,
    Icon      NVARCHAR(10)  NULL,
    MoTa      NVARCHAR(500) NULL,
    IsActive  BIT           NOT NULL DEFAULT 1
);
GO

-- ============================================================
-- TẦNG 2: USERS
-- ============================================================
CREATE TABLE Users (
    Id                INT           IDENTITY(1,1) PRIMARY KEY,
    HoTen             NVARCHAR(100) NOT NULL,
    Email             NVARCHAR(150) NOT NULL UNIQUE,
    MatKhau           NVARCHAR(255) NOT NULL,        -- BCrypt hash
    SoDienThoai       NVARCHAR(20)  NULL,
    VaiTro            NVARCHAR(20)  NOT NULL DEFAULT 'User',
    IsActive          BIT           NOT NULL DEFAULT 1,
    -- NULL với Admin/User/Owner; có giá trị với Staff
    OwnerIdCuaStaff   INT           NULL,
    NgayTao           DATETIME      NOT NULL DEFAULT GETDATE(),
    CONSTRAINT CHK_Users_VaiTro CHECK (VaiTro IN ('User','Owner','Staff','Admin')),
    -- Staff tự tham chiếu về Owner trong cùng bảng Users
    CONSTRAINT FK_Users_OwnerCuaStaff FOREIGN KEY (OwnerIdCuaStaff) REFERENCES Users(Id)
);
GO

-- ============================================================
-- TẦNG 3: SAN BONG — thêm IsHidden
-- ============================================================
CREATE TABLE SanBongs (
    Id               INT           IDENTITY(1,1) PRIMARY KEY,
    TenSan           NVARCHAR(200) NOT NULL,
    DiaChi           NVARCHAR(300) NOT NULL,
    -- Lưu text TenQuan để filter nhanh, không cần JOIN DanhMucQuan mỗi lần
    Quan             NVARCHAR(100) NOT NULL,
    ThanhPho         NVARCHAR(100) NOT NULL,
    -- Lưu MaLoai để tương thích với code cũ
    LoaiSan          NVARCHAR(5)   NOT NULL,
    LoaiCo           NVARCHAR(50)  NOT NULL,
    HinhAnh          NVARCHAR(500) NULL,
    MoTa             NVARCHAR(MAX) NOT NULL DEFAULT '',
    DanhGiaTrungBinh FLOAT         NOT NULL DEFAULT 0,
    Latitude         FLOAT         NOT NULL DEFAULT 0,
    Longitude        FLOAT         NOT NULL DEFAULT 0,
    TrangThaiDuyet   NVARCHAR(20)  NOT NULL DEFAULT 'ChoDuyet',
    -- v2: Tỷ lệ cọc Owner tự cấu hình
    TyLeCoc          DECIMAL(3,2)  NOT NULL DEFAULT 0.30,
    -- v3: Owner tạm ẩn sân khi bảo trì (khác với TuChoi của Admin)
    IsHidden         BIT           NOT NULL DEFAULT 0,
    OwnerId          INT           NOT NULL,
    CONSTRAINT FK_SanBongs_Owner    FOREIGN KEY (OwnerId) REFERENCES Users(Id),
    CONSTRAINT CHK_SanBongs_Duyet   CHECK (TrangThaiDuyet IN ('ChoDuyet','DaDuyet','TuChoi')),
    CONSTRAINT CHK_SanBongs_TyLeCoc CHECK (TyLeCoc BETWEEN 0.10 AND 0.70)
);
GO

-- ============================================================
-- TẦNG 4: KHUNG GIO — thêm LoaiNgay để hỗ trợ Dynamic Pricing
-- ============================================================
CREATE TABLE KhungGios (
    Id                INT           IDENTITY(1,1) PRIMARY KEY,
    SanBongId         INT           NOT NULL,
    GioBatDau         TIME          NOT NULL,
    GioKetThuc        TIME          NOT NULL,
    Gia               DECIMAL(18,2) NOT NULL,       -- giá ngày thường
    GiaGioVang        DECIMAL(18,2) NOT NULL DEFAULT 0,  -- giá giờ cao điểm
    -- v3: phân biệt giá thứ thường / cuối tuần
    GiaCuoiTuan      DECIMAL(18,2) NOT NULL DEFAULT 0,
    -- v3: loại ngày áp dụng khung giờ này
    LoaiNgay          NVARCHAR(20)  NOT NULL DEFAULT 'TatCa',
    TrangThai         NVARCHAR(20)  NOT NULL DEFAULT 'Trong',
    ThoiGianHetGiuCho DATETIME      NULL,
    CONSTRAINT FK_KhungGios_SanBong    FOREIGN KEY (SanBongId) REFERENCES SanBongs(Id),
    CONSTRAINT CHK_KhungGios_TrangThai CHECK (TrangThai IN ('Trong','DaDat','DangGiu')),
    -- TatCa: áp dụng mọi ngày | ThuThuong: T2-T6 | CuoiTuan: T7-CN
    CONSTRAINT CHK_KhungGios_LoaiNgay  CHECK (LoaiNgay IN ('TatCa','ThuThuong','CuoiTuan'))
);
GO

-- ============================================================
-- TẦNG 5: DICH VU — gắn với sân cụ thể (kho của Owner)
-- ============================================================
CREATE TABLE DichVus (
    Id              INT           IDENTITY(1,1) PRIMARY KEY,
    SanBongId       INT           NOT NULL,
    DanhMucDichVuId INT           NOT NULL,
    TenDichVu       NVARCHAR(100) NOT NULL,
    Gia             DECIMAL(18,2) NOT NULL,
    TonKho          INT           NOT NULL DEFAULT 0,
    IsActive        BIT           NOT NULL DEFAULT 1,
    MoTa            NVARCHAR(500) NULL,
    CONSTRAINT FK_DichVus_SanBong  FOREIGN KEY (SanBongId)       REFERENCES SanBongs(Id),
    CONSTRAINT FK_DichVus_DanhMuc  FOREIGN KEY (DanhMucDichVuId) REFERENCES DanhMucDichVu(Id)
);
GO

-- ============================================================
-- TẦNG 6: DAT SAN
--   Thêm: StaffCheckInId, StaffCheckOutId, GhiChuSuCo, LoaiSuCo
--   Thêm trạng thái: DangSuDung
-- ============================================================
CREATE TABLE DatSans (
    Id              INT           IDENTITY(1,1) PRIMARY KEY,
    UserId          INT           NOT NULL,
    KhungGioId      INT           NOT NULL,
    NgayThiDau      DATETIME      NOT NULL,
    TienCoc         DECIMAL(18,2) NOT NULL,
    TongTien        DECIMAL(18,2) NOT NULL DEFAULT 0,
    MaXacNhan       NVARCHAR(50)  NOT NULL UNIQUE,
    TrangThai       NVARCHAR(20)  NOT NULL DEFAULT 'ChoDuyet',
    -- v3: Staff nào thực hiện check-in / check-out
    StaffCheckInId  INT           NULL,
    StaffCheckOutId INT           NULL,
    -- v3: Staff ghi nhận sự cố (No-show / hỏng hóc)
    LoaiSuCo        NVARCHAR(20)  NULL,   -- 'NoShow' | 'HongHoc' | NULL
    GhiChuSuCo      NVARCHAR(500) NULL,
    ThoiGianTao     DATETIME      NOT NULL DEFAULT GETDATE(),
    CONSTRAINT FK_DatSans_User          FOREIGN KEY (UserId)          REFERENCES Users(Id),
    CONSTRAINT FK_DatSans_KhungGio      FOREIGN KEY (KhungGioId)      REFERENCES KhungGios(Id),
    CONSTRAINT FK_DatSans_StaffCheckIn  FOREIGN KEY (StaffCheckInId)  REFERENCES Users(Id),
    CONSTRAINT FK_DatSans_StaffCheckOut FOREIGN KEY (StaffCheckOutId) REFERENCES Users(Id),
    CONSTRAINT CHK_DatSans_TrangThai    CHECK (TrangThai IN (
        'ChoDuyet',    -- vừa đặt, chưa thanh toán cọc
        'DaXacNhan',   -- đã thanh toán cọc, chờ đến ngày đá
        'DangSuDung',  -- Staff check-in, đang đá
        'HoanThanh',   -- Staff check-out, thu đủ tiền
        'DaHuy'        -- đã hủy
    )),
    CONSTRAINT CHK_DatSans_LoaiSuCo CHECK (LoaiSuCo IN ('NoShow','HongHoc') OR LoaiSuCo IS NULL)
);
GO

-- ============================================================
-- TẦNG 7: STAFF SAN PHAN CONG — bảng quan hệ N-N
--   Owner gán Staff nào được quản lý sân nào
-- ============================================================
CREATE TABLE StaffSanPhanCong (
    Id        INT      IDENTITY(1,1) PRIMARY KEY,
    StaffId   INT      NOT NULL,
    SanBongId INT      NOT NULL,
    NgayGan   DATETIME NOT NULL DEFAULT GETDATE(),
    CONSTRAINT FK_StaffSan_Staff   FOREIGN KEY (StaffId)   REFERENCES Users(Id),
    CONSTRAINT FK_StaffSan_SanBong FOREIGN KEY (SanBongId) REFERENCES SanBongs(Id),
    -- Mỗi Staff chỉ được gán vào 1 sân 1 lần
    CONSTRAINT UQ_StaffSan UNIQUE (StaffId, SanBongId)
);
GO

-- ============================================================
-- TẦNG 8: DATSAN_DICHVUS
-- ============================================================
CREATE TABLE DatSan_DichVus (
    Id       INT IDENTITY(1,1) PRIMARY KEY,
    DatSanId INT NOT NULL,
    DichVuId INT NOT NULL,
    SoLuong  INT NOT NULL DEFAULT 1,
    CONSTRAINT FK_DatSanDichVu_DatSan FOREIGN KEY (DatSanId) REFERENCES DatSans(Id),
    CONSTRAINT FK_DatSanDichVu_DichVu FOREIGN KEY (DichVuId) REFERENCES DichVus(Id)
);
GO

-- ============================================================
-- TẦNG 9: DANH GIA
--   Thêm DatSanId + UNIQUE constraint để mỗi User chỉ đánh giá 1 lần/đơn
-- ============================================================
CREATE TABLE DanhGias (
    Id          INT            IDENTITY(1,1) PRIMARY KEY,
    SanBongId   INT            NOT NULL,
    UserId      INT            NOT NULL,
    DatSanId    INT            NOT NULL,
    SoSao       INT            NOT NULL DEFAULT 5,
    NhanXet     NVARCHAR(1000) NULL,
    NgayDanhGia DATETIME       NOT NULL DEFAULT GETDATE(),
    CONSTRAINT FK_DanhGias_SanBong  FOREIGN KEY (SanBongId) REFERENCES SanBongs(Id),
    CONSTRAINT FK_DanhGias_User     FOREIGN KEY (UserId)    REFERENCES Users(Id),
    CONSTRAINT FK_DanhGias_DatSan   FOREIGN KEY (DatSanId)  REFERENCES DatSans(Id),
    CONSTRAINT CHK_DanhGias_SoSao   CHECK (SoSao BETWEEN 1 AND 5),
    CONSTRAINT UQ_DanhGia_User_DatSan UNIQUE (UserId, DatSanId)
);
GO

-- ============================================================
-- TẦNG 10: MATCHMAKINGS
-- ============================================================
CREATE TABLE Matchmakings (
    Id             INT            IDENTITY(1,1) PRIMARY KEY,
    DatSanId       INT            NOT NULL UNIQUE,
    UserId         INT            NOT NULL,
    TieuDe         NVARCHAR(200)  NOT NULL,
    MoTa           NVARCHAR(1000) NULL,
    SoNguoiCanThem INT            NOT NULL DEFAULT 1,
    TrangThai      NVARCHAR(20)   NOT NULL DEFAULT 'DangTim',
    NgayDang       DATETIME       NOT NULL DEFAULT GETDATE(),
    CONSTRAINT FK_Matchmakings_DatSan     FOREIGN KEY (DatSanId) REFERENCES DatSans(Id),
    CONSTRAINT FK_Matchmakings_User       FOREIGN KEY (UserId)   REFERENCES Users(Id),
    CONSTRAINT CHK_Matchmakings_TrangThai CHECK (TrangThai IN ('DangTim','DaDu','DaDong'))
);
GO

-- ============================================================
-- TẦNG 11: KHIEU NAI — Admin xử lý hoàn cọc
-- ============================================================
CREATE TABLE KhieuNais (
    Id          INT            IDENTITY(1,1) PRIMARY KEY,
    DatSanId    INT            NOT NULL,
    UserId      INT            NOT NULL,
    LyDo        NVARCHAR(1000) NOT NULL,
    TrangThai   NVARCHAR(20)   NOT NULL DEFAULT 'ChoXuLy',
    GhiChuAdmin NVARCHAR(500)  NULL,
    SoTienHoan  DECIMAL(18,2)  NULL,
    NgayGui     DATETIME       NOT NULL DEFAULT GETDATE(),
    NgayXuLy    DATETIME       NULL,
    AdminXuLyId INT            NULL,
    CONSTRAINT FK_KhieuNais_DatSan FOREIGN KEY (DatSanId)    REFERENCES DatSans(Id),
    CONSTRAINT FK_KhieuNais_User   FOREIGN KEY (UserId)      REFERENCES Users(Id),
    CONSTRAINT FK_KhieuNais_Admin  FOREIGN KEY (AdminXuLyId) REFERENCES Users(Id),
    CONSTRAINT CHK_KhieuNais_TrangThai CHECK (TrangThai IN ('ChoXuLy','DaHoanCoc','TuChoi'))
);
GO

-- ============================================================
-- TẦNG 12: AUDIT LOGS — Bảo mật & hậu kiểm
-- ============================================================
CREATE TABLE AuditLogs (
    Id         INT            IDENTITY(1,1) PRIMARY KEY,
    UserId     INT            NOT NULL,
    VaiTro     NVARCHAR(20)   NOT NULL,
    HanhDong   NVARCHAR(100)  NOT NULL,
    DoiTuong   NVARCHAR(50)   NOT NULL,
    DoiTuongId INT            NOT NULL,
    MoTa       NVARCHAR(500)  NULL,
    IpAddress  NVARCHAR(50)   NULL,
    ThoiGian   DATETIME       NOT NULL DEFAULT GETDATE(),
    CONSTRAINT FK_AuditLogs_User FOREIGN KEY (UserId) REFERENCES Users(Id)
);
GO

-- ============================================================
-- INDEX — tối ưu hiệu năng truy vấn
-- ============================================================

-- Tìm kiếm sân
CREATE INDEX IX_SanBongs_Quan         ON SanBongs (Quan);
CREATE INDEX IX_SanBongs_TrangThai    ON SanBongs (TrangThaiDuyet);
CREATE INDEX IX_SanBongs_OwnerId      ON SanBongs (OwnerId);
CREATE INDEX IX_SanBongs_IsHidden     ON SanBongs (IsHidden);

-- Time-slot grid
CREATE INDEX IX_KhungGios_SanBongId   ON KhungGios (SanBongId);
CREATE INDEX IX_KhungGios_TrangThai   ON KhungGios (TrangThai);
CREATE INDEX IX_KhungGios_LoaiNgay    ON KhungGios (LoaiNgay);

-- Quản lý đơn
CREATE INDEX IX_DatSans_UserId        ON DatSans (UserId);
CREATE INDEX IX_DatSans_TrangThai     ON DatSans (TrangThai);
CREATE INDEX IX_DatSans_StaffCheckIn  ON DatSans (StaffCheckInId);
CREATE INDEX IX_DatSans_NgayThiDau    ON DatSans (NgayThiDau);

-- Dịch vụ
CREATE INDEX IX_DichVus_SanBongId     ON DichVus (SanBongId);

-- Staff phân công
CREATE INDEX IX_StaffSan_StaffId      ON StaffSanPhanCong (StaffId);
CREATE INDEX IX_StaffSan_SanBongId    ON StaffSanPhanCong (SanBongId);

-- Admin
CREATE INDEX IX_AuditLogs_UserId      ON AuditLogs (UserId);
CREATE INDEX IX_AuditLogs_ThoiGian    ON AuditLogs (ThoiGian);
CREATE INDEX IX_AuditLogs_HanhDong    ON AuditLogs (HanhDong);
CREATE INDEX IX_KhieuNais_TrangThai   ON KhieuNais (TrangThai);

-- Users
CREATE INDEX IX_Users_VaiTro          ON Users (VaiTro);
CREATE INDEX IX_Users_OwnerIdCuaStaff ON Users (OwnerIdCuaStaff);
CREATE INDEX IX_Matchmakings_TrangThai ON Matchmakings (TrangThai);
GO

-- ============================================================
-- SEED DATA
-- ============================================================

-- ⚠️  QUAN TRỌNG VỀ MẬT KHẨU:
--     Các chuỗi hash dưới đây là BCrypt của chuỗi tương ứng.
--     Để generate hash đúng, chạy đoạn code C# sau trong project:
--
--     using BCrypt.Net;
--     Console.WriteLine(BCrypt.HashPassword("admin123"));
--     Console.WriteLine(BCrypt.HashPassword("owner123"));
--     Console.WriteLine(BCrypt.HashPassword("user123"));
--     Console.WriteLine(BCrypt.HashPassword("staff123"));
--
--     Rồi thay thế 4 chuỗi BCRYPT_PLACEHOLDER bên dưới.
--     Tạm thời để plaintext để chạy thử, nhớ đổi lại trước khi demo.

DECLARE
    -- Master Data
    @qCauGiay   INT, @qDongDa   INT, @qHoangMai INT,
    @qLongBien  INT, @qNamTuLiem INT,
    @lsSan5     INT, @lsSan7    INT, @lsSan11   INT,
    @lcNhanTao  INT, @lcTuNhien INT,
    @dmNuoc     INT, @dmBong    INT, @dmTai     INT, @dmAo INT,
    -- Users
    @idAdmin    INT, @idOwner   INT, @idUser    INT, @idStaff INT,
    -- SanBongs
    @idSan1     INT, @idSan2    INT, @idSan3    INT,
    @idSan4     INT, @idSan5    INT,
    -- DichVus
    @dv1Nuoc    INT, @dv1Bong   INT, @dv1Tai    INT, @dv1Ao  INT,
    -- KhungGios
    @kgDat1     INT, @kgDat2    INT,
    -- DatSans
    @ds1        INT, @ds2       INT;

-- ── 1. MASTER DATA: QUẬN ─────────────────────────────────────
INSERT INTO DanhMucQuan (TenQuan, ThanhPho, ThuTu)
VALUES
    (N'Cầu Giấy',     N'Hà Nội', 1),
    (N'Đống Đa',      N'Hà Nội', 2),
    (N'Hoàng Mai',    N'Hà Nội', 3),
    (N'Long Biên',    N'Hà Nội', 4),
    (N'Nam Từ Liêm',  N'Hà Nội', 5),
    (N'Bắc Từ Liêm',  N'Hà Nội', 6),
    (N'Tây Hồ',       N'Hà Nội', 7),
    (N'Ba Đình',      N'Hà Nội', 8),
    (N'Hai Bà Trưng', N'Hà Nội', 9),
    (N'Thanh Xuân',   N'Hà Nội', 10);

SELECT @qCauGiay   = Id FROM DanhMucQuan WHERE TenQuan = N'Cầu Giấy';
SELECT @qDongDa    = Id FROM DanhMucQuan WHERE TenQuan = N'Đống Đa';
SELECT @qHoangMai  = Id FROM DanhMucQuan WHERE TenQuan = N'Hoàng Mai';
SELECT @qLongBien  = Id FROM DanhMucQuan WHERE TenQuan = N'Long Biên';
SELECT @qNamTuLiem = Id FROM DanhMucQuan WHERE TenQuan = N'Nam Từ Liêm';
PRINT N'✔ DanhMucQuan: 10 quận';

-- ── 2. MASTER DATA: LOẠI SÂN ──────────────────────────────────
INSERT INTO DanhMucLoaiSan (MaLoai, TenLoai) VALUES ('5',  N'Sân 5 người');
INSERT INTO DanhMucLoaiSan (MaLoai, TenLoai) VALUES ('7',  N'Sân 7 người');
INSERT INTO DanhMucLoaiSan (MaLoai, TenLoai) VALUES ('11', N'Sân 11 người');
SELECT @lsSan5  = Id FROM DanhMucLoaiSan WHERE MaLoai = '5';
SELECT @lsSan7  = Id FROM DanhMucLoaiSan WHERE MaLoai = '7';
SELECT @lsSan11 = Id FROM DanhMucLoaiSan WHERE MaLoai = '11';
PRINT N'✔ DanhMucLoaiSan: 3 loại';

-- ── 3. MASTER DATA: LOẠI CỎ ───────────────────────────────────
INSERT INTO DanhMucLoaiCo (MaLoai, TenLoai) VALUES ('Nhan tao', N'Cỏ nhân tạo');
INSERT INTO DanhMucLoaiCo (MaLoai, TenLoai) VALUES ('Tu nhien', N'Cỏ tự nhiên');
SELECT @lcNhanTao = Id FROM DanhMucLoaiCo WHERE MaLoai = 'Nhan tao';
SELECT @lcTuNhien = Id FROM DanhMucLoaiCo WHERE MaLoai = 'Tu nhien';
PRINT N'✔ DanhMucLoaiCo: 2 loại';

-- ── 4. MASTER DATA: DỊCH VỤ ──────────────────────────────────
INSERT INTO DanhMucDichVu (TenDichVu, Icon, MoTa)
VALUES
    (N'Nước uống',     N'💧', N'Nước lọc / nước ngọt / tăng lực'),
    (N'Thuê bóng',     N'⚽', N'Bóng thi đấu tiêu chuẩn Size 4/5'),
    (N'Thuê trọng tài',N'🟡', N'Trọng tài có kinh nghiệm'),
    (N'Thuê áo đấu',   N'👕', N'Áo thi đấu có số, 2 màu');
SELECT @dmNuoc = Id FROM DanhMucDichVu WHERE TenDichVu = N'Nước uống';
SELECT @dmBong = Id FROM DanhMucDichVu WHERE TenDichVu = N'Thuê bóng';
SELECT @dmTai  = Id FROM DanhMucDichVu WHERE TenDichVu = N'Thuê trọng tài';
SELECT @dmAo   = Id FROM DanhMucDichVu WHERE TenDichVu = N'Thuê áo đấu';
PRINT N'✔ DanhMucDichVu: 4 danh mục';

-- ── 5. USERS ──────────────────────────────────────────────────
-- ⚠️  Thay BCRYPT_HASH_xxx bằng hash thật từ BCrypt.Net trước khi chạy
-- Tạm thời để plaintext để demo, nhớ đổi trước khi nộp bài

INSERT INTO Users (HoTen, Email, MatKhau, SoDienThoai, VaiTro, IsActive)
VALUES (N'Admin PitchHub', 'admin@pitchhub.vn',
        '$2a$11$f0tXD6o7XYAs7/tE0nx4Reiw1.84L2ItgL0tRwE1Bq.GZbE8MMuzS',   -- đổi thành BCrypt hash của "admin123"
        '0901000001', 'Admin', 1);
SET @idAdmin = SCOPE_IDENTITY();

INSERT INTO Users (HoTen, Email, MatKhau, SoDienThoai, VaiTro, IsActive)
VALUES (N'Vũ Nguyễn Tuấn Kiệt', 'owner1@gmail.com',
        '$2a$11$dZvxGdl0dNQWsvIM4IO2VuM4kGwP60qFmpbIOKvD0iLulA00/4cCW',   -- đổi thành BCrypt hash của "owner123"
        '0901000002', 'Owner', 1);
SET @idOwner = SCOPE_IDENTITY();

INSERT INTO Users (HoTen, Email, MatKhau, SoDienThoai, VaiTro, IsActive)
VALUES (N'Nguyễn Công Nam', 'user1@gmail.com',
        '$2a$11$wmJXgVs5/RBXU2y/vP4Bs.UcgtzPg0r4Iv2t3DgsZvDrOKD32vPzO',    -- đổi thành BCrypt hash của "user123"
        '0901000003', 'User', 1);
SET @idUser = SCOPE_IDENTITY();

-- Staff: OwnerIdCuaStaff trỏ về @idOwner
INSERT INTO Users (HoTen, Email, MatKhau, SoDienThoai, VaiTro, IsActive, OwnerIdCuaStaff)
VALUES (N'Đào Việt Toàn', 'staff@pitchhub.vn',
        '$2a$11$oOUcIEMbESDcI5QECnfcBOzJtTAAsWbRM3ZX7KQhH3XIWWdmtfR1S',   -- đổi thành BCrypt hash của "staff123"
        '0901000004', 'Staff', 1, @idOwner);
SET @idStaff = SCOPE_IDENTITY();

PRINT CONCAT(N'✔ Users: Admin=', @idAdmin,
             ' Owner=', @idOwner,
             ' User=',  @idUser,
             ' Staff=', @idStaff);

-- ── 6. SAN BONG ───────────────────────────────────────────────
INSERT INTO SanBongs (TenSan, DiaChi, Quan, ThanhPho, LoaiSan, LoaiCo,
                      MoTa, DanhGiaTrungBinh, Latitude, Longitude,
                      TrangThaiDuyet, TyLeCoc, IsHidden, OwnerId)
VALUES (N'Sân Cầu Giấy Sport', N'12 Xuân Thủy',
        N'Cầu Giấy', N'Hà Nội', '5', 'Nhan tao',
        N'Sân cỏ nhân tạo chất lượng cao, đèn chiếu sáng ban đêm.',
        4.5, 21.0362, 105.7826, 'DaDuyet', 0.30, 0, @idOwner);
SET @idSan1 = SCOPE_IDENTITY();

INSERT INTO SanBongs (TenSan, DiaChi, Quan, ThanhPho, LoaiSan, LoaiCo,
                      MoTa, DanhGiaTrungBinh, Latitude, Longitude,
                      TrangThaiDuyet, TyLeCoc, IsHidden, OwnerId)
VALUES (N'Sân Đống Đa Arena', N'45 Tây Sơn',
        N'Đống Đa', N'Hà Nội', '7', 'Nhan tao',
        N'Sân 7 người rộng rãi, có mái che, chỗ để xe rộng.',
        4.2, 21.0198, 105.8412, 'DaDuyet', 0.30, 0, @idOwner);
SET @idSan2 = SCOPE_IDENTITY();

INSERT INTO SanBongs (TenSan, DiaChi, Quan, ThanhPho, LoaiSan, LoaiCo,
                      MoTa, DanhGiaTrungBinh, Latitude, Longitude,
                      TrangThaiDuyet, TyLeCoc, IsHidden, OwnerId)
VALUES (N'Sân Hoàng Mai FC', N'78 Giải Phóng',
        N'Hoàng Mai', N'Hà Nội', '5', 'Tu nhien',
        N'Sân cỏ tự nhiên thoáng mát, phù hợp thi đấu buổi sáng.',
        3.8, 20.9876, 105.8543, 'DaDuyet', 0.50, 0, @idOwner);
SET @idSan3 = SCOPE_IDENTITY();

INSERT INTO SanBongs (TenSan, DiaChi, Quan, ThanhPho, LoaiSan, LoaiCo,
                      MoTa, DanhGiaTrungBinh, Latitude, Longitude,
                      TrangThaiDuyet, TyLeCoc, IsHidden, OwnerId)
VALUES (N'Sân Long Biên Star', N'23 Nguyễn Văn Cừ',
        N'Long Biên', N'Hà Nội', '11', 'Nhan tao',
        N'Sân 11 người tiêu chuẩn FIFA, có phòng thay đồ.',
        4.7, 21.0465, 105.8923, 'DaDuyet', 0.30, 0, @idOwner);
SET @idSan4 = SCOPE_IDENTITY();

INSERT INTO SanBongs (TenSan, DiaChi, Quan, ThanhPho, LoaiSan, LoaiCo,
                      MoTa, DanhGiaTrungBinh, Latitude, Longitude,
                      TrangThaiDuyet, TyLeCoc, IsHidden, OwnerId)
VALUES (N'Sân Chờ Duyệt Test', N'99 Test Street',
        N'Nam Từ Liêm', N'Hà Nội', '5', 'Nhan tao',
        N'Sân đang chờ Admin phê duyệt.',
        0, 21.0100, 105.7500, 'ChoDuyet', 0.30, 0, @idOwner);
SET @idSan5 = SCOPE_IDENTITY();

PRINT CONCAT(N'✔ SanBongs: ', @idSan1,' ',@idSan2,' ',@idSan3,' ',@idSan4,' ',@idSan5);

-- ── 7. STAFF PHAN CONG SAN ────────────────────────────────────
-- Staff được gán vào Sân 1 và Sân 2
INSERT INTO StaffSanPhanCong (StaffId, SanBongId) VALUES (@idStaff, @idSan1);
INSERT INTO StaffSanPhanCong (StaffId, SanBongId) VALUES (@idStaff, @idSan2);
PRINT N'✔ StaffSanPhanCong: Staff gán vào Sân 1 + Sân 2';

-- ── 8. KHUNG GIO ─────────────────────────────────────────────
-- Sân 1 — khung giờ thường + cuối tuần
INSERT INTO KhungGios (SanBongId, GioBatDau, GioKetThuc, Gia, GiaGioVang, GiaCuoiTuan, LoaiNgay, TrangThai)
VALUES
    (@idSan1,'06:00','07:30',180000,220000,200000,'TatCa',   'Trong'),
    (@idSan1,'07:30','09:00',180000,220000,200000,'TatCa',   'Trong'),
    (@idSan1,'09:00','10:30',160000,200000,180000,'TatCa',   'Trong'),
    (@idSan1,'15:00','16:30',160000,200000,180000,'TatCa',   'Trong'),
    (@idSan1,'16:30','18:00',200000,260000,240000,'TatCa',   'Trong'),
    (@idSan1,'18:00','19:30',250000,300000,320000,'TatCa',   'DaDat'),
    (@idSan1,'19:30','21:00',250000,300000,320000,'TatCa',   'Trong');
SELECT @kgDat1 = Id FROM KhungGios
WHERE SanBongId = @idSan1 AND GioBatDau = '18:00' AND TrangThai = 'DaDat';

-- Sân 2
INSERT INTO KhungGios (SanBongId, GioBatDau, GioKetThuc, Gia, GiaGioVang, GiaCuoiTuan, LoaiNgay, TrangThai)
VALUES
    (@idSan2,'06:00','07:30',250000,300000,280000,'TatCa','Trong'),
    (@idSan2,'07:30','09:00',250000,300000,280000,'TatCa','Trong'),
    (@idSan2,'17:00','18:30',320000,380000,400000,'TatCa','DaDat'),
    (@idSan2,'18:30','20:00',350000,420000,440000,'TatCa','Trong'),
    (@idSan2,'20:00','21:30',350000,420000,440000,'TatCa','Trong');
SELECT @kgDat2 = Id FROM KhungGios
WHERE SanBongId = @idSan2 AND GioBatDau = '17:00' AND TrangThai = 'DaDat';

-- Sân 3
INSERT INTO KhungGios (SanBongId, GioBatDau, GioKetThuc, Gia, GiaGioVang, GiaCuoiTuan, LoaiNgay, TrangThai)
VALUES
    (@idSan3,'05:30','07:00',150000,180000,170000,'TatCa','Trong'),
    (@idSan3,'07:00','08:30',150000,180000,170000,'TatCa','Trong'),
    (@idSan3,'16:00','17:30',180000,220000,200000,'TatCa','Trong'),
    (@idSan3,'17:30','19:00',200000,250000,230000,'TatCa','Trong');

-- Sân 4
INSERT INTO KhungGios (SanBongId, GioBatDau, GioKetThuc, Gia, GiaGioVang, GiaCuoiTuan, LoaiNgay, TrangThai)
VALUES
    (@idSan4,'06:00','08:00',500000,600000,580000,'TatCa','Trong'),
    (@idSan4,'08:00','10:00',500000,600000,580000,'TatCa','Trong'),
    (@idSan4,'15:00','17:00',550000,650000,620000,'TatCa','Trong'),
    (@idSan4,'17:00','19:00',650000,780000,750000,'TatCa','Trong'),
    (@idSan4,'19:00','21:00',650000,780000,750000,'TatCa','Trong');

PRINT CONCAT(N'✔ KhungGios xong. kgDat1=', @kgDat1, ' kgDat2=', @kgDat2);

-- ── 9. DICH VU CUA SAN 1 ─────────────────────────────────────
INSERT INTO DichVus (SanBongId, DanhMucDichVuId, TenDichVu, Gia, TonKho, MoTa)
VALUES
    (@idSan1, @dmNuoc, N'Nước uống',     15000,  100, N'Nước lọc / nước ngọt'),
    (@idSan1, @dmBong, N'Thuê bóng',     30000,   10, N'Bóng Size 4/5'),
    (@idSan1, @dmTai,  N'Thuê trọng tài',100000,   3, N'Trọng tài kinh nghiệm'),
    (@idSan1, @dmAo,   N'Thuê áo đấu',   20000,   22, N'Áo 2 màu có số');
SELECT @dv1Nuoc = Id FROM DichVus WHERE SanBongId = @idSan1 AND DanhMucDichVuId = @dmNuoc;
SELECT @dv1Bong = Id FROM DichVus WHERE SanBongId = @idSan1 AND DanhMucDichVuId = @dmBong;
SELECT @dv1Tai  = Id FROM DichVus WHERE SanBongId = @idSan1 AND DanhMucDichVuId = @dmTai;
SELECT @dv1Ao   = Id FROM DichVus WHERE SanBongId = @idSan1 AND DanhMucDichVuId = @dmAo;
PRINT N'✔ DichVus Sân 1 xong';

-- ── 10. DAT SAN ───────────────────────────────────────────────
-- Đơn 1: DaXacNhan — đã thanh toán cọc, chờ đến ngày đá
INSERT INTO DatSans (UserId, KhungGioId, NgayThiDau, TienCoc, TongTien, MaXacNhan, TrangThai)
VALUES (@idUser, @kgDat1, DATEADD(DAY,2,GETDATE()), 75000, 250000, 'ABC12345', 'DaXacNhan');
SET @ds1 = SCOPE_IDENTITY();

-- Đơn 2: ChoDuyet — vừa đặt, chưa thanh toán
INSERT INTO DatSans (UserId, KhungGioId, NgayThiDau, TienCoc, TongTien, MaXacNhan, TrangThai)
VALUES (@idUser, @kgDat2, DATEADD(DAY,3,GETDATE()), 105000, 0, 'DEF67890', 'ChoDuyet');
SET @ds2 = SCOPE_IDENTITY();

PRINT CONCAT(N'✔ DatSans: ds1=', @ds1, ' ds2=', @ds2);

-- ── 11. DICH VU KEM THEO ─────────────────────────────────────
INSERT INTO DatSan_DichVus (DatSanId, DichVuId, SoLuong)
VALUES
    (@ds1, @dv1Nuoc, 10),
    (@ds1, @dv1Bong,  1),
    (@ds1, @dv1Tai,   1);

-- ── 12. DANH GIA ─────────────────────────────────────────────
-- Chỉ demo — thực tế controller phải check TrangThai = 'HoanThanh' trước
INSERT INTO DanhGias (SanBongId, UserId, DatSanId, SoSao, NhanXet)
VALUES (@idSan1, @idUser, @ds1, 5, N'Sân rất đẹp, cỏ mới, nhân viên nhiệt tình!');

UPDATE SanBongs SET DanhGiaTrungBinh = 5.0 WHERE Id = @idSan1;
UPDATE SanBongs SET DanhGiaTrungBinh = 4.2 WHERE Id = @idSan2;
UPDATE SanBongs SET DanhGiaTrungBinh = 3.8 WHERE Id = @idSan3;
UPDATE SanBongs SET DanhGiaTrungBinh = 4.7 WHERE Id = @idSan4;

-- ── 13. MATCHMAKING ──────────────────────────────────────────
INSERT INTO Matchmakings (DatSanId, UserId, TieuDe, MoTa, SoNguoiCanThem, TrangThai)
VALUES (@ds1, @idUser,
        N'Cần thêm 2 tiền đạo — Sân Cầu Giấy 18h tối mai',
        N'Team trình độ trung bình, chơi vui là chính. LH ngay!',
        2, 'DangTim');

-- ── 14. AUDIT LOG MẪU ────────────────────────────────────────
INSERT INTO AuditLogs (UserId, VaiTro, HanhDong, DoiTuong, DoiTuongId, MoTa)
VALUES
    (@idAdmin, 'Admin', 'PheDuyetSan', 'SanBong', @idSan1, N'Admin phê duyệt: Sân Cầu Giấy Sport'),
    (@idAdmin, 'Admin', 'PheDuyetSan', 'SanBong', @idSan2, N'Admin phê duyệt: Sân Đống Đa Arena'),
    (@idAdmin, 'Admin', 'PheDuyetSan', 'SanBong', @idSan3, N'Admin phê duyệt: Sân Hoàng Mai FC'),
    (@idAdmin, 'Admin', 'PheDuyetSan', 'SanBong', @idSan4, N'Admin phê duyệt: Sân Long Biên Star');

-- ============================================================
-- KIỂM TRA TỔNG
-- ============================================================
SELECT [Bang] = v.n, [So ban ghi] = v.c FROM (VALUES
    ('DanhMucQuan',      (SELECT COUNT(*) FROM DanhMucQuan)),
    ('DanhMucLoaiSan',   (SELECT COUNT(*) FROM DanhMucLoaiSan)),
    ('DanhMucLoaiCo',    (SELECT COUNT(*) FROM DanhMucLoaiCo)),
    ('DanhMucDichVu',    (SELECT COUNT(*) FROM DanhMucDichVu)),
    ('Users',            (SELECT COUNT(*) FROM Users)),
    ('SanBongs',         (SELECT COUNT(*) FROM SanBongs)),
    ('StaffSanPhanCong', (SELECT COUNT(*) FROM StaffSanPhanCong)),
    ('KhungGios',        (SELECT COUNT(*) FROM KhungGios)),
    ('DichVus',          (SELECT COUNT(*) FROM DichVus)),
    ('DatSans',          (SELECT COUNT(*) FROM DatSans)),
    ('DatSan_DichVus',   (SELECT COUNT(*) FROM DatSan_DichVus)),
    ('DanhGias',         (SELECT COUNT(*) FROM DanhGias)),
    ('Matchmakings',     (SELECT COUNT(*) FROM Matchmakings)),
    ('KhieuNais',        (SELECT COUNT(*) FROM KhieuNais)),
    ('AuditLogs',        (SELECT COUNT(*) FROM AuditLogs))
) v(n,c);

PRINT '=========================================================';
PRINT N'✅ Database SanBongBTL v3 tạo thành công!';
PRINT N'';
PRINT N'⚠️  NHẮC NHỞ QUAN TRỌNG:';
PRINT N'    Thay 4 chuỗi BCRYPT_HASH_xxx bằng hash thật từ BCrypt.Net';
PRINT N'    trước khi chạy file này lần cuối để demo/nộp bài.';
PRINT N'';
PRINT N'Tài khoản test (sau khi cập nhật hash):';
PRINT N'  Admin : admin@pitchhub.vn  / admin123';
PRINT N'  Owner : owner1@gmail.com   / owner123';
PRINT N'  User  : user1@gmail.com    / user123';
PRINT N'  Staff : staff@pitchhub.vn  / staff123';
PRINT '=========================================================';
GO

-- ============================================================
--  PitchHub — SQL PATCH v4
--  Bổ sung:
--    1. Bảng VungKhuVuc (phân vùng hoa hồng linh hoạt)
--    2. DanhMucQuan thêm VungKhuVucId (FK)
--    3. SanBongs thêm DaKyHopDong, NgayKyHopDong, NoiDungHopDong
--  Chạy SAU SanBongBTL_v3.sql
-- ============================================================

USE SanBongBTL;
GO

-- ── 1. BẢNG VUNGKHUVUC ──────────────────────────────────────
-- Admin tạo/sửa/xoá qua giao diện — không cần code lại
CREATE TABLE VungKhuVucs (
    Id             INT           IDENTITY(1,1) PRIMARY KEY,
    TenVung        NVARCHAR(100) NOT NULL UNIQUE,
    MoTa           NVARCHAR(300) NULL,
    -- Tỷ lệ hoa hồng áp dụng cho toàn vùng
    TyLeHoaHong    DECIMAL(3,2)  NOT NULL DEFAULT 0.10,
    -- Màu sắc hiển thị trên bản đồ và UI (hex color)
    MauSac         NVARCHAR(10)  NOT NULL DEFAULT '#1ed760',
    -- Tọa độ trung tâm vùng (để bản đồ tự zoom về)
    Lat            FLOAT         NOT NULL DEFAULT 21.0285,
    Lng            FLOAT         NOT NULL DEFAULT 105.8542,
    DefaultZoom    INT           NOT NULL DEFAULT 12,
    ThuTu          INT           NOT NULL DEFAULT 0,
    IsActive       BIT           NOT NULL DEFAULT 1,
    CONSTRAINT CHK_VungKhuVuc_TyLe CHECK (TyLeHoaHong BETWEEN 0.01 AND 0.50)
);
GO

-- ── 2. CẬP NHẬT DanhMucQuan — thêm VungKhuVucId ─────────────
ALTER TABLE DanhMucQuan
ADD VungKhuVucId INT NULL,
    CONSTRAINT FK_DanhMucQuan_Vung
        FOREIGN KEY (VungKhuVucId) REFERENCES VungKhuVucs(Id);
GO

-- ── 3. CẬP NHẬT SanBongs — thêm trường hợp đồng ────────────
ALTER TABLE SanBongs
ADD DaKyHopDong    BIT           NOT NULL DEFAULT 0,
    NgayKyHopDong  DATETIME      NULL,
    -- Lưu snapshot nội dung HĐ lúc ký — không bao giờ thay đổi sau
    NoiDungHopDong NVARCHAR(MAX) NULL;
GO

-- ── 4. SEED: VungKhuVucs ─────────────────────────────────────
DECLARE @v1 INT, @v2 INT, @v3 INT, @v4 INT;

INSERT INTO VungKhuVucs (TenVung, MoTa, TyLeHoaHong, MauSac, Lat, Lng, DefaultZoom, ThuTu)
VALUES (N'Nội ô trung tâm',
        N'Hoàn Kiếm, Ba Đình, Đống Đa, Hai Bà Trưng — khu vực trung tâm lịch sử',
        0.10, '#1ed760', 21.0285, 105.8542, 13, 1);
SET @v1 = SCOPE_IDENTITY();

INSERT INTO VungKhuVucs (TenVung, MoTa, TyLeHoaHong, MauSac, Lat, Lng, DefaultZoom, ThuTu)
VALUES (N'Nội ô mở rộng',
        N'Cầu Giấy, Tây Hồ, Thanh Xuân, Bắc Từ Liêm — vùng nội ô phát triển',
        0.08, '#6caee0', 21.0500, 105.7900, 12, 2);
SET @v2 = SCOPE_IDENTITY();

INSERT INTO VungKhuVucs (TenVung, MoTa, TyLeHoaHong, MauSac, Lat, Lng, DefaultZoom, ThuTu)
VALUES (N'Vùng ngoại ô',
        N'Hà Đông, Long Biên, Hoàng Mai, Nam Từ Liêm — vùng ven đô',
        0.06, '#f39c12', 20.9800, 105.8800, 11, 3);
SET @v3 = SCOPE_IDENTITY();

INSERT INTO VungKhuVucs (TenVung, MoTa, TyLeHoaHong, MauSac, Lat, Lng, DefaultZoom, ThuTu)
VALUES (N'Vùng xa trung tâm',
        N'Sơn Tây, Ba Vì, Mỹ Đức — vùng ngoại thành xa',
        0.05, '#e74c3c', 21.1200, 105.5000, 10, 4);
SET @v4 = SCOPE_IDENTITY();

-- ── 5. Gán Quận vào Vùng ─────────────────────────────────────
-- Nội ô trung tâm
UPDATE DanhMucQuan SET VungKhuVucId = @v1
WHERE TenQuan IN (N'Hoàn Kiếm', N'Ba Đình', N'Đống Đa', N'Hai Bà Trưng');

-- Nội ô mở rộng
UPDATE DanhMucQuan SET VungKhuVucId = @v2
WHERE TenQuan IN (N'Cầu Giấy', N'Tây Hồ', N'Thanh Xuân', N'Bắc Từ Liêm');

-- Ngoại ô
UPDATE DanhMucQuan SET VungKhuVucId = @v3
WHERE TenQuan IN (N'Hà Đông', N'Long Biên', N'Hoàng Mai', N'Nam Từ Liêm');

-- Vùng xa
-- (Sơn Tây chưa có trong seed DanhMucQuan — thêm nếu cần)

PRINT N'✔ VungKhuVucs: 4 vùng';
PRINT N'✔ DanhMucQuan: đã gán VungKhuVucId';
PRINT N'✔ SanBongs: đã thêm DaKyHopDong, NgayKyHopDong, NoiDungHopDong';
GO

-- Kiểm tra
SELECT q.TenQuan, v.TenVung, v.TyLeHoaHong, v.MauSac
FROM DanhMucQuan q
LEFT JOIN VungKhuVucs v ON q.VungKhuVucId = v.Id
ORDER BY v.ThuTu, q.ThuTu;
GO

--11/5/2026 update
-- 1. Gán tất cả dịch vụ hiện có cho sân số 1 và sân số 2 để kiểm tra
UPDATE DichVus 
SET SanBongId = 2, -- Gán cho sân số 2
    TonKho = 100,  -- Đảm bảo có hàng trong kho
    IsActive = 1   -- Đảm bảo dịch vụ đang hoạt động
WHERE SanBongId IS NULL;

-- 2. Kiểm tra lại dữ liệu xem đã khớp chưa
SELECT Id, TenDichVu, SanBongId, TonKho 
FROM DichVus;

-- ============================================================
-- MIGRATION: Tao bang AnhSanBongs cho Owner upload nhieu anh
-- Chay trong SSMS voi database SanBongBTL
-- ============================================================


--Bo sung 15/5/2026
USE SanBongBTL;
GO

-- Tao bang AnhSanBongs
IF NOT EXISTS (SELECT * FROM sys.objects WHERE name = 'AnhSanBongs' AND type = 'U')
BEGIN
    CREATE TABLE AnhSanBongs (
        Id          INT            IDENTITY(1,1) PRIMARY KEY,
        SanBongId   INT            NOT NULL,
        DuongDan    NVARCHAR(1000) NOT NULL,   -- URL hoac duong dan file
        LoaiAnh     NVARCHAR(20)   NOT NULL DEFAULT 'Upload',  -- 'Upload' | 'URL'
        ThuTu       INT            NOT NULL DEFAULT 0,          -- Thu tu hien thi
        MoTa        NVARCHAR(200)  NULL,
        NgayThem    DATETIME       NOT NULL DEFAULT GETDATE(),
        IsActive    BIT            NOT NULL DEFAULT 1,

        CONSTRAINT FK_AnhSanBongs_SanBong
            FOREIGN KEY (SanBongId) REFERENCES SanBongs(Id) ON DELETE CASCADE
    );

    CREATE INDEX IX_AnhSanBongs_SanBong ON AnhSanBongs (SanBongId, ThuTu);
    PRINT N'Da tao bang AnhSanBongs thanh cong';
END
ELSE
    PRINT N'Bang AnhSanBongs da ton tai';
GO



--UPDATE NGÀY 19/05/2026
-- ============================================================
--  PitchHub.vn — SQL MIGRATION v5
--  Mục đích: Bổ sung các bảng/cột còn thiếu để hoàn thiện flow
--  Chạy SAU SanBongBTL.sql (v3 + v4 patch)
--
--  Danh sách thay đổi:
--    1. Users          : thêm DaXacThucSDT, DiemHienTai
--    2. DanhGias       : thêm SoSaoCoSoVatChat, SoSaoNhanVien
--    3. SanYeuThichs   : BẢNG MỚI — bookmark sân yêu thích
--    4. DiemThuongLogs : BẢNG MỚI — lịch sử giao dịch điểm
--    5. Vouchers       : BẢNG MỚI — voucher hệ thống tạo
--    6. UserVouchers   : BẢNG MỚI — voucher người dùng đang giữ
--    7. OtpCodes       : BẢNG MỚI — xác thực OTP số điện thoại
-- ============================================================

USE SanBongBTL;
GO

PRINT N'========================================================';
PRINT N'  PitchHub Migration v5 — Bắt đầu...';
PRINT N'========================================================';

-- ============================================================
-- 1. BẢNG USERS — thêm 2 cột
-- ============================================================

-- DaXacThucSDT: kiểm soát bước OTP lần đầu đặt sân
IF NOT EXISTS (
    SELECT 1 FROM sys.columns 
    WHERE object_id = OBJECT_ID('Users') AND name = 'DaXacThucSDT'
)
BEGIN
    ALTER TABLE Users ADD DaXacThucSDT BIT NOT NULL DEFAULT 0;
    PRINT N'✔ Users.DaXacThucSDT đã thêm';
END
ELSE
    PRINT N'⚠ Users.DaXacThucSDT đã tồn tại, bỏ qua';

-- DiemHienTai: tổng điểm thưởng hiện có (cache, nguồn thật là DiemThuongLogs)
IF NOT EXISTS (
    SELECT 1 FROM sys.columns 
    WHERE object_id = OBJECT_ID('Users') AND name = 'DiemHienTai'
)
BEGIN
    ALTER TABLE Users ADD DiemHienTai INT NOT NULL DEFAULT 0;
    PRINT N'✔ Users.DiemHienTai đã thêm';
END
ELSE
    PRINT N'⚠ Users.DiemHienTai đã tồn tại, bỏ qua';

GO

-- ============================================================
-- 2. BẢNG DANH GIAS — thêm 2 tiêu chí đánh giá riêng
--    Flow yêu cầu 3 hàng sao: Cỏ / Cơ sở vật chất / Nhân viên
--    SoSao (cũ) giữ nguyên → dùng làm điểm tổng / trung bình
-- ============================================================

IF NOT EXISTS (
    SELECT 1 FROM sys.columns 
    WHERE object_id = OBJECT_ID('DanhGias') AND name = 'SoSaoCoSoVatChat'
)
BEGIN
    ALTER TABLE DanhGias ADD SoSaoCoSoVatChat INT NULL
        CONSTRAINT CHK_DanhGias_SoSaoCSVC CHECK (SoSaoCoSoVatChat BETWEEN 1 AND 5);
    PRINT N'✔ DanhGias.SoSaoCoSoVatChat đã thêm';
END
ELSE
    PRINT N'⚠ DanhGias.SoSaoCoSoVatChat đã tồn tại, bỏ qua';

IF NOT EXISTS (
    SELECT 1 FROM sys.columns 
    WHERE object_id = OBJECT_ID('DanhGias') AND name = 'SoSaoNhanVien'
)
BEGIN
    ALTER TABLE DanhGias ADD SoSaoNhanVien INT NULL
        CONSTRAINT CHK_DanhGias_SoSaoNV CHECK (SoSaoNhanVien BETWEEN 1 AND 5);
    PRINT N'✔ DanhGias.SoSaoNhanVien đã thêm';
END
ELSE
    PRINT N'⚠ DanhGias.SoSaoNhanVien đã tồn tại, bỏ qua';

GO

-- ============================================================
-- 3. BẢNG SAN YEU THICHS — bookmark sân
--    User bấm tim trên card sân → lưu vào đây
--    Xem lại trong tab "Sân yêu thích" ở Hồ sơ
-- ============================================================

IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE name = 'SanYeuThichs' AND type = 'U')
BEGIN
    CREATE TABLE SanYeuThichs (
        Id        INT      IDENTITY(1,1) PRIMARY KEY,
        UserId    INT      NOT NULL,
        SanBongId INT      NOT NULL,
        NgayThem  DATETIME NOT NULL DEFAULT GETDATE(),

        CONSTRAINT FK_SanYeuThich_User    FOREIGN KEY (UserId)    REFERENCES Users(Id)    ON DELETE CASCADE,
        CONSTRAINT FK_SanYeuThich_SanBong FOREIGN KEY (SanBongId) REFERENCES SanBongs(Id) ON DELETE CASCADE,

        -- Mỗi user chỉ bookmark 1 sân 1 lần
        CONSTRAINT UQ_SanYeuThich UNIQUE (UserId, SanBongId)
    );

    CREATE INDEX IX_SanYeuThich_UserId    ON SanYeuThichs (UserId);
    CREATE INDEX IX_SanYeuThich_SanBongId ON SanYeuThichs (SanBongId);

    PRINT N'✔ Bảng SanYeuThichs đã tạo';
END
ELSE
    PRINT N'⚠ Bảng SanYeuThichs đã tồn tại, bỏ qua';

GO

-- ============================================================
-- 4. BẢNG DIEM THUONG LOGS — lịch sử giao dịch điểm
--    Mỗi lần cộng/trừ điểm → ghi 1 dòng vào đây
--    Users.DiemHienTai là cache, nguồn thật là bảng này
-- ============================================================

IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE name = 'DiemThuongLogs' AND type = 'U')
BEGIN
    CREATE TABLE DiemThuongLogs (
        Id          INT            IDENTITY(1,1) PRIMARY KEY,
        UserId      INT            NOT NULL,
        -- Số điểm thay đổi: dương = cộng, âm = trừ
        SoDiem      INT            NOT NULL,
        -- Số dư sau giao dịch này (để hiển thị lịch sử dễ hơn)
        SoDuSauGD   INT            NOT NULL DEFAULT 0,
        -- Loại sự kiện sinh điểm
        LoaiSuKien  NVARCHAR(50)   NOT NULL,
        -- 'DatSan' | 'DanhGia' | 'DoiVoucher' | 'HoaHong' | 'Admin'
        GhiChu      NVARCHAR(200)  NULL,
        -- Liên kết tùy chọn đến đơn đặt sân (nếu điểm từ booking)
        DatSanId    INT            NULL,
        ThoiGian    DATETIME       NOT NULL DEFAULT GETDATE(),

        CONSTRAINT FK_DiemLog_User   FOREIGN KEY (UserId)   REFERENCES Users(Id),
        CONSTRAINT FK_DiemLog_DatSan FOREIGN KEY (DatSanId) REFERENCES DatSans(Id),
        CONSTRAINT CHK_DiemLog_LoaiSuKien CHECK (
            LoaiSuKien IN ('DatSan', 'DanhGia', 'DoiVoucher', 'HoaHong', 'Admin')
        )
    );

    CREATE INDEX IX_DiemLog_UserId   ON DiemThuongLogs (UserId);
    CREATE INDEX IX_DiemLog_ThoiGian ON DiemThuongLogs (ThoiGian DESC);

    PRINT N'✔ Bảng DiemThuongLogs đã tạo';
END
ELSE
    PRINT N'⚠ Bảng DiemThuongLogs đã tồn tại, bỏ qua';

GO

-- ============================================================
-- 5. BẢNG VOUCHERS — kho voucher hệ thống
--    Admin hoặc hệ thống tạo loại voucher
--    User dùng điểm đổi → sinh ra 1 bản ghi UserVouchers
-- ============================================================

IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE name = 'Vouchers' AND type = 'U')
BEGIN
    CREATE TABLE Vouchers (
        Id              INT            IDENTITY(1,1) PRIMARY KEY,
        MaVoucher       NVARCHAR(50)   NOT NULL UNIQUE,
        TenVoucher      NVARCHAR(200)  NOT NULL,
        MoTa            NVARCHAR(500)  NULL,
        -- Loại giảm giá
        LoaiGiam        NVARCHAR(20)   NOT NULL DEFAULT 'PhanTram',
        -- 'PhanTram' = giảm %, 'SoTien' = giảm tiền mặt
        GiaTriGiam      DECIMAL(18,2)  NOT NULL,
        -- Nếu LoaiGiam = 'PhanTram', đây là số tiền tối đa được giảm
        GiamToiDa       DECIMAL(18,2)  NULL,
        -- Điểm cần dùng để đổi (0 = admin tặng trực tiếp)
        DiemCanDoi      INT            NOT NULL DEFAULT 0,
        -- Số ngày hiệu lực kể từ ngày đổi
        SoNgayHieuLuc  INT            NOT NULL DEFAULT 30,
        IsActive        BIT            NOT NULL DEFAULT 1,
        NgayTao         DATETIME       NOT NULL DEFAULT GETDATE(),

        CONSTRAINT CHK_Vouchers_LoaiGiam CHECK (LoaiGiam IN ('PhanTram', 'SoTien')),
        CONSTRAINT CHK_Vouchers_GiaTriGiam CHECK (GiaTriGiam > 0)
    );

    PRINT N'✔ Bảng Vouchers đã tạo';
END
ELSE
    PRINT N'⚠ Bảng Vouchers đã tồn tại, bỏ qua';

GO

-- ============================================================
-- 6. BẢNG USER VOUCHERS — voucher user đang giữ
--    Mỗi lần user đổi điểm → tạo 1 bản ghi ở đây
--    Khi dùng khi đặt sân → IsUsed = 1
-- ============================================================

IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE name = 'UserVouchers' AND type = 'U')
BEGIN
    CREATE TABLE UserVouchers (
        Id          INT      IDENTITY(1,1) PRIMARY KEY,
        UserId      INT      NOT NULL,
        VoucherId   INT      NOT NULL,
        -- Mã duy nhất cho lần đổi này (user có thể đổi cùng loại nhiều lần)
        MaSuDung    NVARCHAR(50) NOT NULL UNIQUE,
        NgayDoi     DATETIME NOT NULL DEFAULT GETDATE(),
        NgayHetHan  DATETIME NOT NULL,
        IsUsed      BIT      NOT NULL DEFAULT 0,
        NgaySuDung  DATETIME NULL,
        -- Liên kết đơn đặt sân đã dùng voucher này (nếu có)
        DatSanId    INT      NULL,

        CONSTRAINT FK_UserVoucher_User    FOREIGN KEY (UserId)    REFERENCES Users(Id),
        CONSTRAINT FK_UserVoucher_Voucher FOREIGN KEY (VoucherId)  REFERENCES Vouchers(Id),
        CONSTRAINT FK_UserVoucher_DatSan  FOREIGN KEY (DatSanId)  REFERENCES DatSans(Id)
    );

    CREATE INDEX IX_UserVoucher_UserId   ON UserVouchers (UserId);
    CREATE INDEX IX_UserVoucher_IsUsed   ON UserVouchers (IsUsed);
    CREATE INDEX IX_UserVoucher_HetHan   ON UserVouchers (NgayHetHan);

    PRINT N'✔ Bảng UserVouchers đã tạo';
END
ELSE
    PRINT N'⚠ Bảng UserVouchers đã tồn tại, bỏ qua';

GO

-- ============================================================
-- 7. BẢNG OTP CODES — xác thực số điện thoại lần đầu đặt sân
--    Sinh OTP 6 số → lưu vào đây → user nhập → verify
--    Hết hạn sau 5 phút, IsUsed = 1 sau khi dùng
-- ============================================================

IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE name = 'OtpCodes' AND type = 'U')
BEGIN
    CREATE TABLE OtpCodes (
        Id            INT          IDENTITY(1,1) PRIMARY KEY,
        UserId        INT          NOT NULL,
        SoDienThoai   NVARCHAR(20) NOT NULL,
        MaOtp         NVARCHAR(10) NOT NULL,
        NgayTao       DATETIME     NOT NULL DEFAULT GETDATE(),
        -- Hết hạn sau 5 phút
        NgayHetHan    DATETIME     NOT NULL DEFAULT DATEADD(MINUTE, 5, GETDATE()),
        IsUsed        BIT          NOT NULL DEFAULT 0,

        CONSTRAINT FK_OtpCodes_User FOREIGN KEY (UserId) REFERENCES Users(Id)
    );

    CREATE INDEX IX_OtpCodes_UserId    ON OtpCodes (UserId);
    CREATE INDEX IX_OtpCodes_HetHan    ON OtpCodes (NgayHetHan);

    PRINT N'✔ Bảng OtpCodes đã tạo';
END
ELSE
    PRINT N'⚠ Bảng OtpCodes đã tồn tại, bỏ qua';

GO

-- ============================================================
-- SEED DATA — Voucher mẫu để test
-- ============================================================

IF NOT EXISTS (SELECT 1 FROM Vouchers WHERE MaVoucher = 'WELCOME10')
BEGIN
    INSERT INTO Vouchers (MaVoucher, TenVoucher, MoTa, LoaiGiam, GiaTriGiam, GiamToiDa, DiemCanDoi, SoNgayHieuLuc)
    VALUES
    (
        'WELCOME10',
        N'Giảm 10% tiền cọc',
        N'Voucher chào mừng — đổi 100 điểm được giảm 10% tiền cọc lần đặt tiếp theo (tối đa 50,000đ)',
        'PhanTram', 10.00, 50000, 100, 30
    ),
    (
        'SAVE50K',
        N'Giảm 50,000đ tiền cọc',
        N'Voucher tiết kiệm — đổi 200 điểm được giảm thẳng 50,000đ tiền cọc',
        'SoTien', 50000, NULL, 200, 30
    ),
    (
        'SAVE100K',
        N'Giảm 100,000đ tiền cọc',
        N'Voucher cao cấp — đổi 350 điểm được giảm thẳng 100,000đ tiền cọc',
        'SoTien', 100000, NULL, 350, 45
    );

    PRINT N'✔ Seed: 3 Vouchers mẫu đã thêm';
END
ELSE
    PRINT N'⚠ Seed Vouchers đã có, bỏ qua';

GO

-- ============================================================
-- KIỂM TRA KẾT QUẢ MIGRATION
-- ============================================================

PRINT N'';
PRINT N'=== KIỂM TRA SAU MIGRATION v5 ===';

SELECT
    'Users.DaXacThucSDT'  AS Cot, 
    CASE WHEN EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('Users') AND name = 'DaXacThucSDT')
         THEN N'✅ Đã có' ELSE N'❌ Thiếu' END AS TrangThai
UNION ALL SELECT 'Users.DiemHienTai',
    CASE WHEN EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('Users') AND name = 'DiemHienTai')
         THEN N'✅ Đã có' ELSE N'❌ Thiếu' END
UNION ALL SELECT 'DanhGias.SoSaoCoSoVatChat',
    CASE WHEN EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('DanhGias') AND name = 'SoSaoCoSoVatChat')
         THEN N'✅ Đã có' ELSE N'❌ Thiếu' END
UNION ALL SELECT 'DanhGias.SoSaoNhanVien',
    CASE WHEN EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('DanhGias') AND name = 'SoSaoNhanVien')
         THEN N'✅ Đã có' ELSE N'❌ Thiếu' END
UNION ALL SELECT 'SanYeuThichs',
    CASE WHEN OBJECT_ID('SanYeuThichs','U') IS NOT NULL
         THEN N'✅ Đã có' ELSE N'❌ Thiếu' END
UNION ALL SELECT 'DiemThuongLogs',
    CASE WHEN OBJECT_ID('DiemThuongLogs','U') IS NOT NULL
         THEN N'✅ Đã có' ELSE N'❌ Thiếu' END
UNION ALL SELECT 'Vouchers',
    CASE WHEN OBJECT_ID('Vouchers','U') IS NOT NULL
         THEN N'✅ Đã có' ELSE N'❌ Thiếu' END
UNION ALL SELECT 'UserVouchers',
    CASE WHEN OBJECT_ID('UserVouchers','U') IS NOT NULL
         THEN N'✅ Đã có' ELSE N'❌ Thiếu' END
UNION ALL SELECT 'OtpCodes',
    CASE WHEN OBJECT_ID('OtpCodes','U') IS NOT NULL
         THEN N'✅ Đã có' ELSE N'❌ Thiếu' END;

SELECT [Bang] = name, [So ban ghi] = SUM(1)
FROM sys.tables
WHERE name IN (
    'SanYeuThichs','DiemThuongLogs','Vouchers','UserVouchers','OtpCodes',
    'Users','DanhGias','DatSans','SanBongs','AuditLogs','KhieuNais','Matchmakings'
)
GROUP BY name
ORDER BY name;

PRINT N'';
PRINT N'========================================================';
PRINT N'  ✅ Migration v5 hoàn thành!';
PRINT N'';
PRINT N'  Bước tiếp theo:';
PRINT N'  1. Cập nhật Models C# tương ứng với schema mới';
PRINT N'  2. Build /User/HoSo — Controller + View';
PRINT N'  3. Build Rating UI trên Venues/Details';
PRINT N'========================================================';
GO

--Ngày 22/05/2026
-- ================================================================
-- PITCHHUB — TOURNAMENT TABLES
-- Chạy sau khi đã có DB SanBongBTL
-- ================================================================

USE SanBongBTL;
GO

-- ── 1. GiaiDaus ─────────────────────────────────────────────────
CREATE TABLE GiaiDaus (
    Id                    INT             IDENTITY(1,1) PRIMARY KEY,
    TenGiai               NVARCHAR(200)   NOT NULL,
    MoTa                  NVARCHAR(2000)  NULL,
    SanBongId             INT             NOT NULL,
    OwnerId               INT             NOT NULL,
    SoDoiToiDa            INT             NOT NULL DEFAULT 8,
    SoBang                INT             NOT NULL DEFAULT 2,
    LePhiGiai             DECIMAL(18,2)   NOT NULL DEFAULT 0,
    TienKyQuy             DECIMAL(18,2)   NOT NULL DEFAULT 0,
    TienPhatTheVang       DECIMAL(18,2)   NOT NULL DEFAULT 20000,
    TienPhatTheDo         DECIMAL(18,2)   NOT NULL DEFAULT 100000,
    SoTranTreoGioTheDo    INT             NOT NULL DEFAULT 1,
    SoTheVangTichLuy      INT             NOT NULL DEFAULT 2,
    NgayBatDau            DATETIME        NOT NULL,
    NgayKetThuc           DATETIME        NOT NULL,
    ThoiGianTao           DATETIME        NOT NULL DEFAULT GETDATE(),
    ThoiGianDongDanhSach  DATETIME        NULL,
    -- Draft→Approved→RegistrationOpen→RegistrationClosed→Active→Finished
    TrangThai             NVARCHAR(30)    NOT NULL DEFAULT 'Draft',

    CONSTRAINT FK_GiaiDaus_SanBong FOREIGN KEY (SanBongId) REFERENCES SanBongs(Id),
    CONSTRAINT FK_GiaiDaus_Owner   FOREIGN KEY (OwnerId)   REFERENCES Users(Id),
    CONSTRAINT CHK_GiaiDaus_State  CHECK (TrangThai IN (
        'Draft','Approved','RegistrationOpen',
        'RegistrationClosed','Active','Finished'
    ))
);
GO

-- ── 2. BangDaus ──────────────────────────────────────────────────
-- Bảng A, Bảng B... trong giải
CREATE TABLE BangDaus (
    Id        INT           IDENTITY(1,1) PRIMARY KEY,
    GiaiDauId INT           NOT NULL,
    TenBang   NVARCHAR(10)  NOT NULL,

    CONSTRAINT FK_BangDaus_GiaiDau FOREIGN KEY (GiaiDauId)
        REFERENCES GiaiDaus(Id) ON DELETE CASCADE
);
GO

-- ── 3. DoiBongs ──────────────────────────────────────────────────
CREATE TABLE DoiBongs (
    Id                INT             IDENTITY(1,1) PRIMARY KEY,
    GiaiDauId         INT             NOT NULL,
    BangId            INT             NULL,       -- null trước khi chia bảng
    DoiTruongId       INT             NOT NULL,
    TenDoi            NVARCHAR(100)   NOT NULL,
    LogoUrl           NVARCHAR(500)   NULL,
    DaThanhToan       BIT             NOT NULL DEFAULT 0,
    ThoiGianThanhToan DATETIME        NULL,
    TrangThai         NVARCHAR(20)    NOT NULL DEFAULT 'Active',
    ThoiGianTao       DATETIME        NOT NULL DEFAULT GETDATE(),

    CONSTRAINT FK_DoiBongs_GiaiDau   FOREIGN KEY (GiaiDauId)
        REFERENCES GiaiDaus(Id) ON DELETE CASCADE,
    CONSTRAINT FK_DoiBongs_BangDau   FOREIGN KEY (BangId)
        REFERENCES BangDaus(Id),
    CONSTRAINT FK_DoiBongs_DoiTruong FOREIGN KEY (DoiTruongId)
        REFERENCES Users(Id)
);
GO

-- ── 4. ThanhVienDois ─────────────────────────────────────────────
CREATE TABLE ThanhVienDois (
    Id            INT           IDENTITY(1,1) PRIMARY KEY,
    DoiId         INT           NOT NULL,
    HoTen         NVARCHAR(100) NOT NULL,
    SoAo          INT           NOT NULL,
    AnhDaiDien    NVARCHAR(500) NULL,
    SoTranTreoGio INT           NOT NULL DEFAULT 0,
    TongBanThang  INT           NOT NULL DEFAULT 0,
    TongTheVang   INT           NOT NULL DEFAULT 0,
    TongTheDo     INT           NOT NULL DEFAULT 0,

    CONSTRAINT FK_ThanhVienDois_Doi FOREIGN KEY (DoiId)
        REFERENCES DoiBongs(Id) ON DELETE CASCADE,
    CONSTRAINT UQ_ThanhVienDoi_SoAo UNIQUE (DoiId, SoAo)
);
GO

-- ── 5. TranDaus ──────────────────────────────────────────────────
CREATE TABLE TranDaus (
    Id              INT          IDENTITY(1,1) PRIMARY KEY,
    GiaiDauId       INT          NOT NULL,
    BangId          INT          NULL,   -- null nếu knock-out
    KhungGioId      INT          NULL,   -- Owner gán sau
    DoiNhaId        INT          NOT NULL DEFAULT 0,  -- 0 = TBD
    DoiKhachId      INT          NOT NULL DEFAULT 0,
    BanThangNha     INT          NULL,
    BanThangKhach   INT          NULL,
    VongDau         INT          NOT NULL,
    LoaiVong        NVARCHAR(20) NOT NULL DEFAULT 'VongBang',
    NgayThiDau      DATETIME     NOT NULL,
    TrangThai       NVARCHAR(20) NOT NULL DEFAULT 'Scheduled',
    StaffPhuTrachId INT          NULL,

    CONSTRAINT FK_TranDaus_GiaiDau FOREIGN KEY (GiaiDauId)
        REFERENCES GiaiDaus(Id) ON DELETE CASCADE,
    CONSTRAINT FK_TranDaus_BangDau FOREIGN KEY (BangId)
        REFERENCES BangDaus(Id),
    CONSTRAINT FK_TranDaus_KhungGio FOREIGN KEY (KhungGioId)
        REFERENCES KhungGios(Id),
    CONSTRAINT FK_TranDaus_Staff FOREIGN KEY (StaffPhuTrachId)
        REFERENCES Users(Id),
    CONSTRAINT CHK_TranDaus_LoaiVong CHECK (LoaiVong IN (
        'VongBang','TuKet','BanKet','ChungKet'
    )),
    CONSTRAINT CHK_TranDaus_State CHECK (TrangThai IN (
        'Scheduled','Pending','InProgress','Closed','Walkover'
    ))
);
GO

-- ── 6. SuKienTrans ───────────────────────────────────────────────
-- Bàn thắng, thẻ phạt của từng trận
CREATE TABLE SuKienTrans (
    Id           INT           IDENTITY(1,1) PRIMARY KEY,
    TranDauId    INT           NOT NULL,
    ThanhVienId  INT           NULL,
    DoiId        INT           NULL,
    LoaiSuKien   NVARCHAR(20)  NOT NULL,
    Phut         INT           NULL,
    GhiChu       NVARCHAR(500) NULL,
    ThoiGianGhi  DATETIME      NOT NULL DEFAULT GETDATE(),

    CONSTRAINT FK_SuKienTrans_TranDau   FOREIGN KEY (TranDauId)
        REFERENCES TranDaus(Id) ON DELETE CASCADE,
    CONSTRAINT FK_SuKienTrans_ThanhVien FOREIGN KEY (ThanhVienId)
        REFERENCES ThanhVienDois(Id),
    CONSTRAINT FK_SuKienTrans_Doi       FOREIGN KEY (DoiId)
        REFERENCES DoiBongs(Id),
    CONSTRAINT CHK_SuKienTrans_Loai CHECK (LoaiSuKien IN (
        'BanThang','TheVang','TheDo','TheVangLan2','SuCo'
    ))
);
GO

-- ================================================================
-- SỬA constraint KhungGios — thêm 'KhoaChoGiai'
-- Khi Owner đặt sân cho trận giải → slot hiện màu đỏ cho khách vãng lai
-- ================================================================
ALTER TABLE KhungGios DROP CONSTRAINT CHK_KhungGios_TrangThai;
GO
ALTER TABLE KhungGios ADD CONSTRAINT CHK_KhungGios_TrangThai
    CHECK (TrangThai IN ('Trong','DaDat','DangGiu','KhoaChoGiai'));
GO

-- ================================================================
-- KIỂM TRA
-- ================================================================
SELECT t.name AS TenBang, p.rows AS SoHang
FROM sys.tables t
JOIN sys.indexes i ON t.object_id = i.object_id AND i.type <= 1
JOIN sys.partitions p ON i.object_id = p.object_id
    AND i.index_id = p.index_id
WHERE t.name IN (
    'GiaiDaus','BangDaus','DoiBongs','ThanhVienDois',
    'TranDaus','SuKienTrans'
)
ORDER BY t.name;


-- ================================================================
-- PITCHHUB — Thêm thông tin ngân hàng cho Owner
-- Dùng trong màn hình Checkout để User biết chuyển khoản về đâu
-- Chạy file này SAU AddTournamentTables.sql
-- ================================================================

-- ── Thêm 3 cột vào bảng Users ───────────────────────────────────
-- Chỉ Owner cần điền, User/Staff/Admin để NULL

IF NOT EXISTS (
    SELECT 1 FROM sys.columns
    WHERE object_id = OBJECT_ID('Users') AND name = 'NganHang'
)
BEGIN
    ALTER TABLE Users ADD NganHang NVARCHAR(50) NULL;
    PRINT N'✔ Users.NganHang đã thêm';
END
ELSE
    PRINT N'⚠ Users.NganHang đã tồn tại, bỏ qua';
GO

IF NOT EXISTS (
    SELECT 1 FROM sys.columns
    WHERE object_id = OBJECT_ID('Users') AND name = 'SoTaiKhoan'
)
BEGIN
    ALTER TABLE Users ADD SoTaiKhoan NVARCHAR(30) NULL;
    PRINT N'✔ Users.SoTaiKhoan đã thêm';
END
ELSE
    PRINT N'⚠ Users.SoTaiKhoan đã tồn tại, bỏ qua';
GO

IF NOT EXISTS (
    SELECT 1 FROM sys.columns
    WHERE object_id = OBJECT_ID('Users') AND name = 'TenTaiKhoan'
)
BEGIN
    ALTER TABLE Users ADD TenTaiKhoan NVARCHAR(100) NULL;
    PRINT N'✔ Users.TenTaiKhoan đã thêm';
END
ELSE
    PRINT N'⚠ Users.TenTaiKhoan đã tồn tại, bỏ qua';
GO

-- ── Kiểm tra kết quả ────────────────────────────────────────────
SELECT
    c.name          AS CotMoi,
    t.name          AS KieuDuLieu,
    c.max_length    AS DoDai,
    c.is_nullable   AS ChoPhepNull
FROM sys.columns c
JOIN sys.types   t ON c.user_type_id = t.user_type_id
WHERE c.object_id = OBJECT_ID('Users')
  AND c.name IN ('NganHang', 'SoTaiKhoan', 'TenTaiKhoan')
ORDER BY c.column_id;
GO

-- ================================================================
-- SAU KHI CHẠY SQL → Scaffold lại User model:
--
-- Scaffold-DbContext "Server=TunKittt;Database=SanBongBTL;
-- User Id=sa;Password=422005;TrustServerCertificate=True;"
-- Microsoft.EntityFrameworkCore.SqlServer
-- -OutputDir EFCore -Force -Context SanBongContext
-- -Tables Users
--
-- Hoặc thêm tay vào class User trong EFCore/User.cs:
-- public string? NganHang    { get; set; }
-- public string? SoTaiKhoan  { get; set; }
-- public string? TenTaiKhoan { get; set; }
-- ================================================================

-- 23/05/2026
-- Fix: thêm cột TienKyQuyConLai vào DoiBongs nếu chưa có
USE SanBongBTL
GO

IF NOT EXISTS (
    SELECT 1 FROM sys.columns
    WHERE object_id = OBJECT_ID('DoiBongs') AND name = 'TienKyQuyConLai'
)
BEGIN
    ALTER TABLE DoiBongs ADD TienKyQuyConLai DECIMAL(18,2) NOT NULL DEFAULT 0;
    PRINT N'✔ DoiBongs.TienKyQuyConLai added';
END
GO

--Nới giới hạn cọc xuống thấp để phù hợp 
ALTER TABLE SanBongs DROP CONSTRAINT CHK_SanBongs_TyLeCoc;
ALTER TABLE SanBongs ADD CONSTRAINT CHK_SanBongs_TyLeCoc 
    CHECK (TyLeCoc BETWEEN 0.05 AND 0.70);
GO

--Nạp dữ liệu vào thêm để cho nó đẹp 
-- ================================================================
-- SEED DATA BỔ SUNG: 30 sân bóng tại Hà Nội
-- Chạy SAU đoạn seed gốc (cần @idOwner đã được khai báo)
-- ================================================================

-- Fix constraint trước
ALTER TABLE SanBongs DROP CONSTRAINT CHK_SanBongs_TyLeCoc;
ALTER TABLE SanBongs ADD CONSTRAINT CHK_SanBongs_TyLeCoc 
    CHECK (TyLeCoc BETWEEN 0.05 AND 0.70);
GO

DECLARE @idOwner INT;
SELECT @idOwner = Id FROM Users WHERE Email = 'owner1@gmail.com';


INSERT INTO SanBongs (TenSan, DiaChi, Quan, ThanhPho, LoaiSan, LoaiCo,
                      MoTa, DanhGiaTrungBinh, Latitude, Longitude,
                      TrangThaiDuyet, TyLeCoc, IsHidden, OwnerId)
VALUES
-- Nội ô trung tâm 10%: Đống Đa, Ba Đình, Hai Bà Trưng
(N'Sân Bóng Đống Đa Champion',     N'67 Ô Chợ Dừa',           N'Đống Đa',      N'Hà Nội', '5',  'Nhan tao', N'Gần phố cổ, đặt sân dễ dàng qua app, hỗ trợ thanh toán QR.',       4.0, 21.0289, 105.8401, 'DaDuyet', 0.10, 0, @idOwner),
(N'Sân Bóng Ba Đình Sport Center', N'9 Hoàng Diệu',           N'Ba Đình',      N'Hà Nội', '11', 'Nhan tao', N'Sân 11 người trung tâm Ba Đình, phù hợp giải đấu lớn.',             4.4, 21.0412, 105.8367, 'DaDuyet', 0.10, 0, @idOwner),
(N'Sân Bóng Ba Đình Arena',        N'78 Kim Mã',              N'Ba Đình',      N'Hà Nội', '7',  'Nhan tao', N'Cạnh các khách sạn lớn, tiện cho khách ngoại tỉnh.',                4.3, 21.0334, 105.8445, 'DaDuyet', 0.10, 0, @idOwner),
(N'Sân Bóng Hai Bà Trưng Arena',   N'78 Minh Khai',           N'Hai Bà Trưng', N'Hà Nội', '7',  'Nhan tao', N'Mái che toàn bộ, chơi được 365 ngày không lo thời tiết.',           4.5, 21.0089, 105.8645, 'DaDuyet', 0.10, 0, @idOwner),
(N'Sân Bóng Hai Bà Trưng Premier', N'45 Trương Định',         N'Hai Bà Trưng', N'Hà Nội', '11', 'Nhan tao', N'Sân 11 người lớn nhất quận, có phòng VIP và dịch vụ ăn uống.',     4.7, 21.0045, 105.8712, 'DaDuyet', 0.10, 0, @idOwner),

-- Nội ô mở rộng 8%: Cầu Giấy, Bắc Từ Liêm, Tây Hồ, Thanh Xuân
(N'Sân Bóng Cầu Giấy Champion',    N'12 Mai Dịch',            N'Cầu Giấy',     N'Hà Nội', '7',  'Tu nhien', N'Sân cỏ tự nhiên hiếm có khu vực nội thành, không khí thoáng mát.', 4.1, 21.0423, 105.7634, 'DaDuyet', 0.08, 0, @idOwner),
(N'Sân Bóng Bắc Từ Liêm Green',    N'34 Phạm Văn Đồng',       N'Bắc Từ Liêm',  N'Hà Nội', '7',  'Tu nhien', N'Cỏ tự nhiên thoáng mát, phù hợp đội bóng phong trào.',             3.9, 21.0712, 105.7534, 'DaDuyet', 0.08, 0, @idOwner),
(N'Sân Bóng Tây Hồ Arena',         N'88 Xuân Diệu',           N'Tây Hồ',       N'Hà Nội', '7',  'Nhan tao', N'View hồ Tây cực đẹp, mặt sân phẳng chuẩn thi đấu.',               4.3, 21.0623, 105.8312, 'DaDuyet', 0.08, 0, @idOwner),
(N'Sân Bóng Tây Hồ Premium',       N'23 Thụy Khuê',           N'Tây Hồ',       N'Hà Nội', '5',  'Nhan tao', N'Sân VIP khu Tây Hồ, dịch vụ 5 sao, có phòng xông hơi.',           4.8, 21.0589, 105.8234, 'DaDuyet', 0.08, 0, @idOwner),
(N'Sân Bóng Thanh Xuân Complex',   N'103 Nguyễn Trãi',        N'Thanh Xuân',   N'Hà Nội', '5',  'Nhan tao', N'Khu phức hợp thể thao, có căng tin và phòng thay đồ sạch sẽ.',    4.5, 20.9956, 105.8123, 'DaDuyet', 0.08, 0, @idOwner),

-- Vùng ngoại ô 6%: Hoàng Mai, Long Biên, Nam Từ Liêm
(N'Sân Bóng Hoàng Mai Premier',    N'12 Tam Trinh',           N'Hoàng Mai',    N'Hà Nội', '5',  'Nhan tao', N'Sân hiện đại khu Hoàng Mai, đèn chiếu sáng ban đêm cực tốt.',     4.2, 20.9823, 105.8634, 'DaDuyet', 0.06, 0, @idOwner),
(N'Sân Bóng Hoàng Mai Elite',      N'15 Yên Duyên',           N'Hoàng Mai',    N'Hà Nội', '11', 'Nhan tao', N'Sân 11 người tiêu chuẩn, thường xuyên tổ chức giải đấu phủi.',   4.6, 20.9934, 105.8556, 'DaDuyet', 0.06, 0, @idOwner),
(N'Sân Bóng Long Biên United',     N'67 Ngô Gia Tự',          N'Long Biên',    N'Hà Nội', '11', 'Nhan tao', N'Sân 11 người chuẩn FIFA, có khán đài nhỏ cho cổ động viên.',      4.7, 21.0534, 105.9012, 'DaDuyet', 0.06, 0, @idOwner),
(N'Sân Bóng Nam Từ Liêm Elite',    N'18 Trung Văn',           N'Nam Từ Liêm',  N'Hà Nội', '5',  'Nhan tao', N'Khu đô thị mới, sân đẹp hiện đại, dịch vụ đầy đủ.',              4.5, 21.0034, 105.7812, 'DaDuyet', 0.06, 0, @idOwner),
(N'Sân Bóng Nam Từ Liêm Sport',    N'56 Đại Mỗ',              N'Nam Từ Liêm',  N'Hà Nội', '5',  'Nhan tao', N'Sân gia đình, giờ mở cửa từ 5 giờ sáng đến 11 giờ đêm.',         3.8, 21.0067, 105.7534, 'DaDuyet', 0.06, 0, @idOwner);

PRINT N'✔ Đã thêm sân bóng bổ sung';

DECLARE @idOwner INT;
SELECT @idOwner = Id FROM Users WHERE Email = 'owner1@gmail.com';

UPDATE SanBongs 
SET OwnerId = @idOwner
WHERE OwnerId IS NULL;

PRINT N'✔ Đã cập nhật OwnerId cho các sân bị NULL';