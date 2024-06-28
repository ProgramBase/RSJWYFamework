package common

import (
	"crypto/sha256"
	"database/sql"
	"encoding/hex"
	"encoding/json"
	"reflect"
	"time"
)

// 把time转为SQL.NullTime
// t：当前时间
func ToNullTime(t time.Time) sql.NullTime {
	return sql.NullTime{
		Time:  t,
		Valid: !t.IsZero(),
	}
}

// 结构体转json
func StructToJson(coderr any) string {
	jsonData, _ := json.Marshal(coderr)
	jsonstr := string(jsonData)
	return jsonstr
}

// 获取具体的类型
func AnyTypeName(_any any) string {
	t := reflect.TypeOf(_any)
	return t.Name()
}

// 计算文本哈希值
func HashToString(str string) (hashString string) {
	hash := sha256.Sum256([]byte(str))
	hashString = hex.EncodeToString(hash[:])
	return hashString
}
