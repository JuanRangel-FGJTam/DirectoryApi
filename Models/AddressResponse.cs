using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using AuthApi.Entities;
using Microsoft.AspNetCore.Mvc.ViewFeatures;

namespace AuthApi.Models
{
    public class AddressResponse
    {

        public string AddressId {get; set;}
        
        public int CountryId {get;set;}
        public string CountryName {get;set;} = "";
        
        public int StateId {get;set;}
        public string StateName {get;set;} = "";
        
        public int? MunicipalityId {get;set;}
        public string? MunicipalityName {get;set;}
       
        public int? ColonyId {get;set;}
        public string? ColonyName {get;set;}
        
        public string? Street {get;set;}

        public string Number {get;set;} = "";
    
        public string? NumberInside {get;set;}

        public int ZipCode {get;set;}

        public AddressResponse( Guid addressID){
            this.AddressId = addressID.ToString();
        }

        public AddressResponse( string addressID){
            this.AddressId = addressID;
        }


        public static AddressResponse FromEntity( Address address){
            var _addressResponse = new AddressResponse( address.Id);

            if( address.Country != null){
                _addressResponse.CountryId = address.Country.Id;
                _addressResponse.CountryName = address.Country.Name;
            }

            if( address.State != null){
                _addressResponse.StateId = address.State.Id;
                _addressResponse.StateName = address.State.Name;
            }

            if( address.Municipality != null){
                _addressResponse.MunicipalityId = address.Municipality.Id;
                _addressResponse.MunicipalityName = address.Municipality.Name;
            }
            
            if( address.Colony != null){
                _addressResponse.ColonyId = address.Colony.Id;
                _addressResponse.ColonyName = address.Colony.Name;
            }
        
            _addressResponse.Street = address.Street;
            _addressResponse.Number = address.Number;
            _addressResponse.NumberInside = address.NumberInside;
            _addressResponse.ZipCode = address.ZipCode;

            return _addressResponse;

        }

    }
}