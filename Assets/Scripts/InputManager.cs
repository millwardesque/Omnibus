using UnityEngine;
using Rewired;

public class InputManager : MonoBehaviour {
	// Use this for initialization
	void Start () {
		GameManager.Instance.Messenger.AddListener("DialogBoxOpened", OnDialogBoxOpen);
		GameManager.Instance.Messenger.AddListener("DialogBoxClosed", OnDialogBoxClose);
	}

	void Update() {
		if (ReInput.players.Players[0].GetButtonDown("Next Dialog"))
		{
			GameManager.Instance.GUI.GameDialogBox.Next();
		}
	}

	void OnDialogBoxOpen(Message message) {
		ReInput.players.Players[0].controllers.maps.SetMapsEnabled(true, "In Dialog");
	}

	void OnDialogBoxClose(Message message) {
		ReInput.players.Players[0].controllers.maps.SetMapsEnabled(false, "In Dialog");
	}
}
