# BattleBit Remastered Community Server API

[![Licencia: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)

Language [English](/README.md) | [中文](/README-zhCN.md) | [한국어](/README-koKR.md) | Español

Este repositorio proporciona una API que puede ser usada para manejar eventos en tu servidor de comunidad y manipularlos.

## Primeros pasos

### Preequisitos

- Tu proprio servidor de BattleBit Remastered con la progresión **deshabilitada** y acceso a los parámetros de lanzamiento.
- Poder escribir y compilar [.NET 6.0](https://dotnet.microsoft.com/en-us/download/dotnet/6.0) en C#.
- (para entorno de producción) Un espacio donde alojar esta API

### Información

La documentación y algunos ejemplos se pueden encontrar en la [wiki](https://github.com/MrOkiDoki/BattleBit-Community-Server-API/wiki) (WIP).

La manera de utilizar esta API es instanciando una clase `ServerListener` (y ejecutándola) a la cual le pasas tus *propias* subclases de `Player` y `GameServer`. En esas subclases, puedes hacer tus propias modificaciones a los métodos que ya existen en `Player` y `GameServer`. Tambien puedes añadir tus propios atributos y métodos.

La manera más fácil de empezar, es usando el archivo proporcionado `Program.cs` y agregar tus propias modificaciones. dentro de `MyPlayer` y `MyGameServer`.

### Compilación

El proyecto se puede compilar tanto usando el comando [`dotnet build`](https://learn.microsoft.com/en-us/dotnet/core/tools/dotnet-build) en una línea de comandos o usando las opciones de compilar dentro de tu IDE preferido.

Alternativamente, puedes usar docker para ejecutarlo. Una forma fácil de hacer eso es ejecutar el comando `docker compose up`.

### Conectando al servidor(es)

Después de programar y compilar el proyecto, necesitarás un sitio donde alojarlo. Esto se podría hacer en el mismo sitio que el servidor del juego donde la latencia sería mínima, o en cualquier lugar completamente diferente. Es recomendable mantener la latencia entre el servidor y la API lo más baja posible para favorecer el rendimiento. Un mismo `ServerListener` puede ser utilizado para *múltiples* servidores del juego al mismo tiempo. Puedes especificar la API (direccion y puerto) en los parámetros de lanzamiento del servidor de juego.

#### Parámetros de lanzamiento del servidor de juego

El servidor se conecta a la API a través del parámetro `"-apiendpoint=<IP>:<port>"`, donde `<port>` es el puerto donde el listener escucha y `<IP>` es la dirección IP de la API.

Si se requiere verificación por `Api Token` en tu API, tendrás que añadir el parámetro `"-apiToken=<ApiToken>"` a los parámetros de lanzamiento de los servidor(es). Si el `<ApiToken>` del servidor es el mismo `Api Token` que está definido en la API, los servidores se podrán comunicar con la API. De no ser así, la conexión será rechazada.

Cuando el servidor de juego esté iniciado completamente, puedes modificar direcamente el `Api Token` del servidor usando el comando `setapitoken <new token>` en la ventana de comandos del servidor que se ha iniciado.

#### Ajustar puerto de escucha de la API

El proyecto está actualmente configurado para escuchar en el puerto `29294`. Si quisieses cambiar esto, asegurate de cambiarlo en el código (en tu `listener.start(port)`). El puerto `29294` tambien está expuesto en Docker y ligado al mismo puerto enel host en Docker Compose. Esto significa que, usando Docker, se tendrá que cambiar el puerto en el archivo `Dockerfile` y en el archivo `docker-compose.yml` (cuando se utiliza Compose) tambien. Léase [EXPOSE in the Dockerfile reference](https://docs.docker.com/engine/reference/builder/#expose) y [networking in Compose](https://docs.docker.com/compose/networking/).
