using UnityEngine;
using StudioScor.AbilitySystem;
using StudioScor.Utilities;
using StudioScor.GameplayTagSystem;

namespace PF.PJT.Duet.Pawn.PawnAbility
{
    [CreateAssetMenu(menuName = "Project/Duet/PawnAbility/Effect/new Apply Groggy Ability", fileName = "GA_PawnAbiltiy_ApplyGroggy")]
    public class ApplyGroggyAbility : GASAbility
    {
        [Header(" [ Apply Groggy Ability ] ")]
        [SerializeField] private GameplayTag _triggerTag;
        [SerializeField] private GameplayTag _blockStiffenTag;

        [Header(" Animation ")]
        [SerializeField] private string _animationName = "Groggy";
        [SerializeField][Range(0f, 1f)] private float _blendInTime = 0.2f;
        [SerializeField][Range(0f, 1f)] private float _animationOffset = 0.2f;
        public override IAbilitySpec CreateSpec(IAbilitySystem abilitySystem, int level = 0)
        {
            return new Spec(this, abilitySystem, level);
        }

        public class Spec : GASPassiveAbilitySpec
        {
            protected new readonly ApplyGroggyAbility _ability;
            private readonly ICharacter _character;
            private readonly AnimationPlayer _animationPlayer;
            private readonly AnimationPlayer.Events _animationEvent = new();
            private readonly int ANIM_GROGGY_ID;

            private bool _wasPlayedAnim;

            public Spec(Ability ability, IAbilitySystem abilitySystem, int level) : base(ability, abilitySystem, level)
            {
                _ability = ability as ApplyGroggyAbility;
                _character = gameObject.GetComponent<ICharacter>();
                _animationPlayer = _character.Model.GetComponent<AnimationPlayer>();
                _animationEvent.OnStarted += _animationEvent_OnStarted;
                _animationEvent.OnFailed += _animationEvent_OnFailed;
                _animationEvent.OnCanceled += _animationEvent_OnCanceled;
                _animationEvent.OnStartedBlendOut += _animationEvent_OnStartedBlendOut;

                ANIM_GROGGY_ID = Animator.StringToHash(_ability._animationName);
            }

            protected override void EnterAbility()
            {
                base.EnterAbility();

                _wasPlayedAnim = false;

                GameplayTagSystem.AddBlockTag(_ability._blockStiffenTag);
                _animationPlayer.Play(ANIM_GROGGY_ID, _ability._blendInTime, _ability._animationOffset);
                _animationPlayer.AnimationEvents = _animationEvent;
            }

            protected override void ExitAbility()
            {
                base.ExitAbility();

                _animationPlayer.TryStopAnimation(ANIM_GROGGY_ID);
            }

            protected override void OnCancelAbility()
            {
                base.OnCancelAbility();

            }
            private void _animationEvent_OnStartedBlendOut()
            {
                GameplayTagSystem.RemoveBlockTag(_ability._blockStiffenTag);
            }

            private void _animationEvent_OnCanceled()
            {
                if (_wasPlayedAnim)
                {
                    GameplayTagSystem.RemoveBlockTag(_ability._blockStiffenTag);
                    CancelAbility();
                }
            }

            private void _animationEvent_OnFailed()
            {
                GameplayTagSystem.RemoveBlockTag(_ability._blockStiffenTag);
                CancelAbility();
            }

            private void _animationEvent_OnStarted()
            {
                _wasPlayedAnim = true;
            }
        }
    }
}
