CREATE TABLE IF NOT EXISTS `__EFMigrationsHistory` (
    `MigrationId` varchar(150) CHARACTER SET utf8mb4 NOT NULL,
    `ProductVersion` varchar(32) CHARACTER SET utf8mb4 NOT NULL,
    CONSTRAINT `PK___EFMigrationsHistory` PRIMARY KEY (`MigrationId`)
) CHARACTER SET=utf8mb4;

START TRANSACTION;

ALTER DATABASE CHARACTER SET utf8mb4;

CREATE TABLE `Tags` (
    `Id` int NOT NULL AUTO_INCREMENT,
    `Value` longtext CHARACTER SET utf8mb4 NULL,
    CONSTRAINT `PK_Tags` PRIMARY KEY (`Id`)
) CHARACTER SET=utf8mb4;

CREATE TABLE `Users` (
    `Id` char(36) COLLATE ascii_general_ci NOT NULL,
    `FirstName` longtext CHARACTER SET utf8mb4 NULL,
    `LastName` longtext CHARACTER SET utf8mb4 NULL,
    `Email` longtext CHARACTER SET utf8mb4 NULL,
    CONSTRAINT `PK_Users` PRIMARY KEY (`Id`)
) CHARACTER SET=utf8mb4;

CREATE TABLE `WorkItemStates` (
    `Id` int NOT NULL AUTO_INCREMENT,
    `State` varchar(50) CHARACTER SET utf8mb4 NOT NULL,
    CONSTRAINT `PK_WorkItemStates` PRIMARY KEY (`Id`)
) CHARACTER SET=utf8mb4;

CREATE TABLE `Adresses` (
    `Id` char(36) COLLATE ascii_general_ci NOT NULL,
    `Country` longtext CHARACTER SET utf8mb4 NULL,
    `City` longtext CHARACTER SET utf8mb4 NULL,
    `Street` longtext CHARACTER SET utf8mb4 NULL,
    `PostaCode` longtext CHARACTER SET utf8mb4 NULL,
    `UserId` char(36) COLLATE ascii_general_ci NOT NULL,
    CONSTRAINT `PK_Adresses` PRIMARY KEY (`Id`),
    CONSTRAINT `FK_Adresses_Users_UserId` FOREIGN KEY (`UserId`) REFERENCES `Users` (`Id`) ON DELETE CASCADE
) CHARACTER SET=utf8mb4;

CREATE TABLE `WorkItems` (
    `Id` int NOT NULL AUTO_INCREMENT,
    `StateId` int NOT NULL,
    `Area` varchar(200) CHARACTER SET utf8mb4 NULL,
    `Iteration_Path` longtext CHARACTER SET utf8mb4 NULL,
    `Priority` int NOT NULL DEFAULT 1,
    `AuthorId` char(36) COLLATE ascii_general_ci NOT NULL,
    `Discriminator` varchar(8) CHARACTER SET utf8mb4 NOT NULL,
    `WorkItemStateId` int NULL,
    `StartDate` datetime(6) NULL,
    `EndDate` datetime(3) NULL,
    `Efford` decimal(5,2) NULL,
    `Activity` varchar(200) CHARACTER SET utf8mb4 NULL,
    `RemaningWork` decimal(14,2) NULL,
    CONSTRAINT `PK_WorkItems` PRIMARY KEY (`Id`),
    CONSTRAINT `FK_WorkItems_Users_AuthorId` FOREIGN KEY (`AuthorId`) REFERENCES `Users` (`Id`) ON DELETE CASCADE,
    CONSTRAINT `FK_WorkItems_WorkItemStates_StateId` FOREIGN KEY (`StateId`) REFERENCES `WorkItemStates` (`Id`) ON DELETE CASCADE,
    CONSTRAINT `FK_WorkItems_WorkItemStates_WorkItemStateId` FOREIGN KEY (`WorkItemStateId`) REFERENCES `WorkItemStates` (`Id`)
) CHARACTER SET=utf8mb4;

