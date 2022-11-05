using BackendAPI.Entities;
using BackendAPI.Models.Post;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BackendAPI.Repositories
{
    public interface IPostRepository
    {
        Task<IEnumerable<Post>> GetAll();
        Task<Post> GetById(int Id);
        Task Create(Post post);
        Task Delete(Post post);
        Task Update(Post post, PostUpdateModel model);
        Task AddAttachment(Post post, IFormFile file);
        Task RemoveAttachment(Attachment attachment);
        Task<Attachment> GetAttachmentById(Guid FileId);
    }
}
