# ARC eye AR SDK

## 개요
ARC eye AR SDK는 AR 컨텐츠 저작툴인 AMapper의 데이터를 이용하여, 공간에 배치 된 오브젝트들을 쉽게 증강시킬 수 있는 Unity 패키지입니다.

## 문서
[ARSDK documentation](https://ar.naverlabs.com/docs/arsdk)

## 샘플 프로젝트 내용
### amproj 파일 로드
* Play mode 실행 시 StreamingAssets 디렉토리 하위의 Example 프로젝트 로드

### 스테이지 로드
* 스테이지 이름을 이용한 로딩 테스트
  1. **Stage** 버튼 클릭
  2. 드롭다운 메뉴에서 스테이지 선택
  3. **TryUpdateStage** 버튼 클릭
* LayerInfo 값을 이용한 로딩 테스트
  1. 'Enter LayerInfo...' 필드에 LayerInfo 값 입력 (ex. example_building_1f_0001234)
  2. **Set LayerInfo** 버튼 클릭

### ARSDK 초기화
1. **Stage** 버튼 클릭
2. **Reset** 버튼 클릭

### 이동
* 왼쪽 Ctrl를 클릭한 후 WASD 키를 눌러서 이동
* 왼쪽 Ctrl를 클릭한 후 마우스를 이동하여 회전

### 미니맵 컨트롤
1. **Map** 버튼 클릭
1. **Show Minimap**, **Show Full Map**, **Hide Map** 버튼을 눌러서 맵 컨트롤 

### POI 로드
* Play mode 실행 시 amproj 파일을 로딩하면서 자동으로 POI 정보 로딩
* 로딩 결과는 ARPlayGround의 `OnPOIList` 이벤트를 통해 `ARSDKExample.cs`에 전달
* 전달 결과를 **POI** 탭에서 확인

### 내비게이션
1. **POI** 버튼 클릭 후 적당한 목적지 선택
1. **Navigation** 버튼 클릭 후 층간 이동 수단 선택
1. **Load Navigation** 버튼을 클릭하여 길찾기 시작
1. **Unload Navigation** 버튼을 클릭하여 길찾기 종료

### 기타
* Custom Range
  1. **misc.** 버튼 클릭
  1. Custom Range 진입 및 이탈 시 결과 화면 출력
* NextStep, ARItem 시각화
  1. **misc.** 버튼 클릭
  1. **Hide NextStep**, **Hide ARItems** 버튼을 클릭해서 시각화 제어

## Contact
* dl_ar@naverlabs.com

## License

전체 오픈소스 라이선스는 [LICENSE](./LICENSE) 에서 확인하세요.
