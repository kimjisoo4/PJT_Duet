using PF.PJT.Duet.Assets._Project.Characters.Skills.HammerUpperSwing;
using StudioScor.AbilitySystem;
using System;
using Unity.Behavior;
using Unity.Properties;
using UnityEngine;
using Action = Unity.Behavior.Action;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "Try Hammer Upper Swing Ability", story: "[Pawn] Try Activate [HammerUpperSwing] On [Target]", category: "Action", id: "7a9bfdff1dd1343577d4f3d4e9d5f6d8")]
public partial class TryHammerUpperSwingAbilityAction : Action
{
    [SerializeReference] public BlackboardVariable<GameObject> Pawn;
    [SerializeReference] public BlackboardVariable<GameObject> Target;
    [SerializeReference] public BlackboardVariable<HammerUpperSwingSkill> HammerUpperSwing;


    private IAbilitySystem _abilitySystem;
    private HammerUpperSwingSkill.Spec _upperSwingSpec;

    private Status _result;

    protected override Status OnStart()
    {
        var pawn = Pawn.Value;

        if (!pawn)
            return Status.Failure;

        _abilitySystem = pawn.GetAbilitySystem();

        if (!_abilitySystem.TryGetAbilitySpec(HammerUpperSwing.Value, out IAbilitySpec spec))
            return Status.Failure;

        _upperSwingSpec = spec as HammerUpperSwingSkill.Spec;

        if (!_upperSwingSpec.CanActiveAbility())
            return Status.Failure;

        _upperSwingSpec.SetAttackTarget(Target.Value);
        _upperSwingSpec.OnFinishedAbility += _upperSwingSpec_OnFinishedAbility;
        _upperSwingSpec.OnCanceledAbility += _upperSwingSpec_OnCanceledAbility;

        _upperSwingSpec.ForceActiveAbility();
        _result = Status.Running;

        return _result;
    }
    protected override void OnEnd()
    {
        if (_upperSwingSpec is not null)
        {
            _upperSwingSpec.OnFinishedAbility -= _upperSwingSpec_OnFinishedAbility;
            _upperSwingSpec.OnCanceledAbility -= _upperSwingSpec_OnCanceledAbility;
        }
    }
    protected override Status OnUpdate()
    {
        return _result;
    }

    private void _upperSwingSpec_OnCanceledAbility(IAbilitySpec abilitySpec)
    {
        _result = Status.Failure;

        if (_upperSwingSpec is not null)
        {
            _upperSwingSpec.OnFinishedAbility -= _upperSwingSpec_OnFinishedAbility;
            _upperSwingSpec.OnCanceledAbility -= _upperSwingSpec_OnCanceledAbility;
        }
    }

    private void _upperSwingSpec_OnFinishedAbility(IAbilitySpec abilitySpec)
    {
        _result = Status.Success;

        if (_upperSwingSpec is not null)
        {
            _upperSwingSpec.OnFinishedAbility -= _upperSwingSpec_OnFinishedAbility;
            _upperSwingSpec.OnCanceledAbility -= _upperSwingSpec_OnCanceledAbility;
        }
    }
}

