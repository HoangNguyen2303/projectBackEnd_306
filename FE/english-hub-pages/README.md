# EnglishHub CMS — đã nối backend thật (EduTrack API)

Bộ 9 trang HTML này ban đầu là prototype tĩnh (dữ liệu giả), nay đã được nối vào
backend thật (`EduTrack.Api`, ASP.NET Core + SQL Server) cho các phần: đăng nhập/đăng ký,
Học sinh, Giáo viên, Lớp học, Môn học, Điểm số, Đăng ký môn học, và số liệu tổng quan
trên Dashboard. Trang **Sức khỏe** đã bị gỡ bỏ vì backend chưa có dữ liệu cho phần này.

## Cấu trúc

```
english-hub-pages/
├── index.html               # Trang gốc — tự chuyển sang 01-login.html
├── assets/
│   ├── support.js           # runtime dùng chung cho mọi trang (không sửa)
│   ├── auth.js               # gọi API đăng nhập/đăng ký, lưu session vào localStorage
│   └── api.js                 # helper gọi API có kèm Bearer token, tự xử lý lỗi 401
├── 01-login.html             # Đăng nhập / Đăng ký — gọi API thật
├── 02-dashboard.html         # Dashboard — 4 số KPI đầu lấy dữ liệu thật
├── 03-students.html          # Quản lý học sinh — CRUD thật + xem GPA
├── 04-teachers.html          # Quản lý giáo viên — CRUD thật
├── 05-classes.html           # Quản lý lớp học — CRUD thật + xem danh sách học sinh trong lớp
├── 06-grades.html            # Quản lý điểm — sửa điểm thật, tổng kết/xếp loại do backend tính
├── 08-subjects.html          # Danh mục môn học (Courses) — CRUD thật
├── 09-registration.html      # Đăng ký môn học — học sinh tự đăng ký lớp còn chỗ, Admin/GV xem + hủy
└── 10-profile.html           # Hồ sơ cá nhân
```

## Cách chạy

Cần chạy backend **và** phục vụ các file này qua HTTP (không mở file trực tiếp bằng
trình duyệt — `localStorage` sẽ không dùng chung được giữa các trang nếu mở kiểu `file://`).

```bash
# Cửa sổ 1 — backend (từ thư mục gốc chứa EduTrack.sln)
cd projectbackend_306/EduTrack.Api
dotnet run --urls http://localhost:5245

# Cửa sổ 2 — phục vụ frontend qua HTTP
cd FE/english-hub-pages
npx serve -l 8080 .
```

Sau đó mở `http://localhost:8080/`.

> Nếu backend chạy ở cổng/host khác `http://localhost:5245`, sửa hằng số `API_URL`
> ở đầu 2 file `assets/auth.js` và `assets/api.js`.
>
> Backend cần cấu hình CORS cho phép origin của frontend (mặc định đã mở sẵn
> `http://localhost:5173`, `5174`, `8080` — xem `EduTrack.Infrastructure/DependencyInjection.cs`).

## Tài khoản demo (được seed sẵn khi backend chạy ở môi trường Development)

| Vai trò | Email | Mật khẩu |
|---|---|---|
| Admin | `admin@edutrack.local` | `Admin@123` |
| Giáo viên | `teacher@edutrack.local` | `Teacher@123` |
| Học sinh | `student@edutrack.local` | `Student@123` |

## Những gì còn là dữ liệu mẫu / chưa nối

- Biểu đồ "Học sinh theo cấp độ" và "Tỉ lệ đăng ký môn" trên Dashboard, và mục
  "Hoạt động gần đây" — backend chưa có API tương ứng.
- Trang Sức khỏe đã bị gỡ (không còn trong bộ file này) vì backend không có bảng dữ liệu
  cho hồ sơ sức khỏe học sinh.
- Đăng ký môn không có bước "chờ duyệt" như bản thiết kế gốc — backend tạo Enrollment
  ngay khi học sinh bấm đăng ký (không có trạng thái pending).
- Tài khoản tạo qua "Đăng ký" trên trang login **chưa tự liên kết** với hồ sơ Học sinh —
  cần Admin tạo hồ sơ Học sinh và liên kết thủ công (chưa có API cho việc này).
