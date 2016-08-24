using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Archer.DataSecurity.Filter
{
    public interface IExpressionTranslator
    {
        Item Translate(Item item);
    }
}
