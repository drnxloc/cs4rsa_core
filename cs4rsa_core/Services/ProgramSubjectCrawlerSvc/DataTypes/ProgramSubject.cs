﻿using Cs4rsa.Services.ProgramSubjectCrawlerSvc.DataTypes.Enums;
using Cs4rsa.Services.ProgramSubjectCrawlerSvc.DataTypes.Interfaces;
using Cs4rsa.Services.SubjectCrawlerSvc.DataTypes.Enums;

using System;

namespace Cs4rsa.Services.ProgramSubjectCrawlerSvc.DataTypes
{
    /// <summary>
    /// Đại diện cho một Row của trong bảng chương trình học.
    /// Chứa thông tin cơ bản của Môn có trong chương trình.
    /// </summary>
    public class ProgramSubject : IProgramNode, IComparable
    {
        public string Id { get; set; }
        public string ChildOfNode { get; }
        public string SubjectCode { get; set; }
        public string SubjectName { get; set; }
        public int StudyUnit { get; }
        public StudyUnitType StudyUnitType { get; }
        public StudyState StudyState { get; }
        public string CourseId { get; }
        public string ParentNodeName { get; }

        public ProgramSubject(
            string id,
            string childOfNode,
            string subjectCode,
            string subjectName,
            int studyUnit,
            StudyUnitType studyUnitType,
            StudyState studyState,
            string courseId,
            string parrentNodeName
        )
        {
            Id = id;
            ChildOfNode = childOfNode;
            SubjectCode = subjectCode;
            SubjectName = subjectName;
            StudyUnit = studyUnit;
            StudyUnitType = studyUnitType;
            StudyState = studyState;
            CourseId = courseId;
            ParentNodeName = parrentNodeName;
        }

        public string GetChildOfNode()
        {
            return ChildOfNode;
        }

        public string GetIdNode()
        {
            return Id;
        }

        /// <summary>
        /// Kiểm tra xem môn học này đã học qua hay chưa.
        /// </summary>
        /// <returns>True nếu đã pass, ngược lại trả về false.</returns>
        public bool IsDone()
        {
            return StudyState == StudyState.Completed;
        }

        public int CompareTo(object obj)
        {
            ProgramSubject other = obj as ProgramSubject;
            return Id.CompareTo(other.Id);
        }

        public NodeType GetNodeType()
        {
            return NodeType.Subject;
        }
    }
}