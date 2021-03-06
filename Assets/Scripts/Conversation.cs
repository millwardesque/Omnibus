﻿using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Holds a 'line' of dialogue spoken by an actor in the conversation.
/// Can consist of more than one on-screen line, but it's spoken all at once by an actor.
/// </summary>
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

/// <summary>
/// Holds a conversation choice that an actor can select. 
/// Intended for use by the player, though there's no reason an AI couldn't use these too.
/// </summary>
public class ConversationChoice {
	string m_label = "";
	public string Label {
		get { return m_label; }
	}

	string m_conversationID = "";
	public string ConversationID {
		get { return m_conversationID; }
	}

	public ConversationChoice(string label, string conversationID) {
		m_label = label;
		m_conversationID = conversationID;
	}

	public override string ToString ()
	{
		return string.Format ("[ConversationChoice: Label={0}, Conversation ID={1}]", Label, ConversationID);
	}
}

/// <summary>
/// A conversation consists of an ID, a mix of lines of dialogue and dialog choices
/// as well as an optional ID  of the next conversation to visit after this one is done.
/// </summary>
public class Conversation {
	string m_id;
	public string ID {
		get { return m_id; }
	}

	List<ConversationLine> m_lines;
	public List<ConversationLine> Lines {
		get { return m_lines; }
	}

	List<ConversationChoice> m_choices;
	public List<ConversationChoice> Choices {
		get { return m_choices; }
	}

	string m_onFinishedConversation = "";
	public string OnFinishedConversation {
		get { return m_onFinishedConversation; }
		set { m_onFinishedConversation = value; }
	}

	public Conversation(string id) {
		m_id = id;
		m_lines = new List<ConversationLine>();
		m_choices = new List<ConversationChoice>();
	}

	public void AddLine(ConversationLine line) {
		m_lines.Add(line);
	}

	public void AddChoice(ConversationChoice choice) {
		m_choices.Add(choice);
	}

	public override string ToString() {
		string output = ID + "\n";

		for (int i = 0; i < m_lines.Count; ++i) {
			output += m_lines[i].ToString();
			if (i + 1 < m_lines.Count) {
				output += "\n\n";
			}
		}

		for (int i = 0; i < m_choices.Count; ++i) {
			output += m_choices[i].ToString();
			if (i + 1 < m_choices.Count) {
				output += "\n\n";
			}
		}

		return output;
	}
}