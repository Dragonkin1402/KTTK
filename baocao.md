# BÁO CÁO CHI TIẾT BÀI TẬP LỚN – KIẾN TRÚC VÀ THIẾT KẾ PHẦN MỀM

**Đề tài 5:** Xây dựng service quản lý cơ hội nghề nghiệp và học bổng cho sinh viên trong hệ thống Microservices (`Campus Opportunities`)  
**Giảng viên hướng dẫn:** ThS. Trần Vũ Đại  
**Nền tảng công nghệ:** .NET 8.0 LTS (ASP.NET Core Minimal API), YARP Reverse Proxy 2.1.0, MongoDB 8.0, RabbitMQ 3, Docker Compose, Azure Container Apps, TinyFish Web Agent

---

## 1. Tổng quan, Mục tiêu & Đối chiếu yêu cầu đề tài

Hệ thống **Campus Opportunities** giải quyết bài toán kết nối sinh viên với các cơ hội việc làm, thực tập và chương trình học bổng trong môi trường đại học theo kiến trúc **Microservices**. Thay vì xây dựng một khối nguyên khối (Monolithic) phụ thuộc chặt chẽ, hệ thống tách biệt rõ ràng từng miền nghiệp vụ (Bounded Context) để có thể phát triển, mở rộng và triển khai độc lập, đồng thời cung cấp bộ RESTful API chuẩn hóa sẵn sàng tích hợp với các hệ thống khác trong trường.

### 1.1. Bảng đối chiếu yêu cầu môn học & hệ sinh thái đề tài

| Yêu cầu / Phân hệ | Giải pháp đã triển khai thực tế trong dự án | Điểm chưa có / Hướng mở rộng |
| :--- | :--- | :--- |
| **1. Kiến trúc Microservices** | Tách thành 6 dịch vụ độc lập (`Gateway`, `OpportunityService`, `ScholarshipService`, `ApplicationService`, `NotificationService`, `CrawlerService`) dùng chung thư viện lõi `Campus.Shared`. | — |
| **2. Cung cấp API chuẩn** | Mọi giao tiếp đi qua **API Gateway (YARP)** tại `http://localhost:8080/api/*` (hoặc `:5000` local), chuẩn REST/JSON, bảo vệ bằng JWT & RBAC (`user`, `admin`). | Có thể bổ sung OpenAPI/Swagger UI tập trung tại Gateway. |
| **3. Quản lý dữ liệu bằng Database** | Mô hình **Database per Service** với **5 MongoDB 8.0 database độc lập** (`identity_db`, `opportunity_db`, `scholarship_db`, `application_db`, `notification_db`). Có công cụ `SqlToMongoMigration` di trú từ SQL Server legacy (`db/*.sql`). | — |
| **4. Giao diện Demo** | Single Page Application (SPA) tại `src/Gateway/wwwroot/index.html`, hỗ trợ Light/Dark Mode, phân tách rõ luồng Sinh viên và Quản trị viên. | — |
| **5. Design / Architectural Patterns** | API Gateway (YARP), Event-Driven Architecture (RabbitMQ Pub/Sub), CQRS (tách Command/Query), Database per Service, Atomic Counter (`MongoStore`), Document Model (`SearchText`), Startup Retry, Dependency Injection. | Có thể bổ sung Transactional Outbox Pattern cho độ tin cậy tuyệt đối. |
| **6. Tài liệu kiến trúc** | Đầy đủ C4 Model (Level 1, 2, 3), 4 Sequence Diagrams, Database ERD 5 DBs và Sơ đồ triển khai. | — |
| **7. Đối chiếu hệ sinh thái (COS, UMS, NS, AMS, MCDS)** | **COS** đầy đủ qua 3 service; **UMS** tích hợp tại Gateway (`identity_db`); **NS** qua `NotificationService`; **AMS** qua policy `AdminOnly` & tab Quản trị; cộng thêm `CrawlerService` (TinyFish). | Chưa có **MCDS** (upload file CV PDF đính kèm), chưa gửi Email/SignalR real-time ở NS, chưa tách UMS riêng cho role Nhà tuyển dụng. |

---

## 2. Cấu trúc Solution & Chi tiết từng Microservice (`.NET 8.0`)

Toàn bộ solution `CampusOpportunities.sln` biên dịch thành công trên `.NET 8.0 SDK` (`net8.0`) với **0 Warning, 0 Error**:

