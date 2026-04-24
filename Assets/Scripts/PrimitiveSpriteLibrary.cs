using UnityEngine;

public static class PrimitiveSpriteLibrary
{
    private static Sprite squareSprite;
    private static Texture2D squareTexture;

    public static Sprite SquareSprite
    {
        get
        {
            if (squareSprite == null)
            {
                squareTexture = new Texture2D(16, 16, TextureFormat.RGBA32, false)
                {
                    filterMode = FilterMode.Point,
                    wrapMode = TextureWrapMode.Clamp,
                    name = "RuntimeSquareTexture"
                };

                Color[] pixels = new Color[16 * 16];
                for (int i = 0; i < pixels.Length; i++)
                {
                    pixels[i] = Color.white;
                }

                squareTexture.SetPixels(pixels);
                squareTexture.Apply();

                squareSprite = Sprite.Create(
                    squareTexture,
                    new Rect(0f, 0f, squareTexture.width, squareTexture.height),
                    new Vector2(0.5f, 0.5f),
                    16f);
                squareSprite.name = "RuntimeSquare";
            }

            return squareSprite;
        }
    }
}
