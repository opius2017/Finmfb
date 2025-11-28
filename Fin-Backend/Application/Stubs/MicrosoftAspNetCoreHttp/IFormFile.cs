using System.IO;

namespace Microsoft.AspNetCore.Http
{
    // Minimal IFormFile stub to satisfy DTO references during compile-fix
    public interface IFormFile
    {
        string FileName { get; }
        long Length { get; }
        Stream OpenReadStream();
    }
}
