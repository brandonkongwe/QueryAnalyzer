using System;
using Microsoft.AspNetCore.Mvc;
using QueryAnalyzer.Services;

namespace QueryAnalyzer.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class QueryController : ControllerBase
    {
        private readonly DatabaseService _databaseService;

        // initializing the database service
        public QueryController(DatabaseService databaseService)
        {
            _databaseService = databaseService;
        }

        // POST api/query/execute
        [HttpPost("execute")]
        public IActionResult ExecuteQuery([FromBody] QueryRequest request)
        {
            var (results, executionTime, error) = _databaseService.ExecuteQuery(request.Query);

            if (!string.IsNullOrEmpty(error))
            {
                return BadRequest(new { Error = error });
            }

            string queryPlan = _databaseService.GetQueryPlan(request.Query);
            var suggestions = _databaseService.AnalyzeQueryPlan(queryPlan);

            // returing the results, execution time, query plan, and suggestions
            return Ok(new
            {
                Results = results,
                ExecutionTime = executionTime.TotalMilliseconds,
                QueryPlan = queryPlan,
                Suggestions = suggestions
            });
        }
    }

    // query request model
    public class QueryRequest
    {
        public string Query { get; set; }
    }
}
