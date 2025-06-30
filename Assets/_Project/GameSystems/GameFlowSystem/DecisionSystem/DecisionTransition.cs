using StudioScor.Utilities;
using UnityEngine;

namespace PF.PJT.Duet.GameFlowSystem
{
    [System.Serializable]
    public class DecisionTransition<TDecision,TValue> where TDecision : BaseDecision
    {
        public enum EOperator
        {
            And,
            Or,
        }

        [SerializeField] private TDecision[] _decisions;
        [SerializeField] private EOperator _operator;
        [SerializeField] private TValue _success;
        [SerializeField] private TValue _fail;

        public EOperator Operator => _operator;
        public TValue Success => _success;
        public TValue Fail => _fail;
        public TValue GetResult(object owner)
        {
            return CheckTransition(owner) ? _success : _fail;
        }

        public bool CheckTransition(object owner)
        {
            switch (_operator)
            {
                case EOperator.And:
                    return CheckAndDecision(owner);
                case EOperator.Or:
                    return CheckOrDesition(owner);
                default:
                    return false;
            }
        }

        private bool CheckOrDesition(object owner)
        {
            if (_decisions.IsNullOrEmpty())
                return true;

            for (int i = 0; i < _decisions.Length; i++)
            {
                var decision = _decisions[i];

                if (decision.Decide(owner))
                {
                    return true;
                }
            }

            return false;
        }
        private bool CheckAndDecision(object owner)
        {
            if (_decisions.IsNullOrEmpty())
                return true;

            for (int i = 0; i < _decisions.Length; i++)
            {
                var decision = _decisions[i];

                if (!decision.Decide(owner))
                {
                    return false;
                }
            }

            return true;
        }
    }
}
