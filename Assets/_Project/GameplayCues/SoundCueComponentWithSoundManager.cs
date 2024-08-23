using PF.PJT.Duet.Pawn;
using StudioScor.GameplayCueSystem;
using StudioScor.Utilities;
using UnityEngine;

namespace PF.PJT.Duet
{

    public class SoundCueComponentWithSoundManager : GameplayCueComponent
    {
        [Header(" [ Sound Cue Component With Sound Manager ] ")]
        [SerializeField] private AudioSource audioSource;
        [SerializeField] private float _randPitch = 10f;
        private void OnDisable()
        {
            Finish();
        }

        public override void Play()
        {
            if (Cue.AttachTarget)
                transform.SetParent(Cue.AttachTarget);

            if (Cue.UseStayWorldPosition)
            {
                transform.SetPositionAndRotation(Position, Rotation);
            }
            else
            {
                transform.SetLocalPositionAndRotation(Position, Rotation);
            }

            audioSource.volume = Volume;

            audioSource.pitch = 1f + Random.Range(-_randPitch / 100, _randPitch / 100);

            audioSource.Play();
        }
        
        public override void Stop()
        {
            if (gameObject.scene.isLoaded && transform.parent && transform.parent != SoundManager.Instance.transform)
                transform.SetParent(SoundManager.Instance.transform);

            audioSource.Stop();
        }
        public override void Pause()
        {
            audioSource.Pause();
        }

        public override void Resume()
        {
            audioSource.UnPause();
        }

        private void Update()
        {
            if (!audioSource.isPlaying && (Cue is null || !Cue.IsPaused))
            {
                if (transform.parent && transform.parent != SoundManager.Instance.transform)
                    transform.SetParent(SoundManager.Instance.transform);

                gameObject.SetActive(false);
            }
        }
    }
}