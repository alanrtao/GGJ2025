using System;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.Rendering.UI;
using Utils;
using Random = UnityEngine.Random;

public class GridGen : MonoBehaviour
{
    [SerializeField] public GameObject gridPoint;
    [SerializeField] public float gridScale;
    [SerializeField] public int gridWidth;
    [SerializeField] public int gridHeight;
    [SerializeField] public int itemRatioMult;
    [SerializeField] public static List<GridPoint> bubbleTiles = new();
    [SerializeField] public static List<GridPoint> allGridPoints = new();

    [SerializeField] public Sprite[] landmarkSprites = new Sprite[7];
    [SerializeField] public Sprite[] itemSprites = new Sprite[4];
   
    private Dictionary<(int x, int y), GridPoint> m_map;

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
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void genGrid(int width, int height)
    {
        m_map = new();
        for (int i = -width / 2; i < Mathf.CeilToInt(width / 2.0f); i++)
        {
            for (int j = -height / 2; j < Mathf.CeilToInt(height / 2.0f); j++)
            {
                GameObject ref1 = Instantiate(gridPoint, new Vector3(i, j, 0), Quaternion.identity);
                ref1.name = "GridPoint:" + i + "," + j;
                var p = ref1.GetComponent<GridPoint>();
                p.x_pos = i;
                p.y_pos = j;
                p.item = GridPoint.itemType.NONE;
                p.landmark = GridPoint.landmarkType.NONE;
                p.type = GridPoint.tileType.FOG;
                p.hasItem = false;
                p.hasLandmark = false;
                p.InitTypeFromCoords();

                allGridPoints.Add(p);
                if ((int)(Mathf.Abs(i)) < 2 && (int)(Mathf.Abs(j)) < 2)
                {
                    p.explored = true;
                    bubbleTiles.Add(p);
                }
                else
                {
                    p.explored = false;
                }

                m_map.Add((i, j), p);
            }
        }

        BubblePostProcManager.OnGridInitialize();

        updateVoidTiles();
        landmarkGenerator();
        itemGenerator();
    }

    void landmarkGenerator() {
        int successCount = 0;
        GridPoint[] gridMap = allGridPoints.ToArray();
        while (successCount < 7) {
            int index = Random.Range(0,allGridPoints.Count);
            GridPoint gp = gridMap[index];
            bool emptyTile = ((gp.hasLandmark == false) && (gp.hasItem == false));
            bool inStartRange = ((int)(Mathf.Abs(gp.x_pos)) < 4 && (int)(Mathf.Abs(gp.y_pos)) < 4);
            bool onEdgeTiles = ((gp.x_pos == -gridWidth/2) || (gp.x_pos == Mathf.CeilToInt(gridWidth/2.0f) - 1) || (gp.y_pos == -gridHeight/2) || (gp.y_pos == Mathf.CeilToInt(gridHeight/2.0f) - 1));
            if (gp.getType() == GridPoint.tileType.FOG && emptyTile && !inStartRange && !onEdgeTiles) {
                gp.hasLandmark = true;
                successCount++;
                gp.landmark = (GridPoint.landmarkType)successCount;
            }
        }
    }

    void itemGenerator() {
        int itemCount = 0;
        GridPoint[] gridMap = allGridPoints.ToArray();
        //2:4:4:1
        //needle:wand:mine:bottle
        while (itemCount < 11*itemRatioMult) {
            int index = Random.Range(0,allGridPoints.Count);
            GridPoint gp = gridMap[index];
            bool emptyTile = ((gp.hasLandmark == false) && (gp.hasItem == false));
            bool inStartRange = ((int)(Mathf.Abs(gp.x_pos)) < 4 && (int)(Mathf.Abs(gp.y_pos)) < 4);
            bool onEdgeTiles = ((gp.x_pos == -gridWidth/2) || (gp.x_pos == Mathf.CeilToInt(gridWidth/2.0f) - 1) || (gp.y_pos == -gridHeight/2) || (gp.y_pos == Mathf.CeilToInt(gridHeight/2.0f) - 1));
            if (gp.getType() == GridPoint.tileType.FOG && emptyTile && !inStartRange && !onEdgeTiles) {
                gp.hasItem = true;
                if (itemCount < 2*itemRatioMult) {
                    gp.item = GridPoint.itemType.NEEDLE;
                } else if (itemCount < 6*itemRatioMult) {
                    gp.item = GridPoint.itemType.BUBBLE_WAND;
                } else if (itemCount < 10*itemRatioMult) {
                    gp.item = GridPoint.itemType.SPIKE_BALL;
                } else {
                    gp.item = GridPoint.itemType.BUBBLE_BOTTLE;
                }
                itemCount++;
            }
        }
    }

