using System;


namespace FutharksAdventure.Core
{
    public class DamageHandler
    {
        private readonly EventBus _eventBus;
        private readonly OccupancyMap _occupancyMap;

        public DamageHandler(EventBus eventBus, OccupancyMap occupancyMap)
        {
            if (eventBus is null) throw new ArgumentNullException(nameof(eventBus));
            if (occupancyMap is null) throw new ArgumentNullException(nameof(occupancyMap));

            _eventBus = eventBus;
            _occupancyMap = occupancyMap;
        }

        public void ApplyDamage(Creature target, int amount)
        {
            if (target is null) throw new ArgumentNullException(nameof(target));
            if (target.IsDead) throw new InvalidOperationException();

            target.TakeDamage(amount);
            _eventBus.Publish(new HealthChanged(target, -amount));

            if (target.IsDead)
                if (target is Player)
                    _eventBus.Publish(new PlayerDied());
                else _occupancyMap.Remove(target);
        }
    }
}
