package handler

import (
	"net/http"

	"RSJWYFameworkExceptionLogServer/logupapi/internal/logic"
	"RSJWYFameworkExceptionLogServer/logupapi/internal/svc"
	"RSJWYFameworkExceptionLogServer/logupapi/internal/types"
	"github.com/zeromicro/go-zero/rest/httpx"
)

func LogUpApiHandler(svcCtx *svc.ServiceContext) http.HandlerFunc {
	return func(w http.ResponseWriter, r *http.Request) {
		var req types.LogReq
		if err := httpx.Parse(r, &req); err != nil {
			httpx.ErrorCtx(r.Context(), w, err)
			return
		}

		l := logic.NewLogUpApiLogic(r.Context(), svcCtx)
		resp, err := l.LogUpApi(&req)
		if err != nil {
			httpx.ErrorCtx(r.Context(), w, err)
		} else {
			httpx.OkJsonCtx(r.Context(), w, resp)
		}
	}
}
