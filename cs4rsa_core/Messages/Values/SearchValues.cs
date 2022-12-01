﻿using CommunityToolkit.Mvvm.Messaging.Messages;
using Cs4rsa.Services.SubjectCrawlerSvc.Models;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cs4rsa.Messages.Values
{
    internal sealed class SearchValues
    {
        internal readonly struct UndoDelValue
        {
            public readonly SubjectModel SubjectModel;
            public readonly ClassGroupModel SeletedClassGroupModel;
            public readonly int Index;

            public UndoDelValue(
                SubjectModel subjectModel, 
                ClassGroupModel seletedClassGroupModel,
                int index
            )
            {
                SubjectModel = subjectModel;
                SeletedClassGroupModel = seletedClassGroupModel;
                Index = index;
            }
        }

        internal readonly struct UndoDelAllValue
        {
            public readonly IEnumerable<SubjectModel> SubjectModels;
            public readonly IEnumerable<ClassGroupModel> ClassGroupModels;

            public UndoDelAllValue(IEnumerable<SubjectModel> subjectModels, IEnumerable<ClassGroupModel> classGroupModels)
            {
                SubjectModels = subjectModels;
                ClassGroupModels = classGroupModels;
            }
        }
    }
}
