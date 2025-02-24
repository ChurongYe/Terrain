using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class TileData
{
    public GameObject tilePrefab; // 瓦片预制体
    public int weight = 1;
    // 每条边的状态 { soilLeft, water, soilRight }
    public bool[] topEdge = new bool[3];
    public bool[] rightEdge = new bool[3];
    public bool[] bottomEdge = new bool[3];
    public bool[] leftEdge = new bool[3];

    // 获取旋转后的边缘状态
    public bool[][] GetRotatedEdges(int rotationAngle)
    {
        bool[][] edges = new bool[][] { topEdge, rightEdge, bottomEdge, leftEdge };
        int rotations = (rotationAngle / 90) % 4;
        bool[][] rotatedEdges = new bool[4][];

        for (int i = 0; i < 4; i++)
        {
            int newIndex = (i - rotations + 4) % 4;
            rotatedEdges[i] = new bool[3];

            // 270° 旋转时保持顺序，其他旋转时倒序
            if (rotationAngle == 270 || rotationAngle == 0)
            {
                rotatedEdges[i] = edges[newIndex];
            }
            else
            {
                rotatedEdges[i][0] = edges[newIndex][2]; // 反转
                rotatedEdges[i][1] = edges[newIndex][1]; // 不变
                rotatedEdges[i][2] = edges[newIndex][0]; // 反转
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
    public List<TileData> tileDatas = new List<TileData>(); // 瓦片列表
    public int mapWidth = 5;
    public int mapHeight = 5;
    public float tileSize = 1f;
    private TileInstance[,] placedTiles; // 存储已放置的瓦片
    private List<Vector2Int> availablePositions; // 存储可用的位置
    private Stack<Vector2Int> placedTileStack = new Stack<Vector2Int>();
    public void TileGenerate()
    {
        if (tileDatas.Count == 0)
        {
            Debug.LogError("请在 Inspector 里添加可用的瓦片数据！");
            return;
        }

        // 清空旧瓦片
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
    }

    void GenerateMap()
    {
        while (availablePositions.Count > 0)
        {
            Vector2Int position = GetLowestEntropyPosition();
            if (!TryPlaceTile(position.x, position.y))
            {
                Debug.LogWarning($"无法为 ({position.x}, {position.y}) 放置瓦片，开始回溯...");

                // 进入回溯逻辑
                bool backtracked = false;
                while (placedTileStack.Count > 0)
                {
                    Vector2Int lastPlaced = placedTileStack.Pop();
                    RemoveTile(lastPlaced.x, lastPlaced.y); // 移除瓦片

                    if (TryPlaceTile(lastPlaced.x, lastPlaced.y)) // 重新放置
                    {
                        Debug.Log($"回溯成功，在 ({lastPlaced.x}, {lastPlaced.y}) 重新放置瓦片");
                        backtracked = true;
                        break;
                    }
                }

                if (!backtracked)
                {
                    Debug.LogError("回溯失败，无法生成完整地图！");
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
                Destroy(tileTransform.gameObject); // 删除瓦片 GameObject
            }
            placedTiles[x, y] = null; // 清除瓦片数据
            availablePositions.Add(new Vector2Int(x, y)); // 重新加入可用位置
        }
    }
    Vector2Int GetLowestEntropyPosition()
    {
        Vector2Int lowestEntropyPos = availablePositions[0];
        int lowestEntropy = int.MaxValue;

        // 遍历所有位置，计算熵，选择熵最小的位置
        foreach (Vector2Int position in availablePositions)
        {
            int entropy = CalculateEntropy(position);
            if (entropy < lowestEntropy)
            {
                lowestEntropy = entropy;
                lowestEntropyPos = position;
            }
        }

        // 从可用位置中移除已选位置
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
        List<(TileData, int)> validTiles = new List<(TileData, int)>(); // 存储 (瓦片, 旋转角度)

        foreach (TileData tile in tileDatas)
        {
            for (int rotation = 0; rotation < 4; rotation++) // 0°, 90°, 180°, 270°
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
            // 通过权重随机选择瓦片
            var (selectedTile, selectedRotation) = WeightedRandomSelection(validTiles);
            PlaceTile(x, y, selectedTile, selectedRotation);
            return true;
        }
        else
        {
            Debug.LogWarning($"无法在 ({x}, {y}) 放置匹配的瓦片，尝试放置一个边缘匹配最多的瓦片...");
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

        return tileList[tileList.Count - 1]; // 兜底返回最后一个
    }
    (TileData tileData, int rotation) GetBestFallbackTile(int x, int y)
    {
        TileData bestTile = null;
        int bestRotation = 0;
        int maxMatchingEdges = -1;

        foreach (TileData tile in tileDatas)
        {
            for (int rotation = 0; rotation < 4; rotation++) // 0°, 90°, 180°, 270°
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

        if (x > 0 && placedTiles[x - 1, y] != null) // 左侧
        {
            bool[][] leftEdges = placedTiles[x - 1, y].GetEdges();
            if (EdgesMatch(leftEdges[3], edges[1])) matchingEdges++;
        }

        if (x < mapWidth - 1 && placedTiles[x + 1, y] != null) // 右侧
        {
            bool[][] rightEdges = placedTiles[x + 1, y].GetEdges();
            if (EdgesMatch(rightEdges[1], edges[3])) matchingEdges++;
        }

        if (y > 0 && placedTiles[x, y - 1] != null) // 下方
        {
            bool[][] bottomEdges = placedTiles[x, y - 1].GetEdges();
            if (EdgesMatch(bottomEdges[2], edges[0])) matchingEdges++;
        }

        if (y < mapHeight - 1 && placedTiles[x, y + 1] != null) // 上方
        {
            bool[][] topEdges = placedTiles[x, y + 1].GetEdges();
            if (EdgesMatch(topEdges[0], edges[2])) matchingEdges++;
        }

        return matchingEdges;
    }

    void PlaceTile(int x, int y, TileData tile, int rotationAngle)
    {
        placedTiles[x, y] = new TileInstance(tile, rotationAngle); // 存储瓦片实例

        // 计算瓦片的世界位置，以 Tilemap GameObject 为中心
        Vector3 centerOffset = new Vector3((mapWidth - 1) * tileSize / 2, 0, (mapHeight - 1) * tileSize / 2);
        Vector3 position = transform.position + new Vector3(x * tileSize, 0, y * tileSize) - centerOffset;

        GameObject tileObj = Instantiate(tile.tilePrefab, position, Quaternion.Euler(0, rotationAngle, 0));
        tileObj.transform.parent = transform; // 让瓦片成为 Tilemap GameObject 的子对象

        bool[][] rotatedEdges = tile.GetRotatedEdges(rotationAngle);

    }

    bool CanPlaceTile(int x, int y, bool[][] edges)
    {
        bool isValid = true;
        Vector2Int currentPos = new Vector2Int(x, y);

        if (x > 0 && placedTiles[x - 1, y] != null) // 左侧
        {
            bool[][] leftEdges = placedTiles[x - 1, y].GetEdges();
            bool match = EdgesMatch(leftEdges[3], edges[1]);
            isValid &= match;
        }

        if (x < mapWidth - 1 && placedTiles[x + 1, y] != null) // 右侧
        {
            bool[][] rightEdges = placedTiles[x + 1, y].GetEdges();
            bool match = EdgesMatch(rightEdges[1], edges[3]);
            isValid &= match;
        }

        if (y > 0 && placedTiles[x, y - 1] != null) // 下方
        {
            bool[][] bottomEdges = placedTiles[x, y - 1].GetEdges();
            bool match = EdgesMatch(bottomEdges[2], edges[0]);
            isValid &= match;
        }

        if (y < mapHeight - 1 && placedTiles[x, y + 1] != null) // 上方
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
