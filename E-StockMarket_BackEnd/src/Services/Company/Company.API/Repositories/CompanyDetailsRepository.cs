using Company.API.Data.Interfaces;
using Company.API.Entities;
using Company.API.Repositories.Interfaces;
using EventBus.Messages.Events;
using MassTransit;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Company.API.Repositories
{
    public class CompanyDetailsRepository : ICompanyDetailsRepository
    {
        private readonly ICompanyContext _context;
        private readonly IPublishEndpoint _publishEndpoint;
        public CompanyDetailsRepository(ICompanyContext context, IPublishEndpoint publishEndpoint)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _publishEndpoint = publishEndpoint ?? throw new ArgumentNullException(nameof(publishEndpoint));
        }

        public async Task<IEnumerable<CompanyDetails>> GetCompanies()
        {
            return await _context
                            .Companies
                            .Find(p => true)
                            .ToListAsync();
        }

        public async Task<CompanyDetails> GetCompany(string id)
        {
            return await _context
                           .Companies
                           .Find(p => p.Id == id)
                           .FirstOrDefaultAsync();
        }

        public async Task<IEnumerable<CompanyDetails>> GetCompanyByName(string name)
        {
            FilterDefinition<CompanyDetails> filter = Builders<CompanyDetails>.Filter.ElemMatch(p => p.Name, name);

            return await _context
                            .Companies
                            .Find(filter)
                            .ToListAsync();
        }

        public async Task<IEnumerable<CompanyDetails>> GetCompanyByCode(string code)
        {
            FilterDefinition<CompanyDetails> filter = Builders<CompanyDetails>.Filter.Eq(p => p.Code, code);

            return await _context
                            .Companies
                            .Find(filter)
                            .ToListAsync();
        }


        public async Task CreateCompany(CompanyDetails company)
        {
            await _context.Companies.InsertOneAsync(company);
        }

        public async Task<bool> UpdateCompany(CompanyDetails company)
        {
            var updateResult = await _context
                                        .Companies
                                        .ReplaceOneAsync(filter: g => g.Id == company.Id, replacement: company);

            return updateResult.IsAcknowledged
                    && updateResult.ModifiedCount > 0;
        }

        public async Task<bool> DeleteCompany(string code)
        {
            FilterDefinition<CompanyDetails> filter = Builders<CompanyDetails>.Filter.Eq(p => p.Code, code);

            //var company = await _context
            //               .Companies
            //               .Find(p => p.Code == code)
            //               .FirstOrDefaultAsync();

            DeleteResult deleteResult = await _context
                                                .Companies
                                                .DeleteOneAsync(filter);

            if(deleteResult.DeletedCount > 0)
            {
                var msg = new CompanyDeleteEvent();
                msg.Code = code;
                msg.UserId = 1234;
                await _publishEndpoint.Publish(msg);
            }
            

            return deleteResult.IsAcknowledged
                && deleteResult.DeletedCount > 0;
        }
    }
}
