using FutharksAdventure.Core;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

namespace FutharksAdventure.Game
{
    public class PlayerStatsDisplay : MonoBehaviour
    {
        [SerializeField] private Slider _healthBar;
        [SerializeField] private TextMeshProUGUI _healthCounter;
        [SerializeField] private TextMeshProUGUI _foodCounter;
        [SerializeField] private TextMeshProUGUI _hatchetsCounter;


        private EventBus _eventBus;
        private Player _player;

        public void Init(EventBus eventBus, Player player)
        {
            if (player is null)
                throw new ArgumentNullException(nameof(player));
            if (eventBus is null)
                throw new ArgumentNullException(nameof(eventBus));

            if (_healthBar == null)
                throw new ArgumentNullException(nameof(_healthBar));
            if (_healthCounter == null)
                throw new ArgumentNullException(nameof(_healthCounter));
            if (_foodCounter == null)
                throw new ArgumentNullException(nameof(_foodCounter));
            if (_hatchetsCounter == null)
                throw new ArgumentNullException(nameof(_hatchetsCounter));

            _player = player;
            _eventBus = eventBus;
            //_eventBus.Subscribe<FoodChanged>(OnFoodChanged);
            //_eventBus.Subscribe<HealthChanged>(OnHealthChanged);
            _eventBus.Subscribe<TurnResolved>(OnTurnResolved);

            UpdatePlayerStatsDisplay();
        }

        private void OnDestroy()
        {
            //_eventBus?.Unsubscribe<FoodChanged>(OnFoodChanged);
            //_eventBus?.Unsubscribe<HealthChanged>(OnHealthChanged);
            _eventBus?.Unsubscribe<TurnResolved>(OnTurnResolved);
        }

        //--- CallBacks --

        //private void OnFoodChanged(FoodChanged evt)
        //{

        //}

        //private void OnHealthChanged(HealthChanged evt)
        //{

        //}

        private void OnTurnResolved(TurnResolved resolved)
        {
            UpdatePlayerStatsDisplay();
        }

        public void UpdatePlayerStatsDisplay()
        {
            _healthBar.maxValue = _player.MaxHealth;
            _healthBar.value = _player.CurrentHealth;
            _healthCounter.text = $"{_player.CurrentHealth}/{_player.MaxHealth}";
            _foodCounter.text = _player.Food.ToString();
            _hatchetsCounter.text = _player.Hatchets.ToString();
        }
    }
}