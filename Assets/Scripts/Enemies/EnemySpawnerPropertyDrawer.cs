// using BadCupcake.Cyclic.Item;
// using BadCupcake.Cyclic.Utility;
using System;
using UnityEditor;
using UnityEngine;
 
namespace Enemies {

    [CustomPropertyDrawer(typeof(BattleEvent))]
    public class EnemySpawnerPropertyDrawer : PropertyDrawer {
        private float rowHeight = 20;
        private float padding = 4;
        private bool isStriped = false;
    
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
            EditorGUI.BeginProperty(position, label, property);
            SerializedProperty eventLabel = property.FindPropertyRelative("eventLabel");
            SerializedProperty type = property.FindPropertyRelative("type");
            SerializedProperty wave = property.FindPropertyRelative("wave");
            SerializedProperty boss = property.FindPropertyRelative("boss");
            SerializedProperty formation = property.FindPropertyRelative("formation");
            SerializedProperty arbitraryTime = property.FindPropertyRelative("arbitraryTime");
            SerializedProperty allowableEnemiesLeft = property.FindPropertyRelative("allowableEnemiesLeft");
            SerializedProperty arbitraryEvent = property.FindPropertyRelative("arbitraryEvent");
            SerializedProperty skip = property.FindPropertyRelative("skip");

            // COLOURS
            SerializedProperty fieldColorWave = property.FindPropertyRelative("fieldColorWave");
            SerializedProperty fieldColorBoss = property.FindPropertyRelative("fieldColorBoss");
            SerializedProperty fieldColorFormation = property.FindPropertyRelative("fieldColorFormation");
            SerializedProperty fieldColorWait = property.FindPropertyRelative("fieldColorWait");
            SerializedProperty fieldColorEvent = property.FindPropertyRelative("fieldColorEvent");
            SerializedProperty fieldColorLabel = property.FindPropertyRelative("fieldColorLabel");

            // BACKGROUND
            isStriped = !isStriped;
            if (isStriped) {
                EditorGUI.DrawRect(position, Color.black);
            } else {
                EditorGUI.DrawRect(position, new Color(0.1f, 0.1f, 0.1f, 1f));
            }
            switch ((BattleEventType)type.enumValueIndex)
            {
                case BattleEventType.Wave:
                    EditorGUI.DrawRect(position, EnemySpawner.fieldColorWave);
                    break;
                case BattleEventType.Boss:
                    EditorGUI.DrawRect(position, EnemySpawner.fieldColorBoss);
                    break;
                case BattleEventType.Formation:
                    EditorGUI.DrawRect(position, EnemySpawner.fieldColorFormation);
                    break;
                case BattleEventType.WaitForArbitraryTime:
                    EditorGUI.DrawRect(position, EnemySpawner.fieldColorWait);
                    break;
                case BattleEventType.WaitUntilEnemiesDestroyed:
                    // EditorGUI.DrawRect(position, EnemySpawner.fieldColorWait);
                    break;
                case BattleEventType.WaitUntilWaveSpawnFinished:
                    EditorGUI.DrawRect(position, EnemySpawner.fieldColorWait);
                    break;
                case BattleEventType.ArbitraryEvent:
                    EditorGUI.DrawRect(position, EnemySpawner.fieldColorEvent);
                    break;
                case BattleEventType.EventLabel:
                    EditorGUI.DrawRect(position, EnemySpawner.fieldColorLabel);
                    break;
                default:
                    break;
            }

            float xMin = position.xMin;
            float yMin = position.yMin;
            Rect labelPosition = new Rect(xMin + padding, yMin + rowHeight*0 + padding, position.width - padding * 2, rowHeight);
            Rect titlePosition = new Rect(xMin + padding, yMin + rowHeight*1 + padding, position.width - padding * 2, rowHeight);
            float col = position.width / 12f;
            Rect c0 = new Rect(xMin + col*0 + padding, yMin + rowHeight*2 + padding, col*2 - padding * 2, rowHeight);
            Rect c1 = new Rect(xMin + col*2 + padding, yMin + rowHeight*2 + padding, col*2 - padding * 2, rowHeight);
            Rect c2 = new Rect(xMin + col*4 + padding, yMin + rowHeight*2 + padding, col*8 - padding * 2, rowHeight);
            // Rect c3 = new Rect(); // more cols if you need 'em
            // Rect c4 = new Rect();
            // Rect c5 = new Rect();
            // Rect c6 = new Rect();
            // Rect c7 = new Rect();
            // Rect c8 = new Rect();
            // Rect c9 = new Rect();

            if ((BattleEventType)type.enumValueIndex == BattleEventType.EventLabel) {
                EditorGUI.PropertyField(labelPosition, type, GUIContent.none);
                EditorGUI.PropertyField(titlePosition, eventLabel, GUIContent.none);
            } else {
                EditorGUI.LabelField(labelPosition, label);
                EditorGUI.PropertyField(titlePosition, type, GUIContent.none);
                EditorGUI.LabelField(c0, "Skip");
                EditorGUI.PropertyField(c1, skip, GUIContent.none);
            }

            switch ((BattleEventType)type.enumValueIndex)
            {
                case BattleEventType.Wave:
                    EditorGUI.PropertyField(c2, wave, GUIContent.none);
                    break;
                case BattleEventType.Boss:
                    EditorGUI.PropertyField(c2, boss, GUIContent.none);
                    break;
                case BattleEventType.Formation:
                    EditorGUI.PropertyField(c2, formation, GUIContent.none);
                    break;
                case BattleEventType.WaitForArbitraryTime:
                    EditorGUI.PropertyField(c2, arbitraryTime, new GUIContent("Seconds"));
                    break;
                case BattleEventType.WaitUntilEnemiesDestroyed:
                    EditorGUI.PropertyField(c2, allowableEnemiesLeft, new GUIContent("Enemies Left"));
                    break;
                case BattleEventType.WaitUntilWaveSpawnFinished:
                    break;
                case BattleEventType.ArbitraryEvent:
                    EditorGUI.PropertyField(c2, arbitraryEvent, GUIContent.none);
                    break;
                default:
                    break;
            }

            // EditorGUI.DrawRect(new Rect(position.xMin, position.yMax - rowPadBottom, position.width, 1f), Color.gray);
            EditorGUI.EndProperty();
        }
    
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label) {
            SerializedProperty type = property.FindPropertyRelative("type");
            switch ((BattleEventType)type.enumValueIndex)
            {
                case BattleEventType.ArbitraryEvent:
                    return (rowHeight + padding * 2) * 7;
                case BattleEventType.EventLabel:
                    return (rowHeight + padding * 2) * 2;
                case BattleEventType.Wave:
                case BattleEventType.Boss:
                case BattleEventType.Formation:
                case BattleEventType.WaitForArbitraryTime:
                case BattleEventType.WaitUntilEnemiesDestroyed:
                case BattleEventType.WaitUntilWaveSpawnFinished:
                default:
                    return (rowHeight + padding * 2) * 3;
            }
        }
    }
}
