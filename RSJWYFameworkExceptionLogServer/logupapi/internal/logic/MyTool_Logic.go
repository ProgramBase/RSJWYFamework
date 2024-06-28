package logic

import (
	"RSJWYFameworkExceptionLogServer/common/ResponseBody"
	"RSJWYFameworkExceptionLogServer/logupapi/internal/types"
	"strings"
)

// 确认用户名和密码传入不为零
func IsNameAndPWLenZero(name, pw string) bool {
	if len(strings.TrimSpace(name)) == 0 || len(strings.TrimSpace(pw)) == 0 {
		return true
	}
	return false
} // 确认输入的字符串不为0

func IsStringLenZero(str string) bool {
	return len(strings.TrimSpace(str)) == 0
}

// 创建请求成功的消息返回
func NewHttpOkJsonResponse(responseData any) ResponseBody.BaseResponse[any] {
	var response ResponseBody.BaseResponse[any]
	response.Code = 200
	response.Data = responseData
	//判断具体请求
	switch responseData.(type) {
	case *types.LogResp:
		response.Msg = "日志上报成功"
	default:
		response.Msg = "成功"
	}
	//返回
	return response
}
