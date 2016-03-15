using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;

public class DialogScriptParser {
	static Regex commentRegex = new Regex(@"^#"); // # This is a comment
	static Regex conversationRegex = new Regex(@"^:([a-zA-Z0-9_]+)"); // :conversation_id
	static Regex varRegex = new Regex(@"^\!([a-zA-Z0-9_]+)\s*([\+\-\*\\]?=)\s*(.+)");	// var = value, var += 20
	static Regex newDialogRegex = new Regex(@"^([a-zA-Z0-9_]+):\s*(.*)"); // actor: line_1
	static Regex extraDialogRegex = new Regex(@"^\s\s(.+)"); //   line_n [Note the two leading spaces.
	static Regex endConversationRegex = new Regex(@"^\s*(\n|$)"); // Blank line.
	static Regex gotoConversationRegex = new Regex(@"^->([a-zA-Z0-9_]+)"); // Go-To conversation.
	static Regex choiceRegex = new Regex(@"^\$(.+)->([a-zA-Z0-9_]+)"); // User choice.

	public static Dictionary<string, Conversation> ParseDialogScript(string script) {
		string[] lines = script.Split(new string[] { "\n" }, StringSplitOptions.None);

		Match match;
		Dictionary<string, Conversation> conversations = new Dictionary<string, Conversation>();
		Conversation conversation = null;
		ConversationLine conversationLine = null;
		for (int i = 0; i < lines.Length; ++i) {
			string line = lines[i];

			match = DialogScriptParser.commentRegex.Match(line);
			if (match.Success) {
				continue;
			}

			match = DialogScriptParser.conversationRegex.Match(line);
			if (match.Success) {
				if (conversation != null) {
					Debug.LogWarning(string.Format("Parsing dialog script: Line at {0} starts a new conversation, but the old one wasn't closed.", i));
					continue;
				}
				// Debug.Log(string.Format("Starting Conversation {0}", match.Groups[1]));
				conversation = new Conversation(match.Groups[1].ToString());
				continue;
			}

			match = DialogScriptParser.varRegex.Match(line);
			if (match.Success) {
				// Debug.Log(string.Format("@TODO Var: {0} {1} {2}", match.Groups[1], match.Groups[2], match.Groups[3]));
				continue;
			}

			match = DialogScriptParser.newDialogRegex.Match(line);
			if (match.Success) {
				// Close the existing conversation line
				if (conversationLine != null) {
					// Debug.Log(string.Format("Closing conversation line."));
					conversation.AddLine(conversationLine);
				}

				// Start the new conversation line
				conversationLine = new ConversationLine(match.Groups[1].ToString(), match.Groups[2].ToString());
				// Debug.Log(string.Format("Starting new line: {0}: {1}", match.Groups[1], match.Groups[2]));
				continue;
			}

			match = DialogScriptParser.extraDialogRegex.Match(line);
			if (match.Success) {
				if (conversationLine == null) {
					Debug.LogWarning(string.Format("Parsing dialog script: Line at {0} adds to conversation line, but there isn't an active line.", i));
					continue;
				}

				// Append the line.
				conversationLine.Line += "\n" + match.Groups[1].ToString();
				// Debug.Log(string.Format("Continued Dialog: {0}", match.Groups[1]));
				continue;
			}

			match = DialogScriptParser.endConversationRegex.Match(line);
			if (match.Success) {
				if (conversation == null) {
					continue;
				}
				DialogScriptParser.CloseConversation(conversations, ref conversation, ref conversationLine);

				// Debug.Log(string.Format("Ending Conversation"));
				continue;
			}

			match = DialogScriptParser.gotoConversationRegex.Match(line);
			if (match.Success) {
				if (conversation == null) {
					Debug.LogWarning(string.Format("Parsing dialog script: Line at {0} adds GoTo conversation, but there isn't an active conversation.", i));
					continue;
				}

				conversation.OnFinishedConversation = match.Groups[1].ToString();

				DialogScriptParser.CloseConversation(conversations, ref conversation, ref conversationLine);
				// Debug.Log(string.Format("Go To Conversation: {0}", match.Groups[1]));
				continue;
			}

			match = DialogScriptParser.choiceRegex.Match(line);
			if (match.Success) {
				if (conversation == null) {
					Debug.LogWarning(string.Format("Parsing dialog script: Line at {0} adds a choice to the conversation, but there isn't an active conversation.", i));
					continue;
				}

				conversation.AddChoice(new ConversationChoice(match.Groups[1].ToString(), match.Groups[2].ToString()));
				// Debug.Log(string.Format("Adding choice: {0} -> {1}", match.Groups[1], match.Groups[2]));
				continue;
			}
		}

		if (conversation != null) {
			DialogScriptParser.CloseConversation(conversations, ref conversation, ref conversationLine);
		}

		return conversations;
	}

	static void CloseConversation(Dictionary<string, Conversation> conversations, ref Conversation conversation, ref ConversationLine conversationLine) {
		// Close the final conversation line if one is still open
		if (conversationLine != null) {
			conversation.AddLine(conversationLine);
			conversationLine = null;
			// Debug.Log(string.Format("Closing conversation line."));
		}

		conversations.Add(conversation.ID, conversation);
		conversation = null;
	}
}
