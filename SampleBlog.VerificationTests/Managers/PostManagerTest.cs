using System;
using System.Collections.Generic;
using System.Linq;

using SampleBlog;
using SampleBlog.Controllers;
using SampleBlog.Models;
using SampleBlog.Managers;
using SampleBlog.Managers.Interface;
using SampleBlog.Repositories;
using SampleBlog.Repositories.Entities;
using SampleBlog.Repositories.Interface;

using FluentAssertions;
using Moq;
using Ninject;
using NUnit.Framework;
using Ploeh.AutoFixture;
using Ploeh.SemanticComparison;
using Ploeh.SemanticComparison.Fluent;
using NUnit.Framework.Constraints;

namespace SampleBlog.VerifyTests.Managers
{
    /// <summary>
    /// Unit tests for post-managing business logic
    /// </summary>
    [TestFixture]
    public class PostManagerTest
    {
        private const int SampleExistingPostId = 1;
        private const string SampleExistingPostTitle = "SamplePostTitle";
        private const string SampleExistingPostContent = "SamplePostContent";
        private const int SampleExistingTagId = 1;
        private const string SampleExistingTagName = "SampleTag";

        private Fixture _fixture;
        PostRepository _mockPostsRepository;
        private IKernel kernel;
        [SetUp]
        public void Init()
        {
            kernel = new StandardKernel(new NinjectTestModule());
            //NinjectWebCommon.RegisterServices(kernel);
            AutoMapperConfig.Init();
            _mockPostsRepository = new PostRepository();
            _fixture = new Fixture();
            var samplePost = new Post
            {
                Id = SampleExistingPostId,
                Title = SampleExistingPostTitle,
                Content = SampleExistingPostContent,
                DateCreated = DateTime.Now,
                Tags = new List<Tag> 
                {
                    new Tag
                    {
                        Id = SampleExistingTagId,
                        Name = SampleExistingTagName
                    }
                }
            };
            _mockPostsRepository.AllItems.Clear();
            _mockPostsRepository.AllItems.Add(samplePost);
            kernel.Unbind<IPostRepository>();
            kernel.Bind<IPostRepository>().ToConstant(_mockPostsRepository);
        }

        [Test]
        public void PostSaveNewSuccessful()
        {
            var manager = kernel.Get<IPostManager>();

            var newPost = _fixture.Build<PostDetailsModel>()
                .Without(p => p.Id)
                .Without(p => p.Tags)
                .Without(p => p.DateModified).Create();
            
            manager.Save(newPost);
            newPost.Id.Should().BeGreaterThan(0);
            
            var createdPost = manager.Get(newPost.Id);
            createdPost.Should().NotBeNull();

            createdPost.AsSource().OfLikeness<PostDetailsModel>().Without(p => p.Tags).ShouldEqual(newPost);
        }

        [Test]
        public void PostSaveNewTitleError()
        {
            var manager = kernel.Get<IPostManager>();
            var newPost = _fixture.Build<PostDetailsModel>()
                .Without(p => p.Id)
                .Without(p => p.Title).Create();

            TestDelegate testDelegate = () => manager.Save(newPost);

            Assert.That(testDelegate, Throws.TypeOf<ArgumentException>());
        }

        [Test]
        public void PostSaveNewContentError()
        {
            var manager = kernel.Get<IPostManager>();
            var newPost = _fixture.Build<PostDetailsModel>()
                .Without(p => p.Id)
                .Without(p => p.Content).Create();
            
            TestDelegate testDelegate = () => manager.Save(newPost);

            Assert.That(testDelegate, Throws.TypeOf<ArgumentException>());
        }

        [Test]
        public void PostSaveExistingSuccessful()
        {
            var manager = kernel.Get<IPostManager>();
            var post = _fixture.Build<PostDetailsModel>()
                .Without(p => p.Id)
                .Without(p => p.Tags)
                .Without(p => p.DateModified)
                .Create();

            post.Id = SampleExistingPostId;
            manager.Save(post);

            var updatedPost = manager.Get(SampleExistingPostId);
            updatedPost.Should().NotBeNull();

            updatedPost.AsSource().OfLikeness<PostDetailsModel>()
                .Without(p => p.DateModified)
                .Without(p => p.Tags)
                .ShouldEqual(post);
        }

