package logic

import (
	"RSJWYFameworkExceptionLogServer/common/ResponseBody"
	"RSJWYFameworkExceptionLogServer/logupapi/internal/svc"
	"RSJWYFameworkExceptionLogServer/logupapi/internal/types"
	"RSJWYFameworkExceptionLogServer/logupmodel/sql/model"
	"context"
	"strings"
	"time"

	"github.com/zeromicro/go-zero/core/logx"
)

type LogUpApiLogic struct {
	logx.Logger
	ctx    context.Context
	svcCtx *svc.ServiceContext
}

func NewLogUpApiLogic(ctx context.Context, svcCtx *svc.ServiceContext) *LogUpApiLogic {
	return &LogUpApiLogic{
		Logger: logx.WithContext(ctx),
		ctx:    ctx,
		svcCtx: svcCtx,
	}
}

func (l *LogUpApiLogic) LogUpApi(req *types.LogReq) (resp *types.LogResp, err error) {
	// todo: add your logic here and delete this line
	//参数合法性确认
	if len(strings.TrimSpace(req.ProjectName)) == 0 ||
		len(strings.TrimSpace(req.AppName)) == 0 ||
		len(strings.TrimSpace(req.AppVersion)) == 0 ||
		len(strings.TrimSpace(req.ERRLog)) == 0 ||
		len(strings.TrimSpace(req.ERRType)) == 0 ||
		len(strings.TrimSpace(req.ERRStackTrace)) == 0 ||
		len(strings.TrimSpace(req.ResourceInfo)) == 0 {
		return nil, ResponseBody.NewCodeError(400, "参数错误，没有")
	}
	var uptime = time.Time{}
	if req.ERRTime <= 0 {
		uptime = time.Now()
	} else {
		uptime = time.Unix(req.ERRTime, 0)
	}
	var db = model.Logup{
		ProjectName:   req.ProjectName,
		AppName:       req.AppName,
		AppVersion:    req.AppVersion,
		ResourceInfo:  req.ResourceInfo,
		ERRTime:       uptime,
		ERRType:       req.ERRType,
		ERRLog:        req.ERRLog,
		ERRStackTrace: req.ERRStackTrace,
	}
	_, err = l.svcCtx.LogupModel.Insert(l.ctx, &db)
	if err != nil {
		return nil, ResponseBody.NewCodeErrorInfo(500, "写入提交时发生异常", err)
	}
	return &types.LogResp{
		Status: "提交成功",
	}, nil
}
