using StudioScor.Utilities;
using UnityEngine;

namespace PF.PJT.Duet.GameFlowSystem
{
    [AddComponentMenu("Duet/GameFlow/State/Wait For Scene Loaded State")]
    public class GameFlowWaitForSceneLoaded : GameFlowState
    {
        [Header(" [ Wait For Scene Loaded State ] ")]
        [SerializeField] private ScriptableSceneLoadSystem _sceneLoadSystem;

        public override bool CanEnterState()
        {
            return base.CanEnterState() && _sceneLoadSystem.IsLoading;
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            _sceneLoadSystem.OnLoadingStateChanged -= SceneLoadSystem_OnLoadingStateChanged;
        }
        protected override void EnterState()
        {
            base.EnterState();

            _sceneLoadSystem.OnLoadingStateChanged += SceneLoadSystem_OnLoadingStateChanged;
        }
        protected override void ExitState()
        {
            base.ExitState();

            _sceneLoadSystem.OnLoadingStateChanged -= SceneLoadSystem_OnLoadingStateChanged;
        }

        private void SceneLoadSystem_OnLoadingStateChanged(ScriptableSceneLoadSystem sceneLoadSystem, bool state)
        {
            ComplateState();
        }
    }
}
