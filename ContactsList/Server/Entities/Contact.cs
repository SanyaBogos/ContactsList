using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace ContactsList.Server.Entities
{
    [Table("Contacts")]
    public class Contact
    {
        [Key]
        public int Id { get; set; }
        
        public int UserId { get; set; }

        [MaxLength(250)]
        public string Name { get; set; }

        [MaxLength(11)]
        public string Phone { get; set; }

        [MaxLength(10)]
        public string Index { get; set; }

        [MaxLength(250)]
        public string Region { get; set; }

        [MaxLength(250)]
        public string City { get; set; }

        [MaxLength(250)]
        public string Address { get; set; }

        [ForeignKey("UserId")]
        public virtual ApplicationUser User { get; set; }
    }
}
