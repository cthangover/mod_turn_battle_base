namespace Cthangover.Core.Battle
{
    /// <summary>
    /// One extra bar of a battle unit next to its health (mana, rage, ammo…), as an
    /// <see cref="IBattleUnitGauge"/> reads it at this moment.
    /// </summary>
    public class BattleGauge
    {
        /// <summary>Stable id (a stat id, for example) the cores key their rows by.</summary>
        public string Id { get; set; }

        /// <summary>Short label, a locale key or plain text ("MP").</summary>
        public string Label { get; set; }

        /// <summary>Current value.</summary>
        public int Current { get; set; }

        /// <summary>Maximum; a bar with Max &lt;= 0 is not drawn.</summary>
        public int Max { get; set; }

        /// <summary>Fill colour as HTML ("#4a8cff"); null = the theme's info colour.</summary>
        public string Color { get; set; }
    }
}
