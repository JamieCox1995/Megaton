using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.Experimental.AssetImporters;
using UnityEngine;

[ScriptedImporter(1, "jhf")]
public class HersheyFontImporter : ScriptedImporter
{
    public override void OnImportAsset(AssetImportContext ctx)
    {
        var font = ScriptableObject.CreateInstance<HersheyFont>();
        
        using (var parser = new HersheyFontParser(File.OpenText(ctx.assetPath)))
        {
            font.Characters = new List<HersheyCharacter>(parser.ReadCharacters());
        }

        ctx.AddObjectToAsset("MainAsset", font);
        ctx.SetMainObject(font);
    }
}
