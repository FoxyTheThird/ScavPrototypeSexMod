using HarmonyLib;
using ScavPrototypeSexMod.Managers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEngine.UI;
using UnityEngine;
using ScavSexMod.Helpers;
using System.Security.Policy;
using System.Reflection;
using System.Collections;
using System.IO;
using System.Net;
using UnityEngine.Profiling;
using System.Xml.Serialization;

namespace ScavPrototypeSexMod.Patches
{
    // Patching Music Manager
    [HarmonyPatch(typeof(MusicManager))]
    public static class MoreFunnyStuff
    {
        private static FieldInfo _playedDeadField;
        public static AudioSource audSource;
        private static AudioClip _customDeathClip;
        private static AudioClip _silence;
        private static bool _initialized;
        private static float _currentVolume = 0f;
        private static bool _startedPlaying = false;

        [HarmonyPatch("Update")]
        [HarmonyPostfix]
        public static void ReplaceDeathMusic(MusicManager __instance)
        {
            if (!_initialized)
            {
                _playedDeadField = typeof(MusicManager).GetField("playedDead", BindingFlags.Instance | BindingFlags.NonPublic);

                _silence = FileLoader.LoadEmbeddedAudio(
                    "ScavPrototypeSexMod.Assets.silence.wav", 1f
                );

                _customDeathClip = FileLoader.LoadEmbeddedAudio("ScavPrototypeSexMod.Assets.shatter.wav", 1f);

                if (_customDeathClip == null)
                {
                    Plugin.Log.LogError("Failed to load custom death clip!");
                    return;
                }

                GameObject audSourceGO = new GameObject("Death");
                audSource = audSourceGO.AddComponent<AudioSource>(); 

                audSource.bypassReverbZones = true;
                audSource.dopplerLevel = 0f;
                audSource.spatialBlend = 0f;
                audSource.playOnAwake = false;
                audSource.loop = false;

                audSource.clip = _customDeathClip;

                _initialized = true;

                Plugin.Log.LogInfo("Custom death audio source initialized.");
            }

            bool playedDead = (bool)_playedDeadField.GetValue(__instance);

            if (__instance.deadClip != _silence)
            {
                Plugin.Log.LogInfo("Silencing Death Music.");
                __instance.deadClip = _silence;
                __instance.critSource.clip = _silence;
                __instance.critSourceUnc.clip = _silence;

            }

            if (playedDead)
            {
                if (!_startedPlaying)
                {
                    audSource.volume = 0f;
                    audSource.Play();
                    _startedPlaying = true;
                }

                _currentVolume = Mathf.MoveTowards(_currentVolume, 1f, Time.deltaTime * 0.25f);

                audSource.volume = _currentVolume;
            }
            else
            {
                if (_startedPlaying)
                {
                    audSource.Stop();
                    audSource.volume = 0f;
                    _currentVolume = 0f;
                    _startedPlaying = false;
                }
            }
        }
    }

    // Patching the player camera
    [HarmonyPatch(typeof(PlayerCamera))]
    public static class MenuShenanigans
    {
        [HarmonyPatch("Awake")]
        [HarmonyPostfix]
        public static void OnAwake(PlayerCamera __instance)
        {
            SharedState.pc = __instance;
        }

        [HarmonyPatch("ToggleTradeMenu")]
        [HarmonyPostfix]
        public static void OnTradeMenuOpened(PlayerCamera __instance)
        {
            if (__instance.tradeMenu.activeSelf)
            {
                SharedState.curTrader = __instance.currentTrader;
                __instance.StartCoroutine(UIManager.CreateTraderSexButton(__instance));

                __instance.currentTrader.reputation += 100;
                SharedState.TraderReputation = __instance.currentTrader.reputation;
            }
        }

        [HarmonyPatch("ToggleWoundView")]
        [HarmonyPostfix]
        public static void PlayerCamera_ToggleWoundView_Postfix(PlayerCamera __instance)
        {
            __instance.StartCoroutine(UIManager.InitSexModWoundView(__instance));
        }

