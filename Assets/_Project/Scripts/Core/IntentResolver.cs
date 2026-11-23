


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

        public GameAction Resolve(Entity entity, Direction direction)
        {
            return new MoveAction(entity, direction, _grid, _occupancyMap, _eventBus);
        }
    }
}
