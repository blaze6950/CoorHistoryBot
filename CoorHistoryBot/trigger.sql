CREATE TRIGGER Complaints_Inserting_Trigger AFTER INSERT
ON Complaints
AS
BEGIN
	DECLARE @num_of_complaints int;
	DECLARE @id_address int;

	SELECT @id_address = Id_Address FROM inserted

	SELECT @num_of_complaints = COUNT(*)
	FROM Complaints
	WHERE Id_Address = @id_address

	if @num_of_complaints >= 5
		BEGIN TRANSACTION MoveAddressToModerationList
			DELETE FROM Complaints WHERE Id_Address = @id_address
			INSERT INTO ModAddresses VALUES(
				SELECT Latitude FROM Addresses WHERE Id = @id_address, 
				SELECT Longitude FROM Addresses WHERE Id = @id_address,
				SELECT Caption FROM Addresses WHERE Id = @id_address,
				SELECT User_Id FROM Addresses WHERE Id = @id_address)
			DELETE FROM Addresses WHERE Id = @id_address
		COMMIT TRANSACTION MoveAddressToModerationList;
END;