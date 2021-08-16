# FFmpeg GUI

## 架构

|项目名|项目名（中文）|介绍|
|-|-|-|
|Core|核心|提供Host和WPF、Web的契约以及公共方法|
|Host|主机|对FFmpeg进行包装，实现其功能，并通过NamedPipe进行发布|
|WPF|桌面GUI|桌面端的GUI实现，暂时不实现|
|WebAPI|服务端|使用ASP.NetCore实现的服务器|
|Web|Web客户端|使用Vue.js实现的网页端|

## Web版本部署

1. 打开Web项目，将`net.ts`文件中的`getUrl`方法中的url切换到发布版本
2. 在VS中发布Host和WebAPI，在Web项目中命令行运行`npm run build`发布前端包
3. 修改WebAPI的`appsettings.json`，主要修改`InputDir`和`OutputDir`项，指定输入和输出目录
4. 新建一个网站文件夹，放置前端文件，新建api文件夹放置WebAPI文件，新建Host文件夹放置Host文件
4. 确保安装了.Net 5 Hosting Bundle，并在Windows中启用了IIS
4. 在IIS中新建网站，指定物理目录为之前新建的目录，设置api为虚拟应用程序
7. 运行Host的exe，然后打开设置的url即可使用

- 若要在IIS中启用自动启动Host功能，还需要：
    1. IIS > 应用程序池
    2. 为网站选择高级设置
    3. 将标识（Identity）更改为 LocalSystem
    4. 重启 IIS

- 若输入或输出文件夹位于网络位置等IIS无权限的位置，则需要：
    - 设置`appsettings.json`中的 `InputDirAccessable`和/或`OutputDirAccessable`为`false`，告知程序无权限访问，那么后端将通过Host对文件进行访问
    - 关闭自动启动Host功能，因为自动启动的Host将继承IIS的权限，依旧无法访问
    - 这种模式下，HTTP上传和下载功能将不可用（懒得写）