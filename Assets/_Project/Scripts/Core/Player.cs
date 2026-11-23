using System;



namespace FutharksAdventure.Core
{
    public class Player : Creature
    {

        public int Food { get; private set; }

        public Player(int maxHealth, int food) : base(maxHealth)
        {
            if (food < 0) throw new ArgumentOutOfRangeException(nameof(food));

            Food = food;
        }

        public bool TryConsumeFood()
        {
            if(Food <= 0) return false;
            
            Food--;
            return true;
        }
    }
}
