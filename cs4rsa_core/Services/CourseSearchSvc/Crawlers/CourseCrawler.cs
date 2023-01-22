﻿using Cs4rsa.Services.CourseSearchSvc.Crawlers.Interfaces;
using Cs4rsa.Services.CourseSearchSvc.DataTypes;
using Cs4rsa.Settings.Interfaces;

using HtmlAgilityPack;

using System.Collections.Generic;
using System.Linq;

namespace Cs4rsa.Services.CourseSearchSvc.Crawlers
{
    /// <summary>
    /// Đảm nhiệm việc cào thông tin học kỳ và năm học.
    /// </summary>
    public class CourseCrawler : ICourseCrawler
    {
        private readonly HtmlWeb _htmlWeb;
        private readonly ISetting _setting;

        public string CurrentYearValue { get; }
        public string CurrentYearInfo { get; }
        public string CurrentSemesterValue { get; }
        public string CurrentSemesterInfo { get; }
        public IEnumerable<CourseYear> CourseYears { get; }

        public CourseCrawler(HtmlWeb htmlWeb, ISetting setting)
        {
            _htmlWeb = htmlWeb;
            _setting = setting;

            try
            {
                string URL_YEAR_COMBOBOX = "http://courses.duytan.edu.vn/Modules/academicprogram/ajax/LoadNamHoc.aspx?namhocname=cboNamHoc2&id=2";
                HtmlDocument document = htmlWeb.Load(URL_YEAR_COMBOBOX);
                CourseYears = GetCourseYears(document);

                CurrentYearValue = GetCurrentValue(document);
                CurrentYearInfo = GetCurrentInfo(document);

                string URL_SEMESTER_COMBOBOX = $"http://courses.duytan.edu.vn/Modules/academicprogram/ajax/LoadHocKy.aspx?hockyname=cboHocKy1&namhoc={CurrentYearValue}";
                document = htmlWeb.Load(URL_SEMESTER_COMBOBOX);
                CurrentSemesterValue = GetCurrentValue(document);
                CurrentSemesterInfo = GetCurrentInfo(document);

                setting.CurrentSetting.CurrentYearValue = CurrentYearValue;
                setting.CurrentSetting.CurrentSemesterValue = CurrentSemesterValue;
                setting.CurrentSetting.CurrentYear = CurrentYearInfo;
                setting.CurrentSetting.CurrentSemester = CurrentSemesterInfo;
                setting.Save();
            }
            catch
            {
                CurrentYearInfo = setting.CurrentSetting.CurrentYear;
                CurrentSemesterInfo = setting.CurrentSetting.CurrentSemester;
                CurrentYearValue = setting.CurrentSetting.CurrentYearValue;
                CurrentSemesterValue = setting.CurrentSetting.CurrentSemesterValue;
            }
        }

        private static string GetCurrentValue(HtmlDocument document)
        {
            IEnumerable<HtmlNode> optionElements = document.DocumentNode.Descendants().Where(node => node.Name == "option");
            return optionElements.Last().Attributes["value"].Value;
        }

        private static string GetCurrentInfo(HtmlDocument document)
        {
            IEnumerable<HtmlNode> optionElements = document.DocumentNode.Descendants().Where(node => node.Name == "option");
            return optionElements.Last().InnerText.Trim();
        }

        private IEnumerable<CourseYear> GetCourseYears(HtmlDocument document)
        {
            IEnumerable<HtmlNode> optionElements = document.DocumentNode
                .Descendants()
                .Where(node => node.Name == "option" && node.Attributes["value"].Value != "0");
            foreach (HtmlNode node in optionElements)
            {
                string name = node.InnerText.Trim();
                string value = node.Attributes["value"].Value;
                IEnumerable<CourseSemester> courseSemesters = GetCourseSemesters(value);
                CourseYear courseYear = new() { Name = name, Value = value, CourseSemesters = courseSemesters };
                yield return courseYear;
            }
        }

        private IEnumerable<CourseSemester> GetCourseSemesters(string yearValue)
        {
            string urlTemplate = "http://courses.duytan.edu.vn/Modules/academicprogram/ajax/LoadHocKy.aspx?hockyname=cboHocKy1&namhoc={0}";
            string url = string.Format(urlTemplate, yearValue);
            HtmlDocument document = _htmlWeb.Load(url);

            IEnumerable<HtmlNode> optionElements = document.DocumentNode.Descendants()
                .Where(node => node.Name == "option" && node.Attributes["value"].Value != "0");
            foreach (HtmlNode node in optionElements)
            {
                string name = node.InnerText.Trim();
                string value = node.Attributes["value"].Value;
                CourseSemester courseSemester = new() { Name = name, Value = value };
                yield return courseSemester;
            }
        }

        public string GetCurrentSemesterValue()
        {
            return CurrentSemesterValue;
        }

        public string GetCurrentSemesterInfo()
        {
            return CurrentSemesterInfo;
        }

        public string GetCurrentYearValue()
        {
            return CurrentYearValue;
        }

        public string GetCurrentYearInfo()
        {
            return CurrentYearInfo;
        }
    }

}
