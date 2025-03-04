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
        var x = await getDatabaseMetaData.GetDatabaseTablesMetaDataAsync(DatabaseDriver.POSTGRESQL);
        return Ok(x);
    }
}
