using BattleBitAPI.Common;

namespace BattleBitAPI.Server.EventArgs
{
    public class PlayerReportedEventArgs<TPlayer> where TPlayer : Player
    {
        /// <summary>
        /// The player who made the report.
        /// </summary>
        public TPlayer Reporter { get; init; }

        /// <summary>
        /// The player being reported.
        /// </summary>
        public TPlayer Reported { get; init; }

        /// <summary>
        /// The report reason.
        /// </summary>
        public ReportReason Reason { get; init; }

        /// <summary>
        /// Additional details about the report.
        /// </summary>
        public string Detail { get; init; }

        internal PlayerReportedEventArgs(TPlayer reporter, TPlayer reported, ReportReason reason, string detail)
        {
            Reporter = reporter;
            Reported = reported;
            Reason = reason;
            Detail = detail;
        }
    }
}