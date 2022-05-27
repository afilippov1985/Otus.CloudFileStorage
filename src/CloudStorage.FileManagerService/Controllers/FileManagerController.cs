using Microsoft.AspNetCore.Mvc;

namespace CloudStorage.FileManagerService.Controllers
{
    /// <summary>
    /// Набор запросов для управления файлами хранилища
    /// </summary>
    
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/filemanager")]
    public class FileManagerController : ControllerBase
    {

    }
}
