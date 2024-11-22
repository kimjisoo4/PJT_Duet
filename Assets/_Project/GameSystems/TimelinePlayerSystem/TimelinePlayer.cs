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

        protected virtual void Awake()
        {
            _playableDirector.stopped += _playableDirector_stopped;
        }

        protected virtual void OnDestroy()
        {
            _isPlaying = false;

            if (_playableDirector)
            {
                _playableDirector.stopped -= _playableDirector_stopped;
            }
        }
        

        public void Play()
        {
            if (_isPlaying)
                return;

            _isPlaying = true;

            OnBeforePlay();
            Invoke_OnBeforePlayed();

            _playableDirector.Play();

            OnAfterPlay();
            Invoke_OnAfterPlayed();
        }

        public void Stop()
        {
            if (!_isPlaying)
                return;

            _isPlaying = false;

            _playableDirector.Stop();

            OnStop();
            Invoke_OnStopped();
        }
        protected virtual void OnBeforePlay() { }
        protected virtual void OnAfterPlay() { }
        protected virtual void OnStop() { }

        private void _playableDirector_stopped(PlayableDirector playableDirector)
        {
            if (!_isPlaying)
                return;

            Stop();
        }

        protected void Invoke_OnBeforePlayed()
        {
            Log($"{nameof(OnBeforePlayed)}");

            OnBeforePlayed?.Invoke(this);
        }
        protected void Invoke_OnAfterPlayed()
        {
            Log($"{nameof(OnAfterPlayed)}");

            OnAfterPlayed?.Invoke(this);
        }
        protected void Invoke_OnStopped()
        {
            Log($"{nameof(OnStopped)}");

            OnStopped?.Invoke(this);
        }
    }
}
