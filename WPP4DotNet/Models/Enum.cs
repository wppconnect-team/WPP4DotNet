namespace WPP4DotNet.Models
{
    public static class Enum
    {
        /// <summary>
        /// Browser
        /// </summary>
        public enum Browser
        {
            chrome,
            firefox,
            edge,
            opera
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
            connected,
            disconnected,
            qrcode,
            error
        }
    }
}
