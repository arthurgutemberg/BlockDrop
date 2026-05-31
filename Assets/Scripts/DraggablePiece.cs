using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using System.Collections;

public class DraggablePiece : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public bool isPlaced = false;
    private Vector3 offset;
    private GridManager gridManager;
    private List<Transform> blocks = new List<Transform>();
    private PieceType pieceType;
    private Vector2Int[] shapeOffsets;
    private GameObject ghostPrefab;
    private Vector3 startPosition;
    private Camera cam;
    [HideInInspector] public GameObject placeParticlesPrefab;

    void Awake()
    {
        cam = Camera.main;
        gridManager = FindAnyObjectByType<GridManager>();
    }

    public void Initialize(PieceType type, Vector2Int[] shape, GameObject ghost, float scale)
    {
        blocks.Clear();
        pieceType = type;
        shapeOffsets = shape;
        ghostPrefab = ghost;

        Destroy(GetComponent<BoxCollider2D>());

        foreach (var offset in shape)
        {
            GameObject block = Instantiate(gridManager.blockPrefab, transform);
            // Posição local sem centralização (usando offsets originais)
            block.transform.localPosition = new Vector3(offset.x, offset.y, 0);
            block.transform.localScale = Vector3.one * scale;
            block.GetComponent<SpriteRenderer>().color = GetColor(type);
            Destroy(block.GetComponent<BoxCollider2D>());
            blocks.Add(block.transform);
        }
        Physics2D.SyncTransforms();

        // Colisor ajustado com corrotina (mantido)
        BoxCollider2D col = gameObject.AddComponent<BoxCollider2D>();
        StartCoroutine(UpdateColliderNextFrame(col));
    }

    // Cor de cada tipo (mantida)
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
            PieceType.Dot    => new Color(1f, 0.08f, 0.58f), // Rosa Choque / Deep Pink
            PieceType.Domino => new Color(0.85f, 0.65f, 0.13f), // Dourado / Goldenrod
            PieceType.Trio   => new Color(0f, 1f, 0.5f), // Verde Menta / Spring Green
            _ => Color.white
        };
    }

    // Métodos de arrasto (inalterados)
    public void OnBeginDrag(PointerEventData eventData)
    {
        if (isPlaced) return;
        Vector3 worldPos = cam.ScreenToWorldPoint(eventData.position);
        offset = transform.position - worldPos;
        if (ghostPrefab != null)
            ghostPrefab.SetActive(true);
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (isPlaced) return;
        Vector3 worldPos = cam.ScreenToWorldPoint(eventData.position);
        transform.position = worldPos + offset;
        UpdateGhostPosition();
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (isPlaced) return;
        if (TryPlace())
        {
            ghostPrefab?.SetActive(false);
            FindAnyObjectByType<PieceManager>()?.OnPiecePlaced();
        }
        else
        {
            transform.position = startPosition;
            ghostPrefab?.SetActive(false);
        }
    }

    void UpdateGhostPosition()
    {
        if (ghostPrefab == null) return;
        Vector3 snapPos = GetSnapPosition();
        ghostPrefab.transform.position = snapPos;
    }

    // SNAP: usa o primeiro bloco (canto mínimo original) como referência
    Vector3 GetSnapPosition()
    {
        Vector3 pivotWorld = transform.TransformPoint(Vector3.zero); // (0,0) local é o canto mínimo
        Vector3 gridOrigin = gridManager.transform.position + new Vector3(gridManager.GridStartX, gridManager.GridStartY, 0);
        int col = Mathf.RoundToInt(pivotWorld.x - gridOrigin.x);
        int row = Mathf.RoundToInt(pivotWorld.y - gridOrigin.y);
        return new Vector3(gridOrigin.x + col, gridOrigin.y + row, 0);
    }

    bool TryPlace()
    {
        Vector3 snapPos = GetSnapPosition();
        int gridX = Mathf.RoundToInt(snapPos.x - (gridManager.transform.position.x + gridManager.GridStartX));
        int gridY = Mathf.RoundToInt(snapPos.y - (gridManager.transform.position.y + gridManager.GridStartY));

        // Verifica células usando shapeOffsets originais
        for (int i = 0; i < shapeOffsets.Length; i++)
        {
            Vector2Int offset = shapeOffsets[i];
            int x = gridX + offset.x;
            int y = gridY + offset.y;

            if (x < 0 || x >= gridManager.width || y < 0 || y >= gridManager.height)
                return false;

            if (gridManager.IsCellOccupied(x, y))
                return false;
        }

        transform.position = snapPos;
        isPlaced = true;

        // Registra ocupação
        for (int i = 0; i < shapeOffsets.Length; i++)
        {
            Vector2Int offset = shapeOffsets[i];
            int x = gridX + offset.x;
            int y = gridY + offset.y;
            gridManager.SetCellOccupied(x, y, transform, blocks[i].gameObject);
        }

        // Partículas com 50% de chance por bloco
        if (placeParticlesPrefab != null)
        {
            foreach (Transform block in blocks)
            {
                if (Random.value < 0.5f)
                    Instantiate(placeParticlesPrefab, block.position, Quaternion.identity);
            }
        }

        Destroy(GetComponent<BoxCollider2D>());
        return true;
    }

    public void SetStartPosition(Vector3 pos)
    {
        startPosition = pos;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        Debug.Log("Clique na peça!");
    }

    System.Collections.IEnumerator UpdateColliderNextFrame(BoxCollider2D col)
    {
        yield return null;
        Bounds bounds = new Bounds(transform.position, Vector3.zero);
        foreach (Transform block in blocks)
        {
            SpriteRenderer sr = block.GetComponent<SpriteRenderer>();
            if (sr != null)
                bounds.Encapsulate(sr.bounds);
        }
        col.size = bounds.size;
        col.offset = bounds.center - transform.position;
    }
}