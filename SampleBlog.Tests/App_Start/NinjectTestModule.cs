using Ninject.Modules;
using SampleBlog.Managers;
using SampleBlog.Managers.Interface;
using SampleBlog.Repositories;
using SampleBlog.Repositories.Interface;

namespace SampleBlog.Tests
{
    public class NinjectTestModule : NinjectModule
    {
        public override void Load()
        {
            Bind<IPostManager>().To<PostManager>();
            Bind<ITagManager>().To<TagManager>();
            Bind<IPostRepository>().To<PostRepository>();
            Bind<ITagRepository>().To<TagRepository>();
        }
    }
}