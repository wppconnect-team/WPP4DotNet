namespace WPP4DotNet.Models
{
    public static class Enum
    {
        /// <summary>
        /// Browser
        /// </summary>
        public enum Browser
        {
            Chrome,
            Firefox,
            Edge,
            Opera
        }

        /// <summary>
        /// MessageType
        /// </summary>
        public enum MessageType
        {
            chat,
            text,
            reply,
            sticker,
            mentioned,
            selection,
            button,
            contact,
            ptt,
            localization,
            link,
            audio,
            video,
            document,
            image,
            payment
        }

        /// <summary>
        /// ChatType
        /// </summary>
        public enum ChatType
        {
            chat,
            group
        }

        /// <summary>
        /// Status
        /// </summary>
        public enum Status
        {
            Connected,
            Disconnected,
            QrCode,
            Error
        }
    }
}
