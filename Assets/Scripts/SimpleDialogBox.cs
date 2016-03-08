using UnityEngine;
using UnityEngine.UI;

public class SimpleDialogBox : DialogBox
{
    Image m_myPanel;
    Text m_dialogLabel;
    Image m_nextDialogImage;

    DialogItem m_currentDialog = null;
    bool m_isVisible = false;

    void Awake()
    {
        m_myPanel = GetComponent<Image>();
    }

    void Start()
    {
        m_dialogLabel = GetComponentInChildren<Text>();
        m_nextDialogImage = GetComponentInChildren<Image>();
    }

    public override void ShowDialog(DialogItem dialog)
    {
        if (dialog != null)
        {
            OpenWindow();

            m_currentDialog = dialog;
            m_dialogLabel.text = dialog.message;
        }
    }

    public override void Next()
    {
        if (null != m_currentDialog.nextDialog)
        {
            ShowDialog(m_currentDialog.nextDialog);
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
}