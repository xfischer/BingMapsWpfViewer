using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BingMapsWPFViewer.Model.Features
{
	public sealed class FeatureFieldHelper
	{
		/// <summary>
		/// Produces a comma separated list of column names to query, with spatial column first
		/// </summary>
		/// <param name="fields"></param>
		/// <returns></returns>
		public static string BuildQueryColumnNamesPart(List<FeatureField> fields)
		{
			if (fields == null || fields.Count == 0)
				throw new ArgumentException("There are no fields specified for query.");

			// Retrieve first spatial field from query
			FeatureField spatialColField = FeatureFieldHelper.GetQuerySpatialField(fields);

			if (spatialColField == null)
				throw new ArgumentException("There are no spatial field specified for query.");


			// init colnames list
			List<string> colNames = new List<string>();
			colNames.Add("[" + spatialColField.FieldName + "]"); // add spatial column
			// get other columns
			List<FeatureField> attributeFields = FeatureFieldHelper.GetQueryAttributeFields(fields);
			colNames.AddRange(attributeFields.Select(f => "[" + f.FieldName + "]"));

			string colnamesFlat = string.Join(", ", colNames);

			return colnamesFlat;
		}

		public static string BuildQueryFilterPart(List<FeatureQueryFilter> filters)
		{
			if (filters == null || filters.Count == 0)
				return null;

			string filterString = string.Join(" AND ", filters
																										.Where(f => f!= null && f.QueryText != null)
																										.Select(f => f.QueryText)
																										.ToArray());

			if (string.IsNullOrWhiteSpace(filterString))
				return null;
			else
				return "AND " + filterString;
		}

		public static List<FeatureField> GetQueryAttributeFields(List<FeatureField> fields)
		{
			List<FeatureField> retList = fields
																		.Where(f => !f.Usage.HasFlag(enFeatureFieldUsage.Spatial) && f.Usage != enFeatureFieldUsage.None && !f.IsSpatial)
																		.OrderBy(f => f.Ordinal)
																		.ToList();

			return retList;
		}

		public static FeatureField GetQuerySpatialField(List<FeatureField> fields)
		{
			if (fields == null)
				return null;

			// Retrieve first spatial field from query
			return fields.Where(f => f.Usage.HasFlag(enFeatureFieldUsage.Spatial))
																						.FirstOrDefault();
		}

		public static int GetNextOrdinal(List<FeatureField> fields)
		{
			if (fields == null || fields.Count == 0)
				return 1;

			return fields.Max(f => f.Ordinal) + 1;
		}

		public static FeatureField GetColumnFilters(List<FeatureField> fields)
		{
			if (fields == null)
				return null;

			// Retrieve first spatial field from query
			return fields.Where(f => f.Usage.HasFlag(enFeatureFieldUsage.Spatial))
																						.FirstOrDefault();
		}

		public static List<FeatureField> CloneFeatureFields(List<FeatureField> fields)
		{
			List<FeatureField> retList = fields
																		.Select(f => f.Clone())
																		.ToList();

			return retList;
		}
		
	}
}
