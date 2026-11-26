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
            var player = new Player(17, 150, 3, 6, 4);

            Assert.That(player.MaxHealth, Is.EqualTo(17));
            Assert.That(player.CurrentHealth, Is.EqualTo(17));
            Assert.That(player.Food, Is.EqualTo(150));
            Assert.That(player.Hatchets, Is.EqualTo(3));
            Assert.That(player.VisionRange, Is.EqualTo(6));
            Assert.That(player.IsDead, Is.False);
            Assert.That(player.AttackDamage, Is.EqualTo(4));
        }

        [TestCase(0)]
        [TestCase(-1)]
        public void Constructor_MaxHealthNotPositive_Throws(int maxHealth)
        {
            Assert.That(() => new Player(maxHealth, 150, 3, 6, 4),
                Throws.TypeOf<ArgumentOutOfRangeException>());
        }

        [Test]
        public void Constructor_NegativeFood_Throws()
        {
            Assert.That(() => new Player(17, -1, 3, 6, 4),
                Throws.TypeOf<ArgumentOutOfRangeException>());
        }

        [Test]
        public void Constructor_ZeroFood_IsAllowed()
        {
            Assert.That(() => new Player(17, 0, 3, 6, 4), Throws.Nothing);
        }

        [Test]
        public void Constructor_NegativeHatchets_Throws()
        {
            Assert.That(() => new Player(17, 150, -1, 6, 4),
                Throws.TypeOf<ArgumentOutOfRangeException>());
        }

        [Test]
        public void Constructor_ZeroHatchets_IsAllowed()
        {
            Assert.That(() => new Player(17, 150, 0, 6, 4), Throws.Nothing);
        }

        [TestCase(0)]
        [TestCase(-1)]
        public void Constructor_VisionRangeNotPositive_Throws(int visionRange)
        {
            Assert.That(() => new Player(17, 150, 3, visionRange, 4),
                Throws.TypeOf<ArgumentOutOfRangeException>());
        }

        [Test]
        public void Constructor_NegativeAttackDamage_Throws()
        {
            Assert.That(() => new Player(17, 150, 3, 6, -1),
                Throws.TypeOf<ArgumentOutOfRangeException>());
        }

        // --- TakeDamage (Creature) ---
        [Test]
        public void TakeDamage_ReducesCurrentHealth()
        {
            var player = TestFactory.CreatePlayer(17, 150);

            player.TakeDamage(5);

            Assert.That(player.CurrentHealth, Is.EqualTo(12));
            Assert.That(player.MaxHealth, Is.EqualTo(17));
        }

        [Test]
        public void TakeDamage_Zero_ChangesNothing()
        {
            var player = TestFactory.CreatePlayer(17, 150);

            player.TakeDamage(0);

            Assert.That(player.CurrentHealth, Is.EqualTo(17));
        }

        [Test]
        public void TakeDamage_Negative_Throws()
        {
            var player = TestFactory.CreatePlayer(17, 150);

            Assert.That(() => player.TakeDamage(-1),
                Throws.TypeOf<ArgumentOutOfRangeException>());
        }

        [Test]
        public void TakeDamage_ExactlyCurrentHealth_IsDead()
        {
            var player = TestFactory.CreatePlayer(17, 150);

            player.TakeDamage(17);

            Assert.That(player.CurrentHealth, Is.EqualTo(0));
            Assert.That(player.IsDead, Is.True);
        }

        [Test]
        public void TakeDamage_MoreThanCurrentHealth_ClampsAtZero()
        {
            var player = TestFactory.CreatePlayer(17, 150);

            player.TakeDamage(50);

            Assert.That(player.CurrentHealth, Is.EqualTo(0));
            Assert.That(player.IsDead, Is.True);
        }

        // --- TryConsumeFood ---

        [Test]
        public void TryConsumeFood_WithFood_RemovesOneAndReturnsTrue()
        {
            var player = TestFactory.CreatePlayer(17, 150);

            bool consumed = player.TryConsumeFood();

            Assert.That(consumed, Is.True);
            Assert.That(player.Food, Is.EqualTo(149));
        }

        [Test]
        public void TryConsumeFood_WithoutFood_ReturnsFalseAndStaysAtZero()
        {
            var player = TestFactory.CreatePlayer(17, 0);

            bool consumed = player.TryConsumeFood();

            Assert.That(consumed, Is.False);
            Assert.That(player.Food, Is.EqualTo(0));
        }

        // --- ConsumeHatchet ---
        [Test]
        public void ConsumeHatchet_WithHatchets_RemovesOne()
        {
            var player = TestFactory.CreatePlayer(hatchets: 3);

            player.ConsumeHatchet();

            Assert.That(player.Hatchets, Is.EqualTo(2));
        }

        [Test] // retirer une hachette inexistante = bug
        public void ConsumeHatchet_WithoutHatchets_ThrowsAndStaysAtZero()
        {
            var player = TestFactory.CreatePlayer(hatchets: 0);

            Assert.That(() => player.ConsumeHatchet(), Throws.TypeOf<InvalidOperationException>());
            Assert.That(player.Hatchets, Is.EqualTo(0));
        }
    }
}