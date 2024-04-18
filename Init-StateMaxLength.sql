START TRANSACTION;

ALTER TABLE `WorkItemStates` MODIFY COLUMN `State` varchar(60) CHARACTER SET utf8mb4 NOT NULL;

INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
VALUES ('20240417185318_StateMaxLength', '8.0.4');

COMMIT;

