CREATE FUNCTION [dbo].[HMAC_isvalid]
(
    @value nvarchar(max),
    @signature varbinary(max)
)
RETURNS bit
AS
BEGIN
	RETURN VerifySignedByAsymKey(AsymKey_Id( 'tamperproof' ), @value, @signature)
END
GO
