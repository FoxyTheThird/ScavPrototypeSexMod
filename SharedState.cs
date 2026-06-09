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
    public enum Gender
    {
        Male,
        Female,
        Intersex,
        NonBinary
    }

    struct SharedState
    {
        public static Dictionary<string, GameObject> ModPrefabs = new Dictionary<string, GameObject>();

        public static TraderScript curTrader;

        public const string PREF_KEY = "GenderSelection";
        public static int savedIndex = 3;

        // Implement this in settings menus on main menu and in run
        // Use sharedprefs to save the values. Just like how you did with gender.
        public static Dictionary<string, bool> kinkOptions = new Dictionary<string, bool>
        {
            { "Watersports", false },
            { "Noncon", false },
            { "Pregnacy", false },
            // With the animals in cas unk, technically.
            { "Bestiality", false },
            { "STDs", false},
        };

        // Add Dicktypes eventually, when u get those dick sprites.

        // Body variables
        public static float Horniness = 100f;
        public static float Hardness = 1f;
        public static float durHorny = 0f;
        public static bool havingSex = false;

        public static bool WearingCondom = false;
        public static bool CondomInInventory = false;
        public static float breakChance;
        public static bool HasSTD = false;
        public static float infectprog = 0f;

        public static Gender CurrentGender = Gender.NonBinary;

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

        public enum MoreWorkoutTypes
        {
            Masturbate
        }

        // Coroutines
        public static Coroutine vCoroutine = null;
        public static Coroutine hCoroutine = null;
        public static Coroutine mCoroutine = null;

        // Custom gameobjects
        public static TextMeshProUGUI horninessText;
        public static GameObject horninessRoot;
        public static GameObject genderRoot;
        public static GameObject masturbateButton;

        // Fuckass persistent variables for my whiny functions
        public static PlayerCamera pc;
        public static WoundView wv = null;
        public static Body bod;
        public static Vomiter vom;
        public static FluidManager fm;
        public static PreRunScript prs;

        // Sprite replacements
        public static Sprite limbtemp = FileLoader.LoadEmbeddedSprite("ScavPrototypeSexMod.Assets.limbtemplate.png", 125);
        public static SpriteRenderer[] limbsprite = new SpriteRenderer[32];
        public static Sprite[,] spritesLimbUI = new Sprite[2, 15];
        public static string[] limbName = new string[32];
        public static int limbCount = 0;

        // Animation Clips for da animations
        internal static byte[] bundleBytes = FileLoader.LoadFileBytes("ScavPrototypeSexMod.Assets.Animations.animations.bundle").Item2;
        internal static AnimationClip[] clips = FileLoader.LoadEmbeddedBundle(bundleBytes);

        public static AnimationClip armsJerk = clips[0];
        public static AnimationClip armsJerkAction = clips[1];
        public static AnimationClip experimentJerkSit = clips[2];

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

        // Particles...
        public static GameObject cpart = null;

        // My BS
        public static bool funnystuff = true;
        public static Sprite expieJoy = FileLoader.LoadEmbeddedSprite("ScavPrototypeSexMod.Assets.UnrelatedAssets.smilingexpiedeath.png");
        public static Sprite expieWater = FileLoader.LoadEmbeddedSprite("ScavPrototypeSexMod.Assets.UnrelatedAssets.waterexpiedeath.png");
        public static Sprite expieThink = FileLoader.LoadEmbeddedSprite("ScavPrototypeSexMod.Assets.UnrelatedAssets.thinkingexpiedeath.png");
        public static Sprite sugarCoated = FileLoader.LoadEmbeddedSprite("ScavPrototypeSexMod.Assets.UnrelatedAssets.sugarcoat.png");
    }
}
