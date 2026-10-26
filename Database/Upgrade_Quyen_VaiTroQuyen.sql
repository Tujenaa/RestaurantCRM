USE RestaurantCRM;
GO

SET NOCOUNT ON;
GO

-- ============================================================================
-- SCRIPT NÂNG CẤP CƠ SỞ DỮ LIỆU: BỔ SUNG BẢNG QUYEN VÀ VAI_TRO_QUYEN
-- Script này an toàn (idempotent), có thể chạy nhiều lần mà không làm hỏng dữ liệu.
-- ============================================================================

-- 1. Tạo bảng QUYEN nếu chưa tồn tại
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'QUYEN')
BEGIN
    CREATE TABLE QUYEN (
        maQuyen VARCHAR(50) PRIMARY KEY,
        tenQuyen NVARCHAR(100) NOT NULL,
        nhomQuyen NVARCHAR(50) NOT NULL,
        moTa NVARCHAR(255) NULL
    );
    PRINT N'-> Đã tạo bảng QUYEN thành công.';
END
ELSE
BEGIN
    PRINT N'-> Bảng QUYEN đã tồn tại.';
END
GO

-- 2. Tạo bảng VAI_TRO_QUYEN nếu chưa tồn tại
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'VAI_TRO_QUYEN')
BEGIN
    CREATE TABLE VAI_TRO_QUYEN (
        maVaiTro VARCHAR(20) NOT NULL,
        maQuyen VARCHAR(50) NOT NULL,
        ngayTao DATETIME DEFAULT GETDATE(),
        CONSTRAINT PK_VAI_TRO_QUYEN PRIMARY KEY (maVaiTro, maQuyen),
        CONSTRAINT FK_VTQ_VAITRO FOREIGN KEY (maVaiTro) REFERENCES VAI_TRO(maVaiTro) ON DELETE CASCADE,
        CONSTRAINT FK_VTQ_QUYEN FOREIGN KEY (maQuyen) REFERENCES QUYEN(maQuyen) ON DELETE CASCADE
    );
    PRINT N'-> Đã tạo bảng VAI_TRO_QUYEN thành công.';
END
ELSE
BEGIN
    PRINT N'-> Bảng VAI_TRO_QUYEN đã tồn tại.';
END
GO

-- 3. Nạp danh mục 16 mã quyền chuẩn vào bảng QUYEN
MERGE INTO QUYEN AS Target
USING (
    VALUES
    -- Nhóm Hệ thống
    ('ADMIN_ACCOUNT', N'Quản lý Tài khoản', N'Hệ thống', N'Quản trị danh sách tài khoản nhân viên'),
    ('ADMIN_ROLE', N'Vai trò & Phân quyền', N'Hệ thống', N'Cấu hình vai trò và gán quyền chi tiết'),
    ('ADMIN_BACKUP', N'Sao lưu & Phục hồi CSDL', N'Hệ thống', N'Toàn quyền sao lưu, tải về và phục hồi cơ sở dữ liệu'),

    -- Nhóm Vận Hành
    ('ORDER_CREATE', N'Tạo đơn hàng mới', N'Vận Hành', N'Cho phép mở màn hình Tạo đơn hàng và lập hóa đơn mới'),
    ('ORDER_VIEW', N'Quản lý đơn hàng', N'Vận Hành', N'Xem danh sách và trạng thái hóa đơn / đơn hàng'),
    ('MENU_VIEW', N'Thực đơn món ăn', N'Vận Hành', N'Xem và tra cứu thực đơn món ăn'),
    ('PROMO_VIEW', N'Khuyến mãi', N'Vận Hành', N'Quản lý chương trình khuyến mãi và voucher'),
    ('INVENTORY_VIEW', N'Kiểm tra kho', N'Vận Hành', N'Xem thông tin tồn kho thực phẩm'),
    ('IMPORT_VIEW', N'Quản lý phiếu nhập', N'Vận Hành', N'Xem và tạo phiếu nhập nguyên vật liệu'),
    ('INGREDIENT_VIEW', N'Nguyên vật liệu', N'Vận Hành', N'Quản lý danh mục nguyên vật liệu nhà hàng'),
    ('REPORT_VIEW', N'Báo cáo doanh thu', N'Vận Hành', N'Xem báo cáo thống kê doanh thu bán hàng'),

    -- Nhóm Khách hàng
    ('CRM_CUSTOMER', N'Quản lý khách hàng', N'Khách hàng', N'Xem và quản lý hồ sơ khách hàng'),
    ('CRM_REVIEW', N'Đánh giá dịch vụ', N'Khách hàng', N'Xem và phản hồi đánh giá món ăn / dịch vụ'),
    ('CRM_FEEDBACK', N'Góp ý & Phản hồi', N'Khách hàng', N'Xử lý góp ý và khiếu nại của khách'),
    ('CRM_SURVEY', N'Khảo sát ý kiến', N'Khách hàng', N'Thiết lập và quản lý các phiếu khảo sát'),
    ('CRM_ANALYTICS', N'Phân tích khách hàng', N'Khách hàng', N'Xem biểu đồ và báo cáo phân tích tệp khách hàng')
) AS Source (maQuyen, tenQuyen, nhomQuyen, moTa)
ON Target.maQuyen = Source.maQuyen
WHEN MATCHED THEN
    UPDATE SET Target.tenQuyen = Source.tenQuyen, Target.nhomQuyen = Source.nhomQuyen, Target.moTa = Source.moTa
