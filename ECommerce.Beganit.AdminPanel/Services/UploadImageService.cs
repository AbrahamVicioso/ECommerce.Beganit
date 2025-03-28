using CloudinaryDotNet;
using CloudinaryDotNet.Actions;

namespace ECommerce.Beganit.AdminPanel.Services
{
    public class UploadImageService
    {
        private readonly Cloudinary cloudinary;

        public UploadImageService(Cloudinary cloudinary) 
        {
            this.cloudinary = cloudinary;
        }
        public async Task<ImageUploadResult> Upload(IFormFile image)
        {
            try
            {
                await using Stream stream = image.OpenReadStream();

                var uploadParams = new ImageUploadParams()
                {
                    File = new FileDescription(image.FileName, stream),
                    PublicId = Guid.NewGuid().ToString(),
                    Overwrite = true
                    // Optional: Add transformation or folder
                    // Folder = "your_folder_name",
                    // Transformation = new Transformation().Width(500).Crop("scale")
                };

                ImageUploadResult uploadResult = await cloudinary.UploadAsync(uploadParams);

                return uploadResult;
            }
            catch (Exception ex) {
                throw ex;
            }
        }
    }
}
