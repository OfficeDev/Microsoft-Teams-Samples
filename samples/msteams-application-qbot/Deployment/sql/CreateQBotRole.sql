-- =========================================================================================
-- Create the qbot role and grant it privileges to the qbot database
-- =========================================================================================
-- Create the database role
IF NOT EXISTS(SELECT * FROM sys.database_principals WHERE name='$(qbot_user_role)')
	CREATE ROLE [$(qbot_user_role)] AUTHORIZATION [dbo]
GO

-- Grant access rights to a specific schema in the database
GRANT  
	DELETE, 
	INSERT, 
	SELECT,  
	UPDATE, 
	VIEW DEFINITION 
ON DATABASE::[$(databaseName)]
	TO qbot_user
GO


 
