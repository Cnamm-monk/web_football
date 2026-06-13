-- ============================================================
--  PitchHub.vn — SEED DEMO DATA
--  8 người dùng ảo + 2 đội + 1 giải đấu mẫu
--  Chạy SAU khi đã chạy SanBongBTL.sql
-- ============================================================

USE SanBongBTL;
GO

-- ============================================================
-- BƯỚC 1: THÊM 8 NGƯỜI DÙNG ẢO
-- Mật khẩu đều là: User@123
-- Hash BCrypt của "User@123"
-- ============================================================

DECLARE @u1 INT, @u2 INT, @u3 INT, @u4 INT,
        @u5 INT, @u6 INT, @u7 INT, @u8 INT;

-- Lấy OwnerId đã có sẵn trong DB
DECLARE @ownerId INT;
SELECT TOP 1 @ownerId = Id FROM Users WHERE VaiTro = 'Owner';

-- Lấy SanBongId đã có sẵn (DaDuyet)
DECLARE @sanId INT;
SELECT TOP 1 @sanId = Id FROM SanBongs WHERE TrangThaiDuyet = 'DaDuyet';

IF NOT EXISTS (SELECT 1 FROM Users WHERE Email = 'nguyenvanan@demo.vn')
BEGIN
    INSERT INTO Users (HoTen, Email, MatKhau, SoDienThoai, VaiTro, IsActive, DiemHienTai, DaXacThucSdt)
    VALUES
    (N'Nguyễn Văn An',    'nguyenvanan@demo.vn',   '$2a$11$wmJXgVs5/RBXU2y/vP4Bs.UcgtzPg0r4Iv2t3DgsZvDrOKD32vPzO', '0901111001', 'User', 1, 50, 1),
    (N'Trần Minh Khoa',   'tranminhkhoa@demo.vn',  '$2a$11$wmJXgVs5/RBXU2y/vP4Bs.UcgtzPg0r4Iv2t3DgsZvDrOKD32vPzO', '0901111002', 'User', 1, 30, 1),
    (N'Lê Hoàng Nam',     'lehoangnam@demo.vn',    '$2a$11$wmJXgVs5/RBXU2y/vP4Bs.UcgtzPg0r4Iv2t3DgsZvDrOKD32vPzO', '0901111003', 'User', 1, 20, 1),
    (N'Phạm Quốc Hùng',   'phamquochung@demo.vn',  '$2a$11$wmJXgVs5/RBXU2y/vP4Bs.UcgtzPg0r4Iv2t3DgsZvDrOKD32vPzO', '0901111004', 'User', 1, 10, 1),
    (N'Đỗ Thành Đạt',     'dothanhddat@demo.vn',   '$2a$11$wmJXgVs5/RBXU2y/vP4Bs.UcgtzPg0r4Iv2t3DgsZvDrOKD32vPzO', '0901111005', 'User', 1, 0,  1),
    (N'Vũ Tiến Dũng',     'vutiendung@demo.vn',    '$2a$11$wmJXgVs5/RBXU2y/vP4Bs.UcgtzPg0r4Iv2t3DgsZvDrOKD32vPzO', '0901111006', 'User', 1, 0,  1),
    (N'Hoàng Đức Anh',    'hoangducanh@demo.vn',   '$2a$11$wmJXgVs5/RBXU2y/vP4Bs.UcgtzPg0r4Iv2t3DgsZvDrOKD32vPzO', '0901111007', 'User', 1, 0,  1),
    (N'Bùi Văn Thắng',    'buivanthang@demo.vn',   '$2a$11$wmJXgVs5/RBXU2y/vP4Bs.UcgtzPg0r4Iv2t3DgsZvDrOKD32vPzO', '0901111008', 'User', 1, 0,  1);

    PRINT N'✔ Đã thêm 8 người dùng demo';
END
ELSE
    PRINT N'⚠ Users demo đã tồn tại, bỏ qua';

-- Lấy Id các user vừa tạo
SELECT @u1 = Id FROM Users WHERE Email = 'nguyenvanan@demo.vn';
SELECT @u2 = Id FROM Users WHERE Email = 'tranminhkhoa@demo.vn';
SELECT @u3 = Id FROM Users WHERE Email = 'lehoangnam@demo.vn';
SELECT @u4 = Id FROM Users WHERE Email = 'phamquochung@demo.vn';
SELECT @u5 = Id FROM Users WHERE Email = 'dothanhddat@demo.vn';
SELECT @u6 = Id FROM Users WHERE Email = 'vutiendung@demo.vn';
SELECT @u7 = Id FROM Users WHERE Email = 'hoangducanh@demo.vn';
SELECT @u8 = Id FROM Users WHERE Email = 'buivanthang@demo.vn';

