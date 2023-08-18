# BattleBit Remastered Community Server API

[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)

Language English | [中文](/README-zhCN.md) | [한국어](/README-koKR.md) | [German](/README-de.md)

Dieses Repository bietet eine API, die verwendet werden kann, um Ereignisse auf deinen Community-Servern zu verwalten und zu manipulieren.

## Erste Schritte

### Voraussetzungen

- Eigener Community-Server innerhalb von BattleBit Remastered mit **deaktiviertem** Fortschritt und Zugriff auf dessen Startoptionen.
- Die Fähigkeit, C#-Code [.NET 6.0](https://dotnet.microsoft.com/en-us/download/dotnet/6.0) zu schreiben und zu kompilieren.
- (nur Produktivbetrieb) Ein Server, um diese API zu hosten.

### Bearbeiten

Dokumentation und Beispiele findest du im [Wiki](https://github.com/MrOkiDoki/BattleBit-Community-Server-API/wiki).

Die Art und Weise, diese API zu verwenden, besteht darin, eine Instanz von `ServerListener` zu erstellen (und zu starten), an die du die Typen deiner eigenen Unterklassen von `Player` & `GameServer` übergibst. In diesen Unterklassen kannst du deine eigenen Überschreibungen für die bereits vorhandenen virtuellen Methoden in `Player` und `GameServer` erstellen. Du kannst auch eigene Methoden sowie Felder/Eigenschaften hinzufügen.

Der einfachste Weg, mit all dem zu beginnen, besteht darin, Program.cs zu verwenden und deine Überschreibungen usw. in MyPlayer & MyGameServer hinzuzufügen.

### Kompilieren

Dieses Projekt kann entweder mit dem Befehl [`dotnet build`](https://learn.microsoft.com/en-us/dotnet/core/tools/dotnet-build) auf der Kommandozeile kompiliert werden oder durch Verwendung der Ausführungs- / Kompilierungsoptionen in deiner bevorzugten Entwicklungsumgebung (IDE).

Alternativ kannst du Docker verwenden, um es auszuführen. Ein einfacher Weg dazu ist, `docker compose up` auszuführen.

### Verbindung zu dem/den Spielserver/n

Nachdem du dieses Projekt geschrieben und kompiliert hast, musst du es irgendwo hosten. Dies kann auf demselben Server erfolgen, auf dem die Spielserver laufen, oder an einem völlig anderen Ort. Wir empfehlen, um Latenz zu den Spielservern minimal zu halten, und eine reibungslose und schnellere Kommunikation zu gewährleisten, den API Server auf dem selben Host wie den Spielserver zu betreiben. Derselbe `ServerListener` kann gleichzeitig für mehrere Spielserver verwendet werden. Du kannst den API-Server (Adresse & Port) in den Startoptionen des Spielservers angeben.

#### Startargumente für Spielserver

Der Spielserver verbindet sich mit der API mit dem Startargument `"-apiendpoint=<IP>:<port>"`, wobei `<port>` der Port ist, auf dem der Listener lauscht, und `<IP>` die IP des API-Servers ist.

Wenn in deiner Server-API eine Überprüfung des `API-Token` erforderlich ist, musst du `"-apiToken=<ApiToken>"` zu den Startparametern des Spielservers hinzufügen. Sollte `<ApiToken>` mit dem in der Server-API definierten `API-Token` übereinstimmen, können Spielserver mit der Server-API kommunizieren.

Wenn der Spielserver läuft, kannst du das `API-Token` des Spielservers auch direkt über die Eingabe von `setapitoken <neues Token>` in der Befehlszeile ändern.

#### Anpassen des API-Ports

Das Projekt ist derzeit so konfiguriert, dass die API auf Port `29294` hört. Wenn du dies ändern möchtest, stelle sicher, dass du es im Code änderst (bei deinem `listener.start(port)`). Port `29294` ist auch in Docker freigegeben und in Docker Compose an denselben Port auf dem Host gebunden. Das bedeutet, wenn du Docker verwendest, musst du den Port auch in dem `Dockerfile` und in `docker-compose.yml` (bei Verwendung von Compose) ändern. Siehe [EXPOSE in the Dockerfile reference](https://docs.docker.com/engine/reference/builder/#expose) und [Networking in Compose](https://docs.docker.com/compose/networking/).
