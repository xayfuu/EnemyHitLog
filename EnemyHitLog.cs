using RoR2;
using BepInEx;
using BepInEx.Configuration;
using UnityEngine;
using System.Text.RegularExpressions;
using System.Collections.Generic;

namespace EnemyHitLog
{
    [BepInDependency("com.bepis.r2api")]
    [BepInPlugin("com.Xay.EnemyHitLog", "EnemyHitLog", "0.1.0")]
    public class EnemyHitLog : BaseUnityPlugin
    {
        public static readonly Regex NoWhitespace = new Regex(@"\s+");
        public static readonly string DefaultHighlightColor = "#b3b3b3";

        public static ConfigEntry<bool> ConfigLogPlayers;
        public static ConfigEntry<bool> ConfigLogAllies;
        public static ConfigEntry<bool> ConfigLogUtility;

        public static Dictionary<uint, List<DotController.DotIndex>> VictimsDebuffCache = new Dictionary<uint, List<DotController.DotIndex>>();

        public void Awake()
        {
            ConfigLogPlayers = Config.Bind("EnemyHitLog.Toggles", "Player", true, "Whether or not to log Players");
            ConfigLogAllies = Config.Bind("EnemyHitLog.Toggles", "Ally", false, "Whether or not to log Allies, like Engineer Turrets, Beetle Guards or Aurelionite");
            ConfigLogUtility = Config.Bind("EnemyHitLog.Toggles", "Utility", false, "Whether or not to log Drones and Turrets that were bought during a run");

            if (AllTogglesDisabled())
                return;

            On.RoR2.GlobalEventManager.ServerDamageDealt += Event_ServerDamageDealt;
            UnityEngine.SceneManagement.SceneManager.activeSceneChanged += SceneManager_activeSceneChanged;
        }

        private void SceneManager_activeSceneChanged(UnityEngine.SceneManagement.Scene arg0, UnityEngine.SceneManagement.Scene arg1)
        {
            Debug.Log("Scene Changed, clearing VictimsDebuffCache");
            VictimsDebuffCache.Clear();
        }

        private void Event_ServerDamageDealt(On.RoR2.GlobalEventManager.orig_ServerDamageDealt orig, DamageReport damageReport)
        {
            orig(damageReport);

            if (!damageReport.victimBody || !damageReport.attackerBody)
                return;

            CharacterBody victim = damageReport.victimBody;
            CharacterBody attacker = damageReport.attackerBody;
            string attackerLabel;
            string victimLabel;
            DotController.DotIndex debuffFirstTick = DotController.DotIndex.None;

            if (VictimIsPlayerLike(damageReport))
            {
                EnsureVictimIsCachedAndAlive(victim);

                if (VictimsDebuffCache.ContainsKey(GetVictimNetworkUserId(victim)))
                {
                    if (damageReport.damageInfo.dotIndex == DotController.DotIndex.PercentBurn)
                    {
                        if (VictimHasDebuff(victim, DotController.DotIndex.PercentBurn))
                            return;
                        debuffFirstTick = DotController.DotIndex.PercentBurn;
                        AddVictimDebuff(victim, DotController.DotIndex.PercentBurn);
                    }
                    else
                        if (!victim.HasBuff(BuffIndex.OnFire))
                            RemoveVictimDebuff(victim, DotController.DotIndex.PercentBurn);

                    if (damageReport.damageInfo.dotIndex == DotController.DotIndex.Bleed)
                    {
                        if (VictimHasDebuff(victim, DotController.DotIndex.Bleed))
                            return;
                        debuffFirstTick = DotController.DotIndex.Bleed;
                        AddVictimDebuff(victim, DotController.DotIndex.Bleed);
                    }
                    else
                        if (!victim.HasBuff(BuffIndex.Bleeding))
                            RemoveVictimDebuff(victim, DotController.DotIndex.Bleed);
                }


                float damageTaken = Mathf.Round(damageReport.damageDealt);
                if (damageTaken == 0)
                    return;


                if (!VictimIsRealPlayer(victim))
                {
                    if (MinionTogglesDisabled())
                        return;

                    CharacterMaster minionOwner = TryResolveMinionOwnerMaster(damageReport.victimMaster);
                    if (minionOwner == null)
                        return;

                    victimLabel = ComposeMinionVictimLabel(victim, minionOwner.GetBody());
                }
                else
                    victimLabel = ComposePlayerVictimLabel(victim);


                if (victimLabel == null)
                    return;


                if (debuffFirstTick != DotController.DotIndex.None)
                {
                    attackerLabel = ComposeDebuffLabel(debuffFirstTick);
                    if (attackerLabel == null)
                        return;

                    Chat.SendBroadcastChat(ComposeNewDebuffInfoMessage(attackerLabel, victimLabel));
                }
                else
                {
                    BuffIndex attackerBuff = BuffIndex.None;
                    foreach (BuffIndex buffIndex in BuffCatalog.eliteBuffIndices)
                    {
                        if (damageReport.attackerBody.HasBuff(buffIndex))
                        {
                            attackerBuff = buffIndex;
                            break;
                        }
                    }
                    attackerLabel = ComposeAttackerLabel(attacker, attackerBuff);
                    Chat.SendBroadcastChat(ComposeNewHitInfoMessage(attackerLabel, victimLabel, damageTaken));
                }
            }
        }

