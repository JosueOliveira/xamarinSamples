using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks; 

namespace ForcaDeVendasMobile.Dependencias
{
    public interface IPermission
    {
        bool CheckLocationPermission();

        bool SuporteDigitalImplementation();

        bool CheckStoragePermisssion();

        void RequestStoragePersmission();
    }
}
