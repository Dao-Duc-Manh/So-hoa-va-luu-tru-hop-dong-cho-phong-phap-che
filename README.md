Số hóa và lưu trữ hợp đồng cho phòng pháp chế
<img width="1616" height="558" alt="image" src="https://github.com/user-attachments/assets/465191b9-6ed4-4fcb-b904-64c5fb4f095b" />
Hệ Thống Quản Lý Hợp Đồng (ChinhSachSo)
Giới Thiệu
Ứng dụng web ASP.NET Core để quản lý hợp đồng, hỗ trợ tải lên, xem, chỉnh sửa, xóa tệp (PDF, JPG, JPEG, PNG) với trích xuất văn bản OCR bằng Tesseract và lưu trữ trên SQL Server.

Cài Đặt Môi Trường

Yêu cầu: .NET 8 SDK, SQL Server, Tesseract OCR (với tessdata chứa vie.traineddata, eng.traineddata), ImageMagick.
Cài đặt:
Sao chép kho lưu trữ: git clone <url>.
Cấu hình chuỗi kết nối trong appsettings.json.
Chạy migrations: dotnet ef migrations add InitialCreate và dotnet ef database update.
Đặt thư mục tessdata trong dự án.
Cài gói NuGet: dotnet restore.

Tính Năng

Tải lên hợp đồng (PDF/ảnh, tối đa 10MB) với trích xuất OCR.
Xem, chỉnh sửa, xóa hợp đồng (riêng lẻ hoặc toàn bộ).
Tìm kiếm và đánh dấu từ khóa trong văn bản OCR.
Lưu trữ tệp trong wwwroot/uploads/temp và dữ liệu trong SQL Server.

Chạy Dự Án
dotnet run

Truy cập: https://localhost:5001/Contract.
Kết Quả

Tải lên và trích xuất văn bản từ tệp hợp đồng.
Quản lý hợp đồng qua giao diện web (xem, sửa, xóa).
Tìm kiếm từ khóa trong OCR để chỉnh sửa dễ dàng.

Kết Luận
Ứng dụng cung cấp giải pháp quản lý hợp đồng hiệu quả với OCR, dễ mở rộng để thêm xác thực, xem trước tệp hoặc tìm kiếm nâng cao.