| Project | Loại ứng dụng | Cổng Local | Cổng Docker | Database riêng | Trách nhiệm chi tiết |
| :--- | :--- | :---: | :---: | :--- | :--- |
| **`Campus.Shared`** | Class Library | — | — | — | Chứa `CampusAuthentication` (JWT + Cookie), `EventBus` (RabbitMQ), `MongoStore` (Atomic Counter), `UserAccountDocument`. |
| **`Gateway`** | ASP.NET Core 8 + YARP | `5000` | `8080` | `identity_db` | Reverse Proxy định tuyến `/api/*`, xác thực đăng ký/đăng nhập, kiểm tra `/api/health`, phục vụ Web UI SPA. |
| **`OpportunityService`** | Minimal API | `5001` | `8080` (nội bộ) | `opportunity_db` | CRUD việc làm & thực tập (`job`/`internship`), tìm kiếm `SearchText`, lọc `open=true`, seed 6 mẫu, publish `opportunity.*`. |
| **`ScholarshipService`** | Minimal API | `5002` | `8080` (nội bộ) | `scholarship_db` | CRUD học bổng, tìm theo tên/nhà tài trợ, lọc `MinGpa <= gpa`, seed 5 mẫu, publish `scholarship.*`. |
| **`ApplicationService`** | Minimal API | `5003` | `8080` (nội bộ) | `application_db` | Nộp hồ sơ (Unique Index chống nộp trùng), xem hồ sơ theo quyền (`user`/`admin`), duyệt trạng thái, publish `application.*`. |
| **`NotificationService`** | Minimal API | `5004` | `8080` (nội bộ) | `notification_db` | Subscribe RabbitMQ (`#`) trên `notification.queue`, lưu thông báo broadcast & cá nhân, trả về tối đa 50 tin mới nhất. |
| **`CrawlerService`** | Minimal API | `5005` | `8080` (nội bộ) | — | Gọi TinyFish Web Agent (`/v1/automation/run`), bóc tách JSON linh hoạt, lọc trùng và POST về `Opportunity`/`Scholarship`. |
| **`BootstrapAdmin`** | Console CLI | — | — | `identity_db` | Tạo tài khoản Admin đầu tiên an toàn (nhập mật khẩu ẩn 12–128 ký tự). |
| **`SqlToMongoMigration`** | Console CLI | — | — | SQL → Mongo | Di trú dữ liệu từ SQL Server (`db/01..04_*.sql`) sang MongoDB và đồng bộ `idCounters`. |

---

## 3. Phân tích sâu các Architectural & Design Patterns (kèm mã nguồn)

### 3.1. API Gateway Pattern (`src/Gateway/Program.cs` & `appsettings.json`)
- Sử dụng **YARP Reverse Proxy 2.1.0** làm điểm vào duy nhất, ẩn toàn bộ cấu trúc mạng nội bộ (`http://opportunity-service:8080`, ...).
- Tự động cắt tiền tố `/api` khi chuyển tiếp (`"PathRemovePrefix": "/api"`).
- Áp dụng phân quyền sớm tại Gateway cho route crawler (`"AuthorizationPolicy": "AdminOnly"`), đồng thời cung cấp endpoint `/api/health` tổng hợp trạng thái `UP`/`DOWN` của cả 5 service phía sau.

### 3.2. Event-Driven Architecture – Pub/Sub (`src/Campus.Shared/EventBus.cs`)
- Giao tiếp bất đồng bộ qua **RabbitMQ Topic Exchange** tên `campus.events` (`durable: true`).
- Khi phát sự kiện (`Publish`), thông điệp được tuần tự hóa UTF-8 JSON, đánh dấu `Persistent = true` và đồng bộ luồng bằng `lock (_lock)` để an toàn trên kênh `IModel`:
  ```csharp
  public void Publish(string routingKey, object payload)
  {
      var body = JsonSerializer.SerializeToUtf8Bytes(payload,
          new JsonSerializerOptions(JsonSerializerDefaults.Web));
      lock (_lock)
      {
          var p = _ch.CreateBasicProperties();
          p.Persistent = true;
          _ch.BasicPublish("campus.events", routingKey, p, body);
      }
  }
  ```
- Phía tiêu thụ (`NotificationService`), hàng đợi `notification.queue` (`durable: true`) bind với pattern `#` để nhận toàn bộ các sự kiện: `opportunity.created`, `scholarship.created`, `application.submitted`, `application.status_changed`, và chỉ gửi `BasicAck` sau khi ghi xong vào `notification_db`.

