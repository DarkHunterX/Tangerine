using HarmonyLib;
using System;
using System.IO;
using System.Collections.Specialized;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using UnityEngine;
using Tangerine.Utils;

namespace Tangerine.Manager
{
    internal class TextureCache : OrderedDictionary
    {
        public void Set(string key, int width, int height, bool isSprite)
        {
            if (!Contains(key))
            {
                // TODO: Change format?
                var texture = new Texture2D(width, height, TextureFormat.RGBA32, false);
                ImageConversion.LoadImage(texture, new Il2CppStructArray<byte>(File.ReadAllBytes(key)), false);

                if (!isSprite)
                {
                    Add(key, texture);
                    LogMessage.LogWarning($"Texture \"{key}\" added to cache", LogMessage.TextureCache);
                }
                else
                {
                    var rect = new Rect(0.0f, 0.0f, texture.width, texture.height);
                    var vect = new Vector2(0.5f, 0.5f);
                    var sprite = Sprite_Create(texture, rect, vect, 100f, 0, SpriteMeshType.Tight, Vector4.zero, false);
                    Add(key, sprite);
                }
                LogMessage.LogWarning($"Sprite \"{key}\" added to cache", LogMessage.TextureCache);
            }
            else
            {
                try
                {
                    if (!isSprite)
                    {
                        var tex = (Texture2D)this[key];
                        var name = tex.name;

                    }
                    else
                    {
                        var spr = (Sprite)this[key];
                        var name = spr.name;
                    }
                }
                // texture or sprite was garbage collected
                catch (Exception)
                {
                    Remove(key);
                    Set(key, width, height, isSprite);
                }
            }
        }

        // String.Format was stripped from the game, so this is an implementation of the unhollowed method
        private static Sprite Sprite_Create(Texture2D texture, Rect rect, Vector2 pivot, float pixelsPerUnit, uint extrude, SpriteMeshType meshType, Vector4 border, bool generateFallbackPhysicsShape)
        {
            bool flag = texture == null;
            Sprite sprite;
            if (flag)
            {
                sprite = null;
            }
            else
            {
                bool flag2 = rect.xMax > (float)texture.width || rect.yMax > (float)texture.height;
                if (flag2)
                {
                    throw new ArgumentException(string.Format("Could not create sprite ({0}, {1}, {2}, {3}) from a {4}x{5} texture.", rect.x, rect.y, rect.width, rect.height, texture.width, texture.height));
                }
                bool flag3 = pixelsPerUnit <= 0f;
                if (flag3)
                {
                    throw new ArgumentException("pixelsPerUnit must be set to a positive non-zero value.");
                }
                sprite = Sprite.CreateSprite(texture, rect, pivot, pixelsPerUnit, extrude, meshType, border, generateFallbackPhysicsShape);
            }
            return sprite;
        }
    }
}