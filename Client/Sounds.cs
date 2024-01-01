using SFML.Audio;

namespace Client
{
    public static class Sounds
    {
        public static readonly SoundBuffer DiceRolling;

        static Sounds()
        {
            DiceRolling = new SoundBuffer(@"..\..\..\res\dice_rolling.wav");
        }
    }
}
