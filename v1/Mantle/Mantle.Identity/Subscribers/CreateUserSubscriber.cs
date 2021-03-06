﻿using Mantle.Identity.Commands;
using Mantle.Identity.Interfaces;
using Mantle.Messaging.Interfaces;
using Mantle.Messaging.Subscribers;

namespace Mantle.Identity.Subscribers
{
    public class CreateUserSubscriber : BaseSubscriber<CreateUser>
    {
        private readonly IMantleUserRepository<MantleUser> userRepository;

        public CreateUserSubscriber(IMantleUserRepository<MantleUser> userRepository)
        {
            this.userRepository = userRepository;
        }

        public override void HandleMessage(IMessageContext<CreateUser> messageContext)
        {
            var user = messageContext.Message.User;

            OnMessageOccurred(messageContext,
                              $"[{nameof(CreateUserSubscriber)}]: Creating user " +
                              $"[{user.Id}/{user.UserName}]...");

            userRepository.CreateUser(messageContext.Message.User);

            OnMessageOccurred(messageContext,
                              $"[{nameof(CreateUserSubscriber)}]: Created user " +
                              $"[{user.Id}/{user.UserName}].");
        }
    }
}