using StardewModdingAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Menus;

namespace StrongerYou
{
    public class ModEntry : Mod
    {
        private ModConfig Config;
        public override void Entry(IModHelper helper)
        {
            Config = this.Helper.ReadConfig<ModConfig>();
            if (Config.Enabled)
            {
                helper.Events.GameLoop.DayEnding += GameLoopOnDayEnding;
                helper.Events.GameLoop.DayStarted += GameLoopOnDayStarted;   
                Monitor.Log("Up and Running", LogLevel.Info);
            }
            else
            {
                Monitor.Log("Disabled.", LogLevel.Info);   
            }
        }

        static int HealthOffset = 50; 
        static int EnergyOffset = 50;
        
        private void CalculateOffsets()
        {
            if (!Context.IsWorldReady)
            {
                HealthOffset = 0;
                EnergyOffset = 0;
                Monitor.Log("Game wasn't ready to calculate offsets.", LogLevel.Error);
                return;
            }

            int FarmingLevel = Game1.player.FarmingLevel;
            int MiningLevel = Game1.player.MiningLevel;
            int ForagingLevel = Game1.player.ForagingLevel;
            int FishingLevel = Game1.player.FishingLevel;
            int CombatLevel = Game1.player.CombatLevel;

            int Bonus = FarmingLevel + MiningLevel + ForagingLevel + FishingLevel + CombatLevel;
            
            //HealthOffset = EnergyOffset = Bonus;

            HealthOffset = (Bonus * Config.HealthMultiplier) + Config.BaseExtraHealth;
            EnergyOffset = (Bonus * Config.EnergyMultiplier) + Config.BaseExtraEnergy;

            Monitor.Log("Base Bonus before Multipliers and Extras: " + Bonus, LogLevel.Info);
        }
        
        private void GameLoopOnDayStarted(object sender, DayStartedEventArgs e)
        {
            if (!Context.IsWorldReady)
            {
                return;
            }
            CalculateOffsets();
            //Add Health and Energy
            Monitor.Log("Current Health is " + Game1.player.maxHealth, LogLevel.Info);
            Monitor.Log("Current Energy is "+ Game1.player.maxStamina, LogLevel.Info);
            Game1.player.maxHealth += HealthOffset;
            Game1.player.health += HealthOffset;
            Game1.player.MaxStamina += EnergyOffset;
            Game1.player.stamina += EnergyOffset;
            Monitor.Log("Added Bonus Health and Energy based on Levels.", LogLevel.Info);
        }

        private void GameLoopOnDayEnding(object sender, DayEndingEventArgs e)
        {
            if (!Context.IsWorldReady)
            {
                return;
            }
            //Remove Health and Energy
            Monitor.Log("Removing Bonus Health and Energy Prior to Save...", LogLevel.Info);
            Game1.player.maxHealth -= HealthOffset;
            Game1.player.MaxStamina -= EnergyOffset;
        }
    }

    class ModConfig
    {
        public bool Enabled = true;
        public int HealthMultiplier = 2;
        public int EnergyMultiplier = 2;
        public int BaseExtraHealth = 0;
        public int BaseExtraEnergy = 0;
    }
}
