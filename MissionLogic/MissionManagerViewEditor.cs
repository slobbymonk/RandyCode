using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(MissionManagerView))]
public class MissionManagerViewEditor : Editor
{
    public override void OnInspectorGUI()
    {
        MissionManagerView view = (MissionManagerView)target;

        DrawDefaultInspector();

        EditorGUILayout.Space();

        if (GUILayout.Button("Show All Cards"))
        {
            view.ShowAllCards();
        }

        if (GUILayout.Button("Hide All Cards"))
        {
            view.HideAllCards();
        }

        EditorGUILayout.Space();

        EditorGUILayout.LabelField("Active Cards", EditorStyles.boldLabel);
        foreach (var card in view.GetAllActiveCards()) // Implement GetAllActiveCards method in MissionManagerView to return a list of all active cards
        {
            EditorGUILayout.LabelField(card._title.text, card._content.text);
        }
    }
}
