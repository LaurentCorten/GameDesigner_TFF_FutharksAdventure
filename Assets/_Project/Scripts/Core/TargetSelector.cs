using System;



namespace FutharksAdventure.Core
{
    public class TargetSelector
    {
        private readonly FloorGrid _floorGrid;
        private readonly OccupancyMap _occupancyMap;
        private Creature _lastTarget;

        public TargetSelector(FloorGrid floorGrid, OccupancyMap occupancyMap)
        {
            if (floorGrid is null) throw new ArgumentNullException(nameof(floorGrid));
            if (occupancyMap is null) throw new ArgumentNullException(nameof(occupancyMap));

            _floorGrid = floorGrid;
            _occupancyMap = occupancyMap;
        }

        public Creature FindTarget(Creature thrower)
        {

        }

        public void RememberTarget(Creature creature)
        {
            if (creature is null) throw new ArgumentNullException(nameof(creature));

            _lastTarget = creature;
        }
    }
}