PRINT CONCAT(N'IDs: ', @u1,' ',@u2,' ',@u3,' ',@u4,' ',@u5,' ',@u6,' ',@u7,' ',@u8);

GO

-- ============================================================
-- BƯỚC 2: TẠO GIẢI ĐẤU MẪU
-- TrangThai = 'Approved' (đã qua Admin duyệt, sẵn sàng mở đăng ký)
-- ============================================================

DECLARE @ownerId INT, @sanId INT, @giaiId INT;
SELECT TOP 1 @ownerId = Id FROM Users WHERE VaiTro = 'Owner';
SELECT TOP 1 @sanId   = Id FROM SanBongs WHERE TrangThaiDuyet = 'DaDuyet';

IF NOT EXISTS (SELECT 1 FROM GiaiDaus WHERE TenGiai = N'Giải Bóng Đá PitchHub Mùa Hè 2026')
BEGIN
    INSERT INTO GiaiDaus (
        TenGiai, MoTa, SanBongId, OwnerId,
        SoDoiToiDa, SoBang,
        LePhiGiai, TienKyQuy,
        TienPhatTheVang, TienPhatTheDo,
        SoTranTreoGioTheDo, SoTheVangTichLuy,
        NgayBatDau, NgayKetThuc,
        ThoiGianTao, TrangThai
    )
    VALUES (
        N'Giải Bóng Đá PitchHub Mùa Hè 2026',
        N'Giải đấu hè dành cho các đội bóng phong trào. Thi đấu vòng bảng + knockout.',
        @sanId, @ownerId,
        4, 2,                          -- 4 đội tối đa, 2 bảng
        500000, 1000000,               -- lệ phí 500k, ký quỹ 1 triệu
        20000, 100000,                 -- phạt thẻ vàng 20k, đỏ 100k
        1, 3,                          -- thẻ đỏ treo 1 trận, 3 thẻ vàng tích lũy
        DATEADD(DAY, 7,  GETDATE()),   -- bắt đầu sau 7 ngày
        DATEADD(DAY, 30, GETDATE()),   -- kết thúc sau 30 ngày
        GETDATE(), 'Approved'
    );
    SET @giaiId = SCOPE_IDENTITY();
    PRINT CONCAT(N'✔ Tạo giải đấu Id=', @giaiId);

    -- Tạo 2 bảng: Bảng A và Bảng B
    DECLARE @bangA INT, @bangB INT;

    INSERT INTO BangDaus (GiaiDauId, TenBang) VALUES (@giaiId, N'Bảng A');
    SET @bangA = SCOPE_IDENTITY();

    INSERT INTO BangDaus (GiaiDauId, TenBang) VALUES (@giaiId, N'Bảng B');
    SET @bangB = SCOPE_IDENTITY();

    PRINT CONCAT(N'✔ Tạo 2 bảng: A=', @bangA, ' B=', @bangB);

    -- Lấy Id 4 user demo
    DECLARE @u1 INT, @u2 INT, @u3 INT, @u4 INT;
    SELECT @u1 = Id FROM Users WHERE Email = 'nguyenvanan@demo.vn';
    SELECT @u2 = Id FROM Users WHERE Email = 'tranminhkhoa@demo.vn';
    SELECT @u3 = Id FROM Users WHERE Email = 'lehoangnam@demo.vn';
    SELECT @u4 = Id FROM Users WHERE Email = 'phamquochung@demo.vn';

    -- Tạo 4 đội đã đăng ký và thanh toán
    DECLARE @doi1 INT, @doi2 INT, @doi3 INT, @doi4 INT;

    INSERT INTO DoiBongs (GiaiDauId, BangId, DoiTruongId, TenDoi, TienKyQuyConLai, DaThanhToan, ThoiGianThanhToan, TrangThai, ThoiGianTao)
    VALUES (@giaiId, @bangA, @u1, N'FC Thăng Long',  1000000, 1, GETDATE(), 'Active', GETDATE());
    SET @doi1 = SCOPE_IDENTITY();

    INSERT INTO DoiBongs (GiaiDauId, BangId, DoiTruongId, TenDoi, TienKyQuyConLai, DaThanhToan, ThoiGianThanhToan, TrangThai, ThoiGianTao)
    VALUES (@giaiId, @bangA, @u2, N'Sông Hồng United', 1000000, 1, GETDATE(), 'Active', GETDATE());
    SET @doi2 = SCOPE_IDENTITY();

    INSERT INTO DoiBongs (GiaiDauId, BangId, DoiTruongId, TenDoi, TienKyQuyConLai, DaThanhToan, ThoiGianThanhToan, TrangThai, ThoiGianTao)
    VALUES (@giaiId, @bangB, @u3, N'Hà Nội Warriors',  1000000, 1, GETDATE(), 'Active', GETDATE());
    SET @doi3 = SCOPE_IDENTITY();

    INSERT INTO DoiBongs (GiaiDauId, BangId, DoiTruongId, TenDoi, TienKyQuyConLai, DaThanhToan, ThoiGianThanhToan, TrangThai, ThoiGianTao)
    VALUES (@giaiId, @bangB, @u4, N'Long Biên Stars',   1000000, 1, GETDATE(), 'Active', GETDATE());
    SET @doi4 = SCOPE_IDENTITY();

    PRINT CONCAT(N'✔ Tạo 4 đội: ', @doi1,' ',@doi2,' ',@doi3,' ',@doi4);

    -- Thêm thành viên cho mỗi đội (5 người/đội)
    -- Đội 1: FC Thăng Long
    INSERT INTO ThanhVienDois (DoiId, HoTen, SoAo) VALUES
        (@doi1, N'Nguyễn Văn An',   10),
        (@doi1, N'Lê Văn Bình',      7),
        (@doi1, N'Trần Văn Cường',   9),
        (@doi1, N'Phạm Văn Dũng',    1),
        (@doi1, N'Hoàng Văn Em',    11);

    -- Đội 2: Sông Hồng United
    INSERT INTO ThanhVienDois (DoiId, HoTen, SoAo) VALUES
        (@doi2, N'Trần Minh Khoa',  10),
        (@doi2, N'Vũ Văn Phong',     8),
        (@doi2, N'Đặng Văn Quân',    6),
        (@doi2, N'Bùi Văn Rạng',     1),
        (@doi2, N'Đinh Văn Sơn',    11);

    -- Đội 3: Hà Nội Warriors
    INSERT INTO ThanhVienDois (DoiId, HoTen, SoAo) VALUES
        (@doi3, N'Lê Hoàng Nam',    10),
        (@doi3, N'Ngô Văn Thái',     5),
        (@doi3, N'Dương Văn Uy',     4),
        (@doi3, N'Lý Văn Vinh',      1),
        (@doi3, N'Mai Văn Xuân',    11);

    -- Đội 4: Long Biên Stars
    INSERT INTO ThanhVienDois (DoiId, HoTen, SoAo) VALUES
        (@doi4, N'Phạm Quốc Hùng',  10),
        (@doi4, N'Cao Văn Yên',      3),
        (@doi4, N'Đinh Văn Zũng',    2),
        (@doi4, N'Hồ Văn An',        1),
        (@doi4, N'Lưu Văn Bảo',     11);

    PRINT N'✔ Đã thêm thành viên cho 4 đội (5 người/đội)';
