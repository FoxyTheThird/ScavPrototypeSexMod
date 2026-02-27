using ScavSexMod.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ScavPrototypeSexMod
{
    public static class SharedState
    {
        public static Dictionary<string, GameObject> ModPrefabs = new Dictionary<string, GameObject>();

        public static TraderScript curTrader;

        public static float Horniness = 100f;
        public static float Hardness = 1f;
        public static float durHorny = 0f;

        public static bool WearingCondom = false;
        public static bool CondomInInventory = false;
        public static float breakChance;
        public static bool HasSTD = false;
        public static float infectprog = 0f;

        public static Dictionary<string, bool> stdTypes = new Dictionary<string, bool>()
        {
            {"Syphilis", false},
            {"HIV", false},
            {"HPV", false},
            {"Herpes", false},
            {"Gonorrhoea", false},
            {"Scabies", false},
            {"Trichomoniasis", false}
        };

        public static float TraderReputation = 0f;
        public static bool IsConstructing;

        public enum Gender
        {
            Male,
            Female,
            Intersex,
            NonBinary
        }

        public enum MoreWorkoutTypes
        {
            Masturbate
        }

        // Coroutines
        public static Coroutine vCoroutine = null;
        public static Coroutine hCoroutine = null;
        public static Coroutine mCoroutine = null;

        public static Gender CurrentGender = Gender.NonBinary;

        public static TextMeshProUGUI horninessText;
        public static GameObject horninessRoot;
        public static GameObject genderRoot;
        public static GameObject masturbateButton;

        // Fuckass persistent variables for my whiny functions
        public static PlayerCamera pc;
        public static WoundView wv;
        public static Body bod;

        // Sprite replacements
        public static Sprite limbtemp = FileLoader.LoadEmbeddedSprite("ScavPrototypeSexMod.Assets.limbtemplate.png", 125);
        public static SpriteRenderer[] limbsprite = new SpriteRenderer[32];
        public static Sprite[,] spritesLimbUI = new Sprite[2, 15];
        public static string[] limbName = new string[32];
        public static int limbCount = 0;

        // For values within the limb list
        // -1 means don't sprite replace for that limb.
        // TODO: Implement dick stages of hardness.
        public static readonly int[] changeAssets = new int[]
        {
            -1, 0, 1, -1, -1, -1, -1, -1, -1, 2, -1, -1, 2, -1, -1
        };

        public static readonly string[] BASE_ASSET_NAMES = new string[]
        {
            "experimentUpTorso.png",
            "experimentDownTorso.png",
            "experimentThigh.png",
        };

        public static string[] ASSET_NAMES;

        // My BS
        public static bool funnystuff = true;
        public static Sprite expieJoy = FileLoader.LoadEmbeddedSprite("ScavPrototypeSexMod.UnrelatedAssets.smilingexpiedeath.png");
        public static Sprite expieWater = FileLoader.LoadEmbeddedSprite("ScavPrototypeSexMod.UnrelatedAssets.waterexpiedeath.png");
        public static Sprite expieThink = FileLoader.LoadEmbeddedSprite("ScavPrototypeSexMod.UnrelatedAssets.thinkingexpiedeath.png");
        public static Sprite sugarCoated = FileLoader.LoadEmbeddedSprite("ScavPrototypeSexMod.UnrelatedAssets.sugarcoat.png");
    }
}
