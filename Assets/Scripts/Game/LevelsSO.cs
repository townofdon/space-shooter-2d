
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

[CreateAssetMenu(fileName = "Levels", menuName = "ScriptableObjects/LevelsSO", order = 0)]
public class LevelsSO : ScriptableObject {
    [SerializeField] List<Scene> levels = new List<Scene>();
    [SerializeField] Scene WarpLevel;
    [SerializeField] Scene UpgradeLevel;
}
