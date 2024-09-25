using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class UnitManager : MonoBehaviour
{
    [SerializeField] private GridManager gridManager;
    [SerializeField] private Tilemap _tilemap;
    [SerializeField] public Unit unitPrefab;

    private Dictionary<Vector3Int, Unit> _unitsOnTiles = new Dictionary<Vector3Int, Unit>();
    // Start is called before the first frame update
    void Start()
    {
        // Instantiate the unit at the world position
        Instantiate(unitPrefab, new Vector3(0, 0, 0), Quaternion.identity);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
