# Thiết kế Website Quản lý Trung tâm Tiếng Anh

> Tài liệu mô tả thiết kế một website hiện đại phục vụ quản lý học sinh và giáo viên cho một trung tâm tiếng Anh.
> Bao gồm: mô tả chức năng, phân quyền người dùng, danh sách trang Frontend, và **2 phần trọng tâm** — Thiết kế giao diện & Bảng màu nền đặc trưng.

---

## 1. Tổng quan dự án

| Hạng mục | Nội dung |
|---|---|
| **Tên hệ thống** | EnglishHub — Center Management System (CMS) |
| **Mục tiêu** | Số hóa toàn bộ quy trình vận hành: quản lý học sinh, giáo viên, lớp học, môn học, điểm số, sức khỏe và đăng ký môn |
| **Nền tảng** | Responsive Web App (Desktop / Tablet / Mobile) |
| **Đối tượng dùng** | Admin (quản trị viên), Teacher (giáo viên), Student (học sinh) |
| **Phong cách** | Hiện đại, tối giản (minimal), thân thiện giáo dục, hỗ trợ Light/Dark mode |

**Gợi ý công nghệ (tham khảo):** React/Next.js + TailwindCSS cho Frontend, thư viện UI như shadcn/ui hoặc Ant Design, biểu đồ dùng Recharts/Chart.js.

---

## 2. Phân quyền người dùng (Roles & Permissions)

Hệ thống chia thành **3 nhóm quyền** chính. Mỗi vai trò chỉ nhìn thấy các menu và thao tác được phép.

### 2.1. Admin (Quản trị viên)
Toàn quyền trên hệ thống.

- Quản lý tài khoản người dùng (tạo / khóa / phân quyền).
- Quản lý toàn bộ học sinh, giáo viên.
- Tạo và cấu hình lớp học, môn học, học kỳ.
- Xem toàn bộ báo cáo, thống kê, dashboard tổng quan.
- Duyệt / hủy đăng ký môn học.
- Quản lý trạng thái hoạt động (kích hoạt, tạm ngưng, tốt nghiệp…).

### 2.2. Teacher (Giáo viên)
Quyền giới hạn theo lớp/môn được phân công.

- Xem danh sách học sinh trong lớp mình dạy.
- Nhập / chỉnh sửa điểm cho học sinh của mình.
- Cập nhật ghi chú tình trạng sức khỏe / thái độ học tập.
- Xem lịch dạy, danh sách môn phụ trách.
- Không được xóa học sinh hay chỉnh sửa cấu hình hệ thống.

### 2.3. Student / User (Học sinh)
Quyền cá nhân.

- Xem điểm số, kết quả học tập của bản thân.
- Đăng ký / hủy đăng ký môn học (trong thời gian mở đăng ký).
- Cập nhật thông tin cá nhân, khai báo tình trạng sức khỏe.
- Xem thời khóa biểu, lớp đang tham gia.
- Không thấy dữ liệu của học sinh khác.

**Bảng tóm tắt quyền:**

| Chức năng | Admin | Teacher | Student |
|---|:---:|:---:|:---:|
| Dashboard tổng quan | ✅ | ⚪ (riêng) | ⚪ (riêng) |
| Quản lý tài khoản | ✅ | ❌ | ❌ |
| Quản lý học sinh/giáo viên | ✅ | 👁️ (xem) | ❌ |
| Nhập điểm | ✅ | ✅ | ❌ |
| Xem điểm | ✅ | ✅ | 👁️ (của mình) |
| Quản lý sức khỏe | ✅ | ✅ (ghi chú) | ✅ (khai báo) |
| Quản lý lớp / môn học | ✅ | 👁️ | ❌ |
| Đăng ký môn học | ✅ (duyệt) | ❌ | ✅ |
| Cấu hình hệ thống | ✅ | ❌ | ❌ |

> Chú thích: ✅ toàn quyền · 👁️ chỉ xem · ⚪ bản riêng của mình · ❌ không có quyền

---

## 3. Các trang Frontend (Sơ đồ luồng)

