using UnityEngine;
using TMPro;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class PieceManager : MonoBehaviour
{
    [Header("Referências da Cena")]
    public GridManager gridManager;
    public GameObject blockPrefab;
    public Transform currentPieceArea;
    public Transform nextPiecePreviewPos;
    public Transform promisePreviewPos;
    public Transform holdPiecePos;

    public TMP_Text highScoreText; // arraste aqui o texto "High Score" da UI na cena Main
    private int highScore;

    [Header("Escalas")]
    public float currentPieceScale = 0.8f;
    public float previewScale = 0.5f;
    public float holdScale = 0.5f;
    public float ghostScale = 0.8f;
    // opcional: escala das peças já colocadas (se quiser manter 1.0, tudo bem)
    public float fixedPieceScale = 1.0f;

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

    [Header("Aparência")]
    public float pieceScale = 0.8f;   // ajuste o valor como quiser (1.0 = tamanho padrão)

    void Start()
    {
        GenerateInitialQueue();
        SpawnCurrentPiece();
        UpdateUI();
        gameOverPanel.SetActive(false);
        highScore = PlayerPrefs.GetInt("HighScore", 0);
        UpdateHighScoreUI();
    }

    void UpdateHighScoreUI()
    {
        if (highScoreText != null)
            highScoreText.text = "High Score: " + highScore;
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
        // 1. Destroi a peça atual (se existir)
        if (currentPieceInstance != null)
            Destroy(currentPieceInstance.gameObject);

        // 2. Destroi o fantasma antigo para não acumular
        if (currentGhost != null)
        {
            Destroy(currentGhost);
            currentGhost = null;
        }

        // 3. Cria o objeto da nova peça
        GameObject pieceObj = new GameObject("CurrentPiece");
        pieceObj.transform.position = currentPieceArea.position;  // escala visual

        // 4. Adiciona o script DraggablePiece e inicializa
        DraggablePiece dp = pieceObj.AddComponent<DraggablePiece>();
        currentGhost = CreateGhost(currentShape);
        dp.Initialize(currentType, currentShape, currentGhost, currentPieceScale);
        dp.SetStartPosition(pieceObj.transform.position);
        currentPieceInstance = dp;

        // 5. Verifica se a peça atual tem espaço no grid
        if (!HasAnyValidPlacement(currentShape))
        {
            if (holdType == null)
            {
                // Hold vazio → guarda a atual e avança a fila
                StoreCurrentInHold();
                AdvanceQueue();
                SpawnCurrentPiece();
            }
            else
            {
                // Hold ocupado → tenta trocar, mas só se a peça do hold couber
                if (HasAnyValidPlacement(holdShape))
                {
                    SwapWithHold();
                    SpawnCurrentPiece();
                }
                else
                {
                    // Nenhuma das duas cabe → Game Over
                    GameOver();
                    return;
                }
            }

            // Se após a troca a nova peça ainda não couber, Game Over
            if (currentPieceInstance != null && !HasAnyValidPlacement(currentShape))
            {
                GameOver();
                return;
            }
        }

        // 6. Atualiza as pré-visualizações das próximas peças
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
        container.transform.localScale = Vector3.one * previewScale;

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
        foreach (var offset in shape)
        {
            GameObject block = Instantiate(blockPrefab, ghost.transform);
            block.transform.localPosition = new Vector3(offset.x, offset.y, 0);
            block.transform.localScale = Vector3.one * ghostScale;
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
            container.transform.localScale = Vector3.one * holdScale; // reduz 50%

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
        if (gameOver) return;
        UpdateUI(); // garante que o último score seja verificado
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

        // Atualiza high score se necessário
        if (score > highScore)
        {
            highScore = score;
            PlayerPrefs.SetInt("HighScore", highScore);
            PlayerPrefs.Save();
            UpdateHighScoreUI();
        }
    }

    public void RestartGame()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene(0);
    }

    public void GoToMenu()
    {
        SceneManager.LoadScene(0); // título
    }
}