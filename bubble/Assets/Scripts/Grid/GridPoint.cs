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
        if ((int)(Mathf.Abs(x_pos)) < 2 && (int)(Mathf.Abs(y_pos)) < 2) {
            type = tileType.FLOOR;
        } else {
            type = tileType.FOG;
        }
    }

    public void changeType(tileType newType)
    {
        type = newType;
    }

    // Update is called once per frame
    void Update()
    {
        this.GetComponent<SpriteRenderer>().color = type switch
        {
            tileType.ABYSS => Color.white,
            tileType.FLOOR => Color.black,
            tileType.FOG => Color.gray,
            _ => this.GetComponent<SpriteRenderer>().color
        };
    }

    public tileType getType() {
        return type;
    }

    void OnMouseDown() {
        if (this.type == tileType.ABYSS) {
             GridGen.updateOnBubblePlaced(x_pos, y_pos);
        }
    }
}
