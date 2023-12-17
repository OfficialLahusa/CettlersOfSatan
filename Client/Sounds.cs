using SFML.Audio;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
