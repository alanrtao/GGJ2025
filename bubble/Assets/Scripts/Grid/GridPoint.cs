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

    public enum itemType {
        NONE,
        NEEDLE,
        BUBBLE_WAND,
        BUBBLE_BOTTLE,
        SPIKE_BALL
    }

    public enum landmarkType {
        NONE,
        DES_1,
        DES_2,
        DES_3,
        DES_4,
        DES_5,
        DES_6,
        DES_7
    }

    public bool explored; // only matters for floor tiles - if Bub has walked on it
    public bool hasLandmark;
    public bool hasItem;
    public itemType item;
    public landmarkType landmark;
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
        if (type == tileType.ABYSS) {
            this.GetComponent<SpriteRenderer>().color = Color.white;
        }
        if (type == tileType.FLOOR && explored == false) {
            this.GetComponent<SpriteRenderer>().color = Color.green;
        } else if (type == tileType.FLOOR) {
            this.GetComponent<SpriteRenderer>().color = Color.yellow;
        }
        if (type == tileType.FOG) {
            this.GetComponent<SpriteRenderer>().color = Color.gray;
        }
    }

    public tileType getType() {
        return type;
    }
    public itemType getItem() {
        return item;
    }
    public landmarkType getLandmark() {
        return landmark;
    }

    void OnMouseDown() {
        if (this.type == tileType.ABYSS) {
             GridGen.updateOnBubblePlaced(x_pos, y_pos);
        }
    }

    public override string ToString()
    {
        return $"({x_pos}, {y_pos}): {type}";
    }
}
