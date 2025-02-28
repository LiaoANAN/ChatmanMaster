namespace Chatman.Models.DTOs
{
    public class UpdateProfileRequest
    {
        public string UserName { get; set; }
        public string Bio { get; set; }
        public string Gender { get; set; }
        public DateTime Birthday { get; set; }
        public IFormFile UserImage { get; set; }
    }
}
