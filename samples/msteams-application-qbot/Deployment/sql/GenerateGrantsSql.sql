WITH grants_cte (statement_string) AS 
(
	SELECT 
		'GRANT INSERT, SELECT, UPDATE,DELETE ON [dbo].[' + name + '] TO qbot_user;'
	FROM 
		sys.tables
	WHERE
		-- IGNORE EF table(s) prefixed with __
		PATINDEX('%[_][_]%', name) != 1
)
SELECT
	STRING_AGG(statement_string, ' ')
FROM 
	grants_cte