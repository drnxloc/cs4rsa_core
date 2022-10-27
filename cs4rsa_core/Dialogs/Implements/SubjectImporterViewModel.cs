﻿using cs4rsa_core.BaseClasses;
using cs4rsa_core.Dialogs.DialogResults;
using cs4rsa_core.Messages.Publishers.Dialogs;

using CommunityToolkit.Mvvm.Messaging;

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using cs4rsa_core.Utils;
using cs4rsa_core.Cs4rsaDatabase.Interfaces;
using cs4rsa_core.Services.SubjectCrawlerSvc.Crawlers.Interfaces;
using cs4rsa_core.Services.SubjectCrawlerSvc.DataTypes;
using cs4rsa_core.Services.SubjectCrawlerSvc.Models;

namespace cs4rsa_core.Dialogs.Implements
{
    /// <summary>
    /// <strong>Trình Quản lý Phiên:</strong>
    /// Bộ ra lệnh cho SearchViewModel thực hiện import các Subject được truyền vào.
    /// </summary>
    public class SubjectImporterViewModel : ViewModelBase
    {
        public ObservableCollection<SubjectInfoData> SubjectInfoDatas { get; set; }
        private readonly IKeywordRepository _keywordRepository;
        private readonly ISubjectCrawler _subjectCrawler;
        private readonly ColorGenerator _colorGenerator;

        public SubjectImporterViewModel(
            IKeywordRepository keywordRepository,
            ISubjectCrawler subjectCrawler,
            ColorGenerator colorGenerator)
        {
            SubjectInfoDatas = new();
            _keywordRepository = keywordRepository;
            _subjectCrawler = subjectCrawler;
            _colorGenerator = colorGenerator;
        }

        public async Task Run(SessionManagerResult sessionManagerResult)
        {
            foreach (SubjectInfoData item in sessionManagerResult.SubjectInfoDatas)
            {
                SubjectInfoDatas.Add(item);
            }

            IEnumerable<int> courseIds = sessionManagerResult.SubjectInfoDatas
                                        .Select(item => _keywordRepository
                                        .GetCourseId(item.SubjectCode));
            IEnumerable<Task<SubjectModel>> subjectTasks = courseIds.Select(courseId => ToSubjectModel(courseId));
            SubjectModel[] subjectModels = await Task.WhenAll(subjectTasks);
            ImportResult importResult = new() { SubjectModels = subjectModels.ToList() };
            Tuple<ImportResult, SessionManagerResult> tup = new(importResult, sessionManagerResult);
            SubjectImporterVmMsgs.ExitImportSubjectMsg message = new(tup);
            Messenger.Send(message);
            CloseDialog();
        }

        private async Task<SubjectModel> ToSubjectModel(int courseId)
        {
            Subject subject = await _subjectCrawler.Crawl(courseId);
            await subject.GetClassGroups();
            SubjectModel subjectModel = await SubjectModel.CreateAsync(subject, _colorGenerator);
            subjectModel.Color = await _colorGenerator.GetColorAsync(subject.CourseId);
            return subjectModel;
        }
    }
}
