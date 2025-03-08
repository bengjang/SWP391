using System;
using System.Collections.Generic;

namespace lamlai.Models;

public partial class User
{
    public int UserId { get; set; }

    public string Name { get; set; } = null!;

    public string FullName { get; set; } = null!;

    public string Password { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string Role { get; set; } = null!;

    public string Phone { get; set; } = null!;

    public string? Address { get; set; }

    public DateTime RegistrationDate { get; set; }

    public string? SkinType { get; set; }

    public virtual ICollection<Conversation> Conversations { get; set; } = new List<Conversation>();

    public virtual ICollection<Message> Messages { get; set; } = new List<Message>();

    public virtual ICollection<Notification> Notifications { get; set; } = new List<Notification>();

    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();

    public virtual ICollection<Review> Reviews { get; set; } = new List<Review>();

    public virtual ICollection<UserQuizResponse> UserQuizResponses { get; set; } = new List<UserQuizResponse>();

    public virtual ICollection<UserSkinTypeResult> UserSkinTypeResults { get; set; } = new List<UserSkinTypeResult>();

    public virtual ICollection<Voucher> Vouchers { get; set; } = new List<Voucher>();
}
