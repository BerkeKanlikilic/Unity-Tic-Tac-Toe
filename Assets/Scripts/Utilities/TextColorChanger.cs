using System;
using TMPro;
using UnityEngine;

public class TextColorChanger : MonoBehaviour
{
    [Header("General Settings")]
    [SerializeField] private bool useDefaultColor = false;
    
    [Header("Color Settings")]
    [SerializeField] private Color defaultColor = Color.white;
    [Space(5)]
    [SerializeField] private Color player1Color = Color.red;
    [SerializeField] private Color player2Color = Color.blue;
    [Space(5)]
    [SerializeField] private Color fadeColor = Color.black;
    
    [Header("References")]
    [SerializeField] private TextMeshProUGUI player1Text;
    [SerializeField] private TextMeshProUGUI player2Text;

    private void Awake()
    {
        if (player1Text == null || player2Text == null)
        {
            Debug.LogError("Player text references are not assigned.");
            return;
        }

        // Set initial colors
        player1Text.color = useDefaultColor ? defaultColor : player1Color;
        player2Text.color = fadeColor;
    }

    public void ChangeTurnTextColor(bool isPlayer1Turn)
    {
        if (isPlayer1Turn)
        {
            player1Text.color = useDefaultColor ? defaultColor : player1Color;
            player2Text.color = fadeColor;
        }
        else
        {
            player1Text.color = fadeColor;
            player2Text.color = useDefaultColor ? defaultColor : player2Color;
        }
    }
    
    public void FadeAllText()
    {
        player1Text.color = fadeColor;
        player2Text.color = fadeColor;
    }
}
