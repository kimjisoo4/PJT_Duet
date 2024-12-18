﻿using PF.PJT.Duet.Pawn.PawnSkill;
using StudioScor.AbilitySystem;
using StudioScor.Utilities;
using UnityEngine;
using UnityEngine.UI;

namespace PF.PJT.Duet
{

    public class InputSkillSlotUI : BaseMonoBehaviour
    {
        [Header(" [ Input Skill SlotUI ] ")]
        [SerializeField] private Image _icon;
        [SerializeField] private SimpleAmountComponent _amount;

        private ISkill _skill;
        private ISkillState _skillState;
        public void SetSkill(IAbility ability, IAbilitySpec abilitySpec)
        {
            _skill = ability as ISkill;
            _skillState = abilitySpec as ISkillState;

            if (_skill is not null)
            {
                _icon.sprite = _skill.Icon;
                _icon.color = Color.white;
            }
            else
            {
                _icon.color = Color.clear;
            }

            if(_skillState is not null)
            {
                Log($"Skill Name [{_skill}]  Remain Time - {_skillState.RemainCoolTime:f2} :: CoolTime - {_skillState.CoolTime:f2}");

                _amount.SetValue(_skillState.RemainCoolTime, _skillState.CoolTime);
            }
            else
            {
                _amount.SetValue(0f, 1f);
            }
        }

        private void LateUpdate()
        {
            if (_skillState is null)
                return;

            _amount.SetCurrentValue(_skillState.RemainCoolTime);
        }
    }
}
