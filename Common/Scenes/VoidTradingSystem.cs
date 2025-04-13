using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ModLoader;
using NoxusBoss.Core.World.GameScenes.AvatarUniverseExploration;
using HeavenlyArsenal.Content.Items.Armor;
using HeavenlyArsenal.Content.Items.Misc;
using NoxusBoss.Assets;
using CalamityMod.Items.Accessories;
using CalamityMod.Items.Weapons.Melee;
using HeavenlyArsenal.Content.Items.Weapons.Melee;
using Luminance.Core.Graphics;
using HeavenlyArsenal.Content.Items.Armor.ShintoArmor;
using CalamityMod.Items.Accessories.Wings;
using CalamityMod.Items;
using HeavenlyArsenal.Content.Items.Weapons.Rogue;
using Terraria.ID;
using HeavenlyArsenal.Content.Items.Weapons.Summon;

using HeavenlyArsenal.Core.Globals;
using NoxusBoss.Core.Utilities;
using CalamityMod.Items.Armor.OmegaBlue;
using HeavenlyArsenal.Content.Items.Armor.NewFolder;
using NoxusBoss.Content.Items.Fishing;
using HeavenlyArsenal.Content.Items.Weapons.Magic;

using HeavenlyArsenal.Content.Items.Accessories.VoidCrestOath;
using HeavenlyArsenal.Content.Items.Accessories.Cosmetic;

namespace HeavenlyArsenal.Common.Scenes
{
    // Define an enum to specify where the output items should appear.
    public enum ItemReturnType
    {
        None,       // Spawned on top of (at) the player.
        ShadowHand, // Delivered to the player's Shadow Hand (not implemented).
        SpatOut     // Spawned from below the bottom of the screen.
    }

    // Data structure that defines a trade.
    public class TradeDefinition
    {
        public int InputItemType { get; set; }
        public float MinDistance { get; set; }
        public List<(int itemType, int quantity)> OutputItems { get; set; }
        public ItemReturnType ReturnType { get; set; }

        public TradeDefinition(int inputItemType, float minDistance, ItemReturnType returnType, params int[] outputItemPairs)
        {
            InputItemType = inputItemType;
            MinDistance = minDistance;
            ReturnType = returnType;
            OutputItems = new List<(int, int)>();

            if (outputItemPairs.Length % 2 != 0)
            {
                throw new ArgumentException("Output item pairs must be in itemType, quantity format.");
            }

            for (int i = 0; i < outputItemPairs.Length; i += 2)
            {
                OutputItems.Add((outputItemPairs[i], outputItemPairs[i + 1]));
            }
        }
    }


    // The main ModSystem that processes trades.
    class VoidTradingSystem : ModSystem
    {
        // A list of all possible trade definitions.
        private List<TradeDefinition> tradeDefinitions = new List<TradeDefinition>();

        public override void PostSetupContent()
        {
            //fun to Blood
            tradeDefinitions.Add(new TradeDefinition(
               //item to trade
               ModContent.ItemType<ChaliceOfFun>(),
               1000f,
               ItemReturnType.None,
               //Items to get back

               ModContent.ItemType<ChaliceOfTheBloodGod>(), 1
                ));

            //Blood to fun
            tradeDefinitions.Add(new TradeDefinition(
               //item to trade
               ModContent.ItemType<ChaliceOfTheBloodGod>(),
               1000f,
               ItemReturnType.None,
               //Items to get back

               ModContent.ItemType<ChaliceOfFun>(), 1
                ));

            //drew wings to dev wings
            tradeDefinitions.Add(new TradeDefinition(
               //item to trade
            ModContent.ItemType<DrewsWings>(),
            1000f,
            ItemReturnType.None,
               //Items to get back
               
            ModContent.ItemType<DevWing>(), 1
              
            ));
            //rock to cessation
            tradeDefinitions.Add(new TradeDefinition(
               //item to trade
               ModContent.ItemType<Rock>(),
               1000f,
               ItemReturnType.None,
               //Items to get back

               ModContent.ItemType<LifeAndCessation>(), 1

                ));
            //solynel
            tradeDefinitions.Add(new TradeDefinition(
               //item to trade
               ItemID.BlandWhip,
               1000f,
               ItemReturnType.None,
               //Items to get back

               ModContent.ItemType<SolynWhip_Item>(), 1

                ));

            //coin for coin
            tradeDefinitions.Add(new TradeDefinition(
               //item to trade
               ModContent.ItemType<CoinofDeceit>(),
               1000f,
               ItemReturnType.None,
               //Items to get back

               ModContent.ItemType<AncientCoin>(), 1
                ));

            //armor bargain
            tradeDefinitions.Add(new TradeDefinition(
              //item to trade
              ModContent.ItemType<AncientCoin>(),
              1000f,
              ItemReturnType.None,
               //Items to get back
               ModContent.ItemType<ShintoArmorLeggings>(), 1,
               ModContent.ItemType<ShintoArmorBreastplate>(), 1,
               ModContent.ItemType<ShintoArmorHelmetAll>(), 1
              
               ));
           
            //nadir to nadir2
            tradeDefinitions.Add(new TradeDefinition(
              //item to trade
              ModContent.ItemType<Nadir>(),
              1000f,
              ItemReturnType.None,
              //Items to get back

              ModContent.ItemType<AvatarLonginus>(), 1
               ));

            //omega blue to temp
            tradeDefinitions.Add(new TradeDefinition(
              //item to trade
              ModContent.ItemType<OmegaBlueChestplate>(),
              1000f,
              ItemReturnType.None,
              //Items to get back

              ModContent.ItemType<TempBreastplate>(), 1
               ));
            //Rod to Flyrift
            tradeDefinitions.Add(new TradeDefinition(
               //item to trade
               ModContent.ItemType<Dreamcatcher>(),
               1000f,
               ItemReturnType.None,
               //Items to get back

               ModContent.ItemType<avatar_FishingRod>(), 1
                ));
            //FlyRift to Dreamcatcher
            tradeDefinitions.Add(new TradeDefinition(
               //item to trade
               ModContent.ItemType<avatar_FishingRod>(),
               1000f,
               ItemReturnType.None,
               //Items to get back

               ModContent.ItemType<Dreamcatcher>(), 1
                ));
            //Warbanner to VoidCrest
            tradeDefinitions.Add(new TradeDefinition(
               //item to trade
               ModContent.ItemType<WarbanneroftheSun>(),
               1000f,
               ItemReturnType.None,
               //Items to get back

               ModContent.ItemType<VoidCrestOath>(), 1
                ));



            TradeInputRegistry.RegisterTrades(tradeDefinitions);
        }
        public static class TradeInputRegistry
        {
            // The list of all input item types used in trades.
            public static List<int> InputItemTypes { get; } = new List<int>();

