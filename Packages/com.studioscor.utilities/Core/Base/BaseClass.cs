﻿using UnityEngine;

namespace StudioScor.Utilities
{

    public abstract class BaseClass
    {
        [field: Header(" Use Debug ")]
        [field: SerializeField] public virtual bool UseDebug { get; set; } = false;
        [HideInInspector] public virtual Object Context { get; set; } = null;


        [System.Diagnostics.Conditional(SUtility.DEFINE_UNITY_EDITOR)]
        protected virtual void Log(object log, string color = SUtility.STRING_COLOR_GREY)
        {
#if UNITY_EDITOR
            if (UseDebug)
                SUtility.Debug.Log($"{GetType().Name} [ {(Context ? Context.name : "Empty")} ] : {log}", Context, color);
#endif
        }

        [System.Diagnostics.Conditional(SUtility.DEFINE_UNITY_EDITOR)]
        protected virtual void LogError(object log, string color = SUtility.STRING_COLOR_GREY)
        {
#if UNITY_EDITOR
            SUtility.Debug.LogError($"{GetType().Name} [ {(Context ? Context.name : "Empty")} ] : {log}", Context, color);
#endif
        }
    }
}