using BeastKeeper.Core;
using UnityEngine;

namespace BeastKeeper.Systems
{
    /// <summary>
    /// Contract for playing global background music and sound effects.
    /// </summary>
    public interface IAudioSystem : IGameService
    {
        void PlayMusic(AudioClip clip, bool loop = true, float fadeDuration = 1f);
        void StopMusic(float fadeDuration = 1f);
        void PlaySfx(AudioClip clip, float volume = 1f);
        void SetMusicVolume(float volume);
        void SetSfxVolume(float volume);
    }
}
