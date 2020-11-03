using HistoryKit;

namespace XRL.World.Limber
{
    using SimpleJSON;
    using System.IO;
    using System.Linq;

    [HasModSensitiveStaticCache]
    public static class IngredientDescriber
    {
        [ModSensitiveCacheInit]
        public static void AddLimbusDescriptors()
        {
            var modInfo = ModManager.Mods.Where(i => i.DisplayTitle == "Limber").First();
            var jsonFile = Path.Combine(modInfo.Path, "IngredientSpice.json");
            var text = File.ReadAllText(jsonFile);
            var root = JSON.Parse(text);
            var ingredients = HistoricSpice.roots["cooking"]["recipeNames"]["ingredients"];

            foreach (var key in root.AsObject.GetKeys()) {
                ingredients[key] = root[key];
            }
        }
    }
}

