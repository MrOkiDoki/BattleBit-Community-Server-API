# BBR服务器API

 [![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)
 
这个是BBR（像素战地）的服务端API

## 如何开始

### 系统需求

- 拥有 BBR 服务端的开服权限，且满足开服条件。
- 可以写基于 [.NET 6.0](https://dotnet.microsoft.com/en-us/download/dotnet/6.0) 的 C# 代码.
- 生产环境中可以部署此代码.

### 如何制作功能

查看维基 [此页面](https://github.com/MrOkiDoki/BattleBit-Community-Server-API/wiki).


The way to use this API is to make an instance of `ServerListener` and add your own handlers to certain events that happen on your server(s).
The easiest way to do this, is to add/put your own code in `Program.cs` and then build the project.

### Building

This project can either be built by using [`dotnet build`](https://learn.microsoft.com/en-us/dotnet/core/tools/dotnet-build) on the command-line or by using the run / build options inside your preferred IDE.

### Connecting to the gameserver

After writing and compiling this project. You will want to host it somewhere. This could be on the same server that the gameserver runs on, or somewhere completely different. We do recommend to keep the latency to the gameserver minimal for smoother and faster communication. The same `ServerListener` can be used for *multiple* gameservers at the same time. You can specify the API server (address & port) in the launch options of the gameserver.
