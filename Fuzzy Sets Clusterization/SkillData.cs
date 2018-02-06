using LinqToExcel.Attributes;

namespace Fuzzy_Sets_Clusterization
{
    internal class SkillData
    {
        [ExcelColumn("middleF1")]
        public double Middle1 { get; set; }

        [ExcelColumn("middleF2")]
        public double Middle2 { get; set; }

        [ExcelColumn("middleF3")]
        public double Middle3 { get; set; }

        [ExcelColumn("middleF4")]
        public double Middle4 { get; set; }

        [ExcelColumn("hardF1")]
        public double Hard1 { get; set; }

        [ExcelColumn("hardF2")]
        public double Hard2 { get; set; }

        [ExcelColumn("hardF3")]
        public double Hard3 { get; set; }

        [ExcelColumn("hardF4")]
        public double Hard4 { get; set; }

        [ExcelColumn("softF1")]
        public double Soft1 { get; set; }

        [ExcelColumn("softF2")]
        public double Soft2 { get; set; }

        [ExcelColumn("softF3")]
        public double Soft3 { get; set; }

        [ExcelColumn("softF4")]
        public double Soft4 { get; set; }

    }
}
