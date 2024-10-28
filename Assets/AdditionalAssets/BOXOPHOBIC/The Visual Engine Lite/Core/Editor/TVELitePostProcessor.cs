using UnityEditor;

namespace TheVisualEngineLite
{
    class TVELitePostProcessor : AssetPostprocessor
    {
        static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
        {
            foreach (var path in importedAssets)
            {
                if (path.EndsWith(".litecollection"))
                {
                    EditorApplication.ExecuteMenuItem("Window/BOXOPHOBIC/The Visual Engine Lite/Material Upgrader");
                }
            }
        }
    }
}