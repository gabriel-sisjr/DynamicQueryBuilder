using DynamicQueryBuilder.Interfaces.Helpers;
using DynamicQueryBuilder.Models.Enums.Helpers.Databases;
using Microsoft.AspNetCore.Mvc;

namespace ClientWebAPI.Controllers;

[Route("api/[controller]")]
[ApiController]
public class DynamicQueryBuilderController(IGetDatabaseMetaData getDatabaseMetaData) : ControllerBase
{
    public async Task<IActionResult> Get()
    {
        var tablesMetaData = await getDatabaseMetaData.GetDatabaseTablesMetaDataAsync();
        return Ok(tablesMetaData);
    }
}
