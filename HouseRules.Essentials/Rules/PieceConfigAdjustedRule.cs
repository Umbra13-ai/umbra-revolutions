namespace HouseRules.Essentials.Rules
{
    using System;
    using System.Collections.Generic;
    using Boardgame;
    using DataKeys;
    using HarmonyLib;
    using HouseRules.Core;
    using HouseRules.Core.Types;

    public sealed class PieceConfigAdjustedRule : Rule, IConfigWritable<List<PieceConfigAdjustedRule.PieceProperty>>,
        IMultiplayerSafe, IDisableOnReconnect
    {
        public override string Description => "Some piece's base stats are adjusted";

        protected override SyncableTrigger ModifiedSyncables => SyncableTrigger.NewPieceModified;

        private readonly List<PieceProperty> _adjustments;
        private List<PieceProperty> _originals;

        public struct PieceProperty
        {
            public BoardPieceId Piece;
            public string Property;
            public float Value;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PieceConfigAdjustedRule"/> class.
        /// </summary>
        /// <param name="adjustments">List of <see cref="PieceProperty"/> that represent PieceConfig properties to replace.</param>
        public PieceConfigAdjustedRule(List<PieceProperty> adjustments)
        {
            _adjustments = adjustments;
            _originals = new List<PieceProperty>();
        }

        public List<PieceProperty> GetConfigObject() => _adjustments;

        protected override void OnPreGameCreated(Context context)
        {
            _originals = ReplaceExistingProperties(_adjustments);
        }

        protected override void OnDeactivate(Context context)
        {
            // Undo in reverse so a property adjusted more than once ends at its true original value.
            var toRestore = new List<PieceProperty>(_originals);
            toRestore.Reverse();
            ReplaceExistingProperties(toRestore, restoring: true);
        }

        /// <summary>
        /// Replaces existing PieceConfig properties with those specified.
        /// </summary>
        /// <returns>Previous properties that are now replaced.</returns>
        private static List<PieceProperty> ReplaceExistingProperties(List<PieceProperty> pieceConfigChanges, bool restoring = false)
        {
            var gameContext = Traverse.Create(typeof(GameHub)).Field<GameContext>("gameContext").Value;
            var previousProperties = new List<PieceProperty>();
            foreach (var item in pieceConfigChanges)
            {
                var pieceConfig = gameContext.gameDataAPI.PieceConfig[MotherbrainGlobalVars.CurrentConfig][item.Piece];
                if (item.Property == "PreciseHealth" || item.Property == "PreciseAttack")
                {
                    // The auto-balance mode is changed on the shared config, so the original mode is recorded
                    // (as the value) and put back when the rule deactivates.
                    var modeField = item.Property == "PreciseHealth" ? "healthAutoBalanceMode" : "attackDamageAutoBalanceMode";
                    previousProperties.Add(new PieceProperty
                    {
                        Piece = item.Piece,
                        Property = item.Property,
                        Value = AccessTools.FieldRefAccess<PieceConfigData, int>(pieceConfig, modeField),
                    });
                    AccessTools.FieldRefAccess<PieceConfigData, int>(pieceConfig, modeField) = restoring ? (int)item.Value : 1;
                    continue;
                }
                else
                {
                    previousProperties.Add(new PieceProperty
                    {
                        Piece = item.Piece,
                        Property = item.Property,
                        Value = Convert.ToSingle(Traverse.Create(pieceConfig).Field(item.Property).GetValue()),
                    });
                }

                // HouseRulesEssentialsBase.LogDebug($"Configs for {item.Piece}: {item.Property} - {Convert.ToSingle(Traverse.Create(pieceConfig).Field(item.Property).GetValue())}"); // Uncomment to see original Configs
                ModifyPieceConfig(ref pieceConfig, item.Property, item.Value);
                gameContext.gameDataAPI.PieceConfig[MotherbrainGlobalVars.CurrentConfig][item.Piece] = pieceConfig;
            }

            return previousProperties;
        }

        private static void ModifyPieceConfig(ref PieceConfigData pieceConfig, string property, float value)
        {
            var valueType = Traverse.Create(pieceConfig).Field(property).GetValueType();
            if (valueType == typeof(int))
            {
                AccessTools.FieldRefAccess<PieceConfigData, int>(pieceConfig, property) = (int)value;
                return;
            }

            if (valueType == typeof(float))
            {
                AccessTools.FieldRefAccess<PieceConfigData, float>(pieceConfig, property) = value;
                return;
            }

            throw new ArgumentException($"Can not support property of type: {valueType}");
        }
    }
}
