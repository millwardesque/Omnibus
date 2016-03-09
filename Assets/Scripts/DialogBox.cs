using UnityEngine;

/// <summary>
/// Represents a single piece of dialog.
/// </summary>
public class DialogItem
{
    public string message;
    public DialogItem nextDialog;

    public DialogItem(string message, DialogItem nextDialog = null)
    {
        this.message = message;
        this.nextDialog = nextDialog;
    }

	public override string ToString() {
		return this.message;
	}
}

/// <summary>
/// Abstract class for dialog boxes
/// </summary>
public abstract class DialogBox : MonoBehaviour {
    public abstract void ShowDialog(DialogItem dialog);
    public abstract void Next();
    public abstract void CloseDialog();
}
