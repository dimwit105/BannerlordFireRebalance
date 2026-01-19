using NavalDLC.GameComponents;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;

namespace FireRebalance
{
    public class ModifiedNavalAgentStatCalculateModel : NavalAgentStatCalculateModel
    {
        public override float GetBreatheHoldMaxDuration(Agent agent, float baseBreatheHoldMaxDuration)
        {
            CharacterObject character = agent.Character as CharacterObject;
            if (character == null) { return 0F;}
            float encumbrance = agent.AgentDrivenProperties.ArmorEncumbrance;
            float total = 60F;
            if (character.IsMariner)
            {
                encumbrance /= 2;
                return MathF.Max(total - encumbrance, 30F);
            }
            
            total -= 20F;
            return MathF.Max(total - encumbrance, 5F);
        }
    }
}