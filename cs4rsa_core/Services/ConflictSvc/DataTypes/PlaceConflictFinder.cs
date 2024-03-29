﻿using Cs4rsa.Services.ConflictSvc.Utils;
using Cs4rsa.Services.SubjectCrawlerSvc.DataTypes;
using Cs4rsa.Services.SubjectCrawlerSvc.DataTypes.Enums;
using Cs4rsa.Services.SubjectCrawlerSvc.Models;

using System;
using System.Collections.Generic;
using System.Linq;

namespace Cs4rsa.Services.ConflictSvc.DataTypes
{

    /// <summary>
    /// Trình tìm kiếm xung đột hai giờ đầu và hai giờ cuối của hai ClassGroup
    /// </summary>
    public class PlaceConflictFinder : BaseConflict
    {
        private static readonly TimeSpan _timeDelta = new(0, 15, 0); // 15 minutes

        public PlaceConflictFinder(SchoolClassModel schoolClass1, SchoolClassModel schoolClass2) : base(schoolClass1, schoolClass2)
        {
        }

        public ConflictPlace GetPlaceConflict()
        {
            // Hai school class không có giao nhau về giai đoạn chắc chắn không xung đột.
            // Check phase
            PhaseIntersect phaseIntersect = PhaseManipulation.GetPhaseIntersect(_schoolClass1.StudyWeek, _schoolClass2.StudyWeek);
            if (phaseIntersect.Equals(PhaseIntersect.NullInstance))
            {
                return null;
            }

            // Kiểm tra hai school class có ngày học chung hay không, nếu không
            // chắc chắn không xảy ra xung đột.
            Schedule scheduleClassGroup1 = _schoolClass1.Schedule;
            Schedule scheduleClassGroup2 = _schoolClass2.Schedule;
            IEnumerable<DayOfWeek> intersectDayOfWeeks = ScheduleManipulation.GetIntersectDate(scheduleClassGroup1, scheduleClassGroup2);
            if (!intersectDayOfWeeks.Any())
            {
                return null;
            }

            // Kiểm tra hai school class có cùng một nơi học hay không. Nếu cùng thì chắc chắn không
            // có xung đột. Nhưng nếu lớn hơn 1 nơi thì có khả năng gây ra xung đột.
            IEnumerable<Place> schoolClass1Places = _schoolClass1.DayPlaceMetaData.GetPlaces();
            IEnumerable<Place> schoolClass2Places = _schoolClass2.DayPlaceMetaData.GetPlaces();
            IEnumerable<Place> dinstictPlaces = schoolClass1Places.Concat(schoolClass2Places).Distinct();
            if (dinstictPlaces.Count() < 2)
            {
                return null;
            }

            Dictionary<DayOfWeek, IEnumerable<PlaceAdjacent>> conflictPlaces = new();
            // Duyệt qua các thứ học để lấy ra các nơi học. Mỗi nơi học
            foreach (DayOfWeek dayOfWeek in intersectDayOfWeeks)
            {
                IEnumerable<PlaceMap> placeMap1 = _schoolClass1.SchoolClass.GetMetaDataMap().GetPlaceMapsAtDay(dayOfWeek);
                IEnumerable<PlaceMap> placeMap2 = _schoolClass2.SchoolClass.GetMetaDataMap().GetPlaceMapsAtDay(dayOfWeek);
                List<PlaceMap> placeMapsJoin = placeMap1.Concat(placeMap2).ToList();

                IEnumerable<Tuple<PlaceMap, PlaceMap>> placeMapPairs = PlaceMapManipulation.PairPlaceMaps(placeMapsJoin);
                IEnumerable<PlaceAdjacent> placeAdjacents = PlaceMapManipulation.GetPlaceAdjacents(
                    placeMapPairs,
                    _timeDelta
                );
                if (placeAdjacents.Any())
                    conflictPlaces.Add(dayOfWeek, placeAdjacents);
            }
            if (conflictPlaces.Count != 0)
            {
                return new ConflictPlace(conflictPlaces);
            }
            else
            {
                return null;
            }
        }
    }
}
