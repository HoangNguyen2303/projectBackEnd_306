# Báo cáo đóng góp — Vũ Thế Phương

## 1. Phạm vi phụ trách

Theo tài liệu dự án, Thế Phương phụ trách vertical slice **Course, Class & Enrollment**: DTO, validation, service, repository, controller, cấu hình EF Core, migration và unit test. Các quy tắc chính gồm giới hạn sĩ số, không đăng ký trùng, hủy đăng ký để giải phóng chỗ, phân quyền Admin/Student/Teacher-owner và response envelope thống nhất.

## 2. Phần đã push trong commit gốc

- Commit: `b99f93e` — `feat: add course class and enrollment management`.
- Tác giả: `Phuong Vu <phuong.vuthe.cit22@eiu.edu.vn>`.
- Quy mô: 31 file, 2.428 dòng thêm và 7 dòng xóa.
- Trạng thái tích hợp: commit đã được merge vào nhánh `develop` của nhóm.

### Course

- Tạo `CourseDto`, `CreateCourseDto`, `UpdateCourseDto`, `CourseQueryDto`.
- Tạo FluentValidation cho mã môn, tên môn, số tín chỉ, khoa và tham số phân trang.
- Tạo `ICourseService`, `ICourseRepository` và triển khai service/repository.
- Cài đặt danh sách có tìm kiếm, lọc theo khoa và phân trang; lấy chi tiết; tạo; sửa; xóa.
- Chuẩn hóa mã môn thành chữ hoa.
- Chặn mã môn trùng, khoa không tồn tại và xóa môn đang có lớp học phần.
- Public được xem danh sách/chi tiết; chỉ Admin được tạo, sửa, xóa.

### Class section

- Tạo DTO cho truy vấn, tạo/sửa lớp, thông tin lớp, roster và sinh viên trong roster.
- Tạo validator, service/repository interface và phần triển khai.
- Hỗ trợ tìm kiếm; lọc theo môn, giảng viên, học kỳ; phân trang.
- Trả về số sinh viên đã đăng ký, số chỗ còn lại và trạng thái đầy lớp.
- Chặn mã lớp trùng, Course/Teacher không tồn tại, giảm `MaxCapacity` thấp hơn số người đã đăng ký và xóa lớp đang có enrollment.
- Public được xem lớp; Admin quản lý CRUD; Admin hoặc đúng giảng viên phụ trách mới được xem roster.

### Enrollment

- Tạo DTO, validator, service/repository interface và phần triển khai.
- Cài đặt đăng ký lớp và hủy đăng ký.
- Chặn lớp đầy và một sinh viên đăng ký cùng lớp hai lần.
- Student chỉ được thao tác enrollment của chính mình; Admin được thao tác thay sinh viên.
- Gán thời điểm đăng ký bằng UTC và trả thông tin Student/Class/Course trong response.

### EF Core, database và Dependency Injection

- Bổ sung độ dài/required cho `CourseCode`, `Course.Name`, `ClassCode`, `Semester`.
- Bổ sung check constraint: `Credits` từ 1 đến 10, `MaxCapacity` lớn hơn 0.
- Dùng unique index `(StudentId, ClassId)` để bảo vệ khỏi đăng ký trùng ở mức database.
- Đặt quan hệ Course–Class và Teacher–Class là `DeleteBehavior.Restrict` để tránh xóa dây chuyền/multiple cascade paths.
- Tạo migration `CourseClassEnrollmentConstraints` và cập nhật model snapshot.
- Đăng ký Course/Class/Enrollment services, validators và repositories vào DI container; nối `AddApplication()` trong API.

### Unit test trong commit gốc

- Lớp đầy trả `409 Conflict`.
- Đăng ký trùng trả `409 Conflict`.
- Hủy đăng ký giải phóng chỗ và có thể đăng ký lại.

## 3. Phần hoàn thiện trong nhánh fix

Nhánh: `feature/ThePhuong-Course-Class-Enrollment-fixes`, tạo từ `origin/develop` mới nhất.

- Bổ sung `GET /api/enrollments` vì tài liệu cho phép Student xem enrollment của chính mình nhưng code cũ chỉ có POST/DELETE.
- Endpoint mới hỗ trợ `studentId`, `classId`, `semester`, `page`, `pageSize`.
- Student luôn bị giới hạn theo `UserId` của chính mình; cố xem Student khác trả `403`.
- Admin được xem toàn bộ hoặc lọc theo Student; Student không tồn tại trả `404`.
- Repository dùng `AsNoTracking`, eager loading Student → Class → Course, sắp xếp và phân trang tại database.
- Thay file `EduTrack.Api.http` lỗi thời bằng kịch bản gọi thật cho Auth, Course, Class, roster và Enrollment.
- Bổ sung 7 unit test: xem enrollment của chính mình; chặn xem/thao tác Student khác; chặn trùng mã Course; chặn xóa Course có Class; chặn giảm capacity; chặn Teacher xem roster không thuộc mình.
- Sau thay đổi: toàn solution có **19/19 test pass**.

## 4. Điểm còn thiếu ở cấp độ toàn dự án

