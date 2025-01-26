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
    [SerializeField] public static List<GridPoint> bubbleTiles = new();
    [SerializeField] public static List<GridPoint> allGridPoints = new();
    
    
    public static GridGen Instance;

    private void Awake()
    {
        Instance = this;
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        bubbleTiles = new List<GridPoint>();
        allGridPoints = new List<GridPoint>();
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
                var p = ref1.GetComponent<GridPoint>();
                p.x_pos = i;
                p.y_pos = j;
                p.item = GridPoint.itemType.NONE;
                p.landmark = GridPoint.landmarkType.NONE;
                
                allGridPoints.Add(p);
                if ((int)(Mathf.Abs(i)) < 2 && (int)(Mathf.Abs(j)) < 2) {
                    p.explored = true;
                    bubbleTiles.Add(p);
                } else {
                    p.explored = false;
                }
            }
        }
        
        BubblePostProcManager.OnGridInitialize(bubbleTiles);
    }

    public static void updateOnBubblePlaced(int i, int j) {
        //Ok, so we've been clicked, add new visible area around where we have clicked
        if (BubbleManager.Instance.current_bubbles == 0) {
            return;
        }
        BubbleManager.loseBubble();
        var p = Find(i, j);
        //make neighbors around ref1 visible/not fog anymore
        //Wall was clicked, turn void neighbors into
        //Look at all tiles around ref1
        
        p.changeType(GridPoint.tileType.FLOOR);
        bubbleTiles.Add(p);
        var newlyPlaced = new HashSet<GridPoint>();
        newlyPlaced.Add(p);

        int i_diff = -1;
        int j_diff = 0;
        for (int k = 0; k < 3; k++)
        {
            var q = Find((i + i_diff), (j + j_diff));
            if (q != null && q.getType() == GridPoint.tileType.ABYSS) {
                q.changeType(GridPoint.tileType.FLOOR);
                bubbleTiles.Add(q);
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
                bubbleTiles.Add(q);
            }
        }
        
        updateVoidTiles();
        BubblePostProcManager.OnGridUpdate(bubbleTiles.Select(bt => bt.GetComponent<GridPoint>()));
    }

    static void updateVoidTiles() {
        foreach (var tile in bubbleTiles) {
            int tileX = tile.x_pos;
            int tileY = tile.y_pos;
            for (int i = -2; i < 3; i++) {
                for (int j = -2; j < 3; j++) {
                    var p = Find(tileX + i, tileY + j);
                    if (p != null && p.getType() == GridPoint.tileType.FOG) {
                        p.changeType(GridPoint.tileType.ABYSS);
                    }
                }
            }
        }
    }

    public static GridPoint Find(int i, int j) => GameObject.Find($"GridPoint:{i},{j}")?.GetComponent<GridPoint>();
    public static bool IsBubble(GridPoint p) => p.type == GridPoint.tileType.FLOOR;
    public static Func<GridPoint, bool> Not(Func<GridPoint, bool> pred) => (p) => !pred(p);

    public static HashSet<GridPoint> GetNeighbors(GridPoint p, Func<GridPoint, bool> predicate)
    {
        var offsets = new []
        {
            (-1, -1), (-1, 0), (-1, 1),
            (0, -1), (0, 1),
            (1, -1), (1, 0), (1, 1)
        };

        var neighbors = offsets.Select(offset =>
        {
            var (x, y) = offset;
            return Find(x + p.x_pos, y + p.y_pos);
        }).Where(q => q != null && predicate(q));

        return new HashSet<GridPoint>(neighbors);
    }

}
