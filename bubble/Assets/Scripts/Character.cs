using UnityEngine;

public class Character : TurnObject
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        ScheduleAfterNow(1, new TurnActionLambda(RepeaterA), MultipleActionResolution.AddAsFirstInTurn);
        ScheduleAfterNow(1, new TurnActionLambda(RepeaterB), MultipleActionResolution.AddAsFirstInTurn);
    }

    private int m_repeaterA = 0;
    TurnResult RepeaterA(TurnContext ctx, TurnObject to)
    {
        const int interval = 5;
        Debug.Log($"Repeater A invocation {++m_repeaterA}, next invocation in {interval} turns...");
        ScheduleAfterNow(interval, new TurnActionLambda(RepeaterA), MultipleActionResolution.AddAsFirstInTurn);
        return TurnResult.Continue;
    }
    
    private int m_repeaterB = 0;
    TurnResult RepeaterB(TurnContext ctx, TurnObject to)
    {
        const int interval = 2;
        Debug.Log($"Repeater B invocation {++m_repeaterB}, next invocation in {interval} turns...");
        ScheduleAfterNow(interval, new TurnActionLambda(RepeaterB), MultipleActionResolution.AddAsFirstInTurn);
        return TurnResult.Continue;
    }
}
