﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace cs4rsa.Models
{
    public class Keyword
    {
        public int KeywordId { get; set; }
        public string Keyword1 { get; set; }
        public int CourseId { get; set; }
        public string SubjectName { get; set; }
        public string Color { get; set; }

        public Discipline Discipline { get; set; }
    }
}