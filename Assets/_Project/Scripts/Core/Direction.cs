using System;
using System.Collections.Generic;

namespace FutharksAdventure.Core
{
    public enum Direction
    {
        North,
        East,
        South,
        West
    }

    public static class DirectionExtensions
    {
        public static readonly IReadOnlyList<Direction> AllDirections = Array.AsReadOnly((Direction[])Enum.GetValues(typeof(Direction)));

        public static GridPosition ToGridOffset(this Direction direction)
        {
            return direction switch
            {
                Direction.North => new GridPosition(0, 1),
                Direction.East => new GridPosition(1, 0),
                Direction.South => new GridPosition(0, -1),
                Direction.West => new GridPosition(-1, 0),
                _ => throw new ArgumentOutOfRangeException(nameof(direction), direction, null)
            };
        }
    }
}
