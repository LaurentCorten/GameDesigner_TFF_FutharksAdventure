using System;


namespace FutharksAdventure.Core
{
    public abstract class Creature : Entity
    {

        public int MaxHealth {  get; }
        public int CurrentHealth { get; private set; }
        public int AttackDamage { get; private set; }
        public int VisionRange { get; }
        public bool IsDead { get; private set; }

        public Creature(int maxHealth, int visionRange, int atkDmg)
        {
            if(maxHealth <= 0) throw new ArgumentOutOfRangeException(nameof(maxHealth));
            if (visionRange <= 0) throw new ArgumentOutOfRangeException(nameof(visionRange));
            if (atkDmg < 0) throw new ArgumentOutOfRangeException(nameof(atkDmg));

            MaxHealth = maxHealth;
            CurrentHealth = maxHealth;
            AttackDamage = atkDmg;
            VisionRange = visionRange;
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
