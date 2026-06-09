using HarmonyLib;
using ScavSexMod.Helpers;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;

namespace ScavPrototypeSexMod.Managers
{
    // Made with chatgpt, clean it up!
    internal class ParticleManager : MonoBehaviour
    {
        readonly static Sprite cummies = FileLoader.LoadEmbeddedSprite("cummies.png");
        readonly static Sprite cummiesvom = FileLoader.LoadEmbeddedSprite("cummies-vom.png");

        static readonly AccessTools.FieldRef<BleedParticle, GameObject> groundRef = AccessTools.FieldRefAccess<BleedParticle, GameObject>("groundBleed");
        static readonly AccessTools.FieldRef<BleedParticle, GameObject> wallRef = AccessTools.FieldRefAccess<BleedParticle, GameObject>("wallBleed");

        public GameObject wallchild = null;
        public GameObject groundchild = null;

        public static void Initialize()
        {
            Plugin.Log.LogInfo("ParticleManager Initialized");
        }

        public static GameObject MakeParticle(Sprite spriteWall, Sprite spriteGround)
        {
            GameObject newPartGO = new GameObject("PartGO");
            ParticleSystem partsys = newPartGO.AddComponent<ParticleSystem>();
            ParticleManager partman = newPartGO.AddComponent<ParticleManager>();

            newPartGO.transform.localScale = new Vector3(1, 0.3f, 1);

            /*GameObject wallBleed = new GameObject("wallBleed");
            GameObject groundBleed = new GameObject("groundBleed");

            groundBleed.transform.SetParent(newPartGO.transform);
            wallBleed.transform.SetParent(newPartGO.transform);*/

            //BleedParticle partbleed = newPartGO.AddComponent<BleedParticle>();
            //GroundBlood groundbleed = newPartGO.AddComponent<GroundBlood>();

            //SpriteRenderer srw = wallBleed.AddComponent<SpriteRenderer>();
            //SpriteRenderer srg = groundBleed.AddComponent<SpriteRenderer>();

            //srw.sprite = spriteWall;
            //srg.sprite = spriteGround;

            //partbleed.vomit = false;

            return newPartGO;
        }

        // For pissing
        public static GameObject makePiss()
        {
            GameObject part = MakeParticle(cummies, cummiesvom);

            var partsys = part.GetComponent<ParticleSystem>();
            ParticleSystemRenderer partrend = part.GetComponent<ParticleSystemRenderer>();
            var main = partsys.main;
            var shape = partsys.shape;
            var velocity = partsys.velocityOverLifetime;
            var noise = partsys.noise;
            var coll = partsys.collision;
            var emission = partsys.emission;
            var partcol = partsys.colorOverLifetime;
            var rotovertime = partsys.rotationOverLifetime;

            main.startLifetime = 1f;
            main.startSpeed = 5f;
            main.gravityModifier = 2.5f;
            main.gravitySource = ParticleSystemGravitySource.Physics2D;
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.simulationSpeed = 0.7f;

            rotovertime.enabled = true;
            rotovertime.y = 90f;
            rotovertime.z = 180f;

            partrend.enabled = true;
            partrend.renderMode = ParticleSystemRenderMode.Billboard;
            partrend.lengthScale = 2f;
            partrend.freeformStretching = false;
            partrend.rotateWithStretchDirection = true;
            partrend.material = new Material(Shader.Find("Sprites/Default"));

            shape.enabled = true;
            shape.shapeType = ParticleSystemShapeType.Box;
            shape.scale = new Vector3(0.1f, 0.1f, 0.1f);

            velocity.enabled = true;
            velocity.space = ParticleSystemSimulationSpace.World;

            noise.enabled = false;
            noise.strength = 0.5f;
            noise.frequency = 0.4f;
            noise.scrollSpeed = 10f;
            noise.damping = true;

            coll.enabled = true;
            coll.type = ParticleSystemCollisionType.World;
            coll.mode = ParticleSystemCollisionMode.Collision2D;
            coll.dampen = 0f;
            coll.bounce = 0f;
            coll.lifetimeLoss = 1f;
            coll.sendCollisionMessages = true;
            // Prolly make it collide with water too which is 16
            coll.collidesWith = 64;

            emission.enabled = true;
            emission.rateOverTime = 15f;

            partcol.enabled = true;
            partcol.color = new Gradient
            {
                colorKeys = new GradientColorKey[]
                {
                    new GradientColorKey(Color.yellow, 0f),
                    new GradientColorKey(Color.yellow, 1f)
                },
                alphaKeys = new GradientAlphaKey[]
                {
                    new GradientAlphaKey(0.8f, 0f),
                    new GradientAlphaKey(0f, 1f)
                }
            };

            return part;
        }

