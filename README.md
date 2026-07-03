# Magic' Hood
## 담당 역할
### 네트워크 / 로비
- [`RoomManager.cs`](https://github.com/Yanghyeonyong/KGAMagicalNetTeam/blob/main/Assets/Scripts/Hyeonyong/Network/RoomManager.cs) : 방 입장, 준비/시작, 플레이어 상태 표시, 방 설정 관리 등 멀티플레이 룸 흐름을 담당했습니다.
- [`LobbyManager.cs`](https://github.com/Yanghyeonyong/KGAMagicalNetTeam/blob/main/Assets/Scripts/Hyeonyong/Network/LobbyManager.cs) : 로비에서 방 생성 및 진입 흐름을 관리하는 로비 시스템 로직을 담당했습니다.
- [`TitleManager.cs`](https://github.com/Yanghyeonyong/KGAMagicalNetTeam/blob/main/Assets/Scripts/Hyeonyong/Network/TitleManager.cs) : 타이틀 화면에서 게임 진입 및 네트워크 연결 흐름을 연결하는 역할을 담당했습니다.
- [`ChattingManager.cs`](https://github.com/Yanghyeonyong/KGAMagicalNetTeam/blob/main/Assets/Scripts/Hyeonyong/Network/ChattingManager.cs) : 방 안의 채팅 송수신과 UI 표시를 관리하는 채팅 시스템을 구현했습니다.
  
### UI / 플레이어
- [`UIManager.cs`](https://github.com/Yanghyeonyong/KGAMagicalNetTeam/blob/main/Assets/Scripts/Hyeonyong/UI/UIManager.cs) : 게임 메뉴, 설정 패널, 인게임 UI 전환 및 공통 UI 상태를 관리했습니다.
- [`PlayerView.cs`](https://github.com/Yanghyeonyong/KGAMagicalNetTeam/blob/main/Assets/Scripts/Hyeonyong/UI/PlayerView.cs) : 플레이어 HUD 및 체력·쿨타임·상태 표시와 관련된 UI 로직을 다뤘습니다.
- [`PlayerController.cs`](https://github.com/Yanghyeonyong/KGAMagicalNetTeam/blob/main/Assets/Scripts/Hyeonyong/UI/PlayerController.cs) : 플레이어 입력과 상호작용 동작을 연결하는 컨트롤러 역할을 담당했습니다.
- [`PlayerSoundHandler.cs`](https://github.com/Yanghyeonyong/KGAMagicalNetTeam/blob/main/Assets/Scripts/Hyeonyong/UI/PlayerSoundHandler.cs) : 플레이어 간 사운드 및 효과음 연동을 관리했습니다.

### 시스템 / 상호작용
- [`FinalRoundManager.cs`](https://github.com/Yanghyeonyong/KGAMagicalNetTeam/blob/main/Assets/Scripts/Hyeonyong/Network/FinalRoundManager.cs) : 라운드 전환 및 결승전 흐름을 관리하는 역할을 담당했습니다.
- [`GameManager.cs'](https://github.com/Yanghyeonyong/KGAMagicalNetTeam/blob/main/Assets/Scripts/Hyeonyong/Network/GameManager.cs) : 게임 진행 흐름, 플레이어 생성, 라운드 및 인게임 상태를 관리했습니다.

### 인증 / 데이터
- [`FirebaseAuthManager.cs`](https://github.com/Yanghyeonyong/KGAMagicalNetTeam/blob/main/Assets/Scripts/Hyeonyong/DataBase/FirebaseAuthManager.cs) : 로그인, 회원가입, 사용자 인증 흐름을 처리하는 인증 시스템을 담당했습니다.
- [`LoginManager.cs`](https://github.com/Yanghyeonyong/KGAMagicalNetTeam/blob/main/Assets/Scripts/Hyeonyong/DataBase/LoginManager.cs) : 로그인 화면 흐름과 인증 연결 로직을 관리했습니다.
