using System;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

using R5T.F0000;
using R5T.F0030;
using R5T.L0029;
using R5T.T0132;


namespace R5T.W0005.Deploy
{
    [FunctionalityMarker]
    public partial interface IDeployScripts : IFunctionalityMarker
    {
        public async Task RunCertbotRenewal()
        {
            var remoteServerFriendlyName = "TechnicalBlog";

            var remoteServerAuthentication = Instances.AwsAuthenticationOperator.GetRemoteServerAuthentication(
                Instances.FilePaths.AwsRemoteServerConfigurationJsonFilePath,
                remoteServerFriendlyName);

            await Instances.LoggingOperator.InConsoleLoggerContext(
                nameof(DeployToLocalCloudBinariesDirectory),
                async logger =>
                {
                    await SshOperator.Instance.InConnectionContext(
                        remoteServerAuthentication,
                        async connection =>
                        {
                            await SshOperator.Instance.InSshContext_Connected(
                                connection,
                                sshClient =>
                                {
                                    logger.LogInformation("Renewing certbot certs renewal...");

                                    var renewCommand = sshClient.RunCommand($"sudo certbot renew");

                                    RemoteCommandOperator.Instance.LogCommandResult(renewCommand, logger);

                                    if(RemoteCommandOperator.Instance.IsFailure(renewCommand))
                                    {
                                        logger.LogInformation("FAILED to renew certbot certs.");
                                    }
                                    else
                                    {
                                        logger.LogInformation("Renewed certbot certs.");
                                    }

                                    return Task.CompletedTask;
                                });
                        });
                });
        }

        public async Task DeployToLocalCloudBinariesDirectory()
        {
            /// Inputs.
            var targetProjectName = "R5T.W0005";
            var localCloudBinariesDirectoryPath = Instances.DirectoryPaths.LocalCloudBinariesDirectoryPath;


            /// Run.
            var targetProjectFilePath = Instances.ProjectPathConventions.GetExecutableSiblingProjectFilePath(targetProjectName);

            var currentBinariesDirectoryPath = await Instances.LoggingOperator.InConsoleLoggerContext(
                nameof(DeployToLocalCloudBinariesDirectory),
                async logger =>
                {
                    var currentBinariesDirectoryPath = await Instances.PublicationOperator.Publish(
                        targetProjectFilePath,
                        localCloudBinariesDirectoryPath,
                        logger);

                    return currentBinariesDirectoryPath;
                });

            Instances.WindowsExplorerOperator.OpenDirectoryInExplorer(currentBinariesDirectoryPath);
        }

        /// <summary>
        /// Requires <see cref="DeployToLocalCloudBinariesDirectory"></see> to be run first.
        /// </summary>
        public async Task DeployToRemoteBinariesDirectory()
        {
            /// Inputs.
            var targetProjectName = "R5T.W0005";
            var remoteServerFriendlyName = "TechnicalBlog";
            var remoteDeployDirectoryPath = $@"/var/www/{targetProjectName}";

            var archiveFileName = @"Archive.zip";
            var localTemporaryDirectoryPath = @"C:\Temp";
            var remoteTemporaryDirectoryPath = @"/home/ec2-user";
            var localCloudBinariesDirectoryPath = Instances.DirectoryPaths.LocalCloudBinariesDirectoryPath;


            /// Run.
            var awsRemoteServerAuthentication = Instances.AwsAuthenticationOperator.GetRemoteServerAuthentication(
                Instances.FilePaths.AwsRemoteServerConfigurationJsonFilePath,
                remoteServerFriendlyName);

            var targetProjectFilePath = Instances.ProjectPathConventions.GetExecutableSiblingProjectFilePath(targetProjectName);

            var currentBinariesDirectoryPath = Instances.PublicationPathsOperator.GetCurrentBinariesOutputDirectoryPath(
                localCloudBinariesDirectoryPath,
                targetProjectFilePath);

            var remoteDeployContext = new RemoteDeployContext
            {
                ArchiveFileName = archiveFileName,
                DestinationRemoteBinariesDirectoryPath = remoteDeployDirectoryPath,
                LocalTemporaryDirectoryPath = localTemporaryDirectoryPath,
                RemoteTemporaryDirectoryPath = remoteTemporaryDirectoryPath,
                SourceLocalBinariesDirectoryPath = currentBinariesDirectoryPath,
            };

            await Instances.LoggingOperator.InConsoleLoggerContext(
                nameof(DeployToLocalCloudBinariesDirectory),
                async logger =>
                {
                    await DeployOperator.Instance.DeployToRemote(
                        awsRemoteServerAuthentication,
                        remoteDeployContext,
                        EnumerableOperator.Instance.From(
                            RemoteDeployActions.Instance.None),
                        EnumerableOperator.Instance.From(
                            RemoteDeployActions.Instance.ChangePermissionsOnRemoteDirectory(logger)),
                        logger);
                });
        }
    }
}
