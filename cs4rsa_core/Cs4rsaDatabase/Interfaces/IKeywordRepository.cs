﻿using Cs4rsa.Cs4rsaDatabase.Models;

using System.Collections.Generic;
using System.Threading.Tasks;

namespace Cs4rsa.Cs4rsaDatabase.Interfaces
{
    public interface IKeywordRepository : IGenericRepository<Keyword>
    {
        Keyword GetKeyword(string discipline, string keyword1);

        /// <summary>
        /// Get Keyword bằng Course ID
        /// </summary>
        /// <param name="courseId">Course ID</param>
        Task<Keyword> GetKeyword(int courseId);

        /// <summary>
        /// Get Keyword bằng Subject Code
        /// </summary>
        /// <param name="subjectCode">Mã môn</param>
        /// <returns></returns>
        Keyword GetKeyword(string subjectCode);
        string GetColorWithSubjectCode(string subjectCode);
        string GetColor(int courseId);
        /// <summary>
        /// Lấy ra Color dựa theo mã môn.
        /// </summary>
        /// <param name="subjectCode">Mã môn</param>
        /// <returns>Color</returns>
        string GetColorBySubjectCode(string subjectCode);
        long Count(string discipline, string keyword1);

        /// <summary>
        /// Tìm tất cả các Keyword phù hợp với điều kiện.
        /// </summary>
        /// <param name="searchText">Văn bản tìm kiếm (không phân biệt hoa thường).</param>
        /// <param name="limit">Giới hạn số lượng.</param>
        /// <returns>Danh sách Keyword.</returns>
        List<Keyword> GetSearchResult(string searchText, int limit);

        /// <summary>
        /// Kiểm tra tồn tại bằng Subject Code
        /// </summary>
        /// <param name="subjectCode">Mã môn</param>
        /// <returns>Tồn tại trả về true, ngược lại trả về false.</returns>
        Task<bool> ExistBySubjectCodeAsync(string subjectCode);
        /// <summary>
        /// Lấy ra cache dựa theo Course ID.
        /// </summary>
        /// <remarks>
        /// Sử dụng RawSql.
        /// </remarks>
        /// <param name="courseId">Course ID</param>
        /// <returns>Cache</returns>
        string GetCache(string courseId);
        /// <summary>
        /// Lấy ra danh sách các Keyword dựa theo Discipline ID.
        /// </summary>
        /// <remarks>Sử dụng RawSql, không Early Load Discipline.</remarks>
        /// <param name="disciplineId">Discipline ID</param>
        /// <returns>Danh sách các Keyword.</returns>
        IEnumerable<Keyword> GetKeywordsByDisciplineId(int disciplineId);
    }
}
