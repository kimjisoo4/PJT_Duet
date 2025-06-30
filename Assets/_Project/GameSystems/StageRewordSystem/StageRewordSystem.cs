using StudioScor.PlayerSystem;
using StudioScor.Utilities;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;

namespace PF.PJT.Duet
{
    public class StageRewordSystem : BaseMonoBehaviour
    {
        public delegate void StageRewordEventHandler(StageRewordSystem stageRewordSystem);
        public delegate void SelectRewordEventHandler(StageRewordSystem stageRewordSystem, ItemSO selectItem);


        [Header(" [ Stage Reword System ] ")]
        [SerializeField] private GameObject _stageRewordActor;
        [SerializeField] private SelectRewordComponent[] _rewordSelects;
        [SerializeField] private RewordInformationDisplay _rewordInformation;
        [Space(5f)]
        [SerializeField] private PlayerManager _playerManager;
        [Space(5f)]
        [Header(" Block Input ")]
        [SerializeField] private InputBlocker _inputBlocker;
        [SerializeField] private EBlockInputState _blockInput = EBlockInputState.Game;

        [Header(" Reword Datas ")]
        [SerializeField] private List<ItemSO> _stageRewordDatas;

        [Header(" Events ")]
        [Header(" Listen ")]
        [SerializeField] private GameEvent _onRequestRewordSystem;
        [Header(" Post ")]
        [SerializeField] private GameEvent _onStartedRewordSystem;
        [SerializeField] private GameEvent _onFinishedRewordSystem;

        private readonly List<ItemSO> _selectedRewords = new();
        private bool _wasInit;
        private int _remainWaitFinishedCount;

        public event StageRewordEventHandler OnActivated;
        public event StageRewordEventHandler OnDeactivated;
        public event SelectRewordEventHandler OnSelectedReword;
        private void OnValidate()
        {
#if UNITY_EDITOR
            if (!_inputBlocker)
            {
                _inputBlocker = SUtility.FindAssetByType<InputBlocker>();
            }
#endif
        }

        private void Awake()
        {
            Init();
        }
        private void OnDestroy()
        {
            if (_onRequestRewordSystem)
            {
                _onRequestRewordSystem.OnTriggerEvent -= _onStageReword_OnTriggerEvent;
            }

            if(_rewordSelects is not null && _rewordSelects.Length > 0)
            {
                foreach (var selectReword in _rewordSelects)
                {
                    if (!selectReword)
                        continue;

                    selectReword.OnSelected -= SelectReword_OnSelected;
                    selectReword.OnSubmited -= SelectReword_OnSubmited;
                    selectReword.OnFinishedActivate -= SelectReword_OnFinishedActivate;
                    selectReword.OnFinishedDeactivate -= SelectReword_OnFinishedDeactivate;
                }
            }

            if(_rewordInformation)
            {
                _rewordInformation.OnFInishedActivate -= _rewordInformation_OnFInishedActivate;
                _rewordInformation.OnFInishedDeactivate -= _rewordInformation_OnFInishedDeactivate;
            }

            _inputBlocker.UnblockInput(this);
        }

        public void Init()
        {
            if (_wasInit)
                return;

            _wasInit = true;

            _onRequestRewordSystem.OnTriggerEvent += _onStageReword_OnTriggerEvent;

            foreach (var selectReword in _rewordSelects)
            {
                selectReword.OnSelected += SelectReword_OnSelected;
                selectReword.OnSubmited += SelectReword_OnSubmited;
                selectReword.OnFinishedActivate += SelectReword_OnFinishedActivate;
                selectReword.OnFinishedDeactivate += SelectReword_OnFinishedDeactivate;

                selectReword.Init();
            }

            _rewordInformation.OnFInishedActivate += _rewordInformation_OnFInishedActivate;
            _rewordInformation.OnFInishedDeactivate += _rewordInformation_OnFInishedDeactivate;
            _rewordInformation.Init();

            _stageRewordActor.SetActive(false);
        }

