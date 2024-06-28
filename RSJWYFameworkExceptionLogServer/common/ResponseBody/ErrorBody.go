package ResponseBody

import (
	"encoding/json"
	"fmt"
)

const defaultCode = 400

// NewCodeError 传入基本错误
func NewCodeError(code int, msg string) error {
	return &BaseResponse[any]{
		Code: code,
		Msg:  msg,
		Data: "",
	}
}

// NewCodeErrorImpossible 传入基本错误
func NewCodeErrorImpossible(code int, msg string) error {
	str := fmt.Sprintf("这是一个几乎不可能发生的错误，请联系管理员！,\n错误信息：%v", msg)
	return &BaseResponse[any]{
		Code: code,
		Msg:  str,
		Data: "",
	}
}

// NewCodeErrorInfo 传入带err的错误，将自行拼接
func NewCodeErrorInfo(code int, msg string, err error) error {

	_msgInfo := fmt.Sprintf("%v,错误信息：%v", msg, err)
	return &BaseResponse[any]{
		Code: code,
		Msg:  _msgInfo,
		Data: "",
	}
}

// 传入带err的错误，将自行拼接
func NewCodeErrorInfoImpossible(code int, msg string, err error) error {

	_msgInfo := fmt.Sprintf("这是一个几乎不可能发生的错误，请联系管理员！,\n错误信息：%v,错误信息：%v", msg, err)
	return &BaseResponse[any]{
		Code: code,
		Msg:  _msgInfo,
		Data: "",
	}
}

// 只传入错误信息，默认为400错误
func NewDefaultError(msg string) error {
	return NewCodeError(defaultCode, msg)
}

// 只传入错误信息，默认为400错误
func NewDefaultErrorImpossible(msg string) error {
	return NewCodeError(defaultCode, msg)
}

// 把错误信息转为json格式
func (e *BaseResponse[any]) Error() string {
	jsonData, _ := json.Marshal(e)
	jsonstr := string(jsonData)
	return jsonstr
}

// 把错误转为json可以处理的类
func (e *BaseResponse[any]) ErrorData() *BaseResponse[any] {
	return &BaseResponse[any]{
		Code: e.Code,
		Msg:  e.Msg,
		Data: e.Data,
	}
}
