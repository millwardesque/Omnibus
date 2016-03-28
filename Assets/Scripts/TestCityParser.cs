using UnityEngine;
using System.Collections;
using UnityEngine.Assertions;

public class TestCityParser : MonoBehaviour {
    public string cityAssetName = "";

	// Use this for initialization
	void Start () {
        TextAsset cityAsset = Resources.Load<TextAsset>(cityAssetName);
        Assert.IsNotNull(cityAsset, string.Format("TestCityParser: City asset '{0}' couldn't be loaded", cityAssetName));

        CityParser.ParseCity(cityAsset.text);
    }
}
