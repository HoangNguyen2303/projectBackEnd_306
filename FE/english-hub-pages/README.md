# EnglishHub CMS — Tách theo từng trang

File gốc `EnglishHub_CMS_dc.html` là một **prototype** dùng ngôn ngữ template riêng
(`<x-dc>`, `sc-if`, `sc-for`, `{{ }}`) được render bởi `support.js`. Toàn bộ các trang
nằm chung trong **một component**, chuyển trang bằng biến `state.page`.

Thư mục này đã tách mỗi trang thành **một file HTML chạy độc lập**: mỗi file giữ nguyên
khung chung (sidebar + topbar) và **đúng một trang nội dung**, được khởi tạo sẵn để mở ra
là hiển thị ngay trang đó.

## Cấu trúc

```
english-hub-pages/
├── assets/
│   └── support.js          # runtime dùng chung cho mọi trang
├── 01-login.html           # Đăng nhập / Đăng ký (toggle trong trang)
├── 02-dashboard.html       # Dashboard
├── 03-students.html        # Quản lý học sinh (+ modal chi tiết / thêm mới)
├── 04-teachers.html        # Quản lý giáo viên
├── 05-classes.html         # Quản lý lớp học
├── 06-grades.html          # Quản lý điểm
├── 07-health.html          # Tình trạng sức khỏe
├── 08-subjects.html        # Quản lý môn học
├── 09-registration.html    # Đăng ký môn học
├── 10-profile.html         # Hồ sơ cá nhân
└── README.md
```

## Cách mở
- Mở trực tiếp bất kỳ file `.html` nào bằng trình duyệt.
- Cần **kết nối mạng** vì trang tải font (Google Fonts) và icon (Lucide) từ CDN.
  Riêng `support.js` đã là file cục bộ trong `assets/`.

## Ghi chú quan trọng
- **Đổi vai trò / giao diện:** dùng các nút **Admin · Teacher · Student** và nút
  sáng/tối trên thanh topbar. Nội dung nhiều trang (điểm, sức khỏe, đăng ký) thay đổi
  theo vai trò. Mặc định là **Admin · Light**.
- **Điều hướng sidebar:** trong mỗi file chỉ có markup của **một trang**. Bấm menu sang
  trang khác sẽ ra vùng nội dung trống — hãy mở file `.html` tương ứng để xem trang đó.
- **Không có trang "Quên mật khẩu" riêng** trong prototype gốc; nó chỉ là link `#forgot`
  trong trang đăng nhập. Nếu cần, có thể tạo thêm một file mới dựa trên `01-login.html`.
- **Dữ liệu & logic dùng chung:** mỗi file chứa cùng một khối `<script type="text/x-dc">`
  (danh sách học sinh, giáo viên, lớp, điểm, sức khỏe, môn học…). Nếu sửa dữ liệu, nhớ
  đồng bộ ở các file khác, hoặc gộp lại về một nguồn khi chuyển sang code thật.

## Gợi ý bước tiếp theo
Nếu muốn chuyển sang một dự án web thật (React/Next.js…), mỗi file này tương ứng một
**route/trang**; phần khung chung (sidebar + topbar) nên tách thành **layout** dùng lại,
và khối dữ liệu tĩnh nên thay bằng API. Mình có thể hỗ trợ chuyển đổi nếu bạn cần.
