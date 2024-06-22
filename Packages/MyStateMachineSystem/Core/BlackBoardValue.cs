using StudioScor.Utilities;

namespace StudioScor.StateMachine
{
    public class BlackBoardValue<T> : BaseClass
    {
        private T _Value;
        private bool _HasValue;
        public bool HasValue => _HasValue;


        public void SetValue(T value)
        {
            _Value = value;

            _HasValue = true;
        }
        public bool TryGetValue(out T value)
        {
            value = _Value;

            return _HasValue;
        }

        public void Clear()
        {
            _Value = default;
            _HasValue = false;
        }
    }
}