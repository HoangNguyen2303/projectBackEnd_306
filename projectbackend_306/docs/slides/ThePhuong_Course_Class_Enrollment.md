# Slide — Course, Class & Enrollment

## Vũ Thế Phương

**Module làm gì**

- Quản lý Department, Course và Class với CRUD, tìm kiếm, lọc và phân trang.
- Cho phép Student đăng ký/hủy lớp và xem enrollment của chính mình.
- Cho phép Admin quản lý toàn bộ; Teacher chỉ xem roster của lớp được phân công.

**Quyết định thiết kế đáng chú ý**

- Kiến trúc Controller → Service → Repository giúp tách API, nghiệp vụ và EF Core.
- Chặn lớp đầy và đăng ký trùng ở service; unique index `(StudentId, ClassId)` bảo vệ thêm ở database.
- Dùng `DeleteBehavior.Restrict` và kiểm tra quan hệ trước khi xóa để không làm mất dữ liệu dây chuyền.
- Tất cả input được kiểm tra bằng FluentValidation; response dùng envelope thống nhất.

**Endpoint minh họa — `POST /api/enrollments`**

```json
{
  "studentId": 1,
  "classId": 1
}
```

- Thành công: `201 Enrollment created`.
- Đăng ký trùng: `409 Student is already enrolled`.
- Lớp đầy: `409 Class is full`.
- Student thao tác tài khoản khác: `403 Forbidden`.

**Kết quả kiểm thử:** 25/25 test toàn solution vượt qua, gồm 16 test cho Department/Course/Class/Enrollment.

> Gợi ý trình bày: đặt sơ đồ `Department → Course → Class ← Enrollment → Student` bên trái và request/response minh họa bên phải.
