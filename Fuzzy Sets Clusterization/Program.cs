using LinqToExcel;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Fuzzy_Sets_Clusterization
{
    class Program
    {
        const int NumberOfClusters = 2;
        const SkillSets skillSet = SkillSets.Hard;
        const int FuzzyCoeff = 2;
        const int EuclideanDistancePower = 2;
        const double TerminationCriteria = 0.1;

        static void Main(string[] args) 
        {
            var excel = new ExcelQueryFactory("../../data.xlsx");
            excel.ReadOnly = true;
            var data = excel.Worksheet<SkillData>().ToList();
            var points = TransformData(data, skillSet);
            
            var initialCentroids = GetClusterCentroids(numberOfClusters: NumberOfClusters, points: points);
            var initialMembershipMatrix = CalculateMembershipMatrix(points, initialCentroids);
            var result = Clusterize(points, initialMembershipMatrix, NumberOfClusters, TerminationCriteria);
        }

        private static List<List<double>> Clusterize(List<List<double>> points, List<List<double>> membershipMatrix, int numberOfClusters, double terminationCriteria)
        {
            var centroids = CalculateCentroids(membershipMatrix, points, numberOfClusters);
            var matrix = CalculateMembershipMatrix(points, centroids);
            if (CompareMembershipMatrices(matrix, membershipMatrix) < TerminationCriteria)
            {
                return matrix;
            }

            return Clusterize(points, matrix, numberOfClusters, terminationCriteria);
        }

        private static double CompareMembershipMatrices(List<List<double>> membershipMatrix, List<List<double>> membershipMatrixToCompare)
        {
            double maxDiff = 0;
            for (int i = 0; i < membershipMatrix.Count; i++)
            {
                for (int j = 0; j < membershipMatrix.First().Count; j++)
                {
                    var diff = Math.Abs(membershipMatrix[i][j] - membershipMatrixToCompare[i][j]);
                    if (diff > maxDiff)
                    {
                        maxDiff = diff;
                    }
                }
            }

            return maxDiff;
        }

        private static List<List<double>> CalculateCentroids(List<List<double>> membershipMatrix, List<List<double>> points, int numberOfClusters)
        {
            var centroids = new List<List<double>>();
            for (int i = 0; i < numberOfClusters; i++)
            {
                var centroid = new List<double>();
                for (int j = 0; j < points.First().Count; j++)
                {
                    double numerator = 0;
                    double denominator = 0;
                    for (int k = 0; k < points.Count; k++)
                    {
                        numerator += (Math.Pow(membershipMatrix[k][i], FuzzyCoeff) * points[k][j]);
                        denominator += Math.Pow(membershipMatrix[k][i], FuzzyCoeff);
                    }

                    centroid.Add(numerator / denominator);
                }

                centroids.Add(centroid);
            }

            return centroids;
        }

        private static List<List<double>> CalculateMembershipMatrix(List<List<double>> points, List<List<double>> centroids)
        {
            var matrix = new List<List<double>>();
            foreach (var point in points)
            {
                var pointMembershipValues = new List<double>();
                var distancesToCentroids = new List<double>(); 
                foreach (var centroid in centroids)
                {
                    distancesToCentroids.Add(CalculateEuclideanDistance(point, centroid, EuclideanDistancePower));
                }

                for (int i = 0; i < centroids.Count; i++)
                {
                    double denominator = 0;
                    foreach (var distanceToCentroids in distancesToCentroids)
                    {
                        denominator += Math.Pow(distancesToCentroids[i] / distanceToCentroids, 2.0 / (FuzzyCoeff - 1));
                    }

                    var membershipValue = 1.0 / denominator;
                    pointMembershipValues.Add(membershipValue);
                }

                matrix.Add(pointMembershipValues);
            }

            return matrix;
        }

        private static double CalculateEuclideanDistance(List<double> point, List<double> target, double power)
        {
            double distance = 0;
            for (int i = 0; i < point.Count; i++)
            {
                distance += Math.Pow(Math.Abs(point[i] - target[i]), power);
            }

            return Math.Pow(distance, 1.0 / power);
        }

        private static List<List<double>> TransformData(IList<SkillData> data, SkillSets skillSet)
        {
            var result = new List<List<double>>();
            var propertiesToTransform = typeof(SkillData).GetProperties().ToList();
            if (SkillSets.All != skillSet)
            {
                propertiesToTransform = propertiesToTransform.Where(p => p.Name.StartsWith(skillSet.ToString())).ToList();
            }

            foreach (var dataItem in data)
            {
                var point = new List<double>();
                foreach (var property in propertiesToTransform)
                {
                    var propertyValue = (double)dataItem.GetType().GetProperty(property.Name).GetValue(dataItem);
                    point.Add(propertyValue);
                }

                result.Add(point);
            }

            return result;
        }

        private static List<List<double>> GetClusterCentroids(int numberOfClusters, List<List<double>> points)
        {
            var rand = new Random();
            var centroids = new List<List<double>>();
            for (int i = 0; i < points.First().Count; i++)
            {
                var minValue = points.Min(x => x[i]);
                var maxValue = points.Max(x => x[i]);

                for (int j = 0; j < numberOfClusters; j++)
                {
                    if (i == 0)
                    {
                        centroids.Add(new List<double> { rand.NextDouble() * (maxValue - minValue) + minValue });
                    }
                    else
                    {
                        centroids[j].Add(rand.NextDouble() * (maxValue - minValue) + minValue);
                    }
                }
            }

            return centroids;
        }
    }
}
