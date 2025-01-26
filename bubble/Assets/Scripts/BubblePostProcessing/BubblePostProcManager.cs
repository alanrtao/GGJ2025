using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BubblePostProcManager : MonoBehaviour
{
    // texture matching the grid size, R channel = x, G channel = y, B channel = vacant or filled
    private Texture2D m_dat;
    
    [SerializeField] private Material material;
    private static readonly int GridID = Shader.PropertyToID("_Grid");

    [SerializeField] private GridPoint.tileType responsibility;

    public static void OnGridInitialize()
    {
        foreach (var man in FindObjectsByType<BubblePostProcManager>(FindObjectsInactive.Exclude,
                     FindObjectsSortMode.None))
        {
            man.m_dat = new Texture2D(GridGen.Instance.gridWidth, GridGen.Instance.gridHeight, TextureFormat.R8, false); // replace with gridgen width and height
        }
        OnGridUpdate();
    }

    public static void OnGridUpdate()
    {
        foreach (var man in FindObjectsByType<BubblePostProcManager>(FindObjectsInactive.Exclude,
                     FindObjectsSortMode.None))
        {
            // Debug.Log($"{man} -> {man.responsibility}");
            // foreach (var p in GridGen.allGridPoints)
            // {
            //     Debug.Log($"[{p.type}]: {p}");
            //     if (p.type == man.responsibility)
            //     {
            //         Debug.Log($"[{man.responsibility}]: {p}");
            //     }
            // }
            man.UpdateGrid(GridGen.allGridPoints.Where(p => p.type == man.responsibility));
        }
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
