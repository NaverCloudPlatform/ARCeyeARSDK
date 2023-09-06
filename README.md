# ARPG-Unity

## 개요
ARPG Unity SDK는 AR 컨텐츠 저작툴인 AMapper의 데이터를 이용하여, 공간에 배치 된 오브젝트들을 쉽게 증강시킬 수 있는 Unity 패키지입니다.

## 설치 방법
1. Unity 프로젝트에 [NewtonSoft.Json](https://github.com/jilleJr/Newtonsoft.Json-for-Unity/wiki/Install-official-via-UPM) 설치
1. Unity 프로젝트에 [glTFast](https://github.com/atteneder/glTFast)를 설치
1. VL SDK unitypackage 추가
2. ARPG SDK unitypackage 추가

## 샘플 프로젝트
샘플 프로젝트는 ARPG > Example > Scenes 경로에서 확인하실 수 있습니다. 테스트를 위해 `0.example_simple`, `1.example_navigation`, `2.example_vlsdk`, `3.example_arnavi`라는 이름의 프로젝트들이 준비 되어 있습니다.

### 샘플 프로젝트 실행 준비 사항
Assets 디렉토리의 하위에 `StreamingAssets`라는 이름의 디렉토리를 생성한 후 AMapper 컨텐츠를 배치해야 합니다. 해당 경로에 배치된 데이터를 바탕으로 화면을 구성합니다.

### 샘플 프로젝트 설명
`0.example_simple`는 ARPG SDK의 최소한의 기능을 확인할 수 있는 프로젝트입니다. 프로젝트를 실행시킨 후 'SetStage' 버튼을 누르면 테스트 지역이 로드 됩니다. `Ctrl`키를 누른 상태에서 `W,S,A,D` 키를 누르면 카메라를 조작할 수 있습니다. 마우스를 이동하여 방향을 설정할 수 있습니다.

`1.example_navigation`은 ARPG SDK의 경로 탐색 기능을 확인할 수 있는 프로젝트입니다. 프로젝트를 실행시킨 후 'SetStage 1F' 버튼을 누르면 테스트 지역이 로드 됩니다. 오른쪽 상단의 'Show Dest Rect' 버튼을 누른 뒤 목적지를 선택하면 해당 경로로 이동하는 내비게이션을 동작시킬 수 있습니다.

`2.example_vlsdk`는 ARPG SDK와 VL SDK를 연동하는 방법을 확인할 수 있는 프로젝트입니다. Scene에 추가 되어 있는 'Video Player'에 동영상 데이터를 할당한 뒤 실행하면 VL SDK가 동작하는 모습을 확인할 수 있습니다.

`3.example_arnavi`는 ARPG SDK와 VL SDK를 연동하여 실제로 동작하는 AR 내비게이션 앱을 구현한 프로젝트입니다. ARPG SDK를 어떤 방식으로 활용하면 좋을지 확인할 수 있습니다.

## 사용 방법
* Wiki 참고


## Contact
* dl_ar@naverlabs.com

## License

전체 오픈소스 라이선스는 [LICENSE](./LICENSE) 에서 확인하세요.