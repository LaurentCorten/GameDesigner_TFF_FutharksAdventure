using System;


namespace FutharksAdventure.Core
{
    public class MoveAction : GameAction
    {
        private readonly EventBus _eventBus;
        private readonly OccupancyMap _occupancyMap;
        private readonly Creature _movingCreature;
        private readonly FloorGrid _grid;
        private readonly GridPosition _fromPosition;
        private readonly GridPosition _toPosition;

        public MoveAction(Creature movingCreature, GridPosition fromPosition, GridPosition toPosition, FloorGrid floorGrid, OccupancyMap occupancyMap, EventBus eventBus)
        {
            if(movingCreature is null) throw new ArgumentNullException(nameof(movingCreature));
            if(floorGrid is null) throw new ArgumentNullException(nameof(floorGrid));
            if(occupancyMap is null) throw new ArgumentNullException(nameof(occupancyMap));
            if(eventBus is null) throw new ArgumentNullException(nameof(eventBus));

            _movingCreature = movingCreature;
            _grid = floorGrid;
            _occupancyMap = occupancyMap;
            _eventBus = eventBus;
            _fromPosition = fromPosition;
            _toPosition = toPosition;
        }

        public override bool CanExecute()
        {
            if (_movingCreature.Position != _fromPosition)
                throw new InvalidOperationException("La position initiale n'est plus valide");

            return _grid.IsWalkable(_toPosition) && !_occupancyMap.IsOccupied(_toPosition);
        }

        protected override void Execute()
        {
            _occupancyMap.Move(_movingCreature, _toPosition);
            _eventBus.Publish(new EntityMoved(_movingCreature, _fromPosition, _toPosition));
        }
    }
}
