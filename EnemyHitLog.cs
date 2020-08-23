using RoR2;
using BepInEx;
using BepInEx.Configuration;

namespace EnemyHitLog
{
    [BepInDependency("com.bepis.r2api")]
    [BepInPlugin("com.Xay.EnemyHitLog", "EnemyHitLog", "0.3.0")]
    public class EnemyHitLog : BaseUnityPlugin
    {
        public static readonly string DefaultHighlightColor = "#b3b3b3";
        public TickDebuffHandler tickDebuffHandler;

        public static ConfigEntry<bool> ConfigLogPlayers;
        public static ConfigEntry<bool> ConfigLogAllies;
        public static ConfigEntry<bool> ConfigLogUtility;
        public static ConfigEntry<bool> ConfigLogDebuff;
        public static ConfigEntry<bool> ConfigLogFallDamage;
        public static ConfigEntry<bool> ConfigLogShrinesOfBlood;
        public static ConfigEntry<int> ConfigHpPercentageFilter;

        public void Awake()
        {
            ConfigLogPlayers = Config.Bind(
                "EnemyHitLog.Toggles", 
                "Player",
                true, 
                "Whether or not to log Players\n"
            );
            ConfigLogAllies = Config.Bind(
                "EnemyHitLog.Toggles", 
                "Ally", 
                false, "Whether or not to log Allies, like Engineer Turrets, Beetle Guards or Aurelionite.\n"
            );
            ConfigLogUtility = Config.Bind(
                "EnemyHitLog.Toggles", 
                "Utility", 
                false, 
                "Whether or not to log Drones and Turrets that were bought during a run.\n"
            );
            ConfigLogDebuff = Config.Bind(
                "EnemyHitLog.Toggles", 
                "Debuffs", 
                true, 
                "Whether or not to log Debuffs.\n"
            );
            ConfigLogFallDamage = Config.Bind(
                "EnemyHitLog.Toggles",
                "FallDamage",
                true,
                "Whether or not to log FallDamage."
            );
            ConfigLogShrinesOfBlood = Config.Bind(
                "EnemyHitLog.Toggles",
                "ShrinesOfBlood",
                false,
                "Whether or not to log sacrificed HP on a Shrine of Blood."
            );
            ConfigHpPercentageFilter = Config.Bind(
                "EnemyHitLog.Filter", 
                "DamageToMaxHealthThreshold", 
                5, 
                "Do not log any damage which has a lower value than the given percentage of the Player's HP.\n\nFor example, if this variable is 10, only damage as high as at least 10% of the Player's max. HP (not counting barrier and shield) will be logged to the chat.\n\nNote: Splash damage results in being ignored as well, since each splash is a separate Hit á e.g. 5 Damage. Hopefully I will find the time to try to fix this in the future...\n"
            );

            if (!AtLeastOnePlayerTeamLogEnabled())
                return;

            if (ConfigHpPercentageFilter.Value < 0)
                ConfigHpPercentageFilter.Value = 0;

            tickDebuffHandler = new TickDebuffHandler();

            On.RoR2.GlobalEventManager.ServerDamageDealt += Event_ServerDamageDealt;
            UnityEngine.SceneManagement.SceneManager.activeSceneChanged += SceneManager_activeSceneChanged;
        }

        private void SceneManager_activeSceneChanged(UnityEngine.SceneManagement.Scene arg0, UnityEngine.SceneManagement.Scene arg1)
        {
            tickDebuffHandler.ClearCache();
        }

        private void Event_ServerDamageDealt(On.RoR2.GlobalEventManager.orig_ServerDamageDealt orig, DamageReport damageReport)
        {
            orig(damageReport);

            DamageReportHandler damageReportHandler = new DamageReportHandler(damageReport);
            Chat.SimpleChatMessage chatMessage = null;
            int hitPointPercentage = ConfigHpPercentageFilter.Value;

            string teamEntityLabel;
            string enemyEntityLabel;

            if (!damageReportHandler.VictimIsInPlayerTeam() || damageReport.damageInfo.rejected)
                return;

            teamEntityLabel = GetTeamEntityLabel(damageReportHandler.Victim);
            if (teamEntityLabel == null)
                return;

        // Fall Damage
            if (damageReportHandler.CheckIfFallDamageBroadcast(hitPointPercentage)
                && AtLeastOnePlayerTeamLogEnabled()
                && LogFallDamage())
            {
                chatMessage = ComposeNewFallDamageMessage(teamEntityLabel, damageReportHandler.Damage);
            }
        // Shrines of Blood
            else if (damageReportHandler.CheckIfShrineBloodDamageBroadcast(hitPointPercentage)
                && AtLeastOnePlayerTeamLogEnabled()
                && LogShrinesOfBlood())
            {
                chatMessage = ComposeNewShrineOfBloodMessage(teamEntityLabel, damageReportHandler.Damage);
            }
        // DoT Debuffs
            else if (tickDebuffHandler.IsTickDamageEvent(damageReportHandler)
                && AtLeastOnePlayerTeamLogEnabled()
                && LogDebuffs())
            {
                enemyEntityLabel = tickDebuffHandler.ComposeLabel(damageReportHandler.TickingDebuffIndex);
                if (enemyEntityLabel == null)
                    return;
                chatMessage = ComposeNewDebuffInfoMessage(enemyEntityLabel, teamEntityLabel);
            }
        // Normal Damage
            else if (damageReportHandler.CheckIfDamageBroadcast(hitPointPercentage)
                && damageReportHandler.Attacker != null
                && AtLeastOnePlayerTeamLogEnabled())
            {
                BuffIndex attackerBuff = GetAttackerEliteBuff(damageReportHandler.Attacker);
                enemyEntityLabel = ComposeEnemyEntitiyLabel(damageReportHandler, attackerBuff);
                if (enemyEntityLabel == null)
                    return;
                chatMessage = ComposeNewHitInfoMessage(enemyEntityLabel, teamEntityLabel, damageReportHandler.Damage);
            }
            
            if (chatMessage != null)
                Chat.SendBroadcastChat(chatMessage);
        }

