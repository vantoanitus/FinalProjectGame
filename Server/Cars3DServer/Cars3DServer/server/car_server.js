
var UUID = require('node-uuid');

var CarServer = {
    clients : {},
    games: {},
};

// Gửi broadcast thong qua gameId
CarServer.broadcastToGame = function (gameId, msg, arg) {
    CarServer.games[gameId].clients.forEach(function (client) {
        client.socket.emit(msg, arg);
    });
};
// Gửi broadcast thông qua clientId
CarServer.broadcastToGameOrPlayer = function (clientId, msg, arg) {
    //if (CarServer.clients[clientId].game !== false) 
    CarServer.broadcastToGame(CarServer.clients[clientId].gameId, msg, arg);
    // else
    //CarServer.clients[clientId].socket.emit(msg, arg);
};


// Thêm sự kiện
CarServer.attachListeners = function (socket, events) {
    events.forEach(function (event) {
        socket.on(
            event, 
            CarServer['on' + event.charAt(0).toUpperCase() + event.slice(1)]
            .bind(null, socket.id)
        );
    });
};

CarServer.onClientConnect = function (socket) {
    
    console.log('\nIncoming connection from: ' 
        + socket.conn.remoteAddress 
        + ' id: ' + socket.id);
    
    socket.removeAllListeners();
    
    
    CarServer.clients[socket.id] = {
        id: socket.id,
        name: 'Unknown',
        car_type: '1',
        pos: '',
        rot: '',
        health: 100,
        score: 0,
        gameId: false,
        socket: socket,
        loaded: false
        //...
    }
    
    CarServer.attachListeners(
        socket,
        [
            'StartGame',
            'Move',
            'Turn',
            'Fire',
            'TakeDamage',
            'DestroyShell',
            //...
            'disconnect'
        ]
    );
    
    CarServer.findGame(CarServer.clients[socket.id]);
};

