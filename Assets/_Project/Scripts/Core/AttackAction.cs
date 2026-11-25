using System;



namespace FutharksAdventure.Core
{
    public class AttackAction : GameAction
    {
        private readonly EventBus _eventBus;
        private readonly OccupancyMap _occupancyMap;
        private readonly Creature _attackingCreature;
        private readonly Creature _targetCreature;
        private readonly GridPosition _fromPosition;
        private readonly GridPosition _toPosition;

        public AttackAction(EventBus eventBus, OccupancyMap occupancyMap, Creature attackingCreature, Creature targetCreature, GridPosition fromPosition, GridPosition toPosition)
        {
            if (attackingCreature is null) throw new ArgumentNullException(nameof(attackingCreature));
            if (targetCreature is null) throw new ArgumentNullException(nameof(targetCreature));
            if (occupancyMap is null) throw new ArgumentNullException(nameof(occupancyMap));
            if (eventBus is null) throw new ArgumentNullException(nameof(eventBus));

            _eventBus = eventBus;
            _occupancyMap = occupancyMap;
            _attackingCreature = attackingCreature;
            _targetCreature = targetCreature;
            _fromPosition = fromPosition;
            _toPosition = toPosition;
        }

        public override bool CanExecute()
        {
            if (_attackingCreature.Position != _fromPosition)
                throw new InvalidOperationException("La position initiale n'est plus valide");
            if (_targetCreature.Position != _toPosition)
                throw new InvalidOperationException("La position target n'est plus valide");
            if (_targetCreature.IsDead)
                return false;

            return true;
        }

        protected override void Execute()
        {
            if (_targetCreature.IsDead)
                throw new InvalidOperationException("La cible est déjà morte, l'attaque n'aurait pas du pouvoir être relancée !");
            int damage = _attackingCreature.AttackDamage; //TEMPORARY -> sera modulé par la def plus tard
            _targetCreature.TakeDamage(damage);
            _eventBus.Publish(new HealthChanged(_targetCreature, -damage));

            if (_targetCreature.IsDead)
                if (_targetCreature is Player)
                    _eventBus.Publish(new PlayerDied());
                else _occupancyMap.Remove(_targetCreature, _toPosition);
        }
    }
}
