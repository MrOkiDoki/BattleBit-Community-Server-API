# BattleBit Remastered Community Server API

 [![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)

Language [English](/README.md) | [中文](/README-zhCN.md) | 한국어
 
이 레포지토리는 BattleBit Remastered 커뮤니티 서버에서 이벤트를 처리하고 조작하는 데 사용할 수 있는 API를 제공합니다.

## 시작하기

### 사전 요구 사항

- BattleBit Remastered 커뮤니티 서버를 개설할 수 있는 권한을 보유하고 BattleBit Remastered 에서 요구하는 조건을 충족해야 합니다.
- [.NET 6.0](https://dotnet.microsoft.com/en-us/download/dotnet/6.0)을 이용해 C# 코드를 작성하고 컴파일할 수 있어야 합니다.
- (프로덕션 한정) 이 API를 호스팅할 장소가 필요합니다.

### 작성

문서와 예제는 [wiki](https://github.com/MrOkiDoki/BattleBit-Community-Server-API/wiki)에서 확인할 수 있습니다.

이 API를 사용하는 방법은 `Player` 및 `GameServer`의 *자체* 서브클래스의 유형을 전달하는 `ServerListener` 인스턴스를 생성하고 이를 시작하는 것입니다. 이러한 서브클래스에서는 `Player` 및 `GameServer`에 이미 존재하는 메서드를 오버라이드하여 자신만의 메서드, 필드 및 프로퍼티를 추가할 수 있습니다

이 모든 걸 시작하는 가장 쉬운 방법은 `Program.cs`에서 `MyPlayer` 및 `MyGameServer`에 오버라이드 등을 추가하는 것입니다.

### 빌드

이 프로젝트는 CMD에서 [`dotnet build`](https://learn.microsoft.com/en-us/dotnet/core/tools/dotnet-build)를 사용하거나 선호하는 IDE에서 Run / Build 옵션을 이용하여 빌드할 수 있습니다.

또는, Docker를 사용할 수 있습니다. 이를 시작하는 쉬운 방법은 `docker compose up`을 실행하는 것입니다.

### 게임 서버와 연결하기

API를 작성하고 컴파일한 후, 다른 곳에서 호스팅하고 싶을 수도 있습니다. 게임 서버가 실행되는 서버와 동일한 서버일 수 있으며 완전히 다른 서버일 수도 있습니다. 보다 원활하고 빠른 통신을 위해 게임 서버와의 지연 시간을 최소화하는 것이 좋습니다. 동일한 `ServerListener`를 동시에 *여러* 게임 서버에 사용할 수 있습니다. 게임 서버의 실행 옵션에서 API 서버(주소와 포트)를 지정할 수 있습니다.

#### 게임 서버 시작 인수

게임 서버는 실행 인수 `-apiendpoint=<IP>:<PORT>`를 사용하여 API에 연결합니다. 여기서 <PORT>는 리스터가 수신하는 포트이고, IP는 API 서버의 IP입니다.

API 서버에서 API 토큰 확인이 필요한 경우, 게임 서버의 시작 인수에 `"-apiToken=<ApiToken>"`을 추가해야 합니다.`<ApiToken>`이 API 서버에 정의된 API 토큰과 같으면 게임 서버는 API 서버와 통신할 수 있습니다.

게임 서버가 시작되면 커맨드라인에 `setapitoken <new token>`을 입력하여 게임 서버의 API 토큰을 직접 수정할 수 있습니다.

#### API 수신 포트 설정

현재 프로젝트의 API는 `29294` 포트에서 수신 대기하도록 구성되어 있습니다. 이를 변경하려면 `listener.start(port)`에서 변경해야 합니다. `29294` 포트는 Docker에 노출되며 Docker Compose에서 호스트의 동일한 포트에 바인딩됩니다. 즉, Docker를 사용할 때 `Dockerfile` 및 `docker-compose.yml` (Compose를 사용할 때)에서도 포트를 변경해야 합니다. [EXPOSE 문서](https://docs.docker.com/engine/reference/builder/#expose) 와 [NETWORKING 문서](https://docs.docker.com/compose/networking/)를 참고하세요.