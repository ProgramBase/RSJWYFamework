package config

import (
	"github.com/zeromicro/go-zero/core/stores/cache"
	"github.com/zeromicro/go-zero/rest"
)

type Config struct {
	rest.RestConf
	//数据库配置
	Mysql struct {
		DataSource string
	}
	//Redis配置
	CacheRedis cache.CacheConf
}
