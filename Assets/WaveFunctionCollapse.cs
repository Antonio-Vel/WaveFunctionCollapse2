using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class WaveFunctionCollapse : MonoBehaviour
{
    public Tilemap map;
    public Vector3Int input;
    public int patternSize;
    struct Pattern
    {
        public int[,] contents;
        public List<Pattern>[] neighbors;
        private int[][] sides;
        public int frequency;
        public int index;

        public int[][] Sides { get => sides; set => sides = value; }
    }

    struct WaveCell
    {
        public List<Pattern> possibilities;
        public WaveCell[] adjacent;
        public float entropy;
        public bool solved;
    }
    /*
     *      0 == UP
     *      1 == DOWN
     *      2 == LEFT
     *      3 == RIGHT
     */
    public void WFC()
    {
        if (map == null)
        {
            Debug.LogError("No Tilemap Selected!");
            return;
        }

        //Start of InputGrid 
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

        int[,] inputGrid = new int[size.y, size.x];

        for (int y = 0; y < size.y; y++)
        {
            for (int x = 0; x < size.x; x++)
            {
                Vector3Int delta = new Vector3Int(x, y);
                TileBase target = map.GetTile(cursor + delta);
                int id = int.Parse(target.name[^1..]);
                if (!indexedtiles.Contains(id))
                {
                    indexedtiles.Add(id);
                    tiles.Add(target);
                }
                inputGrid[y, x] = id;
            }
        }
        //End of InputGrid & Start of OffsetGrid 
        int offset = patternSize - 1;

        int[,] offsetGrid = new int[size.y + 2 * offset, size.x + 2 * offset];


        for (int y = 0; y < inputGrid.GetLength(0); y++)
        {
            for (int x = 0; x < inputGrid.GetLength(1); x++)
            {
                offsetGrid[y + offset, x + offset] = inputGrid[y, x];
            }
        }

        Vector2Int upper = new Vector2Int(size.y + offset - 1, size.x + offset - 1);
        for (int y = 0; y < offsetGrid.GetLength(0); y++)
        {
            for (int x = 0; x < offsetGrid.GetLength(1); x++)
            {
                Vector2Int target = Vector2Int.zero;
                if ((x > upper.x || x < offset) && (y > upper.y || y < offset))
                {
                    target.x = x < offset ? size.x + x : x - size.x;
                    target.y = y < offset ? size.y + y : y - size.y;
                    offsetGrid[y, x] = offsetGrid[target.y, target.x];
                }
                else if (x > upper.x || x < offset)
                {
                    target.x = x < offset ? size.x + x : x - size.x;
                    target.y = y;
                    offsetGrid[y, x] = offsetGrid[target.y, target.x];
                }
                else if (y > upper.y || y < offset)
                {
                    target.x = x;
                    target.y = y < offset ? size.y + y : y - size.y;
                    offsetGrid[y, x] = offsetGrid[target.y, target.x];
                }
            }
        }

        //End of offsetGrid & Start of patternGrid

        Pattern[,] patternGrid = new Pattern[offsetGrid.GetLength(0) - patternSize + 1, offsetGrid.GetLength(1) - patternSize + 1];
        List<Pattern> allPatterns = new();
        int numPatterns = 0;

        //string debugResult = "";
        for (int y = 0; y < patternGrid.GetLength(0); y++)
        {
            for (int x = 0; x < patternGrid.GetLength(1); x++)
            {
                Pattern target = new Pattern
                {
                    contents = new int[patternSize, patternSize],
                    neighbors = new List<Pattern>[]
                    { 
                        new List<Pattern>(),
                        new List<Pattern>(),
                        new List<Pattern>(),
                        new List<Pattern>()
                    },
                    Sides = new int[][]
                    {
                        new int[patternSize],
                        new int[patternSize],
                        new int[patternSize],
                        new int[patternSize]
                    }
                };
                for (int i = 0; i < patternSize; i++)
                {
                    for (int j = 0; j < patternSize; j++)
                    {
                        target.contents[i, j] = offsetGrid[y + i, x + j];
                    }
                }

                bool found = false;
                foreach (Pattern p in allPatterns)
                {
                    if (Compare(p, target))
                    {
                        patternGrid[y, x] = p;
                        found = true;
                    }
                }

                if (!found)
                {
                    target.index = numPatterns;
                    target.frequency++;
                    numPatterns++;
                    allPatterns.Add(target);
                    patternGrid[y, x] = target;
                }
                else
                {
                    patternGrid[y, x].frequency++;
                }

                //debugResult += patternGrid[y, x].index + "\t";
            }
            //debugResult += "\n";
        }

        //print(debugResult);

        //End Of PatternGrid & Start of Pattern Configuration

        foreach (Pattern p in allPatterns)
        {
            int[,] contents = p.contents;

            for (int i = 0; i < contents.GetLength(1); i++) { p.Sides[0][i] = p.contents[patternSize - 1, i]; }
            for (int i = 0; i < contents.GetLength(1); i++) { p.Sides[1][i] = p.contents[0, i]; }
            for (int i = 0; i < contents.GetLength(0); i++) { p.Sides[2][i] = p.contents[i, 0]; }
            for (int i = 0; i < contents.GetLength(0); i++) { p.Sides[3][i] = p.contents[i, patternSize - 1]; }
        }


        foreach (Pattern p in allPatterns)
        {
            for (int i = p.index + 1; i < allPatterns.Count; i++)
            {
                Pattern other = allPatterns[i];

                if (Compare(p.Sides[0], other.Sides[1]))
                {
                    p.neighbors[0].Add(other);
                    other.neighbors[1].Add(p);
                }
                if (Compare(p.Sides[1], other.Sides[0]))
                {
                    p.neighbors[1].Add(other);
                    other.neighbors[0].Add(p);

                }
                if (Compare(p.Sides[2], other.Sides[3]))
                {
                    p.neighbors[2].Add(other);
                    other.neighbors[3].Add(p);
                }
                if (Compare(p.Sides[3], other.Sides[2]))
                {
                    p.neighbors[3].Add(other);
                    other.neighbors[2].Add(p);
                }
            }
        }

        //Little Debug
        Pattern targetPattern = allPatterns[4];
        print("Target Pattern: " + targetPattern.index);
        print("Contents:\n" + ToString(targetPattern.contents));
        print(ToString(targetPattern.Sides[0]) + "" + ToString(targetPattern.Sides[1]) + ToString(targetPattern.Sides[2]) + ToString(targetPattern.Sides[3]));
        print("Neighbors with: " + targetPattern.neighbors[0][0].index + ";\n" + ToString(targetPattern.neighbors[0][0].contents));

        //End of Pattern Configuration & Start of WaveCell

    }

    static bool Compare(Pattern p1, Pattern p2)
    {
        int[,] c1 = p1.contents;
        int[,] c2 = p2.contents;
        for (int y = 0; y < c1.GetLength(0); y++)
        {
            for (int x = 0; x < c1.GetLength(1); x++)
            {
                if (c1[y, x] != c2[y, x])
                {
                    return false;
                }
            }
        }

        return true;
    }

    static bool Compare(int[] a1, int[] a2)
    {
        bool result = true;
        for (int i = 0; i < a1.Length; i++)
        {
            if (a1[i] != a2[i])
            {
                result = false;
            }
        }
        return result;
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

    static string ToString(int[] array)
    {
        string result = "(";
        for (int i = 0; i < array.Length - 1; i++)
        {
            result += array[i] + ",";
        }

        return result + array[^1] + ")";
    }

}
