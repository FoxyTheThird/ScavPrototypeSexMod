using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScavPrototypeSexMod
{
    [Serializable]
    public class ExtraBodyData
    {
        public Gender CurrentGender = Gender.NonBinary;

        public float Horniness = 100f;
        public float Hardness = 1f;
        public float durHorny = 0f;
        public bool havingSex = false;

        public bool WearingCondom = false;
        public bool CondomInInventory = false;
        public float breakChance;
        public bool HasSTD = false;
        public float infectprog = 0f;

    }
}
