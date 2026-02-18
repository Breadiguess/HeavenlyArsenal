namespace HeavenlyArsenal.Utilities.Extensions;

/// <summary>
///     Provides <see cref="Recipe" /> extension methods.
/// </summary>
public static class RecipeExtensions
{
    /// <summary>
    ///     Attempts to add an ingredient to the given <see cref="Recipe" />.
    /// </summary>
    /// <param name="recipe">The <see cref="Recipe" /> to add the ingredient to.</param>
    /// <param name="modName">The internal name of the mod containing the item.</param>
    /// <param name="itemName">The internal name of the item to add as an ingredient.</param>
    /// <remarks>
    ///     If the specified mod or item cannot be found, this method will fail silently and not add any
    ///     ingredient to the recipe.
    /// </remarks>
    public static void TryAddIngredient(this Recipe recipe, string modName, string itemName)
    {
        if (!ModLoader.TryGetMod(modName, out var mod) || !mod.TryFind<ModItem>(itemName, out var item))
        {
            return;
        }

        recipe.AddIngredient(item.Type);
    }
}