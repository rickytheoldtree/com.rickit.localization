## 使用说明

- 可以自己继承`IDictConverter`实现新的字典读取工具；然后在`Config`中选择自己的Converter
- 如果需要打开MainEditor，请从Menu中选择`RicKit` -> `Localization` -> `打开主面板`
- 使用前，需要在`Localization/Config.asset`设置游戏将会支持的所有语言与该语言的ISO码
- 可以使用.csv导入语言与ISO码, 格式为一行一对
    -  `[语言],[ISO]`
- 翻译功能使用`Selenium`，默认驱动版本如果和你的浏览器不匹配可以在Config中自定义drivers的路径
    - 使用Edge浏览器的请看https://learn.microsoft.com/zh-cn/microsoft-edge/webdriver-chromium/?tabs=c-sharp
    - 使用Chrome浏览器的请看https://blog.csdn.net/Z_Lisa/article/details/133307151
    - 使用FireFox浏览器的请看https://blog.csdn.net/weixin_39339460/article/details/113773738
- 支持不同语言文本剥离