using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
using System.Collections.Generic;

public class AddressDuplicateChecker
{
    [MenuItem("Tools/Check Address Duplicate")]
    public static void Check()
    {
        var settings = AddressableAssetSettingsDefaultObject.Settings;
        var map = new Dictionary<string, string>();

        foreach (var group in settings.groups)
        {
            foreach (var entry in group.entries)
            {
                if (map.TryGetValue(entry.address, out var exist))
                {
                    UnityEngine.Debug.LogError(
                        $"Address³åÍ»: {entry.address}\n{exist}\n{entry.AssetPath}"
                    );
                }
                else
                {
                    map[entry.address] = entry.AssetPath;
                }
            }
        }

        UnityEngine.Debug.Log("Address¼ì²éÍê³É");
    }
}
