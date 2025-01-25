using UnityEngine;

public class BubbleManager : MonoBehaviour
{
    public static BubbleManager Instance;
    public static int current_bubbles;
    public static int max_bubbles;
    public static int remaining_health;
    public static int used_bubbles;



    void Awake()
    {
        Instance = this;
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        current_bubbles = 30;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public static void loseBubble() {
        current_bubbles--;
    }

    public static void refundBubbles() {
        current_bubbles += (int)(.75 * (max_bubbles - current_bubbles)); //add back 75% of what we lost
        max_bubbles = current_bubbles;
    }
}
