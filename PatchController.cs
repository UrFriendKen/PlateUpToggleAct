using Kitchen;
using KitchenMods;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;

namespace KitchenToggleAct
{
    public struct CActLookedAt : IComponentData, IModComponent
    {
        public Entity Entity;
    }

    public class PatchController : GenericSystemBase, IModSystem
    {
        static PatchController _instance;

        static Dictionary<int, (Entity, Entity)> _playersLookingAt = new Dictionary<int, (Entity, Entity)>();

        static Dictionary<int, Entity> _playerActHolds = new Dictionary<int, Entity>();

        EntityQuery LookedAts;
        EntityQuery ActLookedAts;

        protected override void Initialise()
        {
            base.Initialise();
            _instance = this;
            LookedAts = GetEntityQuery(new QueryHelper()
                .Any(typeof(CBeingLookedAt), typeof(CBeingActedOnBy)));
            ActLookedAts = GetEntityQuery(typeof(CPlayer), typeof(CActLookedAt));
        }

        protected override void OnUpdate()
        {
            Dictionary<int, (Entity, Entity)> tempplayersLookingAt = new Dictionary<int, (Entity, Entity)>();

            using NativeArray<Entity> lookedAtEntities = LookedAts.ToEntityArray(Allocator.Temp);
            for (int i = 0; i < lookedAtEntities.Length; i++)
            {
                List<Entity> interactors = new List<Entity>();
                if (Require(lookedAtEntities[i], out CBeingLookedAt beingLookedAt))
                    interactors.Add(beingLookedAt.Interactor);
                if (RequireBuffer(lookedAtEntities[i], out DynamicBuffer<CBeingActedOnBy> beingActedOnBys))
                {
                    for(int j = 0; j < beingActedOnBys.Length; j++)
                    {
                        interactors.Add(beingActedOnBys[j].Interactor);
                    }
                }

                foreach (Entity interactor in interactors)
                {
                    if (!Require(interactor, out CPlayer player))
                        continue;
                    tempplayersLookingAt.Add(player.ID, (interactor, lookedAtEntities[i]));
                }
            }

            foreach (KeyValuePair<int, (Entity, Entity)> prevPlayersLookingAt in _playersLookingAt)
            {
                if (!tempplayersLookingAt.TryGetValue(prevPlayersLookingAt.Key, out (Entity player, Entity appliance) playerAppliancePair) || playerAppliancePair.appliance.Index != prevPlayersLookingAt.Value.Item2.Index)
                {
                    if (Has<CActLookedAt>(prevPlayersLookingAt.Value.Item1))
                    {
                        EntityManager.RemoveComponent<CActLookedAt>(prevPlayersLookingAt.Value.Item1);
                    }
                }
            }

            _playersLookingAt = tempplayersLookingAt;


            Dictionary<int, Entity> tempPlayerActHolds = new Dictionary<int, Entity>();
            using NativeArray<Entity> playerEntities = ActLookedAts.ToEntityArray(Allocator.Temp);
            using NativeArray<CPlayer> actLookedAtPlayers = ActLookedAts.ToComponentDataArray<CPlayer>(Allocator.Temp);
            for (int i = 0; i < actLookedAtPlayers.Length; i++)
            {
                if (!tempPlayerActHolds.ContainsKey(actLookedAtPlayers[i].ID))
                    tempPlayerActHolds.Add(actLookedAtPlayers[i].ID, playerEntities[i]);
            }
            _playerActHolds = tempPlayerActHolds;
        }

        // Return true if InteractionAction hold is enabled
        internal static bool OnActPressed(int playerID)
        {
            if (_playersLookingAt.TryGetValue(playerID, out (Entity player, Entity appliance) playerLookAtAppliance))
            {
                if (_instance != null)
                {
                    if (!_instance.Has<CActLookedAt>(playerLookAtAppliance.player))
                    {
                        _instance.Set(playerLookAtAppliance.player, new CActLookedAt()
                        {
                            Entity = playerLookAtAppliance.appliance
                        });
                        return true;
                    }
                }
            }

            if (_playerActHolds.TryGetValue(playerID, out Entity playerEnt))
            {
                _instance.EntityManager.RemoveComponent<CActLookedAt>(playerEnt);
            }


            return false;
        }
        internal static bool OnGrabPressed(int playerID)
        {
            if (_playerActHolds.TryGetValue(playerID, out Entity playerEnt))
            {
                _instance.EntityManager.RemoveComponent<CActLookedAt>(playerEnt);
            }
            return false;
        }

        // Return true if should update InteractAction button state to held.
        internal static bool ShouldActHold(int playerID)
        {
            return _playerActHolds.ContainsKey(playerID);
        }
    }
}
