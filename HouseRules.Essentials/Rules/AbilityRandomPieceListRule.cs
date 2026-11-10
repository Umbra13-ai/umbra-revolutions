namespace HouseRules.Essentials.Rules
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Boardgame;
    using Boardgame.BoardEntities;
    using Boardgame.BoardEntities.Abilities;
    using DataKeys;
    using HarmonyLib;
    using HouseRules.Core;
    using HouseRules.Core.Types;
    using UnityEngine;
    using UnityEngine.AddressableAssets;

    public sealed class AbilityRandomPieceListRule : Rule, IConfigWritable<Dictionary<AbilityKey, List<BoardPieceId>>>, IPatchable, IMultiplayerSafe
    {
        public override string Description => "Random summon abilities are adjusted";

        private static bool _isActivated;
        private static bool _isRatSubstituted;
        private static AbilityFactory? _abilityFactory;
        private static PieceConfigData? _poisonousRatConfig;

        private readonly Dictionary<AbilityKey, List<BoardPieceId>> _adjustments;
        private Dictionary<AbilityKey, List<BoardPieceId>> _originals;

        /// <summary>
        /// Initializes a new instance of the <see cref="AbilityRandomPieceListRule"/> class.
        /// </summary>
        /// <param name="adjustments">Dict of ability name string and list of BoardPieceIDs.</param>
        /// <remarks>Replaces the list of pieces that certain abilities will spawn (e.g. RatWhisperer, SpawnCultists etc).</remarks>
        public AbilityRandomPieceListRule(Dictionary<AbilityKey, List<BoardPieceId>> adjustments)
        {
            _adjustments = adjustments;
            _originals = new Dictionary<AbilityKey, List<BoardPieceId>>();
        }

        public Dictionary<AbilityKey, List<BoardPieceId>> GetConfigObject() => _adjustments;

        protected override void OnActivate(Context context)
        {
            _isActivated = true;
            _isRatSubstituted = false;
            _poisonousRatConfig = null;
            _abilityFactory = context.AbilityFactory;
        }

        protected override void OnPreGameCreated(Context context)
        {
            _originals = ReplaceAbilities(context, _adjustments);
        }

        protected override void OnDeactivate(Context context)
        {
            _isActivated = false;
            _isRatSubstituted = false;
            _poisonousRatConfig = null;
            ReplaceAbilities(context, _originals);
        }

        private static void Patch(Harmony harmony)
        {
            harmony.Patch(
                original: AccessTools.Method(typeof(Piece), "CreatePiece"),
                prefix: new HarmonyMethod(
                    typeof(AbilityRandomPieceListRule),
                    nameof(Piece_CreatePiece_Prefix)));
        }

        /// <summary>
        /// Gives party-team rats the stats and abilities the ruleset defined for the unavailable PoisonousRat.
        /// </summary>
        /// <remarks>
        /// The game has no PoisonousRat model, so summons of it are replaced by a plain Rat. A copy of the Rat
        /// config carrying the PoisonousRat settings is used only for those rats, leaving every other Rat unchanged.
        /// </remarks>
        private static void Piece_CreatePiece_Prefix(ref PieceConfigData config, BoardPieceId pieceType, Team team)
        {
            if (!_isActivated || !_isRatSubstituted || pieceType != BoardPieceId.Rat || team != Team.One)
            {
                return;
            }

            _poisonousRatConfig ??= CreatePoisonousRatConfig(config);
            if (_poisonousRatConfig != null)
            {
                config = _poisonousRatConfig;
            }
        }

        private static PieceConfigData? CreatePoisonousRatConfig(PieceConfigData ratConfig)
        {
            try
            {
                var gameContext = Traverse.Create(typeof(GameHub)).Field<GameContext>("gameContext").Value;
                if (_abilityFactory == null ||
                    !gameContext.gameDataAPI.GetPieceConfig(BoardPieceId.PoisonousRat, MotherbrainGlobalVars.CurrentConfig, out var source))
                {
                    return null;
                }

                var copy = UnityEngine.Object.Instantiate(ratConfig);
                copy.name = "PoisonousRat (Rat)";
                copy.StartHealth = source.StartHealth;
                copy.StartArmor = source.StartArmor;
                copy.AttackDamage = source.AttackDamage;
                copy.MoveRange = source.MoveRange;
                copy.ActionPoint = source.ActionPoint;
                copy.CriticalHitChance = source.CriticalHitChance;
                copy.CriticalHitDamage = source.CriticalHitDamage;
                copy.ImmuneToStatusEffects = source.ImmuneToStatusEffects.ToArray();
                copy.Abilities = source.Abilities.ToArray();
                copy.AbilityPromises = _abilityFactory.LoadAbilities(copy.Abilities);
                return copy;
            }
            catch (Exception e)
            {
                HouseRulesEssentialsBase.LogWarning($"Could not build the poisonous Rat config; summoned Rats stay plain: {e.Message}");
                return null;
            }
        }

        private static Dictionary<AbilityKey, List<BoardPieceId>> ReplaceAbilities(
            Context context,
            Dictionary<AbilityKey, List<BoardPieceId>> replacements)
        {
            var originals = new Dictionary<AbilityKey, List<BoardPieceId>>();

            foreach (var replacement in replacements)
            {
                var abilityPromise = context.AbilityFactory.LoadAbility(replacement.Key);
                abilityPromise.OnLoaded(ability =>
                {
                    var availablePieces = new List<BoardPieceId>();
                    foreach (var pieceId in replacement.Value)
                    {
                        if (IsSpawnable(pieceId))
                        {
                            availablePieces.Add(pieceId);
                        }
                        else if (pieceId == BoardPieceId.PoisonousRat && IsSpawnable(BoardPieceId.Rat))
                        {
                            HouseRulesEssentialsBase.LogWarning($"Using Rat in place of unavailable PoisonousRat for {replacement.Key}.");
                            availablePieces.Add(BoardPieceId.Rat);
                            _isRatSubstituted = true;
                        }
                        else
                        {
                            HouseRulesEssentialsBase.LogWarning($"Skipping unavailable {pieceId} in {replacement.Key}.");
                        }
                    }

                    if (availablePieces.Count == 0)
                    {
                        HouseRulesEssentialsBase.LogWarning($"No available summon pieces configured for {replacement.Key}; keeping its original choices.");
                        return;
                    }

                    originals[replacement.Key] = ability.randomPieceList.ToList();
                    ability.randomPieceList = availablePieces.ToArray();
                });
            }

            // Theoretically, there can be a race condition as there's no guarantee the promise above is fulfilled by
            // the return value is used. Realistically, there's no concern since the value isn't used until after the
            // promise has long been fulfilled.
            return originals;
        }

        private static bool IsSpawnable(BoardPieceId pieceId)
        {
            var lowQuality = BoardPieceRegistry.Instance != null &&
                Traverse.Create(BoardPieceRegistry.Instance).Field<bool>("spawnLQAddressables").Value;
            var assetKey = pieceId.ToString().ToLowerInvariant() + (lowQuality ? "LQ" : string.Empty);
            foreach (var locator in Addressables.ResourceLocators)
            {
                if (locator.Locate(assetKey, typeof(GameObject), out var locations) && locations != null && locations.Count > 0)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