### 3.3. CQRS (Command Query Responsibility Segregation)
- **Luồng Command (Ghi dữ liệu):** Sử dụng các record chuyên biệt (`OpportunityCommand`, `ScholarshipCommand`, `ApplyCommand`, `StatusCommand`) để kiểm tra hợp lệ đầu vào, ghi vào MongoDB và phát sự kiện nghiệp vụ.
- **Luồng Query (Đọc dữ liệu):** Tách biệt sang các DTO đọc (`OpportunityView`, `ScholarshipView`). Đặc biệt `OpportunityDocument` sử dụng kỹ thuật **Controlled Denormalization** tạo sẵn trường `SearchText` (ghép `Title + Company + Location` chữ thường) và `HasNoDeadline` để truy vấn/sắp xếp nhanh trên 1 collection mà không cần `$or` phức tạp.

### 3.4. Atomic Counter Pattern (`src/Campus.Shared/MongoStore.cs`)
- Thay vì dùng `ObjectId` dài khó đọc trên URL (`/api/opportunities/1`), mỗi DB MongoDB có thêm collection `idCounters`.
- Hàm `NextIdAsync` dùng lệnh nguyên tử `FindOneAndUpdateAsync` kết hợp toán tử `$inc: 1` và `IsUpsert = true` để cấp ID số nguyên tăng dần an toàn khi có nhiều request đồng thời.

### 3.5. Bảo mật JWT + HttpOnly Cookie & Chống nộp trùng
- **JWT Authentication (`CampusAuthentication.cs`):** Ký token bằng thuật toán HMAC-SHA256 (`Jwt:SigningKey >= 32 bytes`), lưu vào cookie `campus_auth` với cờ `HttpOnly = true` và `SameSite = SameSiteMode.Lax` (chống đánh cắp token qua XSS), thời hạn 8 giờ. Vẫn hỗ trợ header `Authorization: Bearer` cho hệ thống ngoài.
- **Chống nộp trùng (`ApplicationService/Program.cs`):** Tạo **Unique Compound Index** trên `(StudentId, ItemType, ItemId)`. Nếu sinh viên gửi trùng yêu cầu, MongoDB ném `DuplicateKey` và API trả `409 Conflict`.

### 3.6. Startup Resilience Pattern (`Startup.WaitFor`)
- Khi chạy cụm Docker Compose, các container .NET 8 có thể khởi động nhanh hơn RabbitMQ. `Startup.WaitFor` tự động thử kết nối lại tối đa 40 lần (mỗi lần nghỉ 3 giây) trước khi báo lỗi.

---

## 4. Tài liệu kiến trúc trực quan (C4 Model, Sequence & Database Diagram)

### 4.1. C4 Level 1 – Biểu đồ bối cảnh hệ thống (System Context)
Xác định ranh giới hệ thống với 2 nhóm người dùng chính (Sinh viên, Quản trị viên/Cán bộ) và 2 hệ thống ngoài (TinyFish Web Agent API, Hệ thống khác của trường):

![C4 Level 1 - System Context Diagram](diagrams/c4_level1_context.png)

### 4.2. C4 Level 2 – Biểu đồ thùng chứa (Container Diagram)
Mô tả chi tiết 6 container ứng dụng (.NET 8), Message Broker RabbitMQ và 5 cơ sở dữ liệu MongoDB cô lập theo từng service:

![C4 Level 2 - Container Diagram](diagrams/c4_level2_container.png)

### 4.3. C4 Level 3 – Biểu đồ thành phần (`OpportunityService`)
Đi sâu vào cấu trúc nội tại của một microservice chuẩn trong hệ thống gồm JWT Middleware, Command/Query Endpoints, `MongoStore`, `EventBus` và Composite Index:

![C4 Level 3 - Component Diagram](diagrams/c4_level3_component_opp.png)

### 4.4. Sequence Diagram – Luồng xác thực JWT HttpOnly Cookie qua Gateway
Minh họa chi tiết quá trình đăng nhập, băm/kiểm tra mật khẩu (`PasswordHasher`), phát hành JWT Cookie và kiểm tra policy `AdminOnly` tại Gateway trước khi forward request:

![Sequence Diagram - Auth JWT](diagrams/seq_auth_jwt.png)

