﻿using cs4rsa_core.BaseClasses;
using cs4rsa_core.Dialogs.DialogResults;
using cs4rsa_core.Dialogs.MessageBoxService;
using cs4rsa_core.Messages.Publishers.Dialogs;
using cs4rsa_core.ModelExtensions;
using cs4rsa_core.Utils;
using MaterialDesignThemes.Wpf;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows;
using cs4rsa_core.Cs4rsaDatabase.Interfaces;
using cs4rsa_core.Cs4rsaDatabase.Models;
using cs4rsa_core.Services.CourseSearchSvc.Crawlers.Interfaces;

namespace cs4rsa_core.Dialogs.Implements
{
    public class ImportSessionViewModel : ViewModelBase
    {
        #region Properties
        public ObservableCollection<UserSchedule> ScheduleSessions { get; set; }
        public ObservableCollection<ScheduleDetail> ScheduleSessionDetails { get; set; }
        public ObservableCollection<SessionSchoolClass> SessionSchoolClasses { get; set; }

        private UserSchedule _selectedScheduleSession;
        public UserSchedule SelectedScheduleSession
        {
            get => _selectedScheduleSession;
            set
            {
                _selectedScheduleSession = value;
                OnPropertyChanged();
                LoadScheduleSessionDetail(value);
                CheckIsAvailableSession(value);
                ImportCommand.NotifyCanExecuteChanged();
                DeleteCommand.NotifyCanExecuteChanged();
            }
        }

        private ScheduleDetail _selectedSessionDetail;
        public ScheduleDetail SelectedSessionDetail
        {
            get { return _selectedSessionDetail; }
            set
            {
                _selectedSessionDetail = value;
                OnPropertyChanged();
                LoadSessionSchoolClasses(value);
            }
        }

        public string ShareString { get; set; }

        private int _isAvailableSession;
        public int IsAvailableSession
        {
            get => _isAvailableSession;
            set
            {
                _isAvailableSession = value;
                OnPropertyChanged();
            }
        }
        #endregion

        #region Commands
        public AsyncRelayCommand DeleteCommand { get; set; }
        public RelayCommand ImportCommand { get; set; }
        public AsyncRelayCommand ShareStringCommand { get; set; }
        public RelayCommand CloseDialogCommand { get; set; }
        #endregion

        #region Services
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICourseCrawler _courseCrawler;
        private readonly SessionExtension _sessionExtension;
        private readonly IMessageBox _messageBox;
        private readonly ISnackbarMessageQueue _snackbarMessageQueue;
        private readonly ShareString _shareString;
        #endregion

        public ImportSessionViewModel(
            IUnitOfWork unitOfWork, 
            ICourseCrawler courseCrawler,
            IMessageBox messageBox, 
            ISnackbarMessageQueue snackbarMessageQueue,
            SessionExtension sessionExtension,
            ShareString shareString)
        {
            _messageBox = messageBox;
            _sessionExtension = sessionExtension;
            _unitOfWork = unitOfWork;
            _courseCrawler = courseCrawler;
            _snackbarMessageQueue = snackbarMessageQueue;
            _shareString = shareString;

            ScheduleSessions = new();
            ScheduleSessionDetails = new();
            SessionSchoolClasses = new();
            _isAvailableSession = -1;

            DeleteCommand = new AsyncRelayCommand(OnDelete, CanDelete);
            ShareStringCommand = new AsyncRelayCommand(OnParseShareString);
            ImportCommand = new RelayCommand(OnImport, CanImport);
            CloseDialogCommand = new RelayCommand(CloseDialog);
        }

        private void LoadScheduleSessionDetail(UserSchedule value)
        {
            if (value != null)
            {
                ScheduleSessionDetails.Clear();
                SessionSchoolClasses.Clear();
                IEnumerable<ScheduleDetail> details = _unitOfWork.Sessions.GetSessionDetails(value.SessionId);
                foreach (ScheduleDetail item in details)
                {
                    ScheduleSessionDetails.Add(item);
                }
            }
        }

        private void LoadSessionSchoolClasses(ScheduleDetail value)
        {
            if (value != null)
            {
                SessionSchoolClasses.Clear();
                IEnumerable<SessionSchoolClass> sessionSchoolClasses = _unitOfWork
                    .SessionSchoolClasses
                    .GetSessionSchoolClass(value);
                foreach (SessionSchoolClass sessionSchoolClass in sessionSchoolClasses)
                {
                    SessionSchoolClasses.Add(sessionSchoolClass);
                }
            }
        }

        private void CheckIsAvailableSession(UserSchedule session)
        {
            if (session != null)
            {
                string semester = session.SemesterValue;
                string year = session.YearValue;
                int available = 1;
                int unavailable = 0;
                IsAvailableSession = semester.Equals(_courseCrawler.GetCurrentSemesterValue(), StringComparison.Ordinal)
                    && year.Equals(_courseCrawler.GetCurrentYearValue(), StringComparison.Ordinal) ? available : unavailable;
            }
            else
            {
                IsAvailableSession = -1;
            }
        }

        private async Task OnParseShareString()
        {
            SessionManagerResult result = await _shareString.GetSubjectFromShareString(ShareString);
            if (result != null)
            {
                CloseDialog();
                Messenger.Send(new ImportSessionVmMsgs.ExitImportSubjectMsg(result));
            }
            else
            {
                ShareString = "";
            }
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
            IEnumerable<UserSchedule> sessions = await _unitOfWork.Sessions.GetAllAsync();
            foreach (UserSchedule session in sessions)
            {
                ScheduleSessions.Add(session);
            }
        }

        private void OnImport()
        {
            List<SubjectInfoData> subjectInfoDatas = new();
            foreach (ScheduleDetail item in ScheduleSessionDetails)
            {
                SubjectInfoData data = new()
                {
                    item.SubjectCode,
                    item.ClassGroup,
                    item.SubjectName,
                    item.RegisterCode,
                    item.Sch
                };
                subjectInfoDatas.Add(data);
            }
            SessionManagerResult result = new(subjectInfoDatas);
            CloseDialog();
            Messenger.Send(new ImportSessionVmMsgs.ExitImportSubjectMsg(result));
        }

        private async Task OnDelete()
        {
            string sessionName = _selectedScheduleSession.Name;
            MessageBoxResult result = _messageBox.ShowMessage($"Bạn có chắc muốn xoá phiên {sessionName}?",
                                    "Thông báo",
                                    MessageBoxButton.YesNo,
                                    MessageBoxImage.Question);
            if (result == MessageBoxResult.Yes)
            {
                await _unitOfWork.BeginTransAsync();
                _unitOfWork.Sessions.Remove(_selectedScheduleSession);
                await _unitOfWork.CompleteAsync();
                await _unitOfWork.CommitAsync();
                await Reload();
            }
        }

        private async Task Reload()
        {
            ScheduleSessions.Clear();
            ScheduleSessionDetails.Clear();
            SessionSchoolClasses.Clear();
            await LoadScheduleSession();
        }

        /// <summary>
        /// Sao chép mã đăng ký của lớp hiện tại.
        /// </summary>
        public void OnCopyRegisterCodeAtCurrentClassGroup()
        {
            string registerCode = _selectedSessionDetail.RegisterCode;
            Clipboard.SetData(DataFormats.Text, registerCode);
            string message = $"Đã sao chép {_selectedSessionDetail.SubjectCode}::{_selectedSessionDetail.RegisterCode}";
            _snackbarMessageQueue.Enqueue(message);
        }
    }
}
