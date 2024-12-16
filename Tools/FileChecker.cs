namespace Fryzjer.OtherClasses
{
    public class FileChecker
    {
        private readonly IWebHostEnvironment _environment;

        public FileChecker(IWebHostEnvironment environment)
        {
            _environment = environment;
        }

        public bool DoesFileExist(string? relativePath)
        {
            if (string.IsNullOrEmpty(relativePath))
                return false;

            relativePath = relativePath.TrimStart('/');

            var fullPath = Path.Combine(_environment.WebRootPath, relativePath);
            return File.Exists(fullPath);
        }
    }
}
