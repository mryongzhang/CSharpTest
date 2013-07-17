#MVC中的@Html.Partial和@Html.Action示例

@Html.Partial用于显示静态页面没问题。但是不会执行controller里面的action并返回view。

所以如果要在action里面有数据操作的话，需要用@Html.Action（“actionname”，“controllername”）就可以返回View了