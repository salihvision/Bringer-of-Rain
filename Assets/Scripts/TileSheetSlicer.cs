using System.Collections.Generic;
using UnityEngine;

public static class TileSheetSlicer
{
    private static readonly Dictionary<(string, int, int, int, int), Sprite> Cache = new();

    public static Sprite GetTile(string resourcePath, int column, int row, int tileWidth = 32, int tileHeight = 32, float pixelsPerUnit = 32f)
    {
        var key = (resourcePath, column, row, tileWidth, tileHeight);
        if (Cache.TryGetValue(key, out Sprite cached))
        {
            return cached;
        }

        Texture2D texture = Resources.Load<Texture2D>(resourcePath);
        if (texture == null)
        {
            return null;
        }

        texture.filterMode = FilterMode.Point;
        int yOriginFromBottom = texture.height - (row + 1) * tileHeight;
        Rect rect = new(column * tileWidth, yOriginFromBottom, tileWidth, tileHeight);
        Sprite sprite = Sprite.Create(texture, rect, new Vector2(0.5f, 0.5f), pixelsPerUnit, 0, SpriteMeshType.FullRect);
        sprite.name = $"{resourcePath}_{column}_{row}";
        Cache[key] = sprite;
        return sprite;
    }
}
