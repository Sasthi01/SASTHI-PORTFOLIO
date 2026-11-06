USE comp717_iot;
INSERT INTO Irrigation_Zone (`Zone name`, Location, Size)
SELECT 'Demo Zone', 'Test Area', 1 WHERE NOT EXISTS
(SELECT 1 FROM Irrigation_Zone WHERE `Zone name`='Demo Zone');
INSERT INTO Sensor (Irrigation_zoneID, Active, Batterylevel)
SELECT Irrigation_zoneID, 1, 100 FROM Irrigation_Zone z
WHERE `Zone name`='Demo Zone' AND NOT EXISTS
(SELECT 1 FROM Sensor s WHERE s.Irrigation_zoneID=z.Irrigation_zoneID) LIMIT 1;
SELECT SensorID, Irrigation_zoneID FROM Sensor;
