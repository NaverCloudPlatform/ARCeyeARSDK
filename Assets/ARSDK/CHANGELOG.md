# ChangeLog

## [1.7.0] - 2025-02-27
### Changed
* ARSDK와 관련성이 떨어지는 코드를 삭제하기 위해 기존 샘플 프로젝트 제거.
* 주요 기능들을 위주로 확인할 수 있는 `ARSDKExample.scene` 예제 추가.

## [1.6.1-preview.6] - 2025-02-24
### Changed
* `ARPlayGround.cs`의 `SetStage(string)`을 `TryUpdateStage(string)`으로 변경
* `ARPlayGround.cs`에 `ForceUpdateStage(string)` 메서드 추가
* `ARPlayGround.cs`의 `OnNavigationReSearched()`의 이름을 `OnNavigationRerouted()`로 변경

## [1.6.1-preview.5] - 2025-01-03
### Changed
* 크기가 다른 InfoPanel Frame 이미지 대응
* InfoPanel에서 Use Rounded Border 옵션이 정상적으로 적용되지 않는 문제 수정
* New Input System 지원

## [1.6.1-preview.4] - 2024-12-26
### Changed
* ARItem 태그 추가
* ARPlayGround에 `ShowARItems()`, `HideARItems()` 메서드 추가.

## [1.6.1-preview.2] - 2024-12-24
### Changed
* 좌표값 기반 LoadNavigation 기능 추가

## [1.6.1-preview.1] - 2024-12-19
### Changed
* macOS와 Windows 환경에서 터치 시스템 활성화