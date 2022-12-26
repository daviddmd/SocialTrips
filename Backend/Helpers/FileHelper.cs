using Microsoft.AspNetCore.Http;
using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace BackendAPI.Helpers
{
    /// <summary>
    /// File Helper class to provide helper methods for validating media files before uploading them
    /// </summary>
    public class FileHelper
    {
        public const int ImageMinimumBytes = 512;
        private readonly static string[] AllowedImageExtensions = { ".jpg", ".png", ".gif", ".jpeg" };
        private readonly static string[] AllowedVideoExtensions = { ".mp4", ".avi", ".mov", ".mkv", ".webm", ".wmv" };
        private readonly static string[] AllowedImageMimeTypes = { "image/jpg", "image/jpeg", "image/pjpeg", "image/gif", "image/x-png", "image/png" };
        /// <summary>
        /// Checks for the Validity of an image file (valid file extension, MIME type or any sort of disguised file)
        /// </summary>
        /// <param name="postedFile">Form File of an Image to Upload</param>
        /// <returns></returns>
        public static bool IsImage(IFormFile postedFile)
        {
            if (!AllowedImageMimeTypes.Contains(postedFile.ContentType.ToLower()))
            {
                return false;
            }
            if (!AllowedImageExtensions.Contains(Path.GetExtension(postedFile.FileName).ToLower()))
            {
                return false;
            }
            try
            {
                if (!postedFile.OpenReadStream().CanRead)
                {
                    return false;
                }
                if (postedFile.Length < ImageMinimumBytes)
                {
                    return false;
                }

                byte[] buffer = new byte[ImageMinimumBytes];
                postedFile.OpenReadStream().Read(buffer, 0, ImageMinimumBytes);
                string content = System.Text.Encoding.UTF8.GetString(buffer);
                if (Regex.IsMatch(content, @"<script|<html|<head|<title|<body|<pre|<table|<a\s+href|<img|<plaintext|<cross\-domain\-policy",
                    RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Multiline))
                {
                    return false;
                }
            }
            catch (Exception)
            {
                return false;
            }

            return true;
        }
        /// <summary>
        ///  Checks for the Validity of a video file (valid file extension and MIME type)
        /// </summary>
        /// <param name="postedFile">Form File of a Video to Upload</param>
        /// <returns></returns>
        public static bool IsVideo(IFormFile postedFile)
        {
            if (!postedFile.ContentType.ToLower().StartsWith("video"))
            {
                return false;
            }
            if (!AllowedVideoExtensions.Contains(Path.GetExtension(postedFile.FileName).ToLower()))
            {
                return false;
            }
            return true;
        }

    }
}