        [ContextMenu(nameof(Activate), false, 10000000)]
        public void Activate()
        {
            if (_stageRewordDatas.Count == 0)
                return;

            _selectedRewords.Clear();

            _remainWaitFinishedCount = 0;

            for (int i = 0; i < _rewordSelects.Length; i++)
            {
                var rewordSelect = _rewordSelects[i];
                int targetIndex = Random.Range(0, _stageRewordDatas.Count);
                var rewordData = _stageRewordDatas.ElementAtOrDefault(targetIndex);

                if (rewordData)
                {
                    _stageRewordDatas.RemoveAt(targetIndex);
                    _selectedRewords.Add(rewordData);
                }

                _remainWaitFinishedCount++;
                rewordSelect.SetData(rewordData);
                rewordSelect.Activate();
            }

            _stageRewordActor.SetActive(true);
            _inputBlocker.BlockInput(this, _blockInput);

            Invoke_OnStartedRewordSystem();
        }


        [ContextMenu(nameof(Deactivate), false, 10000000)]
        public void Deactivate()
        {
            _remainWaitFinishedCount = 0;

            _rewordInformation.Deactivate();
        }

        private void OnSubmitReword(ItemSO submitItem)
        {
            if(_playerManager.HasPlayerController)
            {
                if (submitItem.TryUseItem(_playerManager.PlayerController.gameObject))
                {
                    _selectedRewords.Remove(submitItem);

                    Invoke_OnSelectedReword(submitItem);
                }
            }
        }

        private void OnSelectReword(ItemSO selectItem)
        {
            _rewordInformation.SetData(selectItem);
        }

        private void SelectReword_OnSubmited(SelectRewordComponent selectRewordComponent)
        {
            if (_remainWaitFinishedCount != 0)
                return;

            if (!selectRewordComponent || !selectRewordComponent.ItemData)
                return;

            OnSubmitReword(selectRewordComponent.ItemData);
        }


        private void _onStageReword_OnTriggerEvent()
        {
            Activate();
        }

        private void SelectReword_OnSelected(SelectRewordComponent selectRewordComponent)
        {
            if (!selectRewordComponent || !selectRewordComponent.ItemData)
                return;

            OnSelectReword(selectRewordComponent.ItemData);
        }

        private void SelectReword_OnFinishedActivate(SelectRewordComponent selectRewordComponent)
        {
            _remainWaitFinishedCount--;

            if (_remainWaitFinishedCount <= 0)
            {
                _rewordInformation.Activate();
            }
        }

        private void SelectReword_OnFinishedDeactivate(SelectRewordComponent selectRewordComponent)
        {
            _remainWaitFinishedCount--;

            if(_remainWaitFinishedCount <= 0)
            {
                _stageRewordDatas.AddRange(_selectedRewords);
                _selectedRewords.Clear();

                _stageRewordActor.SetActive(false);
                _inputBlocker.UnblockInput(this);

                Invoke_OnFinishedRewordSystem();
            }
        }

        private void _rewordInformation_OnFInishedActivate(RewordInformationDisplay rewordInformationDisplay)
        {
            if (!_rewordSelects.FirstOrDefault((x) => x.gameObject == EventSystem.current.currentSelectedGameObject))
            {
                EventSystem.current.SetSelectedGameObject(_rewordSelects.ElementAt(Mathf.FloorToInt(_rewordSelects.Length * 0.5f)).gameObject);
            }
        }

        private void _rewordInformation_OnFInishedDeactivate(RewordInformationDisplay rewordInformationDisplay)
        {
            foreach (var selectReword in _rewordSelects)
            {
                _remainWaitFinishedCount++;

                selectReword.Deactivate();
            }
        }

        private void Invoke_OnSelectedReword(ItemSO selectedItem)
        {
            Log($"{nameof(OnSelectedReword)} - {selectedItem}");

            OnSelectedReword?.Invoke(this, selectedItem);
        }

        private void Invoke_OnStartedRewordSystem()
        {
            Log(nameof(OnActivated));

            if (_onStartedRewordSystem)
            {
                _onStartedRewordSystem.Invoke();
            }

            OnActivated?.Invoke(this);
        }

        private void Invoke_OnFinishedRewordSystem()
        {
            Log(nameof(OnDeactivated));

            if (_onFinishedRewordSystem)
            {
                _onFinishedRewordSystem.Invoke();
            }

            OnDeactivated?.Invoke(this);
        }
    }
}
