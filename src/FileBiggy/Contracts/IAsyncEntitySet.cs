using System.Collections.Generic;
using System.Threading.Tasks;

namespace FileBiggy.Contracts
{
    public interface IAsyncEntitySet<T>
    {
        Task ClearAsync();
        Task<T> UpdateAsync(T item);
        Task RemoveAsync(T item);
        Task RemoveAsync(IEnumerable<T> items);
        Task AddAsync(T item);
        Task AddAsync(IEnumerable<T> items);
    }
}