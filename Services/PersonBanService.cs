using System;
using AuthApi.Data;
using AuthApi.Entities;
using AuthApi.Migrations;

namespace AuthApi.Services;

public class PersonBanService
{
    private readonly DirectoryDBContext dbContext;
    private readonly ILogger<PersonBanService> logger;

    public PersonBanService(DirectoryDBContext dbContext, ILogger<PersonBanService> logger)
    {
        this.dbContext = dbContext;
        this.logger = logger;
    }

    public (bool Success, string Message, Person? Person) BanPerson(Guid personId, string? reasonMessage)
    {
        var person = dbContext.People.FirstOrDefault(p => p.Id == personId);

        if (person == null)
        {
            return (false, "La persona no se encuentra registrada en el sistema.", null);
        }

        if (person.BannedAt != null)
        {
            return (false, "La persona ya ha sido baneada.", person);
        }

        using var transaction = dbContext.Database.BeginTransaction();
        try
        {
            // * attempt to ban the person
            person.BannedAt = DateTime.UtcNow;
            dbContext.People.Update(person);
            
            // * make the history record
            dbContext.PersonBanHistories.Add( new PersonBanHistory {
                PersonId = person.Id,
                Activo = "BANNED",
                Message = reasonMessage,
                CreatedAt = DateTime.Now
            });
            dbContext.SaveChanges();
            transaction.Commit();

            this.logger.LogInformation("The person [{id}|{name}] was banned.", person.Id, person.FullName);
            return (true, "La persona fue baneada correctamente.", person);
        }
        catch (System.Exception ex)
        {
            this.logger.LogError(ex, "Fail at attempt to ban the person [{id}|{name}]: {message}", person.Id, person.FullName, ex.Message);
            transaction.Rollback();
            return (false, "Error al banear a la persona.", person);
        }
    }

    public (bool Success, string Message, Person? Person) UnbanPerson(Guid personId, string? reasonMessage)
    {
        var person = dbContext.People.FirstOrDefault(p => p.Id == personId);

        if (person == null)
        {
            return (false, "La persona no se encuentra registrada en el sistema.", null);
        }

        if (person.BannedAt == null)
        {
            return (false, "La persona no está baneada actualmente.", person);
        }

        using var transaction = dbContext.Database.BeginTransaction();
        try
        {
            // * attempt to unban the person
            person.BannedAt = null;
            dbContext.People.Update(person);

            // * make the history record
            dbContext.PersonBanHistories.Add( new PersonBanHistory {
                PersonId = person.Id,
                Activo = "UNBANNED",
                Message = reasonMessage,
                CreatedAt = DateTime.Now
            });
            dbContext.SaveChanges();
            transaction.Commit();

            this.logger.LogInformation("The person [{id}|{name}] was unbanned.", person.Id, person.FullName );
            return (true, "La persona fue desbaneada correctamente.", person);
        }
        catch (System.Exception ex)
        {
            this.logger.LogError(ex, "Fail at attempt to unban the person [{id}|{name}]: {message}", person.Id, person.FullName, ex.Message);
            transaction.Rollback();
            return (false, "Error al banear a la persona.", person);
        }
    }

    /// <summary>
    /// get the ban hostory of the person
    /// </summary>
    /// <param name="personId"></param>
    /// <param name="take"></param>
    /// <param name="offset"></param>
    /// <returns></returns>
    /// <exception cref="KeyNotFoundException">La persona no se encuentra en el sistema</exception>
    public IEnumerable<PersonBanHistory> GetBanHistory(Guid personId, int take = 25, int offset = 0)
    {
        var person = dbContext.People.FirstOrDefault(p => p.Id == personId)
            ?? throw new KeyNotFoundException("La persona no se encuentra registrada en el sistema");

        return [.. this.dbContext.PersonBanHistories.Where(item => item.PersonId == personId)
            .OrderByDescending( i => i.CreatedAt)
            .Take(take)
            .Skip(offset)];
    }
}