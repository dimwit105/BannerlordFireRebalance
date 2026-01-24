using TaleWorlds.Library;
using NavalDLC.Missions.MissionLogics;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.MountAndBlade;

namespace FireRebalance
{
    public class FireRebalanceSubmodule : MBSubModuleBase
    {
        protected override void OnSubModuleLoad()
        {
            base.OnSubModuleLoad();
        }

        protected override void OnSubModuleUnloaded()
        {
            base.OnSubModuleUnloaded();
        }

        public override void OnBeforeMissionBehaviorInitialize(Mission mission)
        {
            if (mission.GetMissionBehavior<FireRebalanceBehavior>() == null)
            {
                Debug.Print("[FireRebalance] Fire behavior added!");
                mission.AddMissionBehavior(new FireRebalanceBehavior());
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