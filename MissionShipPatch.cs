using System;
using System.Reflection;
using System.Runtime.CompilerServices;
using HarmonyLib;
using NavalDLC;
using NavalDLC.Missions.MissionLogics;
using NavalDLC.Missions.Objects;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;

namespace FireRebalance
{

    [HarmonyPatch]
    public class MissionShipPatch
    {
       
        public class FireState
        {
            public float LastFireDamage = 0F;
        }
        public static ConditionalWeakTable<MissionShip, FireState> LastFireDamage = new();
        public static FireState GetState(MissionShip ship)
        {
            return LastFireDamage.GetOrCreateValue(ship);
        }
        
        [HarmonyPatch(typeof(MissionShip), "DealFireDamage")]
        [HarmonyPostfix]
        static void Postfix_DealFireDamage(MissionShip __instance, float fireDamage)
        {
            if (Mission.Current == null) return;
            GetState(__instance).LastFireDamage = Mission.Current.CurrentTime;
            InformationManager.DisplayMessage(new InformationMessage(
                $"{__instance.ShipOrigin.Name} : {__instance.FireHitPoints} / {__instance.HitPoints}", new Color(1, 1, 1)));
        }
        [HarmonyPatch(typeof(MissionShip), "InitForMission")]
        [HarmonyPostfix]
        static void Postfix_OnInit(MissionShip __instance, int shipIndex,
            ulong shipUniqueBitwiseID,
            ShipAssignment shipAssignment,
            NavalShipsLogic shipsLogic)
        {
            MissionShip_OnTick_Patch.FireHPSetter.Invoke(__instance, new object[] { __instance.HitPoints});
        }
    }
    [HarmonyPatch]
    public class MissionShip_OnTick_Patch
    {
        public static readonly PropertyInfo FireHPProp =
            typeof(MissionShip).GetProperty("FireHitPoints",
                BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);

        public static readonly MethodInfo FireHPSetter =
            FireHPProp?.GetSetMethod(true);

        static MethodBase TargetMethod()
        {
            return typeof(MissionShip).GetMethod("OnTick", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        }

        static void Prefix(MissionShip __instance, float dt)
        {
            var nextFireHPField = typeof(MissionShip).GetField("_nextFireHitPointRestoreTime", BindingFlags.Instance | BindingFlags.NonPublic);
            nextFireHPField?.SetValue(__instance, float.MaxValue);
        }

        static void Postfix(MissionShip __instance, float dt)
        {
            if (__instance == null || Mission.Current == null) return;
            if (FireRebalanceSubmodule._navalAgentsLogic == null) return;

            var state = MissionShipPatch.GetState(__instance);
            if (Mission.Current.CurrentTime - state.LastFireDamage < 8f) return;

            float fireHPMax = __instance.HitPoints;
            if (__instance.FireHitPoints >= fireHPMax)
            {
                FireHPSetter?.Invoke(__instance, new object[] { fireHPMax });
                return;
            }

            int crewCount = FireRebalanceSubmodule._navalAgentsLogic.GetActiveAgentCountOfShip(__instance);
            float crewMax = __instance.CrewSizeOnMainDeck;
            float crewMult = (float)Math.Pow(crewCount/crewMax, 2);

            float regenPerSecond = __instance.FireHitPoints * 0.005F * crewMult;
            float newFireHP = MathF.Min(__instance.FireHitPoints + regenPerSecond * dt, fireHPMax);
            if (newFireHP > __instance.FireHitPoints)
            {
                InformationManager.DisplayMessage(new InformationMessage(
                    $"{__instance.ShipOrigin.Name} : Healed: {regenPerSecond*dt} with a base of {__instance.FireHitPoints*0.005F} and a crew multiplier of {crewMult}", new Color(1, 1, 1)));
            }

            FireHPSetter?.Invoke(__instance, new object[] { newFireHP });
        }
    }
}