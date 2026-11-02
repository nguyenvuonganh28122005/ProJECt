# Personal CV Builder – phiên bản mở rộng

Ứng dụng React + Vite tạo CV cá nhân, lưu bản nháp bằng LocalStorage và xuất PDF.

## Cài đặt và chạy
1. Cài Node.js bản LTS.
2. Mở thư mục dự án trong VS Code.
3. Chạy `npm install`.
4. Chạy `npm run dev`.
5. Mở địa chỉ localhost mà Vite hiển thị.

## Chức năng bắt buộc
- Nhập thông tin cá nhân, kiểm tra định dạng email.
- Thêm, sửa, xóa học vấn, kỹ năng và dự án.
- Chọn 2 mẫu CV.
- Xem trước cập nhật trực tiếp.
- Tự động lưu và mở lại bản nháp bằng LocalStorage.
- Xuất PDF qua chức năng in của trình duyệt.

## Chức năng mở rộng đã bổ sung
1. **Ảnh đại diện:** chọn ảnh từ máy; chỉ nhận tệp hình ảnh tối đa 2MB; ảnh xuất hiện trong bản xem trước và PDF.
2. **Tùy chỉnh màu chủ đạo:** chọn màu bằng color picker; màu được áp dụng cho tiêu đề và điểm nhấn CV.
3. **Xuất/Nhập JSON:** nút Xuất JSON tải dữ liệu CV thành file `.json`; nút Nhập JSON đọc file và khôi phục nội dung, mẫu và màu. File sai cấu trúc sẽ bị từ chối và dữ liệu hiện tại được giữ nguyên.

## Lưu ý
- Dữ liệu lưu trong LocalStorage của trình duyệt hiện tại, không đồng bộ sang thiết bị khác.
- Ảnh được lưu dưới dạng Data URL trong LocalStorage; nên dùng ảnh nhỏ (tối đa 2MB).
- Khi xuất PDF, chọn "Save as PDF" trong hộp thoại in.

## Kiểm thử
1. Nhập CV, thêm ảnh đại diện, đổi màu, đổi mẫu; tải lại trang và xác nhận các thay đổi còn nguyên.
2. Xuất file JSON; xóa hoặc thay đổi CV; nhập lại file JSON và xác nhận dữ liệu được khôi phục.
3. Chọn một file JSON không đúng cấu trúc; ứng dụng báo lỗi và giữ nguyên dữ liệu đang có.
4. Thêm hai dự án, đổi sang mẫu 02, xóa một dự án, tải lại trang; mẫu 02 và dự án còn lại vẫn được giữ.
5. Nhập email sai định dạng và kiểm tra thông báo lỗi.
6. Xuất PDF, kiểm tra ảnh và màu sắc trên bản in.
