using System;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

// A simple dialog box contains a 'more dialog' indicator, the message and/or a series of choices
public class SimpleDialogBox : DialogBox
{
    Image m_myPanel;
    Text m_dialogLabel;
    Image m_nextDialogImage;

    DialogItem m_currentDialog = null;
	AfterFullDialogShown m_afterShown = null;
	DialogChoiceSelected m_choiceSelected = null;

	int m_selectedIndex = -1;

	public int textColumns;
	public int textRows;
	public string highlightColourName = "white";

    void Awake()
    {
        m_myPanel = GetComponent<Image>();
    }

    void Start()
    {
        m_dialogLabel = GetComponentInChildren<Text>();
		Image[] images = GetComponentsInChildren<Image>();
		for (int i = 0; i < images.Length; ++i) {
			if (images[i].gameObject != gameObject) {
				m_nextDialogImage = images[i];
			}
		}
    }

	/// <summary>
	/// Shows a DialogItem.
	/// Opens the dialog window if it's not already opened.
	/// </summary>
	/// <param name="dialog">The DialogItem to show</param>
	/// <param name="afterShown">Called after the user has advanced past the last line of dialog, but before the dialog box is actually closed. Intended to allow other systems to add subsequent chains of dialog dynamically.</param>
	public override void ShowDialog(DialogItem dialog, AfterFullDialogShown afterShown = null, DialogChoiceSelected choiceSelected = null)
    {
		UnityEngine.Assertions.Assert.IsNotNull(dialog, "SimpleDialogBox: ShowDialog parameter 'dialog' is null");

		OpenWindow();

		m_afterShown = afterShown;
		m_choiceSelected = choiceSelected;
		m_currentDialog = dialog;
		m_selectedIndex = -1;

		RefreshDialog();
		GameManager.Instance.Messenger.SendMessage(this, "DialogBoxOpened");
    }

	/// <summary>
	/// Called when the user intends to advance the dialog to the next one.
	/// </summary>
    public override void Next()
    {
		// You can't go forward if there's no current dialog or the dialog is waiting for a user choice.
		if (null == m_currentDialog) {
			return;
		}
		else if (m_currentDialog.HasChoices()) {
			SelectChoice();
		}
        else if (null != m_currentDialog.nextDialog)
        {
			m_currentDialog = m_currentDialog.nextDialog;
            RefreshDialog();
        }
        else
        {
			if (null != m_afterShown) {
				m_afterShown(m_currentDialog);

				if (null != m_currentDialog.nextDialog) {
					m_currentDialog = m_currentDialog.nextDialog;
					RefreshDialog();
				}
				else {
					CloseDialog();
				}
			}
			else {
            	CloseDialog();
			}
        }
    }

	/// <summary>
	/// Closes the window and clears the current dialog.
	/// </summary>
    public override void CloseDialog()
    {
        CloseWindow();
        m_currentDialog = null;
		GameManager.Instance.Messenger.SendMessage(this, "DialogChoiceSelected");
    }

	/// <summary>
	/// Refreshes the dialog-box based on the contents fo the current dialog item.
	/// </summary>
	void RefreshDialog() {
		m_dialogLabel.text = m_currentDialog.message;

		if (m_currentDialog.HasChoices()) {
			for (int i = 0; i < m_currentDialog.Choices.Count; ++i) {
				m_dialogLabel.text += "\n " + m_currentDialog.Choices[i];
			}

			HighlightChoice(0);
		}
	}

	/// <summary>
	/// Moves the choice selector to the previous choice.
	/// </summary>
	public override void MoveChoicePrevious() {
		HighlightChoice(m_selectedIndex - 1);
	}

	/// <summary>
	/// Moves the choice selector to the next choice.
	/// </summary>
	public override void MoveChoiceNext() {
		HighlightChoice(m_selectedIndex + 1);
	}

