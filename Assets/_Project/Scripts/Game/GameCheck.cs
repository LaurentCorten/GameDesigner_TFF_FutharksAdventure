using UnityEngine;
using FutharksAdventure.Core;


namespace FutharksAdventure.Game
{
    public class GameCheck : MonoBehaviour
    {
        private EventBus _eventBus;
        private Player _player;

        public void Init(EventBus eventBus, Player player)
        {
            _player = player;
            _eventBus = eventBus;
            _eventBus.Subscribe<EntityMoved>(OnEntityMoved);
            _eventBus.Subscribe<FoodChanged>(OnFoodChanged);
            _eventBus.Subscribe<HealthChanged>(OnHealthChanged);
            _eventBus.Subscribe<TurnResolved>(OnTurnResolved);
            _eventBus.Subscribe<PlayerDied>(OnPlayerDied);
        }

        private void OnDestroy()
        {
            _eventBus?.Unsubscribe<EntityMoved>(OnEntityMoved);
            _eventBus?.Unsubscribe<FoodChanged>(OnFoodChanged);
            _eventBus?.Unsubscribe<HealthChanged>(OnHealthChanged);
            _eventBus?.Unsubscribe<TurnResolved>(OnTurnResolved);
            _eventBus?.Unsubscribe<PlayerDied>(OnPlayerDied);
        }

        //--- CallBacks --
        private void OnEntityMoved(EntityMoved evt)
        {
            Debug.Log($"{evt.Entity.GetType()} goes from {evt.From} to {evt.To}");
        }

        private void OnFoodChanged(FoodChanged evt)
        {
            Debug.Log($"Food reduced now at {evt.Current}");

        }

        private void OnHealthChanged(HealthChanged evt)
        {
            Debug.Log($"Health of {evt.Creature.GetType()} change by {evt.Amount}");

        }

        private void OnPlayerDied(PlayerDied evt)
        {
            Debug.Log("PlayerDied");

        }

        private void OnTurnResolved(TurnResolved resolved)
        {
            Debug.Log($"TurnResolved : Player has now {_player.CurrentHealth}/{_player.MaxHealth} PV, {_player.Food} pieces of food and {_player.Hatchets} hatchets.");
        }
    }
}
