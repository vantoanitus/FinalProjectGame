//
// V 1.1

var serverPort = process.env.OPENSHIFT_NODEJS_PORT || 4567;
var serverIpAddress = process.env.OPENSHIFT_NODEJS_PORT || '127.0.0.1';
var express = require('express');
var app = express();
var server = require('http').createServer(app);
var io = require('socket.io')(server);
var CarServer = require('./server/car_server.js');

io.sockets.on('connection', CarServer.onClientConnect);

server.listen(serverPort, serverIpAddress, function () {
    console.log('Listening on ' + serverIpAddress + ', serverPort ' + serverPort);
});