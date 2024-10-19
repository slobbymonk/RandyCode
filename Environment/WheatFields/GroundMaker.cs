using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GroundMaker : MonoBehaviour
{
    [SerializeField] private GameObject groundTile;

    [SerializeField] private Vector2 groundSize;

    private enum RemakeMapEnum
    {
        Idle,
        Remaking
    }
    private RemakeMapEnum remakeMap = RemakeMapEnum.Idle;

    [Tooltip("Toggle to remake map")]
    [SerializeField] private bool _remakeMap;

    private List<GameObject> tiles;

    private void Awake()
    {
        tiles = new List<GameObject>();

        StartCoroutine(MakeMap());
    }
    private void Update()
    {
        if (_remakeMap && remakeMap != RemakeMapEnum.Remaking)
        {
            ClearMap();
            StartCoroutine(MakeMap());
        } 
    }
    private void ClearMap()
    {
        if(tiles.Count > 0)
        {
            foreach(GameObject tile in tiles)
            {
                Destroy(tile);
            }
            tiles.Clear();
        }
    }
    private IEnumerator MakeMap()
    {
        remakeMap = RemakeMapEnum.Remaking;
        for (int i = 0; i < groundSize.x; i++)
        {
            for (int o = 0; o < groundSize.y; o++)
            {
                var xOffset = groundTile.transform.localScale.x * 2;
                var yOffset = groundTile.transform.localScale.y * 2;
                var position = new Vector3(transform.position.x + (xOffset * i)
                    , transform.position.y,
                    transform.position.z + (yOffset * o));
                var tile = Instantiate(groundTile, position, Quaternion.identity, transform);
                tiles.Add(tile);
            }
        }
        remakeMap = RemakeMapEnum.Idle;
        _remakeMap = false;

        yield return null;
    }
}
