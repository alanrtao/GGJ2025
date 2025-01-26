using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;


public class BubbleManager : MonoBehaviour
{
    public static BubbleManager Instance;
    private int m_current_bubbles;
    public static int CurrentBubbles => Instance.m_current_bubbles;
    [SerializeField] public int max_bubbles;
    private int m_current_health;
    [SerializeField] public int max_health;
    [SerializeField] public TMP_Text TEMP_Bubble_Health;
    [SerializeField] public TMP_Text TEMP_Bubbles_Remaining;
    private float m_regenTimer;
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
        m_current_bubbles = max_bubbles;
        m_current_health = max_health;
    }

    // Update is called once per frame
    void Update()
    {
        m_regenTimer += Time.deltaTime;
        if (m_regenTimer >= regenTimerThreshold) {
            regenHealth();
            m_regenTimer = 0;
        }
        // Debug.Log(m_current_health);
        TEMP_Bubble_Health.text = "Health: " + m_current_health;
        TEMP_Bubbles_Remaining.text = "Bubbles: " + m_current_bubbles;
        // if (m_current_health <= 0) {
        //     SceneManager.LoadScene("GameOver");
        // }

    }

    public static void loseBubble() {
        Instance.m_current_bubbles--;
    }

    public static void loseHealth(int deduction) {

        Instance.m_current_health -= deduction;
        if (Instance.m_current_health < 0) {
            Instance.m_current_health = 0;
            SceneManager.LoadScene("GameOver");
        }
    }

    public static void addHealth(int increase) {
        Instance.m_current_health += increase;
        if (Instance.m_current_health > Instance.max_health) {
            Instance.m_current_health = Instance.max_health;
        }
    }

    public static void regenHealth() {
        addHealth(Instance.regenAmount);
    }

    public static void setMaxHealth(int newMax) {
        Instance.max_health = newMax;
        if (Instance.m_current_health > Instance.max_health) {
            Instance.m_current_health = Instance.max_health;
        }
    }

    public static void refundBubbles() {
        Instance.m_current_bubbles += (int)(.75 * (Instance.max_bubbles - Instance.m_current_bubbles)); //add back 75% of what we lost
        Instance.max_bubbles = Instance.m_current_bubbles;
    }

    public static void addBubbles(int bubbleAmount) {
        Instance.m_current_bubbles += bubbleAmount;
        if (Instance.m_current_bubbles > Instance.max_bubbles) {
            Instance.m_current_bubbles = Instance.max_bubbles;
        }
    }
}