        private bool VictimIsPlayerLike(DamageReport dmgReport) => dmgReport.victimTeamIndex == TeamIndex.Player;
        private bool VictimIsRealPlayer(CharacterBody victimBody) => victimBody.isPlayerControlled;
        private bool AllTogglesDisabled() => !ConfigLogPlayers.Value && !ConfigLogAllies.Value && !ConfigLogUtility.Value;
        private bool MinionTogglesDisabled() => !ConfigLogAllies.Value && !ConfigLogUtility.Value;
        private bool VictimHasDebuff(CharacterBody victim, DotController.DotIndex debuffIndex) => VictimsDebuffCache[GetVictimNetworkUserId(victim)].IndexOf(debuffIndex) != -1;
        private uint GetVictimNetworkUserId(CharacterBody victim) => victim.networkIdentity.netId.Value;

        private void AddVictimDebuff(CharacterBody victim, DotController.DotIndex debuffIndex)
        {
            int listIdx = VictimsDebuffCache[GetVictimNetworkUserId(victim)].IndexOf(debuffIndex);
            if (listIdx != -1)
                return;
            VictimsDebuffCache[GetVictimNetworkUserId(victim)].Add(debuffIndex);
        }

        private void RemoveVictimDebuff(CharacterBody victim, DotController.DotIndex debuffIndex)
        {
            int listIdx = VictimsDebuffCache[GetVictimNetworkUserId(victim)].IndexOf(debuffIndex);
            if (listIdx == -1)
                return;
            VictimsDebuffCache[GetVictimNetworkUserId(victim)].RemoveAt(listIdx);
        }

        private void EnsureVictimIsCachedAndAlive(CharacterBody victim)
        {
            if (victim.master.alive && victim.isActiveAndEnabled)
            {
                if (!VictimsDebuffCache.ContainsKey(GetVictimNetworkUserId(victim)))
                    VictimsDebuffCache.Add(GetVictimNetworkUserId(victim), new List<DotController.DotIndex>());
            }
            else
            {
                if (VictimsDebuffCache.ContainsKey(GetVictimNetworkUserId(victim)))
                    VictimsDebuffCache.Remove(GetVictimNetworkUserId(victim));
            }
        }

        private CharacterMaster TryResolveMinionOwnerMaster(CharacterMaster minionMaster)
        {
            foreach (PlayerCharacterMasterController pcm in PlayerCharacterMasterController.instances)
            {
                if (pcm.master.netId == minionMaster.minionOwnership.NetworkownerMasterId)
                    return pcm.master;
            }
            return null;
        }

