using System.ComponentModel.DataAnnotations;

namespace ContactsList.Server.ViewModels
{
    public class UserNameViewModel
    {
        [Required]
        public string FirstName { get; set; }
        [Required]
        public string LastName { get; set; }
    }
}
