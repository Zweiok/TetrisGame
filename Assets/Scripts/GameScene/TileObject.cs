using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileObject : MonoBehaviour
{
    [SerializeField] Tile[] tiles;
    [SerializeField] Tile centerTile;
    
    public Tile[] Tiles
    {
        get
        {
            return tiles;
        }
    }
    
    public Tile CenterTile
    {
        get
        {
            return centerTile;
        }
    }
}
