using System;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using NavalDLC.Missions.MissionLogics;
using NavalDLC.Missions.Objects;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;

namespace FireRebalance
{
    public class FireRebalanceBehavior : MissionBehavior
    {
        private NavalAgentsLogic _navalAgentsLogic;
        private float _fireTickAccumulator;
        private const float FireTickInterval = 0.5f;
        public class FireState
        {
            public float LastFireDamage = 0F;
        }
        public static ConditionalWeakTable<MissionShip, FireState> LastFireDamage = new();
        public static FireState GetState(MissionShip ship)
        {
            return LastFireDamage.GetOrCreateValue(ship);
        }
        
        public static readonly FieldInfo NextFireHitPointRestoreTimeField =
            typeof(MissionShip).GetField("_nextFireHitPointRestoreTime", BindingFlags.Instance | BindingFlags.NonPublic);
        
        public static readonly PropertyInfo FireHPProp =
            typeof(MissionShip).GetProperty("FireHitPoints",
                BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);

        public static readonly MethodInfo FireHPSetter =
            FireHPProp?.GetSetMethod(true);
        
        public override MissionBehaviorType BehaviorType => MissionBehaviorType.Other;

        public override void OnBehaviorInitialize()
        {
            base.OnBehaviorInitialize();
            var navalLogic = Mission.Current.GetMissionBehavior<NavalShipsLogic>();
            _navalAgentsLogic = Mission.Current.GetMissionBehavior<NavalAgentsLogic>();
            navalLogic.ShipHitEvent += OnShipHit;
            foreach (var missionObject in Mission.Current.ActiveMissionObjects.OfType<MissionShip>())
            {
                NextFireHitPointRestoreTimeField?.SetValue(missionObject, float.MaxValue);
            }
        }

        protected override void OnEndMission()
        {
            var navalLogic = Mission.Current.GetMissionBehavior<NavalShipsLogic>();
            navalLogic.ShipHitEvent -= OnShipHit;
        }

        public override void OnMissionTick(float dt)
        {
            _fireTickAccumulator += dt;
            if (_fireTickAccumulator < FireTickInterval)
                return;
            
            _fireTickAccumulator -= FireTickInterval;

            if (Mission.Current == null || _navalAgentsLogic == null)
                return;

            foreach (var missionObject in Mission.Current.ActiveMissionObjects.OfType<MissionShip>())
            {

                float fireHPMax = missionObject.HitPoints;

                if (missionObject.FireHitPoints >= fireHPMax)
                {
                    FireHPSetter?.Invoke(missionObject, new object[] { fireHPMax });
                    continue;
                }

                var state = GetState(missionObject);
                if (Mission.Current.CurrentTime - state.LastFireDamage < 8f)
                    continue;

                int crewCount = _navalAgentsLogic.GetActiveAgentCountOfShip(missionObject);

                float crewMax = missionObject.CrewSizeOnMainDeck;
                float crewMult = MathF.Pow(crewCount / crewMax, 2f);
                
                float regenPerSecond = missionObject.FireHitPoints * 0.005f * crewMult;
                float newFireHP = MathF.Min(
                    missionObject.FireHitPoints + (regenPerSecond * FireTickInterval),
                    fireHPMax
                );

                FireHPSetter?.Invoke(missionObject, new object[] { newFireHP });
            }
        }

        private void OnShipHit(MissionShip ship, Agent attacker, int damage, 
            Vec3 impactPosition, Vec3 impactDirection, MissionWeapon weapon, int affectorWeaponSlotOrMissileIndex)
        {
            if (weapon.CurrentUsageItem != null && weapon.CurrentUsageItem.WeaponFlags.HasAnyFlag<WeaponFlags>(WeaponFlags.Burning))
            {
                GetState(ship).LastFireDamage = Mission.Current.CurrentTime;
            }
        }
    }
}