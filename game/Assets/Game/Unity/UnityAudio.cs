using System.Threading.Tasks;
using Game.Core.Engine;
using UnityEngine;

namespace Game.Unity
{
    /// <summary>
    /// Joue les sons demandés par le jeu. Les sons sont synthétisés au démarrage (SoundBank) ;
    /// les deux boucles longues sont calculées en arrière-plan pour ne pas retarder l'affichage.
    /// </summary>
    public sealed class UnityAudio
    {
        private const int PoolSize = 10;

        private readonly AudioClip[] _clips = new AudioClip[(int)Sfx.Count];
        private readonly AudioSource[] _pool = new AudioSource[PoolSize];
        private readonly AudioSource _drill, _music;
        private Task<float[]> _drillTask, _musicTask;
        private int _next;

        public UnityAudio(GameObject parent)
        {
            for (int i = 0; i < (int)Sfx.Count; i++) _clips[i] = MakeClip(((Sfx)i).ToString(), SoundBank.Build((Sfx)i));
            for (int i = 0; i < PoolSize; i++) _pool[i] = MakeSource(parent, false);
            _drill = MakeSource(parent, true);
            _music = MakeSource(parent, true);
            _drillTask = Task.Run(() => SoundBank.BuildDrillLoop());
            _musicTask = Task.Run(() => SoundBank.BuildMusicLoop());
        }

        private static AudioClip MakeClip(string name, float[] samples)
        {
            AudioClip clip = AudioClip.Create(name, samples.Length, 1, SoundBank.Rate, false);
            clip.SetData(samples, 0);
            return clip;
        }

        private static AudioSource MakeSource(GameObject parent, bool loop)
        {
            AudioSource s = parent.AddComponent<AudioSource>();
            s.playOnAwake = false;
            s.loop = loop;
            s.spatialBlend = 0f;
            s.volume = loop ? 0f : 1f;
            return s;
        }

        public void Flush(AudioBus bus)
        {
            if (_drillTask != null && _drillTask.IsCompleted)
            {
                if (!_drillTask.IsFaulted) _drill.clip = MakeClip("DrillLoop", _drillTask.Result);
                _drillTask = null;
            }
            if (_musicTask != null && _musicTask.IsCompleted)
            {
                if (!_musicTask.IsFaulted) _music.clip = MakeClip("MusicLoop", _musicTask.Result);
                _musicTask = null;
            }

            for (int i = 0; i < bus.Shots.Count; i++)
            {
                AudioBus.Shot shot = bus.Shots[i];
                AudioSource src = _pool[_next];
                _next = (_next + 1) % PoolSize;
                src.pitch = Mathf.Clamp(shot.Pitch, 0.3f, 3f);
                src.PlayOneShot(_clips[(int)shot.Id], Mathf.Clamp01(shot.Vol));
            }
            bus.Shots.Clear();

            UpdateLoop(_drill, bus.SfxOn ? bus.DrillVol : 0f, bus.DrillPitch);
            UpdateLoop(_music, bus.MusicOn ? bus.MusicVol : 0f, 1f);
        }

        private static void UpdateLoop(AudioSource src, float volume, float pitch)
        {
            if (src.clip == null) return;
            src.volume = Mathf.Clamp01(volume);
            src.pitch = Mathf.Clamp(pitch, 0.3f, 3f);
            bool audible = volume > 0.004f;
            if (audible && !src.isPlaying) src.Play();
            else if (!audible && src.isPlaying) src.Pause();
        }
    }
}
