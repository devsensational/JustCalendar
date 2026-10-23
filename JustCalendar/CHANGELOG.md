# Changelog

이 프로젝트의 주요 변경 사항을 기록합니다.

형식은 [Keep a Changelog](https://keepachangelog.com/ko/1.1.0/)를 따르며,
버전은 [Semantic Versioning](https://semver.org/lang/ko/) (`MAJOR.MINOR.PATCH`)을 사용합니다.

## [Unreleased]

## [1.1.0] - 2026-10-23

### Added
- 메모 목록 창: 상단 바의 `📝 메모` 버튼으로 열림. 메모 추가, 삭제, 확인(창 닫기) 버튼 제공
- 메모 행마다 `표시` 토글 버튼: 바탕화면 위젯 표시 여부를 보여주고 켜고 끌 수 있음

### Changed
- 메모 위젯의 ✕ 버튼이 메모를 삭제하지 않고 숨기기만 함. 숨긴 메모는 목록에 남아 있어 다시 표시할 수 있음
- 메모 삭제는 메모 목록에서만 가능하며, 내용이 있는 메모는 삭제 전에 확인을 받음
- 상단 바의 `＋ 메모` 버튼이 메모를 바로 만들지 않고 메모 목록 창을 엶
- 새로 추가하거나 표시한 메모는 달력과 루틴 창보다 위에 표시됨
- 메모와 루틴 창이 달력 창과 한 그룹으로 묶임. Alt+Tab에서 달력을 선택하면 함께 앞으로 나오며, 메모/루틴 창은 항상 달력 창보다 위에 표시됨

### Fixed
- Alt+Tab 목록에 달력, 루틴, 메모 창이 각각 따로 표시되던 문제. 이제 달력 하나만 표시됨
- 종료 버튼으로 앱을 끌 때 열려 있던 메모가 저장 목록에서 빠질 수 있던 문제

## [1.0.0] - 2026-10-23

### Added
- 바탕화면 달력: 월 단위 6x7 달력, 날짜 칸 더블클릭으로 일정 메모 작성/저장/삭제
- 상단 제어 바: 이전/다음 달, 오늘 이동, 드래그로 창 이동
- 투명도 슬라이더 (0.25 ~ 1.0): 달력, 메모, 루틴 창에 함께 적용
- 독립 스티커 메모: 생성, 이동, 최소화, 닫기
- 요일별 루틴 창: 월~일 요일 기준으로 저장되어 매주 반복 표시, 오늘 요일 강조
- 위치 고정(📌) 모드: 모든 위젯 창의 위치를 함께 잠금
- Windows 시작프로그램 등록/해제 (HKCU Run 레지스트리)
- 설정 및 데이터 자동 저장: `%APPDATA%\DesktopCalendarWidget\config.json`
- 모든 위젯 창을 바탕화면 맨 뒤에 배치

### Changed
- 고정 모드가 창 위치만 잠그도록 변경. 고정 중에도 메모 추가, 월 이동, 투명도 조절, 루틴 열기/닫기, 종료, 메모 최소화/닫기, 루틴 창 닫기를 사용할 수 있음 (루틴 창 크기 조절은 계속 잠금)

[Unreleased]: https://github.com/devsensational/JustCalendar/compare/v1.1.0...HEAD
[1.1.0]: https://github.com/devsensational/JustCalendar/compare/v1.0.0...v1.1.0
[1.0.0]: https://github.com/devsensational/JustCalendar/releases/tag/v1.0.0
