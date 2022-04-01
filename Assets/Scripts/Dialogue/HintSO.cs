
using UnityEngine;

namespace Dialogue {

    [CreateAssetMenu(fileName = "HintSO", menuName = "ScriptableObjects/HintSO", order = 0)]
    public class HintSO : ScriptableObject {
        [SerializeField] bool _showButton = false;
        [SerializeField] string _keyboardButton;
        [SerializeField] string _gamepadButton;

        [TextArea(minLines: 3, maxLines: 10)]
        [SerializeField] string _hintText;

        public bool showButton => _showButton;
        public string keyboardButton => _keyboardButton;
        public string gamepadButton => _gamepadButton;
        public string hintText => _hintText;
    }
}

