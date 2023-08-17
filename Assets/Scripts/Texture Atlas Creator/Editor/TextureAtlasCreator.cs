using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace TotalDistraction.TextureAtlasCreator
{
    public static class TextureAtlasCreator
    {
        public static TextureAtlas Create(TextureAtlasBundle bundle)
        {
            List<Texture> bakeList = GetBakeList(bundle);

            Bake.Settings settings = GetBakeSettings(bundle.Settings, bakeList);

            Bake.Result bake = Bake.Texture(settings, bakeList);

            List<GameObject> prefabs = bundle.BundledObjects.Select(CloneInMemory).ToList();
            List<Mesh> meshes = new List<Mesh>();
            List<Material> newMaterials = new List<Material>();

            foreach (GameObject obj in prefabs)
            {
                foreach (MeshRenderer renderer in obj.GetComponentsInChildren<MeshRenderer>())
                {
                    List<Material> sharedMaterials = new List<Material>();

                    List<Rect> uvRects = new List<Rect>();

                    foreach (Material material in renderer.sharedMaterials)
                    {
                        if (material.mainTexture != null)
                        {
                            int index = bakeList.IndexOf(material.mainTexture);

                            Rect rect = bake.TextureRects[index];

                            uvRects.Add(rect);

                            Material newMaterial = Material.Instantiate(material);
                            newMaterial.name = material.name;
                            newMaterial.mainTexture = bake.Output;

                            if (bundle.Settings.UVRemapMode == UVRemapMode.UseInstancedShaders)
                            {
                                newMaterial.shader = bundle.Settings.ShaderReplacements[material.shader];
                            }

                            MaterialEqualityComparer comparer = new MaterialEqualityComparer("_MainTex");

                            Material findMaterial = newMaterials.Find(m => comparer.Equals(m, newMaterial));

                            if (findMaterial == null)
                            {
                                sharedMaterials.Add(newMaterial);
                                newMaterials.Add(newMaterial);
                            }
                            else
                            {
                                sharedMaterials.Add(findMaterial);
                            }
                        }
                        else
                        {
                            sharedMaterials.Add(material);
                            uvRects.Add(new Rect(0, 0, 1, 1));
                        }
                    }

                    if (bundle.Settings.UVRemapMode == UVRemapMode.BakeIntoMesh)
                    {
                        MeshFilter meshFilter = renderer.gameObject.GetComponent<MeshFilter>();

                        Mesh newMesh = UVRemap.Forward(meshFilter.sharedMesh, uvRects.ToArray());

                        meshFilter.sharedMesh = newMesh;
                        meshes.Add(newMesh);

                        renderer.sharedMaterials = sharedMaterials.ToArray();
                    }
                    else
                    {
                        RuntimeUVRemap remap = renderer.gameObject.AddComponent<RuntimeUVRemap>();
                        remap.Mode = bundle.Settings.UVRemapMode;
                        remap.TextureRects = uvRects;
                        remap.Materials = sharedMaterials;
                    }
                }
            }

            TextureAtlas result = ScriptableObject.CreateInstance<TextureAtlas>();

            result.Texture = bake.Output;
            result.Materials = newMaterials;
            result.Meshes = meshes;
            result.Prefabs = prefabs;

            return result;
        }

        public static List<Shader> GetShaders(TextureAtlasBundle bundle)
        {
            HashSet<Shader> shaders = new HashSet<Shader>();

            foreach (GameObject obj in bundle.BundledObjects)
            {
                if (obj == null) continue;

                foreach (MeshRenderer renderer in obj.GetComponentsInChildren<MeshRenderer>())
                {
                    foreach (Material material in renderer.sharedMaterials)
                    {
                        shaders.Add(material.shader);
                    }
                }
            }

            return shaders.ToList();
        }

        public static bool Verify(TextureAtlasBundle bundle)
        {
            VerifyResult verify = VerifyBundle(bundle);

            if (!verify.IsSuccessful) Debug.LogError(verify.ToString(), bundle);

            return verify.IsSuccessful;
        }

        private static VerifyResult VerifyBundle(TextureAtlasBundle bundle)
        {
            VerifyResult result = new VerifyResult();

            if (bundle.BundledObjects.Count == 0)
            {
                result.Errors.Add("There are no objects to bundle.");
            }

            HashSet<Texture> textures = new HashSet<Texture>();

            for (int i = 0; i < bundle.BundledObjects.Count; i++)
            {
                GameObject obj = bundle.BundledObjects[i];

                if (obj == null)
                {
                    result.Errors.Add(string.Format("There is no object in position {0} of the bundled objects list.", i));
                    continue;
                }

                Renderer[] renderers = obj.GetComponentsInChildren<MeshRenderer>();

                if (renderers == null || renderers.Length == 0)
                {
                    result.Errors.Add(string.Format("The object \"{0}\" has no MeshRenderer component attached and cannot be bundled into a texture atlas.", obj.name));
                    continue;
                }
                else
                {
                    foreach (Renderer renderer in renderers)
                    {
                        foreach (Material material in renderer.sharedMaterials)
                        {
                            if (material.mainTexture != null) textures.Add(material.mainTexture);
                        }
                    }
                }
            }

            if (textures.Count == 0)
            {
                result.Errors.Add("There are no textures to bake.");
            }

            return result;
        }

        private static List<Texture> GetBakeList(TextureAtlasBundle bundle)
        {
            List<Texture> textures = new List<Texture>();

            foreach (GameObject obj in bundle.BundledObjects)
            {
                foreach (MeshRenderer renderer in obj.GetComponentsInChildren<MeshRenderer>())
                {
                    foreach (Material material in renderer.sharedMaterials)
                    {
                        if (material.mainTexture != null && !textures.Contains(material.mainTexture)) textures.Add(material.mainTexture);
                    }
                }
            }

            return textures;
        }

        private static Bake.Settings GetBakeSettings(TextureAtlasBundleSettings bundleSettings, List<Texture> bakeList)
        {
            Bake.Settings bakeSettings = new Bake.Settings();

            // Calculate horizontal and vertical slicing
            bakeSettings.XDivisions = Mathf.CeilToInt(Mathf.Sqrt(bakeList.Count));

            if (bundleSettings.KeepAspectRatio)
            {
                bakeSettings.YDivisions = bakeSettings.XDivisions;
            }
            else
            {
                bakeSettings.YDivisions = Mathf.CeilToInt(bakeList.Count / (float)bakeSettings.XDivisions);
            }

            // Calculate texture atlas size
            int maximumSize = (int)Mathf.Pow(2, bundleSettings.MaximumSize);

            int requiredSize = bakeList.Max(tex => tex.width) * bakeSettings.XDivisions;

            bakeSettings.Size = Mathf.Min(maximumSize, requiredSize);

            return bakeSettings;
        }

        private static GameObject CloneInMemory(GameObject original)
        {
            GameObject clone = GameObject.Instantiate(original);
            clone.name = original.name;

            return clone;
        }

        private class VerifyResult
        {
            public bool IsSuccessful
            {
                get
                {
                    return this.ErrorCount == 0;
                }
            }

            public int ErrorCount
            {
                get
                {
                    return this.Errors.Count;
                }
            }

            public List<string> Errors;

            public VerifyResult()
            {
                this.Errors = new List<string>();
            }

            public override string ToString()
            {
                if (this.IsSuccessful) return "Texture Atlas Creator found no errors.";

                StringBuilder builder = new StringBuilder();

                if (this.ErrorCount == 1)
                {
                    builder.AppendLine("Texture Atlas Creator found 1 error:");
                }
                else
                {
                    builder.AppendLine(string.Format("Texture Atlas Creator found {0} errors:", this.ErrorCount));
                }

                //builder.AppendLine();

                foreach (string error in this.Errors)
                {
                    builder.AppendLine(error);
                }

                return builder.ToString();
            }
        } 
    }
}