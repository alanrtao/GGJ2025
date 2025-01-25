using UnityEngine;

public class GridGen : MonoBehaviour
{
    [SerializeField] GameObject gridPoint;
    [SerializeField] float gridScale;
    [SerializeField] int gridWidth;
    [SerializeField] int gridHeight;
    GameObject[,] map;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        genGrid(gridWidth, gridHeight);
        map = new GameObject[gridWidth,gridHeight];
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void genGrid(int width, int height) {
        for (int i = -width/2; i < Mathf.CeilToInt(width/2.0f); i++) {
            for (int j = -height/2; j < Mathf.CeilToInt(height/2.0f); j++) {
                GameObject ref1 = Instantiate(gridPoint, new Vector3(i, j, 0), Quaternion.identity);
                ref1.name = "GridPoint:" + i + "," + j;
            }
        }
    }

    void updateOnBubblePlaced(int i, int j) {
        //Ok, so we've been clicked, add new visible area around where we have clicked
        GameObject ref1 = Find("GridPoint:" + i + "," + j);
        //make neighbors around ref1 visible/not fog anymore
        //Wall was clicked, turn void neighbors into
        //Look at all tiles around ref1
        bool allChecked = false;
        int counter = 3;
        while (counter >= 0 || allChecked) {
            ref2 = Find("GridPoint:" + (i - 1) + "," + j);
            if (ref2 != null && ref2.tileType == tileType.ABYSS) {
                ref2.changeType(tileType.FLOOR);
                counter--;
            }
            ref2 = Find("GridPoint:" + (i - 1) + "," + (j + 1));
            if (ref2 != null && ref2.tileType == tileType.ABYSS) {
                ref2.changeType(tileType.FLOOR);
                counter--;
            }
            ref2 = Find("GridPoint:" + (i) + "," + (j + 1));
            if (ref2 != null && ref2.tileType == tileType.ABYSS) {
                ref2.changeType(tileType.FLOOR);
                counter--;
            }
            ref2 = Find("GridPoint:" + (i + 1) + "," + (j + 1));
            if (ref2 != null && ref2.tileType == tileType.ABYSS) {
                ref2.changeType(tileType.FLOOR);
                counter--;
            }
            ref2 = Find("GridPoint:" + (i + 1) + "," + (j));
            if (ref2 != null && ref2.tileType == tileType.ABYSS) {
                ref2.changeType(tileType.FLOOR);
                counter--;
            }
            ref2 = Find("GridPoint:" + (i + 1) + "," + (j - 1));
            if (ref2 != null && ref2.tileType == tileType.ABYSS) {
                ref2.changeType(tileType.FLOOR);
                counter--;
            }
            ref2 = Find("GridPoint:" + (i) + "," + (j - 1));
            if (ref2 != null && ref2.tileType == tileType.ABYSS) {
                ref2.changeType(tileType.FLOOR);
                counter--;
            }
            ref2 = Find("GridPoint:" + (i - 1) + "," + (j - 1));
            if (ref2 != null && ref2.tileType == tileType.ABYSS) {
                ref2.changeType(tileType.FLOOR);
                counter--;
            }
        }

    }
}
