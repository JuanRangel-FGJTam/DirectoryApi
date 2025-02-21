using System;
using System.Data;

namespace AuthApi.Models;

public class ZipcodeSearchResult
{
    public int ZipCode { get; set; }
    public int CountryId { get; set; }
    public string CountryName { get; set; } = string.Empty;
    public int StateId { get; set; }
    public string State { get; set; } = string.Empty;
    public int MunicipalityId { get; set; }
    public string Municipality { get; set; } = string.Empty;
    public int ColonyId { get; set; }
    public string ColonyName { get; set; } = string.Empty;

    public static ZipcodeSearchResult FromDataReader(IDataReader reader)
    {
        var newItem = new ZipcodeSearchResult();
        newItem.ZipCode = Convert.ToInt32(reader["zipCode"]);
        newItem.CountryId = Convert.ToInt32(reader["countryId"]);
        newItem.CountryName = reader["countryName"].ToString()?? string.Empty;;
        newItem.StateId = Convert.ToInt32(reader["stateId"]);
        newItem.State = reader["state"].ToString() ?? string.Empty;
        newItem.MunicipalityId = Convert.ToInt32(reader["municipalityId"]);
        newItem.Municipality = reader["municipality"].ToString() ?? string.Empty;;
        newItem.ColonyId = Convert.ToInt32(reader["colonyId"]);
        newItem.ColonyName = reader["colonyName"].ToString() ?? string.Empty;;
        return newItem;
    }
}
