#!/usr/bin/env bash
# Chạy 1 lần: tạo solution và build (macOS, .NET 7)
set -e
dotnet new sln -n ThreeTierArchitecture --force
dotnet sln add Todo.API/Todo.API.csproj Todo.Application/Todo.Application.csproj \
               Todo.Domain/Todo.Domain.csproj Todo.Infrastructure/Todo.Infrastructure.csproj
dotnet build
