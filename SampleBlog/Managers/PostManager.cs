using AutoMapper;
using SampleBlog.Managers.Interface;
using SampleBlog.Models;
using SampleBlog.Repositories.Interface;
using SampleBlog.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SampleBlog.Managers
{
    /// <summary>
    /// Implementation for post-related business logic methods
    /// </summary>
    public class PostManager: IPostManager
    {
        private IPostRepository _postRepository;

        public PostManager(IPostRepository postRepository)
        {
            _postRepository = postRepository;
        }

        /// <summary>
        /// saves post
        /// </summary>
        /// <param name="postDetails"></param>
        public void Save(PostDetailsModel postDetails)
        {
            if (string.IsNullOrEmpty(postDetails.Title))
            {
                throw new ArgumentException("Post title is empty");
            }

            if (string.IsNullOrEmpty(postDetails.Content))
            {
                throw new ArgumentException("Post content is empty");
            }
            
            Repositories.Entities.Post savedEntity;

            if (postDetails.Id > 0)
            {
                var existingPost = _postRepository.AsQuery().FirstOrDefault(p => p.Id == postDetails.Id);
                if (existingPost == null)
                {
                    throw new ArgumentException(string.Format("Specified post (id={0}) does not exist", postDetails.Id));
                }

                savedEntity = Mapper.Map<SampleBlog.Models.PostDetailsModel, Repositories.Entities.Post>(postDetails, existingPost);
                savedEntity.DateModified = DateTime.UtcNow;
            }
            else
            {
                savedEntity = Mapper.Map<Repositories.Entities.Post>(postDetails);
                _postRepository.Add(savedEntity);
            }

            postDetails.Id = savedEntity.Id;
        }

        /// <summary>
        /// queries repository and return post by given Id
        /// </summary>
        /// <param name="id">Post Id</param>
        /// <returns>PostDetailsModel</returns>
        public PostDetailsModel Get(int id)
        {
            var post = _postRepository.AsQuery().Where(p => p.Id == id).FirstOrDefault();
            if (post == null)
            {
                throw new ArgumentException(string.Format("Specified post (id={0}) does not exist", id));
            }

            var postDetailsModel =  Mapper.Map<SampleBlog.Models.PostDetailsModel>(post);
            post.Tags.ToList().ForEach(t => postDetailsModel.Tags.Add(t));

            return postDetailsModel;
        }

        public ICollection<PostListItemModel> List()
        {
            return Mapper.Map<List<PostListItemModel>>(_postRepository.AsQuery());
        }

        /// <summary>
        /// removes post by given id
        /// </summary>
        /// <param name="id">Post Id</param>
        public void Remove(int id)
        {
            var post = _postRepository.AsQuery().FirstOrDefault(p => p.Id == id);
            if (post == null)
            {
                throw new ArgumentException(string.Format("Specified post (id={0}) does not exist", id));
            }
            _postRepository.Remove(post);
        }
    }
}