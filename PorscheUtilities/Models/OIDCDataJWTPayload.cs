using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PorscheUtilities.Models
{
    public class OIDCDataJWTPayload
    {
        public string? sub { get; set; }
        public string? name { get; set; }
        public string? given_name { get; set; }
        public string? locale { get; set; }
        public string? family_name { get; set; }
        public string? email { get; set; }
        public string? phone { get; set; }
        public string? centrename { get; set; }
        public List<string> app_roles { get; set; }
    }

    public class app_role
    {
        public string? approle { get; set; }
    }
}
