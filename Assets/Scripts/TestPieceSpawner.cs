using UnityEngine;

public class TestPieceSpawner : MonoBehaviour
{
    public GridManager gridManager;   // arraste o objeto Grid aqui

    void Start()
    {
        // Escolhe uma peça aleatória
        PieceType type = (PieceType)Random.Range(0, 7);
        Vector2Int[] shape = PieceShape.GetRandomShape(type);

        // Cria o GameObject da peça
        GameObject pieceObj = new GameObject("TestPiece");
        DraggablePiece dp = pieceObj.AddComponent<DraggablePiece>();

        // Cria um fantasma simples
        GameObject ghost = CreateGhost(shape);
        ghost.SetActive(false);

        // Inicializa a peça
        dp.Initialize(type, shape, ghost);

        // Define a posição inicial (à direita da grade)
        Vector3 startPos = new Vector3(3, 0, 0);
        pieceObj.transform.position = startPos;
        dp.SetStartPosition(startPos);
    }

    GameObject CreateGhost(Vector2Int[] shape)
    {
        GameObject ghost = new GameObject("Ghost");
        foreach (var offset in shape)
        {
            GameObject block = Instantiate(gridManager.blockPrefab, ghost.transform);
            block.transform.localPosition = new Vector3(offset.x, offset.y, 0);
            SpriteRenderer sr = block.GetComponent<SpriteRenderer>();
            sr.color = new Color(1, 1, 1, 0.3f);
            Destroy(block.GetComponent<BoxCollider2D>());
        }
        return ghost;
    }
}