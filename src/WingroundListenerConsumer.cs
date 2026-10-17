namespace Cthangover.Core.Battle
{
    using Cthangover.Core.Mods.Consumers;

    /// <summary>
    /// Feeds <see cref="WingroundListenerRegistry"/> with every mod's C#
    /// <see cref="IWingroundListener"/> implementations, and removes them when
    /// their mod unloads. GDScript listeners still register through the hook.
    /// </summary>
    public class WingroundListenerConsumer : ModTypeConsumer<IWingroundListener>
    {
        /// <inheritdoc />
        public override string Id => "turn_battle_base.winground_listeners";

        /// <inheritdoc />
        protected override void Add(IWingroundListener instance, string modId) => WingroundListenerRegistry.Instance.RegisterListener(instance);

        /// <inheritdoc />
        protected override void Remove(IWingroundListener instance, string modId) => WingroundListenerRegistry.Instance.UnregisterListener(instance);
    }
}
