using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common
{
    [Flags]
    public enum PortPrivileges : byte
    {
        None = 0,               // 0b00000000
        GenericThreeToOne = 1,  // 0b00000001
        LumberTwoToOne = 2,     // 0b00000010
        BrickTwoToOne = 4,      // 0b00000100
        WoolTwoToOne = 8,       // 0b00001000
        GrainTwoToOne = 16,     // 0b00010000
        OreTwoToOne = 32,       // 0b00100000
    }
}
