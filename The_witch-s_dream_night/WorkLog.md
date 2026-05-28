## 📅 작업 일시: 2026년 3월 27일

### 🛠 1. 주요 수정 및 추가 스크립트 (Scripts)
- **[추가] VNBacklogEntry.cs**: 백로그의 개별 대화 항목(Prefab)을 관리합니다. 화자/내용 가로 배치 및 자동 줄바꿈 지원.
- **[추가] VNBacklogManager.cs**: 전체 백로그 리스트의 생성, 삭제, UI 가시성을 관리합니다. 최대 100개 제한.
- **[수정] VNUIController.cs**: 기존의 단일 텍스트 백로그 로직을 제거하고 VNBacklogManager로 역할을 위임하도록 리팩토링.
- **[수정] VnInkBootstrap.cs**: IEnumerator 사용을 위한 네임스페이스 추가 및 코드 정리.
- **[추가] VNBacklogSetupTool.cs (Editor 전용)**: 유니티 에디터 내에서 클릭 한 번으로 모든 UI 구조를 자동 생성 및 연결하는 도구.

### 🏗 2. 유니티(Unity) 작업 내용 및 방법
- **백로그 UI 자동 구성 시스템**: 'VN Tools -> Setup Final Improved Backlog' 메뉴를 통해 독립 캔버스 및 프리팹 연결 자동화.

---

## 📅 작업 일시: 2026년 3월 28일 (아키텍처 가이드 및 기초 설계)

### 🏗 프로젝트 구현 원칙 및 아키텍처 개요
- **구현 원칙**: 모듈형 구조 지향, UI/스토리/연출/저장 책임 분리, 기존 동작을 깨지 않는 점진적 확장.
- **Layer 1: Story**: InkStoryEngine (Ink JSON 로드 및 라인 반환)
- **Layer 2: Flow Control**: VNDirector (전체 VN 진행 제어 중심)
- **Layer 3: UI**: VNUIController 및 하위 모듈 (Dialogue, Choice, Backlog)
- **Layer 4: Presentation**: VNPresenter & CommandProcessor (태그 해석 및 화면 연출)
- **Layer 5: State / Save**: VNSaveService (저장 및 불러오기)

---

## 📅 작업 일시: 2026년 3월 28일 (실제 구현 및 아키텍처 고도화)

