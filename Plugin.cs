using BepInEx.Logging;
using BepInEx;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using ScavSexMod.Helpers;
using System.IO;
using System.Security.Cryptography;
using TMPro;
using UnityEngine;
using ScavPrototypeSexMod.Managers;

namespace ScavPrototypeSexMod
{
    [BepInPlugin(Guid, Name, Version)]
    public class Plugin : BaseUnityPlugin
    {
        public const string Guid = "mfs.casualtiesunknown.scavsexmod";
        public const string Name = "ScavSexMod";
        public const string Version = "1.0.0";

        public static Plugin Instance;
        public static ManualLogSource Log;
        public static bool doOnce = false;

        public static Dictionary<string, string> localestrings = new Dictionary<string, string>();
        public static AudioClip heartbeatClip;
        public static AudioClip droneClip;

        public void Awake()
        {
            Instance = this;
            Log = Logger;

            UnityEngine.Screen.SetResolution(1366, 768, false);
            //UnityEngine.Screen.SetResolution(1920, 1080, false);
            //UnityEngine.Screen.SetResolution(2400, 1600, true);

            //UnityEngine.Screen.fullScreen = true;

            EmbeddedLoader.Init();

            heartbeatClip = FileLoader.LoadEmbeddedAudio(
        "ScavPrototypeSexMod.Assets.silence.wav", 1f);

            droneClip = FileLoader.LoadEmbeddedAudio(
                "ScavPrototypeSexMod.Assets.time.wav", 1f);

            Logger.LogInfo("ScavSexMod Loaded");

            // Log every embedded resource in the mod
            foreach (var res in Assembly.GetExecutingAssembly().GetManifestResourceNames())
            {
                Log.LogInfo("RESOURCE FOUND: " + res);
            }

            // Load the locales
            FileLoader.LoadLocale("en");

            // Just checking if everything is loading.
            ItemManager.Initialize();
            UIManager.Initialize();
            SexManager.Initialize();
            STDManager.Initialize();
            NetPlayManager.Initialize();

            new Harmony(Guid).PatchAll();
        }
    }
}
