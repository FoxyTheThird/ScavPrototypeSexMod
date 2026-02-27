using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace ScavPrototypeSexMod.Managers
{
    public static class SexManager
    {
        public static void Initialize()
        {
            Plugin.Log.LogInfo("SexManager Initialized");
        }

        public static void ResetVals()
        {
            SharedState.HasSTD = false;
            foreach (var key in SharedState.stdTypes.Keys.ToList())
            {
                SharedState.stdTypes[key] = false;
            }
            SharedState.CondomInInventory = false;
            SharedState.WearingCondom = false;
            SharedState.Horniness = 100f;
            SharedState.Hardness = 0f;
        }

        public static void PickSTD()
        {
            var keys = SharedState.stdTypes.Keys.ToList();
            int rand = UnityEngine.Random.Range(0, keys.Count);
            string randomKey = keys[rand];

            SharedState.stdTypes[randomKey] = true;
        }

        public static IEnumerator ApplySTD(int chara)
        {
            yield return null;

            switch (chara)
            {
                case 0: // Expie
                    if (UnityEngine.Random.value <= 0.15f)
                    {

                        Plugin.Log.LogInfo("STD Time!");
                        SharedState.HasSTD = true;
                        PickSTD();
                    }
                    break;
                case 1: // Milky
                    if (UnityEngine.Random.value <= 0.35f)
                    {
                        Plugin.Log.LogInfo("STD Time!");
                        SharedState.HasSTD = true;
                        PickSTD();
                    }
                    break;
                case 2: // Dune
                    if (UnityEngine.Random.value <= 0.45f)
                    {

                        Plugin.Log.LogInfo("STD Time!");
                        SharedState.HasSTD = true;
                        PickSTD();
                    }
                    break;
                default:
                    Plugin.Log.LogError("What? Something has gone wrong.");
                    break;
            }
        }

        // For being too horny.
        public static IEnumerator HornyRoutine(PlayerCamera cam)
        {
            cam.body.talker.TalkDelayed(0.5f, "F- fuck... I need some relief soon...", null, true, false);

            float penalty;

            while (SharedState.Horniness > 75f)
            {
                SharedState.durHorny += 1f;

                float mitigation = 1f - (cam.body.skills.RESFrom10 * 0.05f);
                mitigation = Mathf.Clamp(mitigation, 0.2f, 1f);

                float h = SharedState.Horniness;
                penalty = 0f;

                if (SharedState.durHorny >= 500f)
                {
                    penalty = h * 0.015f;
                }
                else if (SharedState.durHorny >= 300f)
                {
                    penalty = h * 0.005f;
                }
                else if (SharedState.durHorny >= 100f)
                {
                    penalty = h * 0.001f;
                }

                cam.body.happiness -= penalty * mitigation;
                yield return new WaitForSeconds(1f);
            }

            SharedState.hCoroutine = null;
            SharedState.durHorny = 0f;
        }

        // The main function for trader sex
        // This is now jank city, you prolly need to work on it...
        public static IEnumerator TriggerSex(PlayerCamera cam)
        {
            Plugin.Log.LogInfo("The current shared rep is... " + SharedState.TraderReputation);
            if (SharedState.TraderReputation >= 100)
            {
                Plugin.Log.LogInfo("Trader rep check passed");

                // Checking for an STD
                if (SharedState.HasSTD)
                {
                    cam.PlayUISound(PlayerCamera.UISoundType.Deny, 1f);
                    cam.ToggleTradeMenu();
                    cam.body.talker.Talk("I don't feel in the mood for that...", null, true, false);
                    yield return new WaitForSeconds(3);
                    cam.body.talker.Talk("I think something's wrong with my body.", null, true, false);
                    yield break;
                }

                Plugin.Log.LogInfo("No STD Found!");

                // Checking if you have enough happiness
                if (cam.body.happiness < 1f)
                {
                    cam.UseFailUnhappiness();
                    cam.ToggleTradeMenu();
                    yield return new WaitForSeconds(1f);
                    cam.body.talker.Talk("What's the point...?");
                    yield break;
                }

                Plugin.Log.LogInfo("Happiness check passed");

                // Checking if you're horny enough
                if (SharedState.Horniness < 45f)
                {
                    Plugin.Log.LogInfo("Horniness check failed. Or is the incorrect gender.");
                    cam.ToggleTradeMenu();
                    cam.PlayUISound(PlayerCamera.UISoundType.Deny, 1f);
                    cam.body.talker.Talk("I'm not doing that.", null, true, false);
                    yield break;
                }

                Plugin.Log.LogInfo("Horniness is over 45.");

                // Gets all items in the players inventory, checking for a condom.
                List<Item> curinventory = cam.body.GetAllItems();
                bool condomInInv = false;

                if (curinventory.Contains(ItemManager.condom))
                {
                    condomInInv = true;

                    Plugin.Log.LogInfo("Condom detected in Inventory.");
                    cam.currentTrader.talker.Talk("We should probably use that condom you have, huh?", null, true, false);

                    cam.ToggleTradeMenu();

                    yield return null;

                    cam.radialOpen = false;
                    cam.radialMenu.gameObject.SetActive(false);
                    cam.CloseContainer();

                    yield return new WaitForSeconds(4);
                }

                // Checking if you aren't wearing a condom, or have one in your inventory
                if (!SharedState.WearingCondom)
                {
                    cam.StartCoroutine(ApplySTD(SharedState.curTrader.character));
                }
                else if (!condomInInv)
                {
                    cam.StartCoroutine(ApplySTD(SharedState.curTrader.character));
                }
                else
                {
                    // If you do have a condom, calculate the break chance for the condom.
                    SharedState.breakChance = UnityEngine.Random.value < 0.5f ? 0.15f : 0.35f;
                    if (UnityEngine.Random.value <= SharedState.breakChance)
                    {
                        Plugin.Log.LogInfo("Great! The condom didn't break.");
                    }
                    else
                    {
                        Plugin.Log.LogInfo("Condom Broke!");

                        cam.StartCoroutine(ApplySTD(SharedState.curTrader.character));
                    }
                }

                // Figure out why the radial inventory menu doesn't close when the trade menu closes.
                if (SharedState.CurrentGender != SharedState.Gender.NonBinary)
                {
                    Plugin.Log.LogInfo("Is the correct gender for this.");

                    cam.body.energy = -5f;
                    cam.body.thirst = cam.body.thirst - UnityEngine.Random.Range(0f, 25f);
                    cam.body.hunger = cam.body.hunger - UnityEngine.Random.Range(0f, 25f);
                    cam.body.happiness += UnityEngine.Random.Range(45f, 66f);
                    SharedState.curTrader.reputation += UnityEngine.Random.Range(50f, 55f);
                    cam.body.forcedSleepQuality = new Body.SleepQuality?(Body.SleepQuality.Good);
                    Plugin.Log.LogInfo(cam.body.curSleep);

                    if (cam.craftingPanel.activeSelf)
                    {
                        cam.craftingPanel.SetActive(false);
                        Plugin.Log.LogInfo("Closing crafting panel.");
                        cam.radialOpen = false;
                        cam.radialMenu.gameObject.SetActive(false);
                        cam.CloseContainer();
                    }

                    if (cam.body.conscious)
                    {
                        SharedState.Horniness = 0f;
                        SharedState.CondomInInventory = false;
                        if (SharedState.CurrentGender == SharedState.Gender.Male || SharedState.CurrentGender == SharedState.Gender.Intersex)
                        {
                            SharedState.Hardness = 0f;
                            SharedState.WearingCondom = false;
                        }

                        yield break;
                    }
                }
                else
                {
                    Plugin.Log.LogInfo("Gender check failed.");
                    cam.ToggleTradeMenu();
                    cam.PlayUISound(PlayerCamera.UISoundType.Deny, 1f);
                    cam.body.talker.Talk("Sorry, but I'm not interested.", null, true, false);

                    yield return new WaitForSeconds(5);

                    SharedState.curTrader.talker.Talk("Understandable...", null, true, false);

                    yield break;
                }
            }
            else
            {
                cam.PlayUISound(PlayerCamera.UISoundType.Deny, 1f);
                cam.ToggleTradeMenu();
                SharedState.curTrader.talker.Talk("What the hell is wrong with you?", null, true, false);
                SharedState.curTrader.reputation -= 15f;

                yield break;
            }
        }

        // The main function for masturbation within the workout list
        public static IEnumerator Masturbate(PlayerCamera cam)
        {
            Plugin.Log.LogInfo("Simulate Masturbation...");

            if (cam.body.bodyAnimator.GetBool("exercising"))
                yield break;

            SharedState.MoreWorkoutTypes workoutType = SharedState.MoreWorkoutTypes.Masturbate;
            var field = typeof(Body).GetField("exercising", BindingFlags.Instance | BindingFlags.NonPublic);

            switch (workoutType)
            {
                case SharedState.MoreWorkoutTypes.Masturbate:
                    if (cam.body.happiness > 0f && SharedState.Horniness > 0f)
                    {
                        cam.ToggleWoundView(true);

                        cam.body.bodyAnimator.SetBool("exercising", true);
                        field?.SetValue(cam.body, true);
                        cam.body.bodyAnimator.Play("ExperimentPushups");
                        cam.body.armsAnimator.Play("ArmsPushups");
                        cam.body.bodyAnimator.SetFloat("WorkoutSpeed", 1f + cam.body.skills.RESFrom10 * 0.07f);
                        cam.body.armsAnimator.SetFloat("WorkoutSpeed", 1f + cam.body.skills.RESFrom10 * 0.07f);

                        while (cam.body.bodyAnimator.GetBool("exercising"))
                        {
                            if (cam.body.rb.velocity.magnitude > 1f || !cam.body.standing || cam.body.attackCooldown > 0f)
                            {
                                cam.body.armsAnimator.StopPlayback();
                                cam.body.armsAnimator.Play("Grounded");
                                field?.SetValue(cam.body, false);
                                cam.body.bodyAnimator.SetBool("exercising", false);
                                SharedState.pc.StopCoroutine(SharedState.mCoroutine);
                                yield return null;
                            }

                            if (SharedState.Horniness > 1f)
                            {
                                SharedState.Horniness = Mathf.Max(SharedState.Horniness - Time.deltaTime * 0.25f, 0f);
                            }
                            else
                            {
                                SharedState.Hardness = Mathf.Max(SharedState.Hardness - Time.deltaTime * 0.25f, 0f);
                                cam.body.happiness += 45;

                                cam.body.bodyAnimator.SetBool("exercising", false);
                                cam.body.armsAnimator.StopPlayback();
                                cam.body.armsAnimator.Play("Grounded");
                                field?.SetValue(cam.body, false);
                                SharedState.pc.StopCoroutine(SharedState.mCoroutine);
                                break;
                            }

                            yield return null;
                        }
                    }
                    else
                    {
                        cam.ToggleWoundView(true);
                        cam.PlayUISound(PlayerCamera.UISoundType.Deny, 1f);
                        cam.UseFailUnhappiness();
                        SharedState.pc.StopCoroutine(SharedState.mCoroutine);
                        yield return null;
                    }
                    
                    break;
                default:
                    Plugin.Log.LogInfo("Something went wrong.");
                    break;
            }
        }
    }

}
