﻿using Mantle.Azure;
using Ninject.Modules;

namespace Mantle.Sample.AddressBook.Storage.ConsoleHost.Mantle
{
    public class AzureConfigurationModule : NinjectModule
    {
        public override void Load()
        {
            // TODO: Setup your Azure service bus configuration.

            Bind<IAzureServiceBusConfiguration>()
                .To<AzureServiceBusConfiguration>()
                .InSingletonScope()
                .OnActivation(
                    c =>
                    c.Setup(
                        "Replace this text with your Azure service bus connection string."));

            // TODO: Setup your Azure storage configuration.

            Bind<IAzureStorageConfiguration>()
                .To<AzureStorageConfiguration>()
                .InSingletonScope()
                .OnActivation(
                    c =>
                    c.Setup(
                        "Replace this text with your Azure storage connection string."));
        }
    }
}