        [HarmonyPatch("Update")]
        [HarmonyPostfix]
        public static void PlayerCamera_Update_Postfix(PlayerCamera __instance)
        {
            if (SharedState.wv && SharedState.wv.workoutList.activeSelf)
            {
                if (!SharedState.masturbateButton)
                {
                    __instance.StartCoroutine(UIManager.InitSexModWorkoutList(__instance));
                }
                else if (SharedState.masturbateButton && SharedState.masturbateButton.activeSelf == false)
                {
                    SharedState.masturbateButton.SetActive(true);
                }
            }
            else if (!__instance.gameObject.activeSelf && SharedState.masturbateButton && SharedState.masturbateButton.activeSelf)
            {
                SharedState.masturbateButton.SetActive(false);
            }
        }


        // My stupid BS patches. Make them optional.
        [HarmonyPatch("HandleDeathScreen")]
        [HarmonyPostfix]
        public static void PlayerCamera_HandleDeathScreen_Postfix(PlayerCamera __instance)
        {
            if (!__instance.body.alive && __instance.didDeathScreen && SharedState.funnystuff)
            {
                // stop all audio
                if (__instance.body.inWater)
                    __instance.deathScreen.sprite = SharedState.expieWater;
                else if (__instance.body.totalBleedSpeed > 0.02f)
                    __instance.deathScreen.sprite = SharedState.expieJoy;
                else
                    __instance.deathScreen.sprite = SharedState.expieThink;
            }
        }

        private static void PlayReplacement(AudioClip clip, Vector2 pos, float volume)
        {
            if (clip == null) return;

            GameObject go = new GameObject("LastStandReplacement");
            UnityEngine.Object.DontDestroyOnLoad(go);

            var src = go.AddComponent<AudioSource>();
            go.transform.position = pos;

            src.spatialBlend = 0f;
            src.dopplerLevel = 0f;
            src.volume = volume;
            src.clip = clip;
            src.bypassReverbZones = true;

            src.PlayOneShot(clip);

            UnityEngine.Object.Destroy(go, clip.length + 0.25f);
        }

        // For last stand.
        [HarmonyPatch(typeof(Sound))]
        public static class ReplaceLastStandSounds
        {
            [HarmonyPatch("Play", new Type[]
            {typeof(string), typeof(Vector2), typeof(bool), typeof(bool), typeof(Transform), typeof(float), typeof(float), typeof(bool), typeof(bool)})]
            [HarmonyPrefix]
            public static bool Prefix(ref string clip, Vector2 pos, bool twoDimensional, bool pitchShift, Transform follow, float volume, float pitch, bool noReverb, bool ignoreMixer)
            {
                if (!SharedState.funnystuff)
                    return true;

                if (clip == "laststandheartbeat")
                {
                    PlayReplacement(Plugin.heartbeatClip, pos, volume);
                    return false;
                }

                if (clip == "laststanddrone")
                {
                    PlayReplacement(Plugin.droneClip, pos, volume);
                    return false;
                }

                return true;
            }
        }

        [HarmonyPatch("LastStandSequence")]
        [HarmonyPrefix]
        public static void PlayerCamera_LastStandSequence_Prefix(PlayerCamera __instance)
        {
            if (SharedState.funnystuff)
            {
                if (__instance.lastStandImages == null || __instance.lastStandImages.Length == 0)
                    return;

                for (int i = 0; i < __instance.lastStandImages.Length; i++)
                {
                    __instance.lastStandImages[i] = SharedState.sugarCoated;
                }
            }
        }

        [HarmonyPatch(typeof(PlayerCamera), "Update")]
        public static class PlayerCamera_Update_Patch
        {
            [HarmonyPostfix]
            public static void Postfix(PlayerCamera __instance)
            {
                if (SharedState.Horniness >= 75f && SharedState.hCoroutine == null)
                {
                    SharedState.hCoroutine = __instance.StartCoroutine(SexManager.HornyRoutine(__instance));
                }

                STDManager.UpdateSTD();
            }
        }
    }

    // Adding a body part to the body before it grabs a list of body parts.
    /*[HarmonyPatch(typeof(Body), "Awake")]
    public static class Body_Awake_Patch
    {
        [HarmonyPostfix]
        public static void Postfix(Body __instance)
        {
            // Prevent double-injection (Awake can run more than once in some cases)
            if (__instance.limbs.Any(l => l.name == "Dick"))
            {
                Plugin.Log.LogWarning("Dick limb already exists.");
                return;
            }

            LimbManager.Initialize(__instance);
        }
    }

    [HarmonyPatch(typeof(Limb), "Awake")]
    public static class Limb_Awake_Patch
    {
        [HarmonyPrefix]
        static bool Prefix(Limb __instance)
        {
            if (SharedState.IsConstructing)
            {
                // Skip Awake during manual construction
                return false;
            }

            return true;
        }
    }*/

