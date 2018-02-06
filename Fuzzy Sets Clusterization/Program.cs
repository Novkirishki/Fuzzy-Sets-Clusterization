using LinqToExcel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fuzzy_Sets_Clusterization
{
    class Program
    {
        static void Main(string[] args) 
        {
            var excel = new ExcelQueryFactory("../../data.xlsx");
            excel.ReadOnly = true;
            var data = excel.Worksheet<SkillData>();
        }
    }
}
