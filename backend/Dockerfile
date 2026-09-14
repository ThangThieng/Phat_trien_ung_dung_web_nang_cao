# CONS-009: multi-stage build SDK -> aspnet runtime
# ---------- build ----------
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Restore trước để tận dụng Docker layer cache
COPY global.json Directory.Build.props .editorconfig CulinaryBlog.sln ./
COPY src/CulinaryBlog.Domain/CulinaryBlog.Domain.csproj src/CulinaryBlog.Domain/
COPY src/CulinaryBlog.Application/CulinaryBlog.Application.csproj src/CulinaryBlog.Application/
COPY src/CulinaryBlog.Infrastructure/CulinaryBlog.Infrastructure.csproj src/CulinaryBlog.Infrastructure/
COPY src/CulinaryBlog.API/CulinaryBlog.API.csproj src/CulinaryBlog.API/
RUN dotnet restore src/CulinaryBlog.API/CulinaryBlog.API.csproj

COPY src/ src/
RUN dotnet publish src/CulinaryBlog.API/CulinaryBlog.API.csproj -c Release -o /app/publish --no-restore /p:UseAppHost=false

# ---------- runtime ----------
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
# ASPNETCORE_HTTP_PORTS thay cho ASPNETCORE_URLS để không đè cấu hình cổng mặc định của image .NET
ENV ASPNETCORE_HTTP_PORTS=8080 \
    ASPNETCORE_ENVIRONMENT=Production \
    DOTNET_RUNNING_IN_CONTAINER=true \
    DataProtection__KeysPath=/home/app/dpkeys
COPY --from=build /app/publish .
# Thư mục khóa DataProtection thuộc user "app" để volume mount vào kế thừa quyền ghi
RUN mkdir -p /home/app/dpkeys && chown app:app /home/app/dpkeys
# Image aspnet có sẵn user non-root "app"
USER app
EXPOSE 8080
ENTRYPOINT ["dotnet", "CulinaryBlog.API.dll"]
