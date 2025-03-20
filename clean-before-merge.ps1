# Script xóa tất cả các file tạm thời trước khi merge để tránh xung đột
Write-Host "Đang xóa các file tạm thời trước khi merge..." -ForegroundColor Cyan

# Lưu branch hiện tại
$currentBranch = git rev-parse --abbrev-ref HEAD
Write-Host "Branch hiện tại: $currentBranch" -ForegroundColor Yellow

# Xóa các file bin và obj
Write-Host "Đang xóa thư mục bin và obj..." -ForegroundColor Green
if (Test-Path "bin") {
    Remove-Item -Path "bin" -Recurse -Force -ErrorAction SilentlyContinue
    Write-Host "Đã xóa thư mục bin" -ForegroundColor Green
}

if (Test-Path "obj") {
    Remove-Item -Path "obj" -Recurse -Force -ErrorAction SilentlyContinue
    Write-Host "Đã xóa thư mục obj" -ForegroundColor Green
}

# Xóa các file cache của Visual Studio
Write-Host "Đang xóa các file cache của Visual Studio..." -ForegroundColor Green
if (Test-Path ".vs") {
    Remove-Item -Path ".vs" -Recurse -Force -ErrorAction SilentlyContinue
    Write-Host "Đã xóa thư mục .vs" -ForegroundColor Green
}

# Xóa các file tạm thời khác
Write-Host "Đang xóa các file tạm thời khác..." -ForegroundColor Green
Get-ChildItem -Path "." -Filter "*.tmp" -Recurse -ErrorAction SilentlyContinue | Remove-Item -Force
Get-ChildItem -Path "." -Filter "*.cache" -Recurse -ErrorAction SilentlyContinue | Remove-Item -Force
Get-ChildItem -Path "." -Filter "*.nuget.dgspec.json" -Recurse -ErrorAction SilentlyContinue | Remove-Item -Force
Get-ChildItem -Path "." -Filter "project.assets.json" -Recurse -ErrorAction SilentlyContinue | Remove-Item -Force
Get-ChildItem -Path "." -Filter "project.nuget.cache" -Recurse -ErrorAction SilentlyContinue | Remove-Item -Force
Get-ChildItem -Path "." -Filter "*.nuget.g.props" -Recurse -ErrorAction SilentlyContinue | Remove-Item -Force
Get-ChildItem -Path "." -Filter "*.nuget.g.targets" -Recurse -ErrorAction SilentlyContinue | Remove-Item -Force

# Chạy các lệnh dotnet clean và restore
Write-Host "Đang chạy 'dotnet clean'..." -ForegroundColor Magenta
dotnet clean

Write-Host "Đang chạy 'dotnet restore'..." -ForegroundColor Magenta
dotnet restore

Write-Host "Hoàn thành xóa các file tạm thời. Bạn có thể tiến hành merge bây giờ." -ForegroundColor Cyan
Write-Host "Sử dụng lệnh sau để merge: git merge [branch_name]" -ForegroundColor Yellow
Write-Host "Nếu vẫn có xung đột, hãy sử dụng: git mergetool" -ForegroundColor Yellow
