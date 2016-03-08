using UnityEngine;
using UnityEngine.Assertions;
using Rewired;

public class GameManager : MonoBehaviour {
    GUIManager m_guiManager;
    public GUIManager GuiManager
    {
        get { return m_guiManager;  }
    }

    public static GameManager Instance = null;

    void Awake()
    {
        if (null == Instance)
        {
            Instance = this;
            m_guiManager = FindObjectOfType<GUIManager>();
            Assert.IsNotNull(m_guiManager, "GameManager: No GUI Manager exists in the scene.");
        }
        else
        {
            GameObject.Destroy(gameObject);
        }
    }
	// Use this for initialization
	void Start () {
        m_guiManager.GameDialogBox.ShowDialog(new DialogItem("This is the first of several test messages!\nOn multiple lines, even!"));
        ReInput.players.Players[0].controllers.maps.SetMapsEnabled(true, "In Dialog");
	}
	
	// Update is called once per frame
	void Update () {
        if (ReInput.players.Players[0].GetButtonDown("Next"))
        {
            GuiManager.GameDialogBox.Next();
        }
    }
}
