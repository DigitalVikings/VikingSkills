using System;
using UnityEngine;

namespace VikingSkills.UI
{
    /// <summary>
    /// High-resolution procedural texture engine that generates crisp, anti-aliased 32-bit RGBA sprites
    /// at runtime for a sleek modern Dark RPG HUD. Completely self-contained with no external AssetBundles.
    /// </summary>
    public static class UITextureEngine
    {
        private static bool _initialized;

        public static Sprite HeaderBanner { get; private set; } = null!;
        public static Sprite TabPlaque { get; private set; } = null!;
        public static Sprite GlassPanel { get; private set; } = null!;
        public static Sprite GlassCard { get; private set; } = null!;
        public static Sprite SoftShadow { get; private set; } = null!;
        public static Sprite SocketRing { get; private set; } = null!;
        public static Sprite SocketInnerEmpty { get; private set; } = null!;
        public static Sprite RadialGlow { get; private set; } = null!;
        public static Sprite ConduitTrack { get; private set; } = null!;
        public static Sprite ConduitFill { get; private set; } = null!;
        public static Sprite NodeCircle { get; private set; } = null!;
        public static Sprite NodeDecade { get; private set; } = null!;
        public static Sprite CircleMask { get; private set; } = null!;
        public static Sprite RunicReturnArrow { get; private set; } = null!;
        public static Sprite WhitePixel { get; private set; } = null!;

        public static void EnsureInitialized()
        {
            if (_initialized) return;
            _initialized = true;

            WhitePixel = CreateWhitePixel();
            // Seasoned Nordic Timber Panel (Warm peat/wood tone with weathered bronze border)
            GlassPanel = CreateRoundedGlassPanel(256, 22f, 1.5f, new Color(0.12f, 0.09f, 0.07f, 0.98f), new Color(0.42f, 0.32f, 0.20f, 0.60f));
            // Seasoned Nordic Timber Card (For skill cards, columns, shelf panels)
            GlassCard = CreateRoundedGlassPanel(256, 16f, 1.5f, new Color(0.14f, 0.11f, 0.08f, 0.96f), new Color(0.40f, 0.30f, 0.20f, 0.55f));
            SoftShadow = CreateSoftShadow(128, 20f, 18f);
            // Norse Forged Bronze Ring
            SocketRing = CreateSocketRing(160, 14f, new Color(0.82f, 0.66f, 0.38f, 0.92f), new Color(0.12f, 0.09f, 0.07f, 0.98f));
            SocketInnerEmpty = CreateSocketEmptyReceptacle(160);
            RadialGlow = CreateRadialGlow(160);
            // Carved Timber Groove track
            ConduitTrack = CreateCapsuleTrack(32, 128, new Color(0.08f, 0.06f, 0.05f, 0.95f), new Color(0.26f, 0.20f, 0.13f, 0.80f));
            // Glowing Norse Amber-Gold conduit fill
            ConduitFill = CreateCapsuleTrack(32, 128, new Color(0.96f, 0.68f, 0.20f, 1.00f), new Color(1.00f, 0.88f, 0.45f, 1.00f));
            NodeCircle = CreateCircle(64, true, 2f);
            NodeDecade = CreateDecadeStarNode(80);
            CircleMask = CreateSolidCircle(128);
            RunicReturnArrow = CreateProceduralRunicReturnArrow(128);
            // Flush Top Header Banner with Nordic timber gradient and 3px forged bronze bottom trim
            HeaderBanner = CreateHeaderBanner(256, 128, new Color(0.12f, 0.09f, 0.07f, 0.98f), new Color(0.15f, 0.11f, 0.08f, 0.98f), new Color(0.48f, 0.36f, 0.22f, 0.90f), 3);
            // Sleek Norse Tab Plaque
            TabPlaque = CreateRoundedGlassPanel(128, 6f, 1.2f, new Color(0.18f, 0.13f, 0.10f, 0.92f), new Color(0.40f, 0.30f, 0.20f, 0.50f));
        }

