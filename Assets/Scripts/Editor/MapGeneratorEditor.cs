using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(MapGenerator))]
public class MapGeneratorEditor : Editor {
	private MapGenerator mapGenerator;

	private SerializedProperty width;
	private SerializedProperty height;
	private SerializedProperty seed;

	private SerializedProperty mapRenderer;

	private SerializedProperty wallChance;

	private SerializedProperty numEnemies;

	private void OnEnable() {
		mapGenerator = (MapGenerator) target;

		//get all editable propeties
		width = serializedObject.FindProperty("width");
		height = serializedObject.FindProperty("height");
		seed = serializedObject.FindProperty("seed");

		mapRenderer = serializedObject.FindProperty("mapRenderer");

		wallChance = serializedObject.FindProperty("wallChance");

		numEnemies = serializedObject.FindProperty("numEnemies");
	}

	public override void OnInspectorGUI() {
		//create property fields
		EditorGUI.BeginChangeCheck();

		EditorGUILayout.Space();
		EditorGUILayout.PropertyField(width);
		EditorGUILayout.PropertyField(height);
		EditorGUILayout.PropertyField(seed);

		EditorGUILayout.Space();
		EditorGUILayout.PropertyField(mapRenderer);

		EditorGUILayout.Space();
		EditorGUILayout.PropertyField(wallChance);

		EditorGUILayout.Space();
		EditorGUILayout.PropertyField(numEnemies);

		serializedObject.ApplyModifiedProperties();
		//if changed, regenerate map
		if (EditorGUI.EndChangeCheck())
		{
			mapGenerator.GenerateMap();
		}

		EditorGUILayout.Space();
		EditorGUILayout.Space();
		EditorGUILayout.Space();
		//button for ease because map does not reload when code is changed
		if (GUILayout.Button("Generate Map"))
		{
			mapGenerator.GenerateMap();
		}
		EditorGUILayout.Space();
	}

}
