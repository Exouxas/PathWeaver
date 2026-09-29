using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using VRC.SDKBase.Editor.BuildPipeline;

public class PathWeaverHook : IVRCSDKPreprocessAvatarCallback
{
    // VRCFury runs at -10000. PathWeaver must run before it.
    public int callbackOrder => -20000;

    public bool OnPreprocessAvatar(GameObject avatar)
    {
        foreach (var root in avatar.GetComponentsInChildren<PathWeaver>(true))
            Apply(root, log: false, dryRun: false);
        return true;
    }

    // Right-click the PathWeaver component header to see what a build would do.
    [MenuItem("CONTEXT/PathWeaver/Preview Generated Paths")]
    private static void Preview(MenuCommand cmd) => Apply((PathWeaver)cmd.context, log: true, dryRun: true);

    // Lists every unique VRCFury feature type under the root and its serialized fields.
    [MenuItem("CONTEXT/PathWeaver/Dump Feature Fields")]
    private static void Dump(MenuCommand cmd)
    {
        var root = (PathWeaver)cmd.context;
        var seen = new Dictionary<string, SerializedProperty>();
        var otherTypes = new HashSet<string>();

        foreach (var comp in root.GetComponentsInChildren<Component>(true))
        {
            if (comp == null) continue;
            var typeName = comp.GetType().FullName;
            if (typeName != "VF.Model.VRCFury") { otherTypes.Add(typeName); continue; }
            var content = new SerializedObject(comp).FindProperty("content");
            if (content == null) continue;
            var t = content.managedReferenceFullTypename;
            if (!seen.ContainsKey(t)) seen[t] = content.Copy();
        }

        var sb = new System.Text.StringBuilder();
        sb.AppendLine("[PathWeaver] Other component types under root:\n  " + string.Join("\n  ", otherTypes.OrderBy(x => x)));
        foreach (var kv in seen.OrderBy(k => k.Key))
        {
            sb.AppendLine($"\nFeature: {kv.Key}");
            DumpFields(kv.Value, sb, "  ", 0);
        }
        Debug.Log(sb.ToString(), root);
    }

    private static void DumpFields(SerializedProperty parent, System.Text.StringBuilder sb, string indent, int depth)
    {
        if (depth > 3) return;
        var child = parent.Copy();
        var end = parent.GetEndProperty();
        if (!child.NextVisible(true)) return;
        while (!SerializedProperty.EqualContents(child, end))
        {
            var isList = child.isArray && child.propertyType != SerializedPropertyType.String;
            sb.AppendLine($"{indent}{child.name} : {(isList ? "list" : child.propertyType.ToString())}");
            if (isList && child.arraySize > 0)
                DumpFields(child.GetArrayElementAtIndex(0), sb, indent + "    ", depth + 1);
            if (!child.NextVisible(false)) break;
        }
    }

    private static void Apply(PathWeaver root, bool log, bool dryRun)
    {
        var lines = new List<string>();

        foreach (var comp in root.GetComponentsInChildren<Component>(true))
        {
            if (comp == null) continue;
            var typeName = comp.GetType().FullName;
            if (log && typeName.Contains("VRCFury") && typeName != "VF.Model.VRCFury")
                lines.Add($"[{comp.name}] skipped component type {typeName}");
            if (typeName != "VF.Model.VRCFury") continue;

            var so = new SerializedObject(comp);
            var content = so.FindProperty("content");
            if (content == null)
            {
                if (log) lines.Add($"[{comp.name}] no 'content' field; top-level fields: " + string.Join(", ", TopLevelFields(so)));
                continue;
            }

            var rule = root.rules.FirstOrDefault(r =>
                !string.IsNullOrEmpty(r.featureType) &&
                content.managedReferenceFullTypename.EndsWith(" " + r.featureType));
            if (rule == null) continue;

            var folder = FolderPath(root, comp.transform);
            if (!string.IsNullOrEmpty(root.rootPrefix))
                folder = string.IsNullOrEmpty(folder) ? root.rootPrefix.Trim('/') : root.rootPrefix.Trim('/') + "/" + folder;

            var props = Resolve(content, rule.pathField.Split('.')).ToList();
            if (props.Count == 0)
            {
                lines.Add($"[{comp.name}] field '{rule.pathField}' not found on {rule.featureType}");
                continue;
            }

            foreach (var prop in props)
            {
                if (prop.propertyType != SerializedPropertyType.String) continue;

                var oldPath = prop.stringValue ?? "";
                string newPath;
                if (rule.mode == PathWeaver.PathMode.FolderOnly)
                {
                    newPath = folder;
                }
                else
                {
                    var leaf = oldPath.Split('/').Last().Trim();
                    if (leaf.Length == 0) continue;
                    newPath = string.IsNullOrEmpty(folder) ? leaf : folder + "/" + leaf;
                }

                lines.Add($"[{comp.name}] {oldPath}  ->  {newPath}");
                if (!dryRun && newPath != oldPath) prop.stringValue = newPath;
            }

            if (!dryRun) so.ApplyModifiedPropertiesWithoutUndo();
        }

        if (log) Debug.Log($"[PathWeaver] {lines.Count} item(s):\n" + string.Join("\n", lines), root);
    }

    private static IEnumerable<string> TopLevelFields(SerializedObject so)
    {
        var it = so.GetIterator();
        if (it.NextVisible(true))
            do { yield return it.name; } while (it.NextVisible(false));
    }

    // Walks a dotted field path. When a segment is a list, continues into every element.
    private static IEnumerable<SerializedProperty> Resolve(SerializedProperty parent, string[] segments, int i = 0)
    {
        if (i >= segments.Length) { yield return parent; yield break; }
        var next = parent.FindPropertyRelative(segments[i]);
        if (next == null) yield break;
        if (next.isArray && next.propertyType != SerializedPropertyType.String)
        {
            for (var k = 0; k < next.arraySize; k++)
                foreach (var p in Resolve(next.GetArrayElementAtIndex(k), segments, i + 1)) yield return p;
        }
        else
        {
            foreach (var p in Resolve(next, segments, i + 1)) yield return p;
        }
    }

    // Names of every GameObject from the root (exclusive) down to t (inclusive).
    private static string FolderPath(PathWeaver root, Transform t)
    {
        var parts = new List<string>();
        for (var cur = t; cur != null && cur != root.transform; cur = cur.parent)
        {
            var n = cur.name.Trim();
            if (!string.IsNullOrEmpty(root.skipPrefix) && n.StartsWith(root.skipPrefix)) continue;
            parts.Add(n);
        }
        parts.Reverse();
        return string.Join("/", parts);
    }
}
