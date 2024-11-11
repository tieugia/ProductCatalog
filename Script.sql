USE master
GO

IF NOT EXISTS (SELECT * FROM sys.databases WHERE name = 'ProductCatalog_DEV')
CREATE DATABASE ProductCatalog_DEV;
GO

USE ProductCatalog_DEV;
GO

--TABLES
CREATE TABLE Categories (
    Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    Name NVARCHAR(255) NOT NULL,
    Description NVARCHAR(1000),
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    UpdatedAt DATETIME2 NULL,
    RowVersion ROWVERSION
);

CREATE TABLE CategoryHierarchy (
    Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    ParentId UNIQUEIDENTIFIER NOT NULL,
    ChildId UNIQUEIDENTIFIER NOT NULL,
    RowVersion ROWVERSION,

    CONSTRAINT PK_CategoryHierarchy UNIQUE (ParentId, ChildId),
    CONSTRAINT FK_CategoryHierarchy_Parent FOREIGN KEY (ParentId)
        REFERENCES Categories(Id) ON DELETE CASCADE,
    CONSTRAINT FK_CategoryHierarchy_Child FOREIGN KEY (ChildId)
        REFERENCES Categories(Id)
);

CREATE TABLE Products (
    Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    Name NVARCHAR(255) NOT NULL,
    Description NVARCHAR(1000) NULL,
    Price DECIMAL(18, 2) NOT NULL,
    InventoryLevel INT NOT NULL,
    CategoryId UNIQUEIDENTIFIER NOT NULL,
    ImageUrl NVARCHAR(1000) NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    UpdatedAt DATETIME2 NULL,
    RowVersion ROWVERSION,

    CONSTRAINT FK_Products_Category FOREIGN KEY (CategoryId)
        REFERENCES Categories(Id) ON DELETE CASCADE
);

--INDEXES
CREATE INDEX IX_Product_Category ON Products (CategoryId);
CREATE INDEX IX_CategoryHierarchy_ParentId ON CategoryHierarchy (ParentId);
CREATE INDEX IX_CategoryHierarchy_ChildId ON CategoryHierarchy (ChildId);

GO

CREATE TRIGGER TR_CategoryHierarchy_Delete
ON CategoryHierarchy
AFTER DELETE
AS
BEGIN
    CREATE TABLE #ToDelete (ChildId UNIQUEIDENTIFIER);

    INSERT INTO #ToDelete (ChildId)
    SELECT ChildId
    FROM CategoryHierarchy
    WHERE ParentId IN (SELECT ChildId FROM DELETED);

    WHILE EXISTS (
        SELECT 1
        FROM CategoryHierarchy ch
        INNER JOIN #ToDelete td ON ch.ParentId = td.ChildId
    )
    BEGIN
        INSERT INTO #ToDelete (ChildId)
        SELECT ch.ChildId
        FROM CategoryHierarchy ch
        INNER JOIN #ToDelete td ON ch.ParentId = td.ChildId
        WHERE ch.ChildId NOT IN (SELECT ChildId FROM #ToDelete);
    END;

    DELETE FROM CategoryHierarchy
    WHERE ChildId IN (SELECT ChildId FROM #ToDelete);

    DELETE FROM Categories
    WHERE Id IN (SELECT ChildId FROM #ToDelete);

    DROP TABLE #ToDelete;
END;
