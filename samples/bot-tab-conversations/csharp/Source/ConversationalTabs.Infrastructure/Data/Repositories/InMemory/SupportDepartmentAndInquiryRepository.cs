// Copyright (c) Microsoft Corporation. All rights reserved.

namespace Microsoft.Teams.Samples.ConversationalTabs.Infrastructure.Data.Repositories.InMemory;

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Teams.Samples.ConversationalTabs.Domain.Exceptions;
using Microsoft.Teams.Samples.ConversationalTabs.Domain.IRepositories;
using Microsoft.Teams.Samples.ConversationalTabs.Domain.Models;

public class SupportDepartmentAndInquiryRepository : IRepository<SupportDepartment>, ISubEntityRepository<CustomerInquiry>
{
    private readonly IDictionary<string, SupportDepartment> _supportDepartmentsDictionary;

    public SupportDepartmentAndInquiryRepository()
    {
        _supportDepartmentsDictionary = new ConcurrentDictionary<string, SupportDepartment>();
    }

    public Task<SupportDepartment> CreateOrUpdateObject(SupportDepartment input)
    {
        _supportDepartmentsDictionary.Add(input.Id, input);

        return Task.FromResult(input);
    }

    public Task<ICollection<SupportDepartment>> GetAllObjects()
    {
        return Task.FromResult<ICollection<SupportDepartment>>(_supportDepartmentsDictionary.Values.ToList());
    }

    public Task<SupportDepartment> GetObject(string id)
    {
        if (_supportDepartmentsDictionary.TryGetValue(id, out SupportDepartment category))
        {
            return Task.FromResult(category);
        }

        throw new ApiException(HttpStatusCode.NotFound, ErrorCode.CategoryNotFound, $"Requested support department with id '{id}' was not found.");
    }

    public Task<bool> DeleteObject(SupportDepartment item) => throw new NotImplementedException();

    public Task<CustomerInquiry> CreateSubEntity(string objectEntityId, CustomerInquiry subEntity)
    {
        if (_supportDepartmentsDictionary.TryGetValue(objectEntityId, out SupportDepartment category))
        {
            category.SubEntities.Add(subEntity);
            return Task.FromResult(subEntity);
        }

        throw new ApiException(HttpStatusCode.NotFound, ErrorCode.CategoryNotFound, $"Requested inquiry with id '{objectEntityId}' was not found.");
    }

    public Task<CustomerInquiry> UpdateSubEntity(string objectEntityId, CustomerInquiry subEntity)
    {
        if (_supportDepartmentsDictionary.TryGetValue(objectEntityId, out SupportDepartment category))
        {
            var subEntities = category.SubEntities.ToList();

            var inquiryIndex = category.SubEntities
                .Select((subEntity, index) => new { subEntity, index })
                .FirstOrDefault(s => s.subEntity.SubEntityId == subEntity.SubEntityId)?.index ?? -1;

            if (inquiryIndex > -1)
            {
                subEntities.RemoveAt(inquiryIndex);
                subEntities.Add(subEntity);

                category = category with { SubEntities = subEntities };
                _supportDepartmentsDictionary[objectEntityId] = category;
                return Task.FromResult(subEntity);
            }

            throw new ApiException(HttpStatusCode.NotFound, ErrorCode.ItemNotFound, $"Requested inquiry with id '{subEntity.SubEntityId}' was not found in department '{objectEntityId}'");
        }

        throw new ApiException(HttpStatusCode.NotFound, ErrorCode.CategoryNotFound, $"Requested department with id '{objectEntityId}' was not found.");
    }

    public Task<ICollection<CustomerInquiry>> GetSubEntities(string objectEntityId)
    {
        if (_supportDepartmentsDictionary.TryGetValue(objectEntityId, out SupportDepartment category))
        {
            return Task.FromResult((ICollection<CustomerInquiry>)category.SubEntities);
        }

        throw new ApiException(HttpStatusCode.NotFound, ErrorCode.CategoryNotFound, $"Requested support department with id '{objectEntityId}' was not found.");
    }

    public Task<bool> DeleteSubEntity(string objectEntityId, CustomerInquiry subEntity)
    {
        return Task.FromResult(
            _supportDepartmentsDictionary.TryGetValue(objectEntityId, out SupportDepartment category) &&
            category.SubEntities.Remove(subEntity));
    }
}
