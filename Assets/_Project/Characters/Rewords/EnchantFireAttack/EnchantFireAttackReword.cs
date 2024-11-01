using PF.PJT.Duet.Pawn.Effect;
using StudioScor.AbilitySystem;
using StudioScor.GameplayCueSystem;
using StudioScor.GameplayEffectSystem;
using StudioScor.GameplayTagSystem;
using UnityEngine;

namespace PF.PJT.Duet.Pawn.PawnSkill
{
    [CreateAssetMenu(menuName = "Project/Duet/Reword Ability/new Enchant Fire Attack Reword", fileName = "GA_Reword_Enchant_")]
    public class EnchantFireAttackReword : RewordAbility
    {
        [Header(" [ Enchant Fire Attack Skill ] ")]
        [SerializeField] private GameplayTagSO _onAttackHitTag;

        [Header(" Gameplay Effect ")]
        [SerializeField] private TakeDamageEffect _takeDamageEffect;

        [Header(" Gameplay Cue ")]
        [SerializeField] private FGameplayCue _onHitCue;

        public override string Description
        {
            get
            {
                if (!_takeDamageEffect)
                    return base.Description;

                var stat = _takeDamageEffect.BaseStat as IDisplayName;
                var damageType = _takeDamageEffect.DamageType as IDisplayName;
                var damage = _takeDamageEffect as IDisplayDamage;

                return string.Format(base.Description, stat.Name, damage.Damage, damageType.Name);
            }
        }

        public override IAbilitySpec CreateSpec(IAbilitySystem abilitySystem, int level = 0)
        {
            return new Spec(this, abilitySystem, level);
        }

        public class Spec : GASAbilitySpec
        {
            protected new readonly EnchantFireAttackReword _ability;

            public Spec(Ability ability, IAbilitySystem abilitySystem, int level) : base(ability, abilitySystem, level)
            {
                _ability = ability as EnchantFireAttackReword;
            }

            protected override void OnGrantAbility()
            {
                base.OnGrantAbility();

                ForceActiveAbility();
            }

            protected override void EnterAbility()
            {
                base.EnterAbility();

                GameplayTagSystem.OnTriggeredTag += GameplayTagSystem_OnTriggeredTag;
            }
            protected override void ExitAbility()
            {
                base.ExitAbility();

                GameplayTagSystem.OnTriggeredTag -= GameplayTagSystem_OnTriggeredTag;
            }

            private void ActiveFire(OnAttackHitData hitData)
            {
                Log($"{nameof(ActiveFire)} - {hitData}");

                var actor = hitData.Hitter;

                if (actor.TryGetGameplayEffectSystem(out IGameplayEffectSystem gameplayEffectSystem))
                {
                    if(_ability._takeDamageEffect)
                    {
                        var element = TakeDamageEffect.Element.Get(hitData.HitPoint, hitData.HitNormal, hitData.HitCollider, hitData.AttackDirection, gameObject, gameObject);
                        
                        if(gameplayEffectSystem.TryApplyGameplayEffect(_ability._takeDamageEffect, gameObject, Level, element))
                        {
                            Vector3 position = hitData.HitPoint + actor.transform.TransformDirection(_ability._onHitCue.Position);
                            Vector3 eulerAngles = Quaternion.LookRotation(hitData.AttackDirection, Vector3.up) * _ability._onHitCue.Rotation;
                            Vector3 scale = _ability._onHitCue.Scale;

                            _ability._onHitCue.Cue.Play(position, eulerAngles, scale, _ability._onHitCue.Volume);
                        }

                        element.Release();
                    }
                }
            }

            private void GameplayTagSystem_OnTriggeredTag(IGameplayTagSystem gameplayTagSystem, IGameplayTag gameplayTag, object data = null)
            {
                if (!IsPlaying)
                    return;

                if (_ability._onAttackHitTag != gameplayTag)
                    return;

                if (data is not OnAttackHitData hitData)
                    return;

                ActiveFire(hitData);
            }
        }
    }
}
