using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ForcaDeVendasMobile.Dependencias
{
    public interface ISharedContent
    {
        void SharedHtml(string html, string name);

        void SaveFile(string html, string name);
    }
}