### 3.1. Nhóm trang xác thực (Auth) — không cần đăng nhập
1. **Đăng nhập (Login)** — email/username + mật khẩu, nút "Ghi nhớ đăng nhập", link "Quên mật khẩu".
2. **Đăng ký (Register)** — họ tên, email, mật khẩu, xác nhận mật khẩu, chọn vai trò (mặc định Student), điều khoản sử dụng.
3. **Quên mật khẩu (Forgot Password)** — nhập email → gửi link/OTP đặt lại.
4. **Đặt lại mật khẩu (Reset Password)** — nhập mật khẩu mới + xác nhận.
5. **Xác thực OTP / Email** (tùy chọn) — nhập mã 6 số.

### 3.2. Nhóm trang sau đăng nhập (App)
- **Dashboard** (khác nhau theo vai trò).
- **Quản lý học sinh** — danh sách, chi tiết, thêm/sửa/xóa.
- **Quản lý giáo viên** — danh sách, phân công lớp/môn.
- **Quản lý điểm** — bảng điểm theo lớp/môn, nhập điểm.
- **Tình trạng sức khỏe** — hồ sơ sức khỏe, cảnh báo.
- **Quản lý lớp học** — tạo lớp, gán giáo viên, sĩ số.
- **Quản lý môn học** — danh mục môn, tín chỉ/cấp độ (A1–C2).
- **Đăng ký môn học** — chọn môn, xem lịch trùng, xác nhận.
- **Trạng thái** — trạng thái học sinh/lớp/đăng ký.
- **Hồ sơ cá nhân (Profile)** — thông tin, đổi mật khẩu, ảnh đại diện.
- **Cài đặt (Settings)** — Light/Dark mode, ngôn ngữ, thông báo.

**Luồng điều hướng cơ bản:**
```
Login ──▶ Dashboard ──▶ (menu theo quyền)
  │
  ├─▶ Register ──▶ Login
  └─▶ Forgot Password ──▶ Reset Password ──▶ Login
```

---

## 4. Mô tả chức năng chi tiết theo Module

### 4.1. Dashboard Admin
Bảng điều khiển tổng quan hiển thị số liệu real-time:
- **Thẻ số liệu (KPI cards):** tổng học sinh, tổng giáo viên, số lớp đang mở, số đăng ký chờ duyệt.
- **Biểu đồ:** số học sinh theo cấp độ (A1–C2), tỉ lệ đăng ký môn, phân bố điểm.
- **Bảng hoạt động gần đây:** đăng ký mới, điểm vừa nhập, cảnh báo sức khỏe.
- **Lối tắt (Quick actions):** thêm học sinh, tạo lớp, duyệt đăng ký.

### 4.2. Dashboard User (Student/Teacher)
- **Student:** điểm trung bình, môn đang học, lịch học tuần, thông báo.
- **Teacher:** lớp đang dạy, số học sinh, việc cần làm (nhập điểm), lịch dạy.

### 4.3. Quản lý điểm học sinh
- Nhập điểm theo dạng: điểm chuyên cần, giữa kỳ, cuối kỳ, kỹ năng (Listening/Speaking/Reading/Writing).
- Tự tính điểm trung bình + xếp loại (Pass/Fail, hoặc A/B/C…).
- Xuất bảng điểm ra Excel/PDF.
- Lọc theo lớp, môn, học kỳ.

### 4.4. Tình trạng sức khỏe học sinh
- Hồ sơ: chiều cao, cân nặng, nhóm máu, dị ứng, bệnh nền, liên hệ khẩn cấp.
- Ghi chú theo dõi (giáo viên nhập khi học sinh nghỉ ốm…).
- **Cảnh báo màu:** Khỏe mạnh (xanh), Cần theo dõi (vàng), Cần chú ý (đỏ).
- Bảo mật: chỉ Admin, giáo viên phụ trách và chính học sinh xem được.

### 4.5. Quản lý lớp học
- Tạo lớp: tên lớp, cấp độ, giáo viên phụ trách, phòng học, sĩ số tối đa, lịch học.
- Xem danh sách học sinh trong lớp, thêm/bớt học sinh.
- Trạng thái lớp: Đang mở / Đã đầy / Đã kết thúc.

