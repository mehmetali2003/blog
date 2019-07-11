using NUnit.Framework;
using SampleBlog.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web.Script.Serialization;

namespace SampleBlog.VerifyTests.Integration
{
   
    /// <summary>
    /// Test fixture for integration testing of tag API
    /// </summary>
    [TestFixture]
    public class TagApiTest : BaseIntegrationTest
    {
        private class TagDto
        {
            public int PostId { get; set; }
            public string Tag { get; set; }
        }
        private class PostDto
        {
            public string Title { get; set; }
            public string Content { get; set; }
        }

        private Uri createPostUri => ApiAction("Post");
        private Uri createTagUri => ApiAction("Tag");
        private Func<int, Uri> getPostUri => newPostId => ApiAction(string.Format("Post/{0}", newPostId));
        private Func<int,string, Uri> createTagInPostUri => (postId,tagName) => ApiAction(string.Format("Tag/{0}/{1}", postId, Uri.EscapeUriString(tagName)));
        /// <summary>
        /// Tests both tag API methods and checks for sanity in between
        /// </summary>
        /// <returns></returns>
        [Test]
        public async Task IntegrationAllTagMethods()
        {
            using (var httpClient = new HttpClient())
            {
                var newPost = new PostDto
                {
                    Title = "SampleTitleForTags",
                    Content = "SampleContentForTags"
                };

                // Create new post to test tagging on it
                var newPostId = await CreatePost(httpClient, newPost);

                var newTag = new TagDto()
                {
                    PostId = newPostId,
                    Tag = "Sample Tag"
                };

                await CreateTagInPost(httpClient, newTag);

                // Load post details to check if it contains tag
                var getResult = await GetPostWithTag(httpClient, newPostId, newTag);

                await DeleteTagFromPost(httpClient, newPostId, newTag);

                // Load post details to check that it does not contain tag anymore
                await EnsurePostHasTagDeleted(httpClient, newPostId, getResult, newTag);
            }
        }

        private async Task EnsurePostHasTagDeleted(HttpClient httpClient, int newPostId, HttpResponseMessage getResult, TagDto newTag)
        {
            var getResult2 = await httpClient.GetAsync(getPostUri(newPostId));
            Assert.AreEqual(HttpStatusCode.OK, getResult.StatusCode, "GET API method failed (for post)");

            var postLoaded2 = new JavaScriptSerializer().Deserialize<PostDetailsModel>(getResult2.Content.ReadAsStringAsync().Result);
            Assert.IsFalse(postLoaded2.Tags.Contains(newTag.Tag));
        }

        private async Task DeleteTagFromPost(HttpClient httpClient, int newPostId, TagDto newTag)
        {
            var untagResult = await httpClient.DeleteAsync(createTagInPostUri(newPostId, newTag.Tag));
            Assert.AreEqual(HttpStatusCode.NoContent, untagResult.StatusCode, "DELETE API method failed (for tag)");
        }

        private async Task<HttpResponseMessage> GetPostWithTag(HttpClient httpClient, int newPostId, TagDto newTag)
        {
            var getResult = await httpClient.GetAsync(getPostUri(newPostId));
            Assert.AreEqual(HttpStatusCode.OK, getResult.StatusCode, "GET API method failed (for post)");

            var postLoaded = new JavaScriptSerializer().Deserialize<PostDetailsModel>(getResult.Content.ReadAsStringAsync().Result);
            Assert.IsTrue(postLoaded.Tags.Contains(newTag.Tag));
            return getResult;
        }

        private async Task CreateTagInPost(HttpClient httpClient, TagDto newTag)
        {
            var tagResult = await httpClient.PostAsJsonAsync(createTagUri, newTag);
            Assert.AreEqual(HttpStatusCode.NoContent, tagResult.StatusCode, "POST API method failed (for tag)");
        }

        private async Task<int> CreatePost(HttpClient httpClient, PostDto newPost)
        {
            var createResult = await httpClient.PostAsJsonAsync(createPostUri, newPost);
            Assert.AreEqual(HttpStatusCode.OK, createResult.StatusCode, "POST API method failed (for post)");

            var newPostId = int.Parse(createResult.Content.ReadAsStringAsync().Result);
            Assert.IsTrue(newPostId > 0);

            return newPostId;
        }
    }
}
