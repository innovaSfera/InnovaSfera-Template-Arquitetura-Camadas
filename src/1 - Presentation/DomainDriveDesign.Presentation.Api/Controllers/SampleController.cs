using DomainDrivenDesign.Application.Entities;
using DomainDrivenDesign.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace DomainDriveDesign.Presentation.Api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class SampleController(ISampleDataAppService _service) : ControllerBase
    {

        [HttpGet(Name = "GetSampleData")]
        public async Task<IEnumerable<SampleDataDto>> Get()
        {
            return await _service.GetAll();
        }

        [HttpPost(Name = "AddSampleData")]
        public async Task Add(SampleDataDto sampleData)
        {
            await _service.Add(sampleData);
        }
    }
}