END
ELSE
    PRINT N'⚠ Giải đấu demo đã tồn tại, bỏ qua';

GO

-- ============================================================
-- KIỂM TRA KẾT QUẢ
-- ============================================================
PRINT N'';
PRINT N'=== KẾT QUẢ SEED DEMO ===';

SELECT N'Users demo' AS Loai, COUNT(*) AS SoBanGhi
FROM Users WHERE Email LIKE '%@demo.vn'
UNION ALL
SELECT N'Giải đấu demo', COUNT(*)
FROM GiaiDaus WHERE TenGiai LIKE N'%PitchHub Mùa Hè 2026%'
UNION ALL
SELECT N'Đội bóng', COUNT(*)
FROM DoiBongs WHERE GiaiDauId IN (
    SELECT Id FROM GiaiDaus WHERE TenGiai LIKE N'%PitchHub Mùa Hè 2026%'
)
UNION ALL
SELECT N'Thành viên đội', COUNT(*)
FROM ThanhVienDois WHERE DoiId IN (
    SELECT Id FROM DoiBongs WHERE GiaiDauId IN (
        SELECT Id FROM GiaiDaus WHERE TenGiai LIKE N'%PitchHub Mùa Hè 2026%'
    )
);

PRINT N'';
PRINT N'========================================';
PRINT N'  ✅ Seed Demo hoàn thành!';
PRINT N'';
PRINT N'  Tài khoản demo (mật khẩu: user123):';
PRINT N'  - nguyenvanan@demo.vn';
PRINT N'  - tranminhkhoa@demo.vn';
PRINT N'  - lehoangnam@demo.vn';
PRINT N'  - phamquochung@demo.vn';
PRINT N'  - dothanhddat@demo.vn';
PRINT N'  - vutiendung@demo.vn';
PRINT N'  - hoangducanh@demo.vn';
PRINT N'  - buivanthang@demo.vn';
PRINT N'';
PRINT N'  Giải đấu: "Giải Bóng Đá PitchHub Mùa Hè 2026"';
PRINT N'  TrangThai: Approved (Owner có thể mở đăng ký ngay)';
PRINT N'  4 đội đã chia bảng + 20 thành viên sẵn sàng';
PRINT N'========================================';
GO