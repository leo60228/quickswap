using System;
using System.Reflection;
using BepInEx.Logging;
using HarmonyLib;
using UnityEngine;
using AdventureCore;

namespace Quickswap {
    public class Patches {
        private static FieldInfo f_advanceTimeoutTimer = AccessTools.Field(typeof(ConversationUi), "_advanceTimeoutTimer");

        internal static ManualLogSource Logger;

        [HarmonyPatch(typeof(Party), "WalkToPointer")]
        [HarmonyPrefix]
        private static void PartyWalkToPointer(Hero[] ___PartyMembers, Hero ___Leader) {
            if (!Input.GetKey("left ctrl") || !___Leader.CanMove) {
                return;
            }

            Action start = null;
            start = () => {
                ___Leader.PolyNavAgent.OnNavigationStarted -= start;

                float oldSpeed = ___Leader.PolyNavAgent.speedByScale;
                if (oldSpeed > 15.0f) {
                    return;
                }
                foreach (Hero member in ___PartyMembers) {
                    member.PolyNavAgent.speedByScale = 25.0f;
                }
                ___Leader.PolyNavAgent.speedByScale = 25.0f;
                Action reset = null;
                reset = () => {
                    ___Leader.PolyNavAgent.OnDestinationReached -= reset;
                    ___Leader.PolyNavAgent.OnDestinationInvalid -= reset;

                    foreach (Hero member in ___PartyMembers) {
                        member.PolyNavAgent.speedByScale = oldSpeed;
                    }
                    ___Leader.PolyNavAgent.speedByScale = oldSpeed;
                };
                ___Leader.PolyNavAgent.OnDestinationReached += reset;
                ___Leader.PolyNavAgent.OnDestinationInvalid += reset;
            };
            ___Leader.PolyNavAgent.OnNavigationStarted += start;
        }

        [HarmonyPatch(typeof(Hud), "Update")]
        [HarmonyPrefix]
        private static void HudUpdate(Hud __instance, ConversationUi ____activeConversationUi) {
            if (!Input.GetKey("left ctrl")) {
                return;
            }

            if (__instance.State == HudState.Conversation && ____activeConversationUi != null && !____activeConversationUi.IsDialogChoiceActive) {
                f_advanceTimeoutTimer.SetValue(____activeConversationUi, 0f);
                ____activeConversationUi.OnAdvanceLineClick();
            } else if (__instance.State == HudState.Message) {
                __instance.AdvanceMessage();
            }
        }

        [HarmonyPatch(typeof(MoviePlayer), "OnEnable")]
        [HarmonyPrefix]
        private static void MoviePlayerOnEnable(MoviePlayer __instance) {
            QuickswapPlugin.MoviePlayers.AddLast(__instance);
        }
    }
}
