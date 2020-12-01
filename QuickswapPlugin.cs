using System;
using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using BepInEx;
using AdventureCore;
using UnityEngine;
using UnityEngine.UI;
using UnitySceneManager = UnityEngine.SceneManagement.SceneManager;

namespace Quickswap {
    [BepInPlugin("space.leo60228.plugins.quickswap", "Quickswap", "1.0.0.0")]
    public class QuickswapPlugin : BaseUnityPlugin {
        //private bool skippedSplash = false;

        internal static LinkedList<MoviePlayer> MoviePlayers = new LinkedList<MoviePlayer>();

        private FieldInfo f_activeFadeCoroutine = AccessTools.Field(typeof(SceneWrangler), "_activeFadeCoroutine");
        private FieldInfo f_fader = AccessTools.Field(typeof(SceneWrangler), "_fader");

        void Awake() {
            Logger.LogInfo("Quickswap awake");
            Patches.Logger = Logger;
            Harmony.CreateAndPatchAll(typeof(Patches));
        }

        void Update() {
            if (Input.GetKey("left ctrl")) {
                LinkedListNode<MoviePlayer> elem = MoviePlayers.First;
                LinkedListNode<MoviePlayer> next = null;
                while (elem != null) {
                    next = elem.Next;
                    MoviePlayer player = elem.Value;
                    if (player == null) {
                        MoviePlayers.Remove(elem);
                    } else {
                        if (!player.Loop && !player.inWorld) {
                            Logger.LogInfo("stopping video");
                            player.StopVideo();
                        } else {
                            Logger.LogInfo($"not skipping, Loop: {player.Loop}, inWorld: {player.inWorld}");
                        }
                    }
                    elem = next;
                }

                // if (!skippedSplash && GameManager.instance.Hud.State == HudState.Start && UnitySceneManager.GetActiveScene().name == "Global Module") {
                //     UnitySceneManager.LoadScene("Start Menu");
                //     skippedSplash = true;
                // }
            }
        }
    }
}