WHEN NOT MATCHED THEN
    INSERT (maQuyen, tenQuyen, nhomQuyen, moTa)
    VALUES (Source.maQuyen, Source.tenQuyen, Source.nhomQuyen, Source.moTa);

PRINT N'-> Đã đồng bộ 16 mã quyền chuẩn vào bảng QUYEN.';
GO

-- 4. Gán quyền mặc định cho các vai trò chuẩn nếu có trong hệ thống
-- 4.1. Vai trò VT01 (Hệ thống / admin) -> Toàn quyền (16 quyền)
IF EXISTS (SELECT 1 FROM VAI_TRO WHERE maVaiTro = 'VT01')
BEGIN
    INSERT INTO VAI_TRO_QUYEN (maVaiTro, maQuyen)
    SELECT 'VT01', q.maQuyen
    FROM QUYEN q
    WHERE NOT EXISTS (
        SELECT 1 FROM VAI_TRO_QUYEN vtq WHERE vtq.maVaiTro = 'VT01' AND vtq.maQuyen = q.maQuyen
    );
    PRINT N'-> Đã gán quyền đầy đủ cho vai trò VT01 (Hệ thống).';
END
GO

-- 4.2. Vai trò VT02 (Vận Hành / sale) -> 8 quyền phân hệ Vận Hành
IF EXISTS (SELECT 1 FROM VAI_TRO WHERE maVaiTro = 'VT02')
BEGIN
    INSERT INTO VAI_TRO_QUYEN (maVaiTro, maQuyen)
    SELECT 'VT02', q.maQuyen
    FROM QUYEN q
    WHERE q.nhomQuyen = N'Vận Hành'
      AND NOT EXISTS (
        SELECT 1 FROM VAI_TRO_QUYEN vtq WHERE vtq.maVaiTro = 'VT02' AND vtq.maQuyen = q.maQuyen
    );
    PRINT N'-> Đã gán quyền Vận Hành cho vai trò VT02.';
END
GO

-- 4.3. Vai trò VT03 (Khách hàng / crm) -> 5 quyền phân hệ Khách hàng
IF EXISTS (SELECT 1 FROM VAI_TRO WHERE maVaiTro = 'VT03')
BEGIN
    INSERT INTO VAI_TRO_QUYEN (maVaiTro, maQuyen)
    SELECT 'VT03', q.maQuyen
    FROM QUYEN q
    WHERE q.nhomQuyen = N'Khách hàng'
      AND NOT EXISTS (
        SELECT 1 FROM VAI_TRO_QUYEN vtq WHERE vtq.maVaiTro = 'VT03' AND vtq.maQuyen = q.maQuyen
    );
    PRINT N'-> Đã gán quyền Khách hàng cho vai trò VT03.';
END
GO

-- 4.4. Di chuyển dữ liệu quyền tùy biến từ cột moTa của VAI_TRO sang VAI_TRO_QUYEN (nếu có)
INSERT INTO VAI_TRO_QUYEN (maVaiTro, maQuyen)
SELECT v.maVaiTro, q.maQuyen
FROM VAI_TRO v
CROSS JOIN QUYEN q
WHERE (
    -- Trường hợp chứa dấu * (toàn quyền)
    CHARINDEX('*', v.moTa) > 0
    -- Hoặc chứa chính xác mã quyền trong chuỗi phân tách dấu phẩy
    OR CHARINDEX(',' + q.maQuyen + ',', ',' + REPLACE(REPLACE(v.moTa, ' ', ''), ';', ',') + ',') > 0
)
AND NOT EXISTS (
    SELECT 1 FROM VAI_TRO_QUYEN vtq WHERE vtq.maVaiTro = v.maVaiTro AND vtq.maQuyen = q.maQuyen
);
PRINT N'-> Đã hoàn tất đồng bộ phân quyền từ moTa sang VAI_TRO_QUYEN.';
GO

PRINT N'=== HOÀN TẤT NÂNG CẤP BẢNG PHÂN QUYỀN TRÊN SQL SERVER ===';
GO
