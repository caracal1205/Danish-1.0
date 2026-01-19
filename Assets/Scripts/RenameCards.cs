using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

public class RenameCards : Editor
{
    // Dictionary to map numeric strings to Enum names
    private static readonly Dictionary<string, string> RankMap = new Dictionary<string, string>
    {
        {"2", "Two"}, {"3", "Three"}, {"4", "Four"}, {"5", "Five"},
        {"6", "Six"}, {"7", "Seven"}, {"8", "Eight"}, {"9", "Nine"},
        {"10", "Ten"}, {"11", "Jack"}, {"12", "Queen"}, {"13", "King"}, {"14", "Ace"},
        {"jack", "Jack"}, {"queen", "Queen"}, {"king", "King"}, {"ace", "Ace"}
    };

    [MenuItem("Tools/Rename Cards to Match Enum")]
    public static void RenameSelectedCards()
    {
        // Filter selection to only include Prefabs/Models in the Project tab
        Object[] selection = Selection.GetFiltered(typeof(GameObject), SelectionMode.Assets);

        int count = 0;
        foreach (Object obj in selection)
        {
            string oldName = obj.name; // e.g., "Hearts_2"
            string[] parts = oldName.Split('_');

            if (parts.Length == 2)
            {
                string suit = parts[0];
                string rankInput = parts[1].ToLower();

                if (RankMap.ContainsKey(rankInput))
                {
                    string newRank = RankMap[rankInput];
                    string newName = $"{suit}_{newRank}";

                    string assetPath = AssetDatabase.GetAssetPath(obj);
                    string result = AssetDatabase.RenameAsset(assetPath, newName);

                    if (string.IsNullOrEmpty(result)) count++;
                }
            }
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log($"Successfully renamed {count} cards!");
    }
}