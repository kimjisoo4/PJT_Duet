using StudioScor.AbilitySystem;
using StudioScor.Utilities;
using UnityEngine;

namespace PF.PJT.Duet.Pawn.PawnAbility
{

    [CreateAssetMenu(menuName = "Project/Duet/PawnAbility/Effect/new Dissolve Ability", fileName = "GA_PawnAbiltiy_Dissolve")]
    public class DissolveAbility : GASAbility
    {
        [Header(" [ Dissolve Ability ] ")]
        [SerializeField] private float _dissolveTime = 1f;

        public override IAbilitySpec CreateSpec(IAbilitySystem abilitySystem, int level = 0)
        {
            return new Spec(this, abilitySystem, level);
        }

        public class Spec : GASAbilitySpec, IUpdateableAbilitySpec
        {
            protected new readonly DissolveAbility _ability;
            private readonly ICharacter _character;

            private const string DISSOLVE_ID = "_Dissolve";
            private const string MIN_MAX_Y_ID = "_MinMaxY";

            private readonly Timer _timer = new();
            private readonly int _dissolveID;
            private readonly int _minMaxYID;

            public Spec(Ability ability, IAbilitySystem abilitySystem, int level) : base(ability, abilitySystem, level)
            {
                _ability = ability as DissolveAbility;

                _character = gameObject.GetComponent<ICharacter>();

                _dissolveID = Shader.PropertyToID(DISSOLVE_ID);
                _minMaxYID = Shader.PropertyToID(MIN_MAX_Y_ID);
            }
            protected override void OnGrantAbility()
            {
                base.OnGrantAbility();

                ForceActiveAbility();
            }
            protected override void EnterAbility()
            {
                base.EnterAbility();

                var renderers = _character.Model.GetComponentsInChildren<Renderer>();
                float height = 0f;

                foreach (var renderer in renderers)
                {
                    renderer.ResetBounds();

                    if (height < renderer.bounds.max.y)
                    {
                        height = renderer.bounds.max.y;
                    }
                }

                foreach (var material in _character.Materials)
                {
                    material.SetVector(_minMaxYID, new Vector2(transform.position.y, height));
                }

                _timer.OnTimer(_ability._dissolveTime);
            }

            protected override void ExitAbility()
            {
                base.ExitAbility();

                foreach (var material in _character.Materials)
                {
                    material.SetFloat(_dissolveID, 0f);
                }
            }
            public void UpdateAbility(float deltaTime)
            {
                if (!IsPlaying)
                    return;

                UpdateDissolve(deltaTime);
            }

            public void FixedUpdateAbility(float deltaTime)
            {
                return;
            }

            private void UpdateDissolve(float deltaTime)
            {
                if (!_timer.IsPlaying)
                    return;

                _timer.UpdateTimer(deltaTime);

                foreach (var material in _character.Materials)
                {
                    material.SetFloat(_dissolveID, _timer.NormalizedTime);
                }

                if (_timer.IsFinished)
                {
                    _character.OnDispawn();
                }

            }
        }
    }
}
