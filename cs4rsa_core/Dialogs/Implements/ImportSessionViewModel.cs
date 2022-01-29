﻿using CourseSearchService.Crawlers.Interfaces;
using cs4rsa_core.BaseClasses;
using cs4rsa_core.Dialogs.DialogResults;
using cs4rsa_core.Dialogs.MessageBoxService;
using cs4rsa_core.Messages;
using cs4rsa_core.ModelExtensions;
using cs4rsa_core.Utils;
using cs4rsa_core.ViewModels;
using Cs4rsaDatabaseService.Interfaces;
using Cs4rsaDatabaseService.Models;
using LightMessageBus;
using Microsoft.Toolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace cs4rsa_core.Dialogs.Implements
{
    public class ImportSessionViewModel : ViewModelBase
    {
        public ObservableCollection<Session> ScheduleSessions { get; set; }
        public ObservableCollection<SessionDetail> ScheduleSessionDetails { get; set; }

        private Session _selectedScheduleSession;
        public Session SelectedScheduleSession
        {
            get => _selectedScheduleSession;
            set
            {
                _selectedScheduleSession = value;
                OnPropertyChanged();
                LoadScheduleSessionDetail(value);
                ImportCommand.NotifyCanExecuteChanged();
                DeleteCommand.NotifyCanExecuteChanged();
            }
        }
        public string ShareString { get; set; }

        public RelayCommand DeleteCommand { get; set; }
        public RelayCommand ImportCommand { get; set; }
        public RelayCommand ShareStringCommand { get; set; }
        public RelayCommand CloseDialogCommand { get; set; }

        public Action<SessionManagerResult> CloseDialogCallback { get; set; }

        private readonly IUnitOfWork _unitOfWork;
        private readonly ICourseCrawler _courseCrawler;
        private readonly SessionExtension _sessionExtension;
        private readonly IMessageBox _messageBox;
        public ImportSessionViewModel(IUnitOfWork unitOfWork, ICourseCrawler courseCrawler, 
            SessionExtension sessionExtension, IMessageBox messageBox)
        {
            _messageBox = messageBox;
            _sessionExtension = sessionExtension;
            _unitOfWork = unitOfWork;
            _courseCrawler = courseCrawler;

            ScheduleSessions = new();
            ScheduleSessionDetails = new();

            DeleteCommand = new RelayCommand(OnDelete, CanDelete);
            ImportCommand = new RelayCommand(OnImport, CanImport);
            ShareStringCommand = new RelayCommand(OnParseShareString, CanParse);
            CloseDialogCommand = new RelayCommand(OnCloseDialog);
        }

        public void LoadScheduleSessionDetail(Session value)
        {
            if (value != null)
            {
                ScheduleSessionDetails.Clear();
                IEnumerable<SessionDetail> details = _unitOfWork.Sessions.GetSessionDetails(value.SessionId);
                foreach (SessionDetail item in details)
                {
                    ScheduleSessionDetails.Add(item);
                }
            }
        }

        private void OnCloseDialog()
        {
            (Application.Current.MainWindow.DataContext as MainWindowViewModel).CloseDialog();
        }

        private bool CanParse()
        {
            return true;
        }

        private void OnParseShareString()
        {
            ShareString shareString = new(_unitOfWork, _courseCrawler);
            SessionManagerResult result = shareString.GetSubjectFromShareString(ShareString);
            (Application.Current.MainWindow.DataContext as MainWindowViewModel).CloseDialog();
            MessageBus.Default.Publish(new ExitImportSubjectMessage(result));
        }

        private bool CanImport()
        {
            if (_selectedScheduleSession != null && _sessionExtension.IsValid(_selectedScheduleSession))
            {
                return true;
            }
            return false;
        }

        private bool CanDelete()
        {
            return _selectedScheduleSession != null;
        }

        public async Task LoadScheduleSession()
        {
            ScheduleSessions.Clear();
            IEnumerable<Session> sessions = await _unitOfWork.Sessions.GetAllAsync();
            sessions.ToList().ForEach(session => ScheduleSessions.Add(session));
        }

        private void OnImport()
        {
            List<SubjectInfoData> subjectInfoDatas = new();
            foreach (SessionDetail item in ScheduleSessionDetails)
            {
                SubjectInfoData data = new()
                {
                    SubjectCode = item.SubjectCode,
                    ClassGroup = item.ClassGroup,
                    SubjectName = item.SubjectName
                };
                subjectInfoDatas.Add(data);
            }
            SessionManagerResult result = new(subjectInfoDatas);
            (Application.Current.MainWindow.DataContext as MainWindowViewModel).CloseDialog();
            MessageBus.Default.Publish(new ExitImportSubjectMessage(result));
        }

        private void OnDelete()
        {
            string sessionName = _selectedScheduleSession.Name;
            MessageBoxResult result = _messageBox.ShowMessage($"Bạn có chắc muốn xoá phiên {sessionName}?",
                                    "Thông báo",
                                    MessageBoxButton.YesNo,
                                    MessageBoxImage.Question);
            if (result == MessageBoxResult.Yes)
            {
                _unitOfWork.Sessions.Remove(_selectedScheduleSession);
                _unitOfWork.Complete();
                Reload();
            }
        }

        private void Reload()
        {
            ScheduleSessions.Clear();
            ScheduleSessionDetails.Clear();
            LoadScheduleSession();
        }
    }
}
