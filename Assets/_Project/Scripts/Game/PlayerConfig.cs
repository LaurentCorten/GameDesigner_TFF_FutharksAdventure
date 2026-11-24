using UnityEngine;

namespace FutharksAdventure.Game
{
    [CreateAssetMenu(fileName = "PlayerConfig", menuName = "FutharksAdventure/PlayerConfig")]
    public class PlayerConfig : ScriptableObject
    {

        [SerializeField, Min(1)] private int _maxHealth = 17;
        [SerializeField, Min(0)] private int _food = 150;
        [SerializeField, Min(0)] private int _hatchets = 3;
        [SerializeField, Min(1)] private int _visionRange = 6;

        public int MaxHealth => _maxHealth;
        public int Food => _food;
        public int Hatchets => _hatchets;
        public int VisionRange => _visionRange;

    }
}
