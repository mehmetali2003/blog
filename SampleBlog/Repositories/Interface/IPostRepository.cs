using System.Linq;

using SampleBlog.Repositories.Entities;

namespace SampleBlog.Repositories.Interface
{
    /// <summary>
    /// Interface for storage abstraction (repository) of posts
    /// </summary>
    public interface IPostRepository : ICommonRepository<Post>
    {
        new IQueryable<Post> AsQuery();
        new void Add(Post item);
        new void Remove(Post item);
    }
}