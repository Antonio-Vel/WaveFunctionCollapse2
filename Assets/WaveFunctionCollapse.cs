using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class WaveFunctionCollapse : MonoBehaviour
{
    public Tilemap map;
    public Vector3Int input;
    struct Pattern
    {
        int[,] contents;
        List<Pattern>[] Neighbors;
        int[][] sides;
        int frequency;

    }
    /*
     *      0 == UP
     *      1 == DOWN
     *      2 == LEFT
     *      3 == RIGHT
     */
    public void CreateInputGrid()
    {
        if (map == null)
        {
            Debug.LogError("No Tilemap Selected!");
            return;
        }

        //START OF INPUTGRIDCREATOR
        Vector2Int size = Vector2Int.zero;
        Vector3Int cursor = Vector3Int.zero;
        cursor.x = input.x;
        cursor.y = input.y;
        while (map.GetTile(cursor) != null)
        {
            size.x = 0;
            while (map.GetTile(cursor) != null)
            {
                cursor.x++;
                size.x++;
            }
            cursor.y++;
            size.y++;
            cursor.x = input.x;
        }
        cursor.x = input.x;
        cursor.y = input.y;

        List<int> indexedtiles = new();
        List<TileBase> tiles = new();

        int[,] inputGrid = new int[size.y,size.x];
        
        for (int y = 0; y < size.y; y++)
        {
            for (int x = 0; x < size.x; x++)
            {
                Vector3Int offset = new Vector3Int(x, y);
                TileBase target = map.GetTile(cursor + offset);
                int id = int.Parse(target.name[^1..]);
                if (!indexedtiles.Contains(id))
                {
                    indexedtiles.Add(id);
                    tiles.Add(target);
                }
                inputGrid[y, x] = id;
            }
        }
    }

    static string ToString(int[,] array)
    {
        string result = "";
        for (int y = array.GetLength(0) - 1; y >= 0; y--)
        {
            for (int x = 0; x < array.GetLength(1); x++)
            {
                result += array[y, x] + "\t";
            }
            result += "\n";
        }
        return result;
    }
}
