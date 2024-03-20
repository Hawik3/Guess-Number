using Microsoft.AspNetCore.Mvc;

namespace GuessNumberWebApi.Controllers
{
    [ApiController]
    [Route("[controller]")]

    public class GuessNumberController(ILogger<GuessNumberController> logger) : ControllerBase
    {
        MLModel1.ModelInput sampleData = new();
        private readonly ILogger<GuessNumberController> _logger = logger;

        [HttpPost]
        public IActionResult Get(IFormFile picture)
        {



            using (MemoryStream memoryStream = new())
            {
                picture.CopyTo(memoryStream);
                sampleData.ImageSource = memoryStream.ToArray();
                var sortedScoresWithLabel = MLModel1.PredictAllLabels(sampleData);
                return Ok(sortedScoresWithLabel);

            }

            //else
            //{
            //    return NotFound();
            //}
        }

    }
}
