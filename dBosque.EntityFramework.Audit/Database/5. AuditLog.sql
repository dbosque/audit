CREATE TABLE [dbo].[AuditLog](
	[Id] [bigint] IDENTITY(1,1) NOT NULL,
	[Created] [datetime] NOT NULL,
	[EntityFullName] [nvarchar](150) NULL,
	[Entity] [nvarchar](max) NULL,
	[EntityId] [nvarchar](150) NULL,
	[User] [nvarchar](50) NULL,
	[OldValue] [NVARCHAR](MAX) NULL,
	[NewValue] [nvarchar](MAX) NULL,
	[PropertyName] [nvarchar](MAX) NULL,
	[LogOperation] [nvarchar](50) NULL,
	-- Tampering protection via insert triggers
	[Signature] [varbinary](max) NULL,
    [Isvalid]  AS ([dbo].[HMAC_isvalid]([dbo].[collapse_Auditlog]([Id]),[Signature])),
 CONSTRAINT [PK_AuditLog] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO