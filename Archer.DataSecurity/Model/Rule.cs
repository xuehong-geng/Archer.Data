using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Archer.DataSecurity.Model
{
    public enum AccessType : int
    {
        ListOnly = 0,       // No Access to content, can only know it is exist
        ReadOnly = 1,       // Can read content
        ReadWrite = 2,      // Can read and write content or update properties
        FullAccess = 3      // Can do everything including delete it
    }

    public class Rule
    {
        public string Id { get; set; }
        public string Resource { get; set; }
        public AccessType AccessType { get; set; }
        public string Filter { get; set; }
    }
}