    public static void updateOnBubblePlaced(int i, int j)
    {
        //Ok, so we've been clicked, add new visible area around where we have clicked
        if (BubbleManager.CurrentBubbles == 0)
        {
            return;
        }

        BubbleManager.loseBubble();
        var p = Find(i, j);
        //make neighbors around ref1 visible/not fog anymore
        //Wall was clicked, turn void neighbors into
        //Look at all tiles around ref1

        p.changeType(GridPoint.tileType.FLOOR);
        bubbleTiles.Add(p);
        var newlyPlaced = new HashSet<GridPoint> { p };

        AudioManager.PlaySound(AudioManager.Asset.BubblePlacement);

        int i_diff = -1;
        int j_diff = 0;
        for (int k = 0; k < 3; k++)
        {
            var q = Find((i + i_diff), (j + j_diff));
            if (q != null && q.getType() == GridPoint.tileType.ABYSS)
            {
                q.changeType(GridPoint.tileType.FLOOR);
                bubbleTiles.Add(q);
                newlyPlaced.Add(q);
            }

            if (i_diff == -1 && j_diff == 0)
            {
                j_diff = 1;
            }
            else if (i_diff == -1 && j_diff == 1)
            {
                i_diff = 0;
                j_diff = 1;
            }
        }

        // check for enclosure
        if (GraphAlgo.TryCreateBubble(newlyPlaced, out var newBubble))
        {
            AudioManager.PlaySound(AudioManager.Asset.BubbleLoopTemplate);
            foreach (var q in newBubble)
            {
                q.changeType(GridPoint.tileType.FLOOR);
                bubbleTiles.Add(q);
            }
        }

        updateVoidTiles();
        BubblePostProcManager.OnGridUpdate();
    }

    static void updateVoidTiles()
    {
        bool actuallyChangedType = false;
        foreach (var tile in bubbleTiles)
        {
            int tileX = tile.x_pos;
            int tileY = tile.y_pos;
            for (int i = -2; i < 3; i++)
            {
                for (int j = -2; j < 3; j++)
                {
                    var p = Find(tileX + i, tileY + j);
                    if (p != null && p.getType() == GridPoint.tileType.FOG)
                    {
                        if (p.type != GridPoint.tileType.ABYSS)
                        {
                            actuallyChangedType = true;
                            p.changeType(GridPoint.tileType.ABYSS);
                        }
                    }
                }
            }
        }

        if (actuallyChangedType)
        {
            BubblePostProcManager.OnGridUpdate();
        }
    }

    public static GridPoint Find(int i, int j)
    {
        return Instance.m_map.GetValueOrDefault((i, j));
    }

    // GameObject.Find($"GridPoint:{i},{j}")?.GetComponent<GridPoint>());

    public static bool IsBubble(GridPoint p) => p.type == GridPoint.tileType.FLOOR;

    public static bool IsEdgeBubble(GridPoint p) =>
        p.type == GridPoint.tileType.FLOOR && GetNeighbors(p, Not(IsBubble)).Count > 0;

    public static Func<GridPoint, bool> IsBubbleWithLandmark(GridPoint.landmarkType type) => (p) =>
        p.type == GridPoint.tileType.FLOOR && p.hasLandmark && p.landmark == type;

    public static bool IsBubbleWithAnyItem(GridPoint p) =>
        p.type == GridPoint.tileType.FLOOR && p.hasLandmark;

    public static bool IsUnexploredBubble(GridPoint p) =>
        p.type == GridPoint.tileType.FLOOR && !p.explored;

    public static Func<GridPoint, bool> Not(Func<GridPoint, bool> pred) => (p) => !pred(p);

    private static (int, int)[] offsets_diag = new (int, int)[]
    {
        (-1, -1), (-1, 0), (-1, 1),
        (0, -1), (0, 1),
        (1, -1), (1, 0), (1, 1)
    };

    private static (int, int)[] offsets_nodiag = new (int, int)[]
    {
        (-1, 0),
        (0, -1), (0, 1),
        (1, 0)
    };
    
    public static HashSet<GridPoint> GetNeighbors(GridPoint p, Func<GridPoint, bool> predicate, bool allowDiagonals = true)
    {
        var offsets = allowDiagonals ? offsets_diag : offsets_nodiag;

        var neighbors = offsets.Select(offset =>
        {
            var (x, y) = offset;
            return Find(x + p.x_pos, y + p.y_pos);
        }).Where(q => q != null && predicate(q));

        return new HashSet<GridPoint>(neighbors);
    }
    
    public static bool IsMapBorder(GridPoint p)
    {
        foreach (var (x, y) in offsets_nodiag)
        {
            if (Find(x + p.x_pos, y + p.y_pos) == null)
            {
                // Debug.Log($"{p} is map border");
                return true;
            }
        }
        return false;
    }

}
