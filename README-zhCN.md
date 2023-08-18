# BattleBit Remastered 服务端 API

[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)

Language [English](/README.md) | 中文 | [한국어](/README-koKR.md)
 
BBR（像素战地）的服务端 API 在部署后可以提供`社区服`所需要的游戏服务端事件处理以及事件控制。

## 如何开始

### 前置需求

- 拥有 BBR 服务端的开服权限，且满足开服条件。
- 可以写基于 [.NET 6.0](https://dotnet.microsoft.com/en-us/download/dotnet/6.0) 的 C# 代码。
- 可以在生产环境中部署此代码。

### 制作功能

制作对应的功能可以 [查看维基(制作中)](https://github.com/MrOkiDoki/BattleBit-Community-Server-API/wiki).

本 API 将在运行后开启一个`ServerListener` 的监听进程，传递你 *自己定义* 的 `Player` 和 `GameServer` 类型覆盖原本游戏自身的 `Player` 和 `GameServer` 类型。在这些类型中添加任何你想要的功能，以此来定制属于你自己的游戏玩法。
如果想给你的游戏服务端添加功能，可以直接把覆盖的功能写入 `Program.cs` 的 `MyPlayer` 和 `MyGameServer`中，当然你也可以按照框架规范进行其他的功能纂写。

### 编译

可以直接使用 [`dotnet build`](https://learn.microsoft.com/en-us/dotnet/core/tools/dotnet-build) 的命令进行编译，或者在你的 IDE 中自定义编译。

### 连接游戏服务端

当你将此项目的功能写完并进行了编译后，需要将此 API 服务进行部署。此 API 服务可以部署在本地网络、本机网络或者广域网中。我们强烈建议以「最低延迟」为基准进行部署，以保证 `ServerListener` 可以同时监听 *多个* 游戏服务端。

你可以在游戏服务端的启动配置中对 API 的地址和端口进行设定。

#### 启动参数
游戏服务端通过启动参数 `"-apiEndpoint=<IP>:<端口>"` 来与本 API 实例进行通信, 启动参数中的 `<端口>` 指的是本 API 服务中指定的端口 `<IP>` 指的是本 API 服务部署实例的 IP 地址。

如果你在 API 服务中定义游戏服务端连接 API 服务时需要进行 `Api Token` 的验证，那么在游戏服务端的启动参数中需要增加 `"-apiToken=<你在程序中设置的 ApiToken>"`，当游戏服务端启动项的`Api Token`与 API 服务中`Api Token`的一致时，游戏服务端才可以与指定的 API 服务进行通信。

在游戏服务端的运行时，你也可以通过在命令行中输入 `setapitoken <新的token>` 来直接修改运行中的服务端的 `Api Token`。

#### 调整 API 端口
如果游戏服务端实例与本 API 实例不在同一个实例上进行部署，且你想修改本 API 实例的端口 `29294`，你可以查看 `Progran.cs` 中 `listener.Start(29294);` 并把 `29294` 修改为你想指定或防火墙等安全策略已通过的端口号。

如果你的实例运行在 Docker 容器中，端口 `29294` （或你修改的其他端口）也同时需要在 Docker 容器配置中进行修改并对外暴露。也就是说你需要修改 `Dockerfile` 且（如果有使用到容器集群编排）还有可能需要修改 `docker-compose.yml` 。相关参考资料可以查看 Docker 官方文档 [EXPOSE in the Dockerfile reference](https://docs.docker.com/engine/reference/builder/#expose) 以及 [networking in Compose](https://docs.docker.com/compose/networking/)。
