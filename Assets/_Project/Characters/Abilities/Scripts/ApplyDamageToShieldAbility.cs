using StudioScor.AbilitySystem;
using StudioScor.BodySystem;
using StudioScor.GameplayCueSystem;
using StudioScor.GameplayEffectSystem;
using StudioScor.StatusSystem;
using StudioScor.Utilities;
using UnityEngine;

namespace PF.PJT.Duet.Pawn.PawnAbility
{
    [CreateAssetMenu(menuName = "Project/Duet/PawnAbility/Effect/new Apply Damage To Shield Ability", fileName = "GA_PawnAbiltiy_ApplyDamageToShield")]
    public class ApplyDamageToShieldAbility : GASAbility
    {
        [Header(" [ Apply Damage To Shield Ability ] ")]
        [SerializeField] private StatusTag _shieldStatus;

        [Header(" Gameplay Cue ")]
        [SerializeField] private FGameplayCue _brokenShieldCue;
        [SerializeField] private BodyTag _brokenShieldPoint;

        [Header(" Gameplay Effect ")]
        [SerializeField] private GameplayEffect[] _gameplayEffects;

        public override IAbilitySpec CreateSpec(IAbilitySystem abilitySystem, int level = 0)
        {
            return new Spec(this, abilitySystem, level);
        }

        public class Spec : GASPassiveAbilitySpec
        {
            private new readonly ApplyDamageToShieldAbility _ability;
            private readonly IDamageableSystem _damageableSystem;
            private readonly IBodySystem _bodySystem;
            private readonly IStatusSystem _statusSystem;
            private readonly IGameplayEffectSystem _gameplayEffectSystem;

            private readonly Status _shieldStatus;

            public Spec(Ability ability, IAbilitySystem abilitySystem, int level) : base(ability, abilitySystem, level)
            {
                _ability = ability as ApplyDamageToShieldAbility;

                _statusSystem = gameObject.GetStatusSystem();
                _bodySystem = gameObject.GetBodySystem();
                _gameplayEffectSystem = gameObject.GetGameplayEffectSystem();

                _shieldStatus = _statusSystem.GetOrCreateStatus(_ability._shieldStatus);

                _damageableSystem = gameObject.GetDamageableSystem();
            }

            protected override void EnterAbility()
            {
                base.EnterAbility();

                _damageableSystem.OnTakeAnyDamage += _damageableSystem_OnTakeAnyDamage;
            }

            protected override void ExitAbility()
            {
                base.ExitAbility();

                _damageableSystem.OnTakeAnyDamage -= _damageableSystem_OnTakeAnyDamage;
            }

            private void _damageableSystem_OnTakeAnyDamage(IDamageableSystem damageable, DamageInfoData damageInfo)
            {
                if (!IsPlaying)
                    return;

                _shieldStatus.SubtractCurrentValue(damageInfo.Damage);

                if (_shieldStatus.CurrentState == EStatusState.Emptied)
                {
                    var point = transform;

                    if (_bodySystem.TryGetBodyPart(_ability._brokenShieldPoint, out IBodyPart bodypart))
                    {
                        point = bodypart.transform;
                    }

                    _ability._brokenShieldCue.PlayAttached(point);


                    foreach (var gameplayEffect in _ability._gameplayEffects)
                    {
                        _gameplayEffectSystem.TryApplyGameplayEffect(gameplayEffect, damageInfo.Instigator, Level);
                    }
                }
            }
        }
    }
}
