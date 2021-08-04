﻿using cs4rsa.BaseClasses;
using cs4rsa.BasicData;
using cs4rsa.Crawler;
using cs4rsa.Database;
using cs4rsa.Dialogs.DialogResults;
using cs4rsa.Dialogs.DialogService;
using cs4rsa.Dialogs.DialogViews;
using cs4rsa.Dialogs.Implements;
using cs4rsa.Dialogs.MessageBoxService;
using cs4rsa.Messages;
using cs4rsa.Models;
using cs4rsa.Views;
using LightMessageBus;
using LightMessageBus.Interfaces;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Windows;
using System.Windows.Controls;

namespace cs4rsa.ViewModels
{
    public class MainSchedulingViewModel : ViewModelBase,
        IMessageHandler<SubjectItemChangeMessage>,
        IMessageHandler<ChoicesChangedMessage>,
        IMessageHandler<UpdateSubjectDatabase>
    {
        private string _currentYearInfo;
        private string _currentSemesterInfo;

        public string CurrentYearInfo
        {
            get
            {
                return _currentYearInfo;
            }
            set
            {
                _currentYearInfo = value;
                OnPropertyChanged();
            }
        }
        public string CurrentSemesterInfo
        {
            get
            {
                return _currentSemesterInfo;
            }
            set
            {
                _currentSemesterInfo = value;
                OnPropertyChanged();
            }
        }

        private int _totalCredit = 0;
        public int TotalCredit
        {
            get
            {
                return _totalCredit;
            }
            set
            {
                _totalCredit = value;
                OnPropertyChanged();
            }
        }

        private int _totalSubject = 0;
        public int TotalSubject
        {
            get
            {
                return _totalSubject;
            }
            set
            {
                _totalSubject = value;
                OnPropertyChanged();
            }
        }

        private readonly IMessageBox _messageBox;
        private string _shareString;

        public RelayCommand OpenSettingCommand { get; set; }
        public RelayCommand OpenUpdateWindowCommand { get; set; }
        public RelayCommand OpenAutoScheduling { get; set; }
        public RelayCommand OpenShareStringWindowCommand { get; set; }

        public MainSchedulingViewModel(IMessageBox messageBox)
        {
            MessageBus.Default.FromAny().Where<SubjectItemChangeMessage>().Notify(this);
            MessageBus.Default.FromAny().Where<ChoicesChangedMessage>().Notify(this);
            MessageBus.Default.FromAny().Where<UpdateSubjectDatabase>().Notify(this);
            _messageBox = messageBox;
            HomeCourseSearch homeCourseSearch = HomeCourseSearch.GetInstance();
            CurrentSemesterInfo = homeCourseSearch.CurrentSemesterInfo;
            CurrentYearInfo = homeCourseSearch.CurrentYearInfo;
            OpenUpdateWindowCommand = new RelayCommand(OnOpenUpdateWindow, () => true);
            OpenSettingCommand = new RelayCommand(OnOpenSetting, () => true);
            OpenShareStringWindowCommand = new RelayCommand(OnOpenShareStringWindow, () => true);
            TotalCredit = 0;
            TotalSubject = 0;
        }

        private void OnOpenShareStringWindow(object obj)
        {
            //ShareStringWindow shareStringWindow = new ShareStringWindow();
            //ShareStringViewModel shareStringViewModel = new ShareStringViewModel();
            //DialogService<ShareStringResult>.OpenDialog(shareStringViewModel, shareStringWindow, obj as Window);

            ShareStringUC shareStringUC = new ShareStringUC();
            ShareStringViewModel vm = (shareStringUC.DataContext as ShareStringViewModel);
            vm.ShareString = _shareString;
            vm.CloseDialogCallback = (App.Current.MainWindow.DataContext as MainViewModel).CloseDialog;
            (App.Current.MainWindow.DataContext as MainViewModel).OpenDialog(shareStringUC);
        }

        private void OnOpenUpdateWindow(object obj)
        {
            //Cs4rsaMessageBox cs4RsaMessageBox = new Cs4rsaMessageBox();
            //UpdateWindow updateWindow = new UpdateWindow();
            //UpdateViewModel updateViewModel = new UpdateViewModel(cs4RsaMessageBox, () => { (App.Current.MainWindow.DataContext as MainViewModel).CloseDialog()});
            //UpdateResult result = DialogService<UpdateResult>.OpenDialog(updateViewModel, updateWindow, obj as Window);
            //MessageBus.Default.Publish(new UpdateSuccessMessage(null));

            UpdateUC updateUC = new UpdateUC();
            (updateUC.DataContext as UpdateViewModel).CloseDialogCallback = (App.Current.MainWindow.DataContext as MainViewModel).CloseDialog;
            (App.Current.MainWindow.DataContext as MainViewModel).OpenDialog(updateUC);
        }



        public void Handle(SubjectItemChangeMessage message)
        {
            TotalCredit = message.Source.TotalCredits;
            TotalSubject = message.Source.TotalSubject;
        }

        private void OnOpenSetting(object obj)
        {
            SettingWindow settingWindow = new SettingWindow();
            SettingViewModel settingViewModel = new SettingViewModel();
            DialogService<SettingResult>.OpenDialog(settingViewModel, settingWindow, obj as Window);
        }

        public void Handle(ChoicesChangedMessage message)
        {
            List<ClassGroupModel> classGroupModels = message.Source;
            _shareString = Helpers.ShareString.GetShareString(classGroupModels);
        }

        public void Handle(UpdateSubjectDatabase message)
        {
            OnOpenUpdateWindow(null);
        }
    }
}
