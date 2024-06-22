# BodySystem

Tag 를 통해 Transform 을 관리하는 시스템.


BodySystemComponent 에서 딕셔너리로 키와 BodyPart를 저장해놓고, 불러오는 형태로 사용함.

위치 정보가 필요한 곳에 BodyPartComponent 를 추가하여 BodyTag를 설정하고, 활성화 되면 BodySystem에 자신을 등록시킨다.

이후 BodySystem에서 Tag 를 확인하여 가져오는 형태.



https://github.com/kimjisoo4/MyUtilities 가 필요함.

- 사용은 자유이나 그로 인해 생긴 오류에 대해서는 책임지지 않음.

자세한 정보 : --