### 4.6. Quản lý môn học
- Danh mục môn: tên môn, mã môn, cấp độ (A1–C2), số buổi, mô tả, học phí.
- Gán môn cho lớp, thiết lập môn tiên quyết (nếu có).
- Bật/tắt mở đăng ký.

### 4.7. Đăng ký môn học
- Học sinh xem danh sách môn đang mở → chọn → kiểm tra trùng lịch → xác nhận.
- Trạng thái đăng ký: Chờ duyệt / Đã duyệt / Bị từ chối / Đã hủy.
- Admin duyệt hàng loạt hoặc từng đăng ký.

### 4.8. Trạng thái (Status)
- **Học sinh:** Đang học / Bảo lưu / Tốt nghiệp / Nghỉ học.
- **Lớp:** Đang mở / Đã đầy / Đã đóng.
- **Đăng ký:** Chờ / Duyệt / Từ chối / Hủy.
- Hiển thị bằng **badge màu** (xem phần bảng màu).

---

# PHẦN A — THIẾT KẾ GIAO DIỆN

## 5. Nguyên tắc thiết kế (Design Principles)

1. **Tối giản có chủ đích (Minimal):** nhiều khoảng trắng, ít viền, tập trung vào nội dung.
2. **Nhất quán (Consistent):** cùng một hệ thống spacing, bo góc, đổ bóng trên toàn app.
3. **Phân cấp thị giác rõ (Hierarchy):** tiêu đề đậm, số liệu nổi bật, phụ chú nhạt.
4. **Thân thiện giáo dục:** bo góc mềm (rounded), icon dễ hiểu, cảm giác nhẹ nhàng.
5. **Khả năng tiếp cận (Accessibility):** tương phản đạt chuẩn WCAG AA, chữ tối thiểu 14px.

## 6. Bố cục tổng thể (Layout)

### 6.1. Trang App (sau đăng nhập)
```
┌───────────────────────────────────────────────────────────┐
│  TOPBAR: Logo | Ô tìm kiếm | Chuông thông báo | Avatar     │
├──────────┬────────────────────────────────────────────────┤
│          │                                                 │
│ SIDEBAR  │   MAIN CONTENT                                  │
│ (menu    │   ┌─────────┬─────────┬─────────┐               │
│  theo    │   │ KPI card│ KPI card│ KPI card│  ← thẻ số liệu│
│  quyền)  │   └─────────┴─────────┴─────────┘               │
│          │   ┌───────────────────┬──────────┐              │
│ • Dashbd │   │   Biểu đồ chính   │  Bảng     │              │
│ • Học sinh│  │                   │  hoạt động│              │
│ • Lớp    │   └───────────────────┴──────────┘              │
│ • Môn học│                                                 │
│ • Điểm   │                                                 │
│ • ...    │                                                 │
└──────────┴────────────────────────────────────────────────┘
```
- **Sidebar:** cố định bên trái, có thể thu gọn (collapse) thành icon.
- **Topbar:** cố định trên cùng, chứa tìm kiếm và menu tài khoản.
- **Main:** dạng lưới (grid) 12 cột, responsive.

### 6.2. Trang Auth (Login/Register/Forgot)
- Bố cục **2 cột (split screen)**:
  - **Trái:** ảnh minh họa / banner thương hiệu (gradient + hình học sinh học tiếng Anh).
  - **Phải:** form nhập liệu căn giữa, card trắng nổi trên nền nhạt.
- Trên mobile: gộp thành 1 cột, form chiếm toàn màn hình.

## 7. Hệ thống thành phần (Design System)

