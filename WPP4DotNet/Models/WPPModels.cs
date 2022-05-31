using System.Collections.Generic;

namespace WPP4DotNet.Models
{
    public class MessageModels
    {
        public string Recipient { get; set; }
        public string Message { get; set; }
        public Enum.MessageType Type { get; set; }
        public FileModels File { get; set; }
        public ContactModels Contact { get; set; }
        public LocationModels Location { get; set; }
        public LinkModels Link { get; set; }
        public ButtonModels Button { get; set; }
        public SelectionModels Selection { get; set; }
    }
    public class SendReturnModels
    {
        public string Id { get; set; }
        public string Sender { get; set; }
        public string Recipient { get; set; }
        public bool Status { get; set; }
        public string Error { get; set; }
    }
    public class FileModels
    {
        public string Name { get; set; }
        public string Message { get; set; }
        public string Base64 { get; set; }
    }
    public class ContactModels
    {
        public string Name { get; set; }
        public string Number { get; set; }
    }
    public class LocationModels
    {
        public string Name { get; set; }
        public string Latitude { get; set; }
        public string Longitude { get; set; }
    }
    public class LinkModels
    {
        public string Link { get; set; }
        public string Message { get; set; }
    }
    public class ButtonModels
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public List<ButtonOptionsModels> Options { get; set; }
    }
    public class ButtonOptionsModels
    {
        public int Id { get; set; }
        public string Text { get; set; }
        public int Type { get; set; }
    }
    public class SelectionModels
    {
        public string Link { get; set; }
        public string Message { get; set; }
        public List<SelectionOptionsModels> Options { get; set; }  
    }
    public class SelectionOptionsModels
    {
        public int Id { get; set; }
        public string Text { get; set; }
        public int Type { get; set; }
    }
}
