using NavalDLC.Missions.MissionLogics;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.MountAndBlade;

namespace FireRebalance
{
    public class FireRebalanceSubmodule : MBSubModuleBase
    {
        private const string HarmonyID = "firerebalance";
        protected override void OnSubModuleLoad()
        {
            base.OnSubModuleLoad();
        }

        protected override void OnSubModuleUnloaded()
        {
            base.OnSubModuleUnloaded();
        }

        public override void OnMissionBehaviorInitialize(Mission mission)
        {
            if (mission.HasMissionBehavior<NavalShipsLogic>())
            {
                if (mission.GetMissionBehavior<FireRebalanceBehavior>() == null)
                {
                    mission.AddMissionBehavior(new FireRebalanceBehavior());
                }
            }
        }

        protected override void OnGameStart(Game game, IGameStarter gameStarter)
        {
            base.OnGameStart(game, gameStarter);
            if (game.GameType is Campaign)
            {
                gameStarter.AddModel(new ModifiedNavalAgentStatCalculateModel());
            }
        }
    }
}