| Thành phần | Mô tả thiết kế |
|---|---|
| **Typography** | Font chính: *Inter* / *Be Vietnam Pro* (hỗ trợ tiếng Việt). Tiêu đề 20–32px đậm; nội dung 14–16px; phụ chú 12px. |
| **Spacing** | Hệ 4px (4, 8, 12, 16, 24, 32). Padding card 24px. |
| **Bo góc (Radius)** | Nút & input: 8px · Card: 12–16px · Avatar: bo tròn. |
| **Đổ bóng (Shadow)** | Nhẹ, mềm: `0 1px 3px rgba(0,0,0,.08)`. Hover nâng nhẹ. |
| **Nút (Button)** | Primary (nền đậm), Secondary (viền), Ghost (trong suốt), Danger (đỏ). |
| **Input** | Viền mảnh, focus đổi màu viền theo Primary, có label nổi. |
| **Badge trạng thái** | Viên thuốc (pill) bo tròn, nền nhạt + chữ đậm theo màu trạng thái. |
| **Bảng (Table)** | Hàng cách nhau, hover đổi nền, header dính (sticky), phân trang. |
| **Card** | Nền trắng, bo góc, bóng nhẹ, tiêu đề + nội dung. |
| **Icon** | Bộ line icon đồng nhất (Lucide / Heroicons), 20–24px. |

## 8. Trạng thái tương tác (States)
- **Hover:** đổi nền nhạt / nâng bóng.
- **Focus:** viền màu Primary + vòng sáng (ring).
- **Active/Selected:** nền màu Primary nhạt, chữ Primary đậm.
- **Disabled:** giảm opacity 50%, con trỏ khóa.
- **Loading:** skeleton hoặc spinner.
- **Empty state:** hình minh họa + dòng gợi ý hành động.

---

# PHẦN B — MÀU NỀN & BẢNG MÀU ĐẶC TRƯNG

## 9. Ý tưởng màu sắc

Trung tâm tiếng Anh gợi cảm giác **tin cậy, trẻ trung, năng động và thân thiện**. Do đó bảng màu lấy:
- **Indigo/Xanh tím** làm màu chủ đạo (Primary) → thể hiện sự tin cậy, tri thức.
- **Cyan/Teal** làm màu phụ (Secondary) → tươi mới, cảm giác học tập.
- **Amber/Cam** làm màu nhấn (Accent) → năng lượng, khích lệ.
- Nền **xám xanh rất nhạt (slate)** → sạch, hiện đại, đỡ mỏi mắt.

## 10. Bảng màu chính (Light Mode)

| Vai trò | Tên | HEX | Dùng cho |
|---|---|---|---|
| **Primary** | Indigo 600 | `#4F46E5` | Nút chính, link, menu active, biểu tượng nhấn |
| Primary đậm | Indigo 700 | `#4338CA` | Hover nút chính |
| Primary nhạt | Indigo 50 | `#EEF2FF` | Nền menu được chọn, badge |
| **Secondary** | Cyan 500 | `#06B6D4` | Nút phụ, biểu đồ, điểm nhấn phụ |
| **Accent** | Amber 500 | `#F59E0B` | Cảnh báo nhẹ, huy hiệu nổi bật, CTA phụ |
| **Nền chính (Background)** | Slate 50 | `#F8FAFC` | Nền toàn trang |
| **Nền thẻ (Surface/Card)** | White | `#FFFFFF` | Card, bảng, form |
| **Nền phụ** | Slate 100 | `#F1F5F9` | Nền sidebar, vùng phụ |
| **Viền (Border)** | Slate 200 | `#E2E8F0` | Đường kẻ, viền input |
| **Chữ chính (Text)** | Slate 800 | `#1E293B` | Tiêu đề, nội dung chính |
| **Chữ phụ (Muted)** | Slate 500 | `#64748B` | Phụ chú, placeholder |

## 11. Bảng màu Dark Mode

| Vai trò | HEX | Dùng cho |
|---|---|---|
| Nền chính | `#0F172A` (Slate 900) | Nền toàn trang tối |
| Nền thẻ | `#1E293B` (Slate 800) | Card, bảng |
| Nền phụ | `#334155` (Slate 700) | Sidebar, hover |
| Viền | `#334155` | Đường kẻ |
| Chữ chính | `#F1F5F9` | Nội dung |
| Chữ phụ | `#94A3B8` | Phụ chú |
| Primary | `#818CF8` (Indigo 400) | Nút, link (sáng hơn để nổi trên nền tối) |

