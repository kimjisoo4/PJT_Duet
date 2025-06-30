using PF.PJT.Duet.Pawn.PawnSkill;
using StudioScor.AbilitySystem;
using StudioScor.Utilities;
using System;
using Unity.Behavior;
using Unity.Properties;
using UnityEngine;
using Action = Unity.Behavior.Action;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "Try Dodge Ability On Target", story: "[Pawn] Try [Dodge] Ability On [Target]", category: "Action", id: "ef76bd47856f7a904b21fa09fbb17097")]
public partial class TryDodgeAbilityOnTargetAction : Action
{
    [SerializeReference] public BlackboardVariable<GameObject> Pawn;
    [SerializeReference] public BlackboardVariable<DodgeSkill> Dodge;
    [SerializeReference] public BlackboardVariable<GameObject> Target;

    private IAbilitySystem _abilitySystem;
    private DodgeSkill.Spec _dodgeSkillSpec;

    private Status _result;

    protected override Status OnStart()
    {
        var pawn = Pawn.Value;

        if (!pawn)
            return Status.Failure;

        _abilitySystem = pawn.GetAbilitySystem();

        if (!_abilitySystem.TryGetAbilitySpec(Dodge.Value, out IAbilitySpec spec))
            return Status.Failure;

        _dodgeSkillSpec = spec as DodgeSkill.Spec;

        if (!_dodgeSkillSpec.CanActiveAbility())
            return Status.Failure;

        Vector3 direction = Target.Value.transform.HorizontalDirection(pawn.transform);

        _dodgeSkillSpec.SetDirection(direction);
        _dodgeSkillSpec.OnFinishedAbility += _dodgeSkillSpec_OnFinishedAbility;
        _dodgeSkillSpec.OnCanceledAbility += _dodgeSkillSpec_OnCanceledAbility;

        _dodgeSkillSpec.ForceActiveAbility();

        _result = Status.Running;

        return _result;
    }
    protected override void OnEnd()
    {
        if (_dodgeSkillSpec is not null)
        {
            _dodgeSkillSpec.OnFinishedAbility -= _dodgeSkillSpec_OnFinishedAbility;
            _dodgeSkillSpec.OnCanceledAbility -= _dodgeSkillSpec_OnCanceledAbility;
        }
    }
    protected override Status OnUpdate()
    {
        return _result;
    }

    private void _dodgeSkillSpec_OnCanceledAbility(IAbilitySpec abilitySpec)
    {
        _result = Status.Failure;

        if (_dodgeSkillSpec is not null)
        {
            _dodgeSkillSpec.OnFinishedAbility -= _dodgeSkillSpec_OnFinishedAbility;
            _dodgeSkillSpec.OnCanceledAbility -= _dodgeSkillSpec_OnCanceledAbility;
        }
    }

    private void _dodgeSkillSpec_OnFinishedAbility(IAbilitySpec abilitySpec)
    {
        _result = Status.Success;

        if (_dodgeSkillSpec is not null)
        {
            _dodgeSkillSpec.OnFinishedAbility -= _dodgeSkillSpec_OnFinishedAbility;
            _dodgeSkillSpec.OnCanceledAbility -= _dodgeSkillSpec_OnCanceledAbility;
        }
    }
}

