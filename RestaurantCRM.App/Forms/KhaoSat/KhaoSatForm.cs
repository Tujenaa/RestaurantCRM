using System;
using System.Collections.Generic;
using System.Drawing;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace RestaurantCRM.AdminApp.Forms.KhaoSat
{
    public partial class KhaoSatForm : ModuleFormBase
    {
        private static readonly HttpClient _http = new HttpClient
        {
            BaseAddress = new Uri("http://localhost:5275")
        };

        private List<KhaoSatDTO> _danhSach = new List<KhaoSatDTO>();

        public KhaoSatForm() : base(
            title: "Quản lý Khảo Sát",
            description: "Danh sách khảo sát khách hàng",
            columns: new[] { "Mã KS", "Tiêu Đề", "Mô Tả", "Trạng Thái", "Ngày Tạo" },
            fields: new ModuleField[] { },
            rows: new string[][] { },
            accent: Color.FromArgb(139, 92, 246)
        )
        {
            this.Load += async (s, e) => await LoadData();
        }

        // ==================== LOAD DATA ====================
        public async Task LoadData()
        {
            try
            {
                _danhSach = await _http.GetFromJsonAsync<List<KhaoSatDTO>>("/api/AdminKhaoSat")
                    ?? new List<KhaoSatDTO>();

                var rows = new List<object[]>();
                foreach (var ks in _danhSach)
                {
                    rows.Add(new object[]
                    {
                        ks.MaKhaoSat ?? "",
                        ks.TieuDe ?? "",
                        ks.MoTa ?? "",
                        ks.TrangThai ?? "",
                        ks.NgayTao?.ToString("dd/MM/yyyy") ?? ""
                    });
                }

                SetTableData(rows);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi kết nối API: {ex.Message}",
                    "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // ==================== HOOKS CHO TOOLBAR BUTTONS ====================
        protected override async void OnAdd()
        {
            await MoFormChiTiet(null);
        }

        protected override async void OnEdit()
        {
            string maKS = GetSelectedId();
            if (string.IsNullOrEmpty(maKS))
            {
                MessageBox.Show("Vui lòng chọn khảo sát cần xem/sửa!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            await MoFormChiTiet(maKS);
        }

        protected override async void OnDelete()
        {
            string maKS = GetSelectedId();
            if (string.IsNullOrEmpty(maKS))
            {
                MessageBox.Show("Vui lòng chọn khảo sát cần xóa!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            await XoaKhaoSat(maKS);
        }

        protected override async void OnRowDoubleClick(string id, System.Data.DataRow row)
        {
            if (!string.IsNullOrEmpty(id))
            {
                await MoFormChiTiet(id);
            }
        }

        // ==================== XÓA KHẢO SÁT ====================
        public async Task XoaKhaoSat(string maKS)
        {
            var cf = MessageBox.Show(
                $"Bạn có chắc muốn xóa khảo sát #{maKS}?",
                "Xác nhận xóa", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

            if (cf != DialogResult.Yes) return;

            try
            {
                var res = await _http.DeleteAsync($"/api/AdminKhaoSat/{maKS}");
                if (res.IsSuccessStatusCode)
                {
                    MessageBox.Show("Xóa khảo sát thành công!", "Thành công",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                    await LoadData();
                }
                else
                {
                    string err = await res.Content.ReadAsStringAsync();
                    MessageBox.Show(string.IsNullOrEmpty(err) ? "Xóa thất bại!" : err, "Lỗi",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi kết nối API: {ex.Message}", "Lỗi",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // ==================== MO FORM CHI TIET / TAO MOI ====================
        private async Task MoFormChiTiet(string maKS = null)
        {
            bool isNew = maKS == null;
            KhaoSatChiTietDTO chiTiet = null;

            if (!isNew)
            {
                try
                {
                    chiTiet = await _http.GetFromJsonAsync<KhaoSatChiTietDTO>
                        ($"/api/AdminKhaoSat/{maKS}");
                }
                catch
                {
                    MessageBox.Show("Không lấy được thông tin khảo sát!",
                        "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
            }

            using (var form = new Form())
            {
                form.Text = isNew ? "Tạo khảo sát mới" : $"Chi tiết khảo sát #{maKS}";
                form.Size = new Size(700, 680);
                form.StartPosition = FormStartPosition.CenterParent;
                form.FormBorderStyle = FormBorderStyle.FixedDialog;
                form.MaximizeBox = false;

                // --- Tiêu đề ---
                var lblTieuDe = new Label { Text = "Tiêu Đề *", Location = new Point(16, 16), AutoSize = true };
                var txtTieuDe = new TextBox
                {
                    Location = new Point(16, 36),
                    Width = 650,
                    Text = chiTiet?.TieuDe ?? ""
                };

                // --- Mô tả ---
                var lblMoTa = new Label { Text = "Mô Tả", Location = new Point(16, 70), AutoSize = true };
                var txtMoTa = new RichTextBox
                {
                    Location = new Point(16, 90),
                    Size = new Size(650, 60),
                    Text = chiTiet?.MoTa ?? ""
                };

                // --- Trạng thái ---
                var lblTrangThai = new Label { Text = "Trạng Thái", Location = new Point(16, 164), AutoSize = true };
                var cboTrangThai = new ComboBox
                {
                    Location = new Point(16, 184),
                    Width = 200,
                    DropDownStyle = ComboBoxStyle.DropDownList
                };
                cboTrangThai.Items.AddRange(new object[] { "Đóng", "Mở" });
                cboTrangThai.SelectedItem = chiTiet?.TrangThai ?? "Đóng";

                // --- Danh sách câu hỏi ---
                var lblCauHoi = new Label
                {
                    Text = "Câu hỏi:",
                    Location = new Point(16, 220),
                    AutoSize = true,
                    Font = new Font("Segoe UI", 9, FontStyle.Bold)
                };

                var panelCauHoi = new Panel
                {
                    Location = new Point(16, 240),
                    Size = new Size(650, 300),
                    AutoScroll = true,
                    BorderStyle = BorderStyle.FixedSingle
                };

                // Render câu hỏi hiện có
                var cauHoiList = chiTiet?.CauHois ?? new List<CauHoiDTO>();
                RenderCauHois(panelCauHoi, cauHoiList, maKS, isNew);

                // --- Nút thêm câu hỏi ---
                var btnThemCauHoi = new Button
                {
                    Text = "+ Thêm câu hỏi",
                    Location = new Point(16, 550),
                    Size = new Size(150, 32),
                    FlatStyle = FlatStyle.Flat
                };

                btnThemCauHoi.Click += async (s, e) =>
                {
                    if (isNew)
                    {
                        MessageBox.Show("Vui lòng lưu khảo sát trước khi thêm câu hỏi.",
                            "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        return;
                    }
                    await MoFormCauHoi(maKS, null);
                    // Reload lại chi tiết
                    chiTiet = await _http.GetFromJsonAsync<KhaoSatChiTietDTO>
                        ($"/api/AdminKhaoSat/{maKS}");
                    cauHoiList = chiTiet?.CauHois ?? new List<CauHoiDTO>();
                    RenderCauHois(panelCauHoi, cauHoiList, maKS, false);
                };

                // --- Nút lưu / hủy ---
                var btnLuu = new Button
                {
                    Text = "Lưu",
                    Location = new Point(540, 590),
                    Size = new Size(126, 36),
                    BackColor = Color.FromArgb(139, 92, 246),
                    ForeColor = Color.White,
                    FlatStyle = FlatStyle.Flat
                };

                var btnHuy = new Button
                {
                    Text = "Hủy",
                    Location = new Point(404, 590),
                    Size = new Size(126, 36),
                    FlatStyle = FlatStyle.Flat
                };

                btnHuy.Click += (s, e) => form.Close();

                btnLuu.Click += async (s, e) =>
                {
                    if (string.IsNullOrWhiteSpace(txtTieuDe.Text))
                    {
                        MessageBox.Show("Vui lòng nhập tiêu đề!", "Thiếu thông tin",
                            MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }

                    var dto = new
                    {
                        TieuDe = txtTieuDe.Text.Trim(),
                        MoTa = txtMoTa.Text.Trim(),
                        TrangThai = cboTrangThai.SelectedItem?.ToString()
                    };

                    HttpResponseMessage res;
                    if (isNew)
                        res = await _http.PostAsJsonAsync("/api/AdminKhaoSat", dto);
                    else
                        res = await _http.PutAsJsonAsync($"/api/AdminKhaoSat/{maKS}", dto);

                    if (res.IsSuccessStatusCode)
                    {
                        MessageBox.Show(isNew ? "Tạo khảo sát thành công!" : "Cập nhật thành công!",
                            "Thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        form.Close();
                        await LoadData();
                    }
                    else
                    {
                        MessageBox.Show("Thao tác thất bại!",
                            "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                };

                form.Controls.AddRange(new Control[]
                {
                    lblTieuDe, txtTieuDe,
                    lblMoTa, txtMoTa,
                    lblTrangThai, cboTrangThai,
                    lblCauHoi, panelCauHoi,
                    btnThemCauHoi,
                    btnLuu, btnHuy
                });

                form.ShowDialog(this);
            }
        }

        // ==================== RENDER CAU HOI ====================
        private void RenderCauHois(Panel panel, List<CauHoiDTO> cauHois,
            string maKS, bool isNew)
        {
            panel.Controls.Clear();
            int y = 8;

            foreach (var ch in cauHois)
            {
                // Label câu hỏi
                var lblCH = new Label
                {
                    Text = $"• [{ch.LoaiCauHoi}] {ch.NoiDung}",
                    Location = new Point(8, y),
                    Width = 520,
                    AutoSize = false,
                    Height = 20
                };

                // Nút sửa câu hỏi
                var btnSuaCH = new Button
                {
                    Text = "Sửa",
                    Location = new Point(536, y - 2),
                    Size = new Size(50, 24),
                    FlatStyle = FlatStyle.Flat,
                    Tag = ch.MaCauHoi
                };
                btnSuaCH.Click += async (s, e) =>
                {
                    await MoFormCauHoi(maKS, ch);
                    var chiTiet = await _http.GetFromJsonAsync<KhaoSatChiTietDTO>
                        ($"/api/AdminKhaoSat/{maKS}");
                    RenderCauHois(panel, chiTiet?.CauHois ?? new List<CauHoiDTO>(), maKS, false);
                };

                // Nút xóa câu hỏi
                var btnXoaCH = new Button
                {
                    Text = "Xóa",
                    Location = new Point(592, y - 2),
                    Size = new Size(50, 24),
                    FlatStyle = FlatStyle.Flat,
                    Tag = ch.MaCauHoi
                };
                btnXoaCH.Click += async (s, e) =>
                {
                    var cf = MessageBox.Show("Xóa câu hỏi này?", "Xác nhận",
                        MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                    if (cf != DialogResult.Yes) return;

                    // Gọi API xóa câu hỏi (endpoint cần bổ sung)
                    var res = await _http.DeleteAsync(
                        $"/api/AdminKhaoSat/{maKS}/CauHoi/{ch.MaCauHoi}");
                    if (res.IsSuccessStatusCode)
                    {
                        var chiTiet = await _http.GetFromJsonAsync<KhaoSatChiTietDTO>
                            ($"/api/AdminKhaoSat/{maKS}");
                        RenderCauHois(panel, chiTiet?.CauHois ?? new List<CauHoiDTO>(), maKS, false);
                    }
                    else
                        MessageBox.Show("Xóa thất bại!", "Lỗi",
                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                };

                panel.Controls.AddRange(new Control[] { lblCH, btnSuaCH, btnXoaCH });
                y += 26;

                // Hiển thị tùy chọn nếu không phải tự luận
                if (ch.LoaiCauHoi != "TuLuan" && ch.TuyChons != null)
                {
                    foreach (var tc in ch.TuyChons)
                    {
                        var lblTC = new Label
                        {
                            Text = $"     ◦ {tc.NoiDung}",
                            Location = new Point(24, y),
                            Width = 460,
                            AutoSize = false,
                            Height = 18,
                            ForeColor = Color.Gray
                        };
                        panel.Controls.Add(lblTC);
                        y += 20;
                    }
                }

                y += 6;
            }
        }

        // ==================== FORM CAU HOI ====================
        private async Task MoFormCauHoi(string maKS, CauHoiDTO cauHoi)
        {
            bool isNew = cauHoi == null;

            using (var form = new Form())
            {
                form.Text = isNew ? "Thêm câu hỏi" : "Sửa câu hỏi";
                form.Size = new Size(520, 480);
                form.StartPosition = FormStartPosition.CenterParent;
                form.FormBorderStyle = FormBorderStyle.FixedDialog;
                form.MaximizeBox = false;

                var lblNoiDung = new Label { Text = "Nội dung câu hỏi *", Location = new Point(16, 16), AutoSize = true };
                var txtNoiDung = new TextBox
                {
                    Location = new Point(16, 36),
                    Width = 470,
                    Text = cauHoi?.NoiDung ?? ""
                };

                var lblLoai = new Label { Text = "Loại câu hỏi", Location = new Point(16, 72), AutoSize = true };
                var cboLoai = new ComboBox
                {
                    Location = new Point(16, 92),
                    Width = 200,
                    DropDownStyle = ComboBoxStyle.DropDownList
                };
                cboLoai.Items.AddRange(new object[] { "TracNghiem", "TuLuan" });
                cboLoai.SelectedItem = cauHoi?.LoaiCauHoi ?? "TracNghiem";

                // --- Panel tùy chọn ---
                var lblTuyChon = new Label
                {
                    Text = "Tùy chọn (mỗi dòng 1 tùy chọn):",
                    Location = new Point(16, 128),
                    AutoSize = true,
                    Font = new Font("Segoe UI", 9, FontStyle.Bold)
                };

                var panelTuyChon = new Panel
                {
                    Location = new Point(16, 150),
                    Size = new Size(470, 220),
                    AutoScroll = true,
                    BorderStyle = BorderStyle.FixedSingle
                };

                var tuyChonInputs = new List<TextBox>();

                void RenderTuyChons()
                {
                    panelTuyChon.Controls.Clear();
                    tuyChonInputs.Clear();
                    int ty = 8;

                    var dsTuyChon = cauHoi?.TuyChons ?? new List<TuyChonDTO>();
                    foreach (var tc in dsTuyChon)
                        ThemDongTuyChon(tc.NoiDung);

                    if (dsTuyChon.Count == 0)
                        ThemDongTuyChon("");

                    void ThemDongTuyChon(string text)
                    {
                        var txt = new TextBox
                        {
                            Location = new Point(8, ty),
                            Width = 380,
                            Text = text
                        };
                        var btnXoa = new Button
                        {
                            Text = "✕",
                            Location = new Point(396, ty - 1),
                            Size = new Size(32, 24),
                            FlatStyle = FlatStyle.Flat,
                            Tag = txt
                        };
                        btnXoa.Click += (s, e) =>
                        {
                            tuyChonInputs.Remove(txt);
                            panelTuyChon.Controls.Remove(txt);
                            panelTuyChon.Controls.Remove(btnXoa);
                        };

                        tuyChonInputs.Add(txt);
                        panelTuyChon.Controls.Add(txt);
                        panelTuyChon.Controls.Add(btnXoa);
                        ty += 32;
                    }
                }

                RenderTuyChons();

                // Ẩn/hiện tùy chọn theo loại câu hỏi
                cboLoai.SelectedIndexChanged += (s, e) =>
                {
                    bool isTuLuan = cboLoai.SelectedItem?.ToString() == "TuLuan";
                    lblTuyChon.Visible = !isTuLuan;
                    panelTuyChon.Visible = !isTuLuan;
                };

                if (cauHoi?.LoaiCauHoi == "TuLuan")
                {
                    lblTuyChon.Visible = false;
                    panelTuyChon.Visible = false;
                }

                var btnThemDong = new Button
                {
                    Text = "+ Thêm tùy chọn",
                    Location = new Point(16, 380),
                    Size = new Size(140, 28),
                    FlatStyle = FlatStyle.Flat
                };
                btnThemDong.Click += (s, e) =>
                {
                    // Thêm TextBox mới vào panelTuyChon
                    int ty = panelTuyChon.Controls.Count > 0
                        ? panelTuyChon.Controls[panelTuyChon.Controls.Count - 1].Bottom + 4
                        : 8;
                    var txt = new TextBox { Location = new Point(8, ty), Width = 380 };
                    var btnX = new Button
                    {
                        Text = "✕",
                        Location = new Point(396, ty - 1),
                        Size = new Size(32, 24),
                        FlatStyle = FlatStyle.Flat
                    };
                    btnX.Click += (_, __) =>
                    {
                        tuyChonInputs.Remove(txt);
                        panelTuyChon.Controls.Remove(txt);
                        panelTuyChon.Controls.Remove(btnX);
                    };
                    tuyChonInputs.Add(txt);
                    panelTuyChon.Controls.Add(txt);
                    panelTuyChon.Controls.Add(btnX);
                };

                var btnLuu = new Button
                {
                    Text = "Lưu",
                    Location = new Point(374, 420),
                    Size = new Size(112, 36),
                    BackColor = Color.FromArgb(139, 92, 246),
                    ForeColor = Color.White,
                    FlatStyle = FlatStyle.Flat
                };

                var btnHuy = new Button
                {
                    Text = "Hủy",
                    Location = new Point(254, 420),
                    Size = new Size(112, 36),
                    FlatStyle = FlatStyle.Flat
                };
                btnHuy.Click += (s, e) => form.Close();

                btnLuu.Click += async (s, e) =>
                {
                    if (string.IsNullOrWhiteSpace(txtNoiDung.Text))
                    {
                        MessageBox.Show("Vui lòng nhập nội dung câu hỏi!",
                            "Thiếu thông tin", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }

                    var tuyChons = new List<object>();
                    if (cboLoai.SelectedItem?.ToString() != "TuLuan")
                        foreach (var t in tuyChonInputs)
                            if (!string.IsNullOrWhiteSpace(t.Text))
                                tuyChons.Add(new { NoiDung = t.Text.Trim() });

                    var dto = new
                    {
                        NoiDung = txtNoiDung.Text.Trim(),
                        LoaiCauHoi = cboLoai.SelectedItem?.ToString(),
                        TuyChons = tuyChons
                    };

                    HttpResponseMessage res;
                    if (isNew)
                        // POST thêm câu hỏi (endpoint cần bổ sung vào AdminKhaoSatController)
                        res = await _http.PostAsJsonAsync(
                            $"/api/AdminKhaoSat/{maKS}/CauHoi", dto);
                    else
                        res = await _http.PutAsJsonAsync(
                            $"/api/AdminKhaoSat/{maKS}/CauHoi/{cauHoi.MaCauHoi}", dto);

                    if (res.IsSuccessStatusCode)
                    {
                        MessageBox.Show("Lưu câu hỏi thành công!",
                            "Thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        form.Close();
                    }
                    else
                        MessageBox.Show("Lưu thất bại! Kiểm tra lại API.",
                            "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                };

                form.Controls.AddRange(new Control[]
                {
                    lblNoiDung, txtNoiDung,
                    lblLoai, cboLoai,
                    lblTuyChon, panelTuyChon,
                    btnThemDong,
                    btnLuu, btnHuy
                });

                form.ShowDialog(this);
            }
        }

        // ==================== MO / DONG KHAO SAT ====================
        public async Task DoiTrangThaiKhaoSat(string maKS, string trangThai)
        {
            string msg = trangThai == "Mở" ? "mở" : "đóng";
            var cf = MessageBox.Show($"Bạn có chắc muốn {msg} khảo sát này?",
                "Xác nhận", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (cf != DialogResult.Yes) return;

            var res = await _http.PutAsJsonAsync(
                $"/api/AdminKhaoSat/{maKS}/status",
                new { TrangThai = trangThai });

            if (res.IsSuccessStatusCode)
            {
                MessageBox.Show($"Đã {msg} khảo sát!", "Thành công",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                await LoadData();
            }
            else
                MessageBox.Show("Thao tác thất bại!", "Lỗi",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        // ==================== XEM THONG KE ====================
        /// <summary>
        /// Mở cửa sổ thống kê kết quả khảo sát theo mã và tiêu đề.
        /// </summary>
        public void MoFormThongKe(string maKS, string tieuDe)
        {
            var frm = new ThongKeKhaoSatForm(maKS, tieuDe);
            frm.ShowDialog(this);
        }

        /// <summary>
        /// Mở thống kê cho dòng đang được chọn trong DataGridView.
        /// Gọi từ menu context hoặc nút bên ngoài: maKS = ô [0], tieuDe = ô [1].
        /// </summary>
        public void XemThongKeDongDangChon(DataGridView dgv)
        {
            if (dgv.CurrentRow == null) return;
            string maKS   = dgv.CurrentRow.Cells[0].Value?.ToString();
            string tieuDe = dgv.CurrentRow.Cells[1].Value?.ToString();
            if (string.IsNullOrEmpty(maKS)) return;
            MoFormThongKe(maKS, tieuDe ?? "");
        }
    }

    // ==================== DTOs ====================
    public class KhaoSatDTO
    {
        public string MaKhaoSat { get; set; }
        public string TieuDe { get; set; }
        public string MoTa { get; set; }
        public string TrangThai { get; set; }
        public DateTime? NgayTao { get; set; }
    }

    public class KhaoSatChiTietDTO : KhaoSatDTO
    {
        public List<CauHoiDTO> CauHois { get; set; }
    }

    public class CauHoiDTO
    {
        public string MaCauHoi { get; set; }
        public string NoiDung { get; set; }
        public string LoaiCauHoi { get; set; }
        public List<TuyChonDTO> TuyChons { get; set; }
    }

    public class TuyChonDTO
    {
        public string MaTuyChon { get; set; }
        public string NoiDung { get; set; }
    }
}