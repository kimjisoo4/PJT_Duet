using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using StudioScor.Utilities;

namespace StudioScor.BodySystem
{
    public class TracePartComponent : BodyPartComponent
    {
        [Header("[ Trace Part Component ]")]
        [SerializeField] private float _Radius;
        [SerializeField] private LayerMask _LayerMask;

        [SerializeField] private List<BodyPoint> _BodyPoints;

        #region EDITOR ONLY

        [ContextMenu("New Create Point")]
        public void CreatePoint()
        {
#if UNITY_EDITOR
            var newPoint = new GameObject();

            newPoint.transform.SetParent(this.transform);

            if (_BodyPoints.Count > 0)
            {
                newPoint.transform.position = _BodyPoints.Last().Transform.position;
            }
            else
            {
                newPoint.transform.position = transform.position;
            }

            BodyPoint newbodyPoint = new BodyPoint(newPoint.transform);

            _BodyPoints.Add(newbodyPoint);

            newPoint.name = "Point [" + (_BodyPoints.Count - 1) + "]";
#endif
        }


        private void OnDrawGizmos()
        {
#if UNITY_EDITOR
            if (_BodyPoints is null)
                return;

            if (UseDebug)
            {
                Gizmos.color = Color.red;

                foreach (BodyPoint bodyPoint in _BodyPoints)
                {
                    if (bodyPoint.Transform)
                        Gizmos.DrawWireSphere(bodyPoint.Transform.position, _Radius);
                }
            }
#endif
        }
        private void OnDrawGizmosSelected()
        {
#if UNITY_EDITOR
            if (_BodyPoints is null)
                return;

            Gizmos.color = Color.red;

            foreach (BodyPoint bodyPoint in _BodyPoints)
            {
                if (bodyPoint.Transform)
                    Gizmos.DrawWireSphere(bodyPoint.Transform.position, _Radius);
            }
#endif
        }

    #endregion

        public void OnTrace()
        {
            foreach (BodyPoint bodyPoint in _BodyPoints)
            {
                bodyPoint.PrevPosition = bodyPoint.Transform.position;
            }

        }
        public void OffTrace()
        {

        }

        public bool UpdateTrace(out List<RaycastHit> hits)
        {
            hits = new List<RaycastHit>();

            foreach (BodyPoint bodyPoint in _BodyPoints)
            {
                Vector3 position = bodyPoint.Transform.position;
                Vector3 prevPosition = bodyPoint.PrevPosition;

                SUtility.Physics.DrawSphereCastAll(prevPosition, position, _Radius, _LayerMask, ref hits, null, UseDebug);

                bodyPoint.PrevPosition = position;
            }

            return hits.Count > 0;
        }
    }
}
