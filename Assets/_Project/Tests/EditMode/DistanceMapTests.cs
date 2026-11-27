using System;
using FutharksAdventure.Core;
using NUnit.Framework;

namespace FutharksAdventure.Tests
{
    public class DistanceMapTests
    {
        private const int U = DistanceMap.UNREACHABLE;

        private static GridPosition P(int x, int y) => new GridPosition(x, y);

        /// <summary>
        /// Construit une carte à partir de lignes lisibles à l'écran :
        /// la première ligne est le haut (y max), la dernière est le bas (y = 0).
        /// </summary>
        private static DistanceMap FromRows(params int[][] rows)
        {
            int height = rows.Length;
            int width = rows[0].Length;
            var distances = new int[width, height];

            for (int y = 0; y < height; y++)
                for (int x = 0; x < width; x++)
                    distances[x, y] = rows[height - 1 - y][x];

            return new DistanceMap(distances);
        }

        // 5x5, cible en (2,2), entourée de cases inaccessibles
        private static DistanceMap RoomMap() => FromRows(
            new[] { U, U, U, U, U },   // y = 4
            new[] { U, 2, 1, 2, U },   // y = 3
            new[] { U, 1, 0, 1, U },   // y = 2
            new[] { U, 2, 1, 2, U },   // y = 1
            new[] { U, U, U, U, U });  // y = 0

        // 5x3, couloir horizontal, cible en (1,1)
        private static DistanceMap CorridorMap() => FromRows(
            new[] { U, U, U, U, U },   // y = 2
            new[] { U, 0, 1, 2, U },   // y = 1
            new[] { U, U, U, U, U });  // y = 0

        // --- GetDistanceToTarget ---

        [TestCase(2, 2, 0)]
        [TestCase(2, 3, 1)]
        [TestCase(1, 3, 2)]
        public void GetDistanceToTarget_ReturnsStoredValue(int x, int y, int expected)
        {
            Assert.That(RoomMap().GetDistanceToTarget(P(x, y)), Is.EqualTo(expected));
        }

        [Test]
        public void GetDistanceToTarget_UnreachableTile_ReturnsUnreachable()
        {
            Assert.That(RoomMap().GetDistanceToTarget(P(0, 0)), Is.EqualTo(U));
        }

        [TestCase(-1, 0)]
        [TestCase(0, -1)]
        [TestCase(5, 0)]
        [TestCase(0, 3)]
        public void GetDistanceToTarget_OutsideMap_Throws(int x, int y)
        {
            Assert.That(() => CorridorMap().GetDistanceToTarget(P(x, y)),
                Throws.TypeOf<ArgumentOutOfRangeException>());
        }

        // --- GetApproachingDirections : directions renvoyées ---

        [Test]
        public void GetApproachingDirections_StraightLine_SingleDirection()
        {
            // (3,1) est à 2 : seule la voisine Ouest (2,1) est à 1
            Assert.That(CorridorMap().GetApproachingDirections(P(3, 1)),
                Is.EquivalentTo(new[] { Direction.West }));
        }

        [Test]
        public void GetApproachingDirections_Corner_TwoDirections()
        {
            // (1,3) est à 2 : Est (2,3) et Sud (1,2) sont à 1
            Assert.That(RoomMap().GetApproachingDirections(P(1, 3)),
                Is.EquivalentTo(new[] { Direction.East, Direction.South }));
        }

        [Test]
        public void GetApproachingDirections_NeverReturnsFartherNeighbor()
        {
            // (2,1) est à 1 : Ouest (1,1) est à 0, Est (3,1) est à 2 et ne doit pas sortir
            Assert.That(CorridorMap().GetApproachingDirections(P(2, 1)),
                Is.EquivalentTo(new[] { Direction.West }));
        }

        [Test]
        public void GetApproachingDirections_AtDistanceOne_PointsToTarget()
        {
            // (2,3) est à 1 : la seule voisine à 0 est la cible, au Sud
            Assert.That(RoomMap().GetApproachingDirections(P(2, 3)),
                Is.EquivalentTo(new[] { Direction.South }));
        }

        // --- GetApproachingDirections : erreurs ---
        [Test]
        public void GetApproachingDirections_FromUnreachableTile_Throws()
        {
            // (3,3) est inaccessible, alors que ses voisines (2,3) et (3,2) sont à 1
            var map = FromRows(
                new[] { U, U, U, U, U },
                new[] { U, 2, 1, U, U },
                new[] { U, 1, 0, 1, U },
                new[] { U, 2, 1, 2, U },
                new[] { U, U, U, U, U });

            Assert.That(() => map.GetApproachingDirections(P(3, 3)),
                Throws.TypeOf<ArgumentException>());
        }


        [Test]
        public void GetApproachingDirections_OnTarget_Throws()
        {
            Assert.That(() => RoomMap().GetApproachingDirections(P(2, 2)),
                Throws.TypeOf<ArgumentException>());
        }

        [TestCase(-1, 2)]
        [TestCase(5, 2)]
        public void GetApproachingDirections_FromOutsideMap_Throws(int x, int y)
        {
            Assert.That(() => RoomMap().GetApproachingDirections(P(x, y)),
                Throws.TypeOf<ArgumentOutOfRangeException>());
        }

        [Test]
        public void GetApproachingDirections_ReachableTileOnBorder_Throws()
        {
            // (0,1) est accessible (distance 1) mais au bord : sa voisine Ouest (-1,1) est hors carte.
            // Le générateur entoure toujours le sol de murs : ce cas révèle un bug.
            var map = FromRows(
                new[] { U, U, U },
                new[] { 1, 0, U },
                new[] { U, U, U });

            Assert.That(() => map.GetApproachingDirections(P(0, 1)),
                Throws.TypeOf<ArgumentOutOfRangeException>());
        }

        // --- Gardes ---

        [Test]
        public void Constructor_NullArray_Throws()
        {
            Assert.That(() => new DistanceMap(null), Throws.TypeOf<ArgumentNullException>());
        }
    }
}