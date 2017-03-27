/*
 * CarServer
 */


var CarServer = {
    clients : {},
    games: {}
    // ....
};

var UUID = require('node-uuid');
var md5 = require('md5');
var hash_serv_token = md5(server_token);

// Add listener for socket server
CarServer.attachListeners = function(socket, events) {
    events.forEach(function (event) {
        socket.on(
            event, 
            CarServer['on' + event.charAt(0).toUpperCase() + event.slice(1)]
            .bind(null, socket.id)
        );
    });
}

// send broadcast đến những client đang chơi cùng nhau
CarServer.broadcastToGame = function (gameId, msg, arg) {
    CarServer.games[gameId].clients.forEach(function (client) {
        client.socket.emit(msg, arg);
    });
}

CarServer.broadcastToGameOrPlayer = function (clientId, msg, arg) {
    //if (CarServer.clients[clientId].game !== false) 
    CarServer.broadcastToGame(CarServer.clients[clientId].gameId, msg, arg);
    // else
    //CarServer.clients[clientId].socket.emit(msg, arg);
}

// OnClientConnect
CarServer.onClientConnect = function (socket) {
    
    console.log('\nIncoming connection from: ' 
        + socket.conn.remoteAddress 
        + ' id: ' + socket.id);
    
    socket.removeAllListeners();
    
    CarServer.clients[socket.id] = {
        id: socket.id,
        name: 'Unknown',
        car: '1',
        pos: '',
        rot: '',
        health: 100,
        score: 0,
        gameId: false,
        socket: socket,
        loaded: false,
        auth: false
        //...
    }

    CarServer.attachListeners(
        socket,
        [
            'authenticate',
            'StartGame',
            'disconnect',
            'Move',
            'Turn',
            'Fire',
            'TakeDamage',
            'DestroyShell'
            //...
        ]
    );
    
    CarServer.createOrJoinGame(CarServer.clients[socket.id]);
    //var data = { 'id': socket.id };
    //socket.emit("Connect", data);

    //setTimeout(function () {
    //        CarServer.clients[socket.id].socket.disconnect();
    //}, 3000);
}

// Bắt đầu game
CarServer.onStartGame = function (id, data) {
   
    console.log('\nonStartGame - name: ' + data['name']);
    
    var client = CarServer.clients[id];
    var dataSend = {};

    // Load game
    if (!client.loaded)
        CarServer.loadGame(client, client.gameId);
    
    client.loaded = true;
    client.health = 100;
    client.name = data['name'];
    client.car = data['car'];
    client.bullet = '1';
    client.pos = CarServer.getPosBegin();
    client.rot = { 'x' : '0', 'y' : '0', 'z' : '0', 'w' : '0' };
    
    // Thêm 
    CarServer.games[client.gameId].clients.push(client);

    // Start
    dataSend['id'] = client.id;
    dataSend['car'] = client.car;
    dataSend['pos'] = client.pos;
    dataSend['rot'] = client.rot;
    dataSend['name'] = client.name;
    
    client.socket.emit('Start', dataSend);
    
    // UserConnect
    CarServer.games[client.gameId].clients.forEach(function (client) {
        if (client.id != id)
            client.socket.emit('UserConnect', dataSend);
    });
} // onStartGame


// Emit to other client
CarServer.onMove = function (id, pos) {
    
    var gameId = CarServer.clients[id].gameId;
    var data = {};

    data['id'] = id;
    data['pos'] = pos;
    
    // Update position
    CarServer.clients[id].pos = pos;
    
    CarServer.games[gameId].clients.forEach(function (client) {
        if (client.id != id)
            client.socket.emit('Move', data);
    });
}

// Emit to other client
CarServer.onTurn = function (id, rot) {
    
    var gameId = CarServer.clients[id].gameId;
    var data = {};
    
    data['id'] = id;
    data['rot'] = rot;

    // Update rotation
    CarServer.clients[id].rot = rot;

    CarServer.games[gameId].clients.forEach(function (client) {
        if (client.id != id)
            client.socket.emit('Turn', data);
    });
}

// Emit to other client
CarServer.onFire = function (id, data) {
   
    var gameId = CarServer.clients[id].gameId;
    var dataSend = {};

    dataSend['id'] = id;
    dataSend['velocity'] = data['velocity'];
    dataSend['pos'] = data['pos'];
    dataSend['rot'] = data['rot'];

    CarServer.games[gameId].clients.forEach(function (client) {
        if (client.id != id)
            client.socket.emit('Fire', dataSend);
    });
}

