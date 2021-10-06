using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AKSWebApi.Core.ErrorHandling
{
    public static class ErrorMessages
    {
        public static string UnexpectedException
        {
            get
            {
                return "An unexpected exception has occurred";
            }
        }

        public static string GenericAuthorisationError
        {
            get
            {
                return "An authorisation error has occurred";
            }
        }
    }
}
