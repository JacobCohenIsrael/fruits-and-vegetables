using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Completed
{
    public class MovementButtonsUiManager : MonoBehaviour
    {
        public static MovementButtonsUiManager Instance;

        public List<MovementButton> movementButtons;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else if (Instance != this)
            {
                Destroy(gameObject);
            }
        }
    }
}

