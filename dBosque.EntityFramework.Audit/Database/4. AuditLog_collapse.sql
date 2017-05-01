CREATE FUNCTION [dbo].[collapse_Auditlog]
(
             @id int
)
RETURNS nvarchar(max)
AS
BEGIN

return 
	-- Combine all columns into one xml string,
	-- incorporating the signature of the previous entry to be able to signal row deletion
       CAST(STUFF((SELECT 
	 [Id]     
	,[Created]
	,[EntityFullName]
	,[Entity]
	,[EntityId]
	,[User] 
	,[OldValue]
	,[NewValue] 
	,[PropertyName]
	,[LogOperation] 
    , (select top 1 Signature
		from Auditlog 
		WHERE Id < @id
		order by Id desc)
		FROM Auditlog
		where Id = @id                     
		ORDER BY 1
		FOR XML path('x'), elements XSINIL)
		,1, 1, '') as nvarchar(max))
END
GO
