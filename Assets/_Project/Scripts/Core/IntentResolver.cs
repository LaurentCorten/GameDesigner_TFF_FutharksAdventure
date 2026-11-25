


using System;

namespace FutharksAdventure.Core
{
    public class IntentResolver
    {
        private readonly FloorGrid _grid;
        private readonly OccupancyMap _occupancyMap;
        private readonly EventBus _eventBus;

        public IntentResolver(FloorGrid grid, OccupancyMap occupancyMap, EventBus eventBus)
        {
            if(grid is null) throw new ArgumentNullException(nameof(grid));
            if(occupancyMap is null) throw new ArgumentNullException(nameof(occupancyMap));
            if(eventBus is null) throw new ArgumentNullException(nameof(eventBus));
            
            _grid = grid;
            _occupancyMap = occupancyMap;
            _eventBus = eventBus;
        }

        public GameAction Resolve(Creature creature, Direction direction)
        {
            if (creature is null) throw new ArgumentNullException();

            GridPosition targetPosition = creature.Position + direction.ToGridOffset();
            Entity occupant = _occupancyMap.GetEntity(targetPosition);

            if(occupant is null)
                return new MoveAction(creature, creature.Position, targetPosition, _grid, _occupancyMap, _eventBus);

            if (occupant is Creature)
                return new AttackAction(_eventBus, _occupancyMap, creature, (Creature)occupant, creature.Position, targetPosition);

            return null;
        }
    }
}
