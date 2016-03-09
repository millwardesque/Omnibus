using System;
using UnityEngine;
using UnityEngine.Assertions;
using System.Collections;
using System.Collections.Generic;

public class DialogManager {
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

		Conversation conversation = m_conversations[scriptName];
		DialogItem head = null;
		DialogItem item = null;
		for (int i = 0; i < conversation.Lines.Count; ++i) {
			DialogItem newItem = new DialogItem(conversation.Lines[i].Actor + ": " + conversation.Lines[i].Line);
			if (item == null) {
				head = newItem;
				item = newItem;
			}
			else {
				item.nextDialog = newItem;
				item = newItem;
			}
		}

		GameManager.Instance.GUI.GameDialogBox.ShowDialog(head);
	}
}
