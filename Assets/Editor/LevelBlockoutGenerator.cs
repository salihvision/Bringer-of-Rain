#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

public class LevelBlockoutGenerator : EditorWindow
{
    [MenuItem("Tools/Generate Level Blockout")]
    public static void GenerateBlockout()
    {
        GameObject root = new GameObject("LevelBlockout");

        CreatePlatform(root, "Start_RightWall", new Vector2(2f, -6f), new Vector2(1f, 13f), new Color(0.2f, 0.2f, 0.2f));
        CreatePlatform(root, "Start_LeftWall", new Vector2(-2f, -3.76f), new Vector2(1f, 11f), new Color(0.2f, 0.2f, 0.2f));
        CreatePlatform(root, "Corr1_Ceiling", new Vector2(-6f, -8.83f), new Vector2(9f, 1f), new Color(0.2f, 0.2f, 0.2f));
        CreatePlatform(root, "Central_Ceiling", new Vector2(-18f, -4.66f), new Vector2(17f, 1f), new Color(0.2f, 0.2f, 0.2f));
        CreatePlatform(root, "Corr1_Pit_Platform", new Vector2(-4.95f, -12.5f), new Vector2(3.423f, 0.5f), new Color(0.2f, 0.2f, 0.2f));
        CreatePlatform(root, "Corr1_Floor_Right", new Vector2(0f, -12f), new Vector2(5f, 1f), new Color(0.2f, 0.2f, 0.2f));
        CreatePlatform(root, "Corr1_Pit_RightWall", new Vector2(-2.5f, -13f), new Vector2(1f, 3f), new Color(0.2f, 0.2f, 0.2f));
        CreateOneWayPlatform(root, "Corr1_Pit_Platform", new Vector2(-5f, -13.5f), new Vector2(2f, 0.5f), new Color(0.2f, 0.2f, 0.2f));
        CreatePlatform(root, "Central_RightWall_Upper", new Vector2(-10f, -6.71f), new Vector2(1f, 5f), new Color(0.2f, 0.2f, 0.2f));
        CreatePlatform(root, "Central_LeftWall_Upper", new Vector2(-26f, -7.07f), new Vector2(1f, 5f), new Color(0.2f, 0.2f, 0.2f));
        CreatePlatform(root, "Plat_L1", new Vector2(-22.97f, -14.02f), new Vector2(4f, 0.5f), new Color(0.2f, 0.2f, 0.2f));
        CreatePlatform(root, "Plat_R1", new Vector2(-13.08f, -14.05f), new Vector2(4f, 0.5f), new Color(0.2f, 0.2f, 0.2f));
        CreatePlatform(root, "Plat_L1 (1)", new Vector2(-23.01f, -18.68f), new Vector2(4f, 0.5f), new Color(0.2f, 0.2f, 0.2f));
        CreatePlatform(root, "Plat_L1 (2)", new Vector2(-22.88f, -23.16f), new Vector2(4f, 0.5f), new Color(0.2f, 0.2f, 0.2f));
        CreatePlatform(root, "Plat_R1 (2)", new Vector2(-12.99f, -23.19f), new Vector2(4f, 0.5f), new Color(0.2f, 0.2f, 0.2f));
        CreatePlatform(root, "Plat_L1 (3)", new Vector2(-22.84f, -27.73f), new Vector2(4f, 0.5f), new Color(0.2f, 0.2f, 0.2f));
        CreatePlatform(root, "Plat_R1 (3)", new Vector2(-12.95f, -27.76f), new Vector2(4f, 0.5f), new Color(0.2f, 0.2f, 0.2f));
        CreatePlatform(root, "Plat_L2 (3)", new Vector2(-21f, -29.31f), new Vector2(4f, 0.5f), new Color(0.2f, 0.2f, 0.2f));
        CreatePlatform(root, "Plat_R2 (3)", new Vector2(-15.51f, -29.33f), new Vector2(4f, 0.5f), new Color(0.2f, 0.2f, 0.2f));
        CreatePlatform(root, "Plat_L3 (3)", new Vector2(-17.865f, -26.03f), new Vector2(13.031f, 0.5f), new Color(0.2f, 0.2f, 0.2f));
        CreatePlatform(root, "Plat_L2 (2)", new Vector2(-21.04f, -24.74f), new Vector2(4f, 0.5f), new Color(0.2f, 0.2f, 0.2f));
        CreatePlatform(root, "Plat_R2 (2)", new Vector2(-15.55f, -24.76f), new Vector2(4f, 0.5f), new Color(0.2f, 0.2f, 0.2f));
        CreatePlatform(root, "Plat_L3 (2)", new Vector2(-17.905f, -21.46f), new Vector2(13.031f, 0.5f), new Color(0.2f, 0.2f, 0.2f));
        CreatePlatform(root, "Plat_R1 (1)", new Vector2(-13.12f, -18.71f), new Vector2(4f, 0.5f), new Color(0.2f, 0.2f, 0.2f));
        CreatePlatform(root, "Plat_L3 (1)", new Vector2(-17.995f, -12.32f), new Vector2(13.031f, 0.5f), new Color(0.2f, 0.2f, 0.2f));
        CreatePlatform(root, "Corr1_Pit_Floor", new Vector2(-5f, -15f), new Vector2(6f, 1f), new Color(0.2f, 0.2f, 0.2f));
        CreatePlatform(root, "Corr1_Pit_LeftWall", new Vector2(-7.5f, -13f), new Vector2(1f, 3f), new Color(0.2f, 0.2f, 0.2f));
        CreatePlatform(root, "Plat_L2 (1)", new Vector2(-21.17f, -20.26f), new Vector2(4f, 0.5f), new Color(0.2f, 0.2f, 0.2f));
        CreatePlatform(root, "Central_Floor", new Vector2(-18f, -31f), new Vector2(17f, 1f), new Color(0.2f, 0.2f, 0.2f));
        CreatePlatform(root, "Corr1_Floor_Left", new Vector2(-9f, -12f), new Vector2(3f, 1f), new Color(0.2f, 0.2f, 0.2f));
        CreatePlatform(root, "Plat_R2 (1)", new Vector2(-15.68f, -20.28f), new Vector2(4f, 0.5f), new Color(0.2f, 0.2f, 0.2f));
        CreatePlatform(root, "Central_RightWall_Lower", new Vector2(-10f, -21f), new Vector2(1f, 19f), new Color(0.2f, 0.2f, 0.2f));
        CreatePlatform(root, "Central_LeftWall_Lower", new Vector2(-26f, -21f), new Vector2(1f, 19f), new Color(0.2f, 0.2f, 0.2f));
        CreatePlatform(root, "Plat_L3 (1)", new Vector2(-18.035f, -16.98f), new Vector2(13.031f, 0.5f), new Color(0.2f, 0.2f, 0.2f));
        CreatePlatform(root, "Plat_L2", new Vector2(-21.13f, -15.6f), new Vector2(4f, 0.5f), new Color(0.2f, 0.2f, 0.2f));
        CreatePlatform(root, "Plat_R2", new Vector2(-15.64f, -15.62f), new Vector2(4f, 0.5f), new Color(0.2f, 0.2f, 0.2f));
        CreatePlatform(root, "Corr2_Ceiling", new Vector2(-28.5f, -9.22f), new Vector2(6f, 1f), new Color(0.2f, 0.2f, 0.2f));
        CreatePlatform(root, "Corr2_Floor", new Vector2(-38.386f, -12f), new Vector2(25.773f, 1f), new Color(0.2f, 0.2f, 0.2f));
        CreateDoor(root, "BreakableDoor", new Vector2(-28.5f, -10.56f), new Vector2(0.5f, 2f), new Color(0.6f, 0.2f, 0.2f));
        CreatePlatform(root, "FarLeft_Ceiling", new Vector2(-41f, -4.12f), new Vector2(20f, 1f), new Color(0.2f, 0.2f, 0.2f));
        CreatePlatform(root, "FarLeft_RightWall_Upper", new Vector2(-31f, -6.52f), new Vector2(1f, 6f), new Color(0.2f, 0.2f, 0.2f));
        CreatePlatform(root, "FarLeft_LeftWall", new Vector2(-51f, -8.444f), new Vector2(1f, 7.888f), new Color(0.2f, 0.2f, 0.2f));

        Selection.activeGameObject = root;
        Debug.Log("Level Blockout Generated!");
    }

