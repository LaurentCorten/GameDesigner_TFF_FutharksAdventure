using System;
using System.Collections.Generic;



namespace FutharksAdventure.Core
{
    public class DistanceMap
    {
        public const int UNREACHABLE = -1;
        private readonly int[,] _distances;
        private readonly int _mapHeight;
        private readonly int _mapWidth;

        internal DistanceMap(int[,] distances)
        {
            if (distances is null) throw new ArgumentNullException(nameof(distances));

            _distances = distances;
            _mapWidth = _distances.GetLength(0);
            _mapHeight = _distances.GetLength(1);
        }

        public int GetDistanceToTarget(GridPosition fromPosition)
        {
            if (!IsInMap(fromPosition)) throw new ArgumentOutOfRangeException(nameof(fromPosition));
            
            return _distances[fromPosition.X, fromPosition.Y];
        }

        public IReadOnlyList<Direction> GetApproachingDirections(GridPosition moverPosition)
        {
            if (!IsInMap(moverPosition)) throw new ArgumentOutOfRangeException(nameof(moverPosition));
            if (_distances[moverPosition.X, moverPosition.Y] <= 0) throw new ArgumentException(nameof(moverPosition));

            List<Direction> directions= new List<Direction>(4);
            int current = _distances[moverPosition.X, moverPosition.Y];

            foreach(Direction dir in DirectionExtensions.AllDirections)
            {
                int neighborDistance = GetDistanceToTarget(moverPosition + dir.ToGridOffset());

                if (neighborDistance == UNREACHABLE || neighborDistance > current)
                    continue;

                if (neighborDistance == current - 1) directions.Add(dir);
            }

            return directions;
        }

        private bool IsInMap(GridPosition position)
        {
            return !(position.X < 0 || position.Y < 0 || position.X >= _mapWidth || position.Y >= _mapHeight);
        }
    }
}
