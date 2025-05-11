﻿/*
 * Copyright 2024 Peter Han
 * Permission is hereby granted, free of charge, to any person obtaining a copy of this software
 * and associated documentation files (the "Software"), to deal in the Software without
 * restriction, including without limitation the rights to use, copy, modify, merge, publish,
 * distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the
 * Software is furnished to do so, subject to the following conditions:
 *
 * The above copyright notice and this permission notice shall be included in all copies or
 * substantial portions of the Software.
 *
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING
 * BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
 * NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM,
 * DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
 * FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
 */

using Newtonsoft.Json;
using PeterHan.PLib.Options;

namespace PeterHan.TurnBackTheClock {
	/// <summary>
	/// The options class used for Turn Back the Clock.
	/// </summary>
	[ModInfo("https://github.com/peterhaneve/ONIMods", "preview.png", collapse: true)]
	[ConfigFile(SharedConfigLocation: true)]
	[JsonObject(MemberSerialization.OptIn)]
	[RestartRequired]
	public sealed class TurnBackTheClockOptions : SingletonOptions<TurnBackTheClockOptions> {
		#region MD471618
		/// <summary>
		/// Disables buildings introduced in MD-471618, Breath of Fresh Air.
		/// </summary>
		[Option("Auto-Discover Items", "Fix many save migration issues such as missing items in filters or the\nConsumables menu by re-discovering all items on the map upon load.", "MD-471618: Breath of Fresh Air")]
		[JsonProperty]
		public bool MD471618_DiscoverAll { get; set; } = false;

		/// <summary>
		/// Returns the Desalinator to a minimum 40 C output temperature on brine.
		/// </summary>
		[Option("Desalinator Temperature", "Sets the minimum output temperature of the Desalinator on Brine to 40 C.", "MD-471618: Breath of Fresh Air")]
		[JsonProperty]
		public bool MD471618_DesalinatorTemperature { get; set; } = false;

		/// <summary>
		/// Allows diagonal access for most errands again.
		/// </summary>
		[Option("Diagonal Access", "Allow Duplicant diagonal access through corners for most errands and items.", "MD-471618: Breath of Fresh Air")]
		[JsonProperty]
		public bool MD471618_DiagonalAccess { get; set; } = false;

		/// <summary>
		/// Disables buildings introduced in Breath of Fresh Air.
		/// </summary>
		[Option("Disable Buildings", "Disables these buildings: Oxygen Mask Locker, Oxygen Mask Marker, Crafting Station,\nGas Meter Valve, Liquid Meter Valve, Solid Meter Valve.", "MD-471618: Breath of Fresh Air")]
		[JsonProperty]
		public bool MD471618_DisableBuildings { get; set; } = false;

		/// <summary>
		/// Disables the food rot changes.
		/// </summary>
		[Option("Food Storage", "Reverts food storage to only require Sterile Atmosphere for preservation.\nDisables Eco Mode on Refrigerators.", "MD-471618: Breath of Fresh Air")]
		[JsonProperty]
		public bool MD471618_EzFoodStorage { get; set; } = false;

		/// <summary>
		/// Removes seeds from the diet of all Pacu.
		/// </summary>
		[Option("Pacu Diet", "Pacu will no longer be able to eat seeds.", "MD-471618: Breath of Fresh Air")]
		[JsonProperty]
		public bool MD471618_PacuDiet { get; set; } = false;

		/// <summary>
		/// Disables the deodorizer power and heat.
		/// </summary>
		[Option("Powerless Deodorizers", "Deodorizers no longer require Power or generate Heat.", "MD-471618: Breath of Fresh Air")]
		[JsonProperty]
		public bool MD471618_DeodorizerPower { get; set; } = false;

		/// <summary>
		/// Prevents Duplicants from getting debuffs in bad gases.
		/// </summary>
		[Option("Remove Debuffs", "Yucky Lungs and Eye Irritation cannot be inflicted upon Duplicants.", "MD-471618: Breath of Fresh Air")]
		[JsonProperty]
		public bool MD471618_Debuffs { get; set; } = false;

		/// <summary>
		/// Changes Solar Panels from SCO to LI/GI to allow Heavy-Watt wire to be built in them again.
		/// </summary>
		[Option("Solar Panel Occupies Floor", "Solar Panels are no longer considered solid tiles, changing how Heat transfer works with them.", "MD-471618: Breath of Fresh Air")]
		[JsonProperty]
		public bool MD471618_SolarPanelWiring { get; set; } = false;

