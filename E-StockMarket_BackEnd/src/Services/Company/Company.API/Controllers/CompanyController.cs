using Company.API.Entities;
using Company.API.Repositories.Interfaces;
using DnsClient.Internal;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;

namespace Company.API.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    public class CompanyController : ControllerBase
    {
        private readonly ICompanyDetailsRepository _repository;
        private readonly ILogger<CompanyController> _logger;

        public CompanyController(ICompanyDetailsRepository repository, ILogger<CompanyController> logger)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<CompanyDetails>), (int)HttpStatusCode.OK)]
        public async Task<ActionResult<IEnumerable<CompanyDetails>>> GetCompanies()
        {
            var products = await _repository.GetCompanies();
            return Ok(products);
        }

        [Route("[action]/{code}", Name = "GetCompanyByCode")]
        [HttpGet]
        public async Task<IActionResult> GetCompanyByCode(string code)
        {
            var company = await _repository.GetCompanyByCode(code);
            return Ok(company);
        }

        [HttpPost]
        [ProducesResponseType(typeof(CompanyDetails), (int)HttpStatusCode.OK)]
        public async Task<ActionResult<CompanyDetails>> Post([FromBody] CompanyDetails company)
        {
            await _repository.CreateCompany(company);

            return CreatedAtRoute("GetCompanyByCode", new { code = company.Code }, company);
        }

        [HttpPut]
        [ProducesResponseType(typeof(CompanyDetails), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> Put([FromBody] CompanyDetails company)
        {
            return Ok(await _repository.UpdateCompany(company));
        }

        [HttpDelete]
        [ProducesResponseType(typeof(CompanyDetails), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> Delete(string code)
        {
            return Ok(await _repository.DeleteCompany(code));
        }
    }
}
