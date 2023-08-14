
# BattleBit Remastered 服务器 API

[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)

Language [English](/README.md) | 中文 | [한국어](/README_koKR.md)
 
这个 repo 是 BBR（像素战地）的服务端 API

## 如何开始

### 前置需求

- 拥有 BBR 服务端的开服权限，且满足开服条件。
- 可以写基于 [.NET 6.0](https://dotnet.microsoft.com/en-us/download/dotnet/6.0) 的 C# 代码。
- 可以在生产环境中部署此代码。

### 如何制作功能

制作对应的功能可以 [查看维基(制作中)](https://github.com/MrOkiDoki/BattleBit-Community-Server-API/wiki).

使用这个 API 将在运行本程序后开启一个`ServerListener` 的监听进程，传递你*自己定义*的`Player` 和 `GameServer` 类型覆盖原本游戏自身的`Player` 和 `GameServer`类型。在这些类型中添加任何你想要的功能，以此来定制属于你自己的游戏玩法。
如果想给你的服务端添加功能，可以直接把覆盖的功能写在 `Program.cs` 的 `MyPlayer` 和 `MyGameServer`中，当然也可以按照框架规范进行其他的功能纂写。


### 编译

可以直接使用 [`dotnet build`](https://learn.microsoft.com/en-us/dotnet/core/tools/dotnet-build) 的命令进行编译，或者在你的 IDE 中自定义编译.

### 连接服务端

当你将此项目的功能写完并进行了编译后，需要将此 API 服务进行部署。此 API 服务可以部署在本地网络、本机网络或者广域网中。我们强烈建议以「最低延迟」为基准进行部署，以保证 `ServerListener` 可以同时监听 *多个* 游戏服务端。
你可以在游戏服务端的启动配置中对 API 的地址和端口进行设定。
