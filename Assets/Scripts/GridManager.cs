using UnityEngine;

public class GridManager : MonoBehaviour
{
    public int width = 8;
    public int height = 8;
    public GameObject cellPrefab;
    public GameObject blockPrefab;
    public Transform[,] gridCells;
    private Transform[,] occupiedBy;   // guarda qual peça está em cada célula

    void Start()
    {
        occupiedBy = new Transform[width, height];
        gridCells = new Transform[width, height];
        DrawGrid();
    }

    void DrawGrid()
    {
        GameObject cellsHolder = new GameObject("Cells");
        cellsHolder.transform.parent = transform;

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                GameObject cell = Instantiate(cellPrefab, new Vector3(x, y, 0), Quaternion.identity);
                cell.name = $"Cell ({x},{y})";
                cell.transform.parent = cellsHolder.transform;

                SpriteRenderer sr = cell.GetComponent<SpriteRenderer>();
                if ((x + y) % 2 == 0)
                    sr.color = new Color(0.9f, 0.9f, 0.9f, 0.3f);
                else
                    sr.color = new Color(0.8f, 0.8f, 0.8f, 0.3f);

                gridCells[x, y] = cell.transform;
            }
        }

        transform.position = new Vector3(-5f, -3.5f, 0);
    }

    // Métodos de ocupação
    public bool IsCellOccupied(int x, int y)
    {
        return occupiedBy[x, y] != null;
    }

    public void SetCellOccupied(int x, int y, Transform piece)
    {
        occupiedBy[x, y] = piece;
    }

    public void ClearCell(int x, int y)
    {
        occupiedBy[x, y] = null;
    }

    // Limpa visualmente os blocos de uma linha e libera as células
    public void ClearLine(int row)
    {
        for (int x = 0; x < width; x++)
        {
            if (occupiedBy[x, row] != null)
            {
                Transform pieceParent = occupiedBy[x, row];
                // Procura o bloco filho exatamente nesta posição e o destrói
                foreach (Transform block in pieceParent)
                {
                    Vector3 worldPos = block.position;
                    int bx = Mathf.RoundToInt(worldPos.x - transform.position.x);
                    int by = Mathf.RoundToInt(worldPos.y - transform.position.y);
                    if (bx == x && by == row)
                    {
                        Destroy(block.gameObject);
                        break;
                    }
                }
                ClearCell(x, row);
            }
        }
    }

    // Verifica todas as linhas, limpa as completas e retorna quantas foram removidas
    public int CheckAndClearLines()
    {
        int cleared = 0;
        for (int y = height - 1; y >= 0; y--)
        {
            bool full = true;
            for (int x = 0; x < width; x++)
            {
                if (!IsCellOccupied(x, y))
                {
                    full = false;
                    break;
                }
            }
            if (full)
            {
                ClearLine(y);
                cleared++;
            }
        }
        return cleared;
    }
}