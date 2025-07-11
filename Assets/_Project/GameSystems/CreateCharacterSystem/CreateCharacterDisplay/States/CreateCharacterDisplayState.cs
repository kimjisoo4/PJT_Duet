﻿using StudioScor.Utilities;
using UnityEngine;

namespace PF.PJT.Duet.CreateCharacterSystem
{
    [AddComponentMenu("Duet/CreateCharacter/Display/State/Create Character Display State")]
    public class CreateCharacterDisplayState : BaseStateMono
    {
        [System.Serializable]
        public class StateMachine : FiniteStateMachineSystem<CreateCharacterDisplayState> { }

        [Header(" [ Create Character Display State ] ")]
        [SerializeField] private CreateCharacterDisplay _createCharacterDisplay;
        public CreateCharacterDisplay CreateCharacterDisplay => _createCharacterDisplay;

        protected override void OnValidate()
        {
#if UNITY_EDITOR
            if (SUtility.IsPlayingOrWillChangePlaymode)
                return;

            base.OnValidate();

            if (!_createCharacterDisplay)
            {
                _createCharacterDisplay = GetComponentInParent<CreateCharacterDisplay>();
            }
#endif
        }
    }
}