// data: { 'id', 'damage' }
CarServer.onTakeDamage = function (id, data) {
    
    console.log("onTakeDamage");

    var clientId = data.id;
    var damage = data.damage;
    var client = CarServer.clients[clientId];
    var dataSend = {};

    client.health -= damage;
    console.log(client.health);

    // DEATH
    if (client.health <= 0) {
        dataSend['id'] = clientId;
        CarServer.broadcastToGameOrPlayer(id, 'Death', dataSend);

        var gameId = CarServer.clients[clientId].gameId;
        if (gameId !== false) {
            var game = CarServer.games[gameId];
            
            var index = game.clients.indexOf(client);
            console.log(index);
            if (index != -1) {
                game.clients.splice(index, 1);
            }
        }

    }
    else {
        dataSend['id'] = clientId;
        dataSend['health'] = client.health + '';
        CarServer.broadcastToGameOrPlayer(id, 'TakeDamage', dataSend);
    }
}

// 
CarServer.onDestroyShell = function (id, data) {
    
    var dataSend = {};
    dataSend['id'] = id;

    var gameId = CarServer.clients[id].gameId;
    CarServer.games[gameId].clients.forEach(function (client) {
        if (client.id != id)
            client.socket.emit('DestroyShell', dataSend);
    });
}

// Mất kết nối
CarServer.onDisconnect = function (id) {
    console.log('\n' + id + ' disconnected');

    var gameId = CarServer.clients[id].gameId;
    if (gameId !== false) {
        var game = CarServer.games[gameId];
        game.size--;
        
        var index = game.clients.indexOf(CarServer.clients[id]);
        
        if (index != -1) {
            game.clients.splice(index, 1);

            if (CarServer.games[gameId].size == 0) {
                CarServer.destroyGame(gameId);
            }
            else {
                var data = { 'id': id };
                CarServer.broadcastToGame(
                    gameId, 
                'UserDisconnect',
                data
                );
            }
        }
    }

    delete CarServer.clients[id];
} // onDisconnect


// Function

CarServer.createOrJoinGame = function (client) {
    // Gom nhóm client giới hạn số lượng: n = 10
    // Kiểm tra nhóm trống => ( Có == true )? join : create new game
    var gameId = CarServer.findGameStillEmpty();
    if (gameId !== false) {
        // Join
        client.gameId = gameId;
        CarServer.games[gameId].size++;

        console.log('\n' + client.id + ' vua tham gia game id: ' + gameId);
        console.log("size " + CarServer.games[gameId].size);
    } else {
        // Create new game
        game = {
            gameId: client.id,
            clients: [],
            items: [],
            bullets: [],
            size: 1
        };
        
        gameId = client.id;
        client.gameId = gameId;
        CarServer.games[gameId] = game;
        
        console.log('\n' + client.id + ' đa tao game moi.');
    }

    //CarServer.games[client.gameId].clients.push(client);
} // createOrJoinGame

// return: gameId
CarServer.findGameStillEmpty = function () {
    var listEmpty = [];
    
    for (var key in CarServer.games) {
        if (CarServer.games[key].size < 4) {
            listEmpty.push(game);
        }
    }
    
    if (listEmpty.length > 0) {
        var i = Math.floor((Math.random() * listEmpty.length) + 1);
        return listEmpty[i - 1].gameId;
    }

    return false;
} // findGameStillEmpty

CarServer.loadGame = function (client, gameId) {
   
    console.log("\nloadGame");
    
    var data = {};
    // Load user
    var users = {};
    var game = CarServer.games[gameId];
    if (game != null && game !== undefined) {
        game.clients.forEach(function (_client) {
            // user info
            var user = {};
            user['name'] = _client.name;
            user['health'] = _client.health + '';
            user['car'] = _client.car;
            user['pos'] = _client.pos;
            user['rot'] = _client.rot;
            users[_client.id] = user;
        });
    }
    data['users'] = users;
    
    // Load Item

    client.socket.emit('Load', data);
}


// huỷ game
CarServer.destroyGame = function (gameId) {

    CarServer.games[gameId].clients.forEach(function (client) {
        client.gameId = false;
    });

    delete CarServer.games[gameId];
} // destroyGame

// Lấy vị trí bắt đầu game
CarServer.getPosBegin = function () {
    return { 'x' : '0', 'y': '0', 'z' : '0' };
}

module.exports = CarServer;