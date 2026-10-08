# LAB 2 – Two-Tier Architecture (SQL Server + Dapper, MongoDB Atlas)

| Project | Nội dung | Cơ sở dữ liệu |
|---|---|---|
| `TodoAppV2` | Bài thực hành: Todo + Dapper | SQL Server (Docker trên macOS) |
| `StudentAppMongo` | Bài tập: quản lý sinh viên | MongoDB Atlas (cloud, miễn phí) |

Kiến trúc: `UI → Service → Repository → Database` (Database nằm ở một máy/dịch vụ riêng, nên gọi là Two-Tier: Client app + DB server).

```
LAB/
├── TodoAppV2/
│   ├── TodoAppV2.csproj        # Dapper + Microsoft.Data.SqlClient
│   ├── Program.cs              # Đọc chuỗi kết nối, ghép Repository → Service → UI
│   ├── Todo.cs                 # Model
│   ├── TodoRepository.cs       # DATA  : Dapper + SqlConnection
│   ├── ITodoService.cs         # Interface của tầng Logic
│   ├── TodoService.cs          # LOGIC
│   ├── TodoUI.cs               # UI
│   └── database.sql            # Script tạo DB/bảng (tuỳ chọn, app tự tạo)
└── StudentAppMongo/
    ├── StudentAppMongo.csproj  # MongoDB.Driver
    ├── Program.cs              # Đọc MONGODB_URI, ping, chạy UI
    ├── Student.cs              # Model (Id = ObjectId)
    ├── StudentRepository.cs    # DATA  : IMongoCollection<Student>
    ├── StudentService.cs       # LOGIC : kiểm tra dữ liệu
    └── StudentUI.cs            # UI
```

---

## PHẦN A. TodoAppV2 – SQL Server trên macOS

Chuỗi kết nối trong PDF dùng `Integrated Security=true` (đăng nhập bằng tài khoản Windows) nên **không dùng được trên macOS**. Cách làm: chạy SQL Server trong Docker và đăng nhập bằng tài khoản `sa`.

### 1. Cài Docker Desktop

Tải tại docker.com/products/docker-desktop. Máy chip Apple M1/M2/M3: vào Docker Desktop → Settings → General, bật **Use Rosetta for x86_64/amd64 emulation on Apple Silicon** (nếu có).

### 2. Chạy SQL Server

```bash
docker run --platform linux/amd64 \
  -e "ACCEPT_EULA=Y" \
  -e "MSSQL_SA_PASSWORD=YourStrong@Passw0rd" \
  -p 1433:1433 --name sqlserver \
  -d mcr.microsoft.com/mssql/server:2022-latest
```

Mật khẩu cần đủ mạnh (tối thiểu 8 ký tự, gồm chữ hoa, chữ thường, số, ký tự đặc biệt). Chờ khoảng 20–30 giây cho server khởi động; kiểm tra bằng `docker logs sqlserver`.

Các lần sau chỉ cần:

```bash
docker start sqlserver
```

### 3. Chạy ứng dụng

```bash
cd LAB/TodoAppV2
dotnet restore
dotnet run
```

Ứng dụng tự tạo database `TodoDB` và bảng `Todos` ở lần chạy đầu. Chuỗi kết nối mặc định:

```
Server=localhost,1433;Database=TodoDB;User Id=sa;Password=YourStrong@Passw0rd;TrustServerCertificate=true
```

Nếu bạn đặt mật khẩu khác, đặt biến môi trường trước khi chạy:

```bash
export TODO_DB="Server=localhost,1433;Database=TodoDB;User Id=sa;Password=MatKhauCuaBan;TrustServerCertificate=true"
dotnet run
```

### 4. Xem dữ liệu trong DB (tuỳ chọn)

Dùng **Azure Data Studio** hoặc extension **SQL Server (mssql)** của VS Code, kết nối `localhost,1433`, user `sa`.

### Điểm khác so với code trong PDF

- `Todo` dùng namespace `TodoAppV2` (PDF để `TodoApp` ở file Todo nên bị lệch namespace với các file còn lại).
- `AddAsync` dùng `OUTPUT INSERTED.Id` để trả về Id; code PDF gọi `ExecuteScalarAsync<int>` với câu `INSERT` thường nên không lấy được Id.
- `GetAsync` dùng `QueryFirstOrDefaultAsync` để không văng exception khi không có Id; Service/UI báo "Không tìm thấy công việc".
- Đổi `addASync` thành `AddAsync` cho đúng quy ước; `TodoUI` nhận `ITodoService` thay vì class cụ thể.
- Dùng `ORDER BY Id` để danh sách luôn đúng thứ tự.

---

## PHẦN B. StudentAppMongo – liên kết MongoDB Atlas (web)

MongoDB Atlas là dịch vụ MongoDB trên cloud, có gói miễn phí (M0) đủ cho bài tập. Giao diện web có thể thay đổi chút theo thời gian, nhưng các bước chính như sau.

### Bước 1. Tạo tài khoản và cluster

