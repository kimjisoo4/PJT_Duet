using StudioScor.Utilities;

namespace StudioScor.StateMachine
{
    public class BlackBoardValue<T> : BaseClass
    {
        private T _value;
        private bool _hasValue;
        public bool HasValue => _hasValue;


        public void SetValue(T value)
        {
            _value = value;

            _hasValue = true;
        }

        public T GetValue()
        {
            return _hasValue ? _value : default;
        }
        public bool TryGetValue(out T value)
        {
            value = _value;

            return _hasValue;
        }

        public void Clear()
        {
            _value = default;
            _hasValue = false;
        }
    }
}