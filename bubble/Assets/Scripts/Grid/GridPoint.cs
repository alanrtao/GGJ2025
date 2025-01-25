using UnityEngine;

public class GridPoint : MonoBehaviour
{
    public enum tileType {
        EDGE, //Tile next to a wall
        FOG, //Currently invisible
        FLOOR, // walk in it
        ABYSS, //no walk in it
        WALL //impassable
    }

    bool explored; // only matters for floor tiles - if Bub has walked on it
    [SerializeField] public tileType type;
    public int x_pos;
    public int y_pos;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        type = tileType.FOG;
    }

    public void changeType(tileType newType)
    {
        type = newType;
    }

    // Update is called once per frame
    void Update()
    {
        if (type == tileType.FOG) {
            this.GetComponent<Renderer>().enabled = false;
        }
        if (type == tileType.FLOOR) {
            this.GetComponent<Renderer>().enabled = true;
        }
    }

    public tileType getType() {
        return type;
    }

    void OnMouseDown() {
        if (this.type == ABYSS) {
             GridGen.updateOnBubblePlaced(x_pos, y_pos);
        }
    }
}
