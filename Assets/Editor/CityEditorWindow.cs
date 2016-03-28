using UnityEngine;
using UnityEditor;
using System.Collections;

class CityBuildingData { 
	public string prefabName = "";
	public Vector3 position = Vector3.zero;
	public Vector3 rotation = Vector3.zero;
	public Vector3 scale = Vector3.one;
	public bool showFoldout = false;
}

public class CityEditorWindow : EditorWindow {
	int rows = 5;
	int columns = 5;
	int previousRows;
	int previousColumns;

	Vector2 scrollPos = Vector2.zero;
	CityBuildingData[,] buildingData = null;

	[MenuItem ("Window/Omnibus/City Editor")]
	public static void ShowWindow() {
		EditorWindow.GetWindow(typeof(CityEditorWindow)).titleContent = new GUIContent("City Editor");
	}

	void OnGUI () {
		EditorGUILayout.LabelField("City Grid", EditorStyles.boldLabel);

		previousRows = rows;
		previousColumns = columns;
		rows = EditorGUILayout.IntField("Rows", rows);
		columns = EditorGUILayout.IntField("Columns", columns);

		if (buildingData == null || rows != previousRows || columns != previousColumns) {
			ChangeCitySize(rows, columns);
		}

		scrollPos = EditorGUILayout.BeginScrollView(scrollPos);
		CityBuildingData building;
		for (int y = 0; y < rows; y++) {
			GUILayout.Label("Row " + y, EditorStyles.boldLabel);

			for (int x = 0; x < columns; x++) {
				building = buildingData[x, y];
				building.showFoldout = EditorGUILayout.Foldout(building.showFoldout, "(" + x + ", " + y + ")");
				if (building.showFoldout) {
					building.prefabName = EditorGUILayout.TextField("Prefab", building.prefabName);
					building.position = EditorGUILayout.Vector3Field("Position", building.position);
					building.rotation = EditorGUILayout.Vector3Field("Rotation", building.rotation);
					building.scale = EditorGUILayout.Vector3Field("Scale", building.scale);
				}
			}
		}
		EditorGUILayout.EndScrollView();
	}

	void ChangeCitySize(int rows, int columns) {
		Debug.Log(string.Format("Changing city size to ({0}, {1})", columns, rows));
		CityBuildingData[,] newBuildingData = new CityBuildingData[columns, rows];

		for (int y = 0; y < rows; y++) {
			for (int x = 0; x < columns; x++) {
				if (buildingData != null && buildingData.GetLength(0) > x && buildingData.GetLength(1) > y) {
					newBuildingData[x, y] = buildingData[x, y];
				}
				else {
					newBuildingData[x, y] = new CityBuildingData();
				}
			}
		}
		buildingData = newBuildingData;
	}
}