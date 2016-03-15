using System;
using UnityEngine;
using UnityEngine.Assertions;
using System.Collections;
using System.Collections.Generic;

public class DialogManager {
	Conversation m_currentConversation = null;
	int m_lineIndex = -1;

	/// <summary>
	/// All the conversations in the loaded script, keyed by conversation ID.
	/// </summary>
	Dictionary<string, Conversation> m_conversations;
	public Dictionary<string, Conversation> Conversations {
		get { return m_conversations; }
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="DialogManager"/> class.
	/// </summary>
	public DialogManager() {
		m_conversations = new Dictionary<string, Conversation>();
	}

	/// <summary>
	/// Loads the dialog script from a Unity text resource.
	/// </summary>
	/// <param name="scriptResourceName">Script resource name.</param>
	public void LoadDialogScript(string scriptResourceName) {
		TextAsset scriptAsset = Resources.Load<TextAsset>(scriptResourceName);
		Assert.IsNotNull(scriptAsset, string.Format("DialogManager: Script asset '{0}' couldn't be loaded", scriptResourceName));

		m_conversations = DialogScriptParser.ParseDialogScript(scriptAsset.text);
	}

	/// <summary>
	/// Returns a <see cref="System.String"/> that represents the current <see cref="DialogManager"/>.
	/// </summary>
	/// <returns>A <see cref="System.String"/> that represents the current <see cref="DialogManager"/>.</returns>
	public override string ToString () {
		string output = "";

		foreach(KeyValuePair<string, Conversation> entry in m_conversations) {
			output += entry.Value.ToString();
			output += "\n";
		}

		return output;
	}

	/// <summary>
	/// Runs the dialog script in-game using dialog boxes, etc.
	/// </summary>
	/// <param name="scriptName">Script name.</param>
	public void RunDialogScript(string scriptName) {
		SelectDialogScript(scriptName);
		GameManager.Instance.GUI.GameDialogBox.ShowDialog(GetNextDialogItem(null), OnAfterFullDialogShown, OnDialogChoiceSelected);
	}

	/// <summary>
	/// Called whenever a dialog choice gets made.
	/// </summary>
	/// <param name="dialogItem">Dialog item.</param>
	/// <param name="choiceIndex">Choice index.</param>
	public void OnDialogChoiceSelected(DialogItem dialogItem, int choiceIndex) {
		Assert.IsNotNull(m_currentConversation, string.Format("DialogManager: OnDialogChoiceSelected '{0}': No current conversation is set.", choiceIndex));
		Assert.IsTrue(m_conversations.ContainsKey(m_currentConversation.Choices[choiceIndex].ConversationID), string.Format("DialogManager: OnDialogChoiceSelected '{0}': No choice with that index was found.", choiceIndex));
		string conversationID = m_currentConversation.Choices[choiceIndex].ConversationID;

		SelectDialogScript(conversationID);
		GetNextDialogItem(dialogItem);
	}

	/// <summary>
	/// Selects the dialog script that is actively running
	/// </summary>
	/// <param name="conversationID">Conversation I.</param>
	void SelectDialogScript(string conversationID) {
		Assert.IsTrue(m_conversations.ContainsKey(conversationID), string.Format("DialogManager: Unable to run script {0}: No script with that name found.", conversationID));

		m_currentConversation = m_conversations[conversationID];
		m_lineIndex = -1;
	}

	/// <summary>
	/// Resets the selected dialog script to nothing.
	/// </summary>
	void DeselectDialogScript() {
		m_currentConversation = null;
		m_lineIndex = -1;
	}

	/// <summary>
	/// Callback for once a full dialog script has been shown
	/// </summary>
	/// <param name="item">Item.</param>
	void OnAfterFullDialogShown(DialogItem item) {
		if (m_currentConversation == null) {
			return;
		}

		GetNextDialogItem(item);
	}

	DialogItem GetNextDialogItem(DialogItem item) {
		if (m_currentConversation.Choices.Count > 0 && (item == null || item.Choices == null)) {
			List<string> choices = new List<string>();
			for (int i = 0; i < m_currentConversation.Choices.Count; i++) {
				choices.Add(m_currentConversation.Choices[i].Label);
			}

			string questionSummary = "";
			if (m_currentConversation.Lines.Count > 0) {
				questionSummary = m_currentConversation.Lines[0].Actor + ": " + m_currentConversation.Lines[0].Line;
			}
			DialogItem newItem = new DialogItem(questionSummary, choices);
			if (item == null) {
				item = newItem;
			}
			else {
				item.nextDialog = newItem;
			}
		}
		else if (m_lineIndex + 1 < m_currentConversation.Lines.Count) {
			m_lineIndex++;
				
			DialogItem newItem = new DialogItem(m_currentConversation.Lines[m_lineIndex].Actor + ": " + m_currentConversation.Lines[m_lineIndex].Line);
			if (item == null) {
				item = newItem;
			}
			else {
				item.nextDialog = newItem;
			}
		}
		else if (m_currentConversation.OnFinishedConversation != "") {
			SelectDialogScript(m_currentConversation.OnFinishedConversation);
			item = GetNextDialogItem(item);
		}
		else {
			DeselectDialogScript();
		}

		return item;
	}
}
