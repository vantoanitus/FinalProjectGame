
var express = require('express');
var app = express();

var server = require('http').createServer(app);
var io = require('socket.io').listen(4567);

app.set('port', process.env.PORT || 3000);

// Danh sách các client đang kết nối hiện tại
var clients = [];


// Connect server
// Mỗi client chạy một hàm này riêng biệt
// Biến toàn cục được sử dụng chung
io.on("connection", function (socket) {
    var currentUser;
    
    // Khi người chơi mở game lên và chưa nhấn vào play
    socket.on("USER_CONNECT", function () {
        
        console.log("User connected");
        console.log(clients.length);

        // Load các người chơi hiện thời
        for (var i = 0; i < clients.length; i++) {
            socket.emit("USER_CONNECTED", { name: clients[i].name, position: clients[i].position, rotation: clients[i].rotation });
            console.log("User name " + clients[i].name + "is connected");
        }

    });
    
    // Người chơi bắt đầu PLAY
    socket.on("PLAY", function (data) {
        
        console.log(data);
        
        // Lưu lại thông tin người chơi (SocketClient)
        currentUser = {
            name : data.name,
            position: data.position,
            rotation: data.rotation,
            health: data.health,
            bullet: data.bullet,
            velocity: data.velocity
        }

        // Lưu lại thông tin người chơi
        clients.push(currentUser);
        
        // Gửi tin nhắn bắt đầu chơi đến client người chơi
        socket.emit("PLAY", currentUser);
        
        // Gửi broadcast đến các người chơi khác răng có người mới chơi
        socket.broadcast.emit("USER_CONNECTED", currentUser);
        //io.sockets.emit("USER_CONNECTED", currentUser);
    });

    socket.on("MOVE", function (data) {
        currentUser.position = data.position;
        
        // Gửi xuống client rằng người chơi đã di chuyển
        //socket.emit("MOVE", currentUser);
        
        // Gửi xuống client rằng người chơi khác răng người chơi này đã di chuyển
        socket.broadcast.emit("MOVE", currentUser);
        //io.sockets.emit("MOVE", currentUser);
        
        // Test Server
        //console.log(currentUser.name + "move to " + currentUser.position);
        //console.log(currentUser);
    });
    
    // Xoay trong game di chuyển có xoay
    socket.on("TURN", function (data) {
        currentUser.rotation = data.rotation;

        //socket.emit("TURN", currentUser);

        socket.broadcast.emit("TURN", currentUser);
        //io.sockets.emit("TURN", currentUser);

        //console.log(currentUser.name + " turn " + currentUser.rotation);
    });
    
    socket.on("CHANGE_BULLET", function (data) {
        
        currentUser.bullet = data.bullet;
        socket.emit("CHANGE_BULLET", currentUser);

    });
    
    socket.on("FIRE", function (data) {
        
        currentUser.velocity = data.velocity;
        io.sockets.emit("FIRE", currentUser);

    });
    
    socket.on("UPDATE_POSITION", function (data) {
        console.log("update_position");
        var data1 = {
            player_name : data.player_name,
            position : data.position
        };
        io.sockets.emit("UPDATE_POSITION", data);
    });
    
    socket.on("HEALTH_INCREASE", function (data) { 
        
        io.sockets.emit("HEALTH_INCREASE", data);
    });
    
    socket.on("HEALTH_DECREASE", function (data) {
        console.log("health_decrease");
        io.sockets.emit("HEALTH_DECREASE", data);
    });
    
    socket.on("UPDATE_HEALTH", function (data) {

        currentUser.health = data.health;
        io.sockets.emit("UPDATE_HEALTH", currentUser);
    });
    
    socket.on("USER_DIED", function (data) {

        // TODO


    });
    
    // Người chơi này disconnected
    socket.on("disconnect", function () {
        socket.broadcast.emit("USER_DISCONNECTED", currentUser);
        //io.sockets.emit("USER_DISCONNECTED", currentUser);
        for (var i = 0; i < clients.length; i++) {
            console.log("User " + clients[i].name + " disconnected");
            clients.splice(i, 1);
        }
    });


});

server.listen(app.get('port'), function () {
    console.log("------------- SERVER IS RUNNING -----------")
});
