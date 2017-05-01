CREATE FUNCTION [dbo].[HMAC]
(
       @message NVARCHAR(MAX)
)
RETURNS VARBINARY(MAX)

AS
BEGIN

return SignByAsymKey( AsymKey_Id( 'tamperproof' ), @message, N'<very strong secrect>')

END
