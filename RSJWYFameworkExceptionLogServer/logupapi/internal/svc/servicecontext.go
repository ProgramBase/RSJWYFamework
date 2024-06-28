package svc

import (
	"RSJWYFameworkExceptionLogServer/logupapi/internal/config"
	"RSJWYFameworkExceptionLogServer/logupmodel/sql/model"
	"github.com/zeromicro/go-zero/core/stores/sqlx"
)

type ServiceContext struct {
	Config     config.Config
	LogupModel model.LogupModel
}

func NewServiceContext(c config.Config) *ServiceContext {
	conn := sqlx.NewMysql(c.Mysql.DataSource)
	return &ServiceContext{
		Config:     c,
		LogupModel: model.NewLogupModel(conn, c.CacheRedis),
	}
}
