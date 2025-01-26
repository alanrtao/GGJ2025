using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Utils;
using Random = UnityEngine.Random;

public class Character : TurnObject
{
    [SerializeField] private int bub_x_pos;
    [SerializeField] private int bub_y_pos;
    [SerializeField] private int bubMoveSpeed;
    [SerializeField] private int bubStrength;
    GridPoint.landmarkType bubDesire;
    [SerializeField] private float needleTimer;
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
        ScheduleDecideNewState(1, State.Idle);
    }

    void Update()
    {
        if (hasNeedle) {
            updateNeedleTimer();
        }
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
            // TODO: animate bumping in the direction of nearbyNonBubble
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
        var neighbors = GridGen.GetNeighbors(current, GridGen.Not(GridGen.IsBubble));
        if (neighbors.Count == 0)
        {
            adjBubble = null;
            return false;
        }

        var neighbors_ = neighbors.ToArray();
        adjBubble = neighbors_[Random.Range(0, neighbors_.Length)];
        return true;
    }

    void IndicateState(State state)
    {
        Debug.Log($"Bub state = {state}");
    }
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
        bub_x_pos = p.x_pos;
        bub_y_pos = p.y_pos;
        p.explored = true;
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
            //WIN CODE GOES HERE
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
