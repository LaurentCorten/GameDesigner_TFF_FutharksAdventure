using UnityEngine;

namespace FutharksAdventure.Game
{
    [CreateAssetMenu(fileName = "MockEnemyConfig", menuName = "FutharksAdventure/MockEnemyConfig")]
    public class MockEnemyConfig : ScriptableObject
    {
        [SerializeField, Min(1)] private int _maxHealth = 5;
        [SerializeField, Min(1)] private int _visionRange = 6;
        [SerializeField, Min(0)] private int _attackDamage = 4;

        public int MaxHealth => _maxHealth;
        public int VisionRange => _visionRange;
        public int AttackDamage => _attackDamage;
    }
}
