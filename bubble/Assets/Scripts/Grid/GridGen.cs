using UnityEngine;
using System.Collections.Generic;

public class GridGen : MonoBehaviour
{
    [SerializeField] public GameObject gridPoint;
    [SerializeField] public float gridScale;
    [SerializeField] public int gridWidth;
    [SerializeField] public int gridHeight;
    [SerializeField] public static List<GameObject> bubbleTiles;
    [SerializeField] public static List<GameObject> allGridPoints;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        bubbleTiles = new List<GameObject>();
        allGridPoints = new List<GameObject>();
        genGrid(gridWidth, gridHeight);
        //updateVoidTiles();
        
    }

    // Update is called once per frame
    void Update()
    {
        updateVoidTiles();
    }

    void genGrid(int width, int height) {
        for (int i = -width/2; i < Mathf.CeilToInt(width/2.0f); i++) {
            for (int j = -height/2; j < Mathf.CeilToInt(height/2.0f); j++) {
                GameObject ref1 = Instantiate(gridPoint, new Vector3(i, j, 0), Quaternion.identity);
                ref1.name = "GridPoint:" + i + "," + j;
                ref1.GetComponent<GridPoint>().x_pos = i;
                ref1.GetComponent<GridPoint>().y_pos = j;
                allGridPoints.Add(ref1);
                if ((int)(Mathf.Abs(i)) < 2 && (int)(Mathf.Abs(j)) < 2) {
                    bubbleTiles.Add(ref1);
                }
            }
        }
    }

    public static void updateOnBubblePlaced(int i, int j) {
        //Ok, so we've been clicked, add new visible area around where we have clicked
        //if (BubbleManager.current_bubbles == 0) {
        //    return;
        //}
        GameObject ref1 = GameObject.Find("GridPoint:" + i + "," + j);
        //make neighbors around ref1 visible/not fog anymore
        //Wall was clicked, turn void neighbors into
        //Look at all tiles around ref1
        
        //BubbleManager.loseBubble();

        ref1.GetComponent<GridPoint>().changeType(GridPoint.tileType.FLOOR);
        bool allChecked = false;
        int counter = 3;
        int i_diff = -1;
        int j_diff = 0;
        for (int k = 0; k < 3; k++) {
            GameObject ref2 = GameObject.Find("GridPoint:" + (i + i_diff) + "," + (j + j_diff));
            if (ref2 != null && ref2.GetComponent<GridPoint>().getType() == GridPoint.tileType.ABYSS) {
                ref2.GetComponent<GridPoint>().changeType(GridPoint.tileType.FLOOR);
                bubbleTiles.Add(ref2);
            }
            if (i_diff == -1 && j_diff == 0) {
                j_diff = 1;
            } else if (i_diff == -1 && j_diff == 1) {
                i_diff = 0;
                j_diff = 1;
            }
        }
        updateVoidTiles();
    }

    static void updateVoidTiles() {
        foreach (GameObject tile in bubbleTiles) {
            int tileX = tile.GetComponent<GridPoint>().x_pos;
            int tileY = tile.GetComponent<GridPoint>().y_pos;
            for (int i = -2; i < 3; i++) {
                for (int j = -2; j < 3; j++) {
                    GameObject Obby = GameObject.Find("GridPoint:" + (tileX + i) + "," + (tileY + j));
                    if (Obby != null && Obby.GetComponent<GridPoint>().getType() == GridPoint.tileType.FOG) {
                        Obby.GetComponent<GridPoint>().changeType(GridPoint.tileType.ABYSS);
                    }
                }
            }
        }
    }

}
