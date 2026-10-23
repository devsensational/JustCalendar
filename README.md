# DesktopCalendarWidget (바탕화면 달력 및 독립 메모 위젯)

Windows 바탕화면 위에 상주하며 일정 관리 및 스티커 메모를 작성할 수 있는 **DesktopCal 스타일**의 C# .NET 8 WPF 위젯 프로그램입니다.

> 🤖 **AI 생성 고지**: 이 프로젝트의 코드와 문서는 AI(Anthropic의 Claude)를 활용하여 생성되었습니다.

---

## 📌 주요 기능 구현 사항

1. **바탕화면 달력 표시 & 인라인 편집**:
   - 윈도우 바탕화면 위에 반투명 형태로 깔끔하게 표시됩니다.
   - 달력의 특정 일자 칸을 **더블클릭**하면 팝업 카드가 뜨며 메모/할 일을 작성, 저장, 삭제할 수 있습니다.
2. **상단 제어 바 (Top Bar)**:
   - 이전 달(◀), 다음 달(▶), 오늘 버튼 제공.
   - 상단 바를 마우스로 잡고 바탕화면 원하는 위치로 자유롭게 드래그 이동 가능.
3. **투명도 조절 스크롤바**:
   - 상단 바 좌측의 슬라이더(0.25 ~ 1.0)를 통해 위젯 및 메모장의 투명도를 즉시 조절할 수 있습니다.
4. **위치 고정 (📌) 모드**:
   - 상단 바 우측의 **고정** 버튼을 누르면 위치가 고정되어 마우스 드래그로 움직이지 않습니다.
   - 고정은 창 위치만 잠그며, 상단 바의 제어 버튼들(월 이동, 메모 추가, 투명도 슬라이더 등)은 그대로 사용할 수 있습니다. 고정 버튼을 다시 누르면 해제됩니다.
5. **메모 목록 & 독립 메모장 ('📝 메모' 버튼)**:
   - 상단 바 좌측의 `📝 메모` 버튼을 누르면 메모 목록 창이 열립니다.
   - 목록에서 `＋ 추가`로 새 메모를 만들고, 행을 선택한 뒤 `삭제`로 지울 수 있습니다. `확인`을 누르면 목록 창이 닫힙니다.
   - 각 행의 `표시` 버튼은 현재 표시 상태(ON/OFF)를 보여주며, 누르면 바탕화면에 노란색 포스트잇 메모로 표시하거나 숨깁니다.
   - 메모장 상단 바를 잡고 자유롭게 이동 가능하며, 최소화(`―`)와 숨기기(`✕`) 기능을 제공합니다. 숨긴 메모는 삭제되지 않고 목록에 남습니다.
   - 새로 추가하거나 표시한 메모는 달력과 루틴 창보다 위에 나타납니다.
   - 메인 달력에서 **고정(Pin)**을 켜면 모든 메모장의 위치도 함께 잠깁니다. (최소화·숨기기는 계속 사용 가능)
6. **데이터 & 위치 영속성 (자동 저장)**:
   - 입력한 일정, 메모 내용, 창 좌표, 크기, 투명도, 고정 여부가 `%APPDATA%\DesktopCalendarWidget\config.json`에 실시간 자동 저장됩니다.
7. **윈도우 시작프로그램 등록**:
   - 상단 바의 **시작프로그램** 체크박스를 체크하면 윈도우 부팅 시 자동 실행되도록 레지스트리에 자동 등록/해제됩니다.
8. **요일별 루틴 위젯 (🔁 루틴)**:
   - 상단 바 우측 **고정** 버튼 옆의 `🔁 루틴` 버튼으로 가로로 긴 별도 루틴 창을 열고 닫을 수 있습니다.
   - 월~일 각 요일 칸을 **더블클릭**하면 달력과 같은 편집 카드가 뜨며 해당 요일의 루틴을 작성/저장/삭제할 수 있습니다.
   - 루틴은 날짜가 아닌 **요일 기준**으로 저장되므로 주가 바뀌어도 초기화되지 않고 매주 반복 표시됩니다. 오늘 요일은 파란색으로 강조됩니다.
   - 창 위치·크기·열림 여부가 저장되며, 메인 달력의 **고정**과 **투명도** 설정을 함께 따릅니다.

---

## 🛠️ 빌드 및 실행 방법

### 방법 1: Visual Studio 사용
1. `JustCalendar/DesktopCalendarWidget.sln` 파일을 **Visual Studio 2022** 이상(또는 JetBrains Rider)에서 엽니다.
   *(필수 구성 요소: .NET 데스크톱 개발 워크로드, .NET 8.0 SDK)*
2. 상단 빌드 구성을 `Release` 또는 `Debug`로 설정하고 `F5` 키를 눌러 실행합니다.

### 방법 2: .NET CLI 터미널 사용
저장소 루트에서 `JustCalendar` 폴더로 이동한 뒤 다음 명령어를 실행합니다:
```bash
cd JustCalendar
dotnet build -c Release
dotnet run --project DesktopCalendarWidget
```

단일 실행 파일(Single File)로 배포하려면 (같은 `JustCalendar` 폴더에서):
```bash
# .NET 런타임 포함 (.NET 설치 없이 실행 가능, 용량 큼)
dotnet publish DesktopCalendarWidget -p:PublishProfile=Portable
# → DesktopCalendarWidget/bin/publish/portable/DesktopCalendarWidget-portable.exe

# .NET 런타임 미포함 (.NET 8 Desktop Runtime 필요, 용량 작음)
dotnet publish DesktopCalendarWidget -p:PublishProfile=Portable-net8
# → DesktopCalendarWidget/bin/publish/portable-net8/DesktopCalendarWidget-portable-net8.exe
```
각 출력 폴더에는 exe 파일 하나만 생성됩니다.
Visual Studio에서는 프로젝트 우클릭 → **게시**에서 같은 프로필을 선택할 수 있습니다.