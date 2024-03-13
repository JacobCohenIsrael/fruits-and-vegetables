using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Completed
{
    public class MovementButton : MonoBehaviour
    {

        public Food food;
        private Button button;

        [SerializeField] private TextMeshProUGUI text;

        private void Awake()
        {
            button = GetComponent<Button>();
            button.onClick.AddListener(OnButtonClicked);
        }

        private void OnDestroy()
        {
            button.onClick.RemoveListener(OnButtonClicked);
        }

        private void OnButtonClicked()
        {
            var foodList = GameObject.FindObjectsOfType<Food>();
            foreach (var food in foodList)
            {
                if (food.FoodType == this.food.FoodType)
                {
                    var player = GameObject.FindAnyObjectByType<Player>();
                    player.MovePlayer(food.transform.position);
                    break;
                }
            }
        }

        public void SetFood(Food food)
        {
            this.food = food;
            text.text = food.FoodType;
        }
    }
}

