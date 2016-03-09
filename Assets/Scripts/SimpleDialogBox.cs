using System;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class SimpleDialogBox : DialogBox
{
    Image m_myPanel;
    Text m_dialogLabel;
    Image m_nextDialogImage;

    DialogItem m_currentDialog = null;
    bool m_isVisible = false;

	public int textColumns;
	public int textRows;

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

    public override void ShowDialog(DialogItem dialog)
    {
		UnityEngine.Assertions.Assert.IsNotNull(dialog, "SimpleDialogBox: ShowDialog parameter 'dialog' is null");

		OpenWindow();

		m_currentDialog = SplitDialogItem(dialog);
		RefreshDialog();
		GameManager.Instance.Messenger.SendMessage(this, "DialogBoxOpened");
    }

    public override void Next()
    {
		if (null == m_currentDialog) {
			return;
		}
        else if (null != m_currentDialog.nextDialog)
        {
			m_currentDialog = m_currentDialog.nextDialog;
            RefreshDialog();
        }
        else
        {
            CloseDialog();
        }
    }

    public override void CloseDialog()
    {
        CloseWindow();
        m_currentDialog = null;
		GameManager.Instance.Messenger.SendMessage(this, "DialogBoxClosed");
    }

	void RefreshDialog() {
		m_dialogLabel.text = m_currentDialog.message;
	}

    void OpenWindow()
    {
        if (!m_isVisible)
        {
            m_myPanel.enabled = true;
            m_dialogLabel.enabled = true;
            m_nextDialogImage.enabled = true;
            m_isVisible = true;
        }
    }

    void CloseWindow()
    {
        if (m_isVisible)
        {
            m_myPanel.enabled = false;
            m_dialogLabel.enabled = false;
            m_nextDialogImage.enabled = false;
            m_isVisible = false;
        }
    }

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