CREATE TABLE `Comments` (
    `Id` int NOT NULL AUTO_INCREMENT,
    `Message` longtext CHARACTER SET utf8mb4 NULL,
    `Author` longtext CHARACTER SET utf8mb4 NULL,
    `CreatedDate` datetime(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
    `UpdateDate` datetime(6) NULL,
    `WorkItemId` int NOT NULL,
    CONSTRAINT `PK_Comments` PRIMARY KEY (`Id`),
    CONSTRAINT `FK_Comments_WorkItems_WorkItemId` FOREIGN KEY (`WorkItemId`) REFERENCES `WorkItems` (`Id`) ON DELETE CASCADE
) CHARACTER SET=utf8mb4;

CREATE TABLE `WorkItemTag` (
    `WorkItemId` int NOT NULL,
    `TagId` int NOT NULL,
    `PublicationDate` datetime(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
    CONSTRAINT `PK_WorkItemTag` PRIMARY KEY (`TagId`, `WorkItemId`),
    CONSTRAINT `FK_WorkItemTag_Tags_TagId` FOREIGN KEY (`TagId`) REFERENCES `Tags` (`Id`) ON DELETE CASCADE,
    CONSTRAINT `FK_WorkItemTag_WorkItems_WorkItemId` FOREIGN KEY (`WorkItemId`) REFERENCES `WorkItems` (`Id`) ON DELETE CASCADE
) CHARACTER SET=utf8mb4;

CREATE UNIQUE INDEX `IX_Adresses_UserId` ON `Adresses` (`UserId`);

CREATE INDEX `IX_Comments_WorkItemId` ON `Comments` (`WorkItemId`);

CREATE INDEX `IX_WorkItems_AuthorId` ON `WorkItems` (`AuthorId`);

CREATE INDEX `IX_WorkItems_StateId` ON `WorkItems` (`StateId`);

CREATE INDEX `IX_WorkItems_WorkItemStateId` ON `WorkItems` (`WorkItemStateId`);

CREATE INDEX `IX_WorkItemTag_WorkItemId` ON `WorkItemTag` (`WorkItemId`);

INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
VALUES ('20240417184319_Init', '8.0.4');

COMMIT;

START TRANSACTION;

ALTER TABLE `WorkItemStates` MODIFY COLUMN `State` varchar(60) CHARACTER SET utf8mb4 NOT NULL;

INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
VALUES ('20240417185318_StateMaxLength', '8.0.4');

COMMIT;

START TRANSACTION;

ALTER TABLE `Users` ADD `FullName` longtext CHARACTER SET utf8mb4 NULL;

UPDATE Users SET FullName = CONCAT(FirstName, ' ', LastName)

ALTER TABLE `Users` DROP COLUMN `FirstName`;

ALTER TABLE `Users` DROP COLUMN `LastName`;

INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
VALUES ('20240417190922_UserFullName', '8.0.4');

COMMIT;

START TRANSACTION;

ALTER TABLE `Users` RENAME COLUMN `FullName` TO `LastName`;

ALTER TABLE `Users` ADD `FirstName` longtext CHARACTER SET utf8mb4 NULL;

INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
VALUES ('20240417195650_UserFullNameRevert', '8.0.4');

COMMIT;

START TRANSACTION;

ALTER TABLE `Users` ADD `FullName` longtext CHARACTER SET utf8mb4 NULL;

UPDATE Users SET FullName = CONCAT(FirstName, ' ', LastName)

ALTER TABLE `Users` DROP COLUMN `FirstName`;

ALTER TABLE `Users` DROP COLUMN `LastName`;

INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
VALUES ('20240417195743_UserFullName2', '8.0.4');

COMMIT;

START TRANSACTION;

ALTER TABLE `Comments` DROP COLUMN `Author`;

ALTER TABLE `Tags` ADD `Category` longtext CHARACTER SET utf8mb4 NULL;

ALTER TABLE `Comments` ADD `AuthorId` char(36) COLLATE ascii_general_ci NOT NULL DEFAULT '00000000-0000-0000-0000-000000000000';

CREATE INDEX `IX_Comments_AuthorId` ON `Comments` (`AuthorId`);

ALTER TABLE `Comments` ADD CONSTRAINT `FK_Comments_Users_AuthorId` FOREIGN KEY (`AuthorId`) REFERENCES `Users` (`Id`) ON DELETE CASCADE;

INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
VALUES ('20240418111836_CommentUserRelation', '8.0.4');

COMMIT;

