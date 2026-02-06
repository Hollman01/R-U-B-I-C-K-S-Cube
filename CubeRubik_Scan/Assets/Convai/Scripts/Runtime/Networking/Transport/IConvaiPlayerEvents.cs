namespace Assets.Convai.Scripts.Server
{
    public enum TranscriptionPhase
    {
        /// <summary>
        ///     No transcription is active.
        /// </summary>
        Idle,

        /// <summary>
        ///     The system detected speech onset and is preparing to stream text.
        /// </summary>
        Listening,

        /// <summary>
        ///     The user transcript is streaming; the text is not yet final.
        /// </summary>
        Interim,

        /// <summary>
        ///     Automatic speech recognition produced a final hypothesis (subject to further processing).
        /// </summary>
        AsrFinal,

        /// <summary>
        ///     The transcript has been post-processed/cleaned and is ready for display or persistence.
        /// </summary>
        ProcessedFinal,

        /// <summary>
        ///     The transcription session ended (either naturally or due to cancellation).
        /// </summary>
        Completed
    }

    internal interface IConvaiPlayerEvents
    {
        /// <summary>
        ///     Gets called when a transcription is received.
        /// </summary>
        /// <param name="transcript"></param>
        /// <param name="transcriptionType"></param>
        public void OnUserTranscriptionReceived(string transcript, TranscriptionPhase transcriptionPhase);

        /// <summary>
        ///     Handles the event triggered when speaking starts.
        /// </summary>
        /// <remarks>
        ///     This method is typically called to perform actions or trigger behaviors when a
        ///     speaking process begins. Ensure any necessary setup or state changes are completed before invoking this
        ///     method.
        /// </remarks>
        public void OnUserStartedSpeaking(string sessionId);

        /// <summary>
        ///     Handles the event triggered when speaking has stopped.
        /// </summary>
        /// <remarks>
        ///     This method is typically called to perform actions or cleanup tasks after speech has
        ///     concluded. Ensure any necessary resources are released or state changes are handled appropriately.
        /// </remarks>
        public void OnUserStoppedSpeaking(string sessionId, bool didProduceFinalTranscript);
    }
}
