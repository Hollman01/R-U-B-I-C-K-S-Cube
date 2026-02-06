public interface IConvaiNPCEvents
{
    /// <summary>
    ///     Handles the event when a transcription is received from the speech recognition system.
    /// </summary>
    /// <remarks>
    ///     This method processes both interim and final transcriptions. Interim transcriptions are
    ///     partial and may change,  while final transcriptions are complete and will not be updated further.
    /// </remarks>
    /// <param name="transcript">The text of the transcription received. This value cannot be null or empty.</param>
    /// <param name="isFinal">
    ///     A boolean value indicating whether the transcription is final.  true if the transcription is complete and final;
    ///     otherwise, false.
    /// </param>
    public void OnCharacterTranscriptionReceived(string transcript, bool isFinal);

    /// <summary>
    ///     Handles the event triggered when speaking starts.
    /// </summary>
    /// <remarks>
    ///     This method is typically called to perform actions or update state when speech begins. Ensure
    ///     any necessary setup or state changes are completed before invoking this method.
    /// </remarks>
    public void OnCharacterStartedSpeaking();

    /// <summary>
    ///     Handles the event triggered when speaking has stopped.
    /// </summary>
    /// <remarks>
    ///     This method is typically called to perform actions or cleanup tasks after speech has
    ///     concluded. Ensure any necessary resources are released or state changes are handled appropriately.
    /// </remarks>
    public void OnCharacterStoppedSpeaking();

    /// <summary>
    ///     Invoked when the Large Language Model (LLM) process starts.
    /// </summary>
    /// <remarks>
    ///     This method is typically used to perform any initialization or setup required when the LLM
    ///     process begins. It does not take any parameters and does not return a value.
    /// </remarks>
    public void OnLLMStarted();

    /// <summary>
    ///     Handles the event triggered when the large language model (LLM) operation has stopped.
    /// </summary>
    /// <remarks>
    ///     This method is intended to be called when the LLM operation completes or is terminated.
    ///     Implementations may use this method to perform cleanup, logging, or other post-operation tasks.
    /// </remarks>
    public void OnLLMStopped();

    /// <summary>
    ///     Invoked when a text-to-speech (TTS) operation starts.
    /// </summary>
    /// <remarks>
    ///     This method is typically used to handle events or perform actions when a TTS operation
    ///     begins. Ensure any required setup or state changes are completed before the TTS operation progresses.
    /// </remarks>
    public void OnTTSStarted();

    /// <summary>
    ///     Handles the event triggered when text-to-speech (TTS) playback is stopped.
    /// </summary>
    /// <remarks>
    ///     This method is typically called to perform any necessary cleanup or state updates when TTS
    ///     playback is interrupted or completed. Ensure that any resources associated with the TTS session are properly
    ///     released.
    /// </remarks>
    public void OnTTSStopped();

    /// <summary>
    ///     Handles the event when the current narrative design section ID is received.
    /// </summary>
    /// <remarks>
    ///     This method processes the provided section ID to update or handle the current narrative
    ///     design state. Ensure that <paramref name="sectionID" /> is a valid identifier before calling this
    ///     method.
    /// </remarks>
    /// <param name="sectionID">The identifier of the narrative design section. Must not be null or empty.</param>
    public void OnCurrentNarrativeDesignSectionIDReceived(string sectionID);
}
