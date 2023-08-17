using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class SplatMapUtilsWindow : EditorWindow
{
    public Terrain Terrain;
    public List<Texture2D> SplatChannels;

    private bool isImportMode = true;
    private bool displaySplatChannels;
    private Vector2 scrollPosition;

    [MenuItem("Window/Splat Map Utils")]
    static void Initialize()
    {
        SplatMapUtilsWindow window = EditorWindow.GetWindow<SplatMapUtilsWindow>();
        window.Show();
    }

    private void OnGUI()
    {
        using (EditorGUILayout.ScrollViewScope scope = new EditorGUILayout.ScrollViewScope(scrollPosition, GUILayout.MinWidth(position.width), GUILayout.MinHeight(position.height)))
        {
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.BeginHorizontal();
            isImportMode = GUILayout.Toggle(isImportMode, "Import", EditorStyles.miniButtonLeft);
            isImportMode = !GUILayout.Toggle(!isImportMode, "Export", EditorStyles.miniButtonRight);

            EditorGUILayout.EndHorizontal();

            this.Terrain = (Terrain)EditorGUILayout.ObjectField("Terrain", this.Terrain, typeof(Terrain), true);

            if (this.Terrain == null) return;

            int splatChannelCount = this.Terrain.terrainData.alphamapLayers;

            if (EditorGUI.EndChangeCheck() || this.SplatChannels == null)
            {
                if (isImportMode)
                {
                    this.SplatChannels = new List<Texture2D>();

                    for (int i = 0; i < splatChannelCount; i++) this.SplatChannels.Add(null);
                }
                else
                {
                    this.SplatChannels = ExtractSplatChannelsFromTerrainData(this.Terrain);
                }
            }

            displaySplatChannels = EditorGUILayout.Foldout(displaySplatChannels, "Splat Map Channels");
            if (displaySplatChannels)
            {
                EditorGUI.indentLevel = 1;
                int count = this.SplatChannels.Count;
                for (int i = 0; i < count; i++)
                {
                    this.SplatChannels[i] = (Texture2D)EditorGUILayout.ObjectField("Channel " + i, this.SplatChannels[i], typeof(Texture2D), false);
                }
                EditorGUI.indentLevel = 0;
            }

            if (isImportMode)
            {
                if (GUILayout.Button("Import Splat Channels"))
                {
                    float[,,] splatMaps = ImportSplatChannels(this.SplatChannels);

                    this.Terrain.terrainData.SetAlphamaps(0, 0, splatMaps);
                }
            }
            else
            {
                if (GUILayout.Button("Export Splat Channels"))
                {
                    ExportSplatChannels(this.SplatChannels);
                }
            }


            scrollPosition = scope.scrollPosition;
        }
    }

    private static List<Texture2D> ExtractSplatChannelsFromTerrainData(Terrain terrain)
    {
        int splatChannelCount = terrain.terrainData.alphamapLayers;

        List<Texture2D> splatChannels = new List<Texture2D>(splatChannelCount);

        int width = terrain.terrainData.alphamapWidth;
        int height = terrain.terrainData.alphamapHeight;

        float[,,] splatMaps = terrain.terrainData.GetAlphamaps(0, 0, width, height);

        for (int i = 0; i < splatChannelCount; i++)
        {
            EditorUtility.DisplayProgressBar("Extracting Splat Maps...", "Extracting " + (i + 1) + " of " + splatChannelCount, (float)i / splatChannelCount);

            Texture2D splat = new Texture2D(width, height); //new Texture2D(width, height, TextureFormat.RFloat, false);

            Color[] textureData = new Color[width * height];

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < height; x++)
                {
                    float val = splatMaps[x, y, i];
                    Color color = new Color(val, val, val, 1);

                    textureData[y * width + x] = color;
                }
            }

            splat.SetPixels(textureData);

            splat.Apply();

            splatChannels.Add(splat);

        }

        EditorUtility.ClearProgressBar();

        return splatChannels;
    }

    private static float[,,] ImportSplatChannels(List<Texture2D> splatChannels)
    {
        if (splatChannels == null) throw new ArgumentNullException("splatChannels");
        if (splatChannels.Count == 0) throw new ArgumentException("splatChannels");
        if (splatChannels.Any(t => t == null)) throw new ArgumentException("splatChannels");

        if (splatChannels.Select(t => new Vector2(t.width, t.height)).Distinct().Count() != 1) throw new InvalidOperationException("The splat channels must be of equal size.");

        int width = splatChannels[0].width;
        int height = splatChannels[0].height;
        int layers = splatChannels.Count;
        int pixelCount = width * height;

        float[,,] map = new float[width, height, layers];

        var channelPixels = splatChannels.Select(t => t.GetPixels().Select(c => c.r).ToArray()).ToArray();

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                int j = y * width + x;

                EditorUtility.DisplayProgressBar("Importing Splat Channels...", (j * 100) / pixelCount + "% complete", j / (float)(pixelCount));

                float total = channelPixels.Sum(t => t[y * width + x]);

                for (int i = 0; i < layers; i++)
                {
                    if (total < 1e-5f)
                    {
                        map[x, y, i] = 1f / layers;
                    }
                    else
                    {
                        map[x, y, i] = channelPixels[i][j] / total;
                    }
                }
            }
        }

        EditorUtility.ClearProgressBar();

        return map;
    }

    private static void ExportSplatChannels(List<Texture2D> splatChannels)
    {
        int splatChannelCount = splatChannels.Count;

        string exportName = EditorUtility.SaveFilePanel("Export Splat Channels...", "", "splatChannel.png", "png");

        //if (string.IsNullOrEmpty(exportName)) return;

        int i = 0;
        foreach (Texture2D texture in splatChannels)
        {
            EditorUtility.DisplayProgressBar("Exporting Splat Channels...", "Exporting " + (i + 1) + " of " + splatChannelCount, (float)i / splatChannelCount);

            string textureName;

            int index = exportName.LastIndexOf(".");

            if (index == -1)
            {
                textureName = exportName + i + ".png";
            }
            else
            {
                textureName = exportName.Insert(index, i.ToString());
            }

            byte[] fileBytes = texture.EncodeToPNG();

            if (fileBytes != null) File.WriteAllBytes(textureName, fileBytes);

            i++;
        }

        EditorUtility.ClearProgressBar();
    }
}
