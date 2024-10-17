1. Install mongodb
2.找到mongodb安装目录，找到mongod.exe所在文件夹，执行：mongod -dbpath d:\db，启动数据库
3.安装mongosh-2.2.1-win32-x64，运行mongosh.exe，然后安装数据库：
use ezy-smashers;
db.temp.insert({"key": "value"});
db.createUser({user: "root", pwd: "123456", roles:[{role: "readWrite", db: "ezy-smashers"}]})

4. 使用IntelliJ IDEA 打开server项目，Ctrl+Shift+Alt+A键，打开项目设置，选择jdk版本 1.8，编译，
Run file EzySmashers-startup/src/main/java/org/youngmonkeys/ezysmashers/ApplicationStartup.java

5. 使用Unity2021.3.26f1打开client-unity项目，
打开项目之前，确保ezy-smashers同级别目录下已存在项目Master-Server-Unity（ https://github.com/gcj2450/Master-Server-Unity ）