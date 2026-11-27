using System;
using FutharksAdventure.Core;
using NUnit.Framework;

namespace FutharksAdventure.Tests
{
    public class PathfinderTests
    {
        private static GridPosition P(int x, int y) => new GridPosition(x, y);

        // --- Distances ---

        [Test]
        public void ComputeDistances_Target_IsAtDistanceZero()
        {
            var map = Pathfinder.ComputeDistances(TestFloors.CreateRoom7x7(), P(4, 4));

            Assert.That(map.GetDistanceToTarget(P(4, 4)), Is.EqualTo(0));
        }

        [TestCase(4, 7, 3)] // tout droit au nord
        [TestCase(7, 7, 6)] // coin : 3 + 3
        [TestCase(1, 1, 6)] // coin oppose
        [TestCase(5, 4, 1)] // voisine directe
        public void ComputeDistances_OpenRoom_EqualsStepCount(int x, int y, int expected)
        {
            var map = Pathfinder.ComputeDistances(TestFloors.CreateRoom7x7(), P(4, 4));

            Assert.That(map.GetDistanceToTarget(P(x, y)), Is.EqualTo(expected));
        }

        [Test]
        public void ComputeDistances_Corridor_CountsSteps()
        {
            var map = Pathfinder.ComputeDistances(TestFloors.CreateCorridor3x1(), P(1, 1));

            Assert.That(map.GetDistanceToTarget(P(3, 1)), Is.EqualTo(2));
        }

        [Test]
        public void ComputeDistances_WallInTheWay_GoesAround()
        {
            // (2,1) -> (2,7) : 6, -> (6,7) : 4, -> (6,1) : 6  => 16
            var map = Pathfinder.ComputeDistances(TestFloors.CreateRoom7x7SplitByWall(), P(6, 1));

            Assert.That(map.GetDistanceToTarget(P(2, 1)), Is.EqualTo(16));
        }

        // --- Cases inaccessibles ---

        [Test]
        public void ComputeDistances_WallTile_IsUnreachable()
        {
            var map = Pathfinder.ComputeDistances(TestFloors.CreateRoom7x7SplitByWall(), P(6, 1));

            Assert.That(map.GetDistanceToTarget(P(4, 3)), Is.EqualTo(DistanceMap.UNREACHABLE));
        }

        [Test]
        public void ComputeDistances_EmptyTile_IsUnreachable()
        {
            var map = Pathfinder.ComputeDistances(TestFloors.CreateCorridor3x1(), P(1, 1));

            Assert.That(map.GetDistanceToTarget(P(0, 0)), Is.EqualTo(DistanceMap.UNREACHABLE));
        }

        [Test]
        public void ComputeDistances_UnconnectedRoom_IsUnreachable()
        {
            var map = Pathfinder.ComputeDistances(TestFloors.CreateTwoUnconnectedRooms(), P(2, 2));

            Assert.That(map.GetDistanceToTarget(P(6, 2)), Is.EqualTo(DistanceMap.UNREACHABLE));
        }

        // --- Gardes ---

        [Test]
        public void ComputeDistances_TargetOnWall_Throws()
        {
            Assert.That(() => Pathfinder.ComputeDistances(TestFloors.CreateRoom7x7(), P(0, 0)),
                Throws.TypeOf<ArgumentException>());
        }

        [Test]
        public void ComputeDistances_TargetOutsideGrid_Throws()
        {
            Assert.That(() => Pathfinder.ComputeDistances(TestFloors.CreateRoom7x7(), P(-1, -1)),
                Throws.TypeOf<ArgumentException>());
        }

        [Test]
        public void ComputeDistances_NullGrid_Throws()
        {
            Assert.That(() => Pathfinder.ComputeDistances(null, P(4, 4)),
                Throws.TypeOf<ArgumentNullException>());
        }
    }
}