        private string GetTeamEntityLabel(CharacterBody teamEntity)
        {
            string victimLabel;
            if (!IsRealPlayer(teamEntity))
            {
                if (MinionTogglesDisabled())
                    return null;

                CharacterMaster minionOwner = TryResolveMinionOwnerMaster(teamEntity);
                if (minionOwner == null)
                    return null;

                victimLabel = ComposeMinionVictimLabel(teamEntity, minionOwner.GetBody());
            }
            else
            {
                victimLabel = ComposePlayerVictimLabel(teamEntity);
            }

            return victimLabel;
        }

        private BuffIndex GetAttackerEliteBuff(CharacterBody attacker)
        {
            foreach (BuffIndex buffIndex in BuffCatalog.eliteBuffIndices)
            {
                if (attacker.HasBuff(buffIndex))
                    return buffIndex;
            }
            return BuffIndex.None;
        }

        private string ComposeEnemyEntitiyLabel(DamageReportHandler damageReportHandler, BuffIndex buffIndex)
        {
            switch (buffIndex)
            {
                case BuffIndex.AffixBlue:
                    return __ComposeEnemyEntitiyLabel(damageReportHandler, "#0066cc", "Overloading");
                case BuffIndex.AffixRed:
                    return __ComposeEnemyEntitiyLabel(damageReportHandler, "#b30000", "Blazing");
                case BuffIndex.AffixHaunted:
                    return __ComposeEnemyEntitiyLabel(damageReportHandler, "#99ffbb", "Celestine");
                case BuffIndex.AffixPoison:
                    return __ComposeEnemyEntitiyLabel(damageReportHandler, "#008000", "Malachite");
                case BuffIndex.AffixWhite:
                    return __ComposeEnemyEntitiyLabel(damageReportHandler, "#98e4ed", "Glacial");
                default:
                    return __ComposeEnemyEntitiyLabel(damageReportHandler, DefaultHighlightColor, "");
            }
        }

        private string __ComposeEnemyEntitiyLabel(DamageReportHandler damageReportHandler, string hexColor, string affixLabel)
        {
            string name = damageReportHandler.Attacker.GetDisplayName();

            if (damageReportHandler.AttackIsFriendlyFire() && IsRealPlayer(damageReportHandler.Attacker))
                name = damageReportHandler.Attacker.GetUserName();

            return $"<color={hexColor}>{(affixLabel == "" ? "" : affixLabel + " ")}{name}</color>";
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
                baseToken = $"{victimLabel}: {attackerLabel}"
            };
        }

        private Chat.SimpleChatMessage ComposeNewShrineOfBloodMessage(string victimLabel, int sacrificedHp)
        {
            return new Chat.SimpleChatMessage
            {
                baseToken = $"{victimLabel}: Sacrificed <color=#ff4000>{sacrificedHp}</color> HP"
            };
        }

        private Chat.SimpleChatMessage ComposeNewFallDamageMessage(string victimLabel, float damageTaken)
        {
            return new Chat.SimpleChatMessage
            {
                baseToken = $"{victimLabel}: Hit the ground (<color=#ff4000>{damageTaken}</color> Damage)"
            };
        }

        private bool LogDebuffs()
        {
            return ConfigLogDebuff.Value;
        }

        private bool LogFallDamage()
        {
            return ConfigLogFallDamage.Value;
        }

        private bool LogShrinesOfBlood()
        {
            return ConfigLogShrinesOfBlood.Value;
        }

        private bool LogPlayers()
        {
            return ConfigLogPlayers.Value;
        }

        private bool LogAllies()
        {
            return ConfigLogAllies.Value;
        }

        private bool LogUtility()
        {
            return ConfigLogUtility.Value;
        }

        private bool AtLeastOnePlayerTeamLogEnabled()
        {
            return LogPlayers() || LogAllies() || LogUtility();
        }

        private bool MinionTogglesDisabled()
        {
            return !ConfigLogAllies.Value && !ConfigLogUtility.Value;
        }

        public CharacterMaster TryResolveMinionOwnerMaster(CharacterBody cb)
        {
            foreach (PlayerCharacterMasterController pcm in PlayerCharacterMasterController.instances)
            {
                if (pcm.master.netId == cb.master.minionOwnership.NetworkownerMasterId)
                    return pcm.master;
            }
            return null;
        }

        public bool IsRealPlayer(CharacterBody cb)
        {
            return cb.isPlayerControlled;
        }
    }
}
