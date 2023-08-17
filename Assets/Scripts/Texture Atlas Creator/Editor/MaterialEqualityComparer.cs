using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace TotalDistraction.TextureAtlasCreator
{
    public class MaterialEqualityComparer : IEqualityComparer<Material>
    {
        private string[] _exclude;

        public MaterialEqualityComparer(params string[] exclude)
        {
            _exclude = exclude;
        }

        public bool Equals(Material x, Material y)
        {
            if (x == y) return true;

            if (x.shader != y.shader) return false;

            // check that both materials have the same shader features
            if (!x.shaderKeywords.OrderBy(key => key).SequenceEqual(y.shaderKeywords.OrderBy(key => key))) return false;

            ShaderProperty[] xProperties = ShaderProperty.GetPropertiesFromShader(x.shader);
            ShaderProperty[] yProperties = ShaderProperty.GetPropertiesFromShader(y.shader);

            if (!xProperties.SequenceEqual(yProperties)) return false;

            for (int i = 0; i < xProperties.Length; i++)
            {
                ShaderProperty property = xProperties[i];

                if (_exclude.Contains(property.Name)) continue;

                switch (property.PropertyType)
                {
                    case ShaderUtil.ShaderPropertyType.Color:
                        Color xColor = property.GetColor(x);
                        Color yColor = property.GetColor(y);

                        if (xColor != yColor) return false;

                        break;

                    case ShaderUtil.ShaderPropertyType.Float:
                    case ShaderUtil.ShaderPropertyType.Range:
                        float xFloat = property.GetFloat(x);
                        float yFloat = property.GetFloat(y);

                        if (xFloat != yFloat) return false;

                        break;

                    case ShaderUtil.ShaderPropertyType.TexEnv:
                        Texture xTexture = property.GetTexture(x);
                        Texture yTexture = property.GetTexture(y);

                        Vector2 xScale = property.GetTextureScale(x);
                        Vector2 yScale = property.GetTextureScale(y);

                        Vector2 xOffset = property.GetTextureOffset(x);
                        Vector2 yOffset = property.GetTextureOffset(y);

                        if (xTexture != yTexture || xScale != yScale || xOffset != yOffset) return false;

                        break;

                    case ShaderUtil.ShaderPropertyType.Vector:
                        Vector4 xVector = property.GetVector(x);
                        Vector4 yVector = property.GetVector(y);

                        if (xVector != yVector) return false;

                        break;
                }
            }

            return true;
        }



        public int GetHashCode(Material obj)
        {
            return obj.GetInstanceID();
        }

        private class ShaderProperty : IEquatable<ShaderProperty>
        {
            public Shader Shader;
            public int Index;
            public string Name;
            public string Description;
            public float RangeDefault;
            public float RangeMin;
            public float RangeMax;
            public ShaderUtil.ShaderPropertyType PropertyType;
            public TextureDimension TextureDimension;

            public static ShaderProperty[] GetPropertiesFromShader(Shader shader)
            {
                int propertyCount = ShaderUtil.GetPropertyCount(shader);

                ShaderProperty[] properties = new ShaderProperty[propertyCount];
                
                for (int i = 0; i < propertyCount; i++)
                {
                    string name = ShaderUtil.GetPropertyName(shader, i);
                    string description = ShaderUtil.GetPropertyDescription(shader, i);
                    float rangeDefault = ShaderUtil.GetRangeLimits(shader, i, 0);
                    float rangeMin = ShaderUtil.GetRangeLimits(shader, i, 1);
                    float rangeMax = ShaderUtil.GetRangeLimits(shader, i, 2);
                    ShaderUtil.ShaderPropertyType type = ShaderUtil.GetPropertyType(shader, i);
                    TextureDimension dimension = ShaderUtil.GetTexDim(shader, i);

                    properties[i] = new ShaderProperty
                    {
                        Shader = shader,
                        Index = i,
                        Name = name,
                        Description = description,
                        RangeDefault = rangeDefault,
                        RangeMin = rangeMin,
                        RangeMax = rangeMax,
                        PropertyType = type,
                        TextureDimension = dimension,
                    };
                }

                return properties;
            }

            public Color GetColor(Material material)
            {
                if (this.Shader != material.shader) throw new ArgumentException("The given material does not use the shader this shader property belongs to.");
                if (this.PropertyType != ShaderUtil.ShaderPropertyType.Color) throw new InvalidOperationException();
                
                return material.GetColor(this.Name);
            }

            public float GetFloat(Material material)
            {
                if (this.Shader != material.shader) throw new ArgumentException("The given material does not use the shader this shader property belongs to.");
                if (this.PropertyType != ShaderUtil.ShaderPropertyType.Float && this.PropertyType != ShaderUtil.ShaderPropertyType.Range) throw new InvalidOperationException();
                
                return material.GetFloat(this.Name);
            }

            public Texture GetTexture(Material material)
            {
                if (this.Shader != material.shader) throw new ArgumentException("The given material does not use the shader this shader property belongs to.");
                if (this.PropertyType != ShaderUtil.ShaderPropertyType.TexEnv) throw new InvalidOperationException();
                
                return material.GetTexture(this.Name);
            }

            public Vector2 GetTextureOffset(Material material)
            {
                if (this.Shader != material.shader) throw new ArgumentException("The given material does not use the shader this shader property belongs to.");
                if (this.PropertyType != ShaderUtil.ShaderPropertyType.TexEnv) throw new InvalidOperationException();

                return material.GetTextureOffset(this.Name);
            }

            public Vector2 GetTextureScale(Material material)
            {
                if (this.Shader != material.shader) throw new ArgumentException("The given material does not use the shader this shader property belongs to.");
                if (this.PropertyType != ShaderUtil.ShaderPropertyType.TexEnv) throw new InvalidOperationException();

                return material.GetTextureScale(this.Name);
            }

            public Vector4 GetVector(Material material)
            {
                if (this.Shader != material.shader) throw new ArgumentException("The given material does not use the shader this shader property belongs to.");
                if (this.PropertyType != ShaderUtil.ShaderPropertyType.Vector) throw new InvalidOperationException();

                return material.GetVector(this.Name);
            }

            private bool IsEqual(ShaderProperty other)
            {
                return this.Shader == other.Shader
                    && int.Equals(this.Index, other.Index)
                    && string.Equals(this.Name, other.Name)
                    && string.Equals(this.Description, other.Description)
                    && float.Equals(this.RangeDefault, other.RangeDefault)
                    && float.Equals(this.RangeMin, other.RangeMin)
                    && float.Equals(this.RangeMax, this.RangeMax)
                    && System.Object.Equals(this.PropertyType, other.PropertyType)
                    && System.Object.Equals(this.TextureDimension, other.TextureDimension);
            }

            public bool Equals(ShaderProperty other)
            {
                if (ReferenceEquals(null, other)) return false;
                if (ReferenceEquals(this, other)) return true;

                return IsEqual(other);
            }

            public override bool Equals(object obj)
            {
                if (ReferenceEquals(null, obj)) return false;
                if (ReferenceEquals(this, obj)) return true;
                if (obj.GetType() != this.GetType()) return false;

                return IsEqual((ShaderProperty)obj);
            }

            public override int GetHashCode()
            {
                unchecked
                {
                    const int hashBase = (int)2166136261;
                    const int hashMultiplier = 16777219;
                    int hash = hashBase;

                    hash = (hash * hashMultiplier) ^ this.Shader.GetInstanceID();
                    hash = (hash * hashMultiplier) ^ this.Index.GetHashCode();
                    hash = (hash * hashMultiplier) ^ (ReferenceEquals(null, this.Name) ? 0 : this.Name.GetHashCode());
                    hash = (hash * hashMultiplier) ^ (ReferenceEquals(null, this.Description) ? 0 : this.Description.GetHashCode());
                    hash = (hash * hashMultiplier) ^ this.RangeDefault.GetHashCode();
                    hash = (hash * hashMultiplier) ^ this.RangeMin.GetHashCode();
                    hash = (hash * hashMultiplier) ^ this.RangeMax.GetHashCode();
                    hash = (hash * hashMultiplier) ^ this.PropertyType.GetHashCode();
                    hash = (hash * hashMultiplier) ^ this.TextureDimension.GetHashCode();

                    return hash;
                }
            }

            public static bool operator ==(ShaderProperty a, ShaderProperty b)
            {
                if (ReferenceEquals(a, b)) return true;
                if (ReferenceEquals(null, a)) return false;

                return a.Equals(b);
            }

            public static bool operator !=(ShaderProperty a, ShaderProperty b)
            {
                return !(a == b);
            }

            public override string ToString()
            {
                return string.Format("ShaderProperty \"{0}\" ({1})", this.Name, this.PropertyType);
            }
        }
    }
}