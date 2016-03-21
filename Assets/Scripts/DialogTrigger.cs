using UnityEngine;
using System.Collections;

public class DialogTrigger : MonoBehaviour {
    public string scriptResourceName = "";
    public string scriptName = "";
    public bool triggerOnce = true;
    public bool pauseOnEnter = false;

    bool m_isTriggered = false;

    void Start()
    {
        GameManager.Instance.Messenger.AddListener("DialogScriptFinished", OnDialogScriptFinished);
    }

    void OnTriggerEnter(Collider col)
    {
        if (triggerOnce)
        {
            if (m_isTriggered)
            {
                return;
            }
            else
            {
                m_isTriggered = true;
            }
        }

        Debug.Log(string.Format("Dialog Trigger entered: '{0}' => '{1}'", scriptResourceName, scriptName));
        if (scriptResourceName != "" && scriptName != "")
        {
            GameManager.Instance.Dialog.LoadDialogScript(scriptResourceName);
            GameManager.Instance.Dialog.RunDialogScript(scriptName);

            if (pauseOnEnter)
            {
                Time.timeScale = 0;
            }
        }
    }

    void OnDialogScriptFinished(Message message)
    {
        if (pauseOnEnter && (string)message.data == scriptResourceName)
        {
            Time.timeScale = 1f;
        }
    }
}
