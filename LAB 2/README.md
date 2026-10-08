# LAB 1 – One-Tier Architecture (C# Console, .NET 7)

Bài thực hành môn **Software Architecture and Design**: xây dựng ứng dụng Console theo kiến trúc **One-Tier**, tách code thành 3 phần **UI – Logic – Data**, đọc/ghi dữ liệu ra file.

> **Lab 2** (SQL Server + Dapper, MongoDB Atlas): xem file [README_LAB2.md](README_LAB2.md).

Thư mục này gồm 2 project độc lập cho Lab 1:

| Project | Nội dung | Dữ liệu lưu tại |
|---|---|---|
| `TodoApp` | Bài thực hành mẫu: quản lý danh sách công việc | `todos.txt` |
| `StudentApp` | Bài tập: quản lý sinh viên | `students.txt` |

## Mục tiêu

1. Xây dựng ứng dụng console theo kiến trúc One-Tier.
2. Ôn tập lập trình hướng đối tượng và xử lý file trong C#.

## Yêu cầu môi trường

- macOS (Apple Silicon hoặc Intel)
- [.NET 7 SDK](https://dotnet.microsoft.com/download/dotnet/7.0)
- Visual Studio Code + extension **C# Dev Kit** (hoặc **C#** của Microsoft)

Kiểm tra cài đặt:

```bash
dotnet --list-sdks     # phải có dòng 7.0.xxx
```

> Lưu ý: .NET 7 đã hết hỗ trợ chính thức (05/2024) nhưng vẫn cài và chạy bình thường cho bài lab.

## Cấu trúc thư mục

```
LAB/
├── README.md
├── TodoApp/
│   ├── TodoApp.csproj
│   ├── Program.cs             # Điểm khởi chạy
│   ├── Todo.cs                # Model
│   ├── TodoRepository.cs      # DATA  : đọc/ghi todos.txt
│   ├── TodoService.cs         # LOGIC
│   └── TodoUI.cs              # UI    : menu, nhập/xuất
└── StudentApp/
    ├── StudentApp.csproj
    ├── Program.cs             # Điểm khởi chạy
    ├── Student.cs             # Model
    ├── StudentRepository.cs   # DATA  : đọc/ghi students.txt
    ├── StudentService.cs      # LOGIC : kiểm tra dữ liệu + tìm kiếm
    └── StudentUI.cs           # UI    : menu, nhập/xuất
```

## Kiến trúc

Luồng gọi đi một chiều, UI không gọi thẳng xuống Data:

```
Program → UI → Service → Repository → File (.txt)
          (UI)  (Logic)    (Data)
```

| Tầng | Trách nhiệm | Không được làm |
|---|---|---|
| **UI** | Hiển thị menu, nhận input, in kết quả | Chứa luật nghiệp vụ, đọc/ghi file |
| **Logic (Service)** | Kiểm tra dữ liệu hợp lệ, tìm kiếm, điều phối | Dùng `Console`, thao tác file trực tiếp |
| **Data (Repository)** | Đọc/ghi file, thêm/sửa/xoá trên danh sách | Kiểm tra nghiệp vụ, in ra màn hình |

## Cách chạy

```bash
cd LAB/TodoApp        # hoặc LAB/StudentApp
dotnet run
```

Chạy bằng Terminal thật (Terminal của macOS hoặc iTerm2), **không** dùng Debug Console của VS Code vì `Console.Clear()` và `ReadLine()` cần terminal thật.

Build rồi chạy file đã build:

```bash
dotnet build
dotnet bin/Debug/net7.0/TodoApp.dll      # hoặc StudentApp.dll
```

## TodoApp

Menu:

| Phím | Chức năng |
|---|---|
| 1 | Thêm Todo |
| 2 | Xoá Todo |
| 3 | Đánh dấu hoàn thành (bật/tắt) |
| 4 | Sửa nội dung |
| 0 | Thoát |

Định dạng `todos.txt` (`Id|IsCompleted|Title`):

```
1|True|Learning software architecture
2|False|Làm bài tập quản lý sinh viên
```

## StudentApp

Model `Student`: `Id`, `Name`, `Email`, `Address`, `Age`, `Grade`.

Menu:

| Phím | Chức năng | Ghi chú |
|---|---|---|
| 1 | Hiển thị danh sách | Dạng bảng, có tổng số |
| 2 | Thêm sinh viên | Có kiểm tra dữ liệu |
| 3 | Sửa sinh viên | Nhấn Enter để giữ giá trị cũ |
| 4 | Xoá sinh viên | Có hỏi xác nhận `y/n` |
| 5 | Tìm theo Id | Khớp chính xác |
| 6 | Tìm theo Tên | Chứa từ khoá, không phân biệt hoa thường |
| 7 | Tìm theo Địa chỉ | Chứa từ khoá, không phân biệt hoa thường |
| 8 | Tìm theo Điểm | Chấp nhận `8.5` và `8,5` |
| 0 | Thoát | |

Quy tắc kiểm tra dữ liệu (trong `StudentService`):

- Tên và địa chỉ không được để trống
- Email đúng định dạng `abc@domain.xyz`
- Tuổi từ 1 đến 120
- Điểm từ 0 đến 10
- Không dùng ký tự `|` (là dấu phân cách khi lưu file)

Định dạng `students.txt` (`Id|Name|Email|Address|Age|Grade`):

```
1|Nguyễn Văn An|an@gmail.com|Hà Nội|20|8.5
2|Trần Thị Bình|binh@gmail.com|Đà Nẵng|21|7.25
```

Điểm được lưu theo `InvariantCulture` (dấu `.`) nên file đọc đúng trên mọi máy.

## Kịch bản kiểm tra nhanh

1. Chạy ứng dụng, thêm 3 bản ghi.
2. Sửa 1 bản ghi, xoá 1 bản ghi.
3. Thoát (`0`), chạy lại và kiểm tra dữ liệu còn nguyên.
4. (StudentApp) Thử tìm theo Id, Tên, Địa chỉ, Điểm.
5. (StudentApp) Thử nhập sai: email không có `@`, tuổi `-5`, điểm `11` và xác nhận ứng dụng báo lỗi.

## Khác biệt so với hướng dẫn Visual Studio 2022 trong PDF

- **Vị trí file dữ liệu:** `dotnet run` chạy với thư mục hiện tại là thư mục project, nên `todos.txt` / `students.txt` nằm cạnh `Program.cs`, không nằm trong `bin/Debug/net7.0/`. Chạy trực tiếp file `.dll` trong `bin` thì file nằm ở đó.
- **Không cần các dòng `using System...`:** .NET 7 bật sẵn `ImplicitUsings`.
- **Dùng `string?`:** template .NET 7 bật Nullable nên `Console.ReadLine()` trả về `string?`.

## Xử lý sự cố

| Vấn đề | Cách xử lý |
|---|---|
| `dotnet: command not found` | Cài .NET 7 SDK rồi mở lại Terminal |
| Lỗi `framework net7.0 not found` | Cài đúng bản SDK 7.0 (Arm64 cho chip M1/M2/M3, x64 cho Mac Intel) |
| Hiển thị tiếng Việt bị lỗi font | Dùng Terminal macOS hoặc iTerm2 (hỗ trợ UTF-8) |
| `dotnet run` báo nhiều project | Chạy lệnh trong đúng thư mục `TodoApp` hoặc `StudentApp` |
| Muốn xoá dữ liệu cũ | Xoá file `todos.txt` / `students.txt` |

## Thông tin

- Môn học: Software Architecture and Design
- Giảng viên: Trần Vũ Đại
- Công nghệ: C# / .NET 7.0 Console App
