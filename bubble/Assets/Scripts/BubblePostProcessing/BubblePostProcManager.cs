using System;
using UnityEngine;

public class BubblePostProcManager : MonoBehaviour
{
    // texture matching the grid size, R channel = x, G channel = y, B channel = vacant or filled
    private Texture2D m_dat;
    
    [SerializeField] private Material material;
    private static readonly int GridID = Shader.PropertyToID("_Grid");
    private static readonly int GridSizeID = Shader.PropertyToID("_GridSize");

    private void Awake()
    {
        m_dat = new Texture2D(4 * 6, 1, TextureFormat.R8, false); // replace with gridgen width and height
        
        UpdateGrid();
    }

    public void UpdateGrid()
    {
        var fakeData = new int[][]
        {
            new int[]{0, 0, 1, 1, 0, 0},
            new int[]{1, 0, 1, 1, 0, 0},
            new int[]{0, 1, 0, 0, 1, 1},
            new int[]{0, 0, 0, 0, 1, 1}
        };
        
        for (int i = 0; i < 4; i++)
        {
            for (int j = 0; j < 6; j++)
            {
                if (fakeData[i][j] > 0)
                {
                    m_dat.SetPixel(j, i, Color.red);
                }
                else
                {
                    m_dat.SetPixel(j, i, Color.black);
                }
            }
        }
        m_dat.Apply(false, false);
        material.SetTexture(GridID, m_dat);
        material.SetFloat(GridSizeID, 1); // fetch from grid
    }
}
