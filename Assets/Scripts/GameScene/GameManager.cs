using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    /// <summary>
    /// manager that spawn TileObjects
    /// </summary>
    [SerializeField] SpawnManager spawnManager;

    /// <summary>
    /// manager that control field
    /// </summary>
    [SerializeField] FieldManager fieldManager;

    /// <summary>
    /// spawn position for TileObjects
    /// </summary>
    [SerializeField] Vector2 startTilePos;

    /// <summary>
    /// list of colors for tiles
    /// </summary>
    [SerializeField] List<Color> tileColors = new List<Color>();

    /// <summary>
    /// score text on scene
    /// </summary>
    [SerializeField] TextMesh scoreText;
    
    int currScore;

    /// <summary>
    /// object for interaction
    /// </summary>
    TileObject currentTileObject;
    
    /// <summary>
    /// move delay for objects
    /// </summary>
    float baseMoveDelay = 1f;

    /// <summary>
    /// move delay for current tile object
    /// </summary>
    float objectMoveDelay = 1f;

    [SerializeField] UnityEvent gameEnded;
    
    ScoreData scoreData; 

    void Awake()
    {
        //load score data
        scoreData = JsonUtility.FromJson<ScoreData>(PlayerPrefs.GetString("ScoreData", ""));

        if (scoreData == null)
        {
            scoreData = new ScoreData();
        }

        Time.timeScale = 1;

        fieldManager.linesCleared += UpdateScore;
        
        spawnManager.objectSpawned += (TileObject obj) => {
            currentTileObject = obj; // setrup current tile object
            currentTileObject.gameObject.SetActive(true); 

            objectMoveDelay = baseMoveDelay;

            SetupRundomColor();

            UpdateGameSpeed();
             
            
            if (SetupStartPositions(obj)) //if can setup tile object on start position - set parent for object and start moving otherwise 
            {
                obj.transform.SetParent(fieldManager.gameObject.transform);
                Invoke("DefaultMoveDown", objectMoveDelay);
            }
            else //save scores and invoke gameEnded event
            {
                if (currScore > 0) {
                    scoreData.scores.Add(currScore);
                }
                PlayerPrefs.SetString("ScoreData", JsonUtility.ToJson(scoreData));
                Time.timeScale = 0;
                gameEnded.Invoke();
            }
        }; 
    }

    #region Controll by keys
    void Update()
    {
        if (currentTileObject != null)
        {
            if (Input.GetKeyDown(KeyCode.LeftArrow))
            {
                fieldManager.MoveTiles(currentTileObject.Tiles, Vector2.left);
            }
            if (Input.GetKeyDown(KeyCode.RightArrow))
            {
                fieldManager.MoveTiles(currentTileObject.Tiles, Vector2.right);
            }
            if (Input.GetKeyDown(KeyCode.UpArrow))
            {
                RotateTile();
            }
        }
    }

    void FixedUpdate()
    {
        if (Input.GetKey(KeyCode.DownArrow))
        {
            DefaultMoveDown();
        }
    }
    #endregion

    #region For scene buttons
    public void RestartGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void PauseUnpauseGame()
    {
        Time.timeScale = Time.timeScale == 1 ? 0 : 1; 
    }

    public void GoToMenu()
    {
        SceneManager.LoadScene("Menu");
    }
    #endregion

    void UpdateGameSpeed()
    {
        baseMoveDelay = Mathf.Lerp(baseMoveDelay, .15f, .01f);
        Debug.Log(baseMoveDelay);
    }

    void UpdateScore(int clearedLines)
    {
        currScore += fieldManager.FieldSize.x * (int)Mathf.Pow(clearedLines, 2);
        scoreText.text = currScore.ToString();
    }

    /// <summary>
    /// setup random color for currentTileObject
    /// </summary>
    void SetupRundomColor()
    {
        Color randomColor = tileColors[Random.Range(0, tileColors.Count)];
        foreach (Tile tile in currentTileObject.Tiles)
        {
            tile.GetComponent<MeshRenderer>().material.color = randomColor;
        }
    }

    bool SetupStartPositions(TileObject tileObject)
    {
        tileObject.transform.position = startTilePos;

        foreach (Tile tile in tileObject.Tiles) //check if tiles can be moved and set them movable
        {
            if(!fieldManager.CheckFieldPosition((Vector2)tile.transform.position))
            {
                return false;
            }
            tile.isMovable = true;
        }
        
        foreach (Tile tile in tileObject.Tiles) // move tiles
        {
            Vector2 prevPos = tile.transform.position;
            tile.position = tile.transform.position;
            fieldManager.ChangeTilePosition(tile, Vector2.zero, tile.position);
        }

        return true;
    }

    /// <summary>
    /// default moving for current TileObject
    /// </summary>
    void DefaultMoveDown()
    {
        if (!fieldManager.MoveTiles(currentTileObject.Tiles, Vector2.down)) // if current TileObject cant be moved
        {
            // set unmovable for all tiles of current tileObject
            foreach (Tile tile in currentTileObject.Tiles)
            {
                tile.isMovable = false;
            } 

            // check if the lines are full
            fieldManager.CheckLines(new HashSet<float>(currentTileObject.Tiles.Select(x => x.position.y)));

            // spawn next TileObject
            spawnManager.SpawnNext();
        } else 
        {
            CancelInvoke();
            Invoke("DefaultMoveDown", objectMoveDelay);// repeat after delay time
        }
    }
    
    /// <summary>
    /// rotate current tileObject to the right
    /// </summary>
    void RotateTile()
    {


        if (currentTileObject.CenterTile != null)
        {
            bool canRotate = true;

            Vector2 centerPos = currentTileObject.CenterTile.position;
            
            Vector2 shiftPos = Vector2.zero;

            int isRightInterrupt = 0; // 0 - default, 1 - inrerrupt from right, 2 - interrupt from left

            checkRotation:

            List<Vector2> occupiedPos = new List<Vector2>();

            foreach(Tile tile in currentTileObject.Tiles)
            {
                Vector2 offsetToCenter = centerPos - tile.position;
                Vector2 positionToMove = centerPos + shiftPos + RotateVector(offsetToCenter, 90);
                if (!fieldManager.CheckFieldPosition(positionToMove))
                {
                    canRotate = false;
                    occupiedPos.Add(positionToMove);
                }
            }

            if (!canRotate)
            {
                foreach(Vector2 vector in occupiedPos)
                {
                    if(vector.x > (centerPos.x + shiftPos.x))
                    {
                        if(isRightInterrupt == 2)
                        {
                            return;
                        }
                        isRightInterrupt = 1;
                    }
                    else
                    {
                        if (isRightInterrupt == 1)
                        {
                            return;
                        }
                        isRightInterrupt = 2;
                    }
                }

                shiftPos += isRightInterrupt == 1 ? Vector2.left : Vector2.right;
                canRotate = true;
                goto checkRotation;
            }

            if(canRotate)
            {
                //move tiles to rotated position

                foreach (Tile tile in currentTileObject.Tiles)
                {
                    Vector2 offsetToCenter = centerPos - tile.position;
                    Vector2 prevPos = tile.position;
                    tile.position = centerPos + shiftPos + RotateVector(offsetToCenter, 90);

                    fieldManager.ChangeTilePosition(tile, prevPos, tile.position);
                    tile.transform.position = tile.position;
                }
            }
        }
    }

    /// <summary>
    /// add rotation degree to point
    /// </summary>
    /// <param name="aPoint">point for rotate</param>
    /// <param name="aDegree">rotation degree</param>
    /// <returns></returns>
    Vector2 RotateVector(Vector2 aPoint, float aDegree)
    {
        float rad = aDegree * Mathf.Deg2Rad;
        float s = Mathf.Sin(rad);
        float c = Mathf.Cos(rad);
        return new Vector2(
            aPoint.x * c - aPoint.y * s,
            aPoint.y * c + aPoint.x * s);
    }
}
