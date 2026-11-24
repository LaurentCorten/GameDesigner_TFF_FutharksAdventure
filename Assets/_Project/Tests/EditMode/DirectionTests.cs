using FutharksAdventure.Core;
using NUnit.Framework;
using System;

namespace FutharksAdventure.Tests
{
    public class DirectionTests
    {
        [TestCase(Direction.North, 0, 1)]
        [TestCase(Direction.East, 1, 0)]
        [TestCase(Direction.South, 0, -1)]
        [TestCase(Direction.West, -1, 0)]
        public void ToOffset_ReturnsUnitOffset(Direction direction, int x, int y)
        {
            Assert.That(direction.ToGridOffset(), Is.EqualTo(new GridPosition(x, y)));
        }

        [Test]
        public void ToGridOffset_UndefinedValue_Throws()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => ((Direction)99).ToGridOffset());
        }
    }
}