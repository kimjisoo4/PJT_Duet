﻿using StudioScor.Utilities;
using UnityEngine;


namespace PF.PJT.Duet
{
    public class RoomState : BaseStateMono
    {
        [Header(" [ Room State ] ")]
        [SerializeField] private RoomController _roomController;
        protected RoomController RoomController => _roomController;

        protected virtual void OnValidate()
        {
#if UNITY_EDITOR
            if(!_roomController)
            {
                _roomController = GetComponentInParent<RoomController>();
            }
#endif
        }
    }

}