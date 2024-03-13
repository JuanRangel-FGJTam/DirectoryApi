using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace AuthApi.Data.Exceptions
{
    public class SimpleValidationException : ValidationException
    {
        private ICollection<KeyValuePair<string, string>> _validationErrors = [];
        public ICollection<KeyValuePair<string, string>> ValidationErrors {get => _validationErrors;}
        
        public SimpleValidationException(string message, ICollection<KeyValuePair<string, string>> erros)
            : base(message, null)
        {
            this._validationErrors = erros.ToDictionary<string,string>();
        }

        public SimpleValidationException(string message, ICollection<KeyValuePair<string, string>> erros, Exception? inner)
            : base(message, inner)
        {
            this._validationErrors = erros.ToDictionary<string,string>();
        }

    }
}