# BattleBit Remastered Community Server API

[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)

Language [English](/README.md) | [Русский](/README-ruRU.md) | [中文](/README-zhCN.md) | [한국어](/README-koKR.md) | [Español](/README-esES.md) | Português

Este repositório proporciona uma API que pode ser usada para manipular eventos em seu(s) servidor(es) da comunidade.

## Começando

### Prerequisitos

- Ter seu próprio servidor de BattleBit Remastered com a progressão **desabilitada** e ter acesso às opções de inicializção.
- Conseguir escrever e compilar [.NET 6.0](https://dotnet.microsoft.com/en-us/download/dotnet/6.0) em C#.
- (para produção) Um lugar para hospedar essa API.

### Escrevendo

A documentação e exemplos podem ser encontrados na [wiki](https://github.com/MrOkiDoki/BattleBit-Community-Server-API/wiki) (WIP).

O jeito de usar esta API é criando uma instancia do `ServerListener` (e inicia-lo) na qual você passa os tipos da suas próprias subclasses  do `Player` e `GameServer`. Nessas subclasses, você pode sobre escrever funções existentes em `Player` e `GameServer`. Você tambem pode adicionar suas próprias funções campos e propriedades.  
A maneira mais fácil de começar com tudo isso é usando `Program.cs` e adicionar suas substituições etc. dentro de `MyPlayer` e `MyGameServer`.

### Compilando

Este projeto pode ser compilado tanto com [`dotnet build`](https://learn.microsoft.com/en-us/dotnet/core/tools/dotnet-build) na linha te comando ou usando as opções de build sua IDE preferida.

Alternativamente, você pode usar o Docker para compilar. Uma maneira fácil de fazer isso é executar `docker compose up`.

### Conectando ao servidor do jogo
  
Depois de editar e compilar este projeto. Você vai querer hospedá-lo em algum lugar. Isso pode ser no mesmo servidor em que os servidores de jogos são executados ou em um local completamente diferente. Recomendamos que a latência para o servidor de jogos seja mínima para uma comunicação mais suave e rápida. O mesmo `ServerListener` pode ser usado para *múltiplos* servidores de jogos ao mesmo tempo. Você pode especificar o servidor de API (endereço e porta) nas opções de inicialização do servidor de jogos

#### Argumentos de inicialização do servidor de jogos

O servidor de jogos se conecta à API com o argumento de inicialização `"-apiendpoint=<IP>:<port>"`, em que `<port>` é a porta que o ouvinte escuta e `<IP>` é o IP do serviço da API

Se a verificação de `Api Token` for necessária em sua API do servidor, você precisará adicionar `"-apiToken=<ApiToken>"` aos parâmetros de inicialização do(s) servidor(es) de jogos. Deve `<ApiToken>` ser igual ao `Api Token` definido na API do servidor, o(s) servidor(es) de jogos poderá(ão) se comunicar com a API do servidor.

Quando o servidor de jogos estiver ativo, você também poderá modificar diretamente o `Api Token` do servidor de jogos digitando `setapitoken <new token>` em sua linha de comando.

#### Ajustar a porta da API

O projeto está atualmente configurado para que a API escute na porta `29294`. Se você quiser alterar isso, certifique-se de alterá-lo no código (em seu `listener.start(port)`). A porta `29294` também é exposta no Docker e vinculada à mesma porta no host do Docker Compose. Isso significa que, ao usar o Docker, você terá que alterar a porta no `Dockerfile` e no `docker-compose.yml` (ao usar o Compose) também. Consulte [EXPOSE in the Dockerfile reference](https://docs.docker.com/engine/reference/builder/#expose) e [networking in Compose](https://docs.docker.com/compose/networking/).
