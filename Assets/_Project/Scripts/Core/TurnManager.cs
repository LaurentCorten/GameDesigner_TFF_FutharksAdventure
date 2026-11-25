


using System;

namespace FutharksAdventure.Core
{
    public class TurnManager
    {
        private readonly Player _player;
        private readonly IntentResolver _resolver;
        private readonly EventBus _eventBus;
        private readonly DamageHandler _damageHandler;

        public TurnManager(Player player, IntentResolver resolver, EventBus eventBus, DamageHandler damageHandler)
        {
            if (player is null)
                throw new ArgumentNullException(nameof(player));
            if (resolver is null)
                throw new ArgumentNullException(nameof(resolver));
            if (eventBus is null)
                throw new ArgumentNullException(nameof(eventBus));
            if (damageHandler is null)
                throw new ArgumentNullException(nameof(damageHandler));

            _player = player;
            _resolver = resolver;
            _eventBus = eventBus;
            _damageHandler = damageHandler;
        }

        public bool SubmitMove(Direction direction)
        {
            if (_player.IsDead)
                return false;

            GameAction action = _resolver.Resolve(_player, direction);

            if (action is null || !action.TryExecute())
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
                _damageHandler.ApplyDamage(_player, 1);
        }
    }
}
