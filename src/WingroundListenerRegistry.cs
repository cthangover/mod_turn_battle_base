using System;
using System.Collections.Generic;
using Cthangover.Core.Utils;

namespace Cthangover.Core.Battle
{
	/// <summary>
	/// Singleton registry that collects <see cref="IWingroundListener"/>
	/// implementations (C# from every mod through
	/// <see cref="WingroundListenerConsumer"/>, GDScript via
	/// <see cref="RegisterListener"/>) and broadcasts winground
	/// lifecycle hooks to all of them in registration order.
	/// </summary>
	public class WingroundListenerRegistry
	{
		public static readonly WingroundListenerRegistry Instance = new();

		private readonly List<IWingroundListener> _listeners = new();

		/// <summary>
		/// Registers a listener, replacing any earlier one with the same
		/// <see cref="IWingroundListener.Id"/> (used by the GDScript bridge
		/// <see cref="GDWingroundListener"/> and by
		/// <see cref="WingroundListenerConsumer"/>). Safe to call from any thread — the
		/// underlying list is not thread-safe but all registrations happen during
		/// mod initialisation before battle scenes are loaded.
		/// </summary>
		public void RegisterListener(IWingroundListener listener)
		{
			if (listener == null)
				return;

			_listeners.RemoveAll(existing => existing.Id == listener.Id);
			_listeners.Add(listener);
			GameLogger.Log("BATTLE",
				$"WingroundListenerRegistry: registered listener '{listener.Id}'");
		}

		/// <summary>Removes exactly <paramref name="listener"/> (its mod is being unloaded).</summary>
		public void UnregisterListener(IWingroundListener listener)
		{
			_listeners.Remove(listener);
		}

		/// <summary>
		/// Broadcasts <see cref="IWingroundListener.OnBeforeExpAward"/>
		/// to every registered listener. Exceptions from individual
		/// listeners are logged and swallowed so one broken mod cannot
		/// break the victory screen for others.
		/// </summary>
		public void NotifyBeforeExpAward(WingroundContext ctx)
		{
			foreach (var listener in _listeners)
			{
				try
				{
					listener.OnBeforeExpAward(ctx);
				}
				catch (Exception ex)
				{
					GameLogger.Log("BATTLE",
						$"WingroundListenerRegistry: {listener.Id}.OnBeforeExpAward threw: {ex.Message}",
						LogLevel.Error);
				}
			}
		}

		/// <summary>
		/// Broadcasts <see cref="IWingroundListener.OnAfterLootGenerated"/>
		/// to every registered listener.
		/// </summary>
		public void NotifyAfterLootGenerated(WingroundContext ctx)
		{
			foreach (var listener in _listeners)
			{
				try
				{
					listener.OnAfterLootGenerated(ctx);
				}
				catch (Exception ex)
				{
					GameLogger.Log("BATTLE",
						$"WingroundListenerRegistry: {listener.Id}.OnAfterLootGenerated threw: {ex.Message}",
						LogLevel.Error);
				}
			}
		}

		/// <summary>
		/// Broadcasts <see cref="IWingroundListener.OnBeforeShow"/>
		/// to every registered listener.
		/// </summary>
		public void NotifyBeforeShow(WingroundContext ctx)
		{
			foreach (var listener in _listeners)
			{
				try
				{
					listener.OnBeforeShow(ctx);
				}
				catch (Exception ex)
				{
					GameLogger.Log("BATTLE",
						$"WingroundListenerRegistry: {listener.Id}.OnBeforeShow threw: {ex.Message}",
						LogLevel.Error);
				}
			}
		}
	}
}
