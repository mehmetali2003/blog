using System;
using System.Collections.Generic;
using System.Linq;

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

namespace SampleBlog.VerifyTests.Managers
{
    /// <summary>
    /// Unit tests for tag managing business logic
    /// </summary>
    [TestFixture]
    public class TagManagerTest
    {
        PostRepository _mockPostsRepository;
        TagRepository _mockTagsRepository;
        private Fixture _fixture;

        private const int SampleExistingPostId = 1;
        private const string SampleExistingPostTitle = "SamplePostTitle";
        private const string SampleExistingPostContent = "SamplePostContent";
        private const int SampleExistingTag1Id = 1;
        private const string SampleExistingTag1Name = "SampleTag1";
        private const int SampleExistingTag2Id = 2;
        private const string SampleExistingTag2Name = "SampleTag2";
        private IKernel kernel;

        [SetUp]
        public void Init()
        {
            kernel = new StandardKernel(new NinjectTestModule());
            AutoMapperConfig.Init();
            _fixture = new Fixture();
            _mockPostsRepository = new PostRepository();
            _mockTagsRepository = new TagRepository();

            var sampleTag1 = new Tag
            {
                Id = SampleExistingTag1Id,
                Name = SampleExistingTag1Name
            };

            var sampleTag2 = new Tag
            {
                Id = SampleExistingTag2Id,
                Name = SampleExistingTag2Name
            };

            var samplePost = new Post
            {
                Id = SampleExistingPostId,
                Title = SampleExistingPostTitle,
                Content = SampleExistingPostContent,
                DateCreated = DateTime.Now,
                Tags = new List<Tag> 
                {
                    sampleTag1
                }
            };

            _mockPostsRepository.AllItems.Add(samplePost);
            _mockTagsRepository.AllItems.Add(sampleTag1);
            _mockTagsRepository.AllItems.Add(sampleTag2);

            kernel.Unbind<IPostRepository>();
            kernel.Bind<IPostRepository>().ToConstant(_mockPostsRepository);
            kernel.Unbind<ITagRepository>();
            kernel.Bind<ITagRepository>().ToConstant(_mockTagsRepository);
        }

        [Test]
        public void TagExistingPostNewTagSuccess()
        {
            var manager = kernel.Get<ITagManager>();
            var newTagName = _fixture.Create<string>();
            manager.TagPost(SampleExistingPostId, newTagName);

            var createdTag = _mockTagsRepository.AllItems.FirstOrDefault(t => t.Name == newTagName);

            createdTag.Should().NotBeNull();
            createdTag.Name.Should().Be(newTagName);
            _mockPostsRepository.AllItems.First(p => p.Id == SampleExistingPostId).Tags.Should().Contain(p => p.Name == newTagName);
        }

        [Test]
        public void TagExistingPostExistingTag1Success()
        {
            var manager = kernel.Get<ITagManager>();
            manager.TagPost(SampleExistingPostId, SampleExistingTag1Name);

            _mockPostsRepository.AllItems.First(p => p.Id == SampleExistingPostId).Tags.Should().Contain(p => p.Name == SampleExistingTag1Name);
        }

        [Test]
        public void TagExistingPostExistingTag2Success()
        {
            var manager = kernel.Get<ITagManager>();
            manager.TagPost(SampleExistingPostId, SampleExistingTag2Name);

            _mockPostsRepository.AllItems.First(p => p.Id == SampleExistingPostId).Tags.Should().Contain(p => p.Name == SampleExistingTag2Name);
        }

        [Test]
        public void TagNonExistingPost()
        {
            var manager = kernel.Get<ITagManager>();
            
            TestDelegate testDelegate = () => manager.TagPost(1010, SampleExistingTag1Name);

            Assert.That(testDelegate, Throws.TypeOf<ArgumentException>());
        }

        [Test]
        public void UntagExistingPostExistingTagSuccess()
        {
            var manager = kernel.Get<ITagManager>();
            manager.UntagPost(SampleExistingPostId, SampleExistingTag1Name);

            _mockPostsRepository.AllItems.First(p => p.Id == SampleExistingPostId).Tags.Should().NotContain(p => p.Name == SampleExistingTag1Name);
        }

        [Test]
        public void UntagExistingPostNonExistingTag()
        {
            var manager = kernel.Get<ITagManager>();
            
            TestDelegate testDelegate = () => manager.UntagPost(SampleExistingPostId, "NonExistingTagName");

            Assert.That(testDelegate, Throws.TypeOf<ArgumentException>());
        }

        [Test]
        public void UntagNonExistingPost()
        {
            var manager = kernel.Get<ITagManager>();
           
            TestDelegate testDelegate = () => manager.UntagPost(1010, SampleExistingTag1Name);

            Assert.That(testDelegate, Throws.TypeOf<ArgumentException>());
        }
    }
}
