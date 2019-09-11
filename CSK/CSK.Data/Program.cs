using System;
using TNT.Core.Template.DataService.Helpers;

namespace CSK.Data
{
    class Program
    {
        static void Main(string[] args)
        {
            GeneralHelper.ExecuteScaffoldFromCmd("../../../", "localhost", "ConSauKho", "sa", "123456",
                "Models", "CSKContext", false);
        }
    }
}
