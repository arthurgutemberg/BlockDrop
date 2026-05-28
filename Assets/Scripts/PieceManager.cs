using UnityEngine;
using TMPro;
using System.Collections.Generic;

public class PieceManager : MonoBehaviour
{
    [Header("Referências da Cena")]
    public GridManager gridManager;
    public GameObject blockPrefab;
    public Transform currentPieceArea;
    public Transform nextPiecePreviewPos;
    public Transform promisePreviewPos;
    public Transform holdPiecePos;

    [Header("UI")]
    public TMP_Text scoreText;
    public TMP_Text piecesPlacedText;
    public GameObject gameOverPanel;

    // Fila de peças
    private PieceType currentType, nextType, promiseType;
    private Vector2Int[] currentShape, nextShape, promiseShape;
    private DraggablePiece currentPieceInstance;
    private GameObject currentGhost;

    // Hold
    private PieceType? holdType = null;
    private Vector2Int[] holdShape = null;

    private int score = 0;
    private int piecesPlaced = 0;
    private bool gameOver = false;

    void Start()
    {
        GenerateInitialQueue();
        SpawnCurrentPiece();
        UpdateUI();
        gameOverPanel.SetActive(false);
    }

    void GenerateInitialQueue()
    {
        promiseType = (PieceType)Random.Range(0, 7);
        promiseShape = PieceShape.GetRandomShape(promiseType);
        nextType = (PieceType)Random.Range(0, 7);
        nextShape = PieceShape.GetRandomShape(nextType);
        currentType = (PieceType)Random.Range(0, 7);
        currentShape = PieceShape.GetRandomShape(currentType);
    }

    void SpawnCurrentPiece()
    {
        if (currentPieceInstance != null)
            Destroy(currentPieceInstance.gameObject);

        // Destroi o fantasma antigo, se existir
        if (currentGhost != null)
        {
            Destroy(currentGhost);
            currentGhost = null;
        }

        GameObject pieceObj = new GameObject("CurrentPiece");
        pieceObj.transform.position = currentPieceArea.position;
        DraggablePiece dp = pieceObj.AddComponent<DraggablePiece>();
        currentGhost = CreateGhost(currentShape);
        dp.Initialize(currentType, currentShape, currentGhost);
        dp.SetStartPosition(pieceObj.transform.position);
        currentPieceInstance = dp;

        // Verifica se há espaço; se não, tenta usar o hold ou fim de jogo
        if (!HasAnyValidPlacement(currentShape))
        {
            if (holdType == null)
            {
                StoreCurrentInHold();
                AdvanceQueue();
                SpawnCurrentPiece();
            }
            else
            {
                SwapWithHold();
                SpawnCurrentPiece();
            }
            if (currentPieceInstance != null && !HasAnyValidPlacement(currentShape))
            {
                GameOver();
                return;
            }
        }
        UpdatePreviewVisuals();
    }

    bool HasAnyValidPlacement(Vector2Int[] shape)
    {
        for (int x = 0; x < gridManager.width; x++)
            for (int y = 0; y < gridManager.height; y++)
                if (CanPlaceAt(shape, x, y))
                    return true;
        return false;
    }

    bool CanPlaceAt(Vector2Int[] shape, int originX, int originY)
    {
        foreach (var offset in shape)
        {
            int x = originX + offset.x;
            int y = originY + offset.y;
            if (x < 0 || x >= gridManager.width || y < 0 || y >= gridManager.height)
                return false;
            if (gridManager.IsCellOccupied(x, y))
                return false;
        }
        return true;
    }

    // Chamado pelo DraggablePiece quando uma peça é encaixada
    public void OnPiecePlaced()
    {
        // A peça atual foi colocada, não é mais a "atual"
        currentPieceInstance = null;

        piecesPlaced++;
        int lines = gridManager.CheckAndClearLines();
        score += lines switch
        {
            1 => 100,
            2 => 300,
            3 => 500,
            4 => 800,
            _ => 0
        };
        UpdateUI();
        AdvanceQueue();
        SpawnCurrentPiece();
    }

    void AdvanceQueue()
    {
        currentType = nextType;
        currentShape = nextShape;
        nextType = promiseType;
        nextShape = promiseShape;
        promiseType = (PieceType)Random.Range(0, 7);
        promiseShape = PieceShape.GetRandomShape(promiseType);
    }

