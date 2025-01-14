﻿namespace VmesteGO.Domain.Entities;

public class EventInvitation : BaseEntity<int>
{
    public int EventId { get; set; }
    public Event Event { get; set; } = null!;
    public int SenderId { get; set; }
    public User Sender { get; set; } = null!;
    public int ReceiverId { get; set; }
    public User Receiver { get; set; } = null!;
    public string Status { get; set; } // Possible values: "Pending", "Accepted", "Rejected"
}