# FFmpeg GUI

## 架构

| 项目名     | 项目名（中文） | 介绍                                                  |
| ---------- | -------------- | ----------------------------------------------------- |
| Core       | 核心           | 提供Host和WPF、Web的契约以及公共方法                  |
| Host       | 主机           | 对FFmpeg进行包装，实现其功能，并通过NamedPipe进行发布 |
| WPF        | 桌面GUI        | 桌面端的GUI实现                                       |
| Web.Server | 服务端         | 使用ASP.Net Core实现的服务器                          |
| Web.Client | Web客户端      | 使用Vue.js实现的网页端                                |

