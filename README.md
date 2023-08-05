# BattleBit Remastered Community Server API

 [![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)
 
This repository provides an API that can be used to handle events on your community server(s) and manipulate them.

## Getting started

### Prerequisites

- Your own community server within BattleBit Remastered with progression **disabled** and access to its launch options.
- The ability to write and compile [.NET 6.0](https://dotnet.microsoft.com/en-us/download/dotnet/6.0) C# code.
- (for production) A place to host this API on.

### Writing

Documentation and examples can be found on the [wiki](https://github.com/MrOkiDoki/BattleBit-Community-Server-API/wiki).


The way to use this API is to make an instance of `ServerListener` and add your own handlers to certain events that happen on your server(s).
The easiest way to do this, is to add/put your own code in `Program.cs` and then build the project.

### Building

This project can either be built by using [`dotnet build`](https://learn.microsoft.com/en-us/dotnet/core/tools/dotnet-build) on the command-line or by using the run / build options inside your preferred IDE.

### Connecting to the gameserver

After writing and compiling this project. You will want to host it somewhere. This could be on the same server that the gameserver runs on, or somewhere completely different. We do recommend to keep the latency to the gameserver minimal for smoother and faster communication. The same `ServerListener` can be used for *multiple* gameservers at the same time. You can specify the API server (address & port) in the launch options of the gameserver.
