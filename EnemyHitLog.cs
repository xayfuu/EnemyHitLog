using RoR2;
using BepInEx;
using UnityEngine;
using System.Text.RegularExpressions;
using System.Collections.Generic;

namespace EnemyHitLog
{
    [BepInDependency("com.bepis.r2api")]
    [BepInPlugin("com.Xay.EnemyHitLog", "EnemyHitLog", "0.1.0")]
    public class EnemyHitLog : BaseUnityPlugin
    {
        private static readonly Regex NoWhitespace = new Regex(@"\s+");
        public static readonly string DefaultHighlightColor = "#b3b3b3";

        // - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - 
        public void Awake()
        {
            On.RoR2.GlobalEventManager.ServerDamageDealt += Event_ServerDamageDealt;
        }

        // - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - 
        private void Event_ServerDamageDealt(On.RoR2.GlobalEventManager.orig_ServerDamageDealt orig, DamageReport damageReport)
        {
            orig(damageReport);

            if (!damageReport.victimBody || !damageReport.attackerBody)
                return;

            CharacterBody victim = damageReport.victimBody;
            CharacterBody attacker = damageReport.attackerBody;
            string enemyLabel;
            string victimLabel;

            if (VictimIsPlayerLike(damageReport))
            {
                float damageTaken = Mathf.Round(damageReport.damageDealt);
                if (damageTaken == 0)
                    return;

                if (!VictimIsRealPlayer(victim))
                {
                    CharacterMaster minionOwner = TryResolveMinionOwnerMaster(damageReport.victimMaster);
                    if (minionOwner == null)
                        return;

                    victimLabel = ComposeMinionVictimLabel(victim, minionOwner.GetBody());
                }
                else
                {
                    victimLabel = ComposePlayerVictimLabel(victim);
                }

                if (damageReport.damageInfo.damageType > DamageType.Generic)
                    Debug.Log(damageReport.damageInfo.damageType);
                    // (Freeze2s, AOE) when Glacial Enemy explodes

                if (victim.HasBuff(BuffIndex.BeetleJuice))
                    Debug.Log("Victim has BeetleJuice Debuff");  // Debuff by Beetle Boss
                if (victim.HasBuff(BuffIndex.ClayGoo))
                    Debug.Log("Victim has ClayGoo Debuff");
                if (victim.HasBuff(BuffIndex.Cripple))
                    Debug.Log("Victim has Cripple Debuff");
                if (victim.HasBuff(BuffIndex.HealingDisabled))
                    Debug.Log("Victim has HealingDisabled Debuff");
                if (victim.HasBuff(BuffIndex.OnFire))           // Continuous Fire Damage Logs On Hit by Fire Enemy
                    Debug.Log("Victim has OnFire Debuff");
                if (victim.HasBuff(BuffIndex.Poisoned))
                    Debug.Log("Victim has Poisoned Debuff");
                if (victim.HasBuff(BuffIndex.Slow30))
                    Debug.Log("Victim has Slow30 Debuff");
                if (victim.HasBuff(BuffIndex.Slow50))
                    Debug.Log("Victim has Slow50 Debuff");
                if (victim.HasBuff(BuffIndex.Slow60))
                    Debug.Log("Victim has Slow60 Debuff");
                if (victim.HasBuff(BuffIndex.Slow80))           // On Hit by Glacial Enemy
                    Debug.Log("Victim has Slow80 Debuff");
                if (victim.HasBuff(BuffIndex.Bleeding))
                    Debug.Log("Victim has Bleeding Debuff");    // Continuous Bleeding Damage On Hit by Imp

                BuffIndex enemyBuff = BuffIndex.None;
                foreach (BuffIndex buffIndex in BuffCatalog.eliteBuffIndices)
                {
                    if (damageReport.attackerBody.HasBuff(buffIndex))
                    {
                        enemyBuff = buffIndex;
                        break;
                    }
                }
                enemyLabel = ComposeAttackerLabel(attacker, enemyBuff);
                Chat.SendBroadcastChat(ComposeNewHitInfoMessage(victim, enemyLabel, victimLabel, damageTaken));
            }
        }

        // - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - 
        private bool VictimIsPlayerLike(DamageReport dmgReport) => dmgReport.victimTeamIndex == TeamIndex.Player;
        private bool VictimIsRealPlayer(CharacterBody victimBody) => victimBody.isPlayerControlled;

        // - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - 
        private CharacterMaster TryResolveMinionOwnerMaster(CharacterMaster minionMaster)
        {
            foreach (PlayerCharacterMasterController pcm in PlayerCharacterMasterController.instances)
            {
                if (pcm.master.netId == minionMaster.minionOwnership.NetworkownerMasterId)
                    return pcm.master;
            }
            return null;
        }

        // - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - 
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

        // - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - 
        private string ComposePlayerVictimLabel(CharacterBody player)
        {
            CharacterData characterData = DataCatalog.GetCharacterDataFor(player.GetDisplayName());
            return $"<color={characterData.ColorHex}>{player.GetUserName()}</color> [<color={characterData.ColorHex}>{characterData.TruncatedDisplayName}</color>]";
        }

        // - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - 
        private string ComposeMinionVictimLabel(CharacterBody minion, CharacterBody minionOwner)
        {
            CharacterData minionOwnerCharacterData = DataCatalog.GetCharacterDataFor(minionOwner.GetDisplayName());
            return $"<color={minionOwnerCharacterData.ColorHex}>{minionOwner.GetUserName()}</color> [<color={DefaultHighlightColor}>{TruncateMinionClassDisplayName(minion.GetDisplayName())}</color>]";
        }

        // - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - 
        private string __ComposeAttackerLabel(CharacterBody charBody, string hexColor, string affixLabel)
        {
            return $"<color={hexColor}>{(affixLabel == "" ? "" : affixLabel + " ")}{charBody.GetDisplayName()}</color>";
        }

        // - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - 
        private Chat.SimpleChatMessage ComposeNewHitInfoMessage(CharacterBody charBody, string enemyLabel, string victimLabel, float damageTaken)
        {
            return new Chat.SimpleChatMessage
            {
                baseToken = $"{victimLabel}: Hit by {enemyLabel} (<color=#ff4000>{damageTaken}</color> Damage)"
            };
        }

        // - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - 
        private string TruncateMinionClassDisplayName(string minionName)
        {
            Debug.Log("TruncateMinionClassDisplayName: " + minionName);

            if (minionName.EndsWith("Turret"))
            {
                if (minionName == "Engineer Turret")
                    return "EngiTurret";
                else if (minionName == "Gunner Turret")
                    return "GunTurret";
            }
            else if (minionName.EndsWith("Drone"))
            {
                switch (minionName)
                {
                    case "Healing Drone":
                        return "HealDrone";

                    case "Gunner Drone":
                        return "GunDrone";

                    case "Equipment Drone":
                        return "EquipDrone";

                    default:
                        return NoWhitespace.Replace(minionName, "");
                }
            }
            else if (minionName == "TC-280 Prototype")
            {
                return "TC-280";
            }
            return NoWhitespace.Replace(minionName, "");
        }
    }

    // - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - 
    public enum CharacterClassesIndex
    {
        None = -1,
        Acrid,
        Artificer,
        Commando,
        Engineer,
        Huntress,
        Loader,
        Mercenary,
        Mult,
        Rex
    }

    // - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - 
    public static class DataCatalog
    {
        public static List<CharacterData> Characters = new List<CharacterData>()
        {
            { new CharacterData("Acrid", "Acrid", "#85ff33") },
            { new CharacterData("Artificer", "Artificer", "#f7c1fd") },
            { new CharacterData("Commando", "Commando", "#ed9616") },
            { new CharacterData("Engineer", "Engi", "#5fe286") },
            { new CharacterData("Huntress", "Huntress", "#d53d3d") },
            { new CharacterData("Loader", "Loader", "#35a7ff") },
            { new CharacterData("Mercenary", "Merc", "#6cd1ea") },
            { new CharacterData("MUL-T", "MUL-T>", "#d3c44f") },
            { new CharacterData("Rex", "Rex", "#408000") }
        };

        // - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - 
        public static CharacterData GetCharacterDataFor(string characterDisplayName)
        {
            CharacterClassesIndex character = MapDisplayNameToCharacterClassIndex(characterDisplayName);

            if (character == CharacterClassesIndex.None)
                return new CharacterData(characterDisplayName, characterDisplayName, EnemyHitLog.DefaultHighlightColor);

            return Characters[(int)character];
        }

        // - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - 
        public static CharacterClassesIndex MapDisplayNameToCharacterClassIndex(string characterDisplayName)
        {
            switch (characterDisplayName.ToLower())
            {
                case "acrid":
                    return CharacterClassesIndex.Acrid;
                case "artificer":
                    return CharacterClassesIndex.Artificer;
                case "commando":
                    return CharacterClassesIndex.Commando;
                case "engineer":
                    return CharacterClassesIndex.Engineer;
                case "huntress":
                    return CharacterClassesIndex.Huntress;
                case "loader":
                    return CharacterClassesIndex.Loader;
                case "mercenary":
                    return CharacterClassesIndex.Mercenary;
                case "mul-t":
                    return CharacterClassesIndex.Mult;
                case "rex":
                    return CharacterClassesIndex.Rex;
                default:
                    return CharacterClassesIndex.None;
            }
        }
    }

    // - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - 
    public struct CharacterData
    {
        public string DisplayName;
        public string TruncatedDisplayName;
        public string ColorHex;

        public CharacterData(string displayName, string truncatedDisplayName, string colorHex)
        {
            DisplayName = displayName;
            TruncatedDisplayName = truncatedDisplayName;
            ColorHex = colorHex;
        }
    }
}
