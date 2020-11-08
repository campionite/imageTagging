using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace QvcImageTagger.Predict.Models
{
    public class UploadViewModel
    {
        [Required(ErrorMessage = "Please select a file to upload.")]
        public IFormFile File { get; set; }
    }
}
