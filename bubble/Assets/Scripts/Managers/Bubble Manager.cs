using UnityEngine;
using TMPro;

public class BubbleManager : MonoBehaviour
{
    public static BubbleManager Instance;
    [SerializeField] public int current_bubbles;
    [SerializeField] public int max_bubbles;
    [SerializeField] public int current_health;
    [SerializeField] public int max_health;
    [SerializeField] public TMP_Text TEMP_Bubble_Health;
    [SerializeField] public TMP_Text TEMP_Bubbles_Remaining;
    [SerializeField] public float regenTimer;
    [SerializeField] public int regenTimerThreshold;
    [SerializeField] public int regenAmount;
    [SerializeField] public int placableBubbleAmount;
    [SerializeField] public int healthRecoverAmount;



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
        Instance.current_bubbles--;
    }

    public static void loseHealth(int deduction) {
        Instance.current_health -= deduction;
        if (Instance.current_health < 0) {
            Instance.current_health = 0;
        }
    }

    public static void addHealth(int increase) {
        Instance.current_health += increase;
        if (Instance.current_health > Instance.max_health) {
            Instance.current_health = Instance.max_health;
        }
    }

    public static void regenHealth() {
        addHealth(Instance.regenAmount);
    }

    public static void setMaxHealth(int newMax) {
        Instance.max_health = newMax;
        if (Instance.current_health > Instance.max_health) {
            Instance.current_health = Instance.max_health;
        }
    }

    public static void refundBubbles() {
        Instance.current_bubbles += (int)(.75 * (Instance.max_bubbles - Instance.current_bubbles)); //add back 75% of what we lost
        Instance.max_bubbles = Instance.current_bubbles;
    }

    public static void addBubbles(int bubbleAmount) {
        Instance.current_bubbles += bubbleAmount;
        if (Instance.current_bubbles > Instance.max_bubbles) {
            Instance.current_bubbles = Instance.max_bubbles;
        }
    }
}
