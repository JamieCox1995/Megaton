using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEditor;
using UnityEngine;

public class LowPolyTerrainHeightmapExport : ScriptableWizard
{
    public int resolution;
    public DepthFormat depth;
    public ByteMode byteMode = ByteMode.Intel;

    private LowPolyHeightmap SelectedHeightmap
    {
        get
        {
            var gameObject = Selection.activeGameObject;

            if (gameObject == null) return null;

            var terrain = gameObject.GetComponent<LowPolyTerrain>();

            if (terrain == null) return null;

            return terrain.Heightmap;
        }
    }

    [MenuItem("Window/Low Poly Terrain Heightmap Export")]
    private static void Initialize()
    {
        var wizard = ScriptableWizard.DisplayWizard<LowPolyTerrainHeightmapExport>("Export Heightmap", "Export");
        wizard.Show();
    }

    private void Update()
    {
        if (Selection.activeGameObject == null)
        {
            helpString = "Nothing selected.";
        }
        else if (this.SelectedHeightmap == null)
        {
            helpString = "The selected Game Object has no Low Poly Terrain component.";
        }
        else
        {
            helpString = string.Format("Selected terrain: {0}", Selection.activeGameObject.name);
        }
    }

    private void OnWizardCreate()
    {
        if (this.resolution <= 0) throw new InvalidOperationException();
        if (this.SelectedHeightmap == null) return;

        string fileName = EditorUtility.SaveFilePanelInProject("Save heightmap", "heightmap", "raw", "");

        byte[] fileBytes = GetHeightmapData(this.SelectedHeightmap, this.resolution, this.depth, this.byteMode);

        if (fileBytes == null) return;

        using (var file = System.IO.File.Create(fileName))
        {
            file.Write(fileBytes, 0, fileBytes.Length);
        }
    }

    private static byte[] GetHeightmapData(LowPolyHeightmap heightmap, int resolution, DepthFormat depthFormat, ByteMode byteMode)
    {
        int byteCount = (int)depthFormat / 8;
        int maxValue = 0;
        switch (depthFormat)
        {
            case DepthFormat._8Bit:
                maxValue = byte.MaxValue;
                break;

            case DepthFormat._16Bit:
                maxValue = short.MaxValue;
                break;

            case DepthFormat._32Bit:
                maxValue = int.MaxValue;
                break;
        }

        byte[] bytes = new byte[resolution * resolution * byteCount];

        var timer = Stopwatch.StartNew();

        float scale = heightmap.TerrainSize / resolution;
        float maxHeight = heightmap.TerrainHeight;

        try
        {
            for (int y = 0; y < resolution; y++)
            {
                for (int x = 0; x < resolution; x++)
                {
                    int index = (y * resolution) + x;

                    float progress = (float)index / (float)(resolution * resolution);

                    var timeSpan = EstimateRemainingTime(timer, progress);

                    if (EditorUtility.DisplayCancelableProgressBar(string.Format("Creating heightmap - {0}%", progress * 100f),
                                                                   timeSpan.HasValue ? string.Format("{0} remaining", timeSpan) : "",
                                                                   progress))
                    {
                        return null;
                    }

                    float height = heightmap.SampleHeight(x * scale, y * scale) / maxHeight;

                    if (depthFormat == DepthFormat._32Bit)
                    {
                        // Use floating-point values directly
                        byte[] floatBytes = BitConverter.GetBytes(height);

                        bool swapBytes = (byteMode == ByteMode.Intel) ^ BitConverter.IsLittleEndian;

                        for (int i = 0; i < byteCount; i++)
                        {
                            int j = swapBytes ? byteCount - i - 1 : i;
                            bytes[index * byteCount + i] = floatBytes[j];
                        }
                    }
                    else
                    {
                        // Use an integer value
                        int value = (int)(height * maxValue);

                        for (int i = 0; i < byteCount; i++)
                        {
                            int shift;
                            int mask = 0xff;
                            if (byteMode == ByteMode.Motorola)
                            {
                                shift = (byteCount - i - 1) * 8;
                            }
                            else
                            {
                                shift = i * 8;
                            }

                            bytes[index * byteCount + i] = (byte)((value >> shift) & mask);
                        }
                    }
                }
            }
        }
        finally
        {
            EditorUtility.ClearProgressBar();
        }

        return bytes;
    }

    private static TimeSpan? EstimateRemainingTime(Stopwatch timer, float progress)
    {
        if (progress < 0.01f) return null;

        var estimatedTimeRemaining = (timer.Elapsed.TotalSeconds / progress) - timer.Elapsed.TotalSeconds;

        return TimeSpan.FromSeconds(estimatedTimeRemaining);
    }

    public enum DepthFormat
    {
        _8Bit = 8,
        _16Bit = 16,
        _32Bit = 32
    }

    public enum ByteMode
    {
        Intel,
        Motorola
    }
}