        private string ComposeAttackerLabel(CharacterBody attacker, BuffIndex buffIndex)
        {
            switch (buffIndex)
            {
                case BuffIndex.AffixBlue:
                    return __ComposeAttackerLabel(attacker, "#0066cc", "Overloading");
                case BuffIndex.AffixRed:
                    return __ComposeAttackerLabel(attacker, "#b30000", "Blazing");
                case BuffIndex.AffixHaunted:
                    return __ComposeAttackerLabel(attacker, "#99ffbb", "Celestine");
                case BuffIndex.AffixPoison:
                    return __ComposeAttackerLabel(attacker, "#008000", "Malachite");
                case BuffIndex.AffixWhite:
                    return __ComposeAttackerLabel(attacker, "#98e4ed", "Glacial");
                default:
                    return __ComposeAttackerLabel(attacker, DefaultHighlightColor, "");
            }
        }

        private string ComposeDebuffLabel(DotController.DotIndex debuffIndex)
        {
            if (debuffIndex == DotController.DotIndex.Bleed)
                return $"<color=#8a0303>Bleeding</color>";
            else if (debuffIndex == DotController.DotIndex.PercentBurn)
                return $"<color=#e25822>Burning</color>";
            return null;
        }

        private string ComposePlayerVictimLabel(CharacterBody player)
        {
            DataContainer characterData = DataCatalog.GetCharacterDataFor(player);

            if (!characterData.DataContainerTypeIsEnabled())
                return null;

            return $"<color={characterData.ColorHex}>{player.GetUserName()}</color> [<color={characterData.ColorHex}>{characterData.TruncatedDisplayName}</color>]";
        }

        private string ComposeMinionVictimLabel(CharacterBody minion, CharacterBody minionOwner)
        {
            DataContainer minionOwnerCharacterData = DataCatalog.GetCharacterDataFor(minionOwner);
            DataContainer minionData = DataCatalog.LookupMinionDataFor(minion);

            if (!minionData.DataContainerTypeIsEnabled())
                return null;

            return $"<color={minionOwnerCharacterData.ColorHex}>{minionOwner.GetUserName()}</color> [<color={minionData.ColorHex}>{minionData.TruncatedDisplayName}</color>]";
        }

        private string __ComposeAttackerLabel(CharacterBody charBody, string hexColor, string affixLabel)
        {
            return $"<color={hexColor}>{(affixLabel == "" ? "" : affixLabel + " ")}{charBody.GetDisplayName()}</color>";
        }

        private Chat.SimpleChatMessage ComposeNewHitInfoMessage(string attackerLabel, string victimLabel, float damageTaken)
        {
            return new Chat.SimpleChatMessage
            {
                baseToken = $"{victimLabel}: Hit by {attackerLabel} (<color=#ff4000>{damageTaken}</color> Damage)"
            };
        }

        private Chat.SimpleChatMessage ComposeNewDebuffInfoMessage(string attackerLabel, string victimLabel)
        {
            return new Chat.SimpleChatMessage
            {
                baseToken = $"{victimLabel}: Is {attackerLabel}!"
            };
        }
    }

    // - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - 
    public static class DataCatalog
    {
        private static Dictionary<string, DataContainer> Characters = new Dictionary<string, DataContainer>()
        {
            { "acrid",      new DataContainer("Acrid", "Acrid", "#85ff33", DataContainerType.Character) },
            { "artificer",  new DataContainer("Artificer", "Artificer", "#f7c1fd", DataContainerType.Character) },
            { "commando",   new DataContainer("Commando", "Commando", "#ed9616", DataContainerType.Character) },
            { "engineer",   new DataContainer("Engineer", "Engi", "#5fe286", DataContainerType.Character) },
            { "huntress",   new DataContainer("Huntress", "Huntress", "#d53d3d", DataContainerType.Character) },
            { "loader",     new DataContainer("Loader", "Loader", "#35a7ff", DataContainerType.Character) },
            { "mercenary",  new DataContainer("Mercenary", "Merc", "#6cd1ea", DataContainerType.Character) },
            { "mul-t",      new DataContainer("MUL-T", "MUL-T>", "#d3c44f", DataContainerType.Character) },
            { "rex",        new DataContainer("Rex", "Rex", "#408000", DataContainerType.Character) }
        };

