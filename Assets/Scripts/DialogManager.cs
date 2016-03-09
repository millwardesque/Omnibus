using System;
using UnityEngine;
using UnityEngine.Assertions;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

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
	static Regex commentRegex = new Regex(@"^#"); // # This is a comment
	static Regex conversationRegex = new Regex(@"^:([a-zA-Z0-9_]+)"); // :conversation_id
	static Regex varRegex = new Regex(@"^\!([a-zA-Z0-9_]+)\s*([\+\-\*\\]?=)\s*(.+)");	// var = value, var += 20
	static Regex newDialogRegex = new Regex(@"^([a-zA-Z0-9_]+):\s*(.*)");
	static Regex extraDialogRegex = new Regex(@"^\s\s(.+)");
	static Regex endConversationRegex = new Regex(@"^\s*(\n|$)");

	Dictionary<string, Conversation> m_conversations;
	public Dictionary<string, Conversation> Conversations {
		get { return m_conversations; }
	}

	public DialogManager() {
		m_conversations = new Dictionary<string, Conversation>();
	}

	public void ParseDialogScript(string resourceName) {
		TextAsset scriptAsset = Resources.Load<TextAsset>(resourceName);
		Assert.IsNotNull(scriptAsset, string.Format("DialogManager: Script asset '{0}' couldn't be loaded", resourceName));

		string script = scriptAsset.text;
		string[] lines = script.Split(new string[] { "\n" }, StringSplitOptions.None);

		Match match;
		Conversation conversation = null;
		ConversationLine conversationLine = null;
		for (int i = 0; i < lines.Length; ++i) {
			string line = lines[i];

			match = DialogManager.commentRegex.Match(line);
			if (match.Success) {
				continue;
			}

			match = DialogManager.conversationRegex.Match(line);
			if (match.Success) {
				if (conversation != null) {
					Debug.LogWarning(string.Format("Parsing dialog script {0}: Line at {1} starts a new conversation, but the old one wasn't closed.", resourceName, i));
					continue;
				}
				Debug.Log(string.Format("Starting Conversation {0}", match.Groups[1]));
				conversation = new Conversation(match.Groups[1].ToString());
				continue;
			}

			match = DialogManager.varRegex.Match(line);
			if (match.Success) {
				Debug.Log(string.Format("@TODO Var: {0} {1} {2}", match.Groups[1], match.Groups[2], match.Groups[3]));
				continue;
			}

			match = DialogManager.newDialogRegex.Match(line);
			if (match.Success) {
				// Close the existing conversation line
				if (conversationLine != null) {
					Debug.Log(string.Format("Closing conversation line."));
					conversation.AddLine(conversationLine);
				}

				// Start the new conversation line
				conversationLine = new ConversationLine(match.Groups[1].ToString(), match.Groups[2].ToString());
				Debug.Log(string.Format("Starting new line: {0}: {1}", match.Groups[1], match.Groups[2]));
				continue;
			}

			match = DialogManager.extraDialogRegex.Match(line);
			if (match.Success) {
				if (conversationLine == null) {
					Debug.LogWarning(string.Format("Parsing dialog script {0}: Line at {1} adds to conversation line, but there isn't an active line.", resourceName, i));
					continue;
				}

				// Append the line.
				conversationLine.Line += "\n" + match.Groups[1].ToString();
				Debug.Log(string.Format("Continued Dialog: {0}", match.Groups[1]));
				continue;
			}

			match = DialogManager.endConversationRegex.Match(line);
			if (match.Success) {
				if (conversationLine == null) {
					Debug.LogWarning(string.Format("Parsing dialog script {0}: Line at {1} ends conversation, but there isn't an active conversation.", resourceName, i));
					continue;
				}

				// Close the final conversation line if one is still open
				if (line != null) {
					conversation.AddLine(conversationLine);
					conversationLine = null;
					Debug.Log(string.Format("Closing conversation line."));
				}

				m_conversations.Add(conversation.ID, conversation);
				conversation = null;

				Debug.Log(string.Format("Ending Conversation"));
				continue;
			}
		}

		Debug.Log(ToString());
	}
	
	public override string ToString () {
		string output = "";

		foreach(KeyValuePair<string, Conversation> entry in m_conversations) {
			output += entry.Value.ToString();
		}

		return output;
	}
}
