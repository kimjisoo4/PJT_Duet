# 👩🏻‍💻 김지수 포트폴리오

## Unity 개발자 김지수 입니다.
&nbsp;3D 모델러 1년, 애니메이터 1년, 프로그래머 3년 차인 얕지만 넓은 5년 차 게임 개발자 입니다.     

# 📝 프로젝트
&nbsp;액션 로그라이트를 지향하고 있습니다.   
3인칭 시점으로 만들고 있으며, 스컬, 원신 등과 같이 캐릭터를 변경하며 플레이할 수 있게 제작하고 있습니다.   

&nbsp;포트폴리오를 위해 제작 중인 Action Game 입니다.  
포트폴리오 사용을 위해 그래픽 애셋을 제외한 애셋 사용을 지양하고 제작하고 있습니다.    

&nbsp;이동, 회전, 어빌리티 시스템 등의 기능은 사전에 따로 모듈로서 제작하여 사용하고 있는 기능들입니다. 프로젝트를 진행하면서 수정 보완하고 있습니다.
    
# 📜 진행 사항
 #### 6월 17일 ~ 6월 21일
> &nbsp;현재 기본적인 이동 및 공격과 데미지 처리가 되어있고, 검사 클래스와 격투가 클래스를 일부 제작하였습니다.    
> &nbsp;AI 의 경우 플레이어에게 피격시 플레이어를 쫒아오는 정도만 임시로 만들어 두었습니다.

 # 🔎 모듈

### [Player System](https://github.com/kimjisoo4/MyPlayerSystem)
> &nbsp;Unreal Engine 의 Controller, Pawn 의 개념을 유사하게 따라 사용하고 있는 기능입니다.  
> &nbsp;IController 인터페이스와 IPawn 인터페이스가 1:1 로 매칭되어 일부 입력값을 공유하고 있습니다.    

### [GameplayTagSystem](https://github.com/kimjisoo4/MyGameplayTagSystem)
> &nbsp;Unreal Engine 의 GameplayTag 시스템을 참고하여 만든 기능입니다. Unreal Engine 과는 다르게 상위, 하위 태그의 구분이 없습니다.    
> &nbsp;각 GameplayTag 는 Scriptable Object 로 제작하였습니다.    
> &nbsp;소유자의 상태를 체크하기 위해 사용되며, Ability System, GameplayEffect System 과 연동하여 사용하고 있습니다.    

### [Ability System](https://github.com/kimjisoo4/MyAbilitySystem)
> &nbsp;Unreal Engine의 Gameplay Ability System 을 참고하여 만든 기능입니다. 기본적으로는 어빌리티를 부여하고, 실행 가능 여부를 판단하여 실행하도록 되어 있습니다.    
> &nbsp;Scriptable Object를 상속받은 Ability에서 IAbilitySpec interface 를 상속받은 클래스를 생성하여 사용됩니다.
> &nbsp;GameplayTagSystem 을 함께 사용하여,  Unreal Engine 의 GAS처럼 어빌리티의 실행 가능 여부를 GameplayTag 로 확인하여 실행하도록 하고 있습니다.    
 
### [GameplayEffect System](https://github.com/kimjisoo4/MyGameplayEffectSystem)
> &nbsp;Unreal Engine 의 Gameplay Effect 를 참고하여 만든 기능입니다. Ability System 과 마찬가지로 기본적으로 부여 가능 여부를 판단하여 실행하도록 되어있습니다.
> &nbsp;GameplayEffect 라는 Scriptable Object 로 IGameplayEffectSpec interface 를 상속받은 클래스를 생성하여 사용하고 있습니다.    

### [GameplayCue System](https://github.com/kimjisoo4/MyGameplayCueSystem)
> &nbsp;Scriptable Object 를 기반으로 각종 FX 를 조합하여 실행하는 기능입니다. Unreal Engine 의 기능을 참고하여 제작하였으나, 많은 수정으로 Unreal Engine 과는 다른 기능입니다.   
> &nbsp;GameplayCue 라는 Scriptable Object 와 Cue 라는 Scriptable Object 로 이루어져 있습니다. Cue는 각각의 FX를 가지고 있으며, GameplayCue 는 그런 Cue 를 묶은 컨테이너로 GameplayCue 를 통해 Cue 를 실행시킵니다.     

### [Body System](https://github.com/kimjisoo4/MyBodySystem)
> &nbsp;자식 컴포넌트에 쉽게 접근하기 위해 만든 시스템입니다. Unreal Engine에서 이름을 통해 본을 가져오는 기능을 생각하며 제작하였습니다.    
> &nbsp;부모에 IBodySystem interface 를 상속받은 클래스를 찾아 IBodypart 가 IBodySystem 에 Dictionary에 Scriptable Object 로 제작된 BodyTag를 Key로하여 저장되는 시스템입니다. BodyTag 를 통해 저장된 IBodypart에 손쉽게 접근할 수 있습니다.    

### [Stat System](https://github.com/kimjisoo4/MyStatSystem)
> &nbsp;캐릭터의 각종 속성값을 관리하는 시스템입니다.     
> &nbsp;Scriptable Object 로 만들어지는 StatTag 를 기반으로 Stat 을 저장하고, Modifier 를 통해 해당 값의 변경이 이루어지도록 되어있습니다.    
> &nbsp;IStatSystem 의 레벨에 따라 각 Stat 은 값이 변경될 수 있게 만들어 사용하고 있습니다.    

### [Status System](https://github.com/kimjisoo4/MyStatusSystem)
> &nbsp;캐릭터의 Current, Max 값을 관리하는 시스템입니다. HP 등에 사용되고 있습니다.    
> &nbsp;Stat 과 다르게 0 ~ MaxValue 를 가지며, 해당 값을 관리합니다.     
> &nbsp;StatSystem 과 연동하여 최대 체력을 조정하는 등으로 사용하고 있습니다.    

### [State Machine](https://github.com/kimjisoo4/MyStateMachineSystem)
> &nbsp;Unity Tank! 로 소개된 적이 있는 Scriptable Object 기반의 StateMachine 을 나름대로 조정하여 만들었습니다.    
> &nbsp;AI의 행동 관리를 위해 사용하고 있습니다.    

### [Movement System](https://github.com/kimjisoo4/MyMovementSystem)
> &nbsp;이동을 처리하기 위해 만든 모듈입니다. Modifer 와 같이 사용하여 캐릭터의 이동을 컨트롤 합니다.  이동해야하는 값을 AddVelocity와 AddPosition로 나누어 저장해놓고, 상속받은 클래스에서 이 값을 이용해서 이동을 처리합니다.    
> &nbsp;IMomvenetModifier 를 상속받은 클래스에서 이동에 대한 계산을 처리합니다.    

### [Rotation System](https://github.com/kimjisoo4/MyRotationSystem)
> &nbsp;회전을 처리하기 위해 만든 모듈입니다. 상태에 바라볼 방향, 바라보는 위치, 바라보는 대상으로 회전합니다.     

### [Utility](https://github.com/kimjisoo4/MyUtilities)
> &nbsp;각종 편의를 위해 만들어놓은 작은 클래스들이 있는 모듈입니다.  Base 가 되는 클래스들과 확장 함수, Attribute 등이 있습니다,    
> &nbsp;애니메이션을 CrossFade 형태로 실행시키는 AnimationPlayer, 데미지를 처리하는 IDamageableSystem, 간단한 Pooling 과 Timer 등이 있습니다.
