


using System;

namespace FutharksAdventure.Core
{
    public class TurnManager
    {
        private readonly Player _player;
        private readonly IntentResolver _resolver;
        private readonly EventBus _eventBus;

        public TurnManager(Player player, IntentResolver resolver, EventBus eventBus)
        {
            if (player is null)
                throw new ArgumentNullException(nameof(player));
            if (resolver is null)
                throw new ArgumentNullException(nameof(resolver));
            if (eventBus is null)
                throw new ArgumentNullException(nameof(eventBus));

            _player = player;
            _resolver = resolver;
            _eventBus = eventBus;
        }

        public bool SubmitMove(Direction direction)
        {
            if (_player.IsDead)
                return false;

            GameAction action = _resolver.Resolve(_player, direction);

            if (!action.TryExecute())
                return false;

            ExecuteTurn();

            return true;
        }

        private void ExecuteTurn()
        {
            ConsumeTurnCost();
            _eventBus.Publish(new TurnResolved());
        }

        private void ConsumeTurnCost()
        {
            if (_player.TryConsumeFood())
                _eventBus.Publish(new FoodChanged(_player.Food));
            else
                ConsumeHealth(_player, 1);
        }

        private void ConsumeHealth(Creature creature, int amount)
        {
            creature.TakeDamage(amount);
            _eventBus.Publish(new HealthChanged(creature, -amount));

            if (creature.IsDead && creature==_player)
                _eventBus.Publish(new PlayerDied());
        }
    }
}
