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
            pauseMenu.SetActive(m_paused);
        }
    }

    private IEnumerator Turn()
    {
        int turn = 0;
        while (!gameFinished)
        {
            TurnContext ctx = new()
            {
                TurnNumber = turn
            };

            turnObjects = turnObjects.Where(to =>
            {
                var res = TurnResult.Continue;
                if (to.Schedule.Count == 0)
                {
                    res = to.IdleTurn(ctx);
                }
                else
                {
                    var act = to.Schedule.Dequeue();
                    res = act.DoAction(ctx, to);
                }
                return res != TurnResult.Destroyed;
            }).ToList();
            yield return new WaitForSeconds(turnIntervalSeconds); // timescale = 0 will freeze this, this is intended
            ++turn;
        }
    }
}
