DROP TABLE village CASCADE CONSTRAINTS;

CREATE TABLE village (
	building_id integer PRIMARY KEY,
	name VARCHAR2(30),
	visitors integer,
	building SDO_GEOMETRY
);

/*****************index********************/
delete from user_sdo_geom_metadata where table_name = 'VILLAGE';

INSERT INTO user_sdo_geom_metadata
(	TABLE_NAME,
	COLUMN_NAME,
	DIMINFO,
	SRID
)
VALUES
(	'village',
	'building',
	SDO_DIM_ARRAY( -- 20X20 grid
		SDO_DIM_ELEMENT('X', 0, 1000, 0.5),
		SDO_DIM_ELEMENT('Y', 0, 1000, 0.5)
	),
	NULL -- SRID
);

drop index village_idx;

CREATE INDEX village_idx
	ON village(building)
	INDEXTYPE IS MDSYS.SPATIAL_INDEX;


-- inserts
INSERT INTO village VALUES(1,'Kirche', 10,
	SDO_GEOMETRY(
		2003, 
		NULL,
		NULL,
		SDO_ELEM_INFO_ARRAY(1,1003,1), 
		SDO_ORDINATE_ARRAY(100,100, 140,100,140,160,130,160,130,180,110,180,110,160,100,160 )
	)
);

INSERT INTO village VALUES(2,'Hospiz', 100,
	SDO_GEOMETRY(
		2003, 
		NULL,
		NULL,
		SDO_ELEM_INFO_ARRAY(1,1003,1), 
		SDO_ORDINATE_ARRAY(200,200, 240,200,240,260,230,260,230,280,210,280,210,260,200,260 )
	)
);

INSERT INTO village VALUES(3,'Pentagramm', 200,
	SDO_GEOMETRY(
		2003, 
		NULL,
		NULL,
		SDO_ELEM_INFO_ARRAY(1,1003,1), 
		SDO_ORDINATE_ARRAY(3, 180, 500, 180, 98, 470, 250, 9, 400, 468)
	)
);

drop table visitors;
create table visitors(
	v_id integer,
    v_name varchar2(64),
	position sdo_geometry );

drop sequence visitors_seq;
create sequence visitors_seq;

INSERT INTO visitors VALUES (visitors_seq.nextval, 'Otto',
	SDO_GEOMETRY(
		2001,
		NULL,
		SDO_POINT_TYPE(120, 120, NULL),
		NULL,
		NULL
	)
);

select v.v_id, v.v_name, t.X, t.Y from visitors v, TABLE(SDO_UTIL.GETVERTICES(v.POSITION)) t;

select c.building_id, c.name, t.X, t.Y
FROM village c,
TABLE(SDO_UTIL.GETVERTICES(c.BUILDING)) t;

select village.name from village, visitors WHERE MDSYS.SDO_WITHIN_DISTANCE(village.building, visitors.position, 'distance = 10') = 'TRUE';

SELECT v.v_id, v.v_name
  FROM visitors v INNER JOIN village b on 1=1
  WHERE SDO_CONTAINS(b.building, v.position) = 'FALSE';