            /// <summary>
            /// Registers the input item types from the provided list of trade definitions.
            /// </summary>
            /// <param name="trades">A list of trade definitions to register.</param>
            public static void RegisterTrades(List<TradeDefinition> trades)
            {
                InputItemTypes.Clear();
                foreach (TradeDefinition trade in trades)
                {
                    // Only add each unique input type once.
                    if (!InputItemTypes.Contains(trade.InputItemType))
                    {
                        InputItemTypes.Add(trade.InputItemType);
                    }
                }
            }
        }
        public override void PostUpdateEverything()
        {
           // TODO: only run this code when the player has something in their inventory that can be traded
            // Only process trades when in the Avatar Universe.

            
            if (AvatarUniverseExplorationSystem.InAvatarUniverse)
            {


                Player player = Main.LocalPlayer;
                

                //Main.NewText($"Pushout :{AvatarUniverseExplorationSky.PushPlayersOutInterpolant}");

                //Main.NewText($"Time: {player.GetValueRef<int>(AvatarUniverseExplorationSky.TimeInUniverseVariableName).Value}");
                //Main.NewText(AvatarUniverseExplorationSky.PushPlayersOutInterpolant, Color.AntiqueWhite);

                // Check each trade definition.
                foreach (TradeDefinition trade in tradeDefinitions)
                {
                    // For each trade, iterate over all world items to look for the required input item.
                    for (int i = 0; i < Main.maxItems; i++)
                    {
                        Item worldItem = Main.item[i];

                        if (worldItem.active && worldItem.type == trade.InputItemType)
                        {

                          
                            // Check that the found item meets the minimum distance requirement.
                            if (Vector2.Distance(worldItem.Center, player.Center) > trade.MinDistance)
                            {
                                // Log the deletion for debugging.
                                Main.NewText($"Deleting trade input item: {worldItem.Name}", Color.AntiqueWhite);

                                // Remove the input item.
                                worldItem.TurnToAir();

                                // Play a sound to indicate successful trade execution.
                                SoundEngine.PlaySound(GennedAssets.Sounds.Avatar.Clap with { PitchVariance = 0.2f });
                                ScreenShakeSystem.SetUniversalRumble(4* 20f, MathHelper.TwoPi, null, 0.45f);
                                //AvatarUniverseExplorationSystem.
                                // Process each output item defined in this trade.
                                foreach ((int outputItemType, int quantity) in trade.OutputItems)
                                {
                                    Main.NewText($"Prepairing to create:{outputItemType}", Color.AntiqueWhite);
                                    for (int r = 0; r < quantity; r++)
                                    {
                                        Vector2 spawnPosition = GetSpawnPosition(player, trade.ReturnType);

                                        int index = Item.NewItem(new EntitySource_Misc("VoidTradingSystem"),
                                            (int)spawnPosition.X, (int)spawnPosition.Y,
                                            player.width, player.height, outputItemType); // Ensure outputItemType is correctly used here.
                                        Main.NewText($"Created item: {Main.item[index].Name} (Type: {outputItemType}), Index: {index}", Color.AntiqueWhite);
                                        if (index >= 0 && index < Main.maxItems)
                                        {
                                            // Optional: additional properties
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
       

        /// <summary>
        /// Determines the spawn position for an output item based on the specified return type.
        /// </summary>
        /// <param name="player">Reference to the player.</param>
        /// <param name="returnType">The chosen return type for the spawn location.</param>
        /// <returns>A Vector2 representing the position where the item will spawn.</returns>
        private Vector2 GetSpawnPosition(Player player, ItemReturnType returnType)
        {
            switch (returnType)
            {
                case ItemReturnType.None:
                    // Spawn directly at the player's center.
                    return player.Center;
                case ItemReturnType.ShadowHand:
                    // ShadowHand is not implemented; fallback to player's center.
                    return player.Center;
                case ItemReturnType.SpatOut:
                    // Calculate a position below the bottom of the visible screen.
                    float bottomY = Main.screenPosition.Y + Main.screenHeight;
                    return new Vector2(player.Center.X, bottomY + 200f);
                default:
                    return player.Center;
            }
        }
    }


}
