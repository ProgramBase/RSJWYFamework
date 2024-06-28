package ResponseBody

// BaseResponse is the base response struct.
type BaseResponse[T any] struct {
	// Code represents the business code, not the http status code.
	Code int `json:"Code" xml:"code"`
	// Msg represents the business message, if Code = BusinessCodeOK,
	// and Msg is empty, then the Msg will be set to BusinessMsgOk.
	Msg string `json:"Msg" xml:"msg"`
	// Data represents the business data.
	Data T `json:"Data,omitempty" xml:"data,omitempty"`
}
