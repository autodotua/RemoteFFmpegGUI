mkdir -Force Generation/Release/WebPackage
mkdir -Force Generation/Release/WebPackage/api
mkdir -Force Generation/Release/WebPackage/host
Write-Output "正在复制Web"
Copy-Item SimpleFFmpegGUI.Web/dist/* Generation/Release/WebPackage -Force -Recurse
Write-Output "正在复制WebAPI"
Copy-Item Generation/Release/Api/* Generation/Release/WebPackage/api -Force -Recurse
Write-Output "正在复制Host"
Copy-Item Generation/Release/Host/* Generation/Release/WebPackage/host -Force -Recurse
Write-Output "操作完成，位于Generation/Release/WebPackage。请将ffmpeg相关二进制文件放到Generation/Release/WebPackage/host中。"