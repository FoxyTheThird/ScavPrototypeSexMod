using System;
using System.IO;
using UnityEngine;
using BepInEx.Logging;
using System.Reflection;
using System.Linq;
using NAudio.Wave;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Security.Cryptography;
using TMPro;
using Newtonsoft.Json;
using ScavPrototypeSexMod;

namespace ScavSexMod.Helpers
{
    public static class EmbeddedLoader
    {
        public static void Init()
        {
            AppDomain.CurrentDomain.AssemblyResolve += OnResolveAssembly;
        }

        private static Assembly OnResolveAssembly(object sender, ResolveEventArgs args)
        {
            var requestedAssembly = new AssemblyName(args.Name).Name + ".dll";

            var assembly = Assembly.GetExecutingAssembly();
            var resource = assembly.GetManifestResourceNames()
                .FirstOrDefault(r => r.EndsWith(requestedAssembly, StringComparison.OrdinalIgnoreCase));

            if (resource == null)
                return null;

            using (var stream = assembly.GetManifestResourceStream(resource))
            {
                if (stream == null)
                    return null;

                using (var ms = new MemoryStream())
                {
                    stream.CopyTo(ms);
                    return Assembly.Load(ms.ToArray());
                }
            }
        }
    }

    public static class FileLoader
    {
        // Loads files that have been set to Embedded Resource in the Build Action file properties
        public static (string, Stream) LoadFileStream(string fileName, string folderName = null)
        {
            // Gets the file name. If it isn't exact, it returns null
            Assembly asm = Assembly.GetExecutingAssembly();
            fileName = asm.GetManifestResourceNames().FirstOrDefault(n => n.EndsWith(fileName));
            if (fileName == null)
            {
                Debug.LogError($"File by the name of {fileName} does not exist. Check capitalization and file extension");
                return (null, null);
            }

            // Gets the file stream
            Stream stream = asm.GetManifestResourceStream(fileName);
            if (!fileName.StartsWith(Assembly.GetExecutingAssembly().GetName().Name + "." + (folderName != null ? folderName + "." : "")))
            {
                Debug.LogError($"File does not exist in embedded resources");
                return (null, null);
            }
            int lastDot = fileName.LastIndexOf(".");
            int secondLastDot = fileName.LastIndexOf(".", lastDot - 1);
            return (fileName.Substring(secondLastDot + 1), stream);
        }

        public static (string, byte[]) LoadFileBytes(string fileName)
        {
            (string, Stream) fileInfo = LoadFileStream(fileName);
            Stream stream = fileInfo.Item2;
            byte[] fileData = new byte[stream.Length];
            stream.Read(fileData, 0, fileData.Length);
            return (fileInfo.Item1, fileData);
        }

        public static AudioClip LoadEmbeddedAudio(string fileName, float gain)
        {
            (string, Stream) streamData = LoadFileStream(fileName);
            string newFileName = streamData.Item1;
            Stream stream = streamData.Item2;

            string fileExt = Path.GetExtension(newFileName)
                .TrimStart('.')
                .ToLowerInvariant();

            ISampleProvider provider;
            switch (fileExt)
            {
                case "wav":
                    provider = new WaveFileReader(stream).ToSampleProvider();
                    break;

                case "mp1":
                case "mp2":
                case "mp3":
                    provider = new Mp3FileReader(stream).ToSampleProvider();
                    break;

                case "cue":
                    provider = new CueWaveFileReader(stream).ToSampleProvider();
                    break;

                case "aif":
                case "aiff":
                    provider = new AiffFileReader(stream).ToSampleProvider();
                    break;

                default:
                    Debug.LogError($"Could not load audio file {fileName}: Unknown file extension {fileExt}");
                    return null;
            }

            // Reads file data sample rate and channels to make a samples data array
            List<float> samples = new List<float>();
            float[] buffer = new float[provider.WaveFormat.SampleRate * provider.WaveFormat.Channels];
            int read;
            while ((read = provider.Read(buffer, 0, buffer.Length)) > 0)
            {
                for (int i = 0; i < read; i++)
                {
                    float amplified = buffer[i] * gain;

                    // Clamp so we don't distort into NaNs
                    amplified = Mathf.Clamp(amplified, -1f, 1f);

                    samples.Add(amplified);
                }
            }

            // Sets mandatory variables for sample rate, channels, and samples/channel
            WaveFormat waveFormat = provider.WaveFormat;
            int sampleRate = waveFormat.SampleRate;
            int channels = waveFormat.Channels;
            int samplesPerChannel = samples.Count / channels;

            // Creates and returns the audio clip
            AudioClip clip = AudioClip.Create(fileName, samplesPerChannel, channels, sampleRate, false);
            clip.SetData(samples.ToArray(), 0);
            return clip;
        }

