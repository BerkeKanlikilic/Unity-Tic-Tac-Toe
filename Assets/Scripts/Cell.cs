using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Cell : MonoBehaviour
{
    private bool isClicked = false;
    private PlayerIcon playerIcon = PlayerIcon.Empty;
    public Action<Cell> OnCellClicked;
    
    private Button button;
    private TextMeshProUGUI buttonText;

    public PlayerIcon PlayerCurrentIcon
    {
        get{return playerIcon;}
        set
        {
            playerIcon = value;
            buttonText.text = playerIcon == PlayerIcon.X ? "X" : playerIcon == PlayerIcon.O ? "O" : "";
        }
    }
    
    private void Awake()
    {
        button = GetComponent<Button>();
        buttonText = GetComponentInChildren<TextMeshProUGUI>();
        if (button == null)
        {
            Debug.LogError($"Button component is missing on the cell {gameObject.name}!");
            return;
        }
        if (buttonText == null)
        {
            Debug.LogError($"Text component is missing on the cell {gameObject.name}!");
            return;
        }
    }

    public void OnClick()
    {
        if (isClicked) return;

        isClicked = true;
        OnCellClicked?.Invoke(this);
        button.interactable = false;
    }
}

public enum PlayerIcon
{
    Empty,
    X,
    O,
}