        private static Dictionary<string, DataContainer> Utilities = new Dictionary<string, DataContainer>()
        {
            { "emergency drone",    new DataContainer("Emergency Drone", "EmergDrone", EnemyHitLog.DefaultHighlightColor, DataContainerType.Utility) },
            { "equipment drone",    new DataContainer("Equipment Drone", "EquipDrone", EnemyHitLog.DefaultHighlightColor, DataContainerType.Utility) },
            { "gunner drone",       new DataContainer("Gunner Drone", "GunDrone", EnemyHitLog.DefaultHighlightColor, DataContainerType.Utility) },
            { "gunner turret",      new DataContainer("Gunner Turret", "GunTurret", EnemyHitLog.DefaultHighlightColor, DataContainerType.Utility) },
            { "healing drone",      new DataContainer("Healing Drone", "HealDrone", EnemyHitLog.DefaultHighlightColor, DataContainerType.Utility) },
            { "incinerator drone",  new DataContainer("Incinerator Drone", "IncinDrone", EnemyHitLog.DefaultHighlightColor, DataContainerType.Utility) },
            { "missile drone",      new DataContainer("Missile Drone", "MissileDrone", EnemyHitLog.DefaultHighlightColor, DataContainerType.Utility) },
            { "tc-280 prototype",   new DataContainer("TC-280 Prototype", "TC-280", EnemyHitLog.DefaultHighlightColor, DataContainerType.Utility) }
        };

        private static Dictionary<string, DataContainer> Allies = new Dictionary<string, DataContainer>()
        {
            { "engineer turret",    new DataContainer("Engineer Turret", "EngiTurret", EnemyHitLog.DefaultHighlightColor, DataContainerType.Ally) },
            { "beetle guard",       new DataContainer("Beetle Guard", "BeetleGuard", EnemyHitLog.DefaultHighlightColor, DataContainerType.Ally) },
            { "aurelionite",        new DataContainer("Aurelionite", "Aurelionite", EnemyHitLog.DefaultHighlightColor, DataContainerType.Ally) }
        };

        public static DataContainer GetCharacterDataFor(CharacterBody character)
        {
            string characterDisplayName = character.GetDisplayName().ToLower();

            if (!Characters.ContainsKey(characterDisplayName))
                return new DataContainer(characterDisplayName, characterDisplayName, EnemyHitLog.DefaultHighlightColor, DataContainerType.Character);
            return Characters[characterDisplayName];
        }

        public static DataContainer LookupMinionDataFor(CharacterBody minion)
        {
            string minionDisplayName = minion.GetDisplayName().ToLower();

            if (Allies.ContainsKey(minionDisplayName))
                return Allies[minionDisplayName];

            if (Utilities.ContainsKey(minionDisplayName))
                return Utilities[minionDisplayName];

            return new DataContainer(minionDisplayName, minionDisplayName, EnemyHitLog.DefaultHighlightColor, DataContainerType.Unknown);
        }
    }

    public enum DataContainerType
    {
        Unknown = -1,
        Character,
        Ally,
        Utility
    }

    public struct DataContainer
    {
        public string DisplayName;
        public string TruncatedDisplayName;
        public string ColorHex;
        public DataContainerType DataContainerType;

        public DataContainer(string displayName, string truncatedDisplayName, string colorHex, DataContainerType dataContainerType)
        {
            DisplayName = displayName;
            TruncatedDisplayName = truncatedDisplayName;
            ColorHex = colorHex;
            DataContainerType = dataContainerType;
        }

        public bool DataContainerTypeIsEnabled()
        {
            return (
                (DataContainerType == DataContainerType.Ally && EnemyHitLog.ConfigLogAllies.Value)
                    || (DataContainerType == DataContainerType.Utility && EnemyHitLog.ConfigLogUtility.Value)
                    || (DataContainerType == DataContainerType.Character && EnemyHitLog.ConfigLogPlayers.Value)
                    || (DataContainerType == DataContainerType.Unknown)
            );
        }
    }
}
