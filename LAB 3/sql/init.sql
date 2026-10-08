USE master
GO
IF DB_ID('TodoDB') IS NULL CREATE DATABASE TodoDB
GO
USE TodoDB
GO
IF OBJECT_ID('Todos') IS NULL
CREATE TABLE Todos
(
    Id INT PRIMARY KEY IDENTITY(1,1),
    Title NVARCHAR(50),
    IsCompleted BIT
)
GO
INSERT INTO Todos (Title, IsCompleted) VALUES (N'Learning', 0), (N'Cooking', 0)
GO
