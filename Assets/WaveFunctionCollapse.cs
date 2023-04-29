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

        print(size);
    }

}
