﻿using Mantle.PhotoGallery.PhotoProcessing.Interfaces;
using Mantle.PhotoGallery.PhotoProcessing.Services;
using Ninject.Modules;

namespace Mantle.PhotoGallery.Processor.Console.Mantle.Default
{
    public class ServiceModule : NinjectModule
    {
        public override void Load()
        {
            Bind<IPhotoCopyService>()
                .To<PhotoCopyService>()
                .InTransientScope();

            Bind<IPhotoThumbnailService>()
                .To<PhotoThumbnailService>()
                .InTransientScope();
        }
    }
}