using System;
using System.Threading.Tasks;


namespace R5T.W0005.Deploy
{
    class Program
    {
        static async Task Main()
        {
            //await DeployScripts.Instance.DeployToLocalCloudBinariesDirectory();
            await DeployScripts.Instance.DeployToRemoteBinariesDirectory();
            // await DeployScripts.Instance.RunCertbotRenewal();
        }
    }
}
