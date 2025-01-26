using UnityEngine;
using TMPro;

public class BubbleManager : MonoBehaviour
{
    [SerializeField] public static BubbleManager Instance;
    [SerializeField] public static int current_bubbles;
    [SerializeField] public static int max_bubbles;
    [SerializeField] public static int current_health;
    [SerializeField] public static int max_health;
    [SerializeField] public TMP_Text TEMP_Bubble_Health;
    [SerializeField] public TMP_Text TEMP_Bubbles_Remaining;
    [SerializeField] public static float regenTimer;
    [SerializeField] public static int regenTimerThreshold;
    [SerializeField] public static int regenAmount;
    [SerializeField] public static int placableBubbleAmount;
    [SerializeField] public static int healthRecoverAmount;



    void Awake()
    {
        Instance = this;
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        max_bubbles = 5;
        current_bubbles = 5;
        max_health = 500;
        current_health = 500;
        regenTimerThreshold = 1;
        regenAmount = 5;
        placableBubbleAmount = 3;
        healthRecoverAmount = 150;
    }

    // Update is called once per frame
    void Update()
    {
        regenTimer += Time.deltaTime;
        if (regenTimer >= regenTimerThreshold) {
            regenHealth();
            regenTimer = 0;
        }
        TEMP_Bubble_Health.text = "Health: " + current_health;
        TEMP_Bubbles_Remaining.text = "Bubbles: " + current_bubbles;
    }

    public static void loseBubble() {
        current_bubbles--;
    }

    public static void loseHealth(int deduction) {
        current_health -= deduction;
        if (current_health < 0) {
            current_health = 0;
        }
    }

    public static void addHealth(int increase) {
        current_health += increase;
        if (current_health > max_health) {
            current_health = max_health;
        }
    }

    public static void regenHealth() {
        addHealth(regenAmount);
    }

    public static void setMaxHealth(int newMax) {
        max_health = newMax;
        if (current_health > max_health) {
            current_health = max_health;
        }
    }

    public static void refundBubbles() {
        current_bubbles += (int)(.75 * (max_bubbles - current_bubbles)); //add back 75% of what we lost
        max_bubbles = current_bubbles;
    }

    public static void addBubbles(int bubbleAmount) {
        current_bubbles += bubbleAmount;
        if (current_bubbles > max_bubbles) {
            current_bubbles = max_bubbles;
        }
    }
}
