using StudioScor.Utilities;
using UnityEngine;
using UnityEngine.Playables;

namespace PF.PJT.Duet
{
    public interface ITimelinePlayer
    {
        public delegate void TimelineStateHandler(ITimelinePlayer timelinePlayer);

        public GameObject gameObject { get; }
        public bool IsPlaying { get; }
        public PlayableDirector PlayableDirector { get; }
        public void Play();
        public void Stop();

        public event TimelineStateHandler OnBeforePlayed;
        public event TimelineStateHandler OnAfterPlayed;
        public event TimelineStateHandler OnStopped;
    }

    public class TimelinePlayer : BaseMonoBehaviour, ITimelinePlayer
    {
        [Header(" [ Timeline Player System ] ")]
        [SerializeField] private PlayableDirector _playableDirector;

        private bool _isPlaying = false;
        public PlayableDirector PlayableDirector => _playableDirector;
        public bool IsPlaying => _isPlaying;

        public event ITimelinePlayer.TimelineStateHandler OnBeforePlayed;
        public event ITimelinePlayer.TimelineStateHandler OnAfterPlayed;
        public event ITimelinePlayer.TimelineStateHandler OnStopped;

        protected virtual void OnValidate()
        {
#if UNITY_EDITOR
            if(!_playableDirector)
            {
                _playableDirector = gameObject.GetComponentInParentOrChildren<PlayableDirector>();
            }
#endif
        }
        protected virtual void Awake() { }
        protected virtual void OnDestroy()
        {
            _isPlaying = false;

            if (_playableDirector)
            {
                _playableDirector.stopped -= PlayableDirector_stopped;
            }

            OnBeforePlayed = null;
            OnAfterPlayed = null;
            OnStopped = null;
        }
        

        public void Play()
        {
            if (_isPlaying)
                return;

            _isPlaying = true;
            _playableDirector.stopped += PlayableDirector_stopped;

            OnBeforePlay();
            RaiseOnBeforePlayed();

            _playableDirector.Play();

            OnAfterPlay();
            RaiseOnAfterPlayed();
        }

        public void Stop()
        {
            if (!_isPlaying)
                return;


            _playableDirector.stopped -= PlayableDirector_stopped;
            _isPlaying = false;

            _playableDirector.Stop();

            OnStop();
            RaiseOnStopped();
        }
        protected virtual void OnBeforePlay() { }
        protected virtual void OnAfterPlay() { }
        protected virtual void OnStop() { }

        private void PlayableDirector_stopped(PlayableDirector playableDirector)
        {
            if (!_isPlaying)
                return;

            Stop();
        }

        protected void RaiseOnBeforePlayed()
        {
            Log($"{nameof(OnBeforePlayed)}");

            OnBeforePlayed?.Invoke(this);
        }
        protected void RaiseOnAfterPlayed()
        {
            Log($"{nameof(OnAfterPlayed)}");

            OnAfterPlayed?.Invoke(this);
        }
        protected void RaiseOnStopped()
        {
            Log($"{nameof(OnStopped)}");

            OnStopped?.Invoke(this);
        }
    }
}
