namespace HeavenlyArsenal.Utilities.Extensions;

public static class RecipeExtensions
{
    public static void TryAddIngredient(this Recipe recipe, string modName, string itemName)
    {
        if (!ModLoader.TryGetMod(modName, out var mod) || !mod.TryFind<ModItem>(itemName, out var item))
        {
            return;
        }
        
        recipe.AddIngredient(item.Type);
    }
}