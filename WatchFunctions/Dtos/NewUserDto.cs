using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WatchFunctions.Dtos
{
    public class NewUserDto
    {
        public string Handle { get; set; } 
        public string Password { get; set; }
        public string? Email { get; set; }
    }
}
