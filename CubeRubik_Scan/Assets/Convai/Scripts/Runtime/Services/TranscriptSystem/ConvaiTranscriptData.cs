namespace Convai.Scripts.Services.TranscriptSystem
{
    public struct ConvaiTranscriptData
    {
        /// <summary>
        ///     Identifier for the sender (NPC or Player ID).
        /// </summary>
        public string Identifier;

        /// <summary>
        ///     Unique key for the specific message/utterance. Used to differentiate multiple messages
        ///     from the same sender that may be active simultaneously.
        /// </summary>
        public string MessageKey;

        public string Name;
        public string Message;
        public bool IsLastChunk;


        public ConvaiTranscriptData(string identifier, string name, string message, bool isLastChunk, string messageKey = null)
        {
            Identifier = identifier;
            MessageKey = messageKey ?? identifier;
            Name = name;
            Message = message;
            IsLastChunk = isLastChunk;
        }
    }
}
