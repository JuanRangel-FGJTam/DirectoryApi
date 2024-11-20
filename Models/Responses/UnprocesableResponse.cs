using System;

namespace AuthApi.Models.Responses {
    public class UnprocesableResponse {
        public string Title {get;set;} = default!;
        public IDictionary<string, string> Errors {get;set;} = default!;

        public UnprocesableResponse(){
            this.Title = "Uno o mas campos tienen error.";
        }

        public UnprocesableResponse(string title, IDictionary<string, string> errors ){
            this.Title = title;
            this.Errors = errors;
            
        }

        public UnprocesableResponse(IDictionary<string, string> errors ){
            this.Title = "Uno o mas campos tienen error.";
            this.Errors = errors;
        }

        public UnprocesableResponse(List<FluentValidation.Results.ValidationFailure> errors ){
            var errorsGrouped = errors.GroupBy( p => p.PropertyName);
            var errorsMaps = errorsGrouped.ToDictionary(g=>g.Key, g=>string.Join(", ", g.Select(i=>i.ErrorMessage)));

            this.Title = "Uno o mas campos tienen error.";
            this.Errors = errorsMaps;
        }

    }
}