    private static GameObject CreatePlatform(GameObject parent, string name, Vector2 position, Vector2 size, Color? color = null)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent.transform);
        go.transform.position = position;
        SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Background.psd");
        sr.color = color ?? new Color(0.2f, 0.2f, 0.2f);
        sr.drawMode = SpriteDrawMode.Sliced;
        sr.size = size;
        BoxCollider2D bc = go.AddComponent<BoxCollider2D>();
        bc.size = size;
        int groundLayer = LayerMask.NameToLayer("Ground");
        if (groundLayer != -1) go.layer = groundLayer;
        return go;
    }

    private static GameObject CreateOneWayPlatform(GameObject parent, string name, Vector2 position, Vector2 size, Color? color = null)
    {
        GameObject go = CreatePlatform(parent, name, position, size, color);
        BoxCollider2D bc = go.GetComponent<BoxCollider2D>();
        bc.usedByEffector = true;
        PlatformEffector2D effector = go.AddComponent<PlatformEffector2D>();
        effector.useOneWay = true;
        effector.useOneWayGrouping = true;
        effector.surfaceArc = 170f;
        effector.sideArc = 0f;
        return go;
    }

    private static void CreateDoor(GameObject parent, string name, Vector2 position, Vector2 size, Color? color = null)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent.transform);
        go.transform.position = position;
        SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Background.psd");
        sr.color = color ?? new Color(0.6f, 0.2f, 0.2f);
        sr.drawMode = SpriteDrawMode.Sliced;
        sr.size = size;
        BoxCollider2D bc = go.AddComponent<BoxCollider2D>();
        bc.size = size;
        go.AddComponent<BreakableDoor>();
        int interactLayer = LayerMask.NameToLayer("BurstTarget");
        if (interactLayer != -1) go.layer = interactLayer;
        else { interactLayer = LayerMask.NameToLayer("Enemies"); if (interactLayer != -1) go.layer = interactLayer; }
    }
}
#endif
