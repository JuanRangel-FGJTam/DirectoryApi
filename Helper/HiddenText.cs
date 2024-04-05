using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace AuthApi.Helper
{
    public class HiddenText
    {
        public static string Hidden(string name){
            var parts = name.Split(' ');
            var textBuilder = new System.Text.StringBuilder();
            foreach( var part in parts){
                textBuilder.Append( string.Format("{0}{1} ", 
                    part[0],
                    new string('*', part.Length - 1)
                ));
            }
            return textBuilder.ToString();
        }
    }
}