// data: { 'name', 'car_type' }
CarServer.onStartGame = function (id, data) {
    
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
    //client.pos = CarServer.getPosBegin(client.gameId);
    client.pos = GetRandomPos(client.gameId);;
    client.rot = { 'x' : '0', 'y' : '0', 'z' : '0', 'w' : '0' };
    
    // Thêm 
    CarServer.games[client.gameId].clients.push(client);
    
    // Khởi tạo
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
}; // onStartGame
// pos: {'x','y','z'}
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
};
// rot: { 'x', 'y', 'z', 'w' }
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
};
// data: { 'velocity', 'pos', 'rot' }
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
};
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
};
CarServer.onDestroyShell = function (id, data) {
    
    var dataSend = {};
    dataSend['id'] = id;
    
    var gameId = CarServer.clients[id].gameId;
    CarServer.games[gameId].clients.forEach(function (client) {
        if (client.id != id)
            client.socket.emit('DestroyShell', dataSend);
    });
};
CarServer.onDisconnect = function (id) {
    
    console.log('\n' + id + ' disconnected');

    var gameId = CarServer.clients[id].gameId;
    if (gameId !== false) {
        
        var game = CarServer.games[gameId];
        game.size--;
        
        console.log('GameID: ' + gameId + ' - So nguoi choi: ' + game.size);

        var index = game.clients.indexOf(CarServer.clients[id]);
        if (index != -1) {
            // Remove client in game
            game.clients.splice(index, 1);
            if (CarServer.games[gameId].size == 0) {
                CarServer.destroyGame(gameId);
                console.log('GameID: ' + gameId + ' da bi huy');
            }
            else {
                // Send broadcast to other client
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
}; // onDisconnect



CarServer.findGame = function (client) {
    // Gom nhóm client giới hạn số lượng: n = 10
    // Kiểm tra nhóm trống => ( Có == true )? join : create new game
    var gameId = CarServer.findGameStillEmpty();
    if (gameId !== false) {
        // Join
        client.gameId = gameId;
        CarServer.games[gameId].size++;
        
        console.log('\n' + client.id + ' vua tham gia game id: ' + gameId);
        console.log('GameID: ' + gameId + ' - So nguoi choi: ' + CarServer.games[gameId].size);
    } else {
        
        var gameId = client.id;
        CarServer.games[gameId] = InitGame(gameId);
        client.gameId = gameId;
        
        // Auto random item every 5s
        CarServer.games[gameId].intervalId = setInterval(function () {
            CreateItem(gameId);
        }, 5000);

        console.log('\n' + gameId + ' đa tao game moi.');
    }

    //CarServer.games[client.gameId].clients.push(client);
}; // findGame
// Return: gameId
CarServer.findGameStillEmpty = function () {
    var listEmpty = [];
    
    for (var key in CarServer.games) {
        if (CarServer.games[key].size < 10) {
            listEmpty.push(CarServer.games[key]);
        }
    }
    
    if (listEmpty.length > 0) {
        var i = Math.floor((Math.random() * listEmpty.length) + 1);
        return listEmpty[i - 1].gameId;
    }
    
    return false;
}; // findGameStillEmpty
CarServer.loadGame = function (client, gameId) {
    
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
}; // loadGame
CarServer.destroyGame = function (gameId) {
    clearInterval(CarServer.games[gameId].intervalId);
    CarServer.games[gameId].clients.forEach(function (client) {
        client.gameId = false;
    });
    
    delete CarServer.games[gameId];
}; // destroyGame

// Lấy vị trí bắt đầu game
CarServer.getPosBegin = function (gameId) {
    
    // Chua co nguoi choi nao
    if (CarServer.games[gameId].clients.length == 0)
        return { 'x' : '0', 'y': '0', 'z' : '0' };
    
    var CheckPos = [];
    for (i = 0; i < 64; i++) {
        CheckPos.push(true);
    }
    
    // Vật cản
    CheckPos[0] = false;
    CheckPos[4] = false;
    CheckPos[13] = false;
    CheckPos[21] = false;
    CheckPos[40] = false;
    CheckPos[13] = false;
    CheckPos[21] = false;
    CheckPos[61] = false;
    CheckPos[60] = false;
    CheckPos[58] = false;
    CheckPos[57] = false;
    
    CarServer.games[gameId].clients.forEach(function (client) {
        var x = parseInt(client.pos.x) + 40;
        var z = parseInt(client.pos.z) + 40;
        
        var index = Math.floor(x / 10) + 8 * Math.floor(z / 10);
        CheckPos[index] = false;
    });
    
    
    var arr = [];
    for (i = 0; i < 64; i++) {
        if (CheckPos[i])
            arr.push(i);
    }
    
    var index = Math.floor(Math.random() * arr.length);
    var pos = {
        'x': (- (Math.floor(arr[index] / 8) * 10 - 35)) + '',
        'y': '0',
        'z': ((arr[index] % 8) * 10 - 35) + ''
    };
    
    return pos;

}; // getPosBegin

InitGame = function (gameId) {
    
    var Maps = [];
    for (i = 0; i < 324; i++) {
        Maps.push(true);
    }
    
    // Vi tri khong the random
    Maps[1] = false;
    Maps[19] = false;
    Maps[28] = false;
    Maps[66] = false;
    Maps[84] = false;
    Maps[102] = false;
    Maps[120] = false;
    Maps[182] = false;
    Maps[185] = false;
    Maps[210] = false;
    Maps[194] = false;
    Maps[267] = false;
    Maps[268] = false;
    Maps[281] = false;
    Maps[282] = false;
    Maps[300] = false;
    Maps[301] = false;
    Maps[297] = false;
    Maps[294] = false;
    Maps[292] = false;
    Maps[310] = false;
    Maps[314] = false;
    Maps[315] = false;
    Maps[318] = false;
    Maps[319] = false;

    // Create new game
    var game = {
        gameId: gameId,
        clients: [],
        items: [],
        size: 1,
        maps: Maps
    };

    return game;
};

// maps: bản đồ vị trí có thể đặt item hoặc không - Array
GetRandomPos = function (gameId) {
    
    var game = CarServer.games[gameId];
    var maps = game.maps;
    var clients = game.clients;
    var items = game.items;
    
    var newMaps = maps.slice();
    for (i = 0; i < clients.length; i++) {
        newMaps[GetMapsIndexFromPos(clients[i].pos)] = false;
    }
    
    for (i = 0; i < items.length; i++) {
        newMaps[GetMapsIndexFromPos(items[i].pos)] = false;
    }

    var arr = [];
    for (i = 0; i < newMaps.length; i++) {
        if (newMaps[i])
            arr.push(i);
    }
    
    var index = Math.floor(Math.random() * arr.length);
    var pos = {
        'x': ( - (Math.floor(arr[index] / 18) * 5) + 42) + '',
        'y': '0',
        'z': ((arr[index] % 18) * 5 - 42) + '',
    };
    return pos;
};

GetMapsIndexFromPos = function (pos) {
    var x = -parseInt(pos.x) + 42;
    var z = parseInt(pos.z) + 42;
    var index = Math.floor(x / 5) + Math.floor(z / 5) * 18;

    return Math.abs(index % 324);
};

CreateItem = function (gameId) {
    var pos = GetRandomPos(gameId);
    // 4 là số loại item
    var itemType = Math.floor(Math.random() * 4 - 1);

    var data = { pos: pos, item_type: itemType };
    CarServer.broadcastToGame(gameId, 'NewItem', data);

    console.log('New Item ' + itemType + ' pos: ' + pos);
}

module.exports = CarServer;