### 4.5. Sequence Diagram – Sinh viên nộp hồ sơ & Nhận thông báo bất đồng bộ
Thể hiện sự phối hợp giữa giao tiếp đồng bộ (REST `POST /api/applications` trả về `201 Created` tức thì) và giao tiếp bất đồng bộ (RabbitMQ `application.submitted` → `NotificationService`):

![Sequence Diagram - Apply & Notify](diagrams/seq_apply_notify.png)

### 4.6. Sequence Diagram – Admin tạo cơ hội & Duyệt trạng thái hồ sơ
Minh họa cơ chế tạo thông báo **broadcast** (`StudentId = null`) khi có cơ hội/học bổng mới và thông báo **cá nhân hóa** (`StudentId` cụ thể) khi hồ sơ chuyển trạng thái (`REVIEWING`, `ACCEPTED`, `REJECTED`):

![Sequence Diagram - Create Opportunity & Review](diagrams/seq_create_opp.png)

### 4.7. Sequence Diagram – Thu thập dữ liệu tự động qua TinyFish Web Agent
`CrawlerService` gọi TinyFish API với `DefaultGoal`, bóc tách JSON qua hàm đệ quy `ExtractItems`, kiểm tra trùng tiêu đề trước khi `POST` (kèm chuyển tiếp `Authorization`/`Cookie`):

![Sequence Diagram - Crawler TinyFish](diagrams/seq_crawler_tinyfish.png)

### 4.8. Database Diagram (ERD phân tán – 5 MongoDB Databases)
Mỗi database độc lập hoàn toàn, không có Foreign Key vật lý, liên kết qua ID logic (`StudentId`, `ItemId`) và duy trì `idCounters` riêng:

![Database Diagram - 5 MongoDB Databases](diagrams/erd_database.png)

### 4.9. Kiến trúc Giao diện Web Demo SPA (`src/Gateway/wwwroot/index.html`)
Giao diện đơn trang hỗ trợ Light/Dark Mode, tự động ẩn/hiện phân hệ Quản trị & Crawler theo vai trò `role` từ `/api/auth/me`:

![UI Architecture](diagrams/ui_architecture.png)

### 4.10. Sơ đồ triển khai Cloud (Azure Container Apps & MongoDB Atlas)
Kịch bản `deploy/azure-deploy.sh` tự động đóng gói image lên Azure Container Registry (ACR), triển khai Container Apps Environment (chỉ Gateway có external ingress) và kết nối MongoDB Atlas:

![Deployment Architecture - Azure](diagrams/deploy_azure.png)

---

## 5. Đặc tả API chuẩn RESTful & Gói tin JSON mẫu

### 5.1. Bảng các Endpoint qua API Gateway (`http://localhost:8080`)

| Method | Endpoint | Phân quyền | Mô tả chức năng |
| :--- | :--- | :--- | :--- |
| `POST` | `/api/auth/register` | Public | Đăng ký tài khoản `user` (mật khẩu 12–128 ký tự) + set cookie `campus_auth`. |
| `POST` | `/api/auth/login` | Public | Đăng nhập bằng email/password + set cookie `HttpOnly` (hết hạn 8 giờ). |
| `GET` | `/api/auth/me` | Authenticated | Lấy thông tin tài khoản hiện tại (`id`, `name`, `email`, `role`). |
| `POST` | `/api/auth/logout` | Public | Đăng xuất (xóa cookie `campus_auth`, trả `204 No Content`). |
| `GET` | `/api/opportunities` | Public | Tìm kiếm việc làm/thực tập (`?q=&type=job\|internship&open=true`). |
| `GET` | `/api/opportunities/{id}` | Public | Xem chi tiết 1 cơ hội theo ID. |
| `POST/PUT/DELETE` | `/api/opportunities[/{id}]` | AdminOnly | Thêm/sửa/xóa cơ hội; phát event `opportunity.created/updated/deleted`. |
| `GET` | `/api/scholarships` | Public | Tìm kiếm học bổng (`?q=&gpa=3.0` lọc `MinGpa <= gpa`). |
| `GET` | `/api/scholarships/{id}` | Public | Xem chi tiết 1 học bổng theo ID. |
| `POST/PUT/DELETE` | `/api/scholarships[/{id}]` | AdminOnly | Thêm/sửa/xóa học bổng; phát event `scholarship.created/updated/deleted`. |
| `POST` | `/api/applications` | Authenticated | Sinh viên nộp hồ sơ (`itemType`, `itemId`, `itemTitle`). Trùng → `409 Conflict`. |
| `GET` | `/api/applications` | Authenticated | Sinh viên chỉ xem hồ sơ của mình; Admin xem tất cả hoặc lọc `?studentId=`. |
| `PATCH` | `/api/applications/{id}/status` | AdminOnly | Duyệt hồ sơ: `REVIEWING`, `ACCEPTED`, `REJECTED`; phát `application.status_changed`. |
| `GET` | `/api/notifications` | Authenticated | Lấy tối đa 50 thông báo mới nhất (broadcast `StudentId=null` + cá nhân). |
| `POST` | `/api/crawler/crawl` | AdminOnly | Thu thập tin tự động từ URL qua TinyFish (`{url, kind, limit}`). |
| `GET` | `/api/health` | Public | Kiểm tra trạng thái `UP`/`DOWN` của cả 5 microservice phía sau. |

