using System;
using UnityEngine;
using UnityEngine.Assertions;
using System.Collections;
using System.Collections.Generic;

public class ConversationLine {
	string m_actor;
	public string Actor {
		get { return m_actor; }
	}

	string m_line = "";
	public string Line {
		get { return m_line; }
		set {
			m_line = value;
		}
	}

	public ConversationLine(string actor, string line) {
		m_actor = actor;
		m_line = line;
	}

	public override string ToString ()
	{
		return string.Format ("[ConversationLine: Actor={0}, Line={1}]", Actor, Line);
	}
}

public class Conversation {
	string m_id;
	public string ID {
		get { return m_id; }
	}

	List<ConversationLine> m_lines;
	public List<ConversationLine> Lines {
		get { return m_lines; }
	}

	public Conversation(string id) {
		m_id = id;
		m_lines = new List<ConversationLine>();
	}

	public void AddLine(ConversationLine line) {
		m_lines.Add(line);
	}

	public override string ToString() {
		string output = ID + "\n";

		for (int i = 0; i < m_lines.Count; ++i) {
			output += m_lines[i].ToString();
			if (i + 1 < m_lines.Count) {
				output += "\n\n";
			}
		}

		return output;
	}
}

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
