-- ============================================================================
-- Migrate_ChamMonTuyChon.sql
-- Chấm sao từng món là TÙY CHỌN: CHI_TIET_DANH_GIA.soSao = NULL nghĩa là khách
-- không đánh giá riêng món đó (không còn "thừa hưởng" sao chung của đơn).
-- Chạy được trên CSDL đang có dữ liệu và chạy lại nhiều lần không lỗi.
-- ============================================================================
USE RestaurantCRM;
GO

-- 1. Cho phép soSao NULL + ràng buộc 1..5
ALTER TABLE CHI_TIET_DANH_GIA ALTER COLUMN soSao INT NULL;
GO
IF NOT EXISTS (SELECT 1 FROM sys.check_constraints WHERE name = 'CK_CHI_TIET_DANH_GIA_soSao')
    ALTER TABLE CHI_TIET_DANH_GIA
        ADD CONSTRAINT CK_CHI_TIET_DANH_GIA_soSao CHECK (soSao IS NULL OR soSao BETWEEN 1 AND 5);
GO

-- 2. Món không có nhận xét và có số sao bằng sao chung của đơn → coi là không đánh giá riêng
UPDATE ct
SET ct.soSao = NULL
FROM CHI_TIET_DANH_GIA ct
JOIN DANH_GIA d ON d.maDanhGia = ct.maDanhGia
WHERE (ct.noiDung IS NULL OR LTRIM(RTRIM(ct.noiDung)) = N'')
  AND ct.soSao IS NOT NULL
  AND ct.soSao = d.soSao;
GO

-- 3. Đánh giá chung của đơn không gắn món
UPDATE DANH_GIA SET maMon = NULL WHERE maMon IS NOT NULL;
GO

-- 4. Bổ sung dòng chi tiết (soSao NULL) cho món có trong đơn nhưng chưa có dòng chi tiết
INSERT INTO CHI_TIET_DANH_GIA (maChiTietDanhGia, maDanhGia, maMon, soSao, noiDung)
SELECT 'CT' + UPPER(LEFT(REPLACE(CONVERT(VARCHAR(36), NEWID()), '-', ''), 8)), x.maDanhGia, x.maMon, NULL, NULL
FROM (
    SELECT DISTINCT d.maDanhGia, cthd.maMon
    FROM DANH_GIA d
    JOIN CHI_TIET_HOA_DON cthd ON cthd.maHoaDon = d.maHoaDon
    WHERE d.maHoaDon IS NOT NULL
      AND cthd.maMon IS NOT NULL
      AND cthd.soLuong > 0
      AND NOT EXISTS (SELECT 1 FROM CHI_TIET_DANH_GIA ct
                      WHERE ct.maDanhGia = d.maDanhGia AND ct.maMon = cthd.maMon)
) x;
GO

-- 5. Mỗi đánh giá chỉ giữ câu trả lời sớm nhất của nhân viên
WITH TraLoiXepHang AS (
    SELECT maTraLoi,
           ROW_NUMBER() OVER (PARTITION BY maDanhGia ORDER BY ngayGui, maTraLoi) AS rn
    FROM TRA_LOI_DANH_GIA
)
DELETE FROM TRA_LOI_DANH_GIA
WHERE maTraLoi IN (SELECT maTraLoi FROM TraLoiXepHang WHERE rn > 1);
GO

PRINT N'Đã chuyển CSDL sang chấm món tùy chọn.';
