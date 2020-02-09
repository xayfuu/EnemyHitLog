using RoR2;
using BepInEx;
using UnityEngine;

namespace EnemyHitLog
{
    [BepInDependency("com.bepis.r2api")]
    [BepInPlugin("com.Xay.EnemyHitLog", "EnemyHitLog", "0.1.0")]
    public class EnemyHitLog : BaseUnityPlugin
    {
        public void Awake()
        {
            On.RoR2.GlobalEventManager.ServerDamageDealt += Event_ServerDamageDealt;
        }

        private void Event_ServerDamageDealt(On.RoR2.GlobalEventManager.orig_ServerDamageDealt orig, DamageReport damageReport)
        {
            orig(damageReport);

            if (!damageReport.victim)
                return;

            if (damageReport.victim.body.isPlayerControlled)
            {
                string enemyLabel;

                if (damageReport.attackerBody != null)
                {
                    enemyLabel = $"<color=#b3b3b3>{damageReport.attackerBody.GetDisplayName()}</color>";
                    foreach (BuffIndex buffIndex in BuffCatalog.eliteBuffIndices)
                    {
                        if (damageReport.attackerBody.HasBuff(buffIndex))
                        {
                            switch (buffIndex)
                            {
                                case BuffIndex.AffixBlue:
                                    enemyLabel = $"<color=#0066cc>Overloading {damageReport.attackerBody.GetDisplayName()}</color>";
                                    break;
                                case BuffIndex.AffixRed:
                                    enemyLabel = $"<color=#b30000>Blazing {damageReport.attackerBody.GetDisplayName()}</color>";
                                    break;
                                case BuffIndex.AffixHaunted:
                                    enemyLabel = $"<color=#99ffbb>Celestine {damageReport.attackerBody.GetDisplayName()}</color>";
                                    break;
                                case BuffIndex.AffixPoison:
                                    enemyLabel = $"<color=#008000>Malachite {damageReport.attackerBody.GetDisplayName()}</color>";
                                    break;
                                case BuffIndex.AffixWhite:
                                    enemyLabel = $"<color=#98e4ed>Glacial {damageReport.attackerBody.GetDisplayName()}</color>";
                                    break;
                                default:
                                    break;
                            }
                            break;
                        }
                    }
                }
                else
                    enemyLabel = $"<color=#b3b3b3>{damageReport.damageInfo.inflictor.GetComponent<GenericSkill>().skillName}</color>";

                Chat.SendBroadcastChat(new Chat.SimpleChatMessage
                {
                    baseToken = $"<color=#04ff00>{damageReport.victim.body.GetUserName()}</color>: Hit by {enemyLabel} (<color=#ff4000>{Mathf.Round(damageReport.damageDealt)}</color> Damage)"
                });
            }
        }
    }
}
