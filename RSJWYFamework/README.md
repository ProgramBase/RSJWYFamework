# RSJWYYFamework

#### 介绍
根据工作时用到的功能做一个整合，整体偏向于展厅方向，有较多依赖。参考了[HTFamework](https://gitee.com/SaiTingHu/HTFramework)、[GameFamewoork](http://https://gitee.com/jiangyin/GameFramework)，参考深思数盾（Virbox）SDK官方示例代码，融入到unity中（移除了x86的支持，仅保留了x64）。

#### 依赖
- [Unitask](https://github.com/Cysharp/UniTask)
- [YooAsset](https://github.com/tuyoogame/YooAsset)
- [HbridCLR](https://github.com/focus-creative-games/hybridclr_unity)
- [R3](https://github.com/Cysharp/R3.git)
- [NuGetForUnity](https://github.com/GlitchEnzo/NuGetForUnity)
- [Odin Inspector and Serializer](https://assetstore.unity.com/packages/tools/utilities/odin-inspector-and-serializer-89041)（不是强依赖目前，后续涉及到编辑器的情况下会使用）

#### 目前已完成以下内容
- Network（TCP服务客户端，UDP正在开发）
- Event
- ExceptionLogUp(日志回报，POST提交，[后端源码-Golang](https://gitee.com/RSJWY/rsjwyyfamework/tree/master/RSJWYFameworkExceptionLogServer))
- SenseshieldSDK（[VIRBOX加壳加盾](https://lm.virbox.com/?keyword={baidu-Virbox}&bd_vid=10350612851266965818)）

#### 着手开发
- Procedure 流程控制


#### 下载安装使用
- 下载[RSJWYFamework](https://gitee.com/RSJWY/rsjwyfamework/tree/master/RSJWYFamework)文件夹
- 深思数盾（Virbox）SDK每位开发者的SDK都不一样，请下载各自的SDK并拷贝到Assets/Plugins/senseshield/*下

