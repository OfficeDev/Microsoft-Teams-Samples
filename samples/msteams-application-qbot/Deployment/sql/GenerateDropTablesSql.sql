WITH drop_constraints_cte (statement_string) AS
(
    SELECT 
        'ALTER TABLE [dbo].' +  QUOTENAME(OBJECT_NAME(parent_object_id)) 
        + ' DROP CONSTRAINT ' + QUOTENAME(name)
    FROM sys.foreign_keys
),
drop_tables_cte (statement_string) AS 
(
	SELECT 
		'DROP TABLE [dbo].[' + name + '];'
	FROM 
		sys.tables
),
all_statements (statement_string) AS
(
	SELECT statement_string FROM drop_constraints_cte UNION
	SELECT statement_string FROM drop_tables_cte
)
SELECT
	STRING_AGG(statement_string, ' ')
FROM 
	all_statements