using UnityEngine;
using UnityEngine.Assertions;

public class GUIManager : MonoBehaviour {
    public Canvas mainCanvas;
    public DialogBox dialogBoxPrefab;

	DialogBox m_gameDialogBox;
	public DialogBox GameDialogBox {
		get { return m_gameDialogBox; }
	}

    void Awake()
    {
        Assert.IsNotNull(mainCanvas, "GUIManager: The main canvas isn't set");
        Assert.IsNotNull(dialogBoxPrefab, "GUIManager: The dialog-box prefab isn't set");
		m_gameDialogBox = Instantiate<DialogBox>(dialogBoxPrefab);
		m_gameDialogBox.transform.SetParent(mainCanvas.transform, false);
    }
}
