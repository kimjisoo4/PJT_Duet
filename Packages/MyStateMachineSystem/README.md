# MyStateMachine

과거 Unity 에서 Tank!를 통해 보여준 Scriptable Object 기반의 상태머신 시스템.


State, Decision, Action 의 스크립터블 오브젝트로 구성되어 있음.

StateMachine 에서 현재 State 를 업데이트하여 Action 을 처리한 뒤, Decision 의 결과에 따라 상태 전환을 실시한다.


Unity 에서 소개할 때는 StateMachine 에 데이터 또한 저장해놨던걸로 기억함.

이 시스템은 Dictionary 에 stateMachine 을 키값으로 데이터를 저장해놓는 방식으로 작동함.

일단 DictionaryContainer<TKEY,TValue> 의 스크립터블 오브젝트를 통해 데이터를 캐시하여 작동하도록 구성함.

이부분은 추후 조정할 예정.



- https://github.com/kimjisoo4/MyUtilities 가 필요함.

사용은 자유이나 그로 인해 생긴 오류에 대해서는 책임지지 않음.

자세한 정보 : --
