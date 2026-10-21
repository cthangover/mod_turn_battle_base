using System;
using Cthangover.Core.Cards.StatusEffect;

namespace Cthangover.Core.Factories.Impls
{
    /// <summary>
    /// Thin <c>FileFactory</c> for status effect metadata — duration, stack
    /// behaviour, visual icon, etc. Works in tandem with
    /// <c>StatusEffectActionFactory</c>: this factory provides the <b>data</b>
    /// (what the effect is), while the action factory provides the
    /// <b>behaviour</b> (what the effect does each turn). Files are discovered
    /// by the shared entity resolver through the DTO's
    /// <c>[EntityKind("status_effect")]</c> attribute.
    /// </summary>
    public class StatusEffectInfoFactory : FileFactory<StatusEffectInfo>
    {
        private static readonly Lazy<StatusEffectInfoFactory> lazy = new(() => new StatusEffectInfoFactory());
        /// <summary>Lazy singleton facade over the AppServices-owned factory.</summary>
        public static StatusEffectInfoFactory Instance => lazy.Value;
    }
}
