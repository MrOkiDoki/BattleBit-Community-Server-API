# BattleBit Remastered Community Server API

[![Licencia: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)

Language [English](/README.md) | [中文](/README-zhCN.md) | [한국어](/README-koKR.md) | Espa�ol

Este repositorio proporciona una API que puede ser usada para manejar eventos en tu servidor de comunidad y manipularlos.

## Primeros pasos

### Preequisitos

- Tu proprio servidor de BattleBit Remastered con la progresi�n **deshabilitada** y acceso a los par�metros de lanzamiento.
- Poder escribir y compilar [.NET 6.0](https://dotnet.microsoft.com/en-us/download/dotnet/6.0) en C#.
- (para entorno de producci�n) Un espacio donde alojar esta API

### Informaci�n

La documentaci�n y algunos ejemplos se pueden encontrar en la [wiki](https://github.com/MrOkiDoki/BattleBit-Community-Server-API/wiki) (WIP).

La manera de utilizar esta API es instanciando una clase `ServerListener` (y ejecut�ndola) a la cual le pasas tus *propias* subclases de `Player` y `GameServer`. En esas subclases, puedes hacer tus propias modificaciones a los m�todos que ya existen en `Player` y `GameServer`. Tambien puedes a�adir tus propios atributos y m�todos.

La manera m�s f�cil de empezar, es usando el archivo proporcionado `Program.cs` y agregar tus propias modificaciones. dentro de `MyPlayer` y `MyGameServer`.

### Compilaci�n

El proyecto se puede compilar tanto usando el comando [`dotnet build`](https://learn.microsoft.com/en-us/dotnet/core/tools/dotnet-build) en una l�nea de comandos o usando las opciones de compilar dentro de tu IDE preferido.

Alternativamente, puedes usar docker para ejecutarlo. Una forma f�cil de hacer eso es ejecutar el comando `docker compose up`.

### Conectando al servidor(es)

Despu�s de programar y compilar el proyecto, necesitar�s un sitio donde alojarlo. Esto se podr�a hacer en el mismo sitio que el servidor del juego donde la latencia ser�a m�nima, o en cualquier lugar completamente diferente. Es recomendable mantener la latencia entre el servidor y la API lo m�s baja posible para favorecer el rendimiento. Un mismo `ServerListener` puede ser utilizado para *m�ltiples* servidores del juego al mismo tiempo. Puedes especificar la API (direccion y puerto) en los par�metros de lanzamiento del servidor de juego.

#### Par�metros de lanzamiento del servidor de juego

El servidor se conecta a la API a trav�s del par�metro `"-apiendpoint=<IP>:<port>"`, donde `<port>` es el puerto donde el listener escucha y `<IP>` es la direcci�n IP de la API.

Si se requiere verificaci�n por `Api Token` en tu API, tendr�s que a�adir el par�metro `"-apiToken=<ApiToken>"` a los par�metros de lanzamiento de los servidor(es). Si el `<ApiToken>` del servidor es el mismo `Api Token` que est� definido en la API, los servidores se podr�n comunicar con la API. De no ser as�, la conexi�n ser� rechazada.

Cuando el servidor de juego est� iniciado completamente, puedes modificar direcamente el `Api Token` del servidor usando el comando `setapitoken <new token>` en la ventana de comandos del servidor que se ha iniciado.

#### Ajustar puerto de escucha de la API

El proyecto est� actualmente configurado para escuchar en el puerto `29294`. Si quisieses cambiar esto, asegurate de cambiarlo en el c�digo (en tu `listener.start(port)`). El puerto `29294` tambien est� expuesto en Docker y ligado al mismo puerto enel host en Docker Compose. Esto significa que, usando Docker, se tendr� que cambiar el puerto en el archivo `Dockerfile` y en el archivo `docker-compose.yml` (cuando se utiliza Compose) tambien. L�ase [EXPOSE in the Dockerfile reference](https://docs.docker.com/engine/reference/builder/#expose) y [networking in Compose](https://docs.docker.com/compose/networking/).
