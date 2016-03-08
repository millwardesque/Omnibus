using UnityEngine;
using UnityEngine.Assertions;
using Rewired;

public class GameManager : MonoBehaviour {
	bool hasShownDialog = false;

	InputManager m_inputManager;
	public InputManager Input {
		get { return m_inputManager; }
	}

    GUIManager m_guiManager;
    public GUIManager GUI
    {
        get { return m_guiManager;  }
    }

	MessageManager m_messageManager;
	public MessageManager Messenger {
		get { return m_messageManager; }
	}

    public static GameManager Instance = null;

    void Awake()
    {
        if (null == Instance)
        {
            Instance = this;
			UnityEngine.Assertions.Assert.raiseExceptions = true;

			m_messageManager = new MessageManager();

			m_inputManager = GetComponent<InputManager>();
			Assert.IsNotNull(m_inputManager, "GameManager: No Input Manager exists on the Game Manager.");

			m_guiManager = GetComponent<GUIManager>();
			Assert.IsNotNull(m_guiManager, "GameManager: No GUI Manager exists on the Game Manager.");
        }
        else
        {
            GameObject.Destroy(gameObject);
        }
    }

	// Update is called once per frame
	void Update () {
		if (!hasShownDialog)
		{
			TestShowDialogBox();
		}
    }

	public void TestShowDialogBox() {
		DialogItem item1 = new DialogItem("This one is really long and should be broken up into multiple blah blah blah blah blah blah blah blah blah blah dance dance dance blah blah blah blah blah blah blah blah blah blah test test am I done yet? This one is really long and should be broken up into multiple blah blah blah blah blah blah blah blah blah blah dance dance dance blah blah blah blah blah blah blah blah blah blah test test am I done yet?  This one is really long and should be broken up into multiple blah blah blah blah blah blah blah blah blah blah dance dance dance  blah blah blah blah blah blah blah blah blah blah test test am I done yet?");
		DialogItem item2 = new DialogItem("This is a follow-up test on\nthree\nlines!");
		item1.nextDialog = item2;

		m_guiManager.GameDialogBox.ShowDialog(item1);
		hasShownDialog = true;
	}
}
