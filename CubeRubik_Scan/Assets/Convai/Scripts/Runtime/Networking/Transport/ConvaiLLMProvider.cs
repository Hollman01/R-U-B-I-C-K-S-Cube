namespace Convai.Scripts.Networking.Transport
{
    /// <summary>
    ///     LLM provider options for Convai WebRTC connections.
    ///     Determines which language model backend is used for the conversation.
    /// </summary>
    public enum ConvaiLLMProvider
    {
        /// <summary>
        ///     Dynamic provider selection based on character configuration
        /// </summary>
        Dynamic,

        /// <summary>
        ///     Google Gemini Live (realtime multimodal streaming)
        /// </summary>
        GeminiLive,

        /// <summary>
        ///     Google Gemini with BAML (Boundary ML) integration
        /// </summary>
        GeminiBaml
    }
}

