using StudioScor.Utilities;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

namespace PF.PJT.Duet.Pawn
{
    public class SoundManager : Singleton<SoundManager>
    {
        [Serializable]
        public class SoundSourcePool
        {
            [SerializeField] private AudioSource _defaultSource;
            [SerializeField] private int _startCapacity = 5;

            private readonly List<AudioSource> _pool = new();
            private GameObject _owner;
            private int _lastIndex = -1;

            public void Setup(GameObject owner)
            {
                _owner = owner;

                CreateSources(_startCapacity);
            }

            private AudioSource CreateSource()
            {
                var newSource = Instantiate(_defaultSource, _owner.transform);

                _pool.Add(newSource);

                return newSource;
            }
            private void CreateSources(int count)
            {
                for(int i = 0; i < count; i++)
                {
                    CreateSource();
                }
            }

            public AudioSource Get()
            {
                int poolCount = _pool.Count;
                int startIndex = (_lastIndex + 1) % poolCount;

                for(int i = 0; i < poolCount; i++)
                {
                    int index = (startIndex + i) % poolCount;
                    var source = _pool[index];

                    if (!source.gameObject.activeSelf)
                    {
                        _lastIndex = index;

                        return source;
                    }
                }

                _lastIndex = poolCount;
                
                return CreateSource();
            }
        }

        [Header(" [ Sound Manager ] ")]
        [SerializeField] private AudioMixer _audioMixer;

        [SerializeField] private SoundSourcePool _sfxPoolUI;
        [SerializeField] private SoundSourcePool _sfxPool2D;
        [SerializeField] private SoundSourcePool _sfxPool3D;

        private const string PARAM_VOLUME_MASTER = "Volume_Master";
        private const string PARAM_VOLUME_BGM = "Volume_BGM";
        private const string PARAM_VOLUME_SFX = "Volume_SFX";
        private const string PARAM_VOLUME_UI = "Volume_UI";

        private const float DB_MULTIPLY = 20;

        private void Awake()
        {
            _sfxPoolUI.Setup(gameObject);
            _sfxPool2D.Setup(gameObject);
            _sfxPool3D.Setup(gameObject);
        }
        private float CalcDB(float value)
        {
            return Mathf.Log10(value) * DB_MULTIPLY;
        }

        public void SetMasterVolume(float normalizedVolume)
        {
            _audioMixer.SetFloat(PARAM_VOLUME_MASTER, CalcDB(normalizedVolume));
        }
        public void SetBGMVolume(float normalizedVolume)
        {
            _audioMixer.SetFloat(PARAM_VOLUME_BGM, CalcDB(normalizedVolume));
        }
        public void SetUIVolume(float normalizedVolume)
        {
            _audioMixer.SetFloat(PARAM_VOLUME_UI, CalcDB(normalizedVolume));
        }
        public void SetSFXVoluem(float normalizedVolume)
        {
            _audioMixer.SetFloat(PARAM_VOLUME_SFX, CalcDB(normalizedVolume));
        }

        public AudioSource GetUISource() => _sfxPoolUI.Get();
        public AudioSource GetSFX2DSource() => _sfxPool2D.Get();
        public AudioSource GetSFX3DSource() => _sfxPool3D.Get();

        public AudioSource PlaySFXUI(AudioClip clip)
        {
            var source = _sfxPoolUI.Get();

            source.clip = clip;
            source.Play();

            return source;
        }

        public AudioSource PlaySFX2D(AudioClip clip)
        {
            var source = _sfxPool2D.Get();

            source.clip = clip;
            source.Play();

            return source;
        }

        public AudioSource PlaySFX3D(AudioClip clip, Vector3 position)
        {
            var source = _sfxPool3D.Get();

            source.clip = clip;
            source.transform.position = position;

            source.Play();

            return source;
        }
    }
}
