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

