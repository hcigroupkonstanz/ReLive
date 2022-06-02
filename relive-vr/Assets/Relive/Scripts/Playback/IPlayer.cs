using Relive.Data;

namespace Relive.Playback
{
    public interface IPlayer
    {
        void StartPlayback(LoadedSession loadedSession);
        void StopPlayback();
        void Play();
        void Pause();
        void JumpTo(float seconds);
        void Wind(float seconds);
    }
}
