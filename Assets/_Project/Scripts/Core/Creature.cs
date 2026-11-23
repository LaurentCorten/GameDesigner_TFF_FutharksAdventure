using System;


namespace FutharksAdventure.Core
{
    public abstract class Creature : Entity
    {

        public int MaxHealth {  get; }
        public int CurrentHealth { get; private set; }
        public bool IsDead { get; private set; }

        public Creature(int maxHealth)
        {
            if(maxHealth <= 0) throw new ArgumentOutOfRangeException(nameof(maxHealth));

            MaxHealth = maxHealth;
            CurrentHealth = maxHealth;
            IsDead = false;
        }

        public void TakeDamage(int amount)
        {
            if(amount < 0) throw new ArgumentOutOfRangeException(nameof(amount));

            if(CurrentHealth <= amount)
            {
                CurrentHealth = 0;
                IsDead = true;
                return;
            }

            CurrentHealth -= amount;
        }
    }
}
