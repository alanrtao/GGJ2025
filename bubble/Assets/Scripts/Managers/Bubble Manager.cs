using System.Collections;
using System.Collections.Generic;
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

    private int current_health
    {
        get => m_current_health;
        set
        {
            m_current_health = value;
            hpMaterial.SetFloat(Hp01, ((float) value) / max_health);
        }
    }
    
    private int m_current_health;
    [SerializeField] public int max_health;
    [SerializeField] public TMP_Text TEMP_Bubble_Health;
    [SerializeField] public TMP_Text TEMP_Bubbles_Remaining;
    private float m_regenTimer;
    [SerializeField] public int regenTimerThreshold;
    [SerializeField] public int regenAmount;
    [SerializeField] public int placableBubbleAmount;
    [SerializeField] public int healthRecoverAmount;

    # region UI
    [SerializeField] private Material hpMaterial;
    [SerializeField] private Material bubbleMaterial;
    private Color m_bubbleDefaultColor;
    private static readonly int Hp01 = Shader.PropertyToID("_hp01");
    private static readonly int ColorID = Shader.PropertyToID("_Color");

    #endregion

    void Awake()
    {
        Instance = this;
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        m_current_bubbles = max_bubbles;
        current_health = max_health;
        m_bubbleDefaultColor = bubbleMaterial.GetColor(ColorID);
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
        TEMP_Bubble_Health.text = "Health: " + current_health;
        TEMP_Bubbles_Remaining.text = "Bubbles: " + m_current_bubbles;
        // if (m_current_health <= 0) {
        //     SceneManager.LoadScene("GameOver");
        // }

    }

    public static void loseBubble() {
        Instance.m_current_bubbles--;
    }

    public static void loseHealth(int deduction) {

        Instance.current_health -= deduction;
        if (Instance.current_health < 0) {
            Instance.current_health = 0;
            SceneManager.LoadScene("GameOver");
        }
        Instance.bubbleMaterial.SetColor(ColorID, Instance.m_bubbleDefaultColor);
        Instance.StopAllCoroutines();
        Instance.StartCoroutine(Instance.HealthAnimation(Color.red));
    }

    public static void addHealth(int increase) {
        Instance.current_health += increase;
        if (Instance.current_health > Instance.max_health) {
            Instance.current_health = Instance.max_health;
            // Instance.bubbleMaterial.SetColor(ColorID, Instance.m_bubbleDefaultColor);
            // Instance.StopAllCoroutines();
            // Instance.StartCoroutine(Instance.HealthAnimation(Color.green));
        }
    }

    IEnumerator HealthAnimation(Color overlay)
    {
        for (float i = 0; i < 0.2f; i += Time.deltaTime)
        {
            var t = (0.2f - i) / 0.2f;
            var c = m_bubbleDefaultColor + overlay * t;
            bubbleMaterial.SetColor(ColorID, c);
            yield return null;
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