		/// <summary>
		/// Returns items to their old locations on the tech tree.
		/// </summary>
		[Option("Tech Tree", "Resets all items to their previous locations on the Technology Tree.", "MD-471618: Breath of Fresh Air")]
		[JsonProperty]
		public bool MD471618_TechTree { get; set; } = false;

		/// <summary>
		/// Removes all the new traits and makes initial attributes 7/3/1 again.
		/// </summary>
		[Option("Traits and Skills", "Prevents the acquisition of traits introduced in this update,\nand reverts initial skill distributions to their previous values.", "MD-471618: Breath of Fresh Air")]
		[JsonProperty]
		public bool MD471618_Traits { get; set; } = false;
		#endregion

		#region MD509629
		/// <summary>
		/// Disables buildings introduced in Fast Friends.
		/// </summary>
		[Option("Disable Buildings", "Disables these buildings: Clothing Refashionator.\nRemoves Primo Garb from Care Packages.", "MD-509629: Fast Friends")]
		[JsonProperty]
		public bool MD509629_DisableBuildings { get; set; } = false;

		/// <summary>
		/// Disables critter morphs introduced in Fast Friends.
		/// </summary>
		[Option("Disable Critter Morphs", "Disables these critters: Cuddle Pip, Delecta Vole, Sanishell, Oakshell.", "MD-509629: Fast Friends")]
		[JsonProperty]
		public bool MD509629_DisableCreatures { get; set; } = false;

		/// <summary>
		/// Disables Duplicants introduced in Fast Friends (and their stress/joy reactions).
		/// </summary>
		[Option("Disable Duplicants", "Disables these Duplicants: Amari, Pei, Quinn, Steve.\n<b>WARNING: Saves with these Duplicants in them will fail to load!</b>", "MD-509629: Fast Friends")]
		[JsonProperty]
		public bool MD509629_DisableDuplicants { get; set; } = false;
		
		/// <summary>
		/// Disables the recipe for Curried Beans.
		/// </summary>
		[Option("Disable Food", "Disables these items: Curried Beans.", "MD-509629: Fast Friends")]
		[JsonProperty]
		public bool MD509629_DisableFood { get; set; } = false;
		#endregion

		#region MD525812
		/// <summary>
		/// Disables buildings introduced in Sweet Dreams.
		/// </summary>
		[Option("Disable Buildings", "Disables these buildings: Spice Grinder, Somnium Synthesizer, Critter Flux-O-Matic.\nDisables the Kitchen room as it can no longer be created.", "MD-525812: Sweet Dreams")]
		[JsonProperty]
		public bool MD525812_DisableBuildings { get; set; } = false;

		/// <summary>
		/// Disables all story traits, both from this update and later ones.
		/// </summary>
		[Option("Disable Story Traits", "Disables ALL story traits.", "MD-525812: Sweet Dreams")]
		[JsonProperty]
		public bool MD525812_DisableStoryTraits { get; set; } = false;

#if false
		/// <summary>
		/// Removes refined metals from Plug Slug diets.
		/// </summary>
		[Option("Plug Slug Diet", "Plug Slugs will no longer be able to eat Refined Metals.", "MD-525812: Sweet Dreams")]
		[JsonProperty]
		public bool MD525812_SlugDiet { get; set; } = false;

		/// <summary>
		/// Makes plug slugs drown again and prevent new sponge or smog slugs from morphing
		/// (Existing ones will function normally).
		/// </summary>
		[Option("Plug Slugs Drown", "Disable Smog Slugs and Sponge Slugs from morphing, and allow Plug Slugs to drown.", "MD-525812: Sweet Dreams")]
		[JsonProperty]
		public bool MD525812_SlugsDrown { get; set; } = false;
#endif
		#endregion

		#region MD535720
		/// <summary>
		/// Disables buildings introduced in Hot Shots.
		/// </summary>
		[Option("Disable Buildings", "Disables these buildings: Conduction Panel, Geotuner, Mission Control Station.", "MD-535720: Hot Shots")]
		[JsonProperty]
		public bool MD535720_DisableBuildings { get; set; } = false;

		/// <summary>
		/// Disables rooms introduced in Hot Shots.
		/// </summary>
		[Option("Disable Rooms", "Disables the Laboratory and Private Bedroom.", "MD-535720: Hot Shots")]
		[JsonProperty]
		public bool MD535720_DisableRooms { get; set; } = false;
		
		/// <summary>
		/// Reverts Drywall to its old values.
		/// </summary>
		[Option("Drywall Building", "Restores the old mass and build time of Drywall.", "MD-535720: Hot Shots")]
		[JsonProperty]
		public bool MD535720_Drywall { get; set; } = false;
		#endregion

		public TurnBackTheClockOptions() { }
	}
}
