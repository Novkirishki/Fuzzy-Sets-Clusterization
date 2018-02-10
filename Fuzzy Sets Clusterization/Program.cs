using LinqToExcel;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Fuzzy_Sets_Clusterization
{
    class Program
    {
        const SkillSets skillSet = SkillSets.Hard;
        const int FuzzyCoeff = 2;
        const int EuclideanDistancePower = 2;
        const double TerminationCriteria = 0.1;
        const int IterationsCount = 10;

        static void Main(string[] args) 
        {
            var excel = new ExcelQueryFactory("../../data.xlsx");
            excel.ReadOnly = true;
            var data = excel.Worksheet<SkillData>().ToList();
            var points = TransformData(data, skillSet);
            var numberOfClusters = DetermineOptimalNumberOfClusters(points);

            // algorithm
            var errorData = new List<double>();
            var matrices = new List<List<List<double>>>();
            for (int j = 0; j < IterationsCount; j++)
            {
                var centroids = GetClusterCentroids(numberOfClusters, points: points);
                var initialMembershipMatrix = CalculateMembershipMatrix(points, centroids);
                var membershipMatrix = Clusterize(points, initialMembershipMatrix, numberOfClusters, TerminationCriteria, out centroids);
                errorData.Add(CalculateError(membershipMatrix, centroids, points));
                matrices.Add(membershipMatrix);
            }

            var result = matrices[errorData.IndexOf(errorData.Min())];
        }

        private static int DetermineOptimalNumberOfClusters(List<List<double>> points)
        {
            var errorData = new List<double>();
            for (int i = 2; i < 16; i++)
            {
                double error = 0;
                for (int j = 0; j < IterationsCount; j++)
                {
                    var centroids = GetClusterCentroids(numberOfClusters: i, points: points);
                    var initialMembershipMatrix = CalculateMembershipMatrix(points, centroids);
                    var membershipMatrix = Clusterize(points, initialMembershipMatrix, i, TerminationCriteria, out centroids);
                    error += CalculateError(membershipMatrix, centroids, points);
                }

                errorData.Add(error / IterationsCount);
            }

            var initialErrorValue = errorData[0];
            var minimumErrorDiff = initialErrorValue / 10;
            for (int i = 1; i < errorData.Count; i++)
            {
                var diff = errorData[i - 1] - errorData[i];
                if (diff < minimumErrorDiff)
                {
                    return i + 1;
                }
            }

            return errorData.Count + 1;
        }

        private static double CalculateError(List<List<double>> membershipMatrix, List<List<double>> centroids, List<List<double>> points)
        {
            double error = 0;
            for (int i = 0; i < points.Count; i++)
            {
                for (int j = 0; j < centroids.Count; j++)
                {
                    error += (Math.Pow(membershipMatrix[i][j], FuzzyCoeff) * CalculateEuclideanDistance(points[i], centroids[j], 2));
                }
            }

            return error;
        }

        private static List<List<double>> Clusterize(List<List<double>> points, List<List<double>> membershipMatrix, int numberOfClusters, double terminationCriteria, out List<List<double>> centroids)
        {
            centroids = CalculateCentroids(membershipMatrix, points, numberOfClusters);
            var matrix = CalculateMembershipMatrix(points, centroids);
            if (CompareMembershipMatrices(matrix, membershipMatrix) < TerminationCriteria)
            {
                return matrix;
            }

            return Clusterize(points, matrix, numberOfClusters, terminationCriteria, out centroids);
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