    // Instead of adding limbs atm just replace sprites on body cuz that's easier.
    [HarmonyPatch(typeof(Body))]
    public class SpriteReplacement
    {
        public static void ReplaceSprites()
        {
            for (int i = 0; i < SharedState.bod.limbs.Length; i++)
            {
                Limb limb = SharedState.bod.limbs[i];
                SharedState.limbName[i] = limb.gameObject.name;
                SharedState.limbsprite[i] = limb.gameObject.GetComponent<SpriteRenderer>();
            }

            SharedState.limbCount = SharedState.bod.limbs.Length;

            Sprite[] replacements = new Sprite[SharedState.ASSET_NAMES.Length];
            for (int j = 0; j < SharedState.ASSET_NAMES.Length; j++)
            {
                replacements[j] = FileLoader.LoadEmbeddedSprite(SharedState.ASSET_NAMES[j], 8f);
            }

            for (int i = 0; i < SharedState.limbCount; i++)
            {
                bool flag = SharedState.changeAssets[i] != -1;
                if (flag)
                {
                    SharedState.limbsprite[i].sprite = replacements[SharedState.changeAssets[i]];
                }
            }
        }
        
        [HarmonyPatch("PlaceBody"), HarmonyPostfix]
        public static void Body_PlaceBody_Hook(Body __instance)
        {
            SharedState.ASSET_NAMES = (string[])SharedState.BASE_ASSET_NAMES.Clone();

            switch (SharedState.CurrentGender)
            {
                case SharedState.Gender.Female:
                    SharedState.ASSET_NAMES[0] = "expieboobs.png";
                    SharedState.ASSET_NAMES[1] = "expiepussy.png";
                    break;
                case SharedState.Gender.Male:
                    SharedState.ASSET_NAMES[0] = "experimentUpTorso.png";
                    SharedState.ASSET_NAMES[1] = "experimentDownTorso.png";
                    break;
                case SharedState.Gender.Intersex:
                    SharedState.ASSET_NAMES[0] = "experimentUpTorso.png";
                    SharedState.ASSET_NAMES[1] = "experimentDownTorso.png";
                    break;
                case SharedState.Gender.NonBinary:
                    SharedState.ASSET_NAMES[0] = "experimentUpTorso.png";
                    SharedState.ASSET_NAMES[1] = "experimentDownTorso.png";
                    break;
            }

            SharedState.bod = __instance;

            ReplaceSprites();
        }

        [HarmonyPatch("WearWearable"), HarmonyPostfix]
        public static void Body_WearWearable_Postfix(Body __instance)
        {
            __instance.GetWearableBySlotID("outertorso"); // UpTorso
            __instance.GetWearableBySlotID("torsofront"); // DownTorso

            var upTorso = __instance.GetWearableBySlotID("outertorso") || __instance.GetWearableBySlotID("torsofront");

            var armor = __instance.GetWearable("striderpelt") || __instance.GetWearable("tornshirt") || __instance.GetWearable("bellyarmor");

            if (upTorso && !armor)
            {
                SharedState.ASSET_NAMES[0] = "experimentUpTorso.png";
            }

            ReplaceSprites();
        }

        [HarmonyPatch("DropWearable"), HarmonyPostfix]
        public static void Body_DropWearable_Postfix(Body __instance)
        {
            __instance.GetWearableBySlotID("outertorso"); // UpTorso
            __instance.GetWearableBySlotID("torsofront"); // DownTorso

            var upTorso = __instance.GetWearableBySlotID("outertorso") || __instance.GetWearableBySlotID("torsofront");

            if (!upTorso)
            {
                switch (SharedState.CurrentGender)
                {
                    case SharedState.Gender.Female:
                        SharedState.ASSET_NAMES[0] = "expieboobs.png";
                        break;
                    case SharedState.Gender.Male:
                        SharedState.ASSET_NAMES[0] = "experimentUpTorso.png";
                        break;
                    case SharedState.Gender.Intersex:
                        SharedState.ASSET_NAMES[0] = "experimentUpTorso.png";
                        break;
                    case SharedState.Gender.NonBinary:
                        SharedState.ASSET_NAMES[0] = "experimentUpTorso.png";
                        break;
                }
            }

            ReplaceSprites();
        }
    }

