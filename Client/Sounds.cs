using SFML.Audio;

namespace Client
{
    public static class Sounds
    {
        public static readonly SoundBuffer DiceRolling;
        public static readonly SoundBuffer Place;

        static Sounds()
        {
            DiceRolling = new SoundBuffer(@"..\..\..\res\dice_rolling.wav");
            Place = new SoundBuffer(@"..\..\..\res\place.wav");
        }
    }
}
