using StudioScor.Utilities;
using StudioScor.Utilities.FadeSystem;
using System.Collections;
using UnityEngine;

namespace PF.PJT.Duet.GameFlowSystem
{
    [AddComponentMenu("Duet/GameFlow/State/Load Target Scene State")]
    public class GameFlowLoadTargetSceneState : GameFlowState
    {
        [Header(" [ Load Target Scene State ] ")]
        [SerializeField] private ScriptableFadeSystem _fadeSystem;
        [SerializeField] private ScriptableSceneLoadSystem _sceneLoadSystem;
        [SerializeField] private SceneReferenceSO _loadingScene;
        [SerializeField] private SceneReferenceSO _targetScene;
        [SerializeField] private float _minLoadingTime = 0.5f;

        protected override void OnDestroy()
        {
            base.OnDestroy();

            _fadeSystem.OnFadeOutFinished -= FadeSystem_OnFadeOutFinished;
        }

        protected override void EnterState()
        {
            base.EnterState();

            DontDestroyOnLoad(GameFlowSystem.gameObject);

            _fadeSystem.OnFadeOutFinished += FadeSystem_OnFadeOutFinished;

            _fadeSystem.StartFadeOut(1f);
            
        }
        private IEnumerator LoadTargetScene()
        {
            _sceneLoadSystem.LoadLoadingScene(_loadingScene);
            yield return new WaitWhile(() => _sceneLoadSystem.IsInTransition);

            yield return new WaitForSeconds(0.5f);

            float time = Time.time;
            _sceneLoadSystem.LoadSceneAsync(_targetScene, UnityEngine.SceneManagement.LoadSceneMode.Additive);
            yield return new WaitWhile(() => _sceneLoadSystem.IsInTransition);

            time -= Time.time;
            time -= _minLoadingTime;

            if(time < 0f)
            {
                yield return new WaitForSeconds(time);
            }
            else
            {
                yield return null;
            }

            _sceneLoadSystem.UnloadLoadingScene();
            yield return new WaitWhile(() => _sceneLoadSystem.IsInTransition);

            _fadeSystem.StartFadeIn(1f);

            Destroy(GameFlowSystem.gameObject);
        }


        private void FadeSystem_OnFadeOutFinished(IFadeSystem fadeSystem)
        {
            _fadeSystem.OnFadeOutFinished -= FadeSystem_OnFadeOutFinished;

            StartCoroutine(LoadTargetScene());
        }
    }
}
