using HarmonyLib;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace ScavPrototypeSexMod.Managers
{
    public static class LimbManager
    {
        // Why??
        public static Transform[] limbs = new Transform[32];
        public static string[] limbName = new string[32];
        public static SpriteRenderer[] sprite = new SpriteRenderer[32];
        public static int limbCount = 0;

        public static Dictionary<string, Limb> bodyLimbs = new Dictionary<string, Limb>();
        public static Dictionary<string, SpriteRenderer> limbSprites = new Dictionary<string, SpriteRenderer>();

        public static void GetLimbs(Body bod)
        {
            bodyLimbs.Clear();

            foreach (var limb in bod.limbs)
            {
                if (limb != null && limb.gameObject != null)
                {
                    bodyLimbs[limb.gameObject.name] = limb;
                    Plugin.Log.LogInfo($"Limb {limb.gameObject.name} obtained.");
                }
            }
        }

        public static void GetLimbSprites(Body bod)
        {
            limbSprites.Clear();

            foreach (var limb in bod.limbs)
            {
                if (limb != null && limb.gameObject != null)
                {
                    var sr = limb.gameObject.GetComponent<SpriteRenderer>();
                    limbSprites[limb.gameObject.name] = sr;
                }
            }
        }


        public static void Initialize(Body bod)
        {
            Plugin.Log.LogInfo("LimbManager Initialized");

            GetLimbs(bod);
            GetLimbSprites(bod);
            RegisterLimbs(bod);
        }

        /*public static IEnumerator RegisterLimbs(Body bod)
        {
            yield return null; // wait one frame so Body is fully initialized

            // Pick a safe vanilla limb to clone (arm is usually safest)
            Limb template = bod.limbs[0];

            Limb newLimb = UnityEngine.Object.Instantiate(template, template.transform.parent);

            newLimb.name = "dick"; // or whatever

            UITooltip tip = newLimb.GetComponent<UITooltip>();
            if (tip != null)
            {
                tip.name = newLimb.name;
                tip.tipDesc = "Your dick.";
                tip.skipLocale = true;
            }

            bod.limbs.AddItem(newLimb);

            wv.AddImageToLimb(newLimb, SharedState.limbtemp, false, null);

            Plugin.Log.LogInfo("Successfully cloned and registered limb.");

            yield return null;
        }*/

        // This is chatGPTed
        // LIMBS ARE HARD TO UNDERSTAND?
        // HOW ARE THEY ATTACHED TO THE BODY???

        
        public static void RegisterLimbs(Body bod)
        {
            if (bod == null)
            {
                Plugin.Log.LogWarning("Body is null!");
                return;
            }

            if (bod.limbs.Any(l => l.name == "Dick"))
            {
                Plugin.Log.LogWarning("Dick already exists, skipping.");
                return;
            }

            Limb abdomen = bodyLimbs["DownTorso"];
            Limb thighF = bodyLimbs["ThighF"];
            Limb thighB = bodyLimbs["ThighB"];

            if (abdomen == null || thighF == null || thighB == null)
            {
                Plugin.Log.LogWarning("Can't find required limbs!");
                return;
            }

            // Make the limb GameObject
            GameObject limbGO = new GameObject("Dick");
            limbGO.transform.SetParent(bod.transform, false);
            limbGO.layer = LayerMask.NameToLayer("Limb");
            limbGO.SetActive(false);

            var sr = limbGO.AddComponent<SpriteRenderer>();
            var col = limbGO.AddComponent<BoxCollider2D>();
            var rb = limbGO.AddComponent<Rigidbody2D>();
            var hj = limbGO.AddComponent<HingeJoint2D>();
            var ps = limbGO.AddComponent<ParticleSystem>();

            // Setup SpriteRenderer
            sr.sprite = SharedState.limbtemp;
            sr.sortingLayerName = "Body";
            sr.sortingOrder = 200;

            // BoxCollider
            col.size = new Vector2(0.18f, 0.18f);
            col.enabled = true;

            // Rigidbody2D
            rb.mass = 0.15f;
            rb.angularDrag = 0.05f;
            rb.drag = 0.05f;
            rb.interpolation = RigidbodyInterpolation2D.Interpolate;
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            rb.sleepMode = RigidbodySleepMode2D.NeverSleep;
            rb.constraints = RigidbodyConstraints2D.None;

            SharedState.IsConstructing = true;

            // Add Limb component
            Limb limb = limbGO.AddComponent<Limb>();
            limb.body = bod;
            limb.rb = rb;
            limb.joint = hj;
            limb.baseMass = rb.mass;

            // Run the original private Awake to initialize all internal fields safely
            var awakeMethod = typeof(Limb).GetMethod("Awake", BindingFlags.Instance | BindingFlags.NonPublic);
            awakeMethod?.Invoke(limb, null);

            // Initialize private 'mat' field
            var matField = typeof(Limb).GetField("mat", BindingFlags.Instance | BindingFlags.NonPublic);
            if (matField != null)
            {
                Material matInstance = new Material(sr.material);
                matField.SetValue(limb, matInstance);
                sr.sharedMaterial = matInstance;

                // Bonus renderer, if assigned
                var bonusField = typeof(Limb).GetField("bonusRendererToAffect", BindingFlags.Instance | BindingFlags.NonPublic);
                if (bonusField != null)
                {
                    var bonusRenderer = (SpriteRenderer)bonusField.GetValue(limb);
                    if (bonusRenderer != null)
                        bonusRenderer.sharedMaterial = matInstance;
                }
            }

            // Initialize private 'dripEmmis' field
            var dripField = typeof(Limb).GetField("dripEmmis", BindingFlags.Instance | BindingFlags.NonPublic);
            if (dripField != null)
            {
                dripField.SetValue(limb, ps.emission);
            }

            // Health and bleeding defaults
            limb.skinHealth = 100f;
            limb.muscleHealth = 100f;
            limb.bleedSpeedMult = 1f;

            // Configure HingeJoint
            hj.tag = "Player";
            hj.name = "Dick";
            hj.connectedBody = abdomen.rb;
            hj.autoConfigureConnectedAnchor = false;
            hj.anchor = new Vector2(0f, 0.09f);
            hj.connectedAnchor = new Vector2(0f, -0.05f);
            JointAngleLimits2D lim = new JointAngleLimits2D();
            lim.min = -75f;
            lim.max = 75f;
            hj.limits = lim;
            hj.useLimits = true;

            // Connect to abdomen
            limb.connectedLimbs = new Limb[] { abdomen };

            // Add to the body limbs array safely
            /*Limb[] oldLimbs = bod.limbs ?? new Limb[0];
            Limb[] newLimbs = new Limb[oldLimbs.Length + 1];
            Array.Copy(oldLimbs, newLimbs, oldLimbs.Length);
            newLimbs[newLimbs.Length - 1] = limb;
            bod.limbs = newLimbs;*/

            rb.simulated = true;
            limbGO.SetActive(true);
            SharedState.IsConstructing = false;

            Plugin.Log.LogWarning("Dick is connected to: " + limb.joint.connectedBody);
            Plugin.Log.LogInfo("Custom limb attached to abdomen.");
        }


    }
}
