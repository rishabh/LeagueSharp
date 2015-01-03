﻿// Copyright 2014 - 2014 Esk0r
// Geometry.cs is part of Evade.
// 
// Evade is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// Evade is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with Evade. If not, see <http://www.gnu.org/licenses/>.

#region

using System;
using System.Collections.Generic;
using ClipperLib;
using LeagueSharp.Common;
using SharpDX;
using Color = System.Drawing.Color;
using Path = System.Collections.Generic.List<ClipperLib.IntPoint>;
using Paths = System.Collections.Generic.List<System.Collections.Generic.List<ClipperLib.IntPoint>>;
using GamePath = System.Collections.Generic.List<SharpDX.Vector2>;

#endregion

	/// <summary>
	/// Class that contains the geometry related methods.
	/// </summary>
	public static class Geometry
	{
		private const int CircleLineSegmentN = 9;
        
		public class Circle
		{
			public Vector2 Center;
			public float Radius;

			public Circle(Vector2 center, float radius)
			{
				Center = center;
				Radius = radius;
			}

			public Polygon ToPolygon(int offset = 0, float overrideWidth = -1)
			{
				var result = new Polygon();
				var outRadius = (overrideWidth > 0
					? overrideWidth
					: (offset + Radius) / (float)Math.Cos(2 * Math.PI / CircleLineSegmentN));

				for (var i = 1; i <= CircleLineSegmentN; i++)
				{
					var angle = i * 2 * Math.PI / CircleLineSegmentN;
					var point = new Vector2(
						Center.X + outRadius * (float)Math.Cos(angle), Center.Y + outRadius * (float)Math.Sin(angle));
					result.Add(point);
				}

				return result;
			}
		}

		public class Polygon
		{
			public List<Vector2> Points = new List<Vector2>();

			public void Add(Vector2 point)
			{
				Points.Add(point);
			}

		}
	}