	/// <summary>
	/// Selects the current choice.
	/// </summary>
	public override void SelectChoice() {
		if (m_currentDialog.HasChoices() && m_selectedIndex >= 0 && m_selectedIndex < m_currentDialog.Choices.Count) {
			if (m_choiceSelected != null) {
				m_choiceSelected(m_currentDialog, m_selectedIndex);
				m_currentDialog = m_currentDialog.nextDialog;
				RefreshDialog();
			}
		}
	}

	/// <summary>
	/// Shows the dialog box UI.
	/// </summary>
    void OpenWindow()
    {
        m_myPanel.enabled = true;
        m_dialogLabel.enabled = true;
        m_nextDialogImage.enabled = true;
    }

	/// <summary>
	/// Hides the dialog box UI.
	/// </summary>
    void CloseWindow()
    {
        m_myPanel.enabled = false;
        m_dialogLabel.enabled = false;
        m_nextDialogImage.enabled = false;
		m_afterShown = null;
    }

	/// <summary>
	/// Highlights the user's choice.
	/// </summary>
	/// <param name="newChoiceIndex">New choice index.</param>
	void HighlightChoice(int newChoiceIndex) {
		if (m_currentDialog.HasChoices() && newChoiceIndex >= 0 && newChoiceIndex < m_currentDialog.Choices.Count) {
			m_selectedIndex = newChoiceIndex;

			// Highlight the selected line.
			string[] lines = m_dialogLabel.text.Split(new string[] { "\n" }, StringSplitOptions.None);
			int offset = lines.Length - m_currentDialog.Choices.Count;	// Offset by the number of non-choice lines that appear before the choices.
			int lineIndex = newChoiceIndex + offset;
			string newMessage = "";

			// Process each line and either un-highlight it, or highlight it.
			for (int i = 0; i < lines.Length; ++i) {
				string line = "";
				if (lineIndex == i) {
					line = "<color=\"" + highlightColourName + "\">" + lines[i] + "</color>";
				}
				else {
					line = lines[i].Replace("<color=\"" + highlightColourName + "\">", "");
					line = line.Replace("</color>", "");
				}
				newMessage += line;

				// Add a newline if we're not on the last line.
				if (i + i < lines.Length) {
					newMessage += "\n";
				}
			}

			// Update the dialog box.
			m_dialogLabel.text = newMessage;
		}
	}

	/// <summary>
	/// DISABLED. Splits the dialog item based on length. Not functioning properly with choices right now, so this is unused.
	/// </summary>
	/// <returns>The dialog item.</returns>
	/// <param name="dialog">Dialog.</param>
	DialogItem SplitDialogItem(DialogItem dialog) {
		List<string> rows = new List<string>();
		string[] lines = dialog.message.Split(new string[] { "\n" }, StringSplitOptions.None);
	
		// Split the string into lines that fit into the dialog-box-compatible
		for (int i = 0; i < lines.Length; ++i) {
			string newLine = "";

			// Split long lines into multiple rows
			while (lines[i].Length > textColumns) {
				// TODO: Break along word boundaries
				newLine = lines[i].Substring(0, textColumns);
				rows.Add(newLine);

				lines[i] = lines[i].Substring(textColumns);
			}

			rows.Add(lines[i]);
		}

		// Generate new DialogItems from the new rows.
		DialogItem dialogStart = null;
		DialogItem currentItem = null;
		string newMessage = "";

		for (int i = 0; i < rows.Count; ++i) {
			newMessage += rows[i] + "\n";
			if (i % textRows == textRows - 1 || i + 1 == rows.Count) {
				DialogItem newItem = new DialogItem(newMessage);
				if (null == currentItem) {
					currentItem = newItem;
					dialogStart = newItem;
				}
				else {
					currentItem.nextDialog = newItem;
					currentItem = newItem;
				}

				newMessage = "";	
			}
		}

		// If the original message had a next-dialog set, preserve it.
		currentItem.nextDialog = dialog.nextDialog;
		return dialogStart;
	}
}