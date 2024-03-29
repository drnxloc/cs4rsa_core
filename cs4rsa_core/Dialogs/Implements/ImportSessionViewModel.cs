﻿using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;

using Cs4rsa.BaseClasses;
using Cs4rsa.Constants;
using Cs4rsa.Cs4rsaDatabase.Interfaces;
using Cs4rsa.Cs4rsaDatabase.Models;
using Cs4rsa.Dialogs.DialogResults;
using Cs4rsa.Messages.Publishers.Dialogs;
using Cs4rsa.Services.CourseSearchSvc.Crawlers;
using Cs4rsa.Utils;

using MaterialDesignThemes.Wpf;

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;

namespace Cs4rsa.Dialogs.Implements
{
    public partial class ImportSessionViewModel : ViewModelBase
    {
        #region Properties
        public ObservableCollection<UserSchedule> ScheduleSessions { get; set; }
        public ObservableCollection<UserSubject> UserSubjects { get; set; }

        [ObservableProperty]
        private UserSchedule _selectedScheduleSession;

        [ObservableProperty]
        private ScheduleDetail _selectedSessionDetail;

        [ObservableProperty]
        private int _isAvailableSession;

        [ObservableProperty]
        private bool _isUseCache;

        [ObservableProperty]
        private string _shareStringText;

        #endregion

        #region Commands
        public RelayCommand DeleteCommand { get; set; }
        public RelayCommand ImportCommand { get; set; }
        public RelayCommand CloseDialogCommand { get; set; }
        #endregion

        #region Services
        private readonly IUnitOfWork _unitOfWork;
        private readonly CourseCrawler _courseCrawler;
        private readonly ISnackbarMessageQueue _snackbarMessageQueue;
        #endregion

        public ImportSessionViewModel(
            IUnitOfWork unitOfWork,
            CourseCrawler courseCrawler,
            ISnackbarMessageQueue snackbarMessageQueue)
        {
            _unitOfWork = unitOfWork;
            _courseCrawler = courseCrawler;
            _snackbarMessageQueue = snackbarMessageQueue;

            ScheduleSessions = new();
            UserSubjects = new();
            _isAvailableSession = -1;

            DeleteCommand = new(OnDelete, () => _selectedScheduleSession != null);
            ImportCommand = new(
                OnImport,
                () => (UserSubjects.Any() && IsAvailableSession == 1)
                    || (UserSubjects.Count > 0 && !string.IsNullOrWhiteSpace(ShareStringText))
            );
            CloseDialogCommand = new(CloseDialog);
        }

        partial void OnSelectedScheduleSessionChanged(UserSchedule value)
        {
            LoadUserSubject(value);
            CheckIsAvailableSession(value);
            ImportCommand.NotifyCanExecuteChanged();
            DeleteCommand.NotifyCanExecuteChanged();
        }

        public void OnCopyRegisterCode(string registerCode)
        {
            Clipboard.SetText(registerCode);
            _snackbarMessageQueue.Enqueue(VmConstants.SnbCopySuccess + VmConstants.StrSpace + registerCode);
        }

        private void LoadUserSubject(UserSchedule userSchedule)
        {
            if (userSchedule != null)
            {
                UserSubjects.Clear();
                IEnumerable<UserSubject> userSubjects = _unitOfWork.UserSchedules
                    .GetSessionDetails(userSchedule.UserScheduleId)
                    .Select(
                        sd => new UserSubject()
                        {
                            SubjectCode = sd.SubjectCode,
                            SubjectName = sd.SubjectName,
                            ClassGroup = sd.ClassGroup,
                            SchoolClass = sd.SelectedSchoolClass,
                            RegisterCode = sd.RegisterCode
                        }
                    );

                foreach (UserSubject userSubject in userSubjects)
                {
                    UserSubjects.Add(userSubject);
                }
            }
        }

        public void LoadShareString(string shareString)
        {
            UserSubjects.Clear();
            if (!string.IsNullOrWhiteSpace(shareString))
            {
                IEnumerable<UserSubject> userSubjects = ShareString.GetSubjectFromShareString(shareString);
                if (userSubjects != null)
                {
                    foreach (UserSubject userSubject in userSubjects)
                    {
                        UserSubjects.Add(userSubject);
                    }
                    ImportCommand.NotifyCanExecuteChanged();
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
                IsAvailableSession = semester.Equals(_courseCrawler.CurrentSemesterValue, StringComparison.Ordinal)
                    && year.Equals(_courseCrawler.CurrentYearValue, StringComparison.Ordinal) ? available : unavailable;
            }
            else
            {
                IsAvailableSession = -1;
            }
        }

        public void LoadScheduleSession()
        {
            ScheduleSessions.Clear();
            UserSubjects.Clear();
            List<UserSchedule> sessions = _unitOfWork.UserSchedules.GetAll();
            foreach (UserSchedule session in sessions)
            {
                ScheduleSessions.Add(session);
            }
        }

        private void OnImport()
        {
            CloseDialog();
            Messenger.Send(new ImportSessionVmMsgs.ExitImportSubjectMsg(UserSubjects));
        }

        private void OnDelete()
        {
            string sessionName = SelectedScheduleSession.Name;
            MessageBoxResult result = MessageBox.Show(
                  $"Bạn có chắc muốn xoá phiên {sessionName}?"
                , "Thông báo"
                , MessageBoxButton.YesNo
                , MessageBoxImage.Question
            );
            if (result == MessageBoxResult.Yes)
            {
                int removeResult = _unitOfWork.UserSchedules.Remove(SelectedScheduleSession);
                if (removeResult == 1)
                {
                    Reload();
                }
                else
                {
                    _ = MessageBox.Show(
                          CredizText.Common001("Xoá phiên")
                        , "Thông báo"
                        , MessageBoxButton.OK
                        , MessageBoxImage.Error
                    );
                }
            }
        }

        private void Reload()
        {
            ScheduleSessions.Clear();
            UserSubjects.Clear();
            LoadScheduleSession();
        }
    }
}
