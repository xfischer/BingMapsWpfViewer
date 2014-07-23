BingMapsWPFViewer
Sample Spatial Data

Last revision : 19 dec 2012

-- CONTENTS ---------------
Database format : SQL Server 2008 RTM (661)
Best to use with SQL Server Express
Unzip file and copy to DATA directory (usually C:\Program Files\Microsoft SQL Server\MSSQLSSERVER\MSSQL\DATA)
then attach database (delete log file in attach window)

-- CREDITS ----------------
IGN refers to IGN_GEOFLA data found here http://http://professionnels.ign.fr/geofla under (CC-BY 2.0) license
Full license terms here : http://ddata.over-blog.com/xxxyyy/4/37/99/26/licence/Licence-Ouverte-Open-Licence.pdf

NaturalEarth data found at http://www.naturalearthdata.com/ under (CC0) license
Full license terms here : http://creativecommons.org/about/cc0

-- DETAILS ---------------
All tables have spatial indexes with medium grid density

--- IGN_COMMUNE_4326
- Description: high detail layer for French zips
- Feature count: 36,610
- Geometry Type: Polygon
- Sql server type : geometry
- SRID: 4326 (WGS84)

--- IGN_DEPARTEMENT_4326
- Description: low detail layer for French departments
- Feature count: 96
- Geometry Type: Polygon
- Sql server type : geometry AND geography
- SRID: 4326 (WGS84)

--- IGN_DEPARTEMENT_2154
- Description: low detail layer for French departments
- Feature count: 96
- Geometry Type: Polygon
- Sql server type : geometry
- SRID: 2154 (RGF 93 - Lambert 93)

--- IGN_LIMITE_DEPARTEMENT_4326
- Description: low detail layer for French departments limits
- Feature count: 96
- Geometry Type: Polyline
- Sql server type : geometry
- SRID: 4326 (WGS84)

--- NE_10m_populated_places_simple_4326
- Description: NaturalEarth populated places 
- Feature count: 7322
- Geometry Type: Point
- Sql server type : geometry
- SRID: 4326 (WGS84)

--- NE_110m_admin_0_countries_4326
- Description: NaturalEarth world countries
- Feature count: 177
- Geometry Type: Polygon
- Sql server type : geometry
- SRID: 4326 (WGS84)
