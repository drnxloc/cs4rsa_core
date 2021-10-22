﻿using ConflictService.DataTypes;
using cs4rsa_core.ViewModelFunctions;
using SubjectCrawlService1.DataTypes;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace cs4rsa_core.Models
{
    /// <summary>
    /// Class này đại diện cho sự kết hợp một tập các ClassGroupModel khác nhau.
    /// </summary>
    public class CombinationModel
    {
        public List<SubjectModel> SubjecModels { get; set; }

        private List<ClassGroupModel> _classGroupModels;
        public List<ClassGroupModel> ClassGroupModels
        {
            get => _classGroupModels;
            set => _classGroupModels = value;
        }

        public bool HaveAClassGroupHaveNotSchedule { get; set; }

        public bool HaveAClassGroupHaveZeroEmptySeat { get; set; }

        private ObservableCollection<ConflictModel> _conflictModels = new ObservableCollection<ConflictModel>();
        public ObservableCollection<ConflictModel> ConflictModels
        {
            get { return _conflictModels; }
            set { _conflictModels = value; }
        }

        private ObservableCollection<PlaceConflictFinderModel> _placeConflictFinderModels = new ObservableCollection<PlaceConflictFinderModel>();
        public ObservableCollection<PlaceConflictFinderModel> PlaceConflictFinderModels
        {
            get { return _placeConflictFinderModels; }
            set { _placeConflictFinderModels = value; }
        }

        public bool CanShow { get; set; }

        public CombinationModel(List<SubjectModel> subjectModels, List<ClassGroupModel> classGroupModels)
        {
            SubjecModels = subjectModels;
            _classGroupModels = classGroupModels;
            HaveAClassGroupHaveNotSchedule = IsHaveAClassGroupHaveNotSchedule();
            HaveAClassGroupHaveZeroEmptySeat = IsHaveAClassGroupHaveZeroEmptySeat();
            CanShow = !HaveAClassGroupHaveZeroEmptySeat && !HaveAClassGroupHaveNotSchedule;
            UpdateConflict.UpdateConflictModelCollection(ref _conflictModels, ref _classGroupModels);
            UpdateConflict.UpdatePlaceConflictCollection(ref _placeConflictFinderModels, ref _classGroupModels);
        }

        private bool IsHaveAClassGroupHaveZeroEmptySeat()
        {
            foreach (ClassGroupModel classGroupModel in _classGroupModels)
            {
                if (classGroupModel.EmptySeat == 0)
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Kiểm tra xem combination này có chứa một class group mà class group đó không có schedule
        /// hay không. Nếu không có trả về true,ngược lại trả về false.
        /// </summary>
        /// <returns></returns>
        private bool IsHaveAClassGroupHaveNotSchedule()
        {
            foreach(ClassGroupModel classGroupModel in _classGroupModels)
            {
                if (!classGroupModel.HaveSchedule)
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Kiểm tra xem Bộ này có hợp lệ hay không. Một Bộ hợp lệ là khi từng ClassGroupModel
        /// bên trong thuộc một Subject, mà mỗi Subject là duy nhất.
        /// </summary>
        /// <returns></returns>
        public bool IsValid()
        {
            int count = 0;
            string subjecCode = "";
            foreach (ClassGroupModel classGroupModel in _classGroupModels)
            {
                if (!classGroupModel.SubjectCode.Equals(subjecCode))
                {
                    subjecCode = classGroupModel.SubjectCode;
                    count++;
                }
            }
            if (count == _classGroupModels.Count)
                return true;
            return false;
        }

        /// <summary>
        /// Kiểm tra xem bộ này có chứa xung đột về thời gian hay không.
        /// </summary>
        /// <returns></returns>
        public bool IsHaveTimeConflicts()
        {
            List<SchoolClass> schoolClasses = new List<SchoolClass>();
            foreach (ClassGroupModel classGroupModel in _classGroupModels)
            {
                schoolClasses.AddRange(classGroupModel.ClassGroup.SchoolClasses);
            }
            for (int i = 0; i < schoolClasses.Count; ++i)
            {
                for (int k = i + 1; k < schoolClasses.Count; ++k)
                {
                    Conflict conflict = new Conflict(schoolClasses[i], schoolClasses[k]);
                    ConflictTime conflictTime = conflict.GetConflictTime();
                    if (conflictTime != null)
                        return true;
                }
            }
            return false;
        }

        public bool IsHavePlaceConflicts()
        {
            List<SchoolClass> schoolClasses = new List<SchoolClass>();
            foreach (ClassGroupModel classGroupModel in _classGroupModels)
            {
                schoolClasses.AddRange(classGroupModel.ClassGroup.SchoolClasses);
            }
            for (int i = 0; i < schoolClasses.Count; ++i)
            {
                for (int k = i + 1; k < schoolClasses.Count; ++k)
                {
                    PlaceConflictFinder conflict = new PlaceConflictFinder(schoolClasses[i], schoolClasses[k]);
                    ConflictPlace conflictPlace = conflict.GetPlaceConflict();
                    if (conflictPlace != null)
                        return true;
                }
            }
            return false;
        }

        public Schedule GetSchedule()
        {
            List<Schedule> schedules = _classGroupModels.Select(item => item.ClassGroup.GetSchedule()).ToList();
            return ScheduleManipulation.MergeSchedule(schedules);
        }
    }
}