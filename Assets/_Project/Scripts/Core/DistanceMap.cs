using System;



namespace FutharksAdventure.Core
{
    public class DistanceMap
    {
        public const int UNREACHABLE = -1;
        private readonly int[,] _distances;

        public DistanceMap(int[,] distances)
        {
            _distances = distances;
        }


        public int GetDistance(GridPosition moverPosition)
        {
            return _distances[moverPosition.X, moverPosition.Y];
        }

        public Direction GetApproachingDirections(GridPosition position)
        {
            throw new NotImplementedException();
        }
    }
}