        // For cumming
        /*Coroutine sprayRoutine;
        public void StartSpray()
        {
            if (sprayRoutine != null)
            {
                StopCoroutine(sprayRoutine);
            }
            else
            {
                sprayRoutine = StartCoroutine(SprayLoop());
            }
        }*/

        /*IEnumerator SprayLoop()
        {
            var emission = part.emission;
            var velocity = part.velocityOverLifetime;

            while (true)
            {
                // 🔥 RANDOM ANGLE PER BURST
                float angle = UnityEngine.Random.Range(-25f, 25f);
                Vector3 dir = Quaternion.Euler(0f, angle, 0f) * Vector3.right;

                // APPLY DIRECTION
                velocity.enabled = true;
                velocity.space = ParticleSystemSimulationSpace.World;
                velocity.x = dir.x * 10f;
                velocity.y = 0f;
                velocity.z = dir.z * 10f;

                // 🔥 STREAM ON
                emission.rateOverTime = UnityEngine.Random.Range(20f, 45f);

                yield return new WaitForSeconds(UnityEngine.Random.Range(0.15f, 0.4f));

                // 🔥 STREAM OFF (interrupt)
                emission.rateOverTime = 0f;

                yield return new WaitForSeconds(UnityEngine.Random.Range(0.1f, 0.35f));
            }
        }*/

        public static GameObject makeCum()
        {
            GameObject part = MakeParticle(cummies, cummiesvom);

            return part;

            //partman.StartSpray();
        }

        // This is flawed. I don't know what gpt is doing but I don't think I want this.
        // Problem: Particles collide with expie which makes them disappear, try to figure out a way to prevent that.
        // Possibly use CheckGround on GroundBlood class instead?
        private List<ParticleCollisionEvent> collisionEvents = new List<ParticleCollisionEvent>();

        private Vector2Int vec2int;
        private Vector2 pos;
        private static int particleCounter = 0;

        void OnParticleCollision(GameObject other)
        {
            ParticleSystem partsys = GetComponent<ParticleSystem>();

            if (partsys == null) return;

            int numEvents = partsys.GetCollisionEvents(other, collisionEvents);

            for (int i = 0; i < numEvents; i++)
            {
                var hit = collisionEvents[i];
                var col = hit.colliderComponent;

                if (col == null) continue;

                // Spawns water at the intersection point
                pos = new Vector2((int)hit.intersection.x, (int)hit.intersection.y);

                vec2int = WorldGeneration.world.WorldToBlockPos(pos);

                particleCounter++;

                if (particleCounter >= 100)
                {
                    SharedState.fm.fluid[vec2int.x, vec2int.y] = 1;
                    particleCounter = 0;
                }

                SharedState.fm.fluid[vec2int.x, vec2int.y] = 1;

                // Make it so every 5 or 10 particles is a block of fluid
                // Make it acutally piss ^

                //SpawnGroundSprite(hit.intersection);
                //var part = MakeParticle(cummies, cummiesvom);

                //groundRef(partGO.GetComponent<BleedParticle>()) = groundchild;
            }
        }


        // Find out if this function is even running...
        public static void SpawnGroundSprite(Vector2 position)
        {
            GameObject go = new GameObject("CumDecal");

            // Snap to center of tile
            Vector2Int blockPos = WorldGeneration.world.WorldToBlockPos(position);
            Vector3 snappedPos = new Vector3(blockPos.x + 0.5f, blockPos.y + 0.5f, 0f);

            go.transform.position = snappedPos;

            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = cummies;

            go.transform.localScale = Vector3.one * 0.5f;

            sr.color = new Color(1f, 1f, 1f, UnityEngine.Random.Range(0.2f, 0.8f));

            go.AddComponent<GroundBlood>();

            Plugin.Log.LogInfo("SpawnGroundSprite Triggered!");
        }
    }
}
