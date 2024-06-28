CREATE TABLE `logup` (
                         `id` INTEGER UNSIGNED NOT NULL AUTO_INCREMENT COMMENT '日志的ID序号',
                         `ProjectName` VARCHAR(255) NOT NULL COMMENT '项目名称',
                         `AppName` VARCHAR(255) NOT NULL COMMENT 'app名称',
                         `AppVersion` VARCHAR(64) NOT NULL COMMENT 'app版本',
                         `ResourceInfo` VARCHAR(64) NOT NULL COMMENT '资源包信息',
                         `ERRTime` TIMESTAMP NOT NULL COMMENT '错误发生时间',
                         `ERRType` VARCHAR(64) NOT NULL COMMENT '错误类型',
                         `ERRLog` MEDIUMTEXT NOT NULL COMMENT '错误日志',
                         `ERRStackTrace` MEDIUMTEXT NOT NULL COMMENT '错误堆栈信息',
                         PRIMARY KEY (`id`)
);