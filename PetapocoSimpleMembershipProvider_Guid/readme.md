# PetapocoSimpleMembershipProvider_Guid
MVC4中自定义MembershipProvider，实现Membership的权限控制。
本代码采用Petapoco微型ORM替换默认的EF访问数据库的方式，
从仅能支持SQLServer数据库，修改为理论上支持SQLServer、Oracle和MySql


测试程序的登录用户名：admin
密码：123456

------------
##在前一版的基础上，把各个表的ID类型从int自增类型改为guid类型（varchar(32)），用于oracle等数据库会更加方便。
