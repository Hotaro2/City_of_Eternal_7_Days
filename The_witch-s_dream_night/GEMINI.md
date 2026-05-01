#(The Witch's Dream Night) - 개발 기록 및 가이드

본 문서는 Gemini CLI를 통해 진행된 프로젝트 수정 및 개선 사항을 기록한 문서입니다. 향후 작업 시 이 내용을 최우선으로 참고하십시오.
*작업 시 지켜할 항목* 
1. GEMINI.md 파일 수정 및 최신화 작업을 할 때 적혀 있는 내용을 절대 지우지 말 것.
2. 작업한 내용을 관련하여 GEMINI.md 파일 최신화 할 때 매우 자세하게 적을 것.

---

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
