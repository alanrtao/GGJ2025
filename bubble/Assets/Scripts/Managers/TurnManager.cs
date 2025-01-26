using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class TurnManager : MonoBehaviour
{

    public static TurnManager Instance;
    private List<TurnObject> m_turnObjects;
    
    private bool gameFinished = false;
    
    [SerializeField]
    private int turnIntervalSeconds = 3;
    private int m_turn;
    
    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        m_turnObjects = FindObjectsByType<TurnObject>(FindObjectsInactive.Exclude, FindObjectsSortMode.None).ToList();
        StartCoroutine(Turn());
    }

    public static TurnContext CurrentCtx => new TurnContext()
    {
        TurnNumber = Instance.m_turn
    };

    private IEnumerator Turn()
    {
        while (!gameFinished)
        {
            TurnContext ctx = CurrentCtx;

            var actions = m_turnObjects.Select(to 
                => to.Schedule.Count == 0 ? (to, null as ITurnAction) : (to, to.Schedule.Dequeue()));

            m_turnObjects = actions.Where(to_act =>
            {
                var (to, act) = to_act;
                var res = act?.DoAction(ctx, to) ?? to.IdleTurn(ctx);
                return res != TurnResult.Destroyed;
            }).Select(to_act =>
            {
                var (to, act) = to_act;
                return to;
            }).ToList();
            yield return new WaitForSeconds(turnIntervalSeconds); // timescale = 0 will freeze this, this is intended
            ++m_turn;
        }
    }
}
