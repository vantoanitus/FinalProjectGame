var serverPort = process.env.OPENSHIFT_NODEJS_PORT || 4567;
var serverIpAddress = process.env.OPENSHIFT_NODEJS_PORT || '192.168.14.82';

var express = require('express');
var app = express();

var server = require('http').createServer(app);
var CarServer = require('./server/car_server.js');

var io = require('socket.io')(server);
io.sockets.on('connection', CarServer.onClientConnect);

server.listen(serverPort, serverIpAddress, function () {
    console.log('Listening on ' + serverIpAddress + ', serverPort ' + serverPort);
});