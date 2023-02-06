﻿
using CommunityToolkit.Mvvm.Messaging;

using Cs4rsa.Constants;
using Cs4rsa.Cs4rsaDatabase.DataProviders;
using Cs4rsa.Cs4rsaDatabase.Implements;
using Cs4rsa.Cs4rsaDatabase.Interfaces;
using Cs4rsa.Dialogs.Implements;
using Cs4rsa.ModelExtensions;
using Cs4rsa.Services.CourseSearchSvc.Crawlers;
using Cs4rsa.Services.CourseSearchSvc.Crawlers.Interfaces;
using Cs4rsa.Services.CurriculumCrawlerSvc.Crawlers;
using Cs4rsa.Services.CurriculumCrawlerSvc.Crawlers.Interfaces;
using Cs4rsa.Services.DisciplineCrawlerSvc.Crawlers;
using Cs4rsa.Services.ProgramSubjectCrawlerSvc.Crawlers;
using Cs4rsa.Services.ProgramSubjectCrawlerSvc.Interfaces;
using Cs4rsa.Services.StudentCrawlerSvc.Crawlers;
using Cs4rsa.Services.StudentCrawlerSvc.Crawlers.Interfaces;
using Cs4rsa.Services.SubjectCrawlerSvc.Crawlers;
using Cs4rsa.Services.SubjectCrawlerSvc.Crawlers.Interfaces;
using Cs4rsa.Services.TeacherCrawlerSvc.Crawlers;
using Cs4rsa.Services.TeacherCrawlerSvc.Crawlers.Interfaces;
using Cs4rsa.Settings;
using Cs4rsa.Settings.Interfaces;
using Cs4rsa.Utils;
using Cs4rsa.Utils.Interfaces;
using Cs4rsa.ViewModels;
using Cs4rsa.ViewModels.AutoScheduling;
using Cs4rsa.ViewModels.Database;
using Cs4rsa.ViewModels.ManualScheduling;

using HtmlAgilityPack;

using MaterialDesignThemes.Wpf;

using Microsoft.Extensions.DependencyInjection;

using System;
using System.Net;
using System.Windows;

namespace Cs4rsa
{
    public sealed partial class App : Application
    {
        public IServiceProvider Container { get; set; }
        public IMessenger Messenger { get; set; }
        public App()
        {
            //this.Dispatcher.UnhandledException += Dispatcher_UnhandledException;
            //Application.Current.DispatcherUnhandledException += Current_DispatcherUnhandledException;
        }

        #region Error Handlers
        //private void Current_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        //{
        //    string errorMessage = string.Format("Second 02 Current_DispatcherUnhandledException An unhandled exception occurred: {0}", e.Exception.Message);
        //    MessageBox.Show(errorMessage, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        //    // OR whatever you want like logging etc. MessageBox it's just example
        //    // for quick debugging etc.
        //    e.Handled = true;
        //}

        //private void Dispatcher_UnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        //{
        //    string errorMessage = string.Format("First 04 Dispatcher_UnhandledException An unhandled exception occurred: {0}", e.Exception.Message);
        //    MessageBox.Show(errorMessage, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        //    // OR whatever you want like logging etc. MessageBox it's just example
        //    // for quick debugging etc.
        //    e.Handled = true;
        //}
        #endregion

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            Container = CreateServiceProvider();
            Messenger = WeakReferenceMessenger.Default;

            ISetting setting = Container.GetRequiredService<ISetting>();
            string isDatabaseCreated = setting.Read(VMConstants.STPROPS_IS_DATABASE_CREATED);
            if (isDatabaseCreated == "false")
            {
                Container.GetRequiredService<Cs4rsaDbContext>().Database.EnsureCreated();
                Container.GetService<DisciplineCrawler>().GetDisciplineAndKeyword();
                setting.CurrentSetting.IsDatabaseCreated = "true";
                setting.Save();
            }

            // Create the init folders
            IFolderManager folderManager = Container.GetRequiredService<IFolderManager>();
            folderManager.CreateFoldersAtStartUp();
        }

        private static IServiceProvider CreateServiceProvider()
        {
            IServiceCollection services = new ServiceCollection();
            services.AddDbContext<Cs4rsaDbContext>();
            services.AddSingleton<ICurriculumRepository, CurriculumRepository>();
            services.AddSingleton<IDisciplineRepository, DisciplineRepository>();
            services.AddSingleton<IKeywordRepository, KeywordRepository>();
            services.AddSingleton<IStudentRepository, StudentRepository>();
            services.AddSingleton<IProgramSubjectRepository, ProgramSubjectRepository>();
            services.AddSingleton<IPreParSubjectRepository, PreParSubjectRepository>();
            services.AddSingleton<IPreProDetailsRepository, PreProDetailRepository>();
            services.AddSingleton<IParProDetailsRepository, ParProDetailRepository>();
            services.AddTransient<IUnitOfWork, UnitOfWork>();

            services.AddSingleton<ICourseCrawler, CourseCrawler>();
            services.AddSingleton<ICurriculumCrawler, CurriculumCrawler>();
            services.AddSingleton<ITeacherCrawler, TeacherCrawler>();
            services.AddSingleton<ISubjectCrawler, SubjectCrawler>();
            services.AddSingleton<IPreParSubjectCrawler, PreParSubjectCrawler>();
            services.AddSingleton<IDtuStudentInfoCrawler, DtuStudentInfoCrawlerV2>();
            services.AddSingleton<IStudentProgramCrawler, StudentProgramCrawler>();
            services.AddSingleton<IStudentPlanCrawler, StudentPlanCrawler>();
            services.AddSingleton<DisciplineCrawler>();

            services.AddSingleton<ShareString>();
            services.AddSingleton<ColorGenerator>();
            services.AddSingleton<ShareString>();

            HtmlWeb htmlWeb = new()
            {
                PreRequest = delegate (HttpWebRequest wr)
                {
                    // Set timeout for HtmlWeb
                    wr.Timeout = 2000;
                    return true;
                }
            };
            services.AddSingleton(htmlWeb);

            services.AddSingleton<ISetting, Setting>();
            services.AddSingleton<SessionExtension>();
            services.AddSingleton<IOpenInBrowser, OpenInBrowser>();
            services.AddSingleton<IFolderManager, FolderManager>();
            services.AddSingleton<ISnackbarMessageQueue>(new SnackbarMessageQueue(TimeSpan.FromSeconds(2)));

            services.AddSingleton<SaveSessionViewModel>();
            services.AddSingleton<ImportSessionViewModel>();
            services.AddSingleton<ShareStringViewModel>();
            services.AddSingleton<PhaseStore>();

            services.AddSingleton<MainWindowViewModel>();
            services.AddSingleton<SearchViewModel>();
            services.AddSingleton<ClgViewModel>();
            services.AddSingleton<ChoosedViewModel>();
            services.AddSingleton<SchedulerViewModel>();
            services.AddSingleton<MainSchedulingViewModel>();
            services.AddSingleton<AccountViewModel>();
            services.AddSingleton<ProgramTreeViewModel>();
            services.AddSingleton<ResultViewModel>();
            services.AddSingleton<DbViewModel>();
            services.AddSingleton<DclTabViewModel>();

            return services.BuildServiceProvider();
        }
    }
}
