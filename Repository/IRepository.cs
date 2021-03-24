using DataAccess;
using System;
using System.Collections.Generic;
using System.Text;

namespace Repository
{
    public interface IRepository<T> where T : BaseEntity
    {
        T Get(int id);
        IEnumerable<T> GetAll();

    }
}
