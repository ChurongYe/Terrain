using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class TileData
{
    public GameObject tilePrefab; 
    public int weight = 1;
    public bool[] topEdge = new bool[3];
    public bool[] rightEdge = new bool[3];
    public bool[] bottomEdge = new bool[3];
    public bool[] leftEdge = new bool[3];

    public bool[][] GetRotatedEdges(int rotationAngle)
    {
        bool[][] edges = new bool[][] { topEdge, rightEdge, bottomEdge, leftEdge };
        int rotations = (rotationAngle / 90) % 4;
        bool[][] rotatedEdges = new bool[4][];

        for (int i = 0; i < 4; i++)
        {
            int newIndex = (i - rotations + 4) % 4;
            rotatedEdges[i] = new bool[3];

            if (rotationAngle == 270 || rotationAngle == 0)
            {
                rotatedEdges[i] = edges[newIndex];
            }
            else
            {
                rotatedEdges[i][0] = edges[newIndex][2]; 
                rotatedEdges[i][1] = edges[newIndex][1]; 
                rotatedEdges[i][2] = edges[newIndex][0]; 
            }
        }

        return rotatedEdges;
    }
}

public class TileInstance
{
    public TileData tileData;
    public int rotation;

    public TileInstance(TileData tile, int rot)
    {
        tileData = tile;
        rotation = rot;
    }

    public bool[][] GetEdges()
    {
        return tileData.GetRotatedEdges(rotation);
    }
}

public class Tilemap : MonoBehaviour
{
    public List<TileData> tileDatas = new List<TileData>();
    public int mapWidth = 5;
    public int mapHeight = 5;
    public float tileSize = 1f;
    private TileInstance[,] placedTiles;
    private List<Vector2Int> availablePositions; 
    private Stack<Vector2Int> placedTileStack = new Stack<Vector2Int>();
    public IEnumerator TileGenerate()
    {
        if (tileDatas.Count == 0)
        {
            yield return null;
        }

        foreach (Transform child in transform)
        {
            Destroy(child.gameObject);
        }

        placedTiles = new TileInstance[mapWidth, mapHeight];
        availablePositions = new List<Vector2Int>();

        for (int x = 0; x < mapWidth; x++)
        {
            for (int y = 0; y < mapHeight; y++)
            {
                availablePositions.Add(new Vector2Int(x, y));
            }
        }

        GenerateMap();
        yield return null;
    }

    void GenerateMap()
    {
        while (availablePositions.Count > 0)
        {
            Vector2Int position = GetLowestEntropyPosition();
            if (!TryPlaceTile(position.x, position.y))
            {
                bool backtracked = false;
                while (placedTileStack.Count > 0)
                {
                    Vector2Int lastPlaced = placedTileStack.Pop();
                    RemoveTile(lastPlaced.x, lastPlaced.y);

                    if (TryPlaceTile(lastPlaced.x, lastPlaced.y))
                    {
                        backtracked = true;
                        break;
                    }
                }

                if (!backtracked)
                {
                    Debug.LogError("faliture");
                    break;
                }
            }
        }
    }
    void RemoveTile(int x, int y)
    {
        if (placedTiles[x, y] != null)
        {
            Transform tileTransform = transform.Find($"{x},{y}");
            if (tileTransform != null)
            {
                Destroy(tileTransform.gameObject); 
            }
            placedTiles[x, y] = null; 
            availablePositions.Add(new Vector2Int(x, y)); 
        }
    }
    Vector2Int GetLowestEntropyPosition()
    {
        Vector2Int lowestEntropyPos = availablePositions[0];
        int lowestEntropy = int.MaxValue;

        foreach (Vector2Int position in availablePositions)
        {
            int entropy = CalculateEntropy(position);
            if (entropy < lowestEntropy)
            {
                lowestEntropy = entropy;
                lowestEntropyPos = position;
            }
        }

        availablePositions.Remove(lowestEntropyPos);
        return lowestEntropyPos;
    }

    int CalculateEntropy(Vector2Int position)
    {
        List<(TileData, int)> validTiles = GetValidTilesAtPosition(position);
        return validTiles.Count;
    }

    List<(TileData, int)> GetValidTilesAtPosition(Vector2Int position)
    {
        List<(TileData, int)> validTiles = new List<(TileData, int)>(); 

        foreach (TileData tile in tileDatas)
        {
            for (int rotation = 0; rotation < 4; rotation++) 
            {
                bool[][] rotatedEdges = tile.GetRotatedEdges(rotation * 90);
                if (CanPlaceTile(position.x, position.y, rotatedEdges))
                {
                    validTiles.Add((tile, rotation * 90));
                }
            }
        }

        return validTiles;
    }