        public static Sprite LoadEmbeddedSprite(string fileName, float ppu = 100, FilterMode filterMode = FilterMode.Point, int widthMultiplier = 1, int heightMultiplier = 1)
        {
            try
            {
                (string, Stream) streamData = LoadFileStream(fileName);
                string newFilename = streamData.Item1;
                Stream stream = streamData.Item2;

                if (stream == null)
                {
                    Plugin.Log.LogWarning($"[LoadEmbeddedSprite] Stream is null for file: {fileName}");
                    return null;
                }

                byte[] fileData = new byte[stream.Length];
                int bytesRead = stream.Read(fileData, 0, fileData.Length);
                if (bytesRead != fileData.Length)
                {
                    Plugin.Log.LogWarning($"[LoadEmbeddedSprite] Read {bytesRead}/{fileData.Length} bytes for file: {fileName}");
                }

                Texture2D texture = new Texture2D(2, 2, TextureFormat.RGBAHalf, false)
                {
                    wrapMode = TextureWrapMode.Clamp,
                    filterMode = filterMode,
                    name = newFilename
                };

                if (!texture.LoadImage(fileData))
                {
                    Plugin.Log.LogWarning($"[LoadEmbeddedSprite] Failed to load image data for file: {fileName}");
                    return null;
                }

                texture.Apply();

                Sprite sprite = Sprite.Create(
                    texture,
                    new Rect(0, 0, texture.width, texture.height),
                    new Vector2(0.5f, 0.5f),
                    ppu
                );
                sprite.name = newFilename;

                return sprite;
            }
            catch (Exception ex)
            {
                Plugin.Log.LogError($"[LoadEmbeddedSprite] Exception while loading sprite '{fileName}': {ex}");
                return null;
            }
        }

        // Loading embedded fonts
        public static Font LoadEmbeddedFont(string EmbeddedfontPath)
        {
            (string, byte[]) fileInfo = FileLoader.LoadFileBytes(EmbeddedfontPath);
            string modDir = Path.Combine(Application.persistentDataPath, Assembly.GetExecutingAssembly().GetName().Name);
            Directory.CreateDirectory(modDir);
            string fontPath = Path.Combine(modDir, fileInfo.Item1);
            SHA256 sha256 = SHA256.Create();
            if (!File.Exists(fontPath))
                File.WriteAllBytes(fontPath, fileInfo.Item2);
            else
            {
                byte[] existingFileSha256 = sha256.ComputeHash(File.OpenRead(fontPath));
                byte[] embeddedFileSha256 = sha256.ComputeHash(fileInfo.Item2);
                if (!embeddedFileSha256.SequenceEqual(existingFileSha256))
                {
                    Plugin.Log.LogInfo("Font hash isn't the same, overwriting");
                    File.WriteAllBytes(fontPath, fileInfo.Item2);
                }
            }

            Font cusFont = new Font(fontPath);

            return cusFont;
        }

        public static void LoadLocale(string localeCode)
        {
            try
            {
                string resourceName = $"ScavPrototypeSexMod.Assets.locale.{localeCode}.json";

                Assembly assembly = Assembly.GetExecutingAssembly();

                Stream stream = assembly.GetManifestResourceStream(resourceName);
                if (stream == null)
                {
                    Plugin.Log.LogError($"Locale {localeCode}: resource not found");
                    return;
                }

                StreamReader reader = new StreamReader(stream);
                string jsonText = reader.ReadToEnd();

                // Deserialize JSON
                Dictionary<string, string> dictionary = JsonConvert.DeserializeObject<Dictionary<string, string>>(jsonText);

                if (dictionary != null)
                {
                    foreach (KeyValuePair<string, string> kvp in dictionary)
                    {
                        Plugin.Log.LogInfo($"Loaded locale string: {kvp.Key} = {kvp.Value}");
                        Plugin.localestrings[kvp.Key] = kvp.Value;
                    }
                }
            }
            catch (Exception ex)
            {
                Plugin.Log.LogError($"Locale {localeCode} is not found: {ex.Message}");
            }
        }

        public static string GetLocale(string key)
        {
            string text;
            bool flag = Plugin.localestrings.TryGetValue(key, out text);
            string result;
            if (flag)
            {
                result = text;
            }
            else
            {
                result = key;
            }
            return result;
        }
    }
}