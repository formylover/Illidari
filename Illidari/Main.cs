﻿using CommonBehaviors.Actions;
using Styx;
using Styx.Common;
using Styx.CommonBot.Routines;
using Styx.TreeSharp;
using Styx.WoWInternals.WoWObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

#region Class Redundancy
using HK = Illidari.Core.Managers.Hotkeys;
using H = Illidari.Rotation.Havoc;
using V = Illidari.Rotation.Vengeance;
using U = Illidari.Core.Unit;
using TM = Illidari.Core.Managers.TalentManager;
using D = Illidari.Rotation.Death;
using L = Illidari.Core.Utilities.Log;
using C = Illidari.Core.Helpers.Common;
using R = Illidari.Rotation.Resting;
using System.Diagnostics;
using System.Reflection;
using Styx.CommonBot;
#endregion

namespace Illidari
{
    public class Main : CombatRoutine
    {

        public static Core.IllidariSettings.IllidariSettings IS;
        private static readonly Version version = new Version(10, 03, 2016);
        public override string Name { get { return string.Format($"{CRName} v{version}"); } }
        public override WoWClass Class { get { return WoWClass.DemonHunter; } }
        public static string CRName { get { return "Illidari"; } }
        private static LocalPlayer Me { get { return StyxWoW.Me; } }

        #region Implementations
        public override Composite PreCombatBuffBehavior
        {
            get
            {
                return new ActionRunCoroutine(ctx => C.SpecSelectorPrecombatBuffBahvior());
            }
        }
        public override Composite CombatBuffBehavior
        {
            get
            {
                return new ActionRunCoroutine(ctx => C.SpecSelectorCombatBuffBehavior());
            }
        }
        public override Composite CombatBehavior
        {
            get
            {
                return new ActionRunCoroutine(ctx => C.SpecSelectorRotation());
            }
        }
        public override Composite PullBehavior
        {
            get
            {
                return new ActionRunCoroutine(ctx => C.SpecSelectorPullBehavior());
            }
        }
        public override Composite RestBehavior
        {
            get
            {
                return new ActionRunCoroutine(ctx => R.RestBehavior());
            }
        }
        public override Composite MoveToTargetBehavior
        {
            get
            {
                //if (DateTime.Now.Second % 5 == 0)
                // L.debugLog("Calling MoveToTargetBehavior");
                return new ActionRunCoroutine(ctx => C.EnsureMeleeRange(Me.CurrentTarget));
            }
        }
        public override Composite PullBuffBehavior
        {
            get
            {
                //if (DateTime.Now.Second % 5 == 0)
                // L.debugLog("Calling PullBuffBehavior");
                return base.PullBuffBehavior;
            }
        }

        public override Composite DeathBehavior { get { return new ActionRunCoroutine(ctx => D.DeathBehavor()); } }
        public override bool NeedDeath { get { return Me.IsDead; } }
        public override bool NeedRest
        {
            get
            {
                if (Me.Specialization == WoWSpec.DemonHunterHavoc)
                {
                    return Me.HealthPercent < IS.GeneralRestingRestHp;
                }
                if (Me.Specialization == WoWSpec.DemonHunterVengeance)
                {
                    return Me.HealthPercent < IS.GeneralRestingRestHp;
                }
                return false;
            }
        }
        public override bool NeedCombatBuffs { get { return base.NeedCombatBuffs; } }