## 12. Màu trạng thái (Status Colors)

Dùng cho **badge trạng thái, cảnh báo, sức khỏe**. Mỗi màu có nền nhạt + chữ đậm.

| Trạng thái | Màu chữ/viền | Nền nhạt | Áp dụng |
|---|---|---|---|
| **Success / Đang học / Khỏe mạnh** | `#059669` | `#D1FAE5` | Đăng ký đã duyệt, học sinh đang học, sức khỏe tốt |
| **Warning / Cần theo dõi / Chờ duyệt** | `#D97706` | `#FEF3C7` | Đăng ký chờ, sức khỏe cần theo dõi |
| **Danger / Nghỉ học / Cần chú ý** | `#DC2626` | `#FEE2E2` | Từ chối, học sinh nghỉ, cảnh báo sức khỏe |
| **Info / Bảo lưu / Đã đầy** | `#2563EB` | `#DBEAFE` | Thông tin, lớp đã đầy |
| **Neutral / Đã đóng / Hủy** | `#475569` | `#F1F5F9` | Lớp đã đóng, đăng ký đã hủy |

**Ví dụ badge sức khỏe:**
- 🟢 `Khỏe mạnh` — nền `#D1FAE5`, chữ `#059669`
- 🟡 `Cần theo dõi` — nền `#FEF3C7`, chữ `#D97706`
- 🔴 `Cần chú ý` — nền `#FEE2E2`, chữ `#DC2626`

## 13. Màu cho biểu đồ (Chart Palette)
Bộ màu hài hòa dùng trong Dashboard:

| Thứ tự | HEX |
|---|---|
| 1 | `#4F46E5` (Indigo) |
| 2 | `#06B6D4` (Cyan) |
| 3 | `#F59E0B` (Amber) |
| 4 | `#10B981` (Emerald) |
| 5 | `#EC4899` (Pink) |
| 6 | `#8B5CF6` (Violet) |

## 14. Gradient thương hiệu
Dùng cho banner trang Auth, header đặc biệt, nút CTA lớn:
- **Gradient chính:** từ `#4F46E5` → `#06B6D4` (indigo sang cyan).
- **Gradient nhấn:** từ `#6366F1` → `#8B5CF6` (indigo sang violet).

```css
background: linear-gradient(135deg, #4F46E5 0%, #06B6D4 100%);
```

## 15. Ví dụ biến CSS (CSS Variables)
```css
:root {
  --color-primary: #4F46E5;
  --color-primary-hover: #4338CA;
  --color-primary-light: #EEF2FF;
  --color-secondary: #06B6D4;
  --color-accent: #F59E0B;

  --bg-base: #F8FAFC;
  --bg-surface: #FFFFFF;
  --bg-muted: #F1F5F9;
  --border: #E2E8F0;

  --text-main: #1E293B;
  --text-muted: #64748B;

  --success: #059669;
  --warning: #D97706;
  --danger:  #DC2626;
  --info:    #2563EB;

  --radius: 12px;
  --shadow: 0 1px 3px rgba(0,0,0,.08);
}

[data-theme="dark"] {
  --bg-base: #0F172A;
  --bg-surface: #1E293B;
  --bg-muted: #334155;
  --border: #334155;
  --text-main: #F1F5F9;
  --text-muted: #94A3B8;
  --color-primary: #818CF8;
}
```

---

## 16. Tổng kết nhanh

| Tiêu chí | Lựa chọn |
|---|---|
| Phong cách | Hiện đại, tối giản, thân thiện giáo dục |
| Màu chủ đạo | Indigo `#4F46E5` |
| Màu phụ | Cyan `#06B6D4` |
| Màu nhấn | Amber `#F59E0B` |
| Nền chính | Slate 50 `#F8FAFC` (light) / Slate 900 `#0F172A` (dark) |
| Font | Inter / Be Vietnam Pro |
| Bo góc | 8–16px |
| Chế độ | Light & Dark mode |

> Tài liệu này có thể dùng làm brief giao cho đội thiết kế (UI/UX) hoặc lập trình Frontend triển khai trực tiếp.
