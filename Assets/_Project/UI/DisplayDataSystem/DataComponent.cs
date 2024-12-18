﻿using StudioScor.Utilities;
using System.Collections.Generic;
using UnityEngine;

namespace PF.PJT.Duet
{
    public interface IDataComponent
    {
        public GameObject gameObject { get; }
        public Transform transform { get; }
        public object Data { get; }
        public void UpdateData(object data);
    }

    public class DataComponent : BaseMonoBehaviour, IDataComponent
    {
        [Header(" [ Data Component ] ")]
        [SerializeField] private List<DataUpdater> _updaters;

        private object _data;
        public object Data => _data;

        [ContextMenu(nameof(GetChildComponents), false, 1000000)]
        private void GetChildComponents()
        {
            var updaters = GetComponentsInChildren<DataUpdater>(true);

            foreach (var updater in updaters)
            {
                if(!_updaters.Contains(updater))
                {
                    _updaters.Add(updater);
                }
            }
        }

        public void UpdateData(object data)
        {
            Log($"{nameof(UpdateData)} - {data}");

            _data = data;

            foreach (var modifier in _updaters)
            {
                modifier.UpdateData(_data);
            }
        }
    }
}
