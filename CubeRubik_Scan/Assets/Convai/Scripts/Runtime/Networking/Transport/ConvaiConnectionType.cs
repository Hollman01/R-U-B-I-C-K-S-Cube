namespace Convai.Scripts.Networking.Transport
{
    /// <summary>
    ///     Connection type for Convai WebRTC connections.
    ///     Determines whether audio-only or video-enabled connection is used.
    /// </summary>
    public enum ConvaiConnectionType
    {
        /// <summary>
        ///     Audio-only connection
        /// </summary>
        Audio,

        /// <summary>
        ///     Video-enabled connection (for API compatibility)
        /// </summary>
        Video
    }
}

