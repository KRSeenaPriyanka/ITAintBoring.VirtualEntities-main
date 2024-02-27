INSERT INTO dbo.account
  (account_name, account_start_date, account_address, account_type, account_create_timestamp, account_notes, is_active)
VALUES
  ('Ed''s Account',
   '5/1/2019',
   'Ed''s Address',
   'TEST',
   GETUTCDATE(),
   'This is a test account to model this data.',
   0);

   INSERT INTO dbo.account
  (account_name, account_start_date, account_address, account_type, account_create_timestamp, is_active)
VALUES
  ('Initech',
   '2/19/1999',
   '4120 Freidrich Ln.',
   'LIVE',
   GETUTCDATE(),
   1);

   ALTER TABLE dbo.account ADD CONSTRAINT DF_account_account_notes DEFAULT ('NONE PROVIDED') FOR account_notes;

   INSERT INTO dbo.account
  (account_name, account_start_date, account_address, account_type, account_create_timestamp, is_active)
SELECT
  'Dinosaur Corp',
  '1/1/2003',
  'The Triassic Time Period',
  'DEMO',
  GETUTCDATE(),
  1;

  INSERT INTO dbo.account
SELECT
  'The Pokemon Company',
  '4/23/1998',
  'Roppongi Hills Mori Tower 8F, Tokyo, Japan',
  'LIVE',
  GETUTCDATE(),
  'Very valuable.  They make all the Pokemon!',
   1;

   INSERT INTO dbo.account
SELECT
  'The Pokemon Company',
  '4/23/1998',
  'Roppongi Hills Mori Tower 8F, Tokyo, Japan',
  GETUTCDATE(),
  'Very valuable.  They make all the Pokemon!',
   0;

   INSERT INTO dbo.account
  (account_name, account_start_date, account_address, account_type, account_create_timestamp, account_notes, is_active)
SELECT
  'Microsoft' AS account_name,
  '4/4/1975' AS account_start_date,
  'One Microsoft Way in Redmond, Washington' AS account_address,
  'LIVE' AS account_type,
  GETUTCDATE() AS account_start_date,
  'They make SQL Server.  Thanks!' AS account_notes,
   1 AS is_active;

   INSERT INTO dbo.account
  (account_name, account_start_date, account_address, account_type, account_start_date, account_notes, is_active)
SELECT
  'Microsoft', -- account_name
  '4/4/1975', -- account_start_date
  'One Microsoft Way in Redmond, Washington', -- account_address
  'LIVE', -- account_type
  GETUTCDATE(), -- account_start_date
  'They make SQL Server.  Thanks!', -- account_notes
   1; -- is_active


   SELECT
  'Ed Pollack' AS developer_name,
  'SQL Server 2019 CTP1' AS database_engine_of_choice,
  'Pizza' AS food_choice,
  10 AS spice_level
INTO #developer_info;


SELECT
  tables.name AS TableName,
  columns.name AS ColumnName,
  columns.max_length AS ColumnLength,
  types.name AS TypeName
FROM TempDB.sys.tables
INNER JOIN TempDB.sys.columns
ON tables.object_id = columns.object_id
INNER JOIN TempDB.sys.types
ON types.user_type_id = columns.user_type_id
WHERE tables.name LIKE '#developer_info%';