using System;
using FutharksAdventure.Core;
using NUnit.Framework;

namespace FutharksAdventure.Tests
{
    public class PlayerTests
    {
        // --- Constructeur ---

        [Test]
        public void Constructor_SetsStartingValues()
        {
            var player = new Player(17, 150);

            Assert.That(player.MaxHealth, Is.EqualTo(17));
            Assert.That(player.CurrentHealth, Is.EqualTo(17));
            Assert.That(player.Food, Is.EqualTo(150));
            Assert.That(player.IsDead, Is.False);
        }

        [TestCase(0)]
        [TestCase(-1)]
        public void Constructor_MaxHealthNotPositive_Throws(int maxHealth)
        {
            Assert.That(() => new Player(maxHealth, 150),
                Throws.TypeOf<ArgumentOutOfRangeException>());
        }

        [Test]
        public void Constructor_NegativeFood_Throws()
        {
            Assert.That(() => new Player(17, -1),
                Throws.TypeOf<ArgumentOutOfRangeException>());
        }

        [Test]
        public void Constructor_ZeroFood_IsAllowed()
        {
            Assert.That(() => new Player(17, 0), Throws.Nothing);
        }

        // --- TakeDamage (Creature) ---

        [Test]
        public void TakeDamage_ReducesCurrentHealth()
        {
            var player = new Player(17, 150);

            player.TakeDamage(5);

            Assert.That(player.CurrentHealth, Is.EqualTo(12));
            Assert.That(player.MaxHealth, Is.EqualTo(17));
        }

        [Test]
        public void TakeDamage_Zero_ChangesNothing()
        {
            var player = new Player(17, 150);

            player.TakeDamage(0);

            Assert.That(player.CurrentHealth, Is.EqualTo(17));
        }

        [Test]
        public void TakeDamage_Negative_Throws()
        {
            var player = new Player(17, 150);

            Assert.That(() => player.TakeDamage(-1),
                Throws.TypeOf<ArgumentOutOfRangeException>());
        }

        [Test]
        public void TakeDamage_ExactlyCurrentHealth_IsDead()
        {
            var player = new Player(17, 150);

            player.TakeDamage(17);

            Assert.That(player.CurrentHealth, Is.EqualTo(0));
            Assert.That(player.IsDead, Is.True);
        }

        [Test]
        public void TakeDamage_MoreThanCurrentHealth_ClampsAtZero()
        {
            var player = new Player(17, 150);

            player.TakeDamage(50);

            Assert.That(player.CurrentHealth, Is.EqualTo(0));
            Assert.That(player.IsDead, Is.True);
        }

        // --- TryConsumeFood ---

        [Test]
        public void TryConsumeFood_WithFood_RemovesOneAndReturnsTrue()
        {
            var player = new Player(17, 150);

            bool consumed = player.TryConsumeFood();

            Assert.That(consumed, Is.True);
            Assert.That(player.Food, Is.EqualTo(149));
        }

        [Test]
        public void TryConsumeFood_WithoutFood_ReturnsFalseAndStaysAtZero()
        {
            var player = new Player(17, 0);

            bool consumed = player.TryConsumeFood();

            Assert.That(consumed, Is.False);
            Assert.That(player.Food, Is.EqualTo(0));
        }
    }
}