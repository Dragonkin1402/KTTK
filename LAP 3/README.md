# LAB 3 – Three-Tier Architecture (macOS, .NET 7)

## 1. SQL Server bằng Docker
```bash
docker run --platform linux/amd64 -e ACCEPT_EULA=Y -e MSSQL_SA_PASSWORD='YourStrong@Passw0rd' \
  -p 1433:1433 --name sqlserver -d mcr.microsoft.com/mssql/server:2022-latest
# đợi ~15 giây rồi tạo DB:
docker exec -i sqlserver /opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P 'YourStrong@Passw0rd' -C < sql/init.sql
```
(Mac chip M1/M2/M3: bật "Use Rosetta for x86/amd64 emulation" trong Docker Desktop.)

## 2. Chạy API
```bash
chmod +x setup.sh && ./setup.sh
cd Todo.API && dotnet run --urls http://localhost:5208
```
Swagger: http://localhost:5208/swagger

## 3. Chạy UI
```bash
cd Todo.UI && python3 -m http.server 5500
```
Mở http://localhost:5500
