# Overview

- A program that generates CSV-files containing Serial Numbers for newly created Work Orders.
- It requires some DB changes: specifically creation of a table called SERNUM with one field LSTSERN that will hold last generated serial number
- A script that can be used to create it:

  `CREATE TABLE [dbo].[SERNUM](
  	[LSTSERN] [nvarchar](30) NOT NULL
  ) ON [PRIMARY]`

- Before running it you'll have to put SQL connection string to **appsettings.json** file
- In addition you can add a file path where you want to save your file in **appsetting.json** file. **Default is Desktop**. Also you can **override** it by passing it into the **endpoint**.


## Desktop App

In the last update I've addeed a desktop app that you may find in `./Tool/SerialNumberGeneratorApp.exe`
There is also an **appsetting.json** file where you can put settings like connection string and file path just like for API.