        private static Sprite CreateWhitePixel()
        {
            var tex = new Texture2D(2, 2, TextureFormat.RGBA32, false);
            var px = new[] { Color.white, Color.white, Color.white, Color.white };
            tex.SetPixels(px);
            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, 2, 2), new Vector2(0.5f, 0.5f));
        }

        /// <summary>
        /// Creates a 9-sliced anti-aliased rounded rectangle with subtle top-to-bottom glass sheen and crisp border.
        /// </summary>
        private static Sprite CreateRoundedGlassPanel(int size, float cornerRadius, float borderWidth, Color fillColor, Color borderColor)
        {
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Bilinear;
            float borderInset = borderWidth;

            for (int y = 0; y < size; y++)
            {
                float ny = (float)y / size; // 0 (bottom) to 1 (top)
                // Subtle top-down glass gradient sheen
                float sheen = Mathf.Lerp(0.88f, 1.15f, ny);

                for (int x = 0; x < size; x++)
                {
                    float dist = DistanceToRoundedRect(x + 0.5f, y + 0.5f, size, size, cornerRadius);

                    if (dist > 0.5f)
                    {
                        tex.SetPixel(x, y, Color.clear);
                    }
                    else
                    {
                        // Anti-aliased alpha edge
                        float alphaFactor = Mathf.Clamp01(0.5f - dist);

                        Color c;
                        if (dist > -borderInset)
                        {
                            // On the outer border
                            c = borderColor;
                        }
                        else
                        {
                            // Inner glass body with sheen
                            c = fillColor * sheen;
                            c.a = fillColor.a;
                        }

                        c.a *= alphaFactor;
                        tex.SetPixel(x, y, c);
                    }
                }
            }

            tex.Apply();
            int borderPx = Mathf.CeilToInt(cornerRadius + 4f);
            return Sprite.Create(
                tex,
                new Rect(0, 0, size, size),
                new Vector2(0.5f, 0.5f),
                100f,
                0,
                SpriteMeshType.FullRect,
                new Vector4(borderPx, borderPx, borderPx, borderPx)
            );
        }

        private static float DistanceToRoundedRect(float x, float y, float width, float height, float radius)
        {
            float halfW = width * 0.5f;
            float halfH = height * 0.5f;
            float dx = Mathf.Abs(x - halfW) - (halfW - radius);
            float dy = Mathf.Abs(y - halfH) - (halfH - radius);

            if (dx <= 0f && dy <= 0f)
            {
                return Mathf.Max(dx, dy) - radius;
            }
            if (dx > 0f && dy <= 0f) return dx - radius;
            if (dx <= 0f && dy > 0f) return dy - radius;

            return Mathf.Sqrt(dx * dx + dy * dy) - radius;
        }

        /// <summary>
        /// Creates a 9-sliced soft gaussian drop-shadow sprite.
        /// </summary>
        private static Sprite CreateSoftShadow(int size, float cornerRadius, float blurRadius)
        {
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Bilinear;
            float halfSize = size * 0.5f;

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float dist = DistanceToRoundedRect(x + 0.5f, y + 0.5f, size, size, cornerRadius);
                    float alpha = Mathf.Clamp01(Mathf.Exp(-Mathf.Max(0f, dist) / (blurRadius * 0.45f))) * 0.70f;
                    tex.SetPixel(x, y, new Color(0f, 0f, 0f, alpha));
                }
            }

            tex.Apply();
            int borderPx = Mathf.CeilToInt(cornerRadius + blurRadius);
            return Sprite.Create(
                tex,
                new Rect(0, 0, size, size),
                new Vector2(0.5f, 0.5f),
                100f,
                0,
                SpriteMeshType.FullRect,
                new Vector4(borderPx, borderPx, borderPx, borderPx)
            );
        }

        /// <summary>
        /// Creates a metallic circular socket ring with anti-aliasing and bevel.
        /// </summary>
        private static Sprite CreateSocketRing(int size, float ringThickness, Color ringColor, Color innerFill)
        {
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Bilinear;
            float center = size * 0.5f;
            float outerR = center - 1.5f;
            float innerR = outerR - ringThickness;

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float dx = x + 0.5f - center;
                    float dy = y + 0.5f - center;
                    float dist = Mathf.Sqrt(dx * dx + dy * dy);

                    if (dist > outerR + 0.5f)
                    {
                        tex.SetPixel(x, y, Color.clear);
                    }
                    else if (dist >= outerR - 0.5f)
                    {
                        float alpha = Mathf.Clamp01(outerR + 0.5f - dist);
                        Color c = ringColor;
                        c.a *= alpha;
                        tex.SetPixel(x, y, c);
                    }
                    else if (dist > innerR)
                    {
                        // Metallic bevel gradient across ring
                        float ringT = (dist - innerR) / ringThickness;
                        float bevel = Mathf.Sin(ringT * Mathf.PI);
                        Color c = Color.Lerp(ringColor * 0.75f, ringColor * 1.25f, bevel);
                        tex.SetPixel(x, y, c);
                    }
                    else if (dist >= innerR - 1.0f)
                    {
                        float blend = Mathf.Clamp01(innerR - dist);
                        Color c = Color.Lerp(ringColor * 0.6f, innerFill, blend);
                        tex.SetPixel(x, y, c);
                    }
                    else
                    {
                        // Inner receptacle floor with subtle radial vignette
                        float innerT = dist / innerR;
                        Color c = Color.Lerp(innerFill * 0.70f, innerFill, innerT);
                        tex.SetPixel(x, y, c);
                    }
                }
            }

            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f));
        }

        private static Sprite CreateSocketEmptyReceptacle(int size)
        {
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Bilinear;
            float center = size * 0.5f;
            float r = center - 2f;

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float dx = x + 0.5f - center;
                    float dy = y + 0.5f - center;
                    float dist = Mathf.Sqrt(dx * dx + dy * dy);

                    if (dist > r + 0.5f)
                    {
                        tex.SetPixel(x, y, Color.clear);
                    }
                    else
                    {
                        float edgeA = Mathf.Clamp01(r + 0.5f - dist);
                        // Warm rune groove glow in dark peat receptacle
                        float norm = dist / r;
                        float core = Mathf.Pow(norm, 2.5f);
                        Color c = new Color(0.24f, 0.17f, 0.10f, (0.15f + core * 0.55f) * edgeA);
                        tex.SetPixel(x, y, c);
                    }
                }
            }

            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f));
        }

        /// <summary>
        /// Creates a smooth radial glow sprite.
        /// </summary>
        public static Sprite CreateRadialGlow(int size)
        {
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Bilinear;
            float center = size * 0.5f;
            float maxR = center;

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float dx = x + 0.5f - center;
                    float dy = y + 0.5f - center;
                    float dist = Mathf.Sqrt(dx * dx + dy * dy);
                    float t = Mathf.Clamp01(dist / maxR);

                    // Smoothstep cubic falloff
                    float alpha = (1f - t) * (1f - t) * (1f - t);
                    tex.SetPixel(x, y, new Color(1f, 1f, 1f, alpha));
                }
            }

            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f));
        }

        /// <summary>
        /// Creates a rounded capsule conduit sprite (vertical 9-sliceable).
        /// </summary>
        private static Sprite CreateCapsuleTrack(int width, int height, Color coreColor, Color borderColor)
        {
            var tex = new Texture2D(width, height, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Bilinear;
            float radius = width * 0.5f - 1f;

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    float dist = DistanceToCapsule(x + 0.5f, y + 0.5f, width, height, radius);
                    if (dist > 0.5f)
                    {
                        tex.SetPixel(x, y, Color.clear);
                    }
                    else
                    {
                        float alpha = Mathf.Clamp01(0.5f - dist);
                        Color c = dist > -1.5f ? borderColor : coreColor;
                        c.a *= alpha;
                        tex.SetPixel(x, y, c);
                    }
                }
            }

            tex.Apply();
            int b = Mathf.CeilToInt(radius + 2f);
            return Sprite.Create(
                tex,
                new Rect(0, 0, width, height),
                new Vector2(0.5f, 0.5f),
                100f,
                0,
                SpriteMeshType.FullRect,
                new Vector4(b, b, b, b)
            );
        }

        /// <summary>
        /// Creates a 9-sliced header banner sprite with flat/flush edges on top and sides,
        /// a rich Nordic seasoned timber vertical gradient, and a forged bronze trim border along the bottom edge.
        /// </summary>
        private static Sprite CreateHeaderBanner(int width, int height, Color topColor, Color bottomColor, Color bottomBorderColor, int borderWidth)
        {
            var tex = new Texture2D(width, height, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Bilinear;

            for (int y = 0; y < height; y++)
            {
                Color c;
                if (y < borderWidth)
                {
                    // Bottom forged bronze trim
                    float edge = (float)y / Mathf.Max(1, borderWidth);
                    c = Color.Lerp(bottomBorderColor * 0.85f, bottomBorderColor, edge);
                }
                else
                {
                    // Timber gradient from bottomColor to topColor
                    float t = (float)(y - borderWidth) / Mathf.Max(1, height - borderWidth - 1);
                    c = Color.Lerp(bottomColor, topColor, t);
                }

                for (int x = 0; x < width; x++)
                {
                    tex.SetPixel(x, y, c);
                }
            }

            tex.Apply();

            // 9-slice: left, bottom, right, top
            // Left=16, Bottom=borderWidth+4, Right=16, Top=4 (flat flush top!)
            return Sprite.Create(
                tex,
                new Rect(0, 0, width, height),
                new Vector2(0.5f, 0.5f),
                100f,
                0,
                SpriteMeshType.FullRect,
                new Vector4(16f, borderWidth + 4f, 16f, 4f)
            );
        }

        private static float DistanceToCapsule(float x, float y, float width, float height, float radius)
        {
            float halfW = width * 0.5f;
            float topY = height - radius;
            float botY = radius;

            float clampedY = Mathf.Clamp(y, botY, topY);
            float dx = x - halfW;
            float dy = y - clampedY;
            return Mathf.Sqrt(dx * dx + dy * dy) - radius;
        }

        /// <summary>
        /// Creates an anti-aliased circular node.
        /// </summary>
        private static Sprite CreateCircle(int size, bool hasBorder, float borderWidth)
        {
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Bilinear;
            float center = size * 0.5f;
            float r = center - 1.5f;
            float innerR = r - borderWidth;

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float dx = x + 0.5f - center;
                    float dy = y + 0.5f - center;
                    float dist = Mathf.Sqrt(dx * dx + dy * dy);

                    if (dist > r + 0.5f)
                    {
                        tex.SetPixel(x, y, Color.clear);
                    }
                    else
                    {
                        float alpha = Mathf.Clamp01(r + 0.5f - dist);
                        Color c = (hasBorder && dist > innerR)
                            ? new Color(0.85f, 0.90f, 1.0f, 0.95f)
                            : new Color(0.12f, 0.16f, 0.24f, 0.95f);
                        c.a *= alpha;
                        tex.SetPixel(x, y, c);
                    }
                }
            }

            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f));
        }

        /// <summary>
        /// Creates an ornamented diamond/star node for decade milestones (10, 30, 50, 70, 90).
        /// </summary>
        private static Sprite CreateDecadeStarNode(int size)
        {
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Bilinear;
            float center = size * 0.5f;
            float maxR = center - 2f;

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float dx = Mathf.Abs(x + 0.5f - center);
                    float dy = Mathf.Abs(y + 0.5f - center);

                    // Diamond distance metric
                    float diamondDist = (dx + dy) - maxR;

                    if (diamondDist > 0.5f)
                    {
                        tex.SetPixel(x, y, Color.clear);
                    }
                    else
                    {
                        float alpha = Mathf.Clamp01(0.5f - diamondDist);
                        Color c = diamondDist > -2.5f
                            ? new Color(0.95f, 0.82f, 0.40f, 0.95f) // Gold rim
                            : new Color(0.16f, 0.18f, 0.25f, 0.95f); // Dark core
                        c.a *= alpha;
                        tex.SetPixel(x, y, c);
                    }
                }
            }

            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f));
        }

        /// <summary>
        /// Creates a pure solid white circle sprite with anti-aliased edge falloff for UI masks.
        /// </summary>
        private static Sprite CreateSolidCircle(int size)
        {
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Bilinear;
            float center = size * 0.5f;
            float r = center - 1.5f;

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float dx = x + 0.5f - center;
                    float dy = y + 0.5f - center;
                    float dist = Mathf.Sqrt(dx * dx + dy * dy);

                    if (dist > r + 0.5f)
                    {
                        tex.SetPixel(x, y, Color.clear);
                    }
                    else
                    {
                        float alpha = Mathf.Clamp01(r + 0.5f - dist);
                        tex.SetPixel(x, y, new Color(1f, 1f, 1f, alpha));
                    }
                }
            }

            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f));
        }

        /// <summary>
        /// Creates a high-res anti-aliased procedural runic return arrow medallion.
        /// </summary>
        private static Sprite CreateProceduralRunicReturnArrow(int size)
        {
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Bilinear;
            float center = size * 0.5f;
            float maxR = center - 2f;
            float innerR = maxR - 4f;

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float dx = x + 0.5f - center;
                    float dy = y + 0.5f - center;
                    float dist = Mathf.Sqrt(dx * dx + dy * dy);

                    if (dist > maxR + 0.5f)
                    {
                        tex.SetPixel(x, y, Color.clear);
                        continue;
                    }

                    // Outer circular metallic bezel with anti-aliasing
                    float outerAlpha = Mathf.Clamp01(maxR + 0.5f - dist);
                    Color pixelColor;

                    if (dist >= innerR)
                    {
                        // Metallic border
                        pixelColor = new Color(0.70f, 0.75f, 0.85f, 0.95f);
                    }
                    else
                    {
                        // Dark slate stone core
                        pixelColor = new Color(0.08f, 0.10f, 0.14f, 0.95f);

                        // Draw Runic Return Arrow Glyph inside
                        float arrowDist = DistanceToRunicReturnArrow(dx, dy);

                        if (arrowDist <= 1.0f)
                        {
                            // Glowing cyan / rune core
                            float glyphAlpha = Mathf.Clamp01(1.0f - arrowDist);
                            Color cyanRune = arrowDist < -1.5f
                                ? new Color(0.90f, 0.98f, 1.0f, 1.0f)   // White hot core
                                : new Color(0.15f, 0.80f, 1.0f, 0.95f); // Azure glow
                            pixelColor = Color.Lerp(pixelColor, cyanRune, glyphAlpha);
                        }
                    }

                    pixelColor.a *= outerAlpha;
                    tex.SetPixel(x, y, pixelColor);
                }
            }

            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f));
        }

        private static float DistanceToRunicReturnArrow(float x, float y)
        {
            // Point segment distances for runic return arrow:
            // 1. Top arrowhead barb: from (-22, 0) to (-4, 18)
            float d1 = DistToSegment(x, y, -22f, 0f, -4f, 18f) - 3.5f;
            // 2. Bottom arrowhead barb: from (-22, 0) to (-4, -18)
            float d2 = DistToSegment(x, y, -22f, 0f, -4f, -18f) - 3.5f;
            // 3. Arrow horizontal shaft: from (-4, 0) to (14, 0)
            float d3 = DistToSegment(x, y, -4f, 0f, 14f, 0f) - 3.5f;
            // 4. Return hook curve / tail: from (14, 0) to (20, 14)
            float d4 = DistToSegment(x, y, 14f, 0f, 20f, 14f) - 3.2f;
            // 5. Runic knot tick 1: from (6, -7) to (12, 7)
            float d5 = DistToSegment(x, y, 6f, -7f, 12f, 7f) - 2.0f;
            // 6. Runic knot tick 2: from (-1, -7) to (5, 7)
            float d6 = DistToSegment(x, y, -1f, -7f, 5f, 7f) - 2.0f;

            return Mathf.Min(d1, Mathf.Min(d2, Mathf.Min(d3, Mathf.Min(d4, Mathf.Min(d5, d6)))));
        }

        private static float DistToSegment(float px, float py, float ax, float ay, float bx, float by)
        {
            float pax = px - ax, pay = py - ay;
            float bax = bx - ax, bay = by - ay;
            float h = Mathf.Clamp01((pax * bax + pay * bay) / (bax * bax + bay * bay));
            float dx = pax - bax * h;
            float dy = pay - bay * h;
            return Mathf.Sqrt(dx * dx + dy * dy);
        }
    }
}
