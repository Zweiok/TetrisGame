using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnManager : MonoBehaviour
{
    [SerializeField] TileObject[] objects;
    [SerializeField] Transform posForNext;

    public delegate void OnObjectSpawn(TileObject obj);
    public event OnObjectSpawn objectSpawned;

    TileObject nextObject;

    private void Start()
    {
        nextObject = Instantiate(objects[Random.Range(0, objects.Length)]);
        SpawnNext();
    }

    /// <summary>
    /// Spawn next and current TileObject
    /// </summary>
    public void SpawnNext()
    {

        objectSpawned.Invoke(Instantiate(nextObject));

        Destroy(nextObject.gameObject);
        nextObject = Instantiate(objects[Random.Range(0, objects.Length)]);
        nextObject.transform.position = posForNext.position;
        nextObject.gameObject.SetActive(true);
    }
}
