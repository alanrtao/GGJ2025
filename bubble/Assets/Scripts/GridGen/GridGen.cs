using System;
using UnityEngine;

public class GridGen : MonoBehaviour
{
    [SerializeField] GameObject gridPoint;
    [SerializeField] float gridScale;
    [SerializeField] int gridWidth;
    [SerializeField] int gridHeight;

    public static GridGen Instance;

    private void Awake()
    {
        Instance = this;
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        genGrid(gridWidth, gridHeight);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void genGrid(int width, int height) {
        for (int i = -width/2; i < Mathf.CeilToInt(width/2.0f); i++) {
            for (int j = -height/2; j < Mathf.CeilToInt(height/2.0f); j++) {
                Instantiate(gridPoint, new Vector3(i, j, 0), Quaternion.identity);
            }
        }
    }
}
