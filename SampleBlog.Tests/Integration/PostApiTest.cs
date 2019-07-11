using NUnit.Framework;
using SampleBlog.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web.Script.Serialization;

namespace SampleBlog.Tests.Integration
{
    /// <summary>
    /// Test fixture for integration testing post API
    /// </summary>
    [TestFixture]
    public class PostApiTest : BaseIntegrationTest
    {
        private Uri createPostUri => ApiAction("Post");
        private Func<int, Uri> getPostUri => newPostId => ApiAction(string.Format("Post/{0}", newPostId));

        private class PostDto
        {
            public string Title { get; set; }
            public string Content { get; set; }
        }
        /// <summary>
        /// Performs checks of all post-related API methods one by one with checks of sanity in between
        /// </summary>
        /// <returns></returns>
        [Test]
        public async Task IntegrationAllPostMethods()
        {
            using (var httpClient = new HttpClient())
            {
                var newPost = new PostDto
                {
                    Title = "SampleTitle",
                    Content = "SampleContent"
                };

                // New post creation test
                var newPostId = await CreateNewPost(httpClient, newPost);

                // Load newly created post test
                var postLoaded = await GetPost(httpClient, newPostId, newPost);

                // List posts and check that newly created post is there
                await GetPostLists(httpClient, newPostId, newPost);

                // Modifying post and saving it
                await EditPost(postLoaded, httpClient);

                // Loading the post again to ensure modifications saved
                await GetModifiedPost(httpClient, newPostId, postLoaded);

                // Deleting the post we just created
                await DeletePost(httpClient, newPostId);

                // List all posts again to ensure that post is deleted
                await EnsurePostIsDeleted(httpClient, newPostId);
            }
        }

        private async Task EnsurePostIsDeleted(HttpClient httpClient, int newPostId)
        {
            var listResult2 = await httpClient.GetAsync(createPostUri);
            Assert.AreEqual(HttpStatusCode.OK, listResult2.StatusCode, "GET API method failed");

            var listItems2 =
                new JavaScriptSerializer().Deserialize<List<PostListItemModel>>(listResult2.Content.ReadAsStringAsync().Result);
            Assert.IsFalse(listItems2.Any(p => p.Id == newPostId));
        }

        private async Task DeletePost(HttpClient httpClient, int newPostId)
        {
            var deleteResult = await httpClient.DeleteAsync(getPostUri(newPostId));
            Assert.AreEqual(HttpStatusCode.NoContent, deleteResult.StatusCode, "DELETE API method failed");
        }

        private async Task GetModifiedPost(HttpClient httpClient, int newPostId, PostDetailsModel postLoaded)
        {
            var getResult = await httpClient.GetAsync(getPostUri(newPostId));
            Assert.AreEqual(HttpStatusCode.OK, getResult.StatusCode, "GET API method failed");

            var modifiedPostLoaded = new JavaScriptSerializer().Deserialize<PostDetailsModel>(getResult.Content.ReadAsStringAsync().Result);
            Assert.AreEqual(postLoaded.Title, modifiedPostLoaded.Title);
            Assert.AreEqual(postLoaded.Content, modifiedPostLoaded.Content);
            Assert.IsNotNull(modifiedPostLoaded.DateModified);
            Assert.IsTrue(modifiedPostLoaded.DateCreated > DateTime.MinValue);
        }

        private async Task EditPost(PostDetailsModel postLoaded, HttpClient httpClient)
        {
            postLoaded.Title += "Modified";
            postLoaded.Content += "Modified";
            var putResult = await httpClient.PutAsJsonAsync(createPostUri, postLoaded);
            Assert.AreEqual(HttpStatusCode.NoContent, putResult.StatusCode, "PUT API method failed");
        }

        private async Task GetPostLists(HttpClient httpClient, int newPostId, PostDto newPost)
        {
            var listResult = await httpClient.GetAsync(createPostUri);
            Assert.AreEqual(HttpStatusCode.OK, listResult.StatusCode, "GET API method failed");

            var listItems = new JavaScriptSerializer().Deserialize<List<PostListItemModel>>(listResult.Content.ReadAsStringAsync().Result);
            var postListItemLoaded = listItems.FirstOrDefault(p => p.Id == newPostId);
            Assert.IsNotNull(postListItemLoaded);
            Assert.AreEqual(newPostId, postListItemLoaded.Id);
            Assert.AreEqual(newPost.Title, postListItemLoaded.Title);
            Assert.IsTrue(postListItemLoaded.DateCreated > DateTime.MinValue);
        }

        private async Task<PostDetailsModel> GetPost(HttpClient httpClient, int newPostId, PostDto newPost)
        {
            var getResult = await httpClient.GetAsync(getPostUri(newPostId));
            Assert.AreEqual(HttpStatusCode.OK, getResult.StatusCode, "GET/id API method failed");

            var postLoaded = new JavaScriptSerializer().Deserialize<PostDetailsModel>(getResult.Content.ReadAsStringAsync().Result);
            Assert.AreEqual(newPostId, postLoaded.Id);
            Assert.AreEqual(newPost.Title, postLoaded.Title);
            Assert.AreEqual(newPost.Content, postLoaded.Content);
            Assert.IsNull(postLoaded.DateModified);
            Assert.IsTrue(postLoaded.DateCreated > DateTime.MinValue);

            return postLoaded;
        }

        private async Task<int> CreateNewPost(HttpClient httpClient, object newPost)
        {
            var postResult = await httpClient.PostAsJsonAsync(createPostUri, newPost);
            Assert.AreEqual(HttpStatusCode.OK, postResult.StatusCode, "POST API method failed");

            var newPostId = int.Parse(postResult.Content.ReadAsStringAsync().Result);
            Assert.IsTrue(newPostId > 0);

            return newPostId;
        }
    }
}
