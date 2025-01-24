using System.Collections.Generic;
using UnityEngine;

public class TurnObject : MonoBehaviour
{
    public Queue<ITurnAction> Schedule = new();

    public TurnResult IdleTurn(TurnContext ctx)
    {
        Debug.Log($"Actor {this} idle at turn {ctx.TurnNumber}");
        return TurnResult.Continue;
    }
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

public class TurnActionComposite
{
    public List<ITurnAction> Content = new();
    
    TurnResult DoAction(TurnContext ctx, TurnObject parent)
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

public class TurnActionIdle
{
    TurnResult DoAction(TurnContext ctx, TurnObject parent)
    {
        return parent.IdleTurn(ctx);
    }
}

public class TurnActionDestroy
{
    TurnResult DoAction(TurnContext ctx, TurnObject parent)
    {
        return TurnResult.Destroyed;
    }
}