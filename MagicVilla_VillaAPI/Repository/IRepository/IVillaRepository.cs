using MagicVilla_VillaAPI.Models;
using System.Linq.Expressions;

namespace MagicVilla_VillaAPI.Repository.IRepository
{
    public interface IVillaRepository : IRepository<Villa>
    {
        //We use Task because is asynchronous

        Task<Villa> UpdateAsync(Villa entity);
    }
}
