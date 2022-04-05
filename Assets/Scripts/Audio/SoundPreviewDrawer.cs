using UnityEngine;
using UnityEditor;

#if (UNITY_EDITOR)

[CustomEditor(typeof(Audio.Sound))]
public class SoundDrawer : Editor {
    SerializedProperty sound;

    void OnEnable()
    {
        // Setup the SerializedProperties.
        // damageProp = serializedObject.FindProperty ("damage");
        // armorProp = serializedObject.FindProperty ("armor");
        // gunProp = serializedObject.FindProperty ("gun");
    }

    public override void OnInspectorGUI()
    {
        Debug.Log(serializedObject);
        Debug.Log(target);
        DrawDefaultInspector();
        
        // MyPlayer targetPlayer = (MyPlayer)target;
        EditorGUILayout.LabelField ("Some help", "Some other text");

        // targetPlayer.speed = EditorGUILayout.Slider ("Speed", targetPlayer.speed, 0, 100);

        if (GUILayout.Button("Your ButtonText")) {
             //add everthing the button would do.
        }
    }
}

// SoundPreviewDrawer
// [CustomPropertyDrawer(typeof(Audio.SoundPreview))]
// public class SoundPreviewDrawer : PropertyDrawer
// {
//     // Draw the property inside the given rect
//     public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
//     {
//         // Using BeginProperty / EndProperty on the parent property means that
//         // prefab override logic works on the entire property.
//         EditorGUI.BeginProperty(position, label, property);

//         // Draw label
//         position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);

//         // Don't make child fields be indented
//         // var indent = EditorGUI.indentLevel;
//         // EditorGUI.indentLevel = 0;

//         // // Calculate rects
//         // var amountRect = new Rect(position.x, position.y, 30, position.height);
//         // var unitRect = new Rect(position.x + 35, position.y, 50, position.height);
//         // var nameRect = new Rect(position.x + 90, position.y, position.width - 90, position.height);

//         // // Draw fields - passs GUIContent.none to each so they are drawn without labels
//         // EditorGUI.PropertyField(amountRect, property.FindPropertyRelative("amount"), GUIContent.none);
//         // EditorGUI.PropertyField(unitRect, property.FindPropertyRelative("unit"), GUIContent.none);
//         // EditorGUI.PropertyField(nameRect, property.FindPropertyRelative("name"), GUIContent.none);

//         // // Set indent back to what it was
//         // EditorGUI.indentLevel = indent;

//         // GUIContent content = new GUIContent("I am button yo");
//         // if (EditorGUI.DropdownButton(position, content, FocusType.Passive)) {
//         //     Debug.Log(property.name);
//         //     Debug.Log(property.serializedObject);
//         //     // Debug.Log(property.serializedObject.targetObject.GetType().GetMethod("Play"));
//         //     // System.Reflection.MethodInfo methodInfo = property.serializedObject.targetObject.GetType().GetMethod("Play");
//         //     // methodInfo.Invoke(property, new object[]{});
//         //     // (property.serializedObject.targetObject[property] as Audio.SoundPreview).Play();
//         // }

//         if (GUI.Button(position, "I am a button"))
//         {
//             Debug.Log(property.name);
//             Debug.Log(property.serializedObject.targetObject.GetType());


//             var parameterTypes = new System.Type[]{ typeof(Audio.BaseSound) };
//             System.Reflection.MethodInfo methodInfo = property.serializedObject.targetObject.GetType().GetMethod("PreviewSound", parameterTypes);
//             Debug.Log(methodInfo);
//             if (methodInfo != null) {
//                 // methodInfo.Invoke(property.serializedObject.targetObject, new object[]{ property as Audio.BaseSound });
//             }

//             // if (propertyInfo != null) {
//             //     System.Reflection.MethodInfo methodInfo = propertyInfo.GetType().GetMethod("Play");
//             //     Debug.Log(methodInfo);
//             //     if (methodInfo != null) methodInfo.Invoke(property, new object[]{});
//             // }
//             // System.Reflection.MethodInfo methodInfo = property.GetType().GetMethod("Play");
//         }

//         EditorGUI.EndProperty();
//     }
// }

#endif
