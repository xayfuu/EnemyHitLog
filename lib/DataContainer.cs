using RoR2;
using System.Collections.Generic;

namespace EnemyHitLog
{
    public static class DataCatalog
    {
        private static Dictionary<string, DataContainer> Characters = new Dictionary<string, DataContainer>()
    {
        { "acrid",      new DataContainer("Acrid", "Acrid", "#C9F24D", DataContainerType.Character) },
        { "artificer",  new DataContainer("Artificer", "Artificer", "#F7C1FD", DataContainerType.Character) },
        { "captain",    new DataContainer("Captain", "Captain", "#BEBA92", DataContainerType.Character) },
        { "commando",   new DataContainer("Commando", "Commando", "#ED9616", DataContainerType.Character) },
        { "engineer",   new DataContainer("Engineer", "Engi", "#5FE286", DataContainerType.Character) },
        { "huntress",   new DataContainer("Huntress", "Huntress", "#D53D3D", DataContainerType.Character) },
        { "loader",     new DataContainer("Loader", "Loader", "#6770DE", DataContainerType.Character) },
        { "mercenary",  new DataContainer("Mercenary", "Merc", "#6CD1EA", DataContainerType.Character) },
        { "mul-t",      new DataContainer("MUL-T", "MUL-T", "#D3C44F", DataContainerType.Character) },
        { "rex",        new DataContainer("Rex", "Rex", "#869E54", DataContainerType.Character) },
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

            return new DataContainer(minionDisplayName, Utils.ToCamelCase(minionDisplayName), EnemyHitLog.DefaultHighlightColor, DataContainerType.Unknown);
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
