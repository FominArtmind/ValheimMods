using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using JetBrains.Annotations;
using UnityEngine;

namespace Raido
{
    [RequireComponent(typeof(Minimap))]
    public class MinimapController : MonoBehaviour
    {
        private Minimap _minimap;
        private static Player _player;

        private static bool _drawingOfCirclesRequired = true;

        public virtual void Awake()
        {
            _minimap = GetComponent<Minimap>();
        }

        public virtual void Update()
        {
            if (_drawingOfCirclesRequired)
            {
                EpicLoot.EpicLoot.Log("DRAWING CIRCLES ON THE MAP");
                DrawDistanceLevelCircles();
                _drawingOfCirclesRequired = false;
            }
        }

        public static void Enable(Player player)
        {
            _player = player;
        }

        private void DrawDistanceLevelCircles()
        {
            if (!_minimap)
            {
                EpicLoot.EpicLoot.LogWarning("Minimap pointer is null");
                return;
            }
            if (!_minimap.m_mapTexture || !_minimap.m_forestMaskTexture || !_minimap.m_heightTexture)
            {
                EpicLoot.EpicLoot.LogWarning("Minimap textures are null");
                return;
            }
            if (!ZoneSystem.instance)
            {
                EpicLoot.EpicLoot.LogWarning("Zone instance is null");
                return;
            }
            if (!Game.instance)
            {
                EpicLoot.EpicLoot.LogWarning("Game instance is null");
                return;
            }

            Color[] originalMapColors = _minimap.m_mapTexture.GetPixels();
            Color[] originalForestColors = _minimap.m_forestMaskTexture.GetPixels();
            Color[] originalHeightColors = _minimap.m_heightTexture.GetPixels();

            Color[] mapColors = new Color[originalMapColors.Length];
            Array.Copy(originalMapColors, mapColors, originalMapColors.Length);
            Color[] forestColors = _minimap.m_forestMaskTexture.GetPixels();
            Array.Copy(originalForestColors, forestColors, originalForestColors.Length);
            Color[] heightColors = _minimap.m_heightTexture.GetPixels();
            Array.Copy(originalHeightColors, heightColors, originalHeightColors.Length);

            int[] levels = new int[6] { 500, 2000, 3500, 5000, 6500, 8000 };
            foreach (int item in levels)
            {
                ZoneSystem.instance.GetLocationIcon(Game.instance.m_StartLocation, out var pos);
                Vector2 vector = (float)_minimap.m_textureSize / 2f * Vector2.one + new Vector2(pos.x, pos.z) / _minimap.m_pixelSize;
                float num = (float)item / _minimap.m_pixelSize;
                int num2 = Mathf.CeilToInt(num * 2f * (float)Math.PI);
                int i;
                for (i = 0; i < num2; i++)
                {
                    float num3 = vector.x + num * Mathf.Sin((float)Math.PI * 2f * (float)i / (float)num2);
                    float num4 = vector.y + num * Mathf.Cos((float)Math.PI * 2f * (float)i / (float)num2);
                    int num5 = ((!(num3 % 1f > 0.5f)) ? 1 : (-1));
                    int num6 = ((!(num4 % 1f > 0.5f)) ? 1 : (-1));
                    apply(num3, num4, 1f - Mathf.Abs(0.5f - num3 % 1f) * Mathf.Abs(0.5f - num4 % 1f) * 2f);
                    // apply(num3 + (float)num5, num4, 0.5f - (0.5f - Mathf.Abs(0.5f - num3 % 1f)) * Mathf.Abs(0.5f - num4 % 1f) * 2f);
                    // apply(num3, num4 + (float)num6, 0.5f - (0.5f - Mathf.Abs(0.5f - num4 % 1f)) * Mathf.Abs(0.5f - num3 % 1f) * 2f);
                    // apply(num3 + (float)num5, num4 + (float)num6, 0.5f - (0.5f - Mathf.Abs(0.5f - num3 % 1f)) * (0.5f - Mathf.Abs(0.5f - num4 % 1f)) * 2f);
                }

                void apply(float x, float y, float intensity)
                {
                    intensity = 1.0f;

                    if (!(x < 0f) && Mathf.RoundToInt(x) < _minimap.m_textureSize && !(y < 0f) && Mathf.RoundToInt(y) < _minimap.m_textureSize)
                    {
                        int num7 = Mathf.RoundToInt(y) * _minimap.m_textureSize + Mathf.RoundToInt(x);
                        Color color = Color.white; // ((i % 20 < 10) ? Color.red : Color.blue);
                        mapColors[num7] = Color.white; // ((mapColors[num7] == Color.white) ? color : Color.Lerp(mapColors[num7], color, intensity));
                        if (intensity > 0.2f)
                        {
                            forestColors[num7] = _minimap.noForest;
                            heightColors[num7] = new Color(Mathf.Clamp(heightColors[num7].r, ZoneSystem.instance.m_waterLevel + 4f, 89f), 0f, 0f);
                        }
                    }
                }
            }

            _minimap.m_mapTexture.SetPixels(mapColors);
            _minimap.m_mapTexture.Apply();
            _minimap.m_forestMaskTexture.SetPixels(forestColors);
            _minimap.m_forestMaskTexture.Apply();
            _minimap.m_heightTexture.SetPixels(heightColors);
            _minimap.m_heightTexture.Apply();
        }

    }

    // TO DO : check if this works for CreatureLevelControl map changes to work properly
    [HarmonyPatch(typeof(Minimap))]
    public static class MinimapPatch
    {
        [HarmonyPatch(nameof(Minimap.Awake))]
        [UsedImplicitly]
        public static void Postfix(Minimap __instance)
        {
            __instance.gameObject.AddComponent<MinimapController>();
        }
    }

    [HarmonyPatch(typeof(Player), nameof(Player.SetLocalPlayer))]
    public static class Player_SetLocalPlayer_Patch
    {
        [UsedImplicitly]
        public static void Postfix(Player __instance)
        {
            MinimapController.Enable(__instance);
        }
    }
}
