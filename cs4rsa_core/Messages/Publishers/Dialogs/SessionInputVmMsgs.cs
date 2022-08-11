﻿using cs4rsa_core.Dialogs.DialogResults;
using CommunityToolkit.Mvvm.Messaging.Messages;

namespace cs4rsa_core.Messages.Publishers.Dialogs
{
    internal class SessionInputVmMsgs
    {
        /// <summary>
        /// Thoát tìm kiếm account
        /// </summary>
        internal sealed class ExitSearchAccountMsg : ValueChangedMessage<StudentResult>
        {
            public ExitSearchAccountMsg(StudentResult value) : base(value)
            {
            }
        }
    }
}