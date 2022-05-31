namespace WPP4DotNet.Models
{
    public static class Enum
    {
        public enum Browser
        {
            Chrome,
            Firefox,
            Edge,
            Opera
        }

        public enum MessageType
        {
            Text,
            Reply,
            Sticker,
            Mentioned,
            Selection,
            Button,
            Contact,
            Ptt,
            Localization,
            Link,
            Audio,
            Video,
            Document,
            Image,
            Payment
        }

        public enum Status
        {
            Connected,
            Disconnected,
            QrCode,
            Error
        }
    }
}
