package model

import (
	"github.com/zeromicro/go-zero/core/stores/cache"
	"github.com/zeromicro/go-zero/core/stores/sqlx"
)

var _ LogupModel = (*customLogupModel)(nil)

type (
	// LogupModel is an interface to be customized, add more methods here,
	// and implement the added methods in customLogupModel.
	LogupModel interface {
		logupModel
	}

	customLogupModel struct {
		*defaultLogupModel
	}
)

// NewLogupModel returns a model for the database table.
func NewLogupModel(conn sqlx.SqlConn, c cache.CacheConf, opts ...cache.Option) LogupModel {
	return &customLogupModel{
		defaultLogupModel: newLogupModel(conn, c, opts...),
	}
}
