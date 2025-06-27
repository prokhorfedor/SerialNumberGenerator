# Overview

- A program that generates CSV-files containing Serial Numbers for newly created Work Orders.
- It requires some DB changes: specifically creation of a table called SERNUM with one field LSTSERN that will hold last generated serial number
- A script that can be used to create it:

  `CREATE TABLE [dbo].[SERNUM](
  	[LSTSERN] [nvarchar](30) NOT NULL
  ) ON [PRIMARY]`

- Before running it you'll have to put SQL connection string to **appsettings.json** file
