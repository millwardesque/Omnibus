using UnityEngine;
using Rewired;

public class InputManager : MonoBehaviour {
	// Use this for initialization
	void Start () {
		GameManager.Instance.Messenger.AddListener("DialogBoxOpened", OnDialogBoxOpen);
		GameManager.Instance.Messenger.AddListener("DialogBoxClosed", OnDialogBoxClose);

		GameManager.Instance.Messenger.AddListener("PlayerIsFlying", OnPlayerIsFlying);
		GameManager.Instance.Messenger.AddListener("PlayerIsGrounded", OnPlayerIsGrounded);
	}

	void Update() {
		if(!ReInput.isReady) return;

		if (ReInput.players.Players[0].GetButtonDown("Next Dialog"))
		{
			GameManager.Instance.GUI.GameDialogBox.Next();
		}

		if (ReInput.players.Players[0].GetButtonDown("Move Choice Previous"))
		{
			GameManager.Instance.GUI.GameDialogBox.MoveChoicePrevious();
		}

		if (ReInput.players.Players[0].GetButtonDown("Move Choice Next"))
		{
			GameManager.Instance.GUI.GameDialogBox.MoveChoiceNext();
		}


	}

	void OnDialogBoxOpen(Message message) {
		if(!ReInput.isReady) return;
		ReInput.players.Players[0].controllers.maps.SetMapsEnabled(true, "In Dialog");
	}

	void OnDialogBoxClose(Message message) {
		if(!ReInput.isReady) return;
		ReInput.players.Players[0].controllers.maps.SetMapsEnabled(false, "In Dialog");
	}

	void OnPlayerIsFlying(Message message) {
		if(!ReInput.isReady) return;
		ReInput.players.Players[0].controllers.maps.SetMapsEnabled(false, "Grounded");
		ReInput.players.Players[0].controllers.maps.SetMapsEnabled(true, "Flying");
	}

	void OnPlayerIsGrounded(Message message) {
		if(!ReInput.isReady) return;
		ReInput.players.Players[0].controllers.maps.SetMapsEnabled(true, "Grounded");
		ReInput.players.Players[0].controllers.maps.SetMapsEnabled(false, "Flying");
	}
}
