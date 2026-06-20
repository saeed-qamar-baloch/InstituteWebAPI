using System.ComponentModel.DataAnnotations;

namespace InstituteWebAPI.Models.DTO.WebsitePosts
{
    public class UpdateWebsitePostDto
    {
        [Required, MaxLength(200)]
        public string Title { get; set; }

        [Required]
        public string Body { get; set; }

        [Required, RegularExpression("^(Post|Achievement|Page)$", ErrorMessage = "PostType must be 'Post', 'Achievement' or 'Page'.")]
        public string PostType { get; set; } = "Post";

        /// <summary>Optional URL slug for pages; auto-generated from Title when empty.</summary>
        [MaxLength(200)]
        public string? Slug { get; set; }

        public bool IsPublished { get; set; } = true;
    }
}
