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
        [Header(" [ Stage Reword System ] ")]
        [SerializeField] private GameObject _stageRewordActor;
        [SerializeField] private RewordDataComponent[] _rewordSelects;
        [SerializeField] private RewordDataComponent _rewordInformation;
        [Space(5f)]
        [SerializeField] private PlayerManager _playerManager;
        [Space(5f)]
        [SerializeField] private GameEvent _onStageReword;
        [SerializeField] private GameObjectListVariable _activeUIVariable;

        [Header(" Reword Datas ")]
        [SerializeField] private List<EquipmentItem> _stageRewordDatas;

        private readonly List<EquipmentItem> _selectedRewords = new();

        private void Awake()
        {
            _stageRewordActor.SetActive(false);

            _onStageReword.OnTriggerEvent += _onStageReword_OnTriggerEvent;

            foreach (var rewordDisplay in _rewordSelects)
            {
                if(rewordDisplay.TryGetComponent(out ISubmitEventListener submitEventListener))
                {
                    submitEventListener.OnSubmited += SubmitEventListener_OnSubmited;
                }
                if(rewordDisplay.TryGetComponent(out ISelectEventListener selectEventListener))
                {
                    selectEventListener.OnSelected += SelectEventListener_OnSelected;
                }
                
            }
        }
        private void OnDestroy()
        {
            if (_onStageReword)
            {
                _onStageReword.OnTriggerEvent -= _onStageReword_OnTriggerEvent;
            }

            if(_rewordSelects is not null && _rewordSelects.Length > 0)
            {
                foreach (var rewordDisplay in _rewordSelects)
                {
                    if (!rewordDisplay)
                        continue;

                    if (rewordDisplay.TryGetComponent(out ISubmitEventListener submitEventListener))
                    {
                        submitEventListener.OnSubmited -= SubmitEventListener_OnSubmited;
                    }
                    if (rewordDisplay.TryGetComponent(out ISelectEventListener selectEventListener))
                    {
                        selectEventListener.OnSelected -= SelectEventListener_OnSelected;
                    }
                }
            }
        }

        [ContextMenu(nameof(OnStageReword), false, 10000000)]
        private void OnStageReword()
        {
            if (_stageRewordDatas.Count == 0)
                return;

            _selectedRewords.Clear();
            for (int i = 0; i < _rewordSelects.Length; i++)
            {
                var rewordDisplay = _rewordSelects[i];
                int targetIndex = Random.Range(0, _stageRewordDatas.Count);
                var rewordData = _stageRewordDatas.ElementAtOrDefault(targetIndex);

                if (rewordData)
                {
                    _stageRewordDatas.RemoveAt(targetIndex);
                    _selectedRewords.Add(rewordData);
                }

                rewordDisplay.SetData(rewordData);
            }

            _stageRewordActor.SetActive(true);
            _activeUIVariable.Add(gameObject);

            EventSystem.current.SetSelectedGameObject(_rewordSelects.ElementAt(Mathf.FloorToInt(_rewordSelects.Length * 0.5f)).gameObject);
        }

        [ContextMenu(nameof(EndStageReword), false, 10000000)]
        private void EndStageReword()
        {
            _stageRewordDatas.AddRange(_selectedRewords);
            _selectedRewords.Clear();

            _stageRewordActor.SetActive(false);
            _activeUIVariable.Remove(gameObject);
        }

        private void OnSubmitReword(EquipmentItem equipmentItem)
        {
            if(_playerManager.HasPlayerController)
            {
                if(_playerManager.PlayerController.gameObject.TryGetComponent(out IEquipmentWearer equipmentWearer))
                {
                    var equipment = equipmentItem.CreateSpec();

                    if (equipmentWearer.TryEquipItem(equipment))
                    {
                        _selectedRewords.Remove(equipmentItem);

                        EndStageReword();
                    }
                }
            }
        }

        private void OnSelectReword(EquipmentItem equipmentItem)
        {
            _rewordInformation.SetData(equipmentItem);
        }

        private void _onStageReword_OnTriggerEvent()
        {
            OnStageReword();
        }

        private void SelectEventListener_OnSelected(ISelectEventListener selectEventListener, UnityEngine.EventSystems.BaseEventData obj)
        {
            var reword = _rewordSelects.First(x => x.gameObject == selectEventListener.gameObject);

            OnSelectReword(reword.EquipmentItem);
        }
        private void SubmitEventListener_OnSubmited(ISubmitEventListener submitEventListener, UnityEngine.EventSystems.BaseEventData obj)
        {
            var reword = _rewordSelects.First(x => x.gameObject == submitEventListener.gameObject);

            OnSubmitReword(reword.EquipmentItem);
        }
    }
}