        [Test]
        public void PostSaveExistingTitleError()
        {
            var manager = kernel.Get<IPostManager>();
            var post = _fixture.Build<PostDetailsModel>()
                .Without(p => p.Id)
                .Without(p => p.Title).Create();
            post.Id = SampleExistingPostId;

            TestDelegate testDelegate = () => manager.Save(post);

            Assert.That(testDelegate, Throws.TypeOf<ArgumentException>());
        }

        [Test]
        public void PostSaveExistingContentError()
        {
            var manager = kernel.Get<IPostManager>();
            var post = _fixture.Build<PostDetailsModel>().Without(p => p.Id).Without(p => p.Content).Create();
            post.Id = SampleExistingPostId;

            TestDelegate testDelegate = () => manager.Save(post);

            Assert.That(testDelegate, Throws.TypeOf<ArgumentException>());
        }

        [Test]
        public void PostSaveNotExistingIdError()
        {
            var manager = kernel.Get<IPostManager>();
            var post = _fixture.Build<PostDetailsModel>().Without(p => p.Id).Create();
            post.Id = 1000;

            TestDelegate testDelegate = () => manager.Save(post);

            Assert.That(testDelegate, Throws.TypeOf<ArgumentException>());
        }

        [Test]
        public void GetSuccess()
        {
            var manager = kernel.Get<IPostManager>();
            var existingPost = manager.Get(SampleExistingPostId);

            existingPost.Should().NotBeNull();

            // using Likeness different types of objects (Model and Entity) can be semantically compared
            existingPost.AsSource().OfLikeness<Post>()
                .Without(p => p.Tags)
                .ShouldEqual(_mockPostsRepository.AllItems.First(p => p.Id == SampleExistingPostId));
        }

        [Test]
        public void GetSuccessWithTags()
        {
            var manager = kernel.Get<IPostManager>();
            var existingPost = manager.Get(SampleExistingPostId);

            existingPost.Should().NotBeNull();

            // using Likeness different types of objects (Model and Entity) can be semantically compared
            existingPost.AsSource().OfLikeness<Post>()
                .Without(p => p.Tags)
                .ShouldEqual(_mockPostsRepository.AllItems.First(p => p.Id == SampleExistingPostId));

            existingPost.Tags.Should().ContainSingle(SampleExistingTagName);
        }

        [Test]
        public void GetDontExistError()
        {
            var manager = kernel.Get<IPostManager>();

            TestDelegate testDelegate = () => manager.Get(1001);

            Assert.That(testDelegate, Throws.TypeOf<ArgumentException>());
        }

        [Test]
        public void ListSuccess()
        {
            var manager = kernel.Get<IPostManager>();
            var list = manager.List();

            list.Count.Should().BeGreaterThan(0);
            _mockPostsRepository.AllItems.Count.Should().Be(list.Count);

            var expected = _mockPostsRepository.AllItems.First(p => p.Id == SampleExistingPostId);
            var actual = list.First(p => p.Id == SampleExistingPostId);

            actual.ShortDescription.Should().NotBeNullOrEmpty();

            // using Likeness different types of objects (Model and Entity) can be semantically compared
            expected.AsSource().OfLikeness<PostListItemModel>()
                .Without(p => p.ShortDescription)
                .ShouldEqual(actual);
        }

        [Test]
        public void RemoveSuccess()
        {
            var manager = kernel.Get<IPostManager>();
            manager.Remove(SampleExistingPostId);

            _mockPostsRepository.AllItems.Should().NotContain(p => p.Id == SampleExistingPostId);
        }

        [Test]
        public void RemoveDontExistError()
        {
            var manager = kernel.Get<IPostManager>();
            TestDelegate testDelegate = () => manager.Remove(1001);

            Assert.That(testDelegate, Throws.TypeOf<ArgumentException>());
        }
    }
}
