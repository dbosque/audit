
CREATE TABLE [dbo].[company](
	[com_id] [int] IDENTITY(1,1) NOT NULL,
	[com_code] [varchar](50) NULL,	
	[com_name] [varchar](50) NOT NULL,
	[com_address] [varchar](50) NULL,
	[com_zipcode] [varchar](10) NULL,
	[com_city] [varchar](50) NULL,
	[com_phonenumber] [varchar](20) NULL,	
 CONSTRAINT [PK_company] PRIMARY KEY CLUSTERED 
(
	[com_id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 90) ON [PRIMARY],
 CONSTRAINT [uc_com_code] UNIQUE NONCLUSTERED 
(
	[com_code] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 90) ON [PRIMARY]
) ON [PRIMARY]

GO