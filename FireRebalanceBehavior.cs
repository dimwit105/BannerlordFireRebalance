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
        
        //Reflection garbage:
        
        public static readonly FieldInfo NextFireHitPointRestoreTimeField =
            typeof(MissionShip).GetField("_nextFireHitPointRestoreTime", BindingFlags.Instance | BindingFlags.NonPublic);
        
        
        private delegate void FireHPSetterDelegate(MissionShip ship, float value);
        private static readonly FireHPSetterDelegate FireHPSetter;

        
        public override MissionBehaviorType BehaviorType => MissionBehaviorType.Other;

        static FireRebalanceBehavior()
        {
            var fireHpProp = typeof(MissionShip).GetProperty(
                "FireHitPoints",
                BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
            
            var setter = fireHpProp?.GetSetMethod(true);

            if (setter != null)
            {
                FireHPSetter = (FireHPSetterDelegate)
                    Delegate.CreateDelegate(typeof(FireHPSetterDelegate), setter);
            }
        }
        //End of reflection garbage

        public override void OnBehaviorInitialize()
        {
            base.OnBehaviorInitialize();
            var navalLogic = Mission.Current.GetMissionBehavior<NavalShipsLogic>();
            if (navalLogic == null)
            {
                Debug.Print("[FireRebalance] Not a naval mission! Self-Removing!");
                Mission.Current.RemoveMissionBehavior(this);
                return;
            }
            _navalAgentsLogic = Mission.Current.GetMissionBehavior<NavalAgentsLogic>();
            navalLogic.ShipHitEvent += OnShipHit;
            navalLogic.ShipSpawnedEvent += OnShipSpawned;
            Debug.Print("[FireRebalance] Behavior initialized!");
        }

        private void OnShipSpawned(MissionShip ship)
        {
            NextFireHitPointRestoreTimeField?.SetValue(ship, float.MaxValue);
            FireHPSetter(ship, ship.HitPoints);
            //InformationManager.DisplayMessage(new InformationMessage($"{ship.ShipOrigin.Name} : has {ship.FireHitPoints} out of {ship.MaxFireHealth} fhp", new Color(1F,0.5F,0.0F)));
        }

        protected override void OnEndMission()
        {
            base.OnEndMission();
            var navalLogic = Mission.Current.GetMissionBehavior<NavalShipsLogic>();
            navalLogic.ShipHitEvent -= OnShipHit;
            navalLogic.ShipSpawnedEvent -= OnShipSpawned;
        }

        public override void OnMissionTick(float dt)
        {
            if (Mission.Current == null || _navalAgentsLogic == null)
                return;
            
            _fireTickAccumulator += dt;
            if (_fireTickAccumulator < FireTickInterval)
                return;
            
            _fireTickAccumulator -= FireTickInterval;

            foreach (var missionObject in Mission.Current.ActiveMissionObjects.OfType<MissionShip>())
            {

                float fireHPMax = missionObject.HitPoints;

                if (missionObject.FireHitPoints >= fireHPMax)
                {
                    FireHPSetter(missionObject, fireHPMax);
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
                //InformationManager.DisplayMessage(new InformationMessage($"{missionObject.ShipOrigin.Name} : has {newFireHP} out of {missionObject.MaxFireHealth} fhp. Regen: {regenPerSecond}", new Color(0.1F,1,0.1F)));
                
                FireHPSetter(missionObject, newFireHP);
            }
        }

        private void OnShipHit(MissionShip ship, Agent attacker, int damage, 
            Vec3 impactPosition, Vec3 impactDirection, MissionWeapon weapon, int affectorWeaponSlotOrMissileIndex)
        {
            //InformationManager.DisplayMessage(new InformationMessage($"{ship.ShipOrigin.Name} : has {ship.FireHitPoints} out of {ship.MaxFireHealth} fhp", new Color(1F,0.5F,0.0F)));

            if (weapon.CurrentUsageItem != null && weapon.CurrentUsageItem.WeaponFlags.HasAnyFlag<WeaponFlags>(WeaponFlags.Burning))
            {
                GetState(ship).LastFireDamage = Mission.Current.CurrentTime;
                if (ship.FireHitPoints <= 0)
                {
                    ship.DealDamage(damage, (MissionShip) null, out int _, out int _, out DamageTypes _, out bool _);
                }
            }
        }
    }
}