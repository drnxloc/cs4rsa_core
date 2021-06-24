﻿using cs4rsa.Database;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace cs4rsa.Settings
{
    /// <summary>
    /// Lớp này tuy thao thác với database nhưng các method của nó
    /// không được wrap bởi các method của Cs4rsaDataView hay DataEdit.
    /// Nó chỉ là một phần của setting.
    /// </summary>
    public class SettingReader
    {
        public static string GetSetting(string key)
        {
            Cs4rsaDatabase cs4RsaDatabase = Cs4rsaDatabase.GetInstance();
            string sql = $@"select value from user_settings where key='{key}'";
            return cs4RsaDatabase.GetScalar<string>(sql);
        }

        public static bool IsExistsSetting(string key)
        {
            Cs4rsaDatabase cs4RsaDatabase = Cs4rsaDatabase.GetInstance();
            string sql = $"select count(*) from user_settings where key = '{key}'";
            long result = cs4RsaDatabase.GetScalar<long>(sql);
            if (result > 0)
                return true;
            return false;
        }
    }
}