### 5.2. Gói tin JSON mẫu

```json
// 1. POST /api/opportunities (AdminOnly) - Request
{
  "title": "Thực tập sinh Backend .NET",
  "company": "FPT Software",
  "type": "internship",
  "location": "Hà Nội",
  "description": "Thực tập ASP.NET Core, MongoDB, Docker.",
  "deadline": "2026-12-31"
}

// 2. POST /api/applications (Student) - Request & Response (201 Created)
Request:  { "itemType": "opportunity", "itemId": 1, "itemTitle": "Thực tập sinh Backend .NET" }
Response: {
  "id": 12,
  "studentId": "674a9f10c2e4b10012a34567",
  "studentName": "Nguyễn Văn A",
  "itemType": "opportunity",
  "itemId": 1,
  "itemTitle": "Thực tập sinh Backend .NET",
  "status": "SUBMITTED",
  "createdAt": "2026-11-19T04:15:00Z",
  "updatedAt": "2026-11-19T04:15:00Z"
}

// 3. POST /api/crawler/crawl (AdminOnly) - Response (200 OK)
{
  "source": "https://example.com/jobs",
  "kind": "opportunity",
  "steps": "4",
  "found": 5,
  "imported": 4,
  "skipped": 1,
  "failed": 0,
  "titles": ["Backend Intern", "Frontend Fresher", "QA Intern", "DevOps Intern"]
}
```

---

## 6. Kịch bản Demo 5 phút trước Giảng viên

1. **Bước 1 – Chứng minh cụm Microservices & Message Broker đang chạy:**
   - Mở `http://localhost:8080/api/health` → chỉ ra cả 5 service (`opp`, `sch`, `app`, `noti`, `crawler`) đều `UP`.
   - Mở RabbitMQ Management (`http://localhost:15672`, `guest`/`guest`) → chỉ ra Exchange `campus.events` (type `topic`) và Queue `notification.queue` đang có 1 consumer (`NotificationService`) lắng nghe.
2. **Bước 2 – Demo luồng Sinh viên (`http://localhost:8080`):**
   - Đăng ký 1 tài khoản sinh viên mới (mật khẩu >= 12 ký tự) → hệ thống tự động đăng nhập và cấp cookie `HttpOnly`.
   - Tìm kiếm cơ hội việc làm/thực tập theo từ khóa, lọc học bổng theo GPA → Lưu mục quan tâm → Bấm **Ứng tuyển**.
   - Bấm **Ứng tuyển lại lần 2** vào chính mục đó → hệ thống chặn lại và báo lỗi `409 Conflict` (*"Đã nộp hồ sơ cho mục này"*).
   - Mở tab **Thông báo** → thấy thông báo *"Đã nộp hồ sơ: ..."* vừa được sinh ra bất đồng bộ qua RabbitMQ.
3. **Bước 3 – Demo luồng Quản trị (`Admin`):**
   - Chạy `dotnet run --project tools/BootstrapAdmin` (nếu chưa tạo Admin) → Đăng nhập tài khoản Admin trên web (xuất hiện thêm tab **Quản trị** và **Thu thập TinyFish**).
   - Đăng 1 cơ hội mới hoặc duyệt đơn ứng tuyển của sinh viên sang trạng thái `ACCEPTED`.
   - Quay lại tài khoản sinh viên → vào tab **Thông báo** thấy thông báo duyệt hồ sơ cá nhân hóa và thông báo cơ hội mới.
   - *(Mở rộng)* Chạy thử **TinyFish Crawler** hoặc chạy thử các request trong `api.http` / `auth.http`.

