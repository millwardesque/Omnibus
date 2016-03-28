using System;
using UnityEngine;
using UnityEngine.Assertions;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public class CityParser {
    static string buildingPrefabFolder = "Buildings";   // Resources sub-folder name for the folder that stores building prefabs.

    static Regex commentRegex = new Regex(@"^#"); // # This is a comment
    static Regex sectionEndRegex = new Regex(@"^\s*(\n|$)"); // Blank line.

    // Building definitions
    static Regex buildingRegex = new Regex(@"^:([a-zA-Z0-9_\-]+)"); // :building_type_id
    static Regex buildingPrefabRegex = new Regex(@"^prefab:\s*([a-zA-Z0-9_\ \-]+)"); // prefab: prefab_name

    // Building placement
    static Regex instanceStartRegex = new Regex(@"^\$building\s*$");  // Building instance start block
    static Regex instanceTypeRegex = new Regex(@"^t:\s*([a-zA-Z0-9_]+)"); // t: building_type_id
    static Regex instancePositionRegex = new Regex(@"^p:\s*(\-?\d+\.\d+|\-?\d+) (\-?\d+\.\d+|\-?\d+) (\-?\d+\.\d+|\-?\d+)"); // p: 2.0 1.0 0.0
    static Regex instanceRotationRegex = new Regex(@"^r:\s*(\-?\d+\.\d+|\-?\d+) (\-?\d+\.\d+|\-?\d+) (\-?\d+\.\d+|\-?\d+)"); // r: 0.0 90.0 0.0
    static Regex instanceScaleRegex =    new Regex(@"^s:\s*(\-?\d+\.\d+|\-?\d+) (\-?\d+\.\d+|\-?\d+) (\-?\d+\.\d+|\-?\d+)"); // s: 1.0 -10.0 1.0

    /// <summary>
    /// Parse the city file
    /// </summary>
    /// <param name="script"></param>
    /// <returns></returns>
    public static void ParseCityFromResource(string cityResourceName, Transform parent = null)
    {
        TextAsset scriptAsset = Resources.Load<TextAsset>(cityResourceName);
        Assert.IsNotNull(scriptAsset, string.Format("CityParser: City resource '{0}' couldn't be loaded", cityResourceName));
        CityParser.ParseCity(scriptAsset.text, parent);
    }

    /// <summary>
    /// Parse a city-description text string
    /// </summary>
    /// <param name="script"></param>
    /// <returns></returns>
    public static void ParseCity(string cityString, Transform parent = null)
    {
        // Debug.Log(string.Format("Parsing city"));
        string[] lines = cityString.Split(new string[] { "\n" }, StringSplitOptions.None);
        Match match;
        Dictionary<string, GameObject> buildingTypes = new Dictionary<string, GameObject>();

        // Type construction variables.
        GameObject currentTypePrefab = null;
        string currentTypeID = "";
 
        // Instance construction variables.
        string currentInstanceType = "";
        Vector3 currentInstancePosition = Vector3.zero;
        Quaternion currentInstanceRotation = Quaternion.identity;
        Vector3 currentInstanceScale = Vector3.one;

        for (int i = 0; i < lines.Length; ++i)
        {
            string line = lines[i];

            match = CityParser.commentRegex.Match(line);
            if (match.Success)
            {
                continue;
            }

            match = CityParser.buildingRegex.Match(line);
            if (match.Success)
            {
                if (currentTypeID != "")
                {
                    if (currentTypePrefab != null)
                    {

                        CityParser.CloseBuildingType(buildingTypes, ref currentTypeID, ref currentTypePrefab);
                    }
                    else
                    {
                        Debug.LogWarning(string.Format("Parsing city script: Line at {0} starts a new building type, but the old one named {1} is incomplete.", i, currentTypeID));
                        continue;
                    }
                }


                // Debug.Log(string.Format("Starting building type {0}", match.Groups[1]));
                currentTypeID = match.Groups[1].ToString();
                continue;
            }

            match = CityParser.buildingPrefabRegex.Match(line);
            if (match.Success)
            {
                if (currentTypeID == "")
                {
                    Debug.LogWarning(string.Format("Parsing city script: Line at {0} sets a building type's prefab, but no building type is in progress.", i));
                    continue;
                }

                // Debug.Log(string.Format("Loading building type prefab {0}", match.Groups[1]));
                string prefabName = buildingPrefabFolder + "/" + match.Groups[1].ToString();
                currentTypePrefab = Resources.Load<GameObject>(prefabName);
                if (currentTypePrefab == null)
                {
                    Debug.LogWarning(string.Format("Parsing city script: Line at {0} sets a building type's prefab, but no prefab at {1} could be found", i, prefabName));
                }
                continue;
            }

            match = CityParser.sectionEndRegex.Match(line);
            if (match.Success)
            {
                if (currentTypeID != "")
                {
                    if (currentTypePrefab != null)
                    {
                        CloseBuildingType(buildingTypes, ref currentTypeID, ref currentTypePrefab);
                    }
                    else
                    {
                        Debug.LogWarning(string.Format("Parsing city script: Line at {0} looks like it's trying to close building type {1}, but no prefab is set.", i, currentTypeID));
                    }
                }

                if (currentInstanceType != "")
                {
                    if (buildingTypes.ContainsKey(currentInstanceType))
                    {
                        InstantiateBuilding(buildingTypes, ref currentInstanceType, ref currentInstancePosition, ref currentInstanceRotation, ref currentInstanceScale, parent);
                    }
                    else
                    {
                        Debug.LogWarning(string.Format("Parsing city script: Line at {0} looks like it's trying to close building instance {1}, but instance type doesn't exist", i, currentInstanceType));
                    }
                }

                continue;
            }

            match = CityParser.instanceStartRegex.Match(line);
            if (match.Success)
            {
                if (currentTypeID != "")
                {
                    if (currentTypePrefab != null)
                    {
                        CloseBuildingType(buildingTypes, ref currentTypeID, ref currentTypePrefab);
                    }
                    else
                    {
                        Debug.LogWarning(string.Format("Parsing city script: Line at {0} needs to close building type {1} to start building instance, but no prefab is set.", i, currentTypeID));
                    }
                }

                if (currentInstanceType != "")
                {
                    if (buildingTypes.ContainsKey(currentInstanceType))
                    {
                        InstantiateBuilding(buildingTypes, ref currentInstanceType, ref currentInstancePosition, ref currentInstanceRotation, ref currentInstanceScale, parent);
                    }
                    else
                    {
                        Debug.LogWarning(string.Format("Parsing city script: Line at {0} needs to close building instance {1} to start building instance, but instance type doesn't exist", i, currentInstanceType));
                    }
                }

                // Debug.Log(string.Format("Starting building instance"));
                continue;
            }

            match = CityParser.instanceTypeRegex.Match(line);
            if (match.Success)
            {
                currentInstanceType = match.Groups[1].Value;
                // Debug.Log(string.Format("Setting instance type to {0}", currentInstanceType));
                continue;
            }

            match = CityParser.instancePositionRegex.Match(line);
            if (match.Success)
            {
                float x = float.Parse(match.Groups[1].Value);
                float y = float.Parse(match.Groups[2].Value);
                float z = float.Parse(match.Groups[3].Value);

                currentInstancePosition = new Vector3(x, y, z);
                // Debug.Log(string.Format("Setting instance position to {0}", currentInstancePosition));
                continue;
            }

            match = CityParser.instanceRotationRegex.Match(line);
            if (match.Success)
            {
                float x = float.Parse(match.Groups[1].Value);
                float y = float.Parse(match.Groups[2].Value);
                float z = float.Parse(match.Groups[3].Value);

                currentInstanceRotation = new Quaternion(x, y, z, 1f);
                // Debug.Log(string.Format("Setting instance rotation to {0}", currentInstanceRotation));
                continue;
            }

            match = CityParser.instanceScaleRegex.Match(line);
            if (match.Success)
            {
                float x = float.Parse(match.Groups[1].Value);
                float y = float.Parse(match.Groups[2].Value);
                float z = float.Parse(match.Groups[3].Value);

                currentInstanceScale = new Vector3(x, y, z);
                // Debug.Log(string.Format("Setting instance scale to {0}", currentInstanceScale));
                continue;
            }
        }

        // Final cleanup of any unfinished buildings.
        if (currentTypeID != "")
        {
            if (currentTypePrefab != null)
            {
                CloseBuildingType(buildingTypes, ref currentTypeID, ref currentTypePrefab);
            }
            else
            {
                Debug.LogWarning(string.Format("Parsing city script: End-of-file needs to close building type {0}, but no prefab is set.", currentTypeID));
            }
        }

        if (currentInstanceType != "")
        {
            if (buildingTypes.ContainsKey(currentInstanceType))
            {
                InstantiateBuilding(buildingTypes, ref currentInstanceType, ref currentInstancePosition, ref currentInstanceRotation, ref currentInstanceScale, parent);
            }
            else
            {
                Debug.LogWarning(string.Format("Parsing city script: End-of-file needs to close building instance {0}, but instance type doesn't exist", currentInstanceType));
            }
        }
    }

    static GameObject InstantiateBuilding(Dictionary<string, GameObject> buildingTypes, ref string type, ref Vector3 position, ref Quaternion rotation, ref Vector3 scale, Transform parent = null) {
        //  Debug.Log(string.Format("Instantiating building {0}", type));
        if (type == "" || !buildingTypes.ContainsKey(type))
        {
            Debug.LogWarning(string.Format("Parsing city script: Can't instantiate building because instance type '{0}' doesn't exist", type));
            return null;
        }

        GameObject prefab = buildingTypes[type];
        GameObject building = GameObject.Instantiate<GameObject>(prefab);
        building.name = prefab.name;
        building.transform.position = position;
        building.transform.rotation = rotation;
        building.transform.localScale = scale;
        building.transform.SetParent(parent);

        type = "";
        position = Vector3.zero;
        rotation = Quaternion.identity;
        scale = Vector3.one;

        return building;
    }

    static void CloseBuildingType(Dictionary<string, GameObject> buildings, ref string id, ref GameObject building)
    {
        // Debug.Log(string.Format("Closing building type {0}", id));
        buildings.Add(id, building);
        id = "";
        building = null;
    }
}