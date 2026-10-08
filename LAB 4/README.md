# LAP 4 – Event-Driven Architecture

## Đã làm
1. **TransferMoneyEvent** (`Events/TransferMoneyEvent.cs`) + `AccountService.Transfer(from, to, amount)`.
2. **FraudDetectionService** (`Services/FraudDetectionService.cs`) subscribe `MoneyWithdrawnEvent`, cảnh báo khi rút > 10000.
3. **Thay Event Channel in-memory bằng RabbitMQ**: `IEventBus` + `InMemoryEventBus` + `RabbitMqEventBus`.

## Sửa lỗi bản gốc
- `MoneyWithdrawnEvent` thiếu `: IEvent`.
- Implement `Withdraw` (kiểm tra số dư, publish event); bỏ `WithDrawn` ném NotImplementedException.
- `HandleWithDrawn` sai kiểu event -> sửa thành `MoneyWithdrawnEvent`.
- Xóa `AccountDepositedEvent` (class thừa); đổi `MoneyDepositedEvnet` -> `MoneyDepositedEvent`.

## Chạy
```bash
cd EDA
dotnet run                      # in-memory

docker compose up -d            # (từ thư mục LAP4) bật RabbitMQ
dotnet run -- rabbitmq          # dùng RabbitMQ
```
Management UI: http://localhost:15672 (guest/guest) – xem exchange `eda.events`.

## Kiến trúc RabbitMQ
Exchange `eda.events` (direct) – routing key = tên event.
Mỗi `Subscribe<T>` tạo 1 queue riêng bind vào key đó, nên Analytics và FraudDetection
cùng nhận được mỗi `MoneyWithdrawnEvent` (pub/sub).
