using StudioScor.Utilities;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace PF.PJT.Duet
{
    public class LoadStageScene : BaseMonoBehaviour
    {
        [Header(" [ Loading Scene State ] ")]
        [SerializeField] private FadeBase _fade;
        [SerializeField] private SceneLoader _loadingScene;
        [SerializeField] private SceneLoader _targetScene;

        public void LoadScene()
        {
            // Fade In
            // Scene 로드
            // Scene 로드 완료시
            // Loading Scene Unload

            StartCoroutine(LoadingSceneSeq());
        }

        private IEnumerator LoadingSceneSeq()
        {
            DontDestroyOnLoad(gameObject);

            _fade.FadeOut();

            yield return new WaitUntil(() => _fade.State == EFadeState.Fade);

            _loadingScene.LoadScene();

            yield return new WaitUntil(() => !_loadingScene.IsPlaying);

            _targetScene.LoadScene();
            SceneManager.sceneLoaded += SceneManager_sceneLoaded;

            yield return new WaitUntil(() => !_targetScene.IsPlaying);

            yield return new WaitForSeconds(1f);
            _loadingScene.UnLoadScene();
            _fade.FadeIn();

            yield return new WaitUntil(() => _fade.State == EFadeState.NotFade);


            Destroy(gameObject);
        }

        private void SceneManager_sceneLoaded(Scene arg0, LoadSceneMode arg1)
        {
           if(arg0.name == _targetScene.GetScene.name)
            {

                SceneManager.SetActiveScene(_targetScene.GetScene);
            }
        }
    }
}