        #region Hidden Overrides
        public override void Initialize()
        {
            Logging.Write(Colors.Fuchsia, "Hello {0}", Me.SafeName);
            Logging.Write("");
            Logging.Write(Colors.Fuchsia, "For optimal performance, please use: Enyo");
            Logging.Write("");
            Logging.Write(Colors.Fuchsia, "Current Version:");
            Logging.Write(Colors.Fuchsia, "-- v" + version + " --");
            Logging.Write(Colors.Fuchsia, "-- by SpeshulK926 --");
            Logging.Write(Colors.Fuchsia, "-- A Demon Hunter's Combat Routine --");


            IS = new Core.IllidariSettings.IllidariSettings();

            TM.initTalents();
            Type type = IS.GetType();
            PropertyInfo[] properties = type.GetProperties();

            foreach (PropertyInfo property in properties)
            {
                if (property.Name == "SettingsPath") { continue; }
                if (property.PropertyType == typeof(List<uint>))
                {
                    List<uint> uintList = (List<uint>)property.GetValue(IS, null);
                    foreach (var uintItem in uintList)
                    {
                        L.debugLog(string.Format($"{property.Name}: {uintItem}"));
                    }
                }
                else
                {
                    L.debugLog(string.Format($"{property.Name}: {property.GetValue(IS, null)}"));
                }

            }

            HK.registerHotkeys();

            BotEvents.OnBotStarted += BotEvents_OnBotStarted;
            BotEvents.OnBotStopped += BotEvents_OnBotStopped;

        }

        private void BotEvents_OnBotStopped(EventArgs args)
        {
            HK.removeHotkeys();
            TM.removeTalEvents();
        }

        private void BotEvents_OnBotStarted(EventArgs args)
        {
            Logging.Write(Me.Specialization.ToString());
            TM.removeTalEvents();
            TM.initTalents();

            HK.registerHotkeys();
        }

        public override bool WantButton { get { return true; } }

        public override CapabilityFlags SupportedCapabilities
        {
            get
            {
                // CapabilityFlags.All                      Not used
                // CapabilityFlags.Aoe                      done
                // CapabilityFlags.DefensiveCooldowns       Havoc done
                // CapabilityFlags.DefensiveDispel          not used 
                // CapabilityFlags.Facing                   done
                // CapabilityFlags.GapCloser                Vengeance done (havoc not coded yet)
                // CapabilityFlags.Interrupting             Vengeance done (havoc not coded yet)
                // CapabilityFlags.Kiting                   Not used
                // CapabilityFlags.MoveBehind               Not used
                // CapabilityFlags.Movement                 done
                // CapabilityFlags.MultiMobPull             Not used
                // CapabilityFlags.None                     not used
                // CapabilityFlags.OffensiveCooldowns       Havoc Done (no offensive cd for vengeance yet)
                // CapabilityFlags.OffensiveDispel          Not used
                // CapabilityFlags.PetSummoning             Not used
                // CapabilityFlags.PetUse                   Not Used
                // CapabilityFlags.SpecialAttacks           Not used
                // CapabilityFlags.Targeting                done
                // CapabilityFlags.Taunting                 done


                int capabilities = (int)CapabilityFlags.Aoe; // aoe always on

                // generic settings for movement, targeting, and facing.
                if (IS.GeneralMovement) { capabilities += (int)CapabilityFlags.Movement; }
                if (IS.GeneralTargeting) { capabilities += (int)CapabilityFlags.Targeting; }
                if (IS.GeneralFacing) { capabilities += (int)CapabilityFlags.Facing; }

                if (Me.Specialization == WoWSpec.DemonHunterHavoc)
                {
                    if (IS.HavocDefensiveCooldowns) { capabilities += (int)CapabilityFlags.DefensiveCooldowns; }
                    if (IS.HavocOffensiveCooldowns) { capabilities += (int)CapabilityFlags.OffensiveCooldowns; }
                }

                if (Me.Specialization == WoWSpec.DemonHunterVengeance)
                {
                    if (IS.VengeanceAllowTaunt) { capabilities += (int)CapabilityFlags.Taunting; }
                    if (IS.VengeanceAllowInterrupt) { capabilities += (int)CapabilityFlags.Interrupting; }
                    capabilities += (int)CapabilityFlags.GapCloser;
                }
                return (CapabilityFlags)capabilities;
            }
        }

        public override void OnButtonPress() { IllidariSettingsForm setForm = new IllidariSettingsForm(); setForm.Show(); }
        public override void ShutDown() { HK.removeHotkeys(); }
        #endregion