---

## 7. Bộ câu hỏi phản biện thường gặp & Gợi ý trả lời

1. **Tại sao không dùng khóa ngoại (Foreign Key) giữa `applications` và `opportunities`/`scholarships`?**  
   *Trả lời:* Để tuân thủ nguyên tắc **Database per Service** trong Microservices. Mỗi service sở hữu DB riêng (`application_db`, `opportunity_db`, `scholarship_db`) giúp cô lập dữ liệu và mở rộng độc lập. Liên kết giữa các service được thực hiện qua **ID logic** (`ItemId`, `StudentId`) kết hợp phi chuẩn hóa nhẹ (`ItemTitle`, `StudentName`) để hiển thị nhanh mà không phải JOIN chéo DB.

2. **Nếu `NotificationService` bị tắt hoặc quá tải khi sinh viên nộp hồ sơ thì có mất thông báo không?**  
   *Trả lời:* Không. `ApplicationService` publish thông điệp với thuộc tính `Persistent = true` lên Exchange `campus.events` (`durable: true`), còn `notification.queue` cũng là `durable: true` và chỉ gửi `BasicAck` sau khi ghi xong vào `notification_db`. Tin nhắn sẽ nằm an toàn trong RabbitMQ và được xử lý ngay khi `NotificationService` hoạt động trở lại.

3. **Hệ thống xử lý tình huống sinh viên bấm nút "Ứng tuyển" liên tục nhiều lần (race condition / double-click) như thế nào?**  
   *Trả lời:* Ở tầng database (`ApplicationService/Program.cs`), hệ thống tạo **Unique Compound Index** trên bộ 3 trường `(StudentId, ItemType, ItemId)`. Mọi yêu cầu trùng lặp đều bị MongoDB từ chối với lỗi `DuplicateKey` và backend trả về `409 Conflict`.

4. **Cơ chế bảo mật JWT trong dự án chống tấn công XSS và giả mạo quyền ra sao?**  
   *Trả lời:* Token JWT (ký bằng HMAC-SHA256) được lưu trong cookie `campus_auth` có cờ **`HttpOnly = true`** nên mã JavaScript trên trình duyệt không thể đọc trộm được (chống XSS). Đồng thời, tất cả các endpoint quản trị đều kiểm tra policy `AdminOnly` ở tầng backend (và cả tại Gateway cho route crawler), tuyệt đối không chỉ ẩn nút trên giao diện.

5. **Tại sao lại chuyển từ SQL Server (`db/*.sql`) sang MongoDB 8.0 và làm sao giữ nguyên ID số nguyên (`1, 2, 3...`)?**  
   *Trả lời:* MongoDB Document Model phù hợp để lưu trữ dữ liệu bán cấu trúc (như kết quả crawl từ TinyFish hoặc trường `SearchText` nhúng sẵn). Để giữ ID số nguyên thân thiện và tương thích với dữ liệu SQL cũ, nhóm cài đặt **Atomic Counter Pattern** (`MongoStore.NextIdAsync`) dùng `FindOneAndUpdateAsync` với `$inc` trên collection `idCounters`, kèm công cụ `SqlToMongoMigration` để chuyển đổi dữ liệu an toàn.

6. **Nếu một hệ thống khác của trường muốn gọi API của nhóm thì kết nối như thế nào?**  
   *Trả lời:* Hệ thống ngoài chỉ cần gọi qua **API Gateway** (`http://localhost:8080/api/*`) với định dạng JSON chuẩn. Để xác thực, hệ thống ngoài có thể đăng nhập qua `/api/auth/login` hoặc truyền trực tiếp JWT qua header `Authorization: Bearer <token>` (vì `CampusAuthentication.cs` hỗ trợ cả Bearer header lẫn HttpOnly cookie).

7. **Điểm hạn chế hiện tại của dự án và hướng nâng cấp là gì?**  
   *Trả lời:* Hiện tại hệ thống chưa hỗ trợ đính kèm file CV PDF (cần tích hợp thêm **MCDS** lưu trữ trên Azure Blob/S3), thông báo chưa đẩy real-time qua SignalR/WebSocket hoặc gửi Email, và phân hệ người dùng đang gộp tại Gateway. Hướng phát triển tiếp theo là tách `UserManagementService` độc lập hỗ trợ role Nhà tuyển dụng, bổ sung upload CV và áp dụng **Transactional Outbox Pattern**.
