# Magic' Hood

## 목차
- 게임 소개
- 주요 스크립트
- 기술 스택
- 참고사항

## 게임 소개
- Magic' Hood는 Unity와 Photon PUN2를 기반으로 한 멀티플레이 로비/룸 중심 프로젝트입니다.
- 타이틀 화면부터 로비, 방 생성 및 입장, 채팅, 준비/시작, 라운드 전환까지 이어지는 네트워크 흐름을 중심으로 구성되어 있습니다.
- 인증 시스템과 UI, 플레이어 상호작용 요소를 함께 연결해 온라인 게임의 기본 플레이 흐름의 형태로 구현되었습니다.
<img width="874" height="490" alt="Image" src="https://github.com/user-attachments/assets/a28df6d0-f2ba-4715-ae55-5a2b2d3630d0" />
<img width="868" height="486" alt="Image" src="https://github.com/user-attachments/assets/91fb2ee7-6dbb-49f0-a2ce-ff9d16cd4437" />

## 주요 스크립트
- 양현용이 Magic' Hood 프로젝트에서 작성한 주요 스크립트 모음입니다.
  
## 담당 역할
### 네트워크 / 로비
- [`RoomManager.cs`](https://github.com/Yanghyeonyong/KGAMagicalNetTeam/blob/main/Assets/Scripts/Hyeonyong/Network/RoomManager.cs)
  - 방 입장, 준비/시작, 플레이어 상태 표시, 방 설정 관리 등 멀티플레이 룸 흐름을 담당했습니다.
  - `Start()`: 방 입장 후 BGM 재생, 사용자 정보 갱신, 플레이어 UI 초기화와 준비 상태를 설정합니다.
  - `ReadyGame()` / `CheckReady()`: 로컬 플레이어의 준비 상태를 토글하고, 방 안의 준비 인원을 확인합니다.
  - `StartGame()`: 방장이 게임 시작 요청을 보내고, 룸 씬에서 실제 게임 씬으로 전환합니다.
  - `InitGameRound()` / `InitMoneyCount()`: 라운드와 팀 재화 관련 네트워크 프로퍼티를 초기화합니다.
- [`LobbyManager.cs`](https://github.com/Yanghyeonyong/KGAMagicalNetTeam/blob/main/Assets/Scripts/Hyeonyong/Network/LobbyManager.cs)
  - 로비에서 방 생성 및 진입 흐름을 관리하는 로비 시스템 로직을 담당했습니다.
  - `CreateRoom()` / `JoinRoom()`: 방 생성 및 특정 방 입장 요청을 처리합니다.
  - `JoinRandomRoom()`: 빈 방을 찾아 자동으로 참가하거나 새 방을 생성합니다.
  - `Refresh()` / `OnRoomListUpdate()`: 로비의 방 목록을 새로고침하고, 현재 존재하는 방 버튼을 갱신합니다.
  - `ChangeNickName()`: Firebase 계정의 닉네임을 변경하고 Photon 닉네임과 동기화합니다.
- [`TitleManager.cs`](https://github.com/Yanghyeonyong/KGAMagicalNetTeam/blob/main/Assets/Scripts/Hyeonyong/Network/TitleManager.cs)
  - 타이틀 화면에서 게임 진입 및 네트워크 연결 흐름을 연결하는 역할을 담당했습니다.
  - `ConnectToServer()`: Photon 서버 연결을 시도하고, 연결이 이미 되어 있으면 로비로 바로 진입합니다.
  - `OnConnectedToMaster()` / `OnJoinedLobby()`: 마스터 서버 연결 후 로비 진입 및 로그인 화면 전환을 처리합니다.
  - `QuitGame()`: 에디터와 빌드 환경에서 각각 게임을 종료합니다.
- [`ChattingManager.cs`](https://github.com/Yanghyeonyong/KGAMagicalNetTeam/blob/main/Assets/Scripts/Hyeonyong/Network/ChattingManager.cs)
  - 방 안의 채팅 송수신과 UI 표시를 관리하는 채팅 시스템을 구현했습니다.
  - `SendControl()`: 엔터 입력을 받아 채팅 입력창을 열고, 입력 완료 시 메시지를 전송합니다.
  - `SendMyMessage()`: 입력된 채팅 내용을 RPC로 전파하고 입력창을 초기화합니다.
  - `SendChat()` / `ReceiveMessage()`: 네트워크로 받은 메시지를 UI에 표시하고, 일정 시간 뒤 채팅 패널을 닫습니다.
  - `OnPlayerEnteredRoom()` / `OnPlayerLeftRoom()`: 플레이어 입장·퇴장 알림 메시지를 채팅창에 추가합니다.
  
### UI / 플레이어
- [`UIManager.cs`](https://github.com/Yanghyeonyong/KGAMagicalNetTeam/blob/main/Assets/Scripts/Hyeonyong/UI/UIManager.cs)
  -  게임 메뉴, 설정 패널, 인게임 UI 전환 및 공통 UI 상태를 관리했습니다.
  - `OpenUI()` / `CloseUI()`: UI 패널의 열기와 닫기를 제어합니다.
  - `SetResolutionDropDown()` / `SetResolution()`: 해상도 설정 UI를 초기화하고 사용자 선택에 따라 해상도를 변경합니다.
  - `AddUI()` / `CheckUiClose()`: UI 스택을 관리하고, 현재 열려 있는 패널 상태를 점검합니다.
  - `ExitGame()`: 게임 종료 흐름을 실행합니다.

- [`PlayerView.cs`](https://github.com/Yanghyeonyong/KGAMagicalNetTeam/blob/main/Assets/Scripts/Hyeonyong/UI/PlayerView.cs)
  - 플레이어 HUD 및 체력·쿨타임·상태 표시와 관련된 UI 로직을 다뤘습니다.
  - `SetPlayerInfo()` / `SetMyInfo()`: 플레이어 이름, HP 바, 음성 표시 UI를 각각 바인딩합니다.
  - `UpdatePlayerHP()`: 현재 체력 비율에 맞춰 HP 게이지를 갱신합니다.
  - `SetMagicInfo()` / `SetMagicInfoOnHand()`: 마법 아이콘 UI와 손에 들고 있는 마법 아이콘을 설정합니다.
  - `SetMagicIcon()` / `CheckCoolTimeOnHand()`: 쿨타임이 걸린 마법 아이콘을 표시하고 쿨다운을 갱신합니다.
- [`PlayerController.cs`](https://github.com/Yanghyeonyong/KGAMagicalNetTeam/blob/main/Assets/Scripts/Hyeonyong/UI/PlayerController.cs)
  - 플레이어 입력과 상호작용 동작을 연결하는 컨트롤러 역할을 담당했습니다.
  - `Start()`: 플레이어별로 자신의 UI와 상대방 UI를 생성하고, 이름·HP 정보를 초기화합니다.
  - `TakeDamage()`: 데미지를 받아 플레이어 체력 로직을 호출합니다.
  - `OnMagicInteract()` / `FireballReaction()` / `LightningStrikeReaction()`: 마법 상호작용에 따라 피격, 넉백, 데미지 반응을 처리합니다.
  - `CheckInteractable()`: 해당 마법이 현재 플레이어에게 적용 가능한지 여부를 판단합니다.
- [`PlayerSoundHandler.cs`](https://github.com/Yanghyeonyong/KGAMagicalNetTeam/blob/main/Assets/Scripts/Hyeonyong/UI/PlayerSoundHandler.cs)
  - 플레이어 간 사운드 및 효과음 연동을 관리했습니다.
  - `Start()`: 로컬 플레이어와 다른 플레이어의 오디오 컴포넌트를 구분해 초기화합니다.
  - `SetSoundEvent()`: UI 슬라이더와 음소거 토글 값을 연결해 마이크·음성 볼륨 설정을 반영합니다.
  - `SetMicSound()` / `SetVoiceSound()`: 마이크 및 다른 플레이어 음성의 볼륨을 조절합니다.
  - `SetVoiceMute()` / `SetMicMute()`: 음성 채팅과 마이크 입력의 음소거 상태를 적용합니다.

### 시스템 / 상호작용
- [`FinalRoundManager.cs`](https://github.com/Yanghyeonyong/KGAMagicalNetTeam/blob/main/Assets/Scripts/Hyeonyong/Network/FinalRoundManager.cs)
  - 라운드 전환 및 결승전 흐름을 관리하는 역할을 담당했습니다.
  - `Start()`: 결승/종료 화면에서 스피커 오브젝트와 UI 상태를 초기화합니다.
  - `BackToRoom()`: 마스터 클라이언트가 룸으로 복귀하는 흐름을 시작합니다.
  - `CoLoadRoom()`: 게임 오브젝트 정리 후 잠시 대기한 뒤 룸 씬으로 이동합니다.
- [`GameManager.cs'](https://github.com/Yanghyeonyong/KGAMagicalNetTeam/blob/main/Assets/Scripts/Hyeonyong/Network/GameManager.cs)
  - 게임 진행 흐름, 플레이어 생성, 라운드 및 인게임 상태를 관리했습니다.
  - `SpawnPlayerWhenConnected()`: 네트워크 연결 후 플레이어를 생성하고, 인게임 상태를 초기화합니다.
  - `CheckMoneyCount()` / `UseTeamMoney()` / `CurTeamMoney()`: 팀 재화 상태를 확인하고 사용/조회합니다.
  - `PlusMoneyCount()` / `InitMoneyCountAndStore()`: 재화 증가 및 라운드 시작 시 재화 상태를 갱신합니다.
  - `CheckDie()` / `CheckRoundClear()`: 플레이어 사망 및 라운드 클리어 조건을 판정합니다.
  - `ResetCustomProperty()`: 라운드가 끝난 뒤 커스텀 프로퍼티를 초기화합니다.

### 인증 / 데이터
- [`FirebaseAuthManager.cs`](https://github.com/Yanghyeonyong/KGAMagicalNetTeam/blob/main/Assets/Scripts/Hyeonyong/DataBase/FirebaseAuthManager.cs)
  - 로그인, 회원가입, 사용자 인증 흐름을 처리하는 인증 시스템을 담당했습니다.
  - `Init()`: 로그인 UI 초기 상태를 설정합니다.
  - `Login()` / `Register()`: 이메일과 비밀번호를 이용해 로그인 또는 회원가입 요청을 시작합니다.
  - `RegisterCoroutine()` / `LoginCoroutine()`: Firebase 인증 결과를 기다리고, 성공 시 사용자 정보를 저장합니다.
  - `CheckEmail()` / `OnCheckEmail()`: 이메일 중복 및 가입 여부를 확인해 UI를 분기합니다.
  - `ChangeNickName()` / `RefreshUser()`: 닉네임 변경과 최신 사용자 정보 갱신을 처리합니다.
- [`LoginManager.cs`](https://github.com/Yanghyeonyong/KGAMagicalNetTeam/blob/main/Assets/Scripts/Hyeonyong/DataBase/LoginManager.cs)
  - 로그인 화면 흐름과 인증 연결 로직을 관리했습니다.
  - `NextTab()`: 입력 필드 간 탭 이동을 처리합니다.
  - `EnterGame()`: 엔터 입력 시 현재 활성화된 버튼을 실행합니다.
  - `SetTabNum()`: 현재 포커스된 입력 필드 인덱스를 갱신합니다.
  - `ReturnToMainMenu()`: 메인 메뉴로 돌아가는 씬 전환을 수행합니다.

## 기술 스택
- Unity 6000.2.10f
- Photon PUN2
- URP 17.2.0
- Firebase Authentication
- Input System 1.18.0

## 참고사항
- 로비와 룸 흐름을 분리해 멀티플레이 진행 과정을 명확하게 관리합니다.
- UI, 플레이어 동작, 사운드 처리, 인증 흐름을 모듈화하여 확장성과 유지보수성을 고려했습니다.
- Photon 기반 실시간 네트워크와 Firebase 인증을 함께 결합해 온라인 게임 운영에 필요한 기본 요소를 구현했습니다.
