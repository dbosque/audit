CREATE TRIGGER [dbo].[tamper_trigger] 
ON  [dbo].[Auditlog]
AFTER INSERT 
AS  
BEGIN
       update s
        set s.Signature = dbo.HMAC(dbo.collapse_Auditlog(i.Id))
       from Auditlog s
       inner join INSERTED i on s.Id = i.Id
END

