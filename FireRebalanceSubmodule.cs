using TaleWorlds.Library;
using HarmonyLib;
using NavalDLC.Missions.MissionLogics;
using TaleWorlds.MountAndBlade;
using TaleWorlds.Localization;

namespace FireRebalance
{
    public class FireRebalanceSubmodule : MBSubModuleBase
    {
        private Harmony _harmony;
        public static NavalAgentsLogic _navalAgentsLogic;
        private const string HarmonyID = "firerebalance";
        protected override void OnSubModuleLoad()
        {
            base.OnSubModuleLoad();
            _harmony = new Harmony(HarmonyID);
            Harmony.DEBUG = true;
            _harmony.PatchAll();
            
        }

        protected override void OnSubModuleUnloaded()
        {
            base.OnSubModuleUnloaded();
            _harmony?.UnpatchAll(HarmonyID);
        }

        public override void OnMissionBehaviorInitialize(Mission mission)
        {
            _navalAgentsLogic = mission.GetMissionBehavior<NavalAgentsLogic>();
        }
    }
}