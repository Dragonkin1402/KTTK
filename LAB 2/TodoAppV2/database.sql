-- Tuỳ chọn: chạy tay nếu không muốn ứng dụng tự tạo (app đã tự tạo khi khởi động)
IF DB_ID('TodoDB') IS NULL CREATE DATABASE TodoDB;
GO
USE TodoDB;
GO
IF OBJECT_ID(N'dbo.Todos', N'U') IS NULL
CREATE TABLE dbo.Todos (
    Id          INT IDENTITY(1,1) PRIMARY KEY,
    Title       NVARCHAR(200) NOT NULL,
    IsCompleted BIT NOT NULL DEFAULT 0
);
GO
INSERT INTO Todos (Title, IsCompleted) VALUES (N'Learning software architecture', 1);
GO
