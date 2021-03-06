﻿using UnityEngine;
using UnityEngine.Assertions;
using Rewired;

public class GameManager : MonoBehaviour {
    [SerializeField] string m_cityScript = "";
    [SerializeField] GameObject m_cityContainer = null;

	InputManager m_inputManager;
	public InputManager Input {
		get { return m_inputManager; }
	}

    GUIManager m_guiManager;
    public GUIManager GUI
    {
        get { return m_guiManager;  }
    }

	DialogManager m_dialogManager;
	public DialogManager Dialog {
		get { return m_dialogManager; }
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

			m_dialogManager = new DialogManager();
        }
        else
        {
            GameObject.Destroy(gameObject);
        }
    }

	void Start() {
        if (m_cityScript != "")
        {
            CityParser.ParseCityFromResource(m_cityScript, (m_cityContainer != null ? m_cityContainer.transform : null));
        }
	}

	// Update is called once per frame
	void Update () {
    }
}
