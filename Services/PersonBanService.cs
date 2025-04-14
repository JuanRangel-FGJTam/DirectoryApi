using System;
using AuthApi.Data;
using AuthApi.Entities;

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

    public (bool Success, string Message, Person? Person) BanPerson(Guid personId)
    {
        var person = dbContext.People.FirstOrDefault(p => p.Id == personId);

        if (person == null)
            return (false, "La persona no se encuentra registrada en el sistema.", null);

        if (person.BannedAt != null)
            return (false, "La persona ya ha sido baneada.", person);

        person.BannedAt = DateTime.UtcNow;
        dbContext.People.Update(person);
        dbContext.SaveChanges();

        this.logger.LogInformation("The person [{id}|{name}] was banned.", person.Id, person.FullName);

        return (true, "La persona fue baneada correctamente.", person);
    }

    public (bool Success, string Message, Person? Person) UnbanPerson(Guid personId)
    {
        var person = dbContext.People.FirstOrDefault(p => p.Id == personId);

        if (person == null)
            return (false, "La persona no se encuentra registrada en el sistema.", null);

        if (person.BannedAt == null)
            return (false, "La persona no está baneada actualmente.", person);

        person.BannedAt = null;
        dbContext.People.Update(person);
        dbContext.SaveChanges();

        this.logger.LogInformation("The person [{id}|{name}] was unbanned.", person.Id, person.FullName );

        return (true, "La persona fue desbaneada correctamente.", person);
    }
}
