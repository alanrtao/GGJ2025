using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Utils;
using Random = UnityEngine.Random;
using UnityEngine.SceneManagement;
using UnityEngine.UI;


public class Character : TurnObject
{
    [SerializeField] private int bub_x_pos;
    [SerializeField] private int bub_y_pos;
    [SerializeField] private int bubMoveSpeed;
    [SerializeField] private int bubStrength;
    public GridPoint.landmarkType bubDesire;
    [SerializeField] private float needleTimer;
    bool hasNeedle;
    bool[] fulfilledDesires;

    Animator animator;
    String currentTrigger;
    


    [SerializeField] private RawImage indicatorBg;
    [SerializeField] private RawImage indicatorFg;
    [SerializeField] public Texture[] indicatorDesires = new Texture[7];
    [SerializeField] private TMPro.TextMeshProUGUI indicatorTxt;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        currentTrigger = " ";
        bub_x_pos = 0;
        bub_y_pos = 0;
        fulfilledDesires = new bool[7] {false, false, false, false, false, false, false};
        transform.position = new Vector3(bub_x_pos, bub_y_pos, 0);
        ScheduleDecideNewState(1, State.Idle);
        animator = gameObject.GetComponent<Animator>();
    }

    void Update()
    {
        if (hasNeedle) {
            updateNeedleTimer();
        }
        // transform.position = new Vector3(bub_x_pos, bub_y_pos, 0);
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

    TurnResult ScheduleDecideNewState(int interval, State prev)
    {
        ScheduleAfterEndOfQueue(interval, new TurnActionLambda((ctx, to)
            => BubStateDecideForNextTurn(ctx, to, prev)), MultipleActionResolution.AddAsFirstInTurn);
        return TurnResult.Continue;
    }

    TurnResult ScheduleNextState(int interval, Func<TurnContext, TurnObject, TurnResult> state)
    {
        ScheduleAfterEndOfQueue(interval, new TurnActionLambda(state), MultipleActionResolution.AddAsFirstInTurn);
        return TurnResult.Continue;
    }

    #region STATES
    public enum State
    {
        Idle,
        Item,
        Landmark,
        Escape,
        Explore
    }

    // state transition table
    TurnResult BubStateDecideForNextTurn(TurnContext ctx, TurnObject to, State prev)
    {
        // When Bub is transitioning between different states, he first takes a pause
        // and tells the player what he's going to do. This is the "warn"
        TurnResult WarnOrDoTransition(State state, Func<TurnResult> doTransition)
        {
            if (prev != state)
            {
                IndicateState(state);
                return ScheduleDecideNewState(1, state);
            }
            return doTransition();
        }
        
        /* landmark */
        if (current.hasLandmark && current.landmark == bubDesire)
        {
            return WarnOrDoTransition(State.Landmark, () => BubStateLandmark(null));
        }
        if (GraphAlgo.TryFindPath(current, GridGen.IsBubbleWithLandmark(bubDesire), out var pathsToLandmark))
        {
            return  WarnOrDoTransition(State.Landmark, () => BubStateLandmark(pathsToLandmark));
        }

        /* item */
        if (current.hasItem)
        {
            WarnOrDoTransition(State.Item, () => BubStateItem(null));
        }
        if (GraphAlgo.TryFindPath(current, GridGen.IsBubbleWithAnyItem, out var pathsToItem))
        {
            WarnOrDoTransition(State.Item, () => BubStateItem(pathsToItem));
        }
        
        /* explore */
        if (!current.explored)
        {
            return WarnOrDoTransition(State.Explore, () => BubStateExplore(null));
        }
        if (GraphAlgo.TryFindPath(current, GridGen.IsUnexploredBubble, out var pathsToUnexplored))
        {
            return WarnOrDoTransition(State.Explore, () => BubStateExplore(pathsToUnexplored));
        }
        
        /* escape */
        if (OnEdge(out var randomNearbyEdge))
        {
            return WarnOrDoTransition(State.Escape, () => BubStateEscape(randomNearbyEdge, null));
        }
        if (GraphAlgo.TryFindPath(current, GridGen.IsEdgeBubble, out var pathsToEdge))
        {
            return WarnOrDoTransition(State.Escape, () => BubStateEscape(null, pathsToEdge));
        }

        /* fallback */
        IndicateState(State.Idle);
        return ScheduleDecideNewState(1, State.Idle);
    }

    TurnResult BubStateItem(Dictionary<GridPoint, List<GridPoint>> pathsToItem)
    {
        IndicateState(State.Item);
        
        // already on item
        if (pathsToItem == null)
        {
            TakeItem(current);
        }
        else
        {
            var path = GraphAlgo.PickShortestPath(pathsToItem);
            TakeStep(path);
        }
        return ScheduleDecideNewState(1, State.Item);
    }

    TurnResult BubStateLandmark(Dictionary<GridPoint, List<GridPoint>> pathsToLandmark)
    {
        IndicateState(State.Landmark);

        // already on the desired landmark
        if (pathsToLandmark == null)
        {
            TakeLandmark(current);

            return ScheduleDecideNewState(2, State.Item); // shiny item, bub takes an extra turn to take a look
        }
        
        var path = GraphAlgo.PickShortestPath(pathsToLandmark);
        TakeStep(path);
        return ScheduleDecideNewState(1, State.Landmark);
    }

    TurnResult BubStateExplore(Dictionary<GridPoint, List<GridPoint>> pathsToUnexplored)
    {
        IndicateState(State.Explore);
        
        // already on an unexplored tile
        if (pathsToUnexplored == null)
        {
            current.explored = true;
        }
        else
        {
            var path = GraphAlgo.PickShortestPath(pathsToUnexplored);
            TakeStep(path);
        }
        
        return ScheduleDecideNewState(1, State.Explore);
    }

    TurnResult BubStateEscape(GridPoint attackDirection, Dictionary<GridPoint, List<GridPoint>> pathsToEscape)
    {
        IndicateState(State.Escape);
        
        // already on edge, bump into bubble and deduct health
        if (attackDirection != null)
        {
            StartCoroutine(BumpWall(attackDirection));
            BubbleManager.loseHealth(bubStrength);
        }
        else
        {
            var path = GraphAlgo.PickShortestPath(pathsToEscape);
            TakeStep(path);
        }

        // no edge tile reachable (probably won't be the case ever???), re-decide next turn
        return ScheduleDecideNewState(1, State.Escape);
    }
    
    #endregion

    private GridPoint current => GridGen.Find(bub_x_pos, bub_y_pos);

    bool OnEdge(out GridPoint adjBubble)
    { 
        var neighbors = GridGen.GetNeighbors(current, GridGen.Not(GridGen.IsBubble), allowDiagonals: false);
        if (neighbors.Count == 0)
        {
            adjBubble = null;
            return false;
        }

        var neighbors_ = neighbors.ToArray();
        adjBubble = neighbors_[Random.Range(0, neighbors_.Length)];
        return true;
    }
    
    # region appearance
    IEnumerator BumpWall(GridPoint wall)
    {
        AudioManager.PlaySound(AudioManager.Asset.HurtBubble);
        var start = transform.position;
        for (float i = 0; i < 0.2f; i+=Time.deltaTime)
        {
            transform.position = Vector3.Lerp(start, wall.transform.position, Mathf.Sin((i / 0.2f) * Mathf.PI) * 0.3f);
            yield return null;
        }
        transform.position = start;
    }

    [SerializeField] private Transform indicatorRoot;
    private IEnumerator FlashIndicator()
    {
        for (float i = 0; i < 0.5; i += Time.deltaTime)
        {
            indicatorRoot.localPosition = new Vector3(0, Mathf.Sin((i / 0.5f) * Mathf.PI) * 0.3f, 0);
            yield return null;
        }

        indicatorRoot.localPosition = Vector3.zero;
    }

    private string m_prevIndicated = "";
    void IndicateState(State state)
    {
        var curr = state == State.Landmark ? $"{state}-{bubDesire}" : $"{state}";
        if (m_prevIndicated != curr)
        {
            StartCoroutine(FlashIndicator());
            m_prevIndicated = curr;
        }
        indicatorTxt.text = state.ToString();
        switch (state)
        {
            case State.Idle:
                indicatorTxt.text = "...";
                indicatorFg.enabled = false;
                break;
            case State.Item:
                indicatorTxt.text = "";
                indicatorFg.enabled = true;
                break;
            case State.Landmark:
                indicatorTxt.text = "";
                indicatorFg.enabled = true;
                indicatorFg.texture = indicatorDesires[(int)bubDesire];
                break;
            case State.Escape:
                indicatorTxt.text = ":(";
                indicatorFg.enabled = false;
                break;
            case State.Explore:
                indicatorTxt.text = ":3";
                indicatorFg.enabled = false;
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(state), state, null);
        }
        // Debug.Log($"Bub state = {state}");
    }
    #endregion
    //
    // GameObject selectMoveTarget() {
    //     // PRIORITIES
    //     GameObject selectedTile = null;
    //     GameObject priority1Tile = null;
    //     GameObject priority2Tile = null;
    //     GameObject priority3Tile = null;
    //     foreach (var floorTile in GridGen.bubbleTiles) {
    //         GridPoint tilePoint = floorTile.GetComponent<GridPoint>();
    //         // 1. Unvisited Landmarks Inside Bubble
    //         if (priority1Tile == null && tilePoint.explored == false && tilePoint.hasLandmark == true && tilePoint.landmark == bubDesire) {
    //             priority1Tile = floorTile.gameObject;
    //         // 2. Uncollected Items Inside Bubble
    //         } else if (priority2Tile == null && tilePoint.explored == false && tilePoint.hasItem == true) {
    //             priority2Tile = floorTile.gameObject;
    //         // 3. Unexplored Tiles
    //         } else if (priority3Tile == null && tilePoint.explored == false && tilePoint.hasLandmark == false) {
    //             priority3Tile = floorTile.gameObject;
    //         }
    //     }
    //     if (priority1Tile != null) {
    //         selectedTile = priority1Tile;
    //         return selectedTile;
    //     } else if (priority2Tile != null) {
    //         selectedTile = priority2Tile;
    //         return selectedTile;
    //     } else if (priority3Tile != null) {
    //         selectedTile = priority3Tile;
    //         return selectedTile;
    //     } 
    //     // 4. Nearest Bubble Edge
    //     int min = 99999999;
    //     foreach (var p in GridGen.allGridPoints) {
    //         if (p.getType() != GridPoint.tileType.ABYSS) {
    //             continue;
    //         }
    //         int tileX = p.x_pos;
    //         int tileY = p.y_pos;
    //         if ((tileX-bub_x_pos)*(tileX-bub_x_pos) + (tileY-bub_y_pos)*(tileY-bub_y_pos) < min) {
    //             selectedTile = p.gameObject;
    //         }
    //     }
    //     return selectedTile;
    //     
    //
    // }

    void PrintPath(List<GridPoint> path)
    {
#if DEBUG
        for (int i = 1; i < path.Count; i++)
        {
            Debug.DrawLine(new Vector3(path[i-1].x_pos, path[i-1].y_pos), new Vector3(path[i].x_pos, path[i].y_pos, 0), Color.blue);
        }
        Debug.DrawLine(new Vector3(path[0].x_pos, path[0].y_pos), new Vector3(bub_x_pos, bub_y_pos, 0), Color.blue);
#endif
    }

    private void TakeStep(List<GridPoint> path)
    {
        PrintPath(path);

        var p = path.First();

        int x_diff = p.x_pos - bub_x_pos;
        int y_diff = p.y_pos - bub_y_pos;
        if (!currentTrigger.Equals(" ")) {
            animator.ResetTrigger(currentTrigger);
        }
        if (x_diff == 0 && y_diff == 0) {
            currentTrigger = "idle_trigger";
            // animator.SetTrigger("idle_trigger"); //No diagonal animation, so I'll prioritize using up/forward anims for that
        } 
        if (x_diff > 0) {
            currentTrigger = "move_left";
            // animator.SetTrigger("move_right");
        } 
        if (x_diff < 0) {
            currentTrigger = "move_right";
        } 
        if (bub_y_pos < p.y_pos) {
            currentTrigger = "move_up";
            // animator.SetTrigger("move_up");
        } 
        if (p.y_pos < bub_y_pos) {
            currentTrigger = "move_forward";
            // animator.SetTrigger("move_forward");
        }
        
        animator.SetTrigger(currentTrigger);

        transform.position = new Vector3(p.x_pos, p.y_pos, 0);

        bub_x_pos = p.x_pos;
        bub_y_pos = p.y_pos;
        p.explored = true;
        
        AudioManager.PlaySound(AudioManager.Asset.Footsteps);
    }

    GridPoint.itemType TakeItem(GridPoint p)
    {
        Debug.Log($"Bub taken item {p.item} at {p}");

        var type = p.item;
        p.hasItem = false;
        p.item = GridPoint.itemType.NONE;
        
        switch (type) {
            case GridPoint.itemType.NEEDLE:
                setBubStrength(99999);
                hasNeedle = true;
                break;
            case GridPoint.itemType.BUBBLE_WAND:
                BubbleManager.addBubbles(BubbleManager.Instance.placableBubbleAmount);
                break;
            case GridPoint.itemType.BUBBLE_BOTTLE:
                BubbleManager.addHealth(BubbleManager.Instance.healthRecoverAmount);
                break;
            case GridPoint.itemType.SPIKE_BALL:
                SceneManager.LoadScene("GameOver");
                break;
            default:
                break;
        }
        
        return type;
    }

    GridPoint.landmarkType TakeLandmark(GridPoint p)
    {
        // TODO: landmark effects
        Debug.Log($"Bub taken landmark {p.landmark} at {p}");

        var type = p.landmark;
        p.hasLandmark = false;
        p.landmark = GridPoint.landmarkType.NONE;
        return type;
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
