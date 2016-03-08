using UnityEngine;
using UnityEngine.Assertions;

public class GUIManager : MonoBehaviour {
    public Canvas mainCanvas;
    public DialogBox dialogBoxPrefab;

    public DialogBox GameDialogBox;

    void Awake()
    {
        Assert.IsNotNull(mainCanvas, "GUIManager: The main canvas isn't set");
        Assert.IsNotNull(dialogBoxPrefab, "GUIManager: The dialog-box prefab isn't set");
        GameDialogBox = Instantiate<DialogBox>(dialogBoxPrefab);
        GameDialogBox.transform.SetParent(mainCanvas.transform, false);
    }
}
