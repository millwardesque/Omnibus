using System;
using UnityEngine;
using UnityEngine.Assertions;
using System.Collections;
using System.Collections.Generic;

public class DialogManager {
	Conversation m_currentConversation = null;
	int m_lineIndex = -1;

	Dictionary<string, Conversation> m_conversations;
	public Dictionary<string, Conversation> Conversations {
		get { return m_conversations; }
	}

	public DialogManager() {
		m_conversations = new Dictionary<string, Conversation>();
	}
		
	public void LoadDialogScript(string scriptResourceName) {
		TextAsset scriptAsset = Resources.Load<TextAsset>(scriptResourceName);
		Assert.IsNotNull(scriptAsset, string.Format("DialogManager: Script asset '{0}' couldn't be loaded", scriptResourceName));

		m_conversations = DialogScriptParser.ParseDialogScript(scriptAsset.text);
	}
	
	public override string ToString () {
		string output = "";

		foreach(KeyValuePair<string, Conversation> entry in m_conversations) {
			output += entry.Value.ToString();
		}

		return output;
	}

	public void RunDialogScript(string scriptName) {
		Assert.IsTrue(m_conversations.ContainsKey(scriptName), string.Format("DialogManager: Unable to run script {0}: No script with that name found.", scriptName));

		m_currentConversation = m_conversations[scriptName];
		m_lineIndex = -1;
		GameManager.Instance.GUI.GameDialogBox.ShowDialog(GetNextDialogItem(null), OnAfterFullDialogShown);
	}

	void OnAfterFullDialogShown(DialogItem item) {
		if (m_currentConversation == null) {
			return;
		}

		GetNextDialogItem(item);
	}

	DialogItem GetNextDialogItem(DialogItem item) {
		if (m_lineIndex + 1 < m_currentConversation.Lines.Count) {
			m_lineIndex++;
				
			DialogItem newItem = new DialogItem(m_currentConversation.Lines[m_lineIndex].Actor + ": " + m_currentConversation.Lines[m_lineIndex].Line);
			if (item == null) {
				item = newItem;
			}
			else {
				item.nextDialog = newItem;
			}
		}
		else if (m_currentConversation.Choices.Count > 0) {
			m_currentConversation = null;
			m_lineIndex = -1;
		}
		else {
			m_currentConversation = null;
			m_lineIndex = -1;
		}

		return item;
	}
}
