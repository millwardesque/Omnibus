using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Represents a single piece of dialog.
/// </summary>
public class DialogItem
{
    public string message;
    public DialogItem nextDialog;

	List<string> m_choices;
	public List<string> Choices {
		get { return m_choices; }
	}

	public bool HasChoices() {
		return (m_choices != null && m_choices.Count > 0);
	}

	public DialogItem(string message, List<string> choices = null, DialogItem nextDialog = null)
    {
        this.message = message;
		this.m_choices = choices;
        this.nextDialog = nextDialog;
    }

	public override string ToString() {
		string output = this.message;
		if (m_choices != null) {
			output += string.Format(" + {0} choices", m_choices.Count);
		}

		if (this.nextDialog != null) {
			output += " + next-dialog";
		}

		return output;
	}
}

public delegate void AfterFullDialogShown(DialogItem shownItem);
public delegate void DialogChoiceSelected(DialogItem dialogItem, int choiceIndex);

/// <summary>
/// Abstract class for dialog boxes
/// </summary>
public abstract class DialogBox : MonoBehaviour {
	public abstract void ShowDialog(DialogItem dialog, AfterFullDialogShown afterShown, DialogChoiceSelected choiceSelected);
    public abstract void Next();
    public abstract void CloseDialog();

	public abstract void MoveChoicePrevious();
	public abstract void MoveChoiceNext();
	public abstract void SelectChoice();
}
