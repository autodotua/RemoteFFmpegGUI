mkdir -Force Generation/Publish/WebPackage
mkdir -Force Generation/Publish/WebPackage/api
mkdir -Force Generation/Publish/WebPackage/host

Write-Output "正在发布Web"
cd SimpleFFmpegGUI.Web
npm run build
cd ..
Write-Output "正在复制Web"
Copy-Item SimpleFFmpegGUI.Web/dist/* Generation/Publish/WebPackage -Force -Recurse

Write-Output "正在发布WebAPI"
dotnet publish SimpleFFmpegGUI.WebAPI -c Release -o Generation/Publish/WebPackage/api

Write-Output "正在发布Host"
dotnet publish SimpleFFmpegGUI.Host -c Release -o Generation/Publish/WebPackage/host

Write-Output "正在发布WPF"
dotnet publish SimpleFFmpegGUI.WPF -c Release -o Generation/Publish/WPF

Write-Output "正在清理"
Remove-Item SimpleFFmpegGUI.Web/dist -Recurse
Remove-Item Generation/Release -Recurse

Write-Output "操作完成"
Write-Output "Web包位于Generation/Publish/WebPackage，WPF包位于Generation/Publish/WPF。"
Write-Output "请将ffmpeg和MediaInfo相关二进制文件放到Generation/Publish/WPF和Generation/Publish/WebPackage/host中。"
[Console]::ReadKey()