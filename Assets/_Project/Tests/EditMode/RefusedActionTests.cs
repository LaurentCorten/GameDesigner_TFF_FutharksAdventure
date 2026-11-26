using FutharksAdventure.Core;
using NUnit.Framework;

namespace FutharksAdventure.Tests
{
    public class RefusedActionTests
    {
        [Test]
        public void CanExecute_ReturnsFalse()
        {
            Assert.That(new RefusedAction().CanExecute(), Is.False);
        }

        [Test]
        public void TryExecute_ReturnsFalse()
        {
            Assert.That(new RefusedAction().TryExecute(), Is.False);
        }
    }
}