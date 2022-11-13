param(
    [Parameter()]
    [switch]$d,
    [switch]$w,
    [switch]$s,
    [switch]$f
)
try {
    
    Write-Output "-d：仅生成WPF"
    Write-Output "-w：仅生成Web（Web、WebAPI、Host）"
    Write-Output "-s：WPF生成到单文件"
    Write-Output "-f：WPF包含框架"
    Write-Output ""  
    
    Write-Output "请先阅读ReadMe"
    Write-Output "请确保："
    Write-Output "已经安装npm（Node.JS）"
    Write-Output "已经安装.NET 6 SDK"
    Write-Output "已经将ffmpeg相关二进制文件、MediaInfo.dll、性能测试视频（test.mp4）放置到./bin中"
    Write-Output "按任意键继续"

    [Console]::ReadKey()
    Clear-Host
    
    if (!(Test-Path bin)) {
        throw "不存在bin目录"
    }
    try {
        npm
    }
    catch {
        throw "不存在npm命令"
    }
    
    try {
        dotnet
    }
    catch {
        throw "未安装.NET SDK"
    }

    
    Clear-Host

    Remove-Item Generation/Publish -Recurse
    
    if ($d -eq $false) {
        mkdir -Force Generation/Publish/WebPackage
        mkdir -Force Generation/Publish/WebPackage/api
        mkdir -Force Generation/Publish/WebPackage/host

        Write-Output "正在发布Web"
        Set-Location SimpleFFmpegGUI.Web
        npm install
        npm run build
        Set-Location ..
        Write-Output "正在复制Web"
        Copy-Item SimpleFFmpegGUI.Web/dist/* Generation/Publish/WebPackage -Force -Recurse

        Write-Output "正在发布WebAPI"
        dotnet publish SimpleFFmpegGUI.WebAPI -c Release -o Generation/Publish/WebPackage/api

        Write-Output "正在发布Host"
        dotnet publish SimpleFFmpegGUI.Host -c Release -o Generation/Publish/WebPackage/host

        
        Write-Output "正在复制二进制库"
        Copy-Item bin/* Generation/Publish/WebPackage/host -Force -Recurse
        
        Write-Output "正在清理"
        Remove-Item SimpleFFmpegGUI.Web/dist -Recurse
    }

    
    if ($w -eq $false) {
        Write-Output "正在发布WPF"
        if ($s) {
            dotnet publish SimpleFFmpegGUI.WPF -c Release -o Generation/Publish/WPF -r win-x64 --self-contained false /p:PublishSingleFile=true
        }
        else {
            if ($f) {
                dotnet publish SimpleFFmpegGUI.WPF -c Release -o Generation/Publish/WPF -r win-x64 --self-contained true
            }
            else {
                dotnet publish SimpleFFmpegGUI.WPF -c Release -o Generation/Publish/WPF -r win-x64 --self-contained false
            }
        }
        

        
        Write-Output "正在复制二进制库"
        Copy-Item bin/* Generation/Publish/WPF -Force -Recurse

    }
    
    Write-Output "正在清理"
    Remove-Item Generation/Release -Recurse

    Write-Output "操作完成"
    Write-Output "Web包位于Generation/Publish/WebPackage，WPF包位于Generation/Publish/WPF。"
    [Console]::ReadKey()
}
catch {
    Write-Error $_
}