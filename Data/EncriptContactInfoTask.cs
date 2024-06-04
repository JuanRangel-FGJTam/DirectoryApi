using System;
using System.Data.SqlClient;
using System.Data.Common;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace AuthApi.Data {
    public class EncriptContactInfoTask(DirectoryDBContext dbContext, ICryptographyService cryptographyService)
    {

        private readonly DirectoryDBContext dBContext = dbContext;
        private readonly ICryptographyService cryptographyService = cryptographyService;

        public void Run(){
            
            using(var sqlConnection = new SqlConnection( dBContext.Database.GetConnectionString() )){
                sqlConnection.Open();

                // Seek if the data is already encripted
                string getFlag = "Select name, value From [System].[Flags] where name = 'ENCRIPTED-CONTACT-INFO'";
                var encriptedContactInfoFlag = false;
                using( var command = new SqlCommand( getFlag, sqlConnection )){
                    using(var dataReader = command.ExecuteReader()){
                        if(dataReader.Read()) {
                            encriptedContactInfoFlag = Convert.ToBoolean(dataReader["value"]);
                        }else{
                            throw new Exception("No response at retrive system flag");
                        }
                    }
                }
                
                if(encriptedContactInfoFlag){
                    sqlConnection.Close();
                    return;    
                }


                // Retrive all the original data
                var oldvalues = new Dictionary<string,string>();
                string _getContactInfoValues = "Select id, value From [dbo].[ContactInformations]";
                using( var command = new SqlCommand( _getContactInfoValues, sqlConnection )){
                    using(var dataReader = command.ExecuteReader()){
                        while (dataReader.Read()) {
                            oldvalues.Add(dataReader["id"].ToString()! , dataReader["value"].ToString()!);
                        }
                    }
                }

                // Proccess the old data
                var newValues = new Dictionary<string,string>();
                foreach( var item in oldvalues){
                    newValues.Add(item.Key, this.cryptographyService.EncryptData(item.Value));
                }

                
                // Update the data
                foreach( var valEncripted in newValues){
                    string _queryUpdate = $"Update [dbo].[ContactInformations] Set value='{valEncripted.Value}' Where id='{valEncripted.Key}'";
                    using(var command = new SqlCommand( _queryUpdate, sqlConnection )){
                        command.ExecuteNonQuery();
                    }
                }

                // Update the system flag
                var _updateFlag = "Update [System].[Flags] set value = 1 Where name = 'ENCRIPTED-CONTACT-INFO'";
                var commandF = new SqlCommand(_updateFlag, sqlConnection);
                commandF.ExecuteNonQuery();

                sqlConnection.Close();
            }
        }

    }
}