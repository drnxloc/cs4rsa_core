﻿using cs4rsa_core.BaseClasses;
using cs4rsa_core.Dialogs.DialogResults;
using cs4rsa_core.Messages.Publishers.Dialogs;
using Cs4rsaDatabaseService.Interfaces;

using CommunityToolkit.Mvvm.Messaging;

using HelperService;

using SubjectCrawlService1.Crawlers.Interfaces;
using SubjectCrawlService1.DataTypes;
using SubjectCrawlService1.Models;

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace cs4rsa_core.Dialogs.Implements
{
    /// <summary>
    /// <strong>Trình Quản lý Phiên:</strong>
    /// Bộ ra lệnh cho SearchViewModel thực hiện import các Subject được truyền vào.
    /// </summary>
    public class SubjectImporterViewModel : ViewModelBase
    {
        public ObservableCollection<SubjectInfoData> SubjectInfoDatas { get; set; } = new ObservableCollection<SubjectInfoData>();
        public List<ISubjectCrawler> SubjectCrawlers { get; set; }

        private SessionManagerResult _sessionManagerResult;

        public SessionManagerResult SessionManagerResult
        {
            get { return _sessionManagerResult; }
            set
            {
                _sessionManagerResult = value;
                foreach (SubjectInfoData item in _sessionManagerResult.SubjectInfoDatas)
                {
                    SubjectInfoDatas.Add(item);
                }
            }
        }

        private readonly IKeywordRepository _keywordRepository;
        private readonly ISubjectCrawler _subjectCrawler;
        private readonly ColorGenerator _colorGenerator;

        public SubjectImporterViewModel(IKeywordRepository keywordRepository, ISubjectCrawler subjectCrawler)
        {
            _keywordRepository = keywordRepository;
            _subjectCrawler = subjectCrawler;
            _colorGenerator = new ColorGenerator(keywordRepository);
        }

        public async Task Run()
        {
            IEnumerable<int> courseIds = _sessionManagerResult.SubjectInfoDatas
                                        .Select(item => _keywordRepository
                                        .GetCourseId(item.SubjectCode));
            IEnumerable<Task<SubjectModel>> subjectTasks = courseIds.Select(courseId => ToSubjectModel(courseId));
            SubjectModel[] subjectModels = await Task.WhenAll(subjectTasks);
            ImportResult importResult = new() { SubjectModels = subjectModels.ToList() };
            Tuple<ImportResult, SessionManagerResult> tup = new(importResult, _sessionManagerResult);
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
