# Sử dụng image SDK để build dự án
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /app

# 1. Copy file .sln và .csproj vào đúng thư mục con của nó
COPY *.sln ./
COPY NexusSinhVien.API/*.csproj ./NexusSinhVien.API/

# 2. Di chuyển vào thư mục chứa project và restore
WORKDIR /app/NexusSinhVien.API
RUN dotnet restore

# 3. Copy toàn bộ source code và build
WORKDIR /app
COPY . ./
RUN dotnet publish -c Release -o out NexusSinhVien.API/NexusSinhVien.API.csproj

# 4. Sử dụng image runtime để chạy ứng dụng
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=build /app/out ./

# Cấu hình để ứng dụng lắng nghe trên cổng 8080
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

# Thay 'NexusSinhVien.API.dll' bằng tên file .dll thực tế của bạn
ENTRYPOINT ["dotnet", "NexusSinhVien.API.dll"]