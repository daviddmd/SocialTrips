using BackendAPI.Data;
using BackendAPI.Entities;
using BackendAPI.Entities.Enums;
using BackendAPI.Exceptions;
using BackendAPI.Helpers;
using BackendAPI.Models.Post;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace BackendAPI.Repositories
{
    public class PostRepository : IPostRepository
    {
        private readonly DatabaseContext _context;
        private readonly IGoogleCloudStorageHelper _storageHelper;

        public PostRepository(DatabaseContext context, IGoogleCloudStorageHelper storageHelper)
        {
            _context = context;
            _storageHelper = storageHelper;
        }

        public async Task AddAttachment(Post post, IFormFile file)
        {
            if (file != null && (FileHelper.IsImage(file) || FileHelper.IsVideo(file)))
            {
                String OriginalFileName = file.FileName;
                String DestinationFileName = $"post_{post.Id}-{DateTimeOffset.Now.ToUnixTimeSeconds()}{Path.GetExtension(file.FileName).ToLower()}";
                String Url = await _storageHelper.Upload(file, DestinationFileName);
                Attachment attachment = new() { OriginalFileName = OriginalFileName, StorageName = DestinationFileName, Url = Url, UploadedDate = DateTime.Now };
                _context.Attachments.Add(attachment);
                post.Attachments.Add(attachment);
                await _context.SaveChangesAsync();
            }
            else
            {
                throw new CustomException("No valid image or video passed", ErrorType.MEDIA_ERROR);
            }
        }

        public async Task RemoveAttachment(Attachment attachment)
        {
            await _storageHelper.Delete(attachment.StorageName);
            attachment.Post.Attachments.Remove(attachment);
            _context.Attachments.Remove(attachment);
            await _context.SaveChangesAsync();
        }

        public async Task Create(Post post)
        {
            post.PublishedDate = DateTime.Now;
            post.IsHidden = false;
            if (post.Date < post.Trip.BeginningDate || post.Date > post.Trip.EndingDate)
            {
                throw new CustomException("The date of the post cannot be lower than the trip beginning date or higher than the post ending date", ErrorType.POST_DATE_INVALID);
            }
            _context.Posts.Add(post);
            await _context.SaveChangesAsync();
        }

        public async Task Delete(Post post)
        {
            //Eliminar todos os anexos
            foreach(Attachment attachment in post.Attachments)
            {
                await _storageHelper.Delete(attachment.StorageName);
            }
            _context.Attachments.RemoveRange(post.Attachments);
            post.Attachments.Clear();
            _context.Posts.Remove(post);
            await _context.SaveChangesAsync();
        }
        public async Task<IEnumerable<Post>> GetAll()
        {
            return await _context.Posts.AsQueryable().ToListAsync();
        }

        public async Task<Post> GetById(int Id)
        {
            return await _context.Posts.FirstOrDefaultAsync(x => x.Id == Id);
        }

        public async Task Update(Post post, PostUpdateModel model)
        {
            if (model.Date < post.Trip.BeginningDate || model.Date > post.Trip.EndingDate)
            {
                throw new CustomException("The date of the post cannot be lower than the trip beginning date or higher than the post ending date", ErrorType.POST_DATE_INVALID);
            }
            post.Date = model.Date;
            post.Description = model.Description;
            post.PublishedDate = DateTime.Now;
            post.IsHidden = model.IsHidden;
            _context.Posts.Update(post);
            await _context.SaveChangesAsync();
        }

        public async Task<Attachment> GetAttachmentById(Guid FileId)
        {
            return await _context.Attachments.Where(p=>p.Id==FileId).FirstOrDefaultAsync();
        }
    }
}
