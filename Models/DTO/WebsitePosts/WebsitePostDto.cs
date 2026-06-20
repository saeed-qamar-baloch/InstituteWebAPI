namespace InstituteWebAPI.Models.DTO.WebsitePosts
{
    public class WebsitePostDto
    {
        public Guid WebsitePostID { get; set; }
        public string Title { get; set; }
        public string Body { get; set; }
        public string PostType { get; set; }
        public string Slug { get; set; }
        public string ImageUrl { get; set; }
        public bool IsPublished { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
