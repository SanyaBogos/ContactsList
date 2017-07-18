using ContactsList.Server.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ContactsList.Server.Repositories.Abstract
{
    public interface IContactRepository
    {
        Task DeleteAllUserContact(int userId);
        Task<IEnumerable<Contact>> GetContactsById(int id);
        Task InsertMany(IEnumerable<Contact> items);
        Task Update(IEnumerable<Contact> items);
    }
}
