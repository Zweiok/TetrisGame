using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class FieldManager : MonoBehaviour
{

    [SerializeField] float tileSize;
    public float Step
    {
        get
        {
            return tileSize;
        }
    }

    [SerializeField] Vector2Int fieldSize;
    public Vector2Int FieldSize
    {
        get
        {
            return fieldSize;
        }
    }
    
    /// <summary>
    /// contains all tiles references and all field positions for tiles
    /// </summary>
    public Field field = new Field();
    
    public delegate void LinesCleared(int i);
    public event LinesCleared linesCleared;

    void Awake()
    {
        InitField(fieldSize);
    }

    /// <summary>
    /// setup all position on field
    /// </summary>
    /// <param name="size"></param>
    void InitField(Vector2 size)
    {
        for(int x = 0; x < size.x; x++)
        {
            for (int y = 0; y < size.y; y++)
            {
                field.tiles.Add(new Vector2(x, y) * tileSize + Vector2.one * (tileSize / 2), null);
            }
        }
    }

    /// <summary>
    /// move tile in axis
    /// </summary>
    /// <param name="tiles">tile for move</param>
    /// <param name="axis"></param>
    /// <returns></returns>
    public bool MoveTiles(Tile[] tiles, Vector2 axis)
    {
        // check if tiles can be mover
        foreach (Tile tile in tiles)
        {
            if (!CheckFieldPosition(tile.position + axis * Step))
            {
                return false;
            }
        }

        // move tiles
        foreach (Tile tile in tiles)
        {
            Vector2 prevPos = tile.position;
            tile.position += axis * Step;
            tile.transform.position = tile.position;
            ChangeTilePosition(tile, prevPos, tile.position);
        }


        return true;
    }

    /// <summary>
    /// check if position is free
    /// </summary>
    /// <param name="fieldPos"></param>
    /// <returns></returns>
    public bool CheckFieldPosition(Vector2 fieldPos)
    {
        return (field.tiles.ContainsKey(fieldPos) && (field.tiles[fieldPos] == null || field.tiles[fieldPos].isMovable))
            || (fieldPos.y > fieldSize.y && fieldPos.x < fieldSize.x && fieldPos.x > 0);
    }

    /// <summary>
    /// move tile in field data
    /// </summary>
    /// <param name="tile"></param>
    /// <param name="prevPos"></param>
    /// <param name="nextPos"></param>
    public void ChangeTilePosition(Tile tile, Vector2 prevPos, Vector2 nextPos)
    {
        if (field.tiles.ContainsKey(prevPos) && field.tiles[prevPos] == tile)
        {
            field.tiles[prevPos] = null;
        }
        field.tiles[nextPos] = tile;
    }

    /// <summary>
    /// Clear all full lines
    /// </summary>
    /// <param name="ys"> y positions for check</param>
    public void CheckLines(HashSet<float> ys)
    {
        int quantity = 0; // quantity of cleared lines

        float lowerY = int.MaxValue; //lower y of line that was cleared

        foreach (float y in ys)
        {
            List<KeyValuePair<Vector2, Tile>> tilesOnLine = field.tiles.Where(x => x.Key.y.Equals(y)).ToList(); 

            if (!tilesOnLine.Exists(x => x.Value == null)) //if line are full
            {
                // deleting all tiles on line
                foreach (KeyValuePair<Vector2, Tile> tile in tilesOnLine)
                {
                    Destroy(field.tiles[tile.Key].gameObject);
                    field.tiles[tile.Key] = null;
                }
                quantity++;
                lowerY = y < lowerY ? y : lowerY;
            }
        }

        linesCleared.Invoke(quantity);
        for (int i = 0; i < quantity; i++)
        {
            ShiftUpperTilesToDown(lowerY);
        }
    }

    /// <summary>
    /// shift all lines above y to down
    /// </summary>
    /// <param name="y"></param>
    public void ShiftUpperTilesToDown(float y)
    {
        Tile[] tiles = field.tiles.Where(x => x.Value != null && x.Key.y > y).Select(x => x.Value).OrderBy(x => x.position.y).ToArray();
        tiles.ToList().ForEach(x => x.isMovable = true);
        MoveTiles(tiles, Vector2.down);
        tiles.ToList().ForEach(x => x.isMovable = false);
    }
}