    void UpdatePreviewVisuals()
    {
        ClearPreview(nextPiecePreviewPos);
        ClearPreview(promisePreviewPos);
        CreatePreview(nextType, nextShape, nextPiecePreviewPos);
        CreatePreview(promiseType, promiseShape, promisePreviewPos);
    }

    void ClearPreview(Transform parent)
    {
        foreach (Transform child in parent)
            Destroy(child.gameObject);
    }

    void CreatePreview(PieceType type, Vector2Int[] shape, Transform parent)
    {
        // Cria um objeto container com escala reduzida
        GameObject container = new GameObject("PreviewPiece");
        container.transform.SetParent(parent, false);
        container.transform.localPosition = Vector3.zero;
        container.transform.localScale = Vector3.one * 0.5f; // reduz 50%

        foreach (var offset in shape)
        {
            GameObject block = Instantiate(blockPrefab, container.transform);
            block.transform.localPosition = new Vector3(offset.x, offset.y, 0);
            block.GetComponent<SpriteRenderer>().color = GetColor(type);
            Destroy(block.GetComponent<BoxCollider2D>());
        }
    }

    GameObject CreateGhost(Vector2Int[] shape)
    {
        GameObject ghost = new GameObject("Ghost");
        ghost.transform.localScale = Vector3.one;
        foreach (var offset in shape)
        {
            GameObject block = Instantiate(blockPrefab, ghost.transform);
            block.transform.localPosition = new Vector3(offset.x, offset.y, 0);
            SpriteRenderer sr = block.GetComponent<SpriteRenderer>();
            sr.color = new Color(1, 1, 1, 0.3f);
            Destroy(block.GetComponent<BoxCollider2D>());
        }
        ghost.SetActive(false);
        return ghost;
    }

    Color GetColor(PieceType type)
    {
        return type switch
        {
            PieceType.I => Color.cyan,
            PieceType.O => Color.yellow,
            PieceType.T => new Color(0.6f, 0.2f, 0.8f),
            PieceType.L => Color.blue,
            PieceType.J => new Color(1f, 0.5f, 0f),
            PieceType.S => Color.green,
            PieceType.Z => Color.red,
            _ => Color.white
        };
    }

    // --- Hold ---
    public void OnHoldButtonClicked()
    {
        if (gameOver) return;
        if (holdType == null)
        {
            StoreCurrentInHold();
            AdvanceQueue();
            SpawnCurrentPiece();
        }
        else
        {
            SwapWithHold();
            SpawnCurrentPiece();
        }
    }

    void StoreCurrentInHold()
    {
        holdType = currentType;
        holdShape = currentShape;
        UpdateHoldVisual();
    }

    void SwapWithHold()
    {
        PieceType tempType = currentType;
        Vector2Int[] tempShape = currentShape;
        currentType = holdType.Value;
        currentShape = holdShape;
        holdType = tempType;
        holdShape = tempShape;
        UpdateHoldVisual();
    }

    void UpdateHoldVisual()
    {
        foreach (Transform child in holdPiecePos)
            Destroy(child.gameObject);
        if (holdType != null && holdShape != null)
        {
            GameObject container = new GameObject("HoldPiece");
            container.transform.SetParent(holdPiecePos, false);
            container.transform.localPosition = Vector3.zero;
            container.transform.localScale = Vector3.one * 0.5f; // reduz 50%

            foreach (var offset in holdShape)
            {
                GameObject block = Instantiate(blockPrefab, container.transform);
                block.transform.localPosition = new Vector3(offset.x, offset.y, 0);
                block.GetComponent<SpriteRenderer>().color = GetColor(holdType.Value);
            }
        }
    }

    void GameOver()
    {
        gameOver = true;
        gameOverPanel.SetActive(true);
        if (currentPieceInstance != null)
            Destroy(currentPieceInstance.gameObject);
        if (currentGhost != null)
            Destroy(currentGhost);
    }

    void UpdateUI()
    {
        scoreText.text = "Score: " + score;
        piecesPlacedText.text = "Peças: " + piecesPlaced;
    }

    public void RestartGame()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene(0);
    }
}