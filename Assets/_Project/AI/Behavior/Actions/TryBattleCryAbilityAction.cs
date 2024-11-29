using StudioScor.AbilitySystem;
using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;
using PF.PJT.Duet.Pawn.PawnAbility;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "Try Battle Cry Ability", story: "[Pawn] Try [BattleCry] Ability", category: "Action", id: "580ace0f9454a0ce779a954dae687d23")]
public partial class TryBattleCryAbilityAction : Action
{
    [SerializeReference] public BlackboardVariable<GameObject> Pawn;
    [SerializeReference] public BlackboardVariable<Ability> BattleCry;
    [SerializeReference] public BlackboardVariable<bool> SkipAnimation;

    private IAbilitySystem _abilitySystem;
    private CreateSuperArmorShieldSkill.Spec _battleCrySpec;

    private Status _result;

    protected override Status OnStart()
    {
        var pawn = Pawn.Value;

        if (!pawn)
            return Status.Failure;

        _abilitySystem = pawn.GetAbilitySystem();

        if (!_abilitySystem.TryGetAbilitySpec(BattleCry.Value, out IAbilitySpec spec))
            return Status.Failure;

        _battleCrySpec = spec as CreateSuperArmorShieldSkill.Spec;

        if (!_battleCrySpec.CanActiveAbility())
            return Status.Failure;

        if (SkipAnimation.Value)
        {
            _battleCrySpec.SkipAnimation = true;
            _battleCrySpec.ForceActiveAbility();
            _result = Status.Success;

            return Status.Success;
        }
        else
        {
            _battleCrySpec.SkipAnimation = false;

            _battleCrySpec.OnEndedAbility += _battleCrySpec_OnFinishedAbility;
            _battleCrySpec.OnCanceledAbility += _battleCrySpec_OnCanceledAbility;

            _battleCrySpec.ForceActiveAbility();

            _result = Status.Running;

            return Status.Running;
        }
    }
    protected override void OnEnd()
    {
        if (_battleCrySpec is not null)
        {
            _battleCrySpec.OnFinishedAbility -= _battleCrySpec_OnFinishedAbility;
            _battleCrySpec.OnCanceledAbility -= _battleCrySpec_OnCanceledAbility;
        }
    }
    protected override Status OnUpdate()
    {
        return _result;
    }

    private void _battleCrySpec_OnCanceledAbility(IAbilitySpec abilitySpec)
    {
        _result = Status.Failure;

        if (_battleCrySpec is not null)
        {
            _battleCrySpec.OnEndedAbility -= _battleCrySpec_OnFinishedAbility;
            _battleCrySpec.OnCanceledAbility -= _battleCrySpec_OnCanceledAbility;
        }
    }

    private void _battleCrySpec_OnFinishedAbility(IAbilitySpec abilitySpec)
    {
        _result = Status.Success;

        if(_battleCrySpec is not null)
        {

            _battleCrySpec.OnEndedAbility -= _battleCrySpec_OnFinishedAbility;
            _battleCrySpec.OnCanceledAbility -= _battleCrySpec_OnCanceledAbility;
        }
    }
    
}

