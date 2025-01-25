using UnityEngine;

public class GridPoint : MonoBehaviour
{
    enum tileType {
        EDGE, //Tile next to a wall
        FOG, //Currently invisible
        FLOOR, // walk in it
        SPACE, //no walk in it
        WALL //impassable
    }

    bool explored; // only matters for floor tiles - if Bub has walked on it
    [SerializeField] tileType type;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        type = tileType.FLOOR;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
