CREATE OR REPLACE TRIGGER TRG_CHECK_MAX_VISITORS 
FOR INSERT ON VISITORS
COMPOUND TRIGGER

--global
message VARCHAR2(150):='';
namestring VARCHAR2(30):=''; 
newV_Id VISITORS.V_ID%TYPE;
cnt INTEGER;

AFTER EACH ROW IS
BEGIN
newV_Id := :new.v_id;
END AFTER EACH ROW;


AFTER STATEMENT IS
BEGIN
    select COUNT(*) into CNT FROM village a CROSS JOIN visitors b WHERE SDO_CONTAINS(a.building, b.position)='TRUE' group by a.BUILDING_ID, a.name, a.visitors having a.visitors<count(a.building_id);
    IF CNT <> 0 THEN
        select a.name into namestring FROM village a CROSS JOIN visitors b WHERE SDO_CONTAINS(a.building, b.position)='TRUE' group by a.BUILDING_ID, a.name, a.visitors having a.visitors<count(a.building_id);
        message:=message||'Gebäude überfüllt: '||namestring;
        DELETE FROM visitors WHERE v_id = newV_Id;
        RAISE_APPLICATION_ERROR(-20000, message);
    END IF;
    
END AFTER STATEMENT;
END;