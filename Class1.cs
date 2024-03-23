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
using StardewValley.Buffs;
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

        static float HealthOffset = 0; 
        static float EnergyOffset = 0;
        
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
            Monitor.Log("Current Energy is " + Game1.player.MaxStamina, LogLevel.Info);
            Game1.player.maxHealth += (int)Math.Round(HealthOffset);
            Game1.player.health += (int)Math.Round(HealthOffset);
            Buff buff = new Buff(
                id: "StrongerYou",
                duration: 999999999,
                effects: new BuffEffects()
                {
                    MaxStamina = { (int)Math.Round(EnergyOffset) },
                }
            );
            buff.visible = false;
            Game1.player.Stamina += (int)Math.Round(EnergyOffset);  
            Game1.player.applyBuff(buff);
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
            Game1.player.maxHealth -= (int)Math.Round(HealthOffset);
            //Remove the Buff
            if(Game1.player.hasBuff("StrongerYou"))
            {
                Game1.player.buffs.Remove("StrongerYou");
            }
        }
    }

    class ModConfig
    {
        public bool Enabled = true;
        public float HealthMultiplier = 2.0f;
        public float EnergyMultiplier = 2.0f;
        public int BaseExtraHealth = 0;
        public int BaseExtraEnergy = 0;
    }
}
