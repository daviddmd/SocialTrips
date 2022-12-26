using BackendAPI.Entities;
using BackendAPI.Helpers;
using BackendAPI.Models.Post;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BackendAPI.Repositories
{
    /// <summary>
    /// Repository for the Trip Posts
    /// </summary>
    public interface IPostRepository
    {
        /// <summary>
        /// Get all Posts
        /// </summary>
        /// <returns>List of Posts in the Platform</returns>
        Task<IEnumerable<Post>> GetAll();
        /// <summary>
        /// Get Post by its ID
        /// </summary>
        /// <param name="Id">ID of the Post</param>
        /// <returns>Entity representing the Post</returns>
        Task<Post> GetById(int Id);
        /// <summary>
        /// Create a Post, associated with an User and Trip and containing multiple media attachments, its Date (and published/updated date), Description and visibility status.
        /// Fails if the Post Creation Date is lower than the trip beginning date or higher than the trip ending date.
        /// </summary>
        /// <param name="post">Post to Create</param>
        /// <returns></returns>
        Task Create(Post post);
        /// <summary>
        /// Delete a Post. Removes all associated Attachments.
        /// </summary>
        /// <param name="post">Post to Delete</param>
        /// <returns></returns>
        Task Delete(Post post);
        /// <summary>
        /// Updates a Post. Only the Description, Date and Visibility may be updated.
        /// </summary>
        /// <param name="post">Post to be updated</param>
        /// <param name="model"><see cref="PostUpdateModel">Post Update Model</see> containing the (possibily different) information of the post to update</param>
        /// <returns></returns>
        Task Update(Post post, PostUpdateModel model);
        /// <summary>
        /// Adds an attachment to a post, making use of the <see cref="GoogleCloudStorageHelper"/>. Fails if the media to add is invalid.
        /// </summary>
        /// <param name="post">Post to add an attachment to</param>
        /// <param name="file">Form File to be uploaded and attached to the Post</param>
        /// <returns></returns>
        Task AddAttachment(Post post, IFormFile file);
        /// <summary>
        /// Removes an attachment from the post and the storage bucket, making use of the <see cref="GoogleCloudStorageHelper"/>.
        /// </summary>
        /// <param name="attachment">Attachment to remove</param>
        /// <returns></returns>
        Task RemoveAttachment(Attachment attachment);
        /// <summary>
        /// Get an attachment by its Guid.
        /// </summary>
        /// <param name="FileId">Guid of the file</param>
        /// <returns>Attachment Entity, containing its file name (in the bucket), original file name, bucket file URL, Upload Date and Associated Post</returns>
        Task<Attachment> GetAttachmentById(Guid FileId);
    }
}
