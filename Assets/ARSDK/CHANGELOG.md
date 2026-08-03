# ChangeLog

## [1.10.0] - 2026-08-03
### Changed
* `ARPlayGround`에 단일 인자 오버로드 `SetLayerInfo(string)` 추가. 매개변수가 하나뿐이라 UnityEvent 함수 드롭다운에 노출되어 코드 없이 Inspector에서 바인딩 가능. 기존 `SetLayerInfo(string, bool)`은 그대로 유지

## [1.9.0] - 2026-06-26
### Changed
* `ARPlayGround`에 `SetDestinationArrivalDistance` 메서드 추가

## [1.8.3] - 2026-04-03
### Changed
* glTFast 오브젝트 삭제 시 비정상적인 타이밍에 `NativeArray`가 삭제되는 문제 수정
* 오브젝트 비동기 생성/삭제 관련 예외 처리 추가
* NextStep Dot의 위치 갱신 시 발생하는 떨림 현상 제거
* DefaultTurnSpot의 루핑 애니메이션 실행 중 MeterText, DistanceText 비활성화. 남은 거리 텍스트 중앙 정렬 및 renderQueue 조정

## [1.8.2] - 2026-03-30
### Changed
* 활성 씬이 변경되면 Editor의 amproj 오버레이가 갱신되도록 변경
* TargetTexture가 할당되지 않았거나 카메라가 비활성화된 경우 미니맵이 렌더링되지 않는 문제 수정
* `StageConfig`에서 모델 데이터 브라우저가 활성화되지 않는 문제 수정

## [1.8.1] - 2026-03-18
### Changed
* v3 `.amproj` 맵 모델 로딩 추가
* LayerInfo와 맵 리소스를 설정하는 `StageConfig` 추가(기존 `StageResourceConfig`에서 이름 변경). amproj 버전에 따라 분기하는 `LayerInfoConverter` 추가
* Editor 기반 amproj 시각화 추가
* 커스터마이징 가능한 TurnSpot 생성 구조 추가
* `ContentsPath`를 `ContentsFolder`로 이름 변경하고 경로 검증이 포함된 폴더 브라우저 추가
* `NaviSpotGenerator`를 `NaviItemGenerator`로 이름 변경
* 내장 TextMeshPro 패키지 제거(uGUI 2.0.0에서 제공). New Input System 지원 추가
* LayerInfo 매칭을 부분 문자열 기반으로 변경
* amproj 파일 로딩 리팩터링 및 amproj 버전에 따른 native 로드 메서드 분기
* `ARPlayGround` 없이 `AMProjVisualizer`가 동작하지 않는 문제 수정
* 내비게이션 중 경로를 재탐색할 때 TurnSpot 거리 라벨에서 발생하는 오류 수정

## [1.8.0] - 2026-03-09
### Changed
* URP를 프로젝트 기본 렌더 파이프라인으로 설정하고 MapCamera 자동 처리
* 프로젝트 기준 Unity 버전을 6.3으로 상향
* AMapper Web(`.amproj`) 파일 지원 추가
* Legacy TextMesh를 TextMeshPro로 교체
* `MapCameraRig`를 `MapCameraController`로 이름 변경하고 단일 카메라 3-pass 렌더링으로 전환
* `OnPOIList` 이벤트를 `OnPOIListLoaded`로 이름 변경
* 커스터마이징을 위한 추상 POI 렌더러(`SignPOIRenderer`, `MapPOIRenderer`, `TurnSpotRenderer`) 추가
* `POIGenerator`가 POI 타입에 맞는 프리팹만 허용하도록 변경
* 경로 두께 설정과 `PathAssetGenerator` 추가
* ARSDK 프로젝트 유효성 검사 추가
* 모델 로딩 중 비동기 문제로 간헐적으로 발생하는 크래시 수정
* `LoadOnAwake`가 false일 때 `AMProjVisualizer`가 초기화되지 않는 문제 수정
* `LoadAsync` 사용 시 `LayerInfoConverter`가 로드되지 않는 문제 수정
* `UnityModel`에서 바운딩 박스 계산에 스케일이 반영되지 않는 문제 수정

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