using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game {

    [CreateAssetMenu(fileName = "GameState", menuName = "ScriptableObjects/GameState", order = 0)]
    public class GameStateSO : ScriptableObject {
        [SerializeField] int _initialPoints = 50;

        int _pointsInBank;
        int _pointsGained; // points accumulated since round start - when player dies the number goes to zero

        public int totalPoints => _pointsInBank + _pointsGained;
        public int pointsGained => _pointsGained;

        public void Init() {
            _pointsInBank = _initialPoints;
            _pointsGained = 0;
        }

        public void StorePoints() {
            _pointsInBank += _pointsGained;
            _pointsGained = 0;
        }

        public void GainPoints(int value) {
            _pointsGained += value;
        }

        public void LosePoints() {
            _pointsGained = 0;
        }
    }
}
