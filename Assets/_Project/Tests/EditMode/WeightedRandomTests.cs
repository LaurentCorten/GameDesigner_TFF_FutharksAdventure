using System;
using System.Collections.Generic;
using FutharksAdventure.Core;
using NUnit.Framework;

namespace FutharksAdventure.Tests
{
    public class WeightedRandomTests
    {
        // A = 2, B = 3 -> total 5 : tirages 0-1 => A, tirages 2-4 => B
        private static readonly (string Item, int Weight)[] TwoOptions =
        {
            ("A", 2),
            ("B", 3)
        };

        // --- Selection ---

        [TestCase(0)]
        [TestCase(4)]
        public void TryPick_SingleOption_AlwaysPicksIt(int roll)
        {
            var options = new[] { ("A", 5) };

            bool picked = WeightedRandom.TryPick(options, new FakeRandom(roll), out var result);

            Assert.That(picked, Is.True);
            Assert.That(result, Is.EqualTo("A"));
        }

        [Test]
        public void TryPick_DrawsWithTotalWeight()
        {
            var random = new FakeRandom(0);

            WeightedRandom.TryPick(TwoOptions, random, out _);

            Assert.That(random.LastMaxValue, Is.EqualTo(5));
        }

        [Test]
        public void TryPick_RollZero_PicksFirstOption()
        {
            WeightedRandom.TryPick(TwoOptions, new FakeRandom(0), out var result);

            Assert.That(result, Is.EqualTo("A"));
        }

        [Test]
        public void TryPick_RollTotalMinusOne_PicksLastOption()
        {
            WeightedRandom.TryPick(TwoOptions, new FakeRandom(4), out var result);

            Assert.That(result, Is.EqualTo("B"));
        }

        [Test]
        public void TryPick_RollAtEndOfFirstRange_PicksFirstOption()
        {
            WeightedRandom.TryPick(TwoOptions, new FakeRandom(1), out var result);

            Assert.That(result, Is.EqualTo("A"));
        }

        [Test]
        public void TryPick_RollAtStartOfSecondRange_PicksSecondOption()
        {
            WeightedRandom.TryPick(TwoOptions, new FakeRandom(2), out var result);

            Assert.That(result, Is.EqualTo("B"));
        }

        // --- Poids a 0 ---

        [Test]
        public void TryPick_ZeroWeightInMiddle_IsNeverPicked()
        {
            // A = 2, Z = 0, B = 3 : le tirage 2 tombe juste "apres" A, donc sur B
            var options = new[] { ("A", 2), ("Z", 0), ("B", 3) };

            WeightedRandom.TryPick(options, new FakeRandom(2), out var result);

            Assert.That(result, Is.EqualTo("B"));
        }

        [Test]
        public void TryPick_ZeroWeightFirst_IsNeverPicked()
        {
            var options = new[] { ("Z", 0), ("A", 1) };

            WeightedRandom.TryPick(options, new FakeRandom(0), out var result);

            Assert.That(result, Is.EqualTo("A"));
        }

        [Test]
        public void TryPick_ZeroWeightLast_IsNeverPicked()
        {
            var options = new[] { ("A", 1), ("Z", 0) };

            WeightedRandom.TryPick(options, new FakeRandom(0), out var result);

            Assert.That(result, Is.EqualTo("A"));
        }

        // --- Aucun choix possible ---

        [Test]
        public void TryPick_EmptyList_ReturnsFalseAndDefault()
        {
            var options = new List<(string Item, int Weight)>();

            bool picked = WeightedRandom.TryPick(options, new Random(0), out var result);

            Assert.That(picked, Is.False);
            Assert.That(result, Is.Null);
        }

        [Test]
        public void TryPick_AllWeightsZero_ReturnsFalseAndDefault()
        {
            var options = new[] { ("A", 0), ("B", 0) };

            bool picked = WeightedRandom.TryPick(options, new Random(0), out var result);

            Assert.That(picked, Is.False);
            Assert.That(result, Is.Null);
        }

        // --- Gardes ---

        [Test]
        public void TryPick_NegativeWeight_Throws()
        {
            var options = new[] { ("A", 2), ("B", -1) };

            Assert.That(() => WeightedRandom.TryPick(options, new Random(0), out _),
                Throws.TypeOf<ArgumentOutOfRangeException>());
        }

        [Test]
        public void TryPick_NullOptions_Throws()
        {
            IReadOnlyList<(string Item, int Weight)> options = null;

            Assert.That(() => WeightedRandom.TryPick(options, new Random(0), out _),
                Throws.TypeOf<ArgumentNullException>());
        }

        [Test]
        public void TryPick_NullRandom_Throws()
        {
            Assert.That(() => WeightedRandom.TryPick(TwoOptions, null, out _),
                Throws.TypeOf<ArgumentNullException>());
        }
    }
}