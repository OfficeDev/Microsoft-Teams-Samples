-- Idempotently add the user to the database
IF NOT EXISTS(SELECT * FROM sys.database_principals WHERE name='$(user_upn)')
    CREATE USER [$(user_upn)] FROM EXTERNAL PROVIDER;