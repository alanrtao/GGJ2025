using System;
using System.Collections.Generic;
using UnityEngine;

public class BubblePostProcManager : MonoBehaviour
{
    // texture matching the grid size, R channel = x, G channel = y, B channel = vacant or filled
    private static Texture2D m_dat;
    
    [SerializeField] private Material material;
    private static readonly int GridID = Shader.PropertyToID("_Grid");

    public static BubblePostProcManager Instance;
    
    private void Awake()
    {
        Instance = this;
    }

    public static void OnGridInitialize(IEnumerable<GridPoint> pts)
    {
        m_dat = new Texture2D(GridGen.Instance.gridWidth, GridGen.Instance.gridHeight, TextureFormat.R8, false); // replace with gridgen width and height
        OnGridUpdate(pts);
    }

    public static void OnGridUpdate(IEnumerable<GridPoint> pts)
    {
        Instance.UpdateGrid(pts);
    }
    
    void UpdateGrid(IEnumerable<GridPoint> pts)
    {
        for (int i = 0; i < m_dat.width; i += 1)
        {
            for (int j = 0; j < m_dat.height; j += 1)
            {
                m_dat.SetPixel(i, j, Color.black);
            }
        }

        foreach (var pt in pts)
        {
            m_dat.SetPixel(pt.x_pos + GridGen.Instance.gridWidth / 2, pt.y_pos + GridGen.Instance.gridHeight / 2, Color.red);
        }
        m_dat.Apply(false, false);
        material.SetTexture(GridID, m_dat);
    }
}
