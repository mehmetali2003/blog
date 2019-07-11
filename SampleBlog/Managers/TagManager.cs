using SampleBlog.Managers.Interface;
using SampleBlog.Repositories;
using SampleBlog.Repositories.Interface;
using System;
using System.Linq;

namespace SampleBlog.Managers
{
    /// <summary>
    /// Implementation for tags-related business logic methods
    /// </summary>
    public class TagManager: ITagManager
    {
        private ITagRepository _tagRepository;
        private IPostRepository _postRepository;

        public TagManager(ITagRepository tagRepository, IPostRepository postRepository)
        {
            _tagRepository = tagRepository;
            _postRepository = postRepository;
        }

        /// <summary>
        /// Post with given postId is tagged with given tag.
        /// </summary>
        /// <param name="postId">Post Id</param>
        /// <param name="tag">Tag</param>
        public void TagPost(int postId, string tag)
        {
            if (string.IsNullOrEmpty(tag))
            {
                throw new ArgumentException("Tag name is invalid");
            }

            var post = _postRepository.AsQuery().FirstOrDefault(p => p.Id == postId);
            if (post == null)
            {
                throw new ArgumentException(string.Format("Specified post (id={0}) does not exist", postId));
            }

            var tagEntity = _tagRepository.AsQuery().FirstOrDefault(t => t.Name == tag);

            // check if a given post is already tagged with tag specified
            if (tagEntity == null || !post.Tags.Any(pt => pt.Id == tagEntity.Id))
            {
                if (tagEntity == null)
                {
                    tagEntity = new Repositories.Entities.Tag
                    {
                        Name = tag
                    };
                    _tagRepository.Add(tagEntity);
                }
                post.Tags.Add(tagEntity);
            }
        }

        /// <summary>
        /// Post with given postId is untagged with given tag.
        /// </summary>
        /// <param name="postId"></param>
        /// <param name="tag"></param>
        public void UntagPost(int postId, string tag)
        {
            var post = _postRepository.AsQuery().Where(p => p.Id == postId).FirstOrDefault();
            if (post == null)
            {
                throw new ArgumentException("Post not found!");
            }

            var _tag = post.Tags.Where(t => t.Name == tag).FirstOrDefault();
            if (_tag == null)
            {
                throw new ArgumentException("Tag is not belonging to post!");
            }

            post.Tags.Remove(_tag);
        }
    }
}