    // Patching the trader reputation
    [HarmonyPatch(typeof(TraderScript))]
    public class CheatReputation
    {
        [HarmonyPatch("Awake"), HarmonyPostfix]
        public static void TraderScript_Awake_Hook(TraderScript __instance)
        {
            // Not sure how to influence reputation, seems like there's an extra check.
            //__instance.reputation += 9999f;
            __instance.valueGiven += 60;
            __instance.totalValueGiven += 60;
            __instance.minHugReputation = 0f;
            Plugin.Log.LogInfo("Value given is set to max!");
        }
    }

    [HarmonyPatch(typeof(Item))]
    public class FunnyItemStuff
    {
        [HarmonyPatch("SetupItems"), HarmonyPostfix]
        public static void SetupItems_Postfix()
        {
            ItemManager.RegisterItems();

            // Reset the values so they don't persist between runs
            SexManager.ResetVals();
        }

        [HarmonyPatch("Update"), HarmonyPostfix]
        public static void Update_Postfix()
        {
            if (SharedState.horninessText != null)
            {
                SharedState.horninessText.text = SharedState.Horniness.ToString("F0") + "%";
            }
        }
    }

    // Make it actually do smth when u choose ur gender
    [HarmonyPatch(typeof(PreRunScript), "Start")]
    public class MenuPrefs
    {
        [HarmonyPostfix]
        public static void PreRunScript_Start_Posfix()
        {
            UIManager.CreateGenderRadios();
        }
    }

    // Moodle Manager!
    [HarmonyPatch(typeof(MoodleManager))]
    public class MoodleManagerMayhem
    {

        [HarmonyPatch("AddAllMoodles"), HarmonyPostfix]
        public static void AddAllMoodles_Postfix(MoodleManager __instance)
        {
            float h = SharedState.Horniness;

            if (SharedState.WearingCondom)
            {
                MoodleHandler.AddMoodle(__instance, "protectionmoodle");
            }

            if (h < 1f)
            {
                return;
            }
            else if (h <= 29f)
            {
                MoodleHandler.AddMoodle(__instance, "hornymoodle");
            }
            else if (h <= 54f)
            {
                MoodleHandler.AddMoodle(__instance, "horniermoodle");
            }
            else if (h <= 79f)
            {
                MoodleHandler.AddMoodle(__instance, "evenhorniermoodle");
            }
            else
            {
                MoodleHandler.AddMoodle(__instance, "horniestmoodle");
            }
        }

        [HarmonyPatch("Awake"), HarmonyPostfix]
        public static void Awake_Postfix(MoodleManager __instance)
        {
            MoodleHandler.Initialize(__instance);
        }
    }


    // Roblox sparkle particle ahh
    // Also it said it registered the liquid so, yah.
    // Figure out how to spawn liquids using the fluid manager
    // Maybe replace water with your "cum" liquid
    [HarmonyPatch(typeof(FluidManager), "Start")]
    public class FunnyLiquidStuff
    {
        [HarmonyPostfix]
        public static void FluidManager_Postfix(FluidManager __instance)
        {
            // do LITERALLY nothing
        }
    }

    public class LegallyRequired
    {
        // Yeaaaah no we're not doing that here.
        [HarmonyPatch(typeof(WoundView), "SetCharDetails")]
        public class WoundView_SetCharDetails_Patch
        {
            [HarmonyPrefix]
            public static void Prefix(ref int age)
            {
                int randomAddition = UnityEngine.Random.Range(0, 18);
                age += randomAddition;

                age = Mathf.Clamp(age, 18, 40);
            }

            [HarmonyPatch(typeof(WoundView), "Update")]
            [HarmonyPostfix]
            public static void AddWoundView(WoundView __instance)
            {
                if (SharedState.wv == null)
                {
                    SharedState.wv = __instance;
                }
            }
        }
    }

}
