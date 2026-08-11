using System.ComponentModel.DataAnnotations;

namespace JobSeeker.Models
{
    /// <summary>
    /// A single image attached to a job vacancy (job_vacancy_images table).
    /// Up to 3 images may be attached per Job. Image bytes live in the
    /// "vacancy-images" folder of the main S3 bucket; only the object key
    /// is stored here.
    /// </summary>
    public class JobVacancyImage
    {
        [Key]
        public long JobVacancyImageId { get; set; }

        public long JobId { get; set; }

        [Required]
        [StringLength(1024)]
        public string ImageS3Key { get; set; } = string.Empty;

        /// <summary>0-based position, used to keep the 3 images in upload order.</summary>
        public int DisplayOrder { get; set; }

        public DateTime UploadedAt { get; set; }

        public Job Job { get; set; } = null!;
    }
}
