using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameController : MonoBehaviour
{
    [Header("Timings")]
    [Range(0.1f, 5f)]
    [SerializeField] private float drawFadeInDuration = 1f;
    
    [Header("Colors")]
    [SerializeField] private Color winColor = Color.green;
    
    [Header("References")]
    [SerializeField] private Transform cellParentTransform;
    [SerializeField] private GameObject cellPrefab;
    [SerializeField] private GameObject drawText;
    [SerializeField] private TextMeshProUGUI winText;
    [SerializeField] private GameObject restartButton;
    [SerializeField] private GameObject changePlayerText;
    [Space(5)]
    [SerializeField] private GameObject menuObject;
    [SerializeField] private GameObject gameObject;
    [SerializeField] private GameObject settingsObject;
    
    private Cell[,] cells;
    private List<GameObject> cellObjects;
    
    private bool playerTurn = true; // player 1 = true, player 2 = false
    private int turnCount = 0;
    
    private TextColorChanger textColorChanger;
    
    private static readonly List<Vector2Int[]> winConditions = new List<Vector2Int[]>
    {
        new[] { new Vector2Int(0, 0), new Vector2Int(0, 1), new Vector2Int(0, 2) }, // Top Row
        new[] { new Vector2Int(1, 0), new Vector2Int(1, 1), new Vector2Int(1, 2) }, // Middle Row
        new[] { new Vector2Int(2, 0), new Vector2Int(2, 1), new Vector2Int(2, 2) }, // Bottom Row
        new[] { new Vector2Int(0, 0), new Vector2Int(1, 0), new Vector2Int(2, 0) }, // Left Column
        new[] { new Vector2Int(0, 1), new Vector2Int(1, 1), new Vector2Int(2, 1) }, // Middle Column
        new[] { new Vector2Int(0, 2), new Vector2Int(1, 2), new Vector2Int(2, 2) }, // Right Column
        new[] { new Vector2Int(0, 0), new Vector2Int(1, 1), new Vector2Int(2, 2) }, // Diagonal \
        new[] { new Vector2Int(0, 2), new Vector2Int(1, 1), new Vector2Int(2, 0) }  // Diagonal /
    };

    public bool PlayerTurn
    {
        set
        {
            playerTurn = value;
            textColorChanger.ChangeTurnTextColor(value);
        }
        get
        {
            return playerTurn;
        }
    }

    private void Awake() => textColorChanger = GetComponent<TextColorChanger>();

    private void Start()
    {
        if (cellPrefab == null)
        {
            Debug.LogError("Cell prefab is not assigned.");
            return;
        }

        if (cellParentTransform == null)
        {
            Debug.LogError("Cell parent transform is not assigned.");
            return;
        }
        
        gameObject.SetActive(false);
        settingsObject.SetActive(false);
        changePlayerText.SetActive(false);
        menuObject.SetActive(true);
        
        // InitializeCells();
    }

    private void Update()
    {
        // At any point in the game, if the player presses the escape key and if only the gameObject is active, settingsObject will be active
        if (Input.GetKeyDown(KeyCode.Escape) && gameObject.activeSelf)
        {
            settingsObject.SetActive(!settingsObject.activeSelf);
        }
        
        // If the player presses the enter key and the turnCount is at 0 and settingsObject is not active, the PlayerTurn will be set to opposite value
        if (Input.GetKeyDown(KeyCode.Return) && turnCount == 0 && !settingsObject.activeSelf)
        {
            PlayerTurn = !PlayerTurn;
        }
    }

    public void StartGame()
    {
        gameObject.SetActive(true);
        menuObject.SetActive(false);
        
        InitializeCells();
        winText.gameObject.SetActive(false);
        drawText.SetActive(false);
        restartButton.SetActive(false);
        changePlayerText.SetActive(true);
    }

    public void ResetGame()
    {
        if(settingsObject.activeSelf)
            settingsObject.SetActive(false);
        RemoveCells();
        InitializeCells();
        winText.gameObject.SetActive(false);
        drawText.SetActive(false);
        restartButton.SetActive(false);
        changePlayerText.SetActive(true);
        PlayerTurn = true;
        turnCount = 0;
    }

    public void MainMenu()
    {
        settingsObject.SetActive(false);
        changePlayerText.SetActive(false);
        gameObject.SetActive(false);
        menuObject.SetActive(true);
    }
    
    private void OnACellClicked(Cell cell)
    {
        cell.PlayerCurrentIcon = PlayerTurn ? PlayerIcon.X : PlayerIcon.O;
        
        // Check for win condition
        if (CheckForWinCondition(new Vector2Int(cell.transform.GetSiblingIndex() / 3,
                cell.transform.GetSiblingIndex() % 3))) return;

        // Check for draw condition
        if (CheckForDraw()) return;

        turnCount++;
        PlayerTurn = !PlayerTurn;
        
        if(turnCount > 0)
            changePlayerText.SetActive(false);
    }

    private bool CheckForWinCondition(Vector2 cellPosition)
    {
        // Check if the clicked cell is part of any winning condition
        foreach (var condition in winConditions)
        {
            if (Array.Exists(condition, pos => pos == cellPosition))
            {
                // Check if all cells in the condition have the same player icon, if not return null
                PlayerIcon firstIcon = cells[condition[0].x, condition[0].y].PlayerCurrentIcon;
                if (firstIcon != PlayerIcon.Empty)
                {
                    bool isWinningCondition = true;
                    foreach (var pos in condition)
                    {
                        if (cells[pos.x, pos.y].PlayerCurrentIcon != firstIcon)
                        {
                            isWinningCondition = false;
                            break;
                        }
                    }

                    if (isWinningCondition)
                    {
                        // Highlight winning cells
                        foreach (var pos in condition)
                        {
                            cells[pos.x, pos.y].GetComponent<Image>().color = winColor;
                        }
                        winText.gameObject.SetActive(true);
                        winText.text = $"Player {(PlayerTurn ? "X" : "O")} wins!";
                        restartButton.SetActive(true);
                        DisableAllCells();
                        textColorChanger.FadeAllText();
                        return true;
                    }
                }
            }
        }
        
        // No winning condition found
        return false;
    }

    private bool CheckForDraw()
    {
        // check if the game is a draw
        foreach (var cell in cells)
        {
            if (cell.PlayerCurrentIcon == PlayerIcon.Empty)
            {
                return false;
            }
        }
        // If all cells are filled and no winner, it's a draw
        
        // Lerp drawGroup's alpha from 0 to 1 with a duration of drawFadeInDuration
        drawText.SetActive(true);
        restartButton.SetActive(true);

        DisableAllCells();
        textColorChanger.FadeAllText();
        return true;
    }

    void InitializeCells()
    {
        if (cellObjects != null && cellObjects.Count > 0)
        {
            ResetGame();
            return;
        }
        cells = new Cell[3, 3];
        cellObjects = new List<GameObject>();
        for (int x = 0; x < cells.GetLength(0); x++)
        {
            for (int y = 0; y < cells.GetLength(1); y++)
            {
                GameObject cellObject = Instantiate(cellPrefab, cellParentTransform);
                cellObject.name = x + "," + y;
                Cell cell = cellObject.GetComponent<Cell>();
                cells[x, y] = cell;
                cellObjects.Add(cellObject);
            }
        }

        SubscribeToCells();
    }
    
    void RemoveCells()
    {
        SubscribeToCells(false);
        
        foreach (GameObject cellObject in cellObjects)
        {
            Destroy(cellObject);
        }
        
        cellObjects.Clear();
    }
    
    void DisableAllCells()
    {
        foreach (GameObject cellObject in cellObjects)
        {
            cellObject.GetComponent<Button>().interactable = false;
        }
    }
    
    void SubscribeToCells(bool subscribe = true)
    {
        for (int x = 0; x < cells.GetLength(0); x++)
        {
            for (int y = 0; y < cells.GetLength(1); y++)
            {
                if (subscribe)
                {
                    cells[x, y].OnCellClicked += OnACellClicked;
                }
                else
                {
                    cells[x, y].OnCellClicked -= OnACellClicked;
                }
            }
        }
    }

    private void OnDestroy()
    {
        SubscribeToCells(false);
    }
}
