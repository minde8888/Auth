
namespace Auth.Services.Dtos
{
    public class User
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Surname { get; set; }
        public string Email { get; set; }
        public string Roles { get; set; }
        public string PhoneNumber { get; set; }
        public string ImageName { get; set; }
    }
}
