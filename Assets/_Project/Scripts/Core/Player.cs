using System;



namespace FutharksAdventure.Core
{
    public class Player : Creature
    {

        public int Food { get; private set; }
        public int Hatchets { get; private set; }

        public Player(int maxHealth, int food, int hatchets, int visionRange) : base(maxHealth, visionRange)
        {
            if(food < 0) throw new ArgumentOutOfRangeException(nameof(food));
            if(hatchets < 0) throw new ArgumentOutOfRangeException(nameof(hatchets));

            Food = food;
            Hatchets = hatchets;
        }

        public bool TryConsumeFood()
        {
            if(Food <= 0) return false;
            
            Food--;
            return true;
        }
    }
}
