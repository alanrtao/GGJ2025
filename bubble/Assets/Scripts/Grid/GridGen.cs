using System;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using Utils;

public class GridGen : MonoBehaviour
{
    [SerializeField] public GameObject gridPoint;
    [SerializeField] public float gridScale;
    [SerializeField] public int gridWidth;
    [SerializeField] public int gridHeight;
    [SerializeField] public static List<GameObject> bubbleTiles;
    [SerializeField] public static List<GameObject> allGridPoints;
    
    
    public static GridGen Instance;

    private void Awake()
    {
        Instance = this;
    }

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
                ref1.GetComponent<GridPoint>().item = GridPoint.itemType.NONE;
                ref1.GetComponent<GridPoint>().landmark = GridPoint.landmarkType.NONE;
                allGridPoints.Add(ref1);
                if ((int)(Mathf.Abs(i)) < 2 && (int)(Mathf.Abs(j)) < 2) {
                    ref1.GetComponent<GridPoint>().explored = true;
                    bubbleTiles.Add(ref1);
                } else {
                    ref1.GetComponent<GridPoint>().explored = false;
                }
            }
        }
        
        BubblePostProcManager.OnGridInitialize(bubbleTiles.Select(bt => bt.GetComponent<GridPoint>()));
    }

    public static void updateOnBubblePlaced(int i, int j) {
        //Ok, so we've been clicked, add new visible area around where we have clicked
        if (BubbleManager.current_bubbles == 0) {
            return;
        }
        BubbleManager.loseBubble();
        var p = GameObject.Find("GridPoint:" + i + "," + j)?.GetComponent<GridPoint>();
        //make neighbors around ref1 visible/not fog anymore
        //Wall was clicked, turn void neighbors into
        //Look at all tiles around ref1
        
        p.changeType(GridPoint.tileType.FLOOR);
        bubbleTiles.Add(p.gameObject);
        var newlyPlaced = new HashSet<GridPoint>();
        newlyPlaced.Add(p);

        int i_diff = -1;
        int j_diff = 0;
        for (int k = 0; k < 3; k++) {
            var q = GameObject.Find("GridPoint:" + (i + i_diff) + "," + (j + j_diff))?.GetComponent<GridPoint>();
            if (q != null && q.getType() == GridPoint.tileType.ABYSS) {
                q.changeType(GridPoint.tileType.FLOOR);
                bubbleTiles.Add(q.gameObject);
                newlyPlaced.Add(q);
            }
            if (i_diff == -1 && j_diff == 0) {
                j_diff = 1;
            } else if (i_diff == -1 && j_diff == 1) {
                i_diff = 0;
                j_diff = 1;
            }
        }
        
        // check for enclosure
        if (GraphAlgo.TryCreateBubble(newlyPlaced, out var newBubble))
        {
            foreach (var q in newBubble)
            {
                q.changeType(GridPoint.tileType.FLOOR);
                bubbleTiles.Add(q.gameObject);
            }
        }
        
        updateVoidTiles();
        BubblePostProcManager.OnGridUpdate(bubbleTiles.Select(bt => bt.GetComponent<GridPoint>()));
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

    public static bool IsBubble(GridPoint p) => p.type == GridPoint.tileType.FLOOR;

    public static HashSet<GridPoint> GetNeighbors(GridPoint p, Func<GridPoint, bool> predicate)
    {
        (int, int)[] offsets = new (int, int)[]
        {
            (-1, -1), (-1, 0), (-1, 1),
            (0, -1), (0, 1),
            (1, -1), (1, 0), (1, 1)
        };

        var neighbors = offsets.Select(offset =>
        {
            var (x, y) = offset;
            var q = GameObject.Find("GridPoint:" + (x + p.x_pos) + "," + (y + p.y_pos));
            return q?.GetComponent<GridPoint>();
        }).Where(q => q != null && predicate(q));

        return new HashSet<GridPoint>(neighbors);
    }

}
