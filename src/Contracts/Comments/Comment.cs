namespace Contracts.Comments
{
    using Contracts.Users;
    using System;

    public class Comment
    {
        public long Id { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public string Body { get; set; }
        public Profile Author { get; set; }
    }

}
