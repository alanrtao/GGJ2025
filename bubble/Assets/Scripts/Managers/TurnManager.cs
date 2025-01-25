using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class TurnManager : MonoBehaviour
{

    public static TurnManager Instance;

    [SerializeField]
    private List<TurnObject> turnObjects;
    
    private bool gameFinished = false;
    
    [SerializeField]
    private int turnIntervalSeconds = 3;

    private bool m_paused;
    private int m_turn;
    
    [SerializeField]
    private GameObject pauseMenu;
    
    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        StartCoroutine(Turn());
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            m_paused = !m_paused;
            Time.timeScale = m_paused ? 0 : 1;
            if (m_paused)
            {
                pauseMenu.SetActive(m_paused);
            }
        }
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

            var actions = turnObjects.Select(to 
                => to.Schedule.Count == 0 ? (to, null as ITurnAction) : (to, to.Schedule.Dequeue()));

            turnObjects = actions.Where(to_act =>
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
