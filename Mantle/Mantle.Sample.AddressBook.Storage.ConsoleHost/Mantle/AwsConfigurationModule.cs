﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mantle.Aws;
using Ninject.Modules;

namespace Mantle.Sample.AddressBook.Storage.ConsoleHost.Mantle
{
    public class AwsConfigurationModule : NinjectModule
    {
        public override void Load()
        {
            // TODO: Setup your AWS configuration.

            Bind<IAwsConfiguration>()
                .To<AwsConfiguration>()
                .InSingletonScope()
                .OnActivation(c => c.Setup(
                    "Replace this text with your AWS access key.",
                    "Replace this text with your AWS secret key."));
        }
    }
}