    bool TryPlaceTile(int x, int y)
    {
        List<(TileData, int)> validTiles = GetValidTilesAtPosition(new Vector2Int(x, y));

        if (validTiles.Count > 0)
        {
            var (selectedTile, selectedRotation) = WeightedRandomSelection(validTiles);
            PlaceTile(x, y, selectedTile, selectedRotation);
            return true;
        }
        else
        {
            Debug.LogWarning($"no tile in ({x}, {y})£¬try...");
            var bestFallbackTile = GetBestFallbackTile(x, y);
            if (bestFallbackTile.tileData != null)
            {
                PlaceTile(x, y, bestFallbackTile.tileData, bestFallbackTile.rotation);
                return true;
            }

            return false;
        }
    }
    (TileData, int) WeightedRandomSelection(List<(TileData, int)> tileList)
    {
        int totalWeight = 0;
        foreach (var (tile, _) in tileList)
        {
            totalWeight += tile.weight;
        }

        int randomValue = Random.Range(0, totalWeight);
        int cumulativeWeight = 0;

        foreach (var (tile, rotation) in tileList)
        {
            cumulativeWeight += tile.weight;
            if (randomValue < cumulativeWeight)
            {
                return (tile, rotation);
            }
        }

        return tileList[tileList.Count - 1]; 
    }
    (TileData tileData, int rotation) GetBestFallbackTile(int x, int y)
    {
        TileData bestTile = null;
        int bestRotation = 0;
        int maxMatchingEdges = -1;

        foreach (TileData tile in tileDatas)
        {
            for (int rotation = 0; rotation < 4; rotation++) 
            {
                bool[][] rotatedEdges = tile.GetRotatedEdges(rotation * 90);
                int matchingEdges = CountMatchingEdges(x, y, rotatedEdges);

                if (matchingEdges > maxMatchingEdges)
                {
                    maxMatchingEdges = matchingEdges;
                    bestTile = tile;
                    bestRotation = rotation * 90;
                }
            }
        }

        return (bestTile, bestRotation);
    }
    int CountMatchingEdges(int x, int y, bool[][] edges)
    {
        int matchingEdges = 0;

        if (x > 0 && placedTiles[x - 1, y] != null) // left
        {
            bool[][] leftEdges = placedTiles[x - 1, y].GetEdges();
            if (EdgesMatch(leftEdges[3], edges[1])) matchingEdges++;
        }

        if (x < mapWidth - 1 && placedTiles[x + 1, y] != null) // right
        {
            bool[][] rightEdges = placedTiles[x + 1, y].GetEdges();
            if (EdgesMatch(rightEdges[1], edges[3])) matchingEdges++;
        }

        if (y > 0 && placedTiles[x, y - 1] != null) // down
        {
            bool[][] bottomEdges = placedTiles[x, y - 1].GetEdges();
            if (EdgesMatch(bottomEdges[2], edges[0])) matchingEdges++;
        }

        if (y < mapHeight - 1 && placedTiles[x, y + 1] != null) // up
        {
            bool[][] topEdges = placedTiles[x, y + 1].GetEdges();
            if (EdgesMatch(topEdges[0], edges[2])) matchingEdges++;
        }

        return matchingEdges;
    }

    void PlaceTile(int x, int y, TileData tile, int rotationAngle)
    {
        placedTiles[x, y] = new TileInstance(tile, rotationAngle); 

        Vector3 centerOffset = new Vector3((mapWidth - 1) * tileSize / 2, 0, (mapHeight - 1) * tileSize / 2);
        Vector3 position = transform.position + new Vector3(x * tileSize, 0, y * tileSize) - centerOffset;

        GameObject tileObj = Instantiate(tile.tilePrefab, position, Quaternion.Euler(0, rotationAngle, 0));
        tileObj.transform.parent = transform;

        bool[][] rotatedEdges = tile.GetRotatedEdges(rotationAngle);

    }

    bool CanPlaceTile(int x, int y, bool[][] edges)
    {
        bool isValid = true;
        Vector2Int currentPos = new Vector2Int(x, y);

        if (x > 0 && placedTiles[x - 1, y] != null) // left
        {
            bool[][] leftEdges = placedTiles[x - 1, y].GetEdges();
            bool match = EdgesMatch(leftEdges[3], edges[1]);
            isValid &= match;
        }

        if (x < mapWidth - 1 && placedTiles[x + 1, y] != null) // right
        {
            bool[][] rightEdges = placedTiles[x + 1, y].GetEdges();
            bool match = EdgesMatch(rightEdges[1], edges[3]);
            isValid &= match;
        }

        if (y > 0 && placedTiles[x, y - 1] != null) // down
        {
            bool[][] bottomEdges = placedTiles[x, y - 1].GetEdges();
            bool match = EdgesMatch(bottomEdges[2], edges[0]);
            isValid &= match;
        }

        if (y < mapHeight - 1 && placedTiles[x, y + 1] != null) // up
        {
            bool[][] topEdges = placedTiles[x, y + 1].GetEdges();
            bool match = EdgesMatch(topEdges[0], edges[2]);
            isValid &= match;
        }

        return isValid;
    }

    bool EdgesMatch(bool[] edge1, bool[] edge2)
    {
        bool isMatch = (edge1[0] == edge2[0] && edge1[1] == edge2[1] && edge1[2] == edge2[2]);

        return isMatch;
    }
}
