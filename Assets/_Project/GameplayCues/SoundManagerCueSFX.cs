using PF.PJT.Duet.Pawn;
using StudioScor.GameplayCueSystem;
using UnityEngine;

namespace PF.PJT.Duet
{
    [CreateAssetMenu(menuName = "StudioScor/GameplayCue/CueFX/new Sound Manager Cue SFX", fileName = "SFX_")]
    public class SoundManagerCueSFX : CueFX
    {
        public enum EAudioType
        {
            UI,
            SFX2D,
            SFX3D,
        }

        [Header(" [ Sound Manager Cue SFX ] ")]
        [SerializeField] private AudioClip[] _clips;
        [SerializeField] private EAudioType _audioType = EAudioType.SFX3D;
        [SerializeField] private bool _isLooping = false;

        public override ICueActor GetCueActor()
        {
            AudioSource audioSource;
            var soundManager = SoundManager.Instance;

            switch (_audioType)
            {
                case EAudioType.UI:
                    audioSource = soundManager.GetUISource();
                    break;
                case EAudioType.SFX2D:
                    audioSource = soundManager.GetSFX2DSource();
                    break;
                case EAudioType.SFX3D:
                    audioSource = soundManager.GetSFX3DSource();
                    break;
                default:
                    audioSource = null;
                    break;
            }
            AudioClip clip;

            if (_clips.Length > 1)
                clip = _clips[Random.Range(0, _clips.Length)];
            else
                clip = _clips[0];

            audioSource.clip = clip;
            audioSource.loop = _isLooping;

            var cueActor = audioSource.GetComponent<ICueActor>();

            return cueActor;
        }
    }
}