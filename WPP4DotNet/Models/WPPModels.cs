using System;
using System.Collections.Generic;

namespace WPP4DotNet.Models
{
    /// <summary>
    /// ChatModel
    /// </summary>
    public class ChatModel
    { 
        public string Id { get; set; }
        public string Name { get; set; }
        public string PushName { get; set; }
        public string Number { get; set; }
        public string Server { get; set; }
        public string Image { get; set; }
        public bool HasUnread { get; set; }
        public bool IsMe { get; set; }
        public bool IsContact { get; set; }
        public bool IsWAContact { get; set; }
        public bool IsBusiness { get; set; }
        public bool IsBroadcast { get; set; }
        public bool IsGroup { get; set; }
        public bool IsUser { get; set; }
        public string Type { get; set; }
        public string LastMessage { get; set; }
        public List<MessageModels> Messages { get; set; }
    }

    /// <summary>
    /// MessageModels
    /// </summary>
    public class MessageModels
    {
        public string Id { get; set; }
        public string Sender { get; set; }
        public string Recipient { get; set; }
        public bool FromMe { get; set; }
        public bool SimulateTyping { get; set; }
        public string Message { get; set; }
        public int Ack { get; set; }
        public string Type { get; set; }
        public FileModels File { get; set; }
        public ContactModels Contact { get; set; }
        public LocationModels Location { get; set; }
        public LinkModels Link { get; set; }
        public ButtonModels Button { get; set; }
        public SelectionModels Selection { get; set; }
    }

    /// <summary>
    /// SendReturnModels
    /// </summary>
    public class SendReturnModels
    {
        public string Id { get; set; }
        public string Sender { get; set; }
        public string Recipient { get; set; }
        public bool Status { get; set; }
        public string Error { get; set; }
    }

    /// <summary>
    /// FileModels
    /// </summary>
    public class FileModels
    {
        public string Name { get; set; }
        public string Message { get; set; }
        public string Base64 { get; set; }
    }

    /// <summary>
    /// ContactModels
    /// </summary>
    public class ContactModels
    {
        public string Name { get; set; }
        public string Number { get; set; }
    }

    /// <summary>
    /// LocationModels
    /// </summary>
    public class LocationModels
    {
        public string Name { get; set; }
        public string Latitude { get; set; }
        public string Longitude { get; set; }
    }

    /// <summary>
    /// LinkModels
    /// </summary>
    public class LinkModels
    {
        public string Link { get; set; }
        public string Message { get; set; }
    }

    /// <summary>
    /// ButtonModels
    /// </summary>
    public class ButtonModels
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public List<ButtonOptionsModels> Options { get; set; }
    }

    /// <summary>
    /// ButtonOptionsModels
    /// </summary>
    public class ButtonOptionsModels
    {
        public int Id { get; set; }
        public string Text { get; set; }
        public int Type { get; set; }
    }

    /// <summary>
    /// SelectionModels
    /// </summary>
    public class SelectionModels
    {
        public string Link { get; set; }
        public string Message { get; set; }
        public List<SelectionOptionsModels> Options { get; set; }  
    }

    /// <summary>
    /// SelectionOptionsModels
    /// </summary>
    public class SelectionOptionsModels
    {
        public int Id { get; set; }
        public string Text { get; set; }
        public int Type { get; set; }
    }

    /// <summary>
    /// GroupInfoModels
    /// </summary>
    public class GroupInfoModels
    {
        public string Id { get; set; }
        public bool IsParentGroup { get; set; }
        public bool Announce { get; set; }
        public DateTime Creation { get; set; }
        public bool DefaultSubgroup { get; set; }
        public string Desc { get; set; }
        public string DescID { get; set; }
        public string DescOwner { get; set; }
        public DateTime DescTime { get; set; }
        public bool NoFrequentlyForwarded { get; set; }
        public int NumSubgroups { get; set; }
        public string Owner { get; set; }
        public List<ParticipantsModels> Participants { get; set; }
        public string PvId { get; set; }
        public bool Restrict { get; set; }
        public int Size { get; set; }
        public int Status { get; set; }
        public string Subject { get; set; }
        public string SubjectOwner { get; set; }
        public DateTime SubjectTime { get; set; }
        public bool Support { get; set; }
        public bool Suspended { get; set; }
    }
    public class ParticipantsModels
    {
        public string Id { get; set; }
        public bool IsAdmin { get; set; }
        public bool IsSuperAdmin { get; set; }
    }
}
