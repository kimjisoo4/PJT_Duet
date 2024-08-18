using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace StudioScor.Utilities
{

    public abstract class ListVariableObject<T> : EmptyVariable, ISerializationCallbackReceiver
	{
		#region Events
		public delegate void ListMemberEventHandler(ListVariableObject<T> variable, T value);
		#endregion

		[SerializeField] protected List<T> _InitialValues;
		[SerializeField][SReadOnly] protected List<T> _RuntimeValues;

		public IReadOnlyList<T> InitialValues => _InitialValues;
		public IReadOnlyList<T> Values => _RuntimeValues;


		public event ListMemberEventHandler OnAdded;
		public event ListMemberEventHandler OnRemoved;

		public void OnBeforeSerialize()
		{
		}

		public void OnAfterDeserialize()
		{
			OnReset();
		}

		protected override void OnReset()
        {
			_RuntimeValues = _InitialValues.ToList();

			OnAdded = null;
			OnRemoved = null;
		}

		public void Clear()
        {
			_RuntimeValues.Clear();
		}

        public virtual void Add(T member)
        {
			if (_RuntimeValues.Contains(member))
				return;

			_RuntimeValues.Add(member);

			Invoke_OnAdded(member);
		}
		public virtual void Remove(T member)
        {
            if (_RuntimeValues.Remove(member))
            {
				Invoke_OnRemoved(member);
            }
        }
		public void RemoveAt(int index)
        {
			if (_RuntimeValues.Count <= index)
				return;

			var member = _RuntimeValues[index];

			_RuntimeValues.RemoveAt(index);

			Invoke_OnRemoved(member);
        }

        protected void Invoke_OnAdded(T addMember)
        {
			Log($"{nameof(OnAdded)} - {addMember} ");

			OnAdded?.Invoke(this, addMember);
		}
		protected void Invoke_OnRemoved(T removeMember)
        {
			Log($"{nameof(OnRemoved)} - {removeMember}");

			OnRemoved?.Invoke(this, removeMember);
		}
    }

}
