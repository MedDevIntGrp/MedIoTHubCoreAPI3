using System;
using System.Collections.Generic;
using System.Security.Policy;
using System.Text;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.SignalR;

namespace MedIoTHubCoreAPI3.Services.Model.IoT
{
   public  class Patient
   {
       public string PatientId { get; set; }

       public string LastName { get; set; }

       public string FirstName { get; set; }

       public DateTime DateOfBirth  { get; set; }

       public Url PatientIdentifier { get; set; }
    }
}