        #region Pulse
        //private static Stopwatch _check = new Stopwatch();
        public override void Pulse()
        {

            if (!StyxWoW.IsInGame || !StyxWoW.IsInWorld || Me == null || !Me.IsValid || !Me.IsAlive || Me.Mounted || HK.manualOn)
                return;
            if (!Me.Combat)
                return;

            // if we have an oppsing faction, then clear the target.
            if (U.isOpposingFaction(Me.CurrentTarget) && IS.GeneralIgnoreOpposingFaction)
            {
                Me.ClearTarget();
            }
             
            // cache a few things like my fury/pain
            C.Cache();

            // grab all enemies within 50 yards to find a valid target
            U.enemyAnnex(50f);

            // make sure we aren't using fel devastation or something before we try to grab targets for no good reason.
            if (!Me.IsCasting || !Me.IsChanneling)
            {
                // grab an interrupt target
                if (GetInterruptTarget()) { return; }

                // get a taunt target, if any
                if (GetTauntTarget()) { return; }

                // get a new target, if any
                if (GetNewTarget()) { return; }
            }

        }

        private bool GetInterruptTarget()
        {
            if (Me.Specialization == WoWSpec.DemonHunterVengeance && IS.VengeanceAllowInterrupt && !HK.RotationOnlyOn)
            {
                // consume magic
                if (IS.VengeanceAllowInterruptConsumeMagic && !Core.Spell.OnCooldown(Core.Helpers.Spell_Book.ConsumeMagic))
                {
                    var units = U.activeEnemies(Me.Location, 20f); // get all enemies within 20 yards
                    if (units != null)
                    {
                        var interruptTarget = units.Where(u => (u.IsCasting || u.IsCastingHealingSpell) && u.CanInterruptCurrentSpellCast).OrderBy(d => d.Distance).FirstOrDefault();
                        if (interruptTarget != null)
                        {
                            interruptTarget.Target();
                            return true;
                        }
                    }
                }


            }
            return false;
        }

        private bool GetTauntTarget()
        {
            if (Me.Specialization == WoWSpec.DemonHunterVengeance && IS.VengeanceAllowTaunt && !HK.RotationOnlyOn)
            {
                U.enemiesToTauntAnnex(50f);

                if (U.activeEnemiesToTaunt(Me.Location, 40f).Any())
                {
                    IEnumerable<WoWUnit> tauntEnemies = U.activeEnemiesToTaunt(Me.Location, 40f);
                    if (tauntEnemies != null)
                    {
                        var tauntEnemy = tauntEnemies.OrderBy(u => u.Distance).FirstOrDefault();
                        if (tauntEnemy != null)
                        {
                            L.pullMobLog(string.Format($"Switch taunt target to {tauntEnemy.SafeName} at {tauntEnemy.Distance.ToString("F0")} yds @ {tauntEnemy.HealthPercent.ToString("F0")}% HP"), Core.Helpers.Common.TargetColor);
                            tauntEnemy.Target();
                            return true;
                        }

                    }
                }
            }
            return false;
        }

        private bool GetNewTarget()
        {
            if (!Me.CurrentTarget.IsValidCombatUnit() && !HK.RotationOnlyOn)
            {
                var newUnits = U.activeEnemies(Me.Location, 50f).OrderBy(u => u.Distance).ThenBy(u => u.HealthPercent);
                L.debugLog("Number of new units: " + newUnits.Count());
                if (newUnits != null)
                {
                    var newUnit = newUnits.FirstOrDefault();
                    if (newUnit != null)
                    {
                        L.pullMobLog(string.Format($"Switch target to {newUnit.SafeName} at {newUnit.Distance.ToString("F0")} yds @ {newUnit.HealthPercent.ToString("F0")}% HP"), Core.Helpers.Common.TargetColor);
                        newUnit.Target();
                        return true;
                    }
                }
            }
            return false;
        }

        #endregion

        #endregion
    }
}
