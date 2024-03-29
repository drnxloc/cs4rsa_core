﻿using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;

using Cs4rsa.BaseClasses;
using Cs4rsa.Constants;
using Cs4rsa.Cs4rsaDatabase.Interfaces;
using Cs4rsa.Cs4rsaDatabase.Models;
using Cs4rsa.Messages.Publishers.Dialogs;
using Cs4rsa.Services.ProgramSubjectCrawlerSvc.DataTypes;
using Cs4rsa.Services.ProgramSubjectCrawlerSvc.Interfaces;
using Cs4rsa.Services.StudentCrawlerSvc.Crawlers;
using Cs4rsa.Services.StudentCrawlerSvc.Crawlers.Interfaces;

using MaterialDesignThemes.Wpf;

using Newtonsoft.Json;

using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using System.Windows;

namespace Cs4rsa.Dialogs.Implements
{
    /// <summary>
    /// ViewModel của dialog nhập session id
    /// view model này là sử dụng chính DtuStudentInfoCrawler để cào thông tin sinh viên.
    /// Ngoài ra không có bất cứ viewmodel nào được sử dụng crawler này.
    /// 
    /// 1. Lấy Special String thông qua Session ID
    /// 2. Kiểm tra đã có trong DB thì ngừng tìm kiếm
    /// 3. Lấy thông tin sinh viên và lưu DB bằng Special String
    /// 4. Lấy chương trình học của sinh viên
    /// 5. Lấy chương trình học dự kiến theo mã ngành
    /// 
    /// Message:
    /// 1. SessionInputVmMsgs.ExitFindStudentMsg: Kết thúc việc tìm kiếm.
    ///  
    /// Update Date:
    /// 24/12/2022: Sửa document, update các xử lý cào chương trình học và chương
    ///             trình học dự kiến.
    /// 25/12/2022: Ngừng tìm kiếm khi đã có trong DB.
    /// 22/02/2023: Cập nhật quan hệ Model Curriculum và Student thành dạng quan hệ mềm
    ///             Add check khi curriculum id null.
    /// Author:
    /// toky0s
    /// </summary>
    public partial class SessionInputViewModel : ViewModelBase
    {
        [ObservableProperty]
        private string _sessionId;

        private readonly IDtuStudentInfoCrawler _dtuStudentInfoCrawler;
        private readonly IStudentPlanCrawler _studentPlanCrawler;
        private readonly IStudentProgramCrawler _studentProgramCrawler;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ISnackbarMessageQueue _snackbarMessageQueue;

        public SessionInputViewModel(
            IDtuStudentInfoCrawler dtuStudentInfoCrawler,
            IStudentPlanCrawler studentPlanCrawler,
            IStudentProgramCrawler studentProgramCrawler,
            IUnitOfWork unitOfWork,
            ISnackbarMessageQueue snackbarMessageQueue
        )
        {
            _dtuStudentInfoCrawler = dtuStudentInfoCrawler;
            _studentPlanCrawler = studentPlanCrawler;
            _studentProgramCrawler = studentProgramCrawler;
            _unitOfWork = unitOfWork;
            _snackbarMessageQueue = snackbarMessageQueue;
        }

        public async Task Find()
        {
            // 1. Lấy Special String
            SpecialStringCrawlerV2 specialStringCrawlerV2 = new();
            string specialStringV2 = await specialStringCrawlerV2.GetSpecialString(SessionId);
            if (specialStringV2 == null)
            {
                string message = "Hãy chắc chắn bạn đã đăng nhập vào MyDTU trước khi lấy UserSchedules ID, " +
                    "và đảm bảo lúc này server DTU không bảo trì. Hãy thử lại sau.";
                MessageBox.Show(message, "Thông báo", MessageBoxButton.OK, MessageBoxImage.Exclamation);
            }
            else
            {
                // 2. Ngừng tìm kiếm khi special string đã tồn tại trong DB
                if (_unitOfWork.Students.ExistsBySpecialString(specialStringV2))
                {
                    Student resultStudent = _unitOfWork.Students.GetBySpecialString(specialStringV2);
                    _snackbarMessageQueue.Enqueue($"{resultStudent.Name} đã tồn tại trong cơ sở dữ liệu");
                    return;
                }
                // 3. Lấy thông tin sinh viên
                Student student = await _dtuStudentInfoCrawler.Crawl(specialStringV2);
                if (student != null && student.CurriculumId.HasValue)
                {
                    // 4. Lấy chương trình học
                    Debug.WriteLine("4. Lấy chương trình học");
                    ProgramFolder[] programs = await _studentProgramCrawler.GetProgramFolders(specialStringV2, student.CurriculumId.Value);
                    string programFilePath = CredizText.PathProgramJsonFile(student.StudentId);

                    // 4.1 Lưu chương trình học vào file JSON
                    Debug.WriteLine("4.1 Lưu chương trình học vào file JSON");
                    JsonSerializer serializer = new();
                    using (StreamWriter sw = new(programFilePath))
                    using (JsonWriter writer = new JsonTextWriter(sw))
                    {
                        serializer.Serialize(writer, programs);
                    }

                    // 5. Lấy chương trình học dự kiến (nếu nó rỗng thì chả làm gì hết)
                    Debug.WriteLine("5. Lấy chương trình học dự kiến (nếu nó rỗng thì chả làm gì hết)");
                    List<PlanTable> planTables = await _studentPlanCrawler.GetPlanTables(student.CurriculumId.Value, SessionId);

                    if (planTables.Count > 0)
                    {
                        // 5.1 Lưu chương trình học dự kiến vào file JSON
                        Debug.WriteLine("5.1 Lưu chương trình học dự kiến vào file JSON");
                        using StreamWriter sw = new(CredizText.PathPlanJsonFile(student.CurriculumId.Value));
                        using JsonWriter writer = new JsonTextWriter(sw);
                        serializer.Serialize(writer, planTables);
                    }

                    Messenger.Send(new SessionInputVmMsgs.ExitFindStudentMsg(student));
                }
            }
        }
    }
}
