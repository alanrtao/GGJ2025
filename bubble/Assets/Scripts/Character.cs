using UnityEngine;
using UnityEngine.SceneManagement;
public class Character : TurnObject
{
    [SerializeField] GameObject currentTargetPoint;
    GameObject nextDestination;
    int bub_x_pos;
    int bub_y_pos;
    int bubMoveSpeed;
    int bubStrength;
    GridPoint.landmarkType bubDesire;
    float needleTimer;
    bool hasNeedle;
    bool[] fulfilledDesires;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        bub_x_pos = 0;
        bub_y_pos = 0;
        bubMoveSpeed = 2;
        bubStrength = 20;
        needleTimer = 180;
        fulfilledDesires = new bool[7] {false, false, false, false, false, false, false};
        transform.position = new Vector3(bub_x_pos, bub_y_pos, 0);
        // ScheduleAfterNow(1, new TurnActionLambda(RepeaterA), MultipleActionResolution.AddAsFirstInTurn);
        // ScheduleAfterNow(1, new TurnActionLambda(RepeaterB), MultipleActionResolution.AddAsFirstInTurn);
    }

    void Update()
    {
        if (hasNeedle) {
            updateNeedleTimer();
        }
        currentTargetPoint = selectMoveTarget();
        if (Input.GetKeyDown("w")) {
            bub_y_pos++;
        }
        if (Input.GetKeyDown("a")) {
            bub_x_pos--;
        }
        if (Input.GetKeyDown("s")) {
            bub_y_pos--;
        }
        if (Input.GetKeyDown("d")) {
            bub_x_pos++;
        }
        nextTileToMove();
        transform.position = new Vector3(bub_x_pos, bub_y_pos, 0);
    }

    // private int m_repeaterA = 0;
    // TurnResult RepeaterA(TurnContext ctx, TurnObject to)
    // {
    //     const int interval = 5;
    //     Debug.Log($"Repeater A invocation {++m_repeaterA}, next invocation in {interval} turns...");
    //     ScheduleAfterNow(interval, new TurnActionLambda(RepeaterA), MultipleActionResolution.AddAsFirstInTurn);
    //     return TurnResult.Continue;
    // }
    
    // private int m_repeaterB = 0;
    // TurnResult RepeaterB(TurnContext ctx, TurnObject to)
    // {
    //     const int interval = 2;
    //     Debug.Log($"Repeater B invocation {++m_repeaterB}, next invocation in {interval} turns...");
    //     ScheduleAfterNow(interval, new TurnActionLambda(RepeaterB), MultipleActionResolution.AddAsFirstInTurn);
    //     return TurnResult.Continue;
    // }

    private int m_bubTakeAction = 0;
    TurnResult bubTakeAction(TurnContext ctx, TurnObject to) {
        int interval = bubMoveSpeed;
        if (nextDestination == null || nextDestination.GetComponent<GridPoint>().getType() == GridPoint.tileType.ABYSS) {
            BubbleManager.loseHealth(bubStrength);
        } else {
            bub_x_pos = nextDestination.GetComponent<GridPoint>().x_pos;
            bub_y_pos = nextDestination.GetComponent<GridPoint>().y_pos;
            nextDestination.GetComponent<GridPoint>().explored = true;
            if (nextDestination.GetComponent<GridPoint>().hasLandmark && nextDestination.GetComponent<GridPoint>().getLandmark() != bubDesire) {
                nextDestination.GetComponent<GridPoint>().explored = false;
            }
            switch (nextDestination.GetComponent<GridPoint>().getItem()) {
                case GridPoint.itemType.NEEDLE:
                    setBubStrength(99999);
                    hasNeedle = true;
                    break;
                case GridPoint.itemType.BUBBLE_WAND:
                    BubbleManager.addBubbles(BubbleManager.placableBubbleAmount);
                    break;
                case GridPoint.itemType.BUBBLE_BOTTLE:
                    BubbleManager.addHealth(BubbleManager.healthRecoverAmount);
                    break;
                default:
                    break;
            }
            nextDestination.GetComponent<GridPoint>().item = GridPoint.itemType.NONE;
            //ADD LANDMARK CODE
        }

        Debug.Log($"bubTakeAction invocation {++m_bubTakeAction}, next invocation in {interval} turns...");
        ScheduleAfterEndOfQueue(interval, new TurnActionLambda(bubTakeAction), MultipleActionResolution.AddAsFirstInTurn);
        return TurnResult.Continue;
    }

    GameObject selectMoveTarget() {
        // PRIORITIES
        GameObject selectedTile = null;
        GameObject priority1Tile = null;
        GameObject priority2Tile = null;
        GameObject priority3Tile = null;
        foreach (GameObject floorTile in GridGen.bubbleTiles) {
            GridPoint tilePoint = floorTile.GetComponent<GridPoint>();
            // 1. Unvisited Landmarks Inside Bubble
            if (priority1Tile == null && tilePoint.explored == false && tilePoint.hasLandmark == true && tilePoint.landmark == bubDesire) {
                priority1Tile = floorTile;
            // 2. Uncollected Items Inside Bubble
            } else if (priority2Tile == null && tilePoint.explored == false && tilePoint.hasItem == true) {
                priority2Tile = floorTile;
            // 3. Unexplored Tiles
            } else if (priority3Tile == null && tilePoint.explored == false && tilePoint.hasLandmark == false) {
                priority3Tile = floorTile;
            }
        }
        if (priority1Tile != null) {
            selectedTile = priority1Tile;
            return selectedTile;
        } else if (priority2Tile != null) {
            selectedTile = priority2Tile;
            return selectedTile;
        } else if (priority3Tile != null) {
            selectedTile = priority3Tile;
            return selectedTile;
        } 
        // 4. Nearest Bubble Edge
        int min = 99999999;
        foreach (GameObject tile in GridGen.allGridPoints) {
            GridPoint gridTile = tile.GetComponent<GridPoint>();
            if (gridTile.getType() != GridPoint.tileType.ABYSS) {
                continue;
            }
            int tileX = gridTile.x_pos;
            int tileY = gridTile.y_pos;
            if ((tileX-bub_x_pos)*(tileX-bub_x_pos) + (tileY-bub_y_pos)*(tileY-bub_y_pos) < min) {
                selectedTile = tile;
            }
        }
        return selectedTile;
        

    }

    void nextTileToMove() {
        nextDestination = GameObject.Find("GridPoint:" + bub_x_pos + "," + bub_y_pos);
        if (nextDestination == null || nextDestination.GetComponent<GridPoint>().getType() == GridPoint.tileType.ABYSS) {
            BubbleManager.loseHealth(bubStrength);
        } else {
            //bub_x_pos = nextDestination.GetComponent<GridPoint>().x_pos;
            //bub_y_pos = nextDestination.GetComponent<GridPoint>().y_pos;
            nextDestination.GetComponent<GridPoint>().explored = true;
        }
    }

    void selectNewDesire() {
        bool allFulfilled = true;
        for (int i = 0; i < fulfilledDesires.Length; i++) {
            allFulfilled = allFulfilled && fulfilledDesires[i];
        }
        if (allFulfilled) {
            SceneManager.LoadScene("YouWin");
        }

        int pickDesire = Random.Range(0,7);
        while (fulfilledDesires[pickDesire]) {
            pickDesire = Random.Range(0,7);
        }

        switch (pickDesire) {
            case 0:
                bubDesire = GridPoint.landmarkType.DES_1;
                break;
            case 1:
                bubDesire = GridPoint.landmarkType.DES_2;
                break;
            case 2:
                bubDesire = GridPoint.landmarkType.DES_3;
                break;
            case 3:
                bubDesire = GridPoint.landmarkType.DES_4;
                break;
            case 4:
                bubDesire = GridPoint.landmarkType.DES_5;
                break;
            case 5:
                bubDesire = GridPoint.landmarkType.DES_6;
                break;
            case 6:
                bubDesire = GridPoint.landmarkType.DES_7;
                break;
            default:
                bubDesire = GridPoint.landmarkType.NONE;
                break;
        }
    }

    void setBubSpeed(int newSpeed) {
        bubMoveSpeed = newSpeed;
    }

    void setBubStrength(int newStrength) {
        bubStrength = newStrength;
    }

    void updateNeedleTimer() {
        needleTimer -= Time.deltaTime;
        if (needleTimer <= 0) {
            needleTimer = 0;
            hasNeedle = false;
        }
    }
}