### 🛠 1. 계층별 모듈 구현 및 리팩토링 완료
- **[추가] VNDirector.cs**: 프로젝트의 메인 오케스트레이터. 스토리 루프, 태그 명령 하달, UI 출력 요청, 세이브/로드 통합 제어 수행.
- **[추가] VNTextTyper.cs**: 텍스트 타이핑 연출 및 스킵 로직 전담 (VNUIController에서 분리).
- **[추가] VNChoicePanel.cs**: 선택지 UI 생성 및 버튼 이벤트 관리 전담 (VNUIController에서 분리).
- **[추가] VNCommandProcessor.cs**: Ink 태그(#bg, #ch, #bgm 등)를 해석하여 연출 명령으로 변환.
- **[추가] VNPresenter.cs / IVNPresenter.cs**: 배경/캐릭터/사운드 연출의 실제 구현 및 인터페이스 규칙 수립.
- **[추가] VNAssetDatabase.cs**: 문자열 키와 실제 유니티 에셋(Sprite, AudioClip)을 매핑하는 데이터베이스.
- **[추가] VNSaveData.cs / VNSaveService.cs**: JSON 기반 로컬 파일 세이브/로드 시스템 구축.

### 🐞 주요 버그 수정 및 개선
- **백로그 닫기 문제**: `VNBacklogManager`에 자가 치유(Self-healing) 로직을 추가하여 버튼 참조 및 이벤트 연결 자동화.
- **태그 미작동 문제**: `VNDirector`가 `IVNPresenter` 구현체를 씬 전체에서 정밀하게 필터링하여 찾도록 수정.
- **로그 정리**: 모든 주요 스크립트에서 불필요한 디버깅용 `Debug.Log`를 제거하여 코드 최적화.

### 🔧 신규 에디터 자동화 도구
- **VN Tools / Setup Core System**: `Director`와 `Bootstrap`을 생성하고 상호 연결.
- **VN Tools / Setup UI Modules (Final Fix)**: `Typer`, `ChoicePanel`, `Backlog`의 비어있는 모든 참조를 자동으로 찾아 연결.

---

## 📅 업데이트 날짜: 2026년 4월 10일

### 🛠 1. 연출 시스템 고도화 (Visual Polish)
- **[수정] VNPresenter.cs**: 배경 크로스페이드 및 캐릭터 페이드 인/아웃 기능을 코루틴으로 구현.
    - `FadeBackgroundRoutine`: 새 배경을 임시 오브젝트로 생성하여 알파값을 조절하며 교체.
    - `FadeCharacterRoutine`: `CanvasGroup`을 활용하여 캐릭터 등장/퇴장 시 부드러운 투명도 변화 적용.
- **[추가] 화면 흔들기 (Screen Shake)**: 
    - `#shake [시간] [강도]` 명령어를 통해 전체 UI 패널을 무작위로 흔드는 연출 추가.
    - `masterUIRoot` 참조를 통해 독립적인 UI 레이어 진동 지원.
- **[수정] VNCommandProcessor.cs**: 연출 명령어 파싱 로직 대폭 강화.
    - `#bg`, `#ch`, `#hide` 태그 뒤에 `fade` 키워드를 붙여 트랜지션 여부를 결정하도록 수정.
    - `#ch` 명령 시 표정을 생략해도 위치값(`left`, `right` 등)을 자동 감지하도록 지능화.

### 🛠 2. 핵심 편의 기능 보완 (Core UX Improvements)
- **[개선] 지능형 오토(Auto) 모드**: 
    - 대사 길이에 비례하여 대기 시간을 자동 계산하는 로직 적용 (`autoBaseWait + lineLength * autoPerCharWait`).
    - 플레이어 설정에 따라 대기 시간을 배속할 수 있는 `autoWaitMultiplier` 시스템 구축.
- **[개선] 반응형 스킵(Skip) 모드**:
    - 스킵 토글을 켜는 즉시 대기 중인 상태를 깨고 다음 대사로 진행하도록 로직 수정.
    - 스킵 중에도 대사가 "지나가는 느낌"을 주기 위해 찰나의 시각적 대기(약 0.08초) 추가.
    - 스킵 모드 활성 시 모든 페이드 연출 시간을 1/10 수준으로 단축하여 고속 진행 지원.
- **[추가] 커스텀 텍스트 속도**:
    - `VNTextTyper.cs`에서 `SetSpeed` 메서드를 통해 타이핑 속도를 실시간 조절 가능하도록 수정.

### 🏗 3. 캐릭터 자산 관리 리팩토링
- **[수정] VNAssetDatabase.cs**: 캐릭터 이미지를 '이름'과 '표정 리스트'로 그룹화하여 관리하는 구조로 변경.
    - 표정 미지정 시 자동으로 `Default Sprite`를 사용하도록 폴백 로직 구현.
    - 유니티 인스펙터에서의 캐릭터 자산 관리 편의성 대폭 향상.

### 🐞 버그 수정 및 안정화
- **배경 로드 오류 수정**: 배경이 없는 초기 지점을 로드할 때 이전 세션의 배경이 남아있던 문제 해결 (`SetBackground`에 빈 키 전달 시 명시적 초기화 추가).
- **컴파일 오류 해결**: `VNTextTyper` 변수명 불일치 및 `VNPresenter` 중괄호 중복 문제 수리.

---

## 📅 업데이트 날짜: 2026년 4월 11일

### 🛠 1. 게임 설정(Settings) 시스템 구축
- **[추가] VNSettingsData.cs**: `PlayerPrefs`와 JSON을 결합하여 오디오 볼륨, 텍스트 속도 등 사용자 설정을 영구 저장하는 데이터 클래스.
- **[수정] VNOptionPanel.cs**: 설정 UI의 핵심 오케스트레이터로 대폭 리팩토링.
    - **이중 레이어 구조**: '일시정지 메뉴(버튼)'와 '상세 설정창(슬라이더/탭)'을 분리하여 관리.
    - **지연 적용(Apply) 시스템**: 슬라이더 조작 시 즉시 반영되지 않고, [APPLY] 버튼 클릭 시에만 실제 시스템에 적용 및 저장되도록 구현.
    - **타이틀 모드 지원**: `MenuRoot`가 없는 환경(타이틀 화면 등)에서도 설정창 전용으로 작동할 수 있도록 지능형 분기 로직 추가.
- **[수정] VNPresenter.cs**: 전역 볼륨(`master`, `bgm`, `sfx`) 제어 메서드 추가 및 재생 시 설정값 자동 반영 로직 구축.

### 🏠 2. 타이틀 화면(Title Screen) 시스템 구현
- **[추가] VNTitleManager.cs**: 타이틀 화면의 전체 흐름 제어.
    - **이어하기(Continue)**: 가장 최근에 저장된 슬롯을 찾아 즉시 게임 씬으로 연결.
    - **UI 연동**: 설정 및 세이브/로드 패널을 타이틀에서 직접 호출하여 독립적으로 작동하게 함.
    - **페이드 연출**: 씬 전환 시 부드러운 검은색 페이드 인/아웃 지원.
- **[추가] VNButtonEffects.cs**: 마우스 호버(Hover) 시 버튼 확대 및 클릭 시 축소 애니메이션을 처리하는 공용 컴포넌트.
- **[수정] VnInkBootstrap.cs**: 타이틀에서 넘어온 로드 요청(`LoadOnStart_Slot`)이 있을 경우, 초기 Knot 시작 대신 세이브 데이터를 로드하도록 부팅 로직 수정.

### 🔧 3. 에디터 자동화 도구 고도화
- **[수정] VNUISetupTool.cs**: 
    - **Professional Option UI Build**: 상단 탭 + 4컬럼 그리드 레이아웃 구조의 전문 설정창을 자동으로 생성. 폰트 깨짐(네모 칸) 방지를 위해 프로젝트 폰트 자동 할당.
    - **Setup Title Scene**: 캔버스, 로고, 메뉴 버튼, 페이더, 이벤트 시스템을 원클릭으로 셋업.
    - **안전한 리빌드**: 기존 중복 이벤트를 모두 지우고 최신 런타임 이벤트를 강제로 재연결하는 '이벤트 정리' 로직 포함.
- **[수정] VNSaveLoadUIBuilder.cs**: `VNUIController` 부재 상황(타이틀 씬 등)에서도 Canvas만 있다면 독립적인 세이브/로드 패널을 구축할 수 있도록 유연성 강화.

### 🐞 주요 버그 수정 및 안정화
- **NullReferenceException 방지**: `settings` 데이터 로드 전 참조 시도를 막기 위해 모든 접근 시점에 `EnsureSettings()` 초기화 보강.
- **입력 차단 해결**: `SceneFader` 이미지가 버튼을 가려 클릭을 방해하던 문제 해결 (`raycastTarget = false`).
- **정렬 문제 해결**: 세이브 슬롯 그리드가 어긋나 보이던 현상을 `GridLayoutGroup` 정밀 설정을 통해 3열 정렬로 고정.
- **컴파일 오류 수리**: `GridLayoutGroup.Corner` 열거형 명칭 오류 및 변수명 오타(`soOption`) 해결.

---

### 🔍 4. 현재 시스템 구조 및 데이터 처리 방식 분석

| 모듈명 | 핵심 로직 및 동작 원리 |
| :--- | :--- |
| **VNTitleManager** | 타이틀 씬 전담 매니저. `PlayerPrefs`를 통해 게임 씬으로 로드 명령을 전달. |
| **VNOptionPanel** | `Time.timeScale = 0`을 통해 메뉴 오픈 시 게임 일시정지 수행. [APPLY] 버튼으로만 설정 확정. |
| **VNSettingsData** | 오디오(0~1), 텍스트 속도(0~1), 오토 대기(0.5~1.5) 값을 관리하고 로컬에 저장. |
| **VNSaveLoadPanel** | `director` 유무를 체크하여 '현재 씬 로드'와 '씬 전환 후 로드'를 지능적으로 선택. |

---

### 🚀 향후 작업 로드맵 (업데이트됨)

#### **A. 캐릭터 연출 심화 (Visual Polish)**
- **[추가] 캐릭터 흔들기**: 놀람 연출 등을 위한 개별 캐릭터 쉐이크 태그 (`#ch_shake [이름]`).
- **[추가] 위치 이동 애니메이션**: 특정 위치로 슬라이드하며 이동하는 연출 지원.

#### **B. 수집 및 편의 요소**
- **[추가] CG 갤러리**: 게임 중 해금된 일러스트를 다시 볼 수 있는 독립된 갤러리 모드.
- **[추가] 로그 복구**: 세이브 로드 시 현재 보고 있는 대사가 백로그에 바로 반영되지 않는 현상 점검 및 개선.

#### **C. 사운드 시스템 완성**
- **[추가] 효과음(SFX) 풀링**: 여러 효과음을 동시에 재생할 수 있도록 시스템 확장.
- **[추가] 탭 클릭음**: 설정창이나 타이틀 버튼 클릭 시 피드백 사운드 자동 재생.

---

## 📅 업데이트 날짜: 2026년 4월 18일 (프로젝트 구조 재분석 및 실제 씬 연결 상태 점검)

### 🧭 1. 이번 점검의 목적과 검증 방법
- **목적**: 문서상 아키텍처와 실제 유니티 씬/스크립트/에셋 연결 상태가 일치하는지 검증하고, 향후 작업 우선순위를 재정렬하기 위함.
- **점검 범위**:
  - `Assets/Scenes/SampleScene.unity`
  - `Assets/Scenes/TitleScene.unity`
  - `Assets/Scripts/*.cs`
  - `Assets/Scripts/Editor/*.cs`
  - `Assets/Ink/main.ink`, `Assets/Ink/main.json`
  - `Assets/ScriptObjects/VNAssetDatabase.asset`
  - `Packages/manifest.json`
  - `ProjectSettings/EditorBuildSettings.asset`, `ProjectSettings/ProjectVersion.txt`
- **검증 방식**:
  - 씬 YAML을 직접 읽어 실제 컴포넌트 참조와 GameObject 배치를 확인.
  - Ink 태그와 `VNCommandProcessor`, `VNAssetDatabase`의 키 매핑 상태를 대조.
  - 런타임/에디터 C# 프로젝트 빌드로 문법 및 참조 이상 여부를 확인.
  - Unity 공식 문서 및 inkle 공식 저장소를 기준으로 프로젝트 폴더 구조와 Ink 운용 방식을 교차 검증.

### 🏗 2. 현재 프로젝트의 실제 상위 구조 요약
- **Unity 버전 확인**: `ProjectVersion.txt` 기준 현재 에디터 버전은 `6000.3.5f2`.
- **프로젝트 폴더 구조**:
  - `Assets`: 게임 리소스/스크립트/씬/설정/프리팹 보관.
  - `Packages`: UPM 패키지 의존성 관리.
  - `ProjectSettings`: 에디터/플레이어/빌드 설정.
  - `Library`: 유니티 내부 캐시 및 임포트 결과 저장소.
  - `Logs`, `Temp`, `UserSettings`: 실행 로그, 임시 산출물, 사용자별 설정.
- **실제 사용 패키지 핵심 요약**:
  - `com.inkle.ink-unity-integration`
  - `com.unity.render-pipelines.universal`
  - `com.unity.inputsystem`
  - `com.unity.ugui`
  - `com.unity.visualscripting`
- **현재 활성 씬 파일**:
  - `Assets/Scenes/SampleScene.unity`
  - `Assets/Scenes/TitleScene.unity`

### 🎮 3. SampleScene 실제 연결 구조 분석

#### 3-1. 핵심 런타임 오브젝트 구성
- **`VNInkBootStrap` 오브젝트 존재 확인**:
  - `VnInkBootstrap`가 연결되어 있으며, `compiledInkJson`은 `Assets/Ink/main.json`을 참조.
  - `startKnot`는 `start`.
- **`VNDirector` 오브젝트 존재 확인**:
  - `VNDirector`가 별도 GameObject로 존재.
  - `ui`는 `VNUIController`를 참조.
  - `presenterComponent`는 `VNPresenter`를 참조.
- **`VNUIController` 오브젝트 존재 확인**:
  - 동일 GameObject에 `VNBacklogManager`, `VNTextTyper`, `VNChoicePanel`이 함께 붙어 있음.
  - 대사/선택지/백로그/옵션/세이브 패널 참조가 모두 연결되어 있음.
- **`VNPresenter` 오브젝트 존재 확인**:
  - `VNAssetDatabase.asset`를 참조.
  - `backgroundImage`, `characterRoot`, `masterUIRoot`는 연결되어 있음.
  - `characterPrefab`는 비어 있음. 다만 현재 코드는 null이면 런타임에 임시 UI 오브젝트를 생성하므로 즉시 치명적이지는 않음.

#### 3-2. UI 모듈 연결 상태
- **옵션 패널(`OptionPanel`)**
  - `ui`, `presenter`, 슬라이더/토글/버튼, `menuRoot`, `settingsRoot`가 연결되어 있음.
  - 인게임 일시정지 메뉴 + 상세 설정창 구조가 실제 씬에도 반영됨.
- **세이브/로드 패널(`SaveLoadPanel`)**
  - `director`, `slotRoot`, `slotPrefab`, 닫기 버튼, 저장/불러오기 탭 버튼이 연결되어 있음.
  - `titleText`는 비어 있음. 기능상 즉시 치명적이지는 않지만 모드 제목 표시가 생략될 수 있음.
- **백로그**
  - `VNBacklogManager`가 `VNUIController`에 붙어 있으며, 저장 시 백로그 복구 구조와 연결됨.

#### 3-3. 현재 SampleScene에서 확인된 핵심 리스크
- **오디오 출력 컴포넌트 미연결**:
  - `VNPresenter`의 `bgmSource`, `sfxSource`가 둘 다 비어 있음.
  - 따라서 `PlayBGM`, `PlaySFX`, `SetVolume` 일부 로직이 조기 종료되어 실제 사운드가 재생되지 않을 가능성이 매우 높음.
- **사운드 데이터도 미완성 상태**:
  - `VNAssetDatabase.asset`의 `bgm` 목록에 유효한 `forest` 키가 없음.
  - 스토리에서는 `#bgm forest`를 사용 중이므로, 데이터 기준으로는 재생 대상이 존재하지 않음.

### 🏠 4. TitleScene 실제 연결 구조 분석

#### 4-1. 핵심 오브젝트 구성
- **`TitleManager` 오브젝트 존재 확인**:
  - `VNTitleManager`가 연결되어 있음.
  - `gameSceneName`은 `SampleScene`.
  - `optionPanel`, `saveLoadPanel`, 시작/이어하기/로드/옵션/종료 버튼, `SceneFader`, 버전 텍스트가 연결되어 있음.
- **`OptionPanel` 오브젝트 존재 확인**:
  - 타이틀 모드용 설정창으로 배치되어 있음.
  - `ui = null`, `presenter = null`, `menuRoot = null`, `settingsRoot`만 연결.
  - 즉, 타이틀 씬에서는 “설정창 전용 모드”로 사용되도록 구성됨.
- **`SaveLoadPanel` 오브젝트 존재 확인**:
  - `director = null` 상태.
  - 타이틀 씬에서 세이브는 불가하고, 불러오기는 `PlayerPrefs`에 슬롯 번호를 저장한 뒤 `SampleScene`으로 전환하는 분기 구조와 맞물림.

#### 4-2. 타이틀 씬에서 확인된 핵심 리스크
- **`VNPresenter` 부재**:
  - `TitleScene`에는 `VNPresenter`가 존재하지 않음.
  - `VNTitleManager`는 `FindFirstObjectByType<VNPresenter>()`로 타이틀 BGM 재생을 시도하지만, 실제 씬에는 찾을 대상이 없음.
  - 결과적으로 현재 구조상 타이틀 BGM은 재생되지 않을 가능성이 높음.
- **타이틀 설정창 닫기 동선 문제**:
  - `VNOptionPanel`의 `closeButton`은 `ShowMainMenu()`에 연결됨.
  - 하지만 타이틀 씬용 `OptionPanel`은 `menuRoot = null`이라 `ShowMainMenu()`가 즉시 반환됨.
  - 즉, 현재 구조상 타이틀에서 설정창을 열었을 때 `APPLY`를 누르지 않으면 정상적으로 닫기 어렵다.
  - `UIController`가 없는 씬이므로 인게임처럼 `Esc` 처리도 기대하기 어렵다.

### 🧩 5. Ink 스토리와 명령 태그 연결 상태 분석

#### 5-1. `main.ink`에서 실제 사용 중인 태그
- `#ch Watari center fade`
- `#speaker Scenario`
- `#speaker Player`
- `#speaker 마녀`
- `#bg Forest`
- `#bg City fade`
- `#bgm forest`
- `#shake 0.4 40`

#### 5-2. 태그 해석기와의 호환성 점검 결과
- **정상 호환 확인**:
  - `#speaker`는 `InkStoryEngine.ExtractSpeakerFromTags()`가 추출 가능.
  - `#bg`, `#ch`, `#shake`, `#bgm`는 `VNCommandProcessor`가 처리 가능.
  - `#ch Watari center fade`처럼 표정을 생략하고 위치/페이드를 함께 쓰는 형태도 현재 파서 로직으로 소화 가능.
- **실제 에셋 매핑 확인**:
  - `Forest`, `City` 배경 키는 `VNAssetDatabase.asset`에 존재.
  - `Watari` 캐릭터 키도 존재.
- **불일치 확인**:
  - `forest` BGM 키는 현재 데이터베이스에 없음.
  - `VNTitleManager`의 `title_theme` 역시 현재 데이터베이스에 없음.

### 💾 6. 저장/불러오기 및 설정 시스템 분석

#### 6-1. 저장 데이터에 실제로 포함되는 것
- `inkStateJson`
- 현재 배경 키
- 현재 BGM 키
- 활성 캐릭터 상태 목록
- 마지막 화자 / 마지막 대사
- 챕터명 / 플레이 시간
- 백로그 전체

#### 6-2. 현재 구조의 장점
- 세이브 시 Ink 상태와 UI/연출 상태를 함께 저장하므로, 단순 텍스트 복원보다 풍부한 상태 복구가 가능함.
- 타이틀 씬과 인게임 씬이 동일한 저장 UI 로직을 공유하면서도, `director` 유무로 분기하여 역할을 나눔.
- 백로그를 세이브 데이터에 포함한 점은 비주얼노벨 UX 측면에서 좋은 방향.

#### 6-3. 현재 구조의 누락/아쉬운 지점
- `VNSaveSlotItem`에는 `dateText` 필드가 존재하지만, `VNSaveLoadUIBuilder.CreateSlotTemplate()`에서 이 참조를 연결하지 않음.
- 그 결과 세이브 날짜를 저장하더라도, 자동 생성된 슬롯 UI에 날짜가 표시되지 않을 가능성이 높음.
- 썸네일도 아직 TODO 상태라 저장 슬롯 정보 밀도가 낮음.

### 🧪 7. 빌드 및 안정성 점검 결과
- **C# 빌드 결과**:
  - `Assembly-CSharp.csproj` 빌드 통과.
  - `Assembly-CSharp-Editor.csproj` 빌드 통과.
- **현재 확인된 경고**:
  - `TMP_Text.enableWordWrapping` 사용이 obsolete 경고를 발생시킴.
  - 현재 즉시 치명적이지는 않지만, TextMeshPro 최신 API(`textWrappingMode`)로 차후 정리 필요.
- **중요 해석**:
  - 현재 프로젝트는 “컴파일이 깨진 상태”는 아니지만, “씬 참조와 데이터 연결이 일부 비어 있어 기능이 조용히 비활성화되는 상태”에 더 가깝다.

### 🚨 8. 이번 분석 기준 우선 해결 필요 항목

#### **Priority 1. 오디오 시스템 실사용 가능 상태 만들기**
- 이유:
  - 현재 `SampleScene`의 `VNPresenter`에 `bgmSource`, `sfxSource`가 비어 있음.
  - `VNAssetDatabase`에도 `forest`, `title_theme` 등 필요한 키가 없음.
  - 스토리와 타이틀이 모두 사운드 연출을 호출하고 있으나, 실질적으로 동작하지 않을 가능성이 큼.
- 필요한 작업:
  - 씬에 `AudioSource` 배치 및 `VNPresenter` 연결.
  - `VNAssetDatabase`에 BGM/SFX 키 정식 등록.
  - 타이틀 씬용 오디오 재생 경로를 `VNPresenter` 기반으로 통일할지, `VNTitleManager` 내부 전용 오디오 플레이어를 둘지 결정.

#### **Priority 2. 타이틀 설정창 닫기 UX 수정**
- 이유:
  - 현재 타이틀에서 옵션창을 열면 `Close` 버튼이 논리적으로 동작하지 않을 가능성이 큼.
  - 사용자가 설정 변경 없이 닫고 싶을 때 막히는 UX 결함.
- 필요한 작업:
  - `menuRoot == null`인 타이틀 모드에서는 `closeButton`이 `Close()` 또는 `ShowSettings` 반대 동작으로 연결되도록 분기 수정.
  - 타이틀 씬 단독 입력(Esc) 보조 처리 여부도 검토.

#### **Priority 3. 빌드 시작 씬 순서 재검토**
- 이유:
  - 현재 `EditorBuildSettings.asset` 기준 빌드 순서가 `SampleScene` → `TitleScene`.
  - 프로젝트 의도가 타이틀 화면 진입형이라면 빌드 첫 진입 씬이 잘못 설정된 상태일 수 있음.
- 필요한 작업:
  - 실제 배포 의도 확인 후 `TitleScene`을 첫 씬으로 재정렬할지 결정.

#### **Priority 4. 세이브 슬롯 정보 밀도 개선**
- 이유:
  - `dateText` 미연결, 썸네일 미구현으로 슬롯 판독성이 낮음.
- 필요한 작업:
  - `dateText` 참조 연결.
  - 썸네일 저장/복원 방식 설계.

### 🛠 9. 이번 분석 후 제안한 개선안 3가지와 선택 결과
- **개선안 A**: 오디오 시스템부터 우선 복구하여 태그/설정/타이틀 BGM이 실제로 들리게 만들기.
- **개선안 B**: 타이틀 옵션창 닫기 UX를 먼저 수정하여 사용자가 막히지 않도록 하기.
- **개선안 C**: 이번 분석 내용을 문서화하여 이후 모든 작업자가 현재 구조와 리스크를 동일하게 이해하도록 하기.
- **이번에 선택한 개선안**: **C안**
  - 이유:
    - 지금 단계에서는 구조 파악과 분석이 목적이므로, 가장 먼저 해야 할 일은 “현재 상태의 정확한 기록”을 남기는 것.
    - 오디오/타이틀 UX 수정은 다음 작업에서 바로 이어갈 수 있지만, 그 전에 현재 리스크와 실제 연결 상태를 AGENTS.md에 남겨 두어야 이후 수정 근거가 일관됨.
- **프로젝트 반영 내용**:
  - 본 재분석 결과를 AGENTS.md에 상세 기록하여, 이후 작업의 기준 문서로 사용할 수 있도록 최신화함.

### 📌 10. 다음 작업 시 바로 착수 가능한 추천 순서
1. `VNPresenter` 오디오 연결과 `VNAssetDatabase` BGM 키 정비.
2. 타이틀 설정창 닫기 흐름 수정.
3. 빌드 시작 씬 순서 정리.
4. 세이브 슬롯 날짜/썸네일 정보 보강.

---

## 📅 업데이트 날짜: 2026년 4월 18일 (오디오 연결 자동화 및 설정 UI 볼륨 반영 보강)

### 🛠 1. 작업 목표
- **요청 목적**: 오디오 음원 자체는 나중에 추가하더라도, 현재 시점에서 오디오 시스템의 연결 구조와 설정 UI의 볼륨 적용 경로를 먼저 완성해 두는 것.
- **핵심 요구사항 해석**:
  - 씬에 `AudioSource`를 수동으로 붙이지 않아도 런타임에서 자동으로 준비될 것.
  - 인게임(`SampleScene`)과 타이틀(`TitleScene`) 모두에서 설정 UI의 `master / bgm / sfx` 값이 실제 오디오 재생 경로에 반영될 것.
  - 아직 실제 `AudioClip`이 비어 있어도, 나중에 `VNAssetDatabase`에 키와 클립만 넣으면 바로 작동할 것.

### 🔍 2. 작업 전 검토한 3가지 방법과 선택
- **방법 A**: `SampleScene`, `TitleScene`에 직접 `AudioSource`를 배치하고 모든 참조를 씬에 수동 연결.
  - 장점: 코드 변경이 적음.
  - 단점: 씬마다 수동 배선이 필요하고, 참조가 다시 비면 동일 문제가 반복될 수 있음.
- **방법 B**: `VNPresenter`, `VNTitleManager`가 런타임에 필요한 `AudioSource`를 자동 생성하고, 설정 UI는 해당 관리자들을 자동 탐색하여 볼륨을 적용.
  - 장점: 현재 요청한 “연결 부분”에 가장 직접적이며, 이후 음원만 추가하면 동작함.
  - 단점: 코드가 약간 늘어나지만 유지보수성은 오히려 좋아짐.
- **방법 C**: 별도 전역 오디오 매니저를 새로 설계하여 모든 사운드 경로를 그쪽으로 강제 통합.
  - 장점: 장기적으로는 구조가 더 깔끔해질 수 있음.
  - 단점: 현재 요청 범위를 넘는 리팩토링이며, 기존 구조와의 접합 비용이 큼.
- **최종 선택**: **방법 B**
  - 이유: 현재 프로젝트의 기존 구조를 크게 깨지 않으면서, 사용자가 요청한 “오디오 연결”과 “설정 UI 볼륨 반영”을 가장 안정적으로 만족함.

### 🧩 3. 실제 반영한 코드 변경 사항

#### **A. `VNPresenter.cs` 오디오 자동 연결 보강**
- `bgmSource`, `sfxSource`가 비어 있어도 런타임에 자식 오브젝트(`BGMSource`, `SFXSource`)를 자동 생성하도록 수정.
- 자동 생성된 `AudioSource`에 대해 다음 기본값을 강제 설정:
  - `playOnAwake = false`
  - `loop`는 BGM/SFX 용도에 따라 자동 지정
  - `spatialBlend = 0f`
- `SetVolume()` 호출 시 오디오 소스가 없으면 먼저 보장한 뒤 볼륨을 적용하도록 수정.
- `ApplyVolume()`에서 BGM뿐 아니라 SFX 소스의 기본 볼륨도 함께 갱신하도록 보강.
- `PlayBGM()`에서 빈 키가 들어오면 기존 BGM을 정지하고 클립 참조를 비우도록 수정.
- `PlayBGM()`에서 같은 클립이 이미 연결돼 있다면 불필요한 재시작을 줄이되, 정지 상태면 다시 재생되도록 처리.
- `PlaySFX()`도 오디오 소스 보장 후 재생하도록 보강.

#### **B. `VNTitleManager.cs` 타이틀 전용 오디오 폴백 경로 추가**
- 타이틀 씬에는 `VNPresenter`가 없을 수 있으므로, `VNTitleManager`가 자체적으로 타이틀 오디오를 다룰 수 있게 확장.
- 신규 직렬화 필드 추가:
  - `assetDatabase`
  - `titleBgmSource`
  - `titleSfxSource`
- `VNPresenter`가 씬에 있으면 기존처럼 `VNPresenter` 경로를 사용하고,
  - 없으면 `VNTitleManager`가 직접 `TitleBGMSource`, `TitleSFXSource`를 자동 생성하여 사용하도록 분기.
- `VNSettingsData.Load()`로 저장된 볼륨을 읽어 타이틀 진입 시 즉시 적용하도록 수정.
- `title_theme` 키가 나중에 `VNAssetDatabase`에 추가되면, 현재 구조 그대로 즉시 재생 가능하도록 연결 완료.

#### **C. `VNOptionPanel.cs` 설정 UI 볼륨 적용 경로 보강**
- `ui`, `presenter`, `titleManager`를 런타임에 자동 탐색하는 `RefreshRuntimeBindings()` 로직 추가.
- 인게임 씬에서는 `VNPresenter.SetVolume()`에 볼륨을 적용.
- 타이틀 씬에서는 `VNTitleManager.SetVolume()`에 볼륨을 적용.
- 즉, 동일한 설정 UI가 씬 종류에 따라 적절한 오디오 대상에 자동으로 연결되도록 수정.
- `Open()`, `ShowSettings()`, `ApplySettingsToSystems()`, `OpenSaveLoad()`에서 모두 최신 참조를 다시 확인하도록 보강.

### 🏠 4. 타이틀 설정 UI 관련 추가 개선 반영
- 이번 작업 중 추가 개선 포인트 3가지를 다시 점검함:
  - **개선안 1**: 타이틀 설정창에서도 `Close` 버튼이 정상적으로 닫히도록 수정.
  - **개선안 2**: 빈 BGM 키 입력 시 이전 BGM이 남지 않도록 정지 처리 추가.
  - **개선안 3**: 타이틀 진입 시 저장된 사용자 볼륨이 즉시 반영되도록 초기 적용 경로 보강.
- **선택하여 실제 반영한 개선안**: **개선안 1, 2, 3 모두 현재 작업과 강하게 연관되어 있어 함께 반영**
- **구체적 반영 내용**:
  - `VNOptionPanel`의 `closeButton`이 타이틀 모드(`menuRoot == null`)에서는 `Close()`로 동작하도록 수정.
  - 이로 인해 기존에 분석에서 확인했던 “타이틀 설정창을 APPLY 없이 닫기 어려운 문제”가 해결됨.
  - 오디오 초기화와 설정 반영이 분리되지 않고 하나의 흐름으로 이어지도록 정리함.

### 🎵 5. 현재 상태에서의 실제 동작 의미
- 아직 `VNAssetDatabase`에 유효한 BGM/SFX 클립이 충분히 등록되지 않았더라도,
  - 오디오 소스 생성
  - 볼륨 적용
  - 타이틀/인게임 분기
  - 빈 키 정리
  - 설정 저장값 초기 반영
  의 구조는 모두 준비된 상태.
- 따라서 이후에는 **에셋 데이터베이스에 실제 `AudioClip`만 등록하면 곧바로 사용 가능**한 상태가 됨.

### 🧪 6. 검증 결과
- **런타임 어셈블리 빌드**: `Assembly-CSharp.csproj` 통과.
- **에디터 어셈블리 빌드**: `Assembly-CSharp-Editor.csproj` 통과.
- **현재 남은 경고**:
  - `VNBacklogEntry.cs`
  - `VNBacklogSetupTool.cs`
  - 공통 원인: `TMP_Text.enableWordWrapping` obsolete 경고.
- **중요 판단**:
  - 이번 오디오 연결 작업으로 새 컴파일 오류는 발생하지 않음.
  - 오디오 관련 핵심 기능은 “음원 파일 미삽입 상태에서도 구조적으로 연결 완료” 단계에 도달함.

### 📌 7. 이번 작업 이후 다음 추천 순서
1. `VNAssetDatabase.asset`에 `forest`, `title_theme` 등 실제 BGM 키와 `AudioClip` 등록.
2. 버튼 클릭/호버용 SFX 키 정책 정리 (`hover_click`, `tab_click` 등).
3. 필요 시 타이틀 씬에도 버튼 클릭 시 SFX 재생 로직 연결.

---

## 📅 업데이트 날짜: 2026년 4월 18일 (오디오 후속 작업 보류 기록 및 세이브 슬롯 정보 보강)

### 🎯 1. 이번 요청의 목적과 우선순위 재조정
- **사용자 요청 해석**:
  - 직전 작업에서 제안했던 다음 우선순위 중,
    - 1번 `VNAssetDatabase` 오디오 키/클립 등록
    - 2번 버튼 SFX 정책 정리
    는 **실제 어떤 사운드를 사용할지 나중에 결정한 뒤 진행**하기로 변경됨.
  - 따라서 위 2개 항목은 지금 즉시 구현하지 않고, **후속 작업으로 보류 상태임을 문서에 명시**해야 했음.
  - 대신 바로 이어서 **4번 세이브 슬롯 정보 보강 작업**을 우선 수행.
- **이번 작업의 핵심 목표**:
  - 세이브 슬롯 UI에 저장 시각이 안정적으로 표시되도록 보강.
  - 세이브 슬롯이 현재 배경 상태를 시각적으로 보여줄 수 있도록 썸네일 복원 경로를 연결.
  - 기존에 자동 생성된 프리팹/씬 참조가 일부 비어 있어도, 런타임에서 가능한 한 스스로 복구(Self-healing) 하도록 안정성 보강.

### 🧭 2. 작업 전에 검토한 3가지 방법
- **방법 A**: `VNSaveLoadUIBuilder`만 수정해서 앞으로 새로 만드는 슬롯 프리팹에 `dateText`와 썸네일 참조를 완전히 연결하는 방식.
  - 장점: 에디터 자동 생성 결과가 정리됨.
  - 단점: 이미 만들어져 있는 씬/프리팹 인스턴스에는 즉시 효과가 없을 수 있음.
- **방법 B**: `VNSaveSlotItem`에 런타임 자가 복구 로직을 넣어서, 참조가 비어 있어도 `ContentGroup`, `Thumbnail`, `Title`, `Time`, `Date`를 직접 찾아 연결하거나 부족한 경우 생성하는 방식.
  - 장점: 이미 존재하는 씬/프리팹 인스턴스에도 즉시 대응 가능.
  - 단점: 코드량은 조금 늘어나며, UI 구조 이름 규칙에 의존하는 부분이 생김.
- **방법 C**: 세이브 데이터 구조를 대폭 확장해서 저장 시 썸네일 이미지를 별도 파일 또는 Base64 등으로 직접 저장하는 방식.
  - 장점: 가장 정확한 “저장 시점 화면 캡처형 썸네일” 구현으로 발전 가능.
  - 단점: 현재 요구 범위를 넘는 확장이고, 저장 용량/성능/파일 관리 복잡도가 크게 증가함.

### ✅ 3. 최종 선택한 방식과 선정 이유
- **최종 선택**: **방법 A + 방법 B의 혼합 방식**
- **선정 이유**:
  - 사용자 요구는 “세이브 슬롯 정보 보강”이며, 지금 즉시 체감 가능한 안정적인 개선이 중요했음.
  - 따라서 **앞으로 생성될 UI 구조는 에디터 빌더에서 올바르게 생성되게 하고**, 동시에 **이미 존재하는 슬롯은 런타임 자가 복구로 살리는** 2중 안전장치가 가장 적합하다고 판단.
  - 방법 C는 장기적으로는 매력적이지만, 지금 필요한 것은 “저장 날짜 표기 + 현재 배경 기반 썸네일 복원” 수준이므로 범위 초과라고 판단하여 제외.

### 🛠 4. 실제 반영한 코드 변경 사항

#### A. `VNSaveSlotItem.cs` 세이브 슬롯 표시 책임 강화
- 기존 `VNSaveSlotItem`는 `chapterTitleText`, `playTimeText`, `dateText`, `thumbnailImage` 필드를 갖고 있었지만, 실제로는 `dateText` 참조가 비어 있을 가능성이 있었고, 썸네일 복원도 TODO 성격에 가까웠음.
- 이번 작업에서 다음을 보강:
  - `EnsureReferences()` 메서드를 통해 아래 참조를 런타임에서 재탐색:
    - `actionButton`
    - `plusIcon`
    - `contentGroup`
    - `thumbnailImage`
    - `chapterTitleText`
    - `playTimeText`
    - `dateText`
  - `Date` 텍스트 오브젝트가 아예 없으면 `CreateDateLabel()`을 통해 TMP 라벨을 동적으로 생성.
  - 세이브 데이터가 있을 경우:
    - 챕터명 표시
    - 저장 날짜 표시
    - 플레이타임 표시
    - 현재 배경 키 기반 썸네일 복원
  - 빈 슬롯일 경우:
    - 플러스 아이콘 표시
    - 컨텐츠 그룹 숨김
    - 썸네일 숨김
- 추가로 `ResolveAssetDatabase()` 로직을 넣어, 세이브 슬롯이 `VNPresenter` 또는 `VNTitleManager`로부터 `VNAssetDatabase`를 찾아 배경 스프라이트를 복원할 수 있게 했음.

#### B. `VNPresenter.cs` / `VNTitleManager.cs`에 `VNAssetDatabase` 노출 경로 추가
- 세이브 슬롯 썸네일 복원은 **저장된 배경 키 문자열**을 실제 `Sprite`로 바꾸는 과정이 필요함.
- 그런데 기존 구조에서는 `VNSaveSlotItem`이 `VNAssetDatabase`에 직접 접근할 공식 경로가 부족했음.
- 이를 해결하기 위해:
  - `VNPresenter`에 `public VNAssetDatabase AssetDatabase => assetDB;`
  - `VNTitleManager`에 `public VNAssetDatabase AssetDatabase => assetDatabase;`
  를 추가함.
- 결과적으로 세이브 슬롯 UI는 게임 씬이든 타이틀 씬이든, 현재 씬에 존재하는 관리자를 통해 배경 데이터베이스를 찾아 썸네일을 복원할 수 있게 되었음.

#### C. `VNSaveLoadUIBuilder.cs`에서 날짜 텍스트 자동 생성 및 연결
- 에디터 자동 생성 도구도 함께 보강하여, 앞으로 새로 생성되는 세이브 슬롯 템플릿에는 `Date` TMP 텍스트가 구조적으로 포함되도록 수정.
- 수정 내용:
  - `CreateSlotTemplate()` 내부에서 `Date` 오브젝트를 생성.
  - `VNSaveSlotItem`의 직렬화 필드 `dateText`에 해당 TMP 컴포넌트를 자동 연결.
  - 기존 `Title`, `Time`, `Thumbnail` 배치와 함께 세이브 슬롯 정보 밀도를 개선.
- 이로 인해 향후 UI를 다시 빌드하더라도 저장 날짜 정보가 누락될 가능성이 크게 줄어듦.

#### D. `VNSaveData.cs` 저장 시각 포맷 보강
- 기존 `saveDate`는 `dd/MM/yyyy` 형식으로만 저장되고 있었음.
- 이 형식은:
  - 시간 정보가 없음.
  - 지역/문화권에 따라 월/일 순서 해석이 헷갈릴 수 있음.
- 이번 작업에서 저장 포맷을 `yyyy-MM-dd HH:mm`으로 변경.
- 기대 효과:
  - 정렬 및 육안 판독이 쉬움.
  - “같은 날 여러 번 저장했을 때”도 어느 슬롯이 최신인지 구분 가능.

#### E. `VNSaveService.cs` 로드 시 구버전/누락 데이터 보완
- 이미 존재하는 저장 파일 중에는 `saveDate`가 비어 있거나, 예전 구조로 저장된 데이터가 있을 수 있음.
- 이를 고려하여 `LoadSlot()`에서:
  - `saveDate`가 비어 있으면
  - 해당 세이브 파일의 마지막 수정 시각(File Last Write Time)을 읽어
  - `yyyy-MM-dd HH:mm` 형식으로 보정하도록 처리.
- 즉, 이번 변경은 **신규 세이브 포맷 개선**과 함께 **기존 세이브와의 하위 호환성**도 어느 정도 확보한 작업임.

### 🔍 5. 작업 후 추가로 점검한 개선 포인트 3가지와 선택 결과
- **개선안 1**: 세이브 슬롯의 `dateText`가 빠져 있어도 런타임에서 자동 생성되게 하기.
  - 선택 결과: **반영**
  - 이유: 이미 구성된 씬/프리팹을 다시 손으로 뜯지 않아도 되므로 즉시 안정성이 올라감.
- **개선안 2**: 세이브 슬롯 썸네일을 단순 플레이스홀더가 아니라 현재 배경 키 기반으로 복원되게 하기.
  - 선택 결과: **반영**
  - 이유: 추가 에셋 저장 구조를 도입하지 않고도, 플레이어가 어떤 장면 근처의 세이브인지 직관적으로 파악할 수 있음.
- **개선안 3**: 저장 날짜에 시간까지 포함하고, 구버전 세이브에는 파일 수정 시각으로 보정하기.
  - 선택 결과: **반영**
  - 이유: 최신 세이브 식별성과 하위 호환성을 동시에 개선할 수 있었음.

### ⏸ 6. 보류한 작업 항목 기록
- 아래 항목은 **삭제하거나 잊지 않기 위해 문서에 명시적으로 보류 상태로 기록**:
  1. `VNAssetDatabase`에 실제 BGM 키(`forest`, `title_theme` 등)와 `AudioClip` 등록.
  2. 버튼 클릭/호버용 SFX 키 정책 설계 및 적용.
- **보류 이유**:
  - 사용자 결정에 따라 어떤 사운드를 사용할지 아직 확정되지 않았음.
  - 지금 시점에서 구조를 더 밀어붙이면, 나중에 실제 사운드 선택 이후 다시 바꿔야 할 가능성이 높음.
- **현재 상태 요약**:
  - 오디오 시스템의 **코드 연결 구조와 볼륨 연동 경로는 준비 완료**.
  - 실제 에셋과 키 정책이 정해지면 그때 데이터만 채우는 방향으로 후속 작업 진행 가능.

### 🧪 7. 검증 결과
- **런타임 어셈블리 빌드**: `Assembly-CSharp.csproj` 통과.
- **에디터 어셈블리 빌드**: `Assembly-CSharp-Editor.csproj` 통과.
- **현재 남아 있는 경고**:
  - `VNBacklogEntry.cs`의 `TMP_Text.enableWordWrapping` obsolete 경고.
  - `VNBacklogSetupTool.cs`의 `TMP_Text.enableWordWrapping` obsolete 경고.
- **중요 판단**:
  - 이번 세이브 슬롯 정보 보강으로 인한 신규 컴파일 오류는 최종적으로 없음.
  - 저장 날짜 누락, 참조 누락, 배경 썸네일 미복원 문제에 대해 구조적 방어가 추가된 상태.

### 📌 8. 현재 기준 다음 추천 작업
1. 실제 세이브/로드 패널을 유니티 에디터에서 열어, 슬롯 레이아웃상 날짜 텍스트의 위치와 가독성 최종 확인.
2. 원한다면 다음 단계로 “최근 저장 순 정렬 강조”, “현재 진행 중 챕터명 자동 보강”, “진짜 화면 캡처형 썸네일” 중 하나를 선택해 확장 가능.
3. 오디오 관련은 사운드 리소스 결정 이후 재개.

---

## 📅 업데이트 날짜: 2026년 4월 18일 (세이브 슬롯 레이아웃 정렬 보정 및 타이틀 씬 Load 전용화)

### 🎯 1. 이번 요청의 핵심 요구사항
- **사용자 요청 요약**:
  - 세이브 슬롯 내부에서 **썸네일 이미지와 텍스트들이 서로 겹치지 않도록 정렬**할 것.
  - 이 수정은 **게임 씬의 세이브/로드 패널**과 **타이틀 씬의 로드 패널** 모두에 동일하게 적용되어야 함.
  - 추가로 **타이틀 씬에서는 Save 기능이 필요 없으므로, Save 섹션/탭이 보이지 않게 정리**할 것.
- **문제의 실제 증상**:
  - 슬롯 내부에서 `Chapter`, `Playtime`, 저장 날짜 텍스트가 모두 절대 좌표 기반으로 배치되어 있어,
    - 폰트 크기,
    - 줄 수,
    - 날짜 문자열 길이,
    - TMP 렌더링 결과
    에 따라 쉽게 겹칠 수 있는 구조였음.
  - 특히 `Playtime`이 2줄(`Playtime` + 시간) 구조였고, 날짜 텍스트가 바로 아래에 고정 좌표로 들어가면서 시각적으로 충돌이 발생.
  - 타이틀 씬의 `VNSaveLoadPanel`은 `director == null` 환경이므로 실제 저장은 불가능한데도, 상단 `Save` 탭 버튼은 계속 노출되고 있었음.

### 🧭 2. 작업 전 검토한 3가지 방법
- **방법 A**: 현재 좌표 기반 배치를 유지하면서 `Title`, `Time`, `Date`, `Thumbnail`의 `anchoredPosition`만 다시 손보는 방식.
  - 장점: 코드 수정량이 가장 적음.
  - 단점: 해상도나 폰트 변경, 텍스트 길이 변화가 생기면 다시 겹칠 가능성이 높고, 구조적으로 취약함.
- **방법 B**: `VNSaveSlotItem` 내부에서 슬롯을 **레이아웃 그룹 기반 구조(Horizontal + Vertical)** 로 재정렬하도록 변경하고, 기존 씬/프리팹도 런타임에서 자동 보정되게 하는 방식.
  - 장점: 현재 씬과 앞으로 생성될 슬롯 모두에 강한 안정성을 제공.
  - 단점: 좌표 수정보다 코드량이 늘어나며, UI 자동 배치 규칙을 명확히 설계해야 함.
- **방법 C**: 씬 파일(`SampleScene`, `TitleScene`) 안의 `SaveSlotItem_Template`와 각 탭 오브젝트를 직접 수동 수정해서 장면별로 따로 정리하는 방식.
  - 장점: 에디터상 결과를 직접 고정 가능.
  - 단점: 같은 구조를 두 씬에서 중복 관리하게 되고, 나중에 `VN Tools`로 UI를 다시 빌드하면 수정이 다시 덮일 수 있음.

### ✅ 3. 최종 선택 방식과 이유
- **최종 선택**: **방법 B를 기본으로 하고, 에디터 자동 생성기(`VNSaveLoadUIBuilder`)도 같은 구조로 맞추는 혼합 방식**
- **선정 이유**:
  - 사용자 요구는 “두 씬 모두에서 겹치지 않게”라는 공통 안정성 확보가 핵심이었음.
  - 따라서 이미 존재하는 씬 인스턴스는 `VNSaveSlotItem` 런타임 보정으로 살리고,
  - 앞으로 새로 생성되는 템플릿은 `VNSaveLoadUIBuilder`에서 처음부터 올바른 구조로 생성하게 만드는 방식이 가장 안전하다고 판단.
  - 타이틀 씬의 `Save` 탭 제거도 씬 파일을 직접 뜯는 것보다, `director` 유무에 따라 자동 분기하도록 하는 쪽이 유지보수에 유리하다고 판단.

### 🛠 4. 실제 반영한 변경 사항

#### A. `VNSaveSlotItem.cs`를 좌표 배치형에서 레이아웃 보정형으로 확장
- 세이브 슬롯이 `Setup()`될 때, 단순 텍스트 대입만 하지 않고 **UI 구조를 먼저 정규화**하도록 수정.
- 추가된 핵심 로직:
  - `EnsureLayoutStructure()`
  - `ConfigureSlotFrame()`
  - `ConfigurePlusIcon()`
  - `ConfigureThumbnail()`
  - `ConfigureTitleText()`
  - `ConfigurePlayTimeText()`
  - `ConfigureDateText()`
  - `EnsureInfoColumn()`
- 동작 방식:
  - `ContentGroup`에 `HorizontalLayoutGroup`을 적용하여,
    - 왼쪽에는 썸네일,
    - 오른쪽에는 텍스트 컬럼
    구조로 강제 정렬.
  - 오른쪽 텍스트 컬럼(`InfoColumn`)에는 `VerticalLayoutGroup`을 적용하여,
    - 챕터명
    - 플레이타임
    - 저장 날짜
    가 위에서 아래로 겹치지 않게 배치되도록 변경.
  - 기존 템플릿이 예전 구조여도, `Title`, `Time`, `Date`를 `InfoColumn` 아래로 재배치하도록 처리.
- 기대 효과:
  - 수동 좌표 충돌 문제 완화.
  - 텍스트 길이가 조금 달라져도 레이아웃이 더 안정적으로 유지.
  - 게임 씬과 타이틀 씬 모두 동일한 슬롯 정렬 규칙 적용.

#### B. `Playtime` 문자열 자체를 더 압축적인 1줄 구조로 변경
- 기존:
  - `Playtime`
  - `00:00:00`
  의 2줄 표시.
- 변경:
  - `Playtime  00:00:00`
  의 1줄 표시.
- 이유:
  - 슬롯 높이가 제한된 상태에서 2줄 플레이타임은 날짜 정보와 충돌하기 쉬웠음.
  - 1줄 구조로 바꾸면 정보량은 유지하면서 세로 공간 충돌이 크게 줄어듦.

#### C. 텍스트 표시 규칙 정리
- `chapterTitleText`
  - 더 큰 글자, 한 줄, 좌측 정렬, 넘치면 말줄임표(`Ellipsis`) 처리.
- `playTimeText`
  - 보조 정보 컬러, 한 줄, 좌측 정렬.
- `dateText`
  - 가장 낮은 시각적 우선순위 컬러, 한 줄, 좌측 정렬.
- 즉, 슬롯 내부 정보의 시각적 우선순위를
  - 제목 > 플레이타임 > 저장 날짜
  순서로 재정의.

#### D. `VNSaveLoadPanel.cs`에 “타이틀 씬에서는 Load 전용” 분기 추가
- `CanSaveInCurrentScene()`를 추가하여 `director != null` 여부로 저장 가능 여부를 판별.
- `RefreshTabVisibility()`를 추가하여:
  - 저장이 불가능한 씬에서는 `saveTabButton`을 숨김.
  - 현재 모드가 `Save`라도 저장 불가 환경이면 자동으로 `Load`로 전환.
  - 탭 레이아웃을 다시 빌드하여 `Load` 버튼이 자연스럽게 정렬되도록 보정.
- 결과:
  - 타이틀 씬에서는 `Load`만 노출되는 구조가 되었고,
  - 게임 씬에서는 기존처럼 `Save / Load` 둘 다 사용 가능.

#### E. `VNSaveLoadUIBuilder.cs`도 새 슬롯 구조에 맞게 재설계
- 에디터 메뉴 `VN Tools/Build SaveLoad UI Only`로 다시 생성하더라도,
  - 썸네일 왼쪽
  - 정보 컬럼 오른쪽
  - `Title / Time / Date` 세로 정렬
  구조가 기본값이 되도록 수정.
- 구체적으로:
  - `ContentGroup`에 `HorizontalLayoutGroup` 추가
  - `InfoColumn` 오브젝트 신설
  - `Title`, `Time`, `Date`를 `InfoColumn` 아래 생성
  - 각 UI 요소에 `LayoutElement`를 지정하여 크기 정책을 명확히 함
- 이 작업을 통해 **런타임 임시 보정만이 아니라, 앞으로의 기본 템플릿 자체도 개선**됨.

### 🔍 5. 추가 점검한 개선안 3가지와 선택 결과
- **개선안 1**: 게임 씬과 타이틀 씬에서 슬롯 템플릿 좌표를 각각 따로 손보는 방식
  - 선택 결과: **미선택**
  - 이유: 중복 유지보수가 발생하고, 자동 생성기 재실행 시 다시 깨질 수 있음.
- **개선안 2**: 슬롯 정렬 문제를 `VNSaveSlotItem` 런타임 보정으로 흡수하는 방식
  - 선택 결과: **반영**
  - 이유: 현재 두 씬 모두에 공통 대응 가능하고, 이미 존재하는 템플릿도 살릴 수 있음.
- **개선안 3**: 타이틀 씬에서 저장이 불가능하면 `Save` 탭 자체를 숨기는 방식
  - 선택 결과: **반영**
  - 이유: 기능적으로 불가능한 행동을 UI에서 미리 제거하는 것이 더 명확하고 안전함.

### 🧪 6. 검증 결과
- **런타임 어셈블리 빌드**: `Assembly-CSharp.csproj` 통과.
- **에디터 어셈블리 빌드**: `Assembly-CSharp-Editor.csproj` 통과.
- **남은 경고**:
  - `VNBacklogEntry.cs`의 `TMP_Text.enableWordWrapping` obsolete 경고.
  - `VNBacklogSetupTool.cs`의 `TMP_Text.enableWordWrapping` obsolete 경고.
- **중요 판단**:
  - 이번 작업으로 세이브 슬롯 겹침 완화를 위한 구조적 정렬 규칙이 들어갔음.
  - 타이틀 씬에서는 `Load`만 남기고 `Save` 탭은 숨기는 요구사항을 충족하도록 로직이 보강되었음.

### 📌 7. 현재 기준 다음 확인 권장 사항
1. 유니티 에디터에서 `SampleScene`, `TitleScene` 각각 실행하여 실제 슬롯 폭 안에서 긴 챕터명이 말줄임으로 자연스럽게 보이는지 확인.
2. 원한다면 다음 단계로 세이브 슬롯에 “슬롯 번호” 또는 “최근 저장 배지”를 추가해 가독성을 더 높일 수 있음.
3. 나중에 실제 오디오 리소스가 정해지면, 현재 보류된 사운드 후속 작업을 재개.

---

## 📅 업데이트 날짜: 2026년 4월 18일 (Load 이후 화면 흔들기 무한 지속 버그 분석 및 수정)

### 🎯 1. 사용자 보고 증상과 재현 조건
- **보고된 현상**:
  - 연출 태그 `#shake`로 구현된 화면 흔들기가 일반 진행에서는 정상 동작하는 것처럼 보임.
  - 그러나 **Load를 한 뒤 다시 같은 흔들기 구간을 지나가면**, 흔들기가 지정된 시간만 동작하지 않고 **계속 흔들리는 것처럼 보이는 버그**가 발생.
- **문제의 성격**:
  - 단순히 흔들기 루틴 내부의 계산 실수라기보다,
    - `Load` 경로에서 어떤 코루틴이 멈추고 어떤 코루틴이 살아남는지,
    - `Time.timeScale`이 로드 후 어떤 값으로 유지되는지,
    - 화면 흔들기 루틴이 `Time.deltaTime`과 `Time.unscaledDeltaTime` 중 무엇을 쓰는지
    가 동시에 얽힌 복합 문제로 판단.

### 🧭 2. 작업 전에 검토한 3가지 해결 방법
- **방법 A**: `VNPresenter.ShakeUIRoutine()`만 수정해서 `Time.deltaTime` 대신 `Time.unscaledDeltaTime`을 사용하게 바꾸는 방식.
  - 장점: 흔들기 지속 시간이 `timeScale`에 영향을 받지 않게 되어 증상이 즉시 완화될 수 있음.
  - 단점: 로드 시점에 이미 살아 있던 예전 흔들기 코루틴이나, 배경 페이드 같은 다른 연출 코루틴이 남아 있는 문제는 해결하지 못함.
- **방법 B**: `VNDirector.Load()`에서 `Time.timeScale = 1f`를 보장하여, 로드 후 시간이 멈춘 상태가 지속되지 않게 하는 방식.
  - 장점: 인게임 로드 이후 `deltaTime == 0` 상태가 유지되는 문제를 직접 해결.
  - 단점: 로드 전에 이미 `VNPresenter`에서 실행 중이던 코루틴이 계속 살아남는 구조는 그대로 남음.
- **방법 C**: `Load` 시점에 **프레젠터의 일시 연출 코루틴을 명시적으로 초기화**하고, 동시에 `timeScale`도 정상값으로 복원하며, 흔들기 지속 시간 계산도 `unscaledDeltaTime`으로 전환하는 방식.
  - 장점: 현재 보고된 버그 원인을 다층적으로 차단.
  - 단점: 코드 수정 범위가 가장 넓고, 인터페이스(`IVNPresenter`)까지 변경해야 함.

### ✅ 3. 최종 선택 방식과 선정 이유
- **최종 선택**: **방법 C**
- **선정 이유**:
  - 실제 코드 분석 결과, 현재 버그는 한 군데만 고쳐서는 재발 여지가 남는 구조였음.
  - 따라서
    1. `Load` 후 `timeScale` 복원,
    2. `VNPresenter`에 남아 있던 코루틴 정리,
    3. 흔들기 루틴을 `unscaledDeltaTime` 기반으로 전환
    을 동시에 반영하는 쪽이 가장 안전하다고 판단.

### 🔍 4. 실제 원인 분석 결과

#### A. `VNDirector.Load()`는 자기 자신 코루틴만 멈추고 `VNPresenter` 코루틴은 멈추지 않았음
- 기존 `VNDirector.Load()`는 `StopAllCoroutines()`를 호출하고 있었음.
- 그러나 Unity 공식 문서 기준으로 `MonoBehaviour.StopAllCoroutines()`는 **해당 MonoBehaviour에 붙어 있는 코루틴만 멈춤**.
- 즉,
  - `VNDirector`에서 돌아가던 `StoryLoop()`는 멈추지만,
  - `VNPresenter`에서 `StartCoroutine(ShakeUIRoutine(...))`로 실행 중이던 흔들기 코루틴은 **그대로 살아남을 수 있었음**.
- 이 상태에서 로드 후 다시 같은 흔들기 태그를 만나면, 이전 흔들기 상태와 새 흔들기가 섞여 비정상 동작할 여지가 존재.

#### B. 인게임 Load는 `Time.timeScale = 0` 상태를 끌고 갈 가능성이 있었음
- 현재 구조에서 `VNOptionPanel`은 메뉴를 열 때 `Time.timeScale = 0f`로 게임 시간을 멈춤.
- 사용자가 이 상태에서 `Save/Load` 패널을 열고 Load를 수행하면, 기존 로드 경로에는 **시간 스케일을 다시 1로 되돌리는 보장 코드가 없었음**.
- 그 결과 로드 후에도 `Time.timeScale == 0`이 유지될 수 있었고,
  - `VNUIController`, `VNTextTyper`는 `Time.unscaledDeltaTime`과 `WaitForSecondsRealtime`를 쓰므로 겉보기엔 진행이 계속됨.
  - 반면 화면 흔들기 루틴은 `Time.deltaTime`을 사용하고 있었기 때문에, `elapsed`가 증가하지 않아 **지속 시간이 끝나지 않는 상태**가 될 수 있었음.
- 즉, 사용자가 체감한 “계속 흔들린다” 현상은 실제로는 **시간이 멈춘 상태에서 흔들기 루틴이 종료 조건에 도달하지 못한 결과**로 해석 가능.

### 🛠 5. 실제 반영한 수정 사항

#### A. `IVNPresenter.cs`에 `ResetTransientState()` 추가
- 로드 시점에 프레젠터가 들고 있던 일시 연출 상태를 공통 인터페이스 수준에서 초기화할 수 있도록
  - `void ResetTransientState();`
  메서드를 추가.
- 이유:
  - `VNDirector`는 구체 타입이 아니라 `IVNPresenter`를 통해 동작하고 있으므로,
  - 로드 시점 연출 초기화를 안전하게 호출하려면 인터페이스 확장이 필요했음.

#### B. `VNPresenter.cs`에 “일시 연출 상태 초기화” 로직 추가
- `shakeRoutine` 핸들을 새로 추가하여 현재 흔들기 코루틴을 추적.
- `ShakeScreen()` 호출 시:
  - 이미 흔들기 코루틴이 돌고 있으면 먼저 중지.
  - `masterUIRoot` 위치를 원점으로 되돌린 후 새 흔들기를 시작.
- `ShakeUIRoutine()` 수정:
  - `Time.deltaTime` 대신 `Time.unscaledDeltaTime` 사용.
  - 지속 시간이 0 이하이거나 강도가 0 이하이면 즉시 원위치 복구 후 종료.
  - 종료 시에도 반드시 화면 위치를 원점으로 복구.
- `ResetTransientState()` 구현:
  - `StopAllCoroutines()`로 프레젠터에 걸린 코루틴 전체 정지.
  - 흔들기 루틴 핸들 초기화.
  - `BackgroundFade` 임시 오브젝트 제거.
  - `masterUIRoot` 위치를 원래 좌표로 복구.
- 이로 인해 로드 시점에
  - 남아 있던 흔들기,
  - 남아 있던 배경 페이드
  같은 일시 연출이 복구 상태를 덮어쓰는 문제를 차단.

#### C. `VNDirector.cs`의 Load 경로 보강
- `Load()` 진입 직후 `Time.timeScale = 1f;`를 추가하여, 인게임 로드 후 시간이 멈춘 상태로 남지 않도록 수정.
- 이어서 `presenter.ResetTransientState()`를 호출한 뒤:
  - 캐릭터 정리
  - 배경 복원
  - BGM 복원
  - 캐릭터 상태 복원
  순서로 다시 적용.
- 이 순서를 사용한 이유:
  - 먼저 남아 있는 연출 찌꺼기를 비우고,
  - 그 다음 세이브 데이터 기준의 정상 상태를 적용해야 복구 결과가 덮어쓰기 당하지 않음.

### 🔍 6. 작업 후 추가로 점검한 개선 포인트 3가지와 선택 결과
- **개선안 1**: 흔들기 지속 시간만 `unscaledDeltaTime`으로 바꾸고 로드 로직은 건드리지 않기
  - 선택 결과: **미선택**
  - 이유: 현재 증상은 줄일 수 있어도, 로드 시점에 살아남는 다른 프레젠터 코루틴 문제는 남음.
- **개선안 2**: `Load()`에서 `timeScale`만 1로 되돌리기
  - 선택 결과: **부분 반영**
  - 이유: 중요한 원인 하나를 직접 해결하므로 꼭 필요했지만, 이것만으로는 프레젠터 코루틴 잔존 문제를 막기 부족함.
- **개선안 3**: 프레젠터 인터페이스에 연출 초기화 메서드를 추가해 로드 시점에 강제로 비우기
  - 선택 결과: **반영**
  - 이유: 이번 흔들기 버그뿐 아니라, 앞으로 배경 페이드/캐릭터 페이드가 로드와 충돌하는 문제까지 예방할 수 있음.

### 🧪 7. 검증 결과
- **런타임 어셈블리 빌드**: `Assembly-CSharp.csproj` 통과.
- **에디터 어셈블리 빌드**: `Assembly-CSharp-Editor.csproj` 통과.
- **남아 있는 경고**:
  - `VNBacklogEntry.cs`의 `TMP_Text.enableWordWrapping` obsolete 경고.
  - `VNBacklogSetupTool.cs`의 `TMP_Text.enableWordWrapping` obsolete 경고.
- **현재 판단**:
  - 보고된 “Load 이후 화면 흔들기 무한 지속” 현상에 대해 구조적 원인을 제거하는 방향으로 수정이 들어감.
  - 특히 로드 시점 코루틴 정리와 `timeScale` 복원이 동시에 들어갔기 때문에, 동일 계열의 연출 꼬임 재발 가능성이 낮아짐.

### 📌 8. 현재 기준 다음 확인 권장 사항
1. 실제 플레이에서 `#shake` 직전 세이브 → Load → 다시 `#shake` 구간 진입 순서로 재현 테스트를 진행해, 지속 시간이 정상 종료되는지 확인.
2. 원한다면 다음 단계로 `FadeBackgroundRoutine`, `FadeCharacterRoutine`도 `unscaledDeltaTime` 기반으로 통일할지 검토 가능.
3. 추가로, 인게임에서 `Save/Load` 패널을 닫았을 때 옵션 메뉴/시간 정지가 자연스럽게 복귀하는지 UX 측면 재점검 권장.

---

## 📅 업데이트 날짜: 2026년 5월 1일 (저장 후 배경 전환 멈춤 버그 수정)

### 🎯 1. 사용자 보고 증상
- 인게임에서 배경 전환 직전 저장을 수행한 뒤, 저장/로드 패널을 닫고 다음 대사로 진행하면 `#bg ... fade` 배경 전환이 정상적으로 끝나지 않는 문제가 보고됨.

### 🔍 2. 원인 분석
- `VNOptionPanel.OpenSaveLoad()`는 옵션 메뉴에서 저장/로드 패널을 열 때 옵션 패널을 숨기지만, `Time.timeScale = 0f` 상태는 그대로 유지함.
- 기존 `VNSaveLoadPanel.Close()`는 패널만 닫고 `Time.timeScale`을 복구하지 않았음.
- 대사 진행과 텍스트 타이핑은 `unscaledDeltaTime` 계열을 사용하므로 계속 진행되는 것처럼 보였지만, `VNPresenter.FadeBackgroundRoutine()`은 `Time.deltaTime`을 사용하고 있어 `timeScale == 0` 상태에서는 페이드 시간이 증가하지 않았음.

### 🛠 3. 반영한 수정 사항
- `VNSaveLoadPanel.Close()`에서 인게임 패널(`director != null`)일 경우 `Time.timeScale = 1f`로 복구하도록 수정.
- `VNPresenter.FadeBackgroundRoutine()`과 `FadeCharacterRoutine()`을 `Time.unscaledDeltaTime` 기반으로 변경하여 일시정지 상태가 남아도 연출 코루틴이 멈추지 않도록 보강.

### 🧪 4. 검증 기준
- 배경 전환 직전 저장 → 저장 패널 닫기 → 다음 대사 진행 → `#bg ... fade` 전환이 정상 완료되는지 확인.
- 캐릭터 페이드도 동일한 시간 기준으로 동작하므로, 저장/옵션 UI 이후 캐릭터 등장 페이드가 멈추지 않는지 함께 확인.

---

## 📅 업데이트 날짜: 2026년 5월 22일 (챕터 1 Ink 반영 및 프롤로그 전용 연출 시스템 구축)

### 🎯 1. 이번 작업의 핵심 목표
- PDF로 정리된 챕터 1 스토리 흐름을 기준으로 `main.ink`를 교체하고, 현재 2D 비주얼 노벨 구조에 맞지 않는 전투/튜토리얼 구간은 텍스트 연출과 효과음 지시문으로 대체.
- 초반 프롤로그를 영상 레퍼런스처럼 **검은 화면 중앙 텍스트 + 클릭 진행 + 하단 선택지** 방식으로 구현.
- 이후 다른 장면에서도 재사용할 수 있도록 Ink 태그 기반 화면 연출을 확장.
- 증가한 스크립트 파일을 기능별 폴더로 정리하여 유지보수성을 개선.

### 🛠 2. Ink 스토리 및 컴파일 작업
- `Assets/Ink/main.ink`를 챕터 1 흐름 중심으로 재구성.
  - `start` knot을 유지하여 기존 `VnInkBootstrap.startKnot = start` 설정과 호환되도록 보강.
  - `VAR player_resolve = 0`를 전역 선언으로 추가하여 선택지 분기 변수 오류 해결.
  - 전투 조작 튜토리얼은 실제 조작 대신 `[전투 연출]`, `[효과음]`, `#shake` 등 VN용 문구/연출로 대체.
- `main.ink` 변경 후 `main.json`을 다시 컴파일.
- Ink 태그의 `#` 문자 충돌 문제 확인:
  - `<color=#ff6f7d>`가 Ink 태그로 오인되어 `<color=`만 출력되던 문제 발생.
  - TMP 호환을 위해 `<color=red>이 세상이 멸망하기 전에.</color>` 형태로 수정.

### 🎬 3. 프롤로그 전용 표시 모드 구현
- `#mode prologue`
  - 일반 대화창 대신 프롤로그 전용 UI 사용.
  - 검은 화면, 중앙 텍스트, 하단 선택지 구성.
- `#mode normal`
  - 프롤로그 UI 종료 후 일반 VN 대화 UI로 복귀.
- `#clear`
  - 프롤로그 중앙 텍스트 누적 내용을 지워 다음 문장 묶음으로 전환.
- `#textFade 초`
  - 프롤로그 텍스트가 클릭 시 페이드인되도록 구현.
  - 예: `#textFade 0.45`
- 텍스트 페이드 구현 중 TMP `<alpha>` 태그 닫힘 문제 수정:
  - 잘못된 `</alpha>` 태그가 화면에 잠깐 보이는 문제 발생.
  - TMP 문법에 맞춰 `<alpha=#XX>문장<alpha=#FF>` 방식으로 수정.

### 🧩 4. 프롤로그 UI를 Scene 기반으로 전환
- 초기에는 프롤로그 검은 배경/텍스트/선택지를 코드에서 런타임 생성했으나, 사용자가 Scene View에서 직접 조정할 수 있도록 구조 변경.
- `VNUIController`에 다음 참조 필드 추가:
  - `prologueRoot`
  - `prologueText`
  - `prologueAdvanceButton`
  - `prologueChoiceButtons`
- `VNPrologueUISetupTool.cs` 추가:
  - Unity 메뉴 `VN Tools > Setup Prologue UI`로 `PrologueOverlay`, `PrologueText`, `PrologueChoices`, `Choice_1`, `Choice_2` 생성 및 자동 연결.
  - 생성된 오브젝트는 Scene/Inspector에서 직접 위치, 폰트 크기, 색상 조절 가능.

### ✨ 5. 선택지 표시 및 클릭 효과 개선
- 프롤로그 선택지 표시 시 `CanvasGroup` 기반 페이드인 구현.
  - `prologueChoiceFadeSeconds`로 표시 속도 조절 가능.
- `VNChoiceSelectionEffect.cs` 추가:
  - 선택지를 클릭하면 선택한 항목이 살짝 확대되고 색이 밝아짐.
  - 선택하지 않은 항목은 흐려진 뒤 전체 선택지가 사라짐.
  - 프롤로그뿐 아니라 다른 선택지 버튼에도 붙여 재사용 가능한 모듈로 분리.
- `VNPrologueUISetupTool`에서 생성되는 선택지 슬롯에 `CanvasGroup`, `VNChoiceSelectionEffect`가 자동 부착되도록 수정.

### 🌑 6. 범용 화면 페이드 태그 추가
- 화면 전체 페이드 인/아웃을 Ink에서 제어할 수 있도록 추가.
- 지원 태그:
  - `#fadeOut 0.8`
  - `#fadeIn 0.8`
  - `#afterFadeOut 0.8`
  - `#afterFadeIn 0.8`
- 색상 인자도 일부 지원:
  - `black`
  - `white`
  - `red`
  - `clear`
- 프롤로그 마지막 문장 이후 다음 장면으로 넘어갈 때:
  - 마지막 문장 확인 클릭 후 `#afterFadeOut 0.8` 실행.
  - `collapse_preview` 진입 후 `#fadeIn 0.8` 실행.
- 페이드 오버레이가 입력을 가로막지 않도록 `raycastTarget = false`로 조정.
- `afterFadeOut` 실행 순서를 “문장 표시 직후”가 아니라 “플레이어가 클릭해 문장을 확인한 뒤”로 변경하여 암전 후 진행이 막히는 문제 수정.

### 🧭 7. 현재 Ink 연출 태그 목록 정리
- 기본 VN 연출:
  - `#speaker 이름`
  - `#bg 배경키 [fade]`
  - `#ch 캐릭터명 [표정] [위치] [fade]`
  - `#hide 캐릭터명 [fade]`
  - `#shake 시간 강도`
  - `#bgm 음악키`
  - `#sfx 효과음키`
- 프롤로그/화면 연출:
  - `#mode prologue`
  - `#mode normal`
  - `#clear`
  - `#textFade 초`
  - `#fadeOut 초 [색상]`
  - `#fadeIn 초 [색상]`
  - `#afterFadeOut 초 [색상]`
  - `#afterFadeIn 초 [색상]`

### 🗂 8. 스크립트 폴더 구조 정리
- `Assets/Scripts` 루트에 스크립트가 많아져 기능별 폴더로 이동.
- `.cs` 파일과 `.meta` 파일을 함께 이동하여 Unity GUID 참조가 유지되도록 처리.
- 정리된 구조:
  - `Core/`: `VNDirector`, `VNCommandProcessor`, `VNLine`, `IVNPresenter`
  - `Ink/`: `InkStoryEngine`, `VnInkBootstrap`
  - `Presentation/`: `VNPresenter`, `VNAssetDatabase`
  - `SaveLoad/`: `VNSaveData`, `VNSaveService`, `VNSaveLoadPanel`, `VNSaveSlotItem`
  - `Settings/`: `VNSettingsData`
  - `Title/`: `VNTitleManager`
  - `UI/`: `VNUIController`, `VNTextTyper`, `VNButtonEffects`
  - `UI/Backlog/`: `VNBacklogManager`, `VNBacklogEntry`
  - `UI/Choices/`: `VNChoicePanel`, `VNChoiceSelectionEffect`
  - `UI/Options/`: `VNOptionPanel`
  - `Editor/`: 각종 Setup/Builder 도구
- 로컬 빌드 검증을 위해 `Assembly-CSharp.csproj`의 Compile 경로도 새 폴더 구조에 맞춰 갱신.

### 🧪 9. 검증 결과
- `main.ink` → `main.json` 컴파일 성공.
- `Assembly-CSharp.csproj` 빌드 통과.
- `Assembly-CSharp-Editor.csproj` 빌드 통과.
- 최종 확인 시점 기준 빌드 경고 0개 / 오류 0개.

### 📌 10. 다음 작업 제안
1. 스토리 파일을 `main.ink` 하나로 계속 키우기보다 `chapter_01.ink`, `chapter_02.ink` 등으로 분리하는 구조 검토.
2. `main.ink`는 전역 변수와 INCLUDE/진입점만 담당하게 정리.
3. 프롤로그 UI는 Scene에서 실제 해상도 기준으로 텍스트 위치, 선택지 크기, 페이드 속도를 플레이 테스트하며 조정.
4. 다음 챕터 작업 전, 공통 Ink 연출 태그 사용 예시를 `effects.ink` 또는 문서로 정리하면 작업 효율이 좋아질 것으로 판단.

---

## 📅 업데이트 날짜: 2026년 5월 28일 (로비/미니게임/스토리 진행 상태 설계 방향 기록)

### 🎯 1. 설계 목적
- 향후 `LobbyScene`을 중심으로 메인스토리, 서브스토리, 미니게임, 저장/설정으로 이동할 수 있는 허브 구조를 도입할 예정.
- 미니게임은 단순히 스토리 중간에 삽입되는 이벤트뿐 아니라, 로비에서 선택 가능한 활동으로도 사용한다.
- 미니게임 결과는 즉시 게임오버나 강제 진행이 아니라, 스토리 분기 변수·호감도·위험도·단서 획득 상태를 올리거나 내리는 방식으로 활용한다.

### 🏗 2. 권장 씬 구성 방향
- `TitleScene`: 게임 시작, 이어하기, 설정, 로드 진입점.
- `LobbyScene`: 메인스토리/서브스토리/미니게임/휴식/저장 등 활동 선택 허브.
- `SampleScene` 또는 VN 전용 씬: 실제 Ink 기반 비주얼 노벨 진행.
- `MiniGameScene`: 미니게임 단독 개발 및 테스트용 샌드박스.
- 실제 적용 시에는 미니게임을 씬에 강하게 묶지 않고, 프리팹/모듈 형태로 `LobbyScene` 또는 VN 씬 위에서 호출할 수 있게 설계한다.

### 🧩 3. 미니게임 모듈 원칙
- 미니게임은 스토리를 직접 진행하지 않고 `MiniGameResult`만 반환한다.
- 호출자는 `MiniGameManager.StartMiniGame(id, callback)` 형태로 미니게임을 시작하고, 결과를 받아 장기 진행 상태나 Ink 변수에 반영한다.
- QTE, 3레인 회피, 단서 찾기, 타이밍 게이지는 같은 결과 반환 규칙을 공유한다.
- 실패해도 진행 불가능 상태로 막지 않고, 대사 변화·위험도 증가·호감도 변화·단서 미획득 등으로 처리한다.

### 💾 4. 상태 데이터 분리 방향
- `Ink` 변수: 현재 장면 안에서 쓰는 단기 대사/선택지 분기.
- 장기 진행 상태(`GameProgressState` 예정): 일차, 행동력, 도시 위험도, 캐릭터 호감도/신뢰도, 해금된 서브스토리, 미니게임 결과, 주요 StoryFlag 관리.
- 로비와 미니게임은 장기 진행 상태를 갱신하고, VN 씬은 필요한 값을 Ink 변수로 주입하거나 자체 분기 조건으로 참조한다.

### 📌 5. 다음 구현 권장 순서
1. `MiniGameScene`에서 QTE 미니게임을 독립 프로토타입으로 먼저 구현.
2. `MiniGameResult`, `MiniGameManager`, 공통 결과 반환 콜백 구조 확정.
3. `GameProgressState` 초안 작성 후 저장/로드 대상 범위 검토.
4. QTE 결과를 Ink 변수 또는 장기 상태에 반영해 짧은 대사 분기를 검증.
5. 이후 3레인 회피, 단서 찾기, 타이밍 게이지를 같은 인터페이스로 확장.
