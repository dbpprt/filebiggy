using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FileBiggy.Contracts
{
    public interface IAsyncBiggyStore<T>
    {
        Task<List<T>> AllAsync();
        Task ClearAsync();
        Task AddAsync(T item);
        Task AddAsync(List<T> items);
        Task<T> UpdateAsync(T item);
        Task RemoveAsync(T item);
        Task RemoveAsync(IEnumerable<T> items);
        IQueryable<T> AsQueryable();
    }
}