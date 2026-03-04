using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class DialogueManager : MonoBehaviour
{
    public Image characterImage;
    public TextMeshProUGUI dialogueText;

    public DialogueLine[] dialogueLines;

    private int currentLine = 0;

    void Start()
    {
        ShowLine();
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0) ||
            Input.GetKeyDown(KeyCode.Z) ||
            Input.GetButtonDown("Submit"))
        {
            NextLine();
        }

        if (Input.anyKeyDown)
        {
            Debug.Log("Key Pressed");
        }
    }

    void ShowLine()
    {
        dialogueText.text = dialogueLines[currentLine].dialogueText;
        characterImage.sprite = dialogueLines[currentLine].characterSprite;
    }

    void NextLine()
    {
        currentLine++;

        if (currentLine < dialogueLines.Length)
        {
            ShowLine();
        }
        else
        {
            SceneManager.LoadScene("level 1");
        }
    }
}
