namespace XRL.World.Limber {
    using System.Collections.Generic;

    [HasModSensitiveStaticCache]
    public static class PopulationInitializer {
        [ModSensitiveCacheInit]
        public static void AddLimberTonicToPopulations() {
            foreach (var Population in PopulationManager.Populations.Values) {
                List<PopulationItem> newItems = null;
                foreach (var Item in Population.Items) {
                    var result = AddLimberTonicToItemDeep(Item);
                    if (result != null) {
                        if (newItems == null) {
                            newItems = result;
                        } else {
                            newItems.AddRange(result);
                        }
                    }
                }
                if (newItems != null) {
                    Population.Items.AddRange(newItems);
                }
            }
        }

        private static List<PopulationItem> AddLimberTonicToItemDeep(PopulationItem item) {
            if (item is PopulationObject obj) {
                // just add Limber stuff anywhere Nectar stuff show up
                if (obj.Blueprint == "NectarTonic") {
                    var newObj = new PopulationObject("LimberTonic", obj.Number, obj.Weight, obj.Builder);
                    return new List<PopulationItem> { newObj };
                }
                if (obj.Blueprint == "Drop of Nectar") {
                    var newObj = new PopulationObject("LimberDrop", obj.Number, obj.Weight, obj.Builder);
                    return new List<PopulationItem> { newObj };
                }
            } else if (item is PopulationGroup Group) {
                List<PopulationItem> newItems = null;
                foreach (var Item in Group.Items) {
                    var result = AddLimberTonicToItemDeep(Item);
                    if (result != null) {
                        if (newItems == null) {
                            newItems = result;
                        } else {
                            newItems.AddRange(result);
                        }
                    }
                }
                if (newItems != null) {
                    Group.Items.AddRange(newItems);
                }
            }
            return null;
        }
    }
}
