using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GridManager : MonoBehaviour
{
    public int width = 8;
    public int height = 8;
    public GameObject cellPrefab;
    public GameObject blockPrefab;
    public Transform[,] gridCells;
    private Transform[,] occupiedBy;   // guarda qual peça está em cada célula
    private GameObject[,] occupiedBlock; // guarda o GameObject do bloco em cada célula
    public float GridStartX { get; private set; }
    public float GridStartY { get; private set; }
    void Awake()
    {
        occupiedBy = new Transform[width, height];
        gridCells = new Transform[width, height];
        occupiedBlock = new GameObject[width, height];
        DrawGrid();
    }

    void DrawGrid()
    {
        // Cria um objeto para organizar as células como filho do Grid
        GameObject cellsHolder = new GameObject("Cells");
        cellsHolder.transform.parent = transform;
        cellsHolder.transform.localPosition = Vector3.zero; // posição local zero em relação ao Grid

        float offsetX = -width / 2f + 0.5f;
        float offsetY = -height / 2f + 0.5f;
        
        GridStartX = offsetX;
        GridStartY = offsetY;

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                // Instancia a célula como filha do cellsHolder
                GameObject cell = Instantiate(cellPrefab, cellsHolder.transform);
                // Define a posição local em relação ao cellsHolder
                cell.transform.localPosition = new Vector3(x + offsetX, y + offsetY, 0);
                cell.name = $"Cell ({x},{y})";

                SpriteRenderer sr = cell.GetComponent<SpriteRenderer>();
                if ((x + y) % 2 == 0)
                    sr.color = new Color(0.9f, 0.9f, 0.9f, 0.3f);
                else
                    sr.color = new Color(0.8f, 0.8f, 0.8f, 0.3f);

                gridCells[x, y] = cell.transform;
            }
        }
        // Não precisa definir transform.position = ... – a posição do Grid é controlada manualmente
    }

    // Métodos de ocupação
    public bool IsCellOccupied(int x, int y)
    {
        return occupiedBy[x, y] != null;
    }

    public void SetCellOccupied(int x, int y, Transform piece, GameObject block)
    {
        occupiedBy[x, y] = piece;
        occupiedBlock[x, y] = block;
    }

    public void ClearCell(int x, int y)
    {
        occupiedBy[x, y] = null;
        occupiedBlock[x, y] = null;
    }
    public int CheckAndClearFullLinesAndColumns()
    {
        List<int> rowsToClear = new List<int>();
        List<int> colsToClear = new List<int>();

        // Identifica linhas completas
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
            if (full) rowsToClear.Add(y);
        }

        // Identifica colunas completas
        for (int x = 0; x < width; x++)
        {
            bool full = true;
            for (int y = 0; y < height; y++)
            {
                if (!IsCellOccupied(x, y))
                {
                    full = false;
                    break;
                }
            }
            if (full) colsToClear.Add(x);
        }

        // Conjunto para guardar blocos já em fade (evita dupla destruição)
        HashSet<GameObject> blocksToFade = new HashSet<GameObject>();

        // Marca células das linhas
        foreach (int row in rowsToClear)
        {
            for (int x = 0; x < width; x++)
            {
                if (occupiedBlock[x, row] != null)
                    blocksToFade.Add(occupiedBlock[x, row]);
            }
        }

        // Marca células das colunas (sem duplicar)
        foreach (int col in colsToClear)
        {
            for (int y = 0; y < height; y++)
            {
                if (occupiedBlock[col, y] != null)
                    blocksToFade.Add(occupiedBlock[col, y]);
            }
        }

        // Inicia o fade para cada bloco único
        foreach (GameObject block in blocksToFade)
        {
            StartCoroutine(FadeAndDestroy(block, 0.3f));
        }

        // Liberta todas as células marcadas (linhas + colunas)
        foreach (int row in rowsToClear)
        {
            for (int x = 0; x < width; x++)
                ClearCell(x, row);
        }
        foreach (int col in colsToClear)
        {
            for (int y = 0; y < height; y++)
                ClearCell(col, y);
        }

        return rowsToClear.Count + colsToClear.Count;
    }

    IEnumerator FadeAndDestroy(GameObject block, float duration)
    {
        SpriteRenderer sr = block.GetComponent<SpriteRenderer>();
        if (sr == null)
        {
            Destroy(block);
            yield break;
        }

        Color startColor = sr.color;
        Color endColor = new Color(startColor.r, startColor.g, startColor.b, 0f);
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            sr.color = Color.Lerp(startColor, endColor, elapsed / duration);
            yield return null;
        }

        Destroy(block);
    }
}