Các mục sau không thuộc riêng module của Thế Phương và nên được phân công/giải quyết trước khi nộp cuối kỳ:

- Chưa có integration test cho luồng Admin tạo Student → Student enroll → Teacher grade → Student xem grade/GPA; hiện chỉ có unit test.
- `DbSeeder` mới tạo ba tài khoản đăng nhập, chưa seed đủ Department/Course/Class/Student/Enrollment/Grade như tài liệu yêu cầu và chưa bảo đảm tài khoản Student/Teacher được liên kết với profile tương ứng.
- Đã bổ sung Postman collection cho Department/Course/Class/Enrollment; nhóm vẫn cần hợp nhất các module khác để có collection chung cho toàn bộ API.
- `Program.cs` chưa có global exception middleware/response wrapper cho lỗi ngoài dự kiến.
- Build còn cảnh báo nullable trong `TeacherService` và cảnh báo phương thức `async` không có `await` trong `UserService`.
- API Grades/Auth hiện còn một số route khác hợp đồng trong tài liệu; nhóm nên thống nhất tài liệu hoặc sửa route trước demo.
- Kiểm tra capacity hiện đáp ứng bài toán thông thường, nhưng môi trường production có nhiều request đồng thời nên bổ sung transaction/optimistic concurrency để chống vượt sĩ số do race condition.

## 5. Kịch bản demo đề xuất

1. Đăng nhập Admin và lấy JWT.
2. Tạo Course rồi tạo Class với `MaxCapacity = 1`.
3. Đăng ký Student thứ nhất: nhận `201`.
4. Đăng ký Student thứ hai: nhận `409 Class is full`.
5. Đăng ký Student thứ nhất lần nữa: nhận `409 already enrolled`.
6. Dùng Student token xem `GET /api/enrollments`: chỉ thấy dữ liệu của mình.
7. Dùng Student token và `studentId` của người khác: nhận `403`.
8. Hủy enrollment rồi đăng ký lại để chứng minh chỗ đã được giải phóng.
9. Dùng Teacher đúng lớp xem roster; dùng Teacher khác để chứng minh trả `403`.

## 6. Git từng bước để đưa thay đổi lên dự án

Chạy lệnh tại thư mục repository:

```powershell
cd D:\CSW_306\Project\EduTrack
git switch feature/ThePhuong-Course-Class-Enrollment-fixes
git status
dotnet restore
dotnet test EduTrack.sln --no-restore
```

Chỉ stage các file thuộc thay đổi này, sau đó kiểm tra lại:

```powershell
git add EduTrack.Api/EduTrack.Api.http
git add EduTrack.Api/Controllers/EnrollmentsController.cs
git add EduTrack.Application/DTOs/Enrollments/EnrollmentDtos.cs
git add EduTrack.Application/Interfaces/IEnrollmentRepository.cs
git add EduTrack.Application/Interfaces/IEnrollmentService.cs
git add EduTrack.Application/Services/EnrollmentService.cs
git add EduTrack.Application/Validators/EnrollmentValidators.cs
git add EduTrack.Infrastructure/Repositories/EnrollmentRepository.cs
git add EduTrack.Tests/Unit/EnrollmentServiceTests.cs
git add EduTrack.Tests/Unit/CourseClassServiceTests.cs
git add docs/ThePhuong_Contribution_Report.md
git diff --cached
```

Nếu diff đúng, commit và push nhánh:

```powershell
git commit -m "feat: add enrollment history and strengthen module tests"
git push -u origin feature/ThePhuong-Course-Class-Enrollment-fixes
```

Trên GitHub, tạo Pull Request với:

- Base: `develop`.
- Compare: `feature/ThePhuong-Course-Class-Enrollment-fixes`.
- Tiêu đề: `feat: complete Course, Class and Enrollment module`.
- Nội dung: tóm tắt endpoint mới, authorization, test mới và kết quả `19/19`.
- Nhờ ít nhất một thành viên review; không push thẳng vào `develop` hoặc `main`.

## 7. Hoàn thành phân công Meeting 4 — Departments CRUD

- Bổ sung đầy đủ `GET/POST/PUT/DELETE /api/departments`; public được đọc và chỉ Admin được ghi dữ liệu.
- Danh sách Department hỗ trợ tìm kiếm, phân trang và trả số lượng Student, Teacher, Course trực thuộc.
- Chuẩn hóa mã khoa thành chữ hoa; kiểm tra mã trùng, độ dài Code tối đa 10 và Name tối đa 120 ký tự.
- Chặn xóa Department đang có Student, Teacher hoặc Course bằng cả business rule và `DeleteBehavior.Restrict`.
- Bổ sung DTO, FluentValidation, service/repository interfaces, implementations, controller và Dependency Injection.
- Tạo migration `AddDepartmentManagementConstraints` và cập nhật EF Core model snapshot.
- Bổ sung 6 unit test cho validation, duplicate code, create/update/delete và kiểm tra dữ liệu liên quan.
- Cập nhật `EduTrack.Api.http`, thêm Postman collection của module và nội dung một slide thuyết trình.
- Sau Meeting 4: toàn solution có **25/25 test pass**, trong đó 16 test thuộc Department/Course/Class/Enrollment.
