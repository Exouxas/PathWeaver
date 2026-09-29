using System;
using System.Collections.Generic;
using UnityEngine;
using VRC.SDKBase;

// PathWeaver: the GameObject hierarchy under this component becomes the menu folder structure.
// At build time, every matching VRCFury feature underneath gets its path rewritten to
//   [rootPrefix/] <object path relative to this root> [/ <last segment of the existing path>]
[AddComponentMenu("PathWeaver/PathWeaver")]
[DisallowMultipleComponent]
public class PathWeaver : MonoBehaviour, IEditorOnly
{
    public enum PathMode
    {
        [Tooltip("Folder path + the last segment of the existing value (for Toggles)")]
        FolderPlusLeaf,
        [Tooltip("Folder path only, existing value is replaced (for Full Controller menu prefix)")]
        FolderOnly,
    }

    [Serializable]
    public class Rule
    {
        [Tooltip("Full type name of the VRCFury feature, e.g. VF.Model.Feature.Toggle")]
        public string featureType = "VF.Model.Feature.Toggle";

        [Tooltip("Field holding the path. Use dots to step into lists, e.g. menus.prefix")]
        public string pathField = "name";

        public PathMode mode = PathMode.FolderPlusLeaf;
    }

    [Tooltip("Prepended to every generated path, e.g. \"My Avatar\". Leave empty for none.")]
    public string rootPrefix = "";

    [Tooltip("Objects whose name starts with this are skipped when building the folder path, e.g. \"_\".")]
    public string skipPrefix = "";

    public List<Rule> rules = new List<Rule>
    {
        new Rule { featureType = "VF.Model.Feature.Toggle",         pathField = "name",         mode = PathMode.FolderPlusLeaf },
        new Rule { featureType = "VF.Model.Feature.FullController", pathField = "menus.prefix", mode = PathMode.FolderOnly },
    };
}
