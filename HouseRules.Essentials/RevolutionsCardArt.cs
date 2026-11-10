namespace HouseRules.Essentials
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using DataKeys;
    using UnityEngine.AddressableAssets;
    using UnityEngine.AddressableAssets.ResourceLocators;
    using UnityEngine.ResourceManagement.ResourceLocations;

    /// <summary>
    /// Card art for the cards Revolutions creates from abilities that have no player card in Demeo.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Demeo finds a card's picture by the ability's name in its Addressables catalog. Revolutions puts abilities
    /// into heroes' hands that Demeo never made into player cards (starting cards such as Grab and Net, the abilities
    /// the Energy Potion and critical hits grant), so the catalog had no picture for them: every time a hand was
    /// drawn the lookup failed and logged an exception (hundreds a minute on 8 November 2026), and the card had no
    /// art. This adds those names to the catalog, each pointing at the art of a similar card, so the game finds a
    /// picture the ordinary way. The card list is the one the removed local art patch used (be23860).
    /// </para>
    /// <para>
    /// A name the game already has art for is never answered here, so real art always wins and nothing is doubled
    /// (the game merges the answers of all catalogs). Unmodded guests keep the missing pictures on their side; only
    /// machines with this plugin get them.
    /// </para>
    /// </remarks>
    internal sealed class RevolutionsCardArt : IResourceLocator
    {
        private static readonly Dictionary<string, string> ArtByCard = new Dictionary<string, string>
        {
            // Revolutions class cards.
            { nameof(AbilityKey.Net), "WebBomb" },
            { nameof(AbilityKey.EnemyFlashbang), "Flashbang" },
            { nameof(AbilityKey.Grab), "Grapple" },
            { nameof(AbilityKey.EnemyFireball), "Fireball" },
            { nameof(AbilityKey.DiseasedBite), "PoisonedTip" },
            { nameof(AbilityKey.Electricity), "LightningBolt" },
            { nameof(AbilityKey.ExplodingLampPlaceholder), "ExplodingVortexLamp" },

            // The Energy Potion and the abilities it grants by class.
            { nameof(AbilityKey.EnergyPotion), "ExtraEnergyPotion" },
            { nameof(AbilityKey.ImplosionExplosionRain), "Tornado" },
            { nameof(AbilityKey.LeapHeavy), "PlayerLeap" },
            { nameof(AbilityKey.DeathBeam), "ScrollElectricity" },
            { nameof(AbilityKey.WeakeningShout), "ShatteringVoice" },

            // Additional cards granted by critical hits.
            { nameof(AbilityKey.EnemyFrostball), "Freeze" },
            { nameof(AbilityKey.SpawnRandomLamp), "SpellPowerPotion" },
            { nameof(AbilityKey.SpellPower), "SpellPowerPotion" },
        };

        private static RevolutionsCardArt? _registered;

        // Per card name: the stand-in art's locations, or null when the game has its own art or no stand-in exists.
        private readonly Dictionary<string, IList<IResourceLocation>?> _answers = new Dictionary<string, IList<IResourceLocation>?>();

        public string LocatorId => "HouseRules.Essentials.RevolutionsCardArt";

        public IEnumerable<object> Keys => ArtByCard.Keys;

        public IEnumerable<IResourceLocation> AllLocations => Enumerable.Empty<IResourceLocation>();

        /// <summary>
        /// Adds the card art entries to the game's catalog, once. Called when a Revolutions rule activates.
        /// </summary>
        internal static void EnsureRegistered()
        {
            if (_registered != null)
            {
                return;
            }

            try
            {
                _registered = new RevolutionsCardArt();
                Addressables.AddResourceLocator(_registered);
                HouseRulesEssentialsBase.LogDebug($"Added card art for {ArtByCard.Count} Revolutions cards that have none in Demeo.");
            }
            catch (Exception e)
            {
                _registered = null;
                HouseRulesEssentialsBase.LogWarning($"Could not add the Revolutions card art: {e.Message}");
            }
        }

        public bool Locate(object key, Type type, out IList<IResourceLocation> locations)
        {
            locations = null!;
            if (!(key is string name) || type != typeof(CardArtMapping) || !ArtByCard.TryGetValue(name, out var art))
            {
                return false;
            }

            if (!_answers.TryGetValue(name, out var answer))
            {
                if (LocateElsewhere(name, type, out _))
                {
                    answer = null;
                }
                else if (LocateElsewhere(art, type, out var standIn))
                {
                    answer = standIn;
                }
                else
                {
                    answer = null;
                    HouseRulesEssentialsBase.LogWarning($"No card art for {name}, and its stand-in {art} has none either.");
                }

                _answers[name] = answer;
            }

            if (answer == null)
            {
                return false;
            }

            locations = answer;
            return true;
        }

        private bool LocateElsewhere(string key, Type type, out IList<IResourceLocation> locations)
        {
            foreach (var locator in Addressables.ResourceLocators)
            {
                if (ReferenceEquals(locator, this))
                {
                    continue;
                }

                if (locator.Locate(key, type, out var found) && found != null && found.Count > 0)
                {
                    locations = found;
                    return true;
                }
            }

            locations = null!;
            return false;
        }
    }
}
