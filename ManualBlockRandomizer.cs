using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ModLoader;
using Terraria.DataStructures;
using Microsoft.Xna.Framework;
using Terraria.ID;
using Terraria.World;
using Terraria.World.Generation;
using Terraria.Utilities;
using Terraria.Enums;

namespace ProgressionBlockRandomizer
{
    class ManualBlockRandomizer : ModItem
    {
		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Manual Block Randomizer");
			Tooltip.SetDefault("Use to randomize the world");  //The (English) text shown below your weapon's name
		}

		public override void SetDefaults()
		{
			item.width = 40;
			item.height = 40;
			item.useTime = 20;
			item.useAnimation = 20;
			item.value = Item.buyPrice(platinum: 10);
			item.rare = ItemRarityID.Purple;
			item.UseSound = SoundID.Item1;
			item.maxStack = 99;

			item.consumable = true;

			item.useStyle = ItemUseStyleID.EatingUsing; // 1 is the useStyle
		}

		public override void AddRecipes()
		{
			ModRecipe recipe = new ModRecipe(mod);
			recipe.AddIngredient(ItemID.MoonLordTrophy, 1);
			recipe.AddTile(TileID.LunarCraftingStation);
			recipe.SetResult(this);
			recipe.AddRecipe();

			ModRecipe recipe2 = new ModRecipe(mod);
			recipe2.AddIngredient(ItemID.LunarBar, 10);
			recipe2.AddIngredient(ItemID.FragmentVortex, 1);
			recipe2.AddIngredient(ItemID.FragmentNebula, 1);
			recipe2.AddIngredient(ItemID.FragmentSolar, 1);
			recipe2.AddIngredient(ItemID.FragmentStardust, 1);
			recipe2.AddTile(TileID.LunarCraftingStation); 
			recipe2.SetResult(this); 
			recipe2.AddRecipe();

			ModRecipe recipe3 = new ModRecipe(mod);
			recipe3.AddIngredient(ItemID.PlatinumCoin, 20);
			recipe3.AddTile(TileID.WorkBenches);
			recipe3.SetResult(this);
			recipe3.AddRecipe();
		}


		public override bool UseItem(Player player)
		{
			return ProgressionRandomizerWorld.StartRandomizing();
		}
	}
}
