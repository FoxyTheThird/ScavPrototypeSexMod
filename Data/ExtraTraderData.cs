using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScavPrototypeSexMod
{
    [Serializable]
    public class ExtraTraderData
    {
        public Dictionary<string, bool> bdsm = new Dictionary<string, bool>
        {
            { "Switch", false },
            { "Top", false },
            { "Bottom", false },
            { "Masochist", false },
            { "Sadist", false }
        };

        public float Tightness = 100f;

        public float Horniness = 0f;
        public float Hardness = 1f;
        public bool havingSex = false;
        public bool wearingCondom = false;
        public bool hasSTD = false;

        // Perhaps add a times fucked counter too? Not sure.
    }
}
