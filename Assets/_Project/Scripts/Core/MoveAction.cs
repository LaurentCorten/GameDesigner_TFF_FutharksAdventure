using System;


namespace FutharksAdventure.Core
{
    public class MoveAction : GameAction
    {
        private readonly Entity _entity;
        private readonly FloorGrid _grid;
        private readonly OccupancyMap _occupancyMap;
        private readonly EventBus _eventBus;
        private readonly GridPosition _fromPosition;
        private readonly GridPosition _toPosition;

        public MoveAction(Entity entity, Direction direction, FloorGrid floorGrid, OccupancyMap occupancyMap, EventBus eventBus)
        {
            _entity = entity;
            _grid = floorGrid;
            _occupancyMap = occupancyMap;
            _eventBus = eventBus;
            _fromPosition = entity.Position;
            _toPosition = _fromPosition + direction.ToGridOffset();
        }

        public override bool CanExecute() => _grid.IsWalkable(_toPosition) && !_occupancyMap.IsOccupied(_toPosition);

        protected override void Execute()
        {
            if (_entity.Position != _fromPosition)
                throw new InvalidOperationException("La position initiale n'est plus valide");
            
            _occupancyMap.Move(_entity, _toPosition);
            _eventBus.Publish(new EntityMoved(_entity, _fromPosition, _toPosition));
        }
    }
}
