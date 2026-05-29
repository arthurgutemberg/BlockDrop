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
            block.transform.localPosition = new Vector3(offset.x, offset.y, 0);
            block.transform.localScale = Vector3.one * scale;   // 🔹 escala do bloco
            block.GetComponent<SpriteRenderer>().color = GetColor(type);
            Destroy(block.GetComponent<BoxCollider2D>());
            blocks.Add(block.transform);
        }
        Physics2D.SyncTransforms();

        // Adiciona um BoxCollider2D que cubra todos os blocos
        // Adiciona um BoxCollider2D que cubra todos os blocos
        BoxCollider2D col = gameObject.AddComponent<BoxCollider2D>();
        // Aguarda um frame para garantir que as posições dos sprites estejam atualizadas
        StartCoroutine(UpdateColliderNextFrame(col));
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
            FindAnyObjectByType<PieceManager>()?.OnPiecePlaced();  // será descomentado na Fase 4
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

    Vector3 GetSnapPosition()
    {
        Vector3 pivotWorld = transform.TransformPoint(Vector3.zero);
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

        // 1. Verificar se todos os blocos cabem (dentro do grid e célula vazia)
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

        // 2. Posicionar a peça e marcá-la como colocada
        transform.position = snapPos;
        isPlaced = true;

        // 3. Registrar ocupação no GridManager (agora com o GameObject do bloco)
        for (int i = 0; i < shapeOffsets.Length; i++)
        {
            Vector2Int offset = shapeOffsets[i];
            int x = gridX + offset.x;
            int y = gridY + offset.y;
            gridManager.SetCellOccupied(x, y, transform, blocks[i].gameObject);
        }

        // 4. Remover o colisor para não interferir em cliques futuros
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
        yield return null; // espera um frame para os Sprites estarem posicionados
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