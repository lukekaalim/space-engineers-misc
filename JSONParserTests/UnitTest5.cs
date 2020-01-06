using System;
using System.Numerics;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace JSONParserTests
{
	static class SplineUtils
	{
		public static Vector3[] SampleSplineToLine(Vector3[] splinePoints, int precision = 3)
		{
			Vector3[] linePoints = new Vector3[precision];
			for (int i = 0; i < precision; i++)
				linePoints[i] = GetPointOnSpline(splinePoints, (float)i / (precision - 1));
			return linePoints;
		}

		public static float GetSplineLength(Vector3[] splinePoints, int precision = 3)
		{
			Vector3[] linePoints = SampleSplineToLine(splinePoints, precision);

			float distance = (
				Vector3.Distance(splinePoints[0], linePoints[0]) +
				Vector3.Distance(splinePoints[splinePoints.Length - 1], linePoints[linePoints.Length - 1])
			);
			for (int i = 0; i < linePoints.Length - 1; i++)
				distance += Vector3.Distance(linePoints[i], linePoints[i + 1]);

			return distance;
		}

		public static Vector3 GetPointOnSpline(Vector3[] points, float relativeDistance)
		{
			if (points.Length == 2)
				return Vector3.Lerp(points[0], points[1], relativeDistance);

			var newPoints = new Vector3[points.Length - 1];
			for (int i = 0; i < newPoints.Length; i++)
			{
				newPoints[i] = Vector3.Lerp(points[i], points[i + 1], relativeDistance);
			}
			return GetPointOnSpline(newPoints, relativeDistance);
		}

		public static Vector3 GetAbsolutePointOnSpline(Vector3[] points, float splineLength, float absoluteDistance)
		{
			if (absoluteDistance >= splineLength)
				return points[points.Length - 1];
			if (absoluteDistance == 0)
				return points[0];

			return GetPointOnSpline(points, absoluteDistance / splineLength);
		}
	}

	[TestClass]
	public class UnitTest5
	{
		[TestMethod]
		public void TestMethod1()
		{
		}
	}
}
