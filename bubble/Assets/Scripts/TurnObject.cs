using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class TurnObject : MonoBehaviour
{
    public Queue<ITurnAction> Schedule = new();

    public TurnResult IdleTurn(TurnContext ctx)
    {
        Debug.Log($"Actor {this} idle at turn {ctx.TurnNumber}");
        return TurnResult.Continue;
    }

    /**
     * returns scheduled turn number, -1 means not scheduled
     * turnsAfterNow (1) means to schedule next turn
     */
    public int ScheduleAfterNow(int turnsAfterNow, ITurnAction action, MultipleActionResolution strategy)
    {
        if (turnsAfterNow <= 0)
        {
            throw new Exception($"Must schedule at least 1 turn after ({turnsAfterNow})");
        }

        if (action == null)
        {
            throw new Exception(
                $"Do not intentionally schedule a null (idle) action! System should insert idles by itself");
        }
        
        int now = TurnManager.CurrentCtx.TurnNumber;
        int sched = now + turnsAfterNow;

        if (Schedule.Count + 1 <= turnsAfterNow)
        {
            // add item to end of schedule
            for (int i = 0; i < turnsAfterNow - 1; i++)
            {
                Schedule.Enqueue(null); // insert empty turn
            }
            Schedule.Enqueue(action); // i = t_rel - 1, this is the ideal state
        }
        else
        {
            // replace existing schedule item
            var existing = Schedule.ToList();

            int i = turnsAfterNow - 1;

            if (existing[i] == null)
            {
                existing[i] = action;
            }
            else
            {
                var target = existing[i] as TurnActionComposite ?? new TurnActionComposite()
                {
                    Content = new List<ITurnAction>
                    {
                        existing[i]
                    }
                };
                
                switch (strategy)
                {
                    case MultipleActionResolution.AddAsFirstInTurn:
                        target.Content.Insert(0, action);
                        break;
                    case MultipleActionResolution.AddAsLastInTurn:
                        target.Content.Add(action);
                        break;
                    case MultipleActionResolution.DoNotAddInTurn:
                        return -1;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(strategy), strategy, null);
                }

                existing[i] = target;
            }

            Schedule = new Queue<ITurnAction>(existing);
        }
        
        return sched;
    }

    public int ScheduleAfterEndOfQueue(int turnsAfterQueue, ITurnAction action, MultipleActionResolution strategy)
        => ScheduleAfterNow(Schedule.Count + turnsAfterQueue, action, strategy);
}

/**
 * If there's already something else happening at the turn, how should the new action be scheduled?
 */
public enum MultipleActionResolution
{
    AddAsFirstInTurn,
    AddAsLastInTurn,
    DoNotAddInTurn
}

public struct TurnContext
{
    public int TurnNumber;
}

public enum TurnResult
{
    Continue,
    Destroyed
}

public interface ITurnAction
{
    TurnResult DoAction(TurnContext ctx, TurnObject parent);
}

public class TurnActionLambda : ITurnAction
{
    private Func<TurnContext, TurnObject, TurnResult> Do;

    public TurnActionLambda(Func<TurnContext, TurnObject, TurnResult> Do)
    {
        this.Do = Do;
    }

    public TurnResult DoAction(TurnContext ctx, TurnObject parent) => Do?.Invoke(ctx, parent) ?? parent.IdleTurn(ctx);
}

public class TurnActionComposite: ITurnAction
{
    public List<ITurnAction> Content = new();
    
    public TurnResult DoAction(TurnContext ctx, TurnObject parent)
    {
        foreach (var action in Content)
        {
            var result = action.DoAction(ctx, parent);
            if (result == TurnResult.Destroyed)
            {
                return result;
            }
        }

        return TurnResult.Continue;
    }
}

public class TurnActionDestroy: ITurnAction
{
    public TurnResult DoAction(TurnContext ctx, TurnObject parent)
    {
        return TurnResult.Destroyed;
    }
}