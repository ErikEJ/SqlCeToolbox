using System;
using EnvDTE;
using EnvDTE80;

namespace ErikEJ.SqlCeToolbox.Helpers
{
    /// <summary>
    /// Reports anonymous usage through ApplicationInsights
    /// </summary>
    public static class Telemetry
    {
        /// <summary>
        /// Initializes the telemetry client.
        /// </summary>
        public static void Initialize(DTE2 dte, string version, string vsVersion, string telemetryKey)
        {
        }

        public static bool Enabled { get; set; }

        /// <summary>Tracks an event to ApplicationInsights.</summary>
        public static void TrackEvent(string key)
        {
        }

        public static void TrackPageView(string key)
        {
        }

        /// <summary>Tracks any exception.</summary>
        public static void TrackException(Exception ex)
        {
        }
    }
}
