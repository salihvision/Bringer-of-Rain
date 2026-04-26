#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System.IO;
using System.Text;
using System.Globalization;

public class BlockoutExporter : EditorWindow
{
    [MenuItem("Tools/Export Current Blockout to Script")]
    public static void ExportBlockout()
    {
        GameObject root = GameObject.Find("LevelBlockout");
        if (root == null)
        {
            Debug.LogError("Could not find a GameObject named 'LevelBlockout' in the scene. Make sure your blockout is under a parent object named exactly 'LevelBlockout'.");
            return;
        }

        StringBuilder sb = new StringBuilder();
        sb.AppendLine("#if UNITY_EDITOR");
        sb.AppendLine("using UnityEditor;");
        sb.AppendLine("using UnityEngine;");
        sb.AppendLine("");
        sb.AppendLine("public class LevelBlockoutGenerator : EditorWindow");
        sb.AppendLine("{");
        sb.AppendLine("    [MenuItem(\"Tools/Generate Level Blockout\")]");
        sb.AppendLine("    public static void GenerateBlockout()");
        sb.AppendLine("    {");
        sb.AppendLine("        GameObject root = new GameObject(\"LevelBlockout\");");
        sb.AppendLine("");

        foreach (Transform child in root.transform)
        {
            SpriteRenderer sr = child.GetComponent<SpriteRenderer>();
            PlatformEffector2D effector = child.GetComponent<PlatformEffector2D>();
            BreakableDoor door = child.GetComponent<BreakableDoor>();

            if (sr == null) continue;

            string name = child.name;
            float px = child.position.x;
            float py = child.position.y;
            float sx = sr.size.x;
            float sy = sr.size.y;
            Color c = sr.color;

            string posStr = $"new Vector2({px.ToString("0.###", CultureInfo.InvariantCulture)}f, {py.ToString("0.###", CultureInfo.InvariantCulture)}f)";
            string sizeStr = $"new Vector2({sx.ToString("0.###", CultureInfo.InvariantCulture)}f, {sy.ToString("0.###", CultureInfo.InvariantCulture)}f)";
            string colorStr = $"new Color({c.r.ToString("0.###", CultureInfo.InvariantCulture)}f, {c.g.ToString("0.###", CultureInfo.InvariantCulture)}f, {c.b.ToString("0.###", CultureInfo.InvariantCulture)}f)";

            if (door != null)
            {
                sb.AppendLine($"        CreateDoor(root, \"{name}\", {posStr}, {sizeStr}, {colorStr});");
            }
            else if (effector != null)
            {
                sb.AppendLine($"        CreateOneWayPlatform(root, \"{name}\", {posStr}, {sizeStr}, {colorStr});");
            }
            else
            {
                sb.AppendLine($"        CreatePlatform(root, \"{name}\", {posStr}, {sizeStr}, {colorStr});");
            }
        }

        sb.AppendLine("");
        sb.AppendLine("        Selection.activeGameObject = root;");
        sb.AppendLine("        Debug.Log(\"Level Blockout Generated!\");");
        sb.AppendLine("    }");
        sb.AppendLine("");
        sb.AppendLine("    private static GameObject CreatePlatform(GameObject parent, string name, Vector2 position, Vector2 size, Color? color = null)");
        sb.AppendLine("    {");
        sb.AppendLine("        GameObject go = new GameObject(name);");
        sb.AppendLine("        go.transform.SetParent(parent.transform);");
        sb.AppendLine("        go.transform.position = position;");
        sb.AppendLine("        SpriteRenderer sr = go.AddComponent<SpriteRenderer>();");
        sb.AppendLine("        sr.sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>(\"UI/Skin/Background.psd\");");
        sb.AppendLine("        sr.color = color ?? new Color(0.2f, 0.2f, 0.2f);");
        sb.AppendLine("        sr.drawMode = SpriteDrawMode.Sliced;");
        sb.AppendLine("        sr.size = size;");
        sb.AppendLine("        BoxCollider2D bc = go.AddComponent<BoxCollider2D>();");
        sb.AppendLine("        bc.size = size;");
        sb.AppendLine("        int groundLayer = LayerMask.NameToLayer(\"Ground\");");
        sb.AppendLine("        if (groundLayer != -1) go.layer = groundLayer;");
        sb.AppendLine("        return go;");
        sb.AppendLine("    }");
        sb.AppendLine("");
        sb.AppendLine("    private static GameObject CreateOneWayPlatform(GameObject parent, string name, Vector2 position, Vector2 size, Color? color = null)");
        sb.AppendLine("    {");
        sb.AppendLine("        GameObject go = CreatePlatform(parent, name, position, size, color);");
        sb.AppendLine("        BoxCollider2D bc = go.GetComponent<BoxCollider2D>();");
        sb.AppendLine("        bc.usedByEffector = true;");
        sb.AppendLine("        PlatformEffector2D effector = go.AddComponent<PlatformEffector2D>();");
        sb.AppendLine("        effector.useOneWay = true;");
        sb.AppendLine("        effector.useOneWayGrouping = true;");
        sb.AppendLine("        effector.surfaceArc = 170f;");
        sb.AppendLine("        effector.sideArc = 0f;");
        sb.AppendLine("        return go;");
        sb.AppendLine("    }");
        sb.AppendLine("");
        sb.AppendLine("    private static void CreateDoor(GameObject parent, string name, Vector2 position, Vector2 size, Color? color = null)");
        sb.AppendLine("    {");
        sb.AppendLine("        GameObject go = new GameObject(name);");
        sb.AppendLine("        go.transform.SetParent(parent.transform);");
        sb.AppendLine("        go.transform.position = position;");
        sb.AppendLine("        SpriteRenderer sr = go.AddComponent<SpriteRenderer>();");
        sb.AppendLine("        sr.sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>(\"UI/Skin/Background.psd\");");
        sb.AppendLine("        sr.color = color ?? new Color(0.6f, 0.2f, 0.2f);");
        sb.AppendLine("        sr.drawMode = SpriteDrawMode.Sliced;");
        sb.AppendLine("        sr.size = size;");
        sb.AppendLine("        BoxCollider2D bc = go.AddComponent<BoxCollider2D>();");
        sb.AppendLine("        bc.size = size;");
        sb.AppendLine("        go.AddComponent<BreakableDoor>();");
        sb.AppendLine("        int interactLayer = LayerMask.NameToLayer(\"BurstTarget\");");
        sb.AppendLine("        if (interactLayer != -1) go.layer = interactLayer;");
        sb.AppendLine("        else { interactLayer = LayerMask.NameToLayer(\"Enemies\"); if (interactLayer != -1) go.layer = interactLayer; }");
        sb.AppendLine("    }");
        sb.AppendLine("}");
        sb.AppendLine("#endif");

        string scriptPath = Path.Combine(Application.dataPath, "Editor/LevelBlockoutGenerator.cs");
        File.WriteAllText(scriptPath, sb.ToString());
        AssetDatabase.Refresh();

        Debug.Log($"Successfully updated LevelBlockoutGenerator.cs with your current scene layout!");
    }
}
#endif
