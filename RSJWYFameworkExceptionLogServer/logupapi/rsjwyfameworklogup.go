package main

import (
	"RSJWYFameworkExceptionLogServer/common/ResponseBody"
	"RSJWYFameworkExceptionLogServer/logupapi/internal/logic"
	"context"
	"errors"
	"flag"
	"fmt"
	"github.com/zeromicro/go-zero/rest/httpx"
	"net/http"

	"RSJWYFameworkExceptionLogServer/logupapi/internal/config"
	"RSJWYFameworkExceptionLogServer/logupapi/internal/handler"
	"RSJWYFameworkExceptionLogServer/logupapi/internal/svc"

	"github.com/zeromicro/go-zero/core/conf"
	"github.com/zeromicro/go-zero/rest"
)

var configFile = flag.String("f", "etc/rsjwyfameworklogup.yaml", "the config file")

func main() {
	flag.Parse()

	var c config.Config
	conf.MustLoad(*configFile, &c)

	server := rest.MustNewServer(c.RestConf)
	UserApiCustomContent(&c)
	defer server.Stop()

	ctx := svc.NewServiceContext(c)
	handler.RegisterHandlers(server, ctx)

	fmt.Printf("Starting server at %s:%d...\n", c.Host, c.Port)
	server.Start()
}

// UserApiCustomContent 在自定义内容
func UserApiCustomContent(c *config.Config) {
	// 自定义错误
	httpx.SetErrorHandler(func(err error) (int, interface{}) {
		var e *ResponseBody.BaseResponse[any]
		switch {
		//处理请求体没问题时返回的错误
		case errors.As(err, &e):
			return http.StatusBadRequest, e.ErrorData()
		default:
			//处理请求体有错误时返回错误，
			//这种情况下，是请求体必传的参数没传全，发生错误，并对错误封装请求体返回
			_err := ResponseBody.NewCodeErrorInfo(400, "请求体错误", err)
			return http.StatusBadRequest, _err
		}
	})
	//自定义成功
	httpx.SetOkHandler(func(ctx context.Context, response any) any {
		return logic.NewHttpOkJsonResponse(response)
	})
	////自定义JWT错误
	//server = rest.MustNewServer(c.RestConf, rest.WithUnauthorizedCallback(func(w http.ResponseWriter, r *http.Request, err error) {
	//	// 自定义处理返回,针对于JWT
	//	_err := ResponseBody.NewCodeError(401, fmt.Sprintf("JWT鉴权失败！！不是非法就是令牌已过期，详细错误信息：%v", err.Error()))
	//	_errjson := common.StructToJson(_err)
	//	w.Header().Set("Content-Type", "application/json; charset=utf-8")
	//	http.Error(w, _errjson, http.StatusUnauthorized)
	//}))
	//return server

}
