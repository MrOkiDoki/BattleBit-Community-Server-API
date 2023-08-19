﻿# BattleBit Remastered Community Server API

[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)

Language English | [中文](/README-zhCN.md) | [한국어](/README-koKR.md) | [Español](/README-esES.md)

This repository provides an API that can be used to handle events on your community server(s) and manipulate them.

## Getting started

### Prerequisites

- Your own community server within BattleBit Remastered with progression **disabled** and access to its launch options.
- The ability to write and compile [.NET 6.0](https://dotnet.microsoft.com/en-us/download/dotnet/6.0) C# code.
- (for production) A place to host this API on.

### Writing

Documentation and examples can be found on the [wiki](https://github.com/MrOkiDoki/BattleBit-Community-Server-API/wiki) (WIP).

The way to use this API is to make an instance of `ServerListener` (and start it) on which you pass the types of your *own* subclasses of `Player` & `GameServer`. In those subclasses, you can make your own overrides to the already existing methods in `Player` and `GameServer`. You can also add your own methods and fields/properties.

The easiest way to get started with all of this, is to use `Program.cs` and add your overrides etc. into `MyPlayer` & `MyGameServer`.

### Building

This project can either be built by using [`dotnet build`](https://learn.microsoft.com/en-us/dotnet/core/tools/dotnet-build) on the command-line or by using the run / build options inside your preferred IDE.

Alternatively, you can use Docker to run it. An easy way to do this it to run `docker compose up`.

### Connecting to the gameserver(s)

After writing and compiling this project. You will want to host it somewhere. This could be on the same server that the gameservers run on, or somewhere completely different. We do recommend to keep the latency to the gameserver minimal for smoother and faster communication. The same `ServerListener` can be used for *multiple* gameservers at the same time. You can specify the API server (address & port) in the launch options of the gameserver.

#### Gameserver start arguments

The gameserver connects to the API with the launch argument `"-apiendpoint=<IP>:<port>"`, where `<port>` is the port that the listener listens on and the `<IP>` is the IP of the API server.

If `Api Token` verification is required in your Server API, you need to add `"-apiToken=<ApiToken>"` to the startup parameters of the gameserver(s). Should `<ApiToken>` the same as `Api Token` defined in Server API, gameserver(s) can communicate with Server API.

When the gameserver is up, you can also directly modify the `Api Token` of the gameserver by entering `setapitoken <new token>` in its command line.

#### Adjust API listening port

The project is currently configured to have the API listen on port `29294`. If you want to change this, make sure to change it in the code (on your `listener.start(port)`). Port `29294` is also exposed in Docker and bound to the same port on the host in Docker Compose. This means that when using Docker, you will have to change the port in the `Dockerfile` and in `docker-compose.yml` (when using Compose) as well. See [EXPOSE in the Dockerfile reference](https://docs.docker.com/engine/reference/builder/#expose) and [networking in Compose](https://docs.docker.com/compose/networking/).
