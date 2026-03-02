using HeavenlyArsenal.Content.Items.Misc;
using HeavenlyArsenal.Content.Items.Weapons.CCR_Weapon;
using NoxusBoss.Core.Autoloaders.SolynBooks;
using NoxusBoss.Core.Graphics.UI.Books;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HeavenlyArsenal.Core.Systems
{
    internal class TaxidermyBookReward : ModSystem
    {
        public override void PostSetupContent()
         {
            AutoloadableSolynBook taxidermyBook;
            ModItem a = ModContent.GetModItem(SolynBookRegistry.SolynBookItemType);
            if (a.Type == SolynBookRegistry.SolynBookItemType)
            {
                taxidermyBook = a as AutoloadableSolynBook;
                SolynReward taxidermyReward;

                if (ModLoader.HasMod("CalamityHunt"))
                {
                    taxidermyReward = new SolynReward()
                    {
                        ItemName = "BadApple",
                        MinStack = 1,
                        MaxStack = 1

                    };
                }
                else
                {
                    taxidermyReward = new SolynReward()
                    {
                        ItemName = "CombatStimItem",
                        MinStack = 30,
                        MaxStack = 30

                    };
                }

                SolynReward e = new SolynReward()
                {
                    ItemName = "BloodyTear",
                    MinStack = 1,
                    MaxStack = 3
                };
                SolynBookRewardsSystem.AddRewardForBook(taxidermyBook, taxidermyReward);
                SolynBookRewardsSystem.AddRewardForBook(taxidermyBook, e);
            }
        }
    }
}
