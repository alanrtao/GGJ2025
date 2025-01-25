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
                ref1.GetComponent<GridPoint>().x_pos = i;
                ref1.GetComponent<GridPoint>().y_pos = j;
            }
        }
    }

    public static void updateOnBubblePlaced(int i, int j) {
        //Ok, so we've been clicked, add new visible area around where we have clicked
        GameObject ref1 = GameObject.Find("GridPoint:" + i + "," + j);
        //make neighbors around ref1 visible/not fog anymore
        //Wall was clicked, turn void neighbors into
        //Look at all tiles around ref1
        ref1.GetComponent<GridPoint>().changeType(GridPoint.tileType.FLOOR);
        bool allChecked = false;
        int counter = 3;
        int i_diff = -1;
        int j_diff = 0;
        for (int k = 0; k < 3; k++) {
            GameObject ref2 = GameObject.Find("GridPoint:" + (i + i_diff) + "," + (j + j_diff));
            if (ref2 != null && ref2.GetComponent<GridPoint>().getType() == GridPoint.tileType.ABYSS) {
                ref2.GetComponent<GridPoint>().changeType(GridPoint.tileType.FLOOR);
            }
            if (i_diff == -1 && j_diff == 0) {
                j_diff = 1;
            } else if (i_diff == -1 && j_diff == 1) {
                i_diff = 0;
                j_diff = 1;
            }
        }
    }
}
