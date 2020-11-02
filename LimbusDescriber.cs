using HistoryKit;

namespace XRL.World.Limbus
{
    using SimpleJSON;
    using System.IO;
    using System.Linq;
    using XRL.World.Limber;

    [HasModSensitiveStaticCache]
    public static class LimbusDescriber
    {
        [ModSensitiveCacheInit]
        public static void AddLimbusDescriptors()
        {
            var modInfo = ModManager.Mods.Where(i => i.DisplayTitle == "Limber").First();
            var jsonFile = Path.Combine(modInfo.Path, "IngredientSpice.json");
            var text = File.ReadAllText(jsonFile);

            // HistoricSpice.ResolveRelativeLinks is private 😭😭😭
            text = text.Replace("^.", "spice.cooking.recipeNames.ingredients.LimberDrop.");

            var node = JSON.Parse(text);
            var ingredients = HistoricSpice.roots["cooking"]["recipeNames"]["ingredients"];
            ingredients["LimberDrop"] = node["LimberDrop"];
        }
    }
}