1. Vào **mongodb.com/atlas** → **Try Free** → đăng ký (Google hoặc email).
2. Tạo một **Cluster miễn phí (Free / M0)**, chọn nhà cung cấp và vùng gần bạn (ví dụ Singapore). Đặt tên tuỳ ý.

### Bước 2. Tạo Database User

1. Menu trái: **Security → Database Access** → **Add New Database User**.
2. Chọn **Password**, nhập username (ví dụ `labuser`) và mật khẩu.
3. Nên dùng mật khẩu chỉ gồm chữ và số. Nếu có ký tự đặc biệt (`@`, `:`, `/`, `#`…), phải mã hoá URL (ví dụ `@` → `%40`) khi đưa vào chuỗi kết nối.
4. Quyền: **Read and write to any database** → **Add User**.

### Bước 3. Cho phép IP truy cập (Network Access)

1. Menu trái: **Security → Network Access** → **Add IP Address**.
2. Chọn **Add Current IP Address** (an toàn hơn), hoặc `0.0.0.0/0` (cho phép mọi IP, chỉ nên dùng khi làm bài lab rồi xoá sau).
3. Nếu đổi mạng (nhà → trường), IP thay đổi, cần thêm lại IP mới, nếu không sẽ bị lỗi timeout.

### Bước 4. Lấy chuỗi kết nối

1. Vào **Database** → bấm **Connect** ở cluster → **Drivers**.
2. Chọn **C# / .NET**, copy chuỗi dạng:

```
mongodb+srv://labuser:<db_password>@cluster0.xxxxx.mongodb.net/?retryWrites=true&w=majority
```

3. Thay `<db_password>` bằng mật khẩu thật (bỏ luôn cặp dấu `< >`).

### Bước 5. Chạy ứng dụng

Đặt chuỗi kết nối vào biến môi trường (dùng dấu nháy đơn `' '`):

```bash
cd LAB/StudentAppMongo
export MONGODB_URI='mongodb+srv://labuser:MatKhau123@cluster0.xxxxx.mongodb.net/?retryWrites=true&w=majority'
dotnet restore
dotnet run
```

Nếu chưa đặt biến, ứng dụng sẽ hỏi bạn dán chuỗi kết nối. Khi chạy đúng sẽ thấy `Kết nối thành công!`.

Không ghi mật khẩu thẳng vào code hoặc đẩy lên GitHub. Nếu lỡ lộ, vào Atlas đổi mật khẩu Database User.

### Bước 6. Xem dữ liệu trên web

Trong Atlas: **Database → Browse Collections** → database `StudentDB` → collection `students`. Dữ liệu thêm từ ứng dụng sẽ xuất hiện ở đây. Database và collection tự tạo khi thêm sinh viên đầu tiên, không cần tạo trước.

Muốn xem bằng ứng dụng desktop: cài **MongoDB Compass**, dán cùng chuỗi kết nối.

### Lưu ý về Id

MongoDB tự sinh `_id` kiểu **ObjectId** (24 ký tự, ví dụ `66f1a2b3c4d5e6f7a8b9c0d1`). Khi sửa/xoá/tìm theo Id, copy Id từ bảng hiển thị (menu 1) rồi dán vào.

### Chức năng

| Phím | Chức năng |
|---|---|
| 1 | Hiển thị danh sách |
| 2 | Thêm sinh viên |
| 3 | Sửa (Enter để giữ giá trị cũ) |
| 4 | Xoá (có xác nhận y/n) |
| 5 | Tìm theo Id |
| 6 | Tìm theo Tên (chứa từ khoá, không phân biệt hoa thường) |
| 7 | Tìm theo Địa chỉ |
| 8 | Tìm theo Điểm |
| 0 | Thoát |

Việc lọc được đẩy xuống MongoDB (regex và so sánh bằng) thay vì tải hết về rồi lọc trong C#.

---

## Xử lý sự cố

| Lỗi | Nguyên nhân / Cách xử lý |
|---|---|
| `Login failed for user 'sa'` | Sai mật khẩu; kiểm tra `MSSQL_SA_PASSWORD` hoặc biến `TODO_DB` |
| `A network-related error ... 1433` | Container chưa chạy hoặc chưa khởi động xong: `docker ps`, `docker logs sqlserver` |
| Container SQL Server tự tắt | Mật khẩu `sa` không đủ mạnh, hoặc thiếu RAM (cần tối thiểu ~2 GB cho Docker) |
| `Timeout ... server selection` (MongoDB) | IP chưa được thêm ở Network Access, hoặc mạng chặn cổng 27017 |
| `Authentication failed` (MongoDB) | Sai username/mật khẩu, hoặc mật khẩu có ký tự đặc biệt chưa mã hoá URL |
| Không resolve được `mongodb+srv` | Mạng/DNS lỗi; thử đổi mạng (4G, Wi-Fi khác) |
| `dotnet restore` lỗi NuGet | Kiểm tra Internet; chạy lại `dotnet restore` |
