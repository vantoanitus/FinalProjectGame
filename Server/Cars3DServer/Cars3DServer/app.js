/*
 *  Version 1.0.2
 */

var serverPort = process.env.OPENSHIFT_NODEJS_PORT || 4567;
var serverIpAddress = process.env.OPENSHIFT_NODEJS_PORT || '127.0.0.1';

const bearerToken = require('express-bearer-token');

var express = require('express');
var app = express();

app.use(express.static('app'));
app.use(bearerToken());

app.use(function (req, res) {
    res.send('Token ' + req.token);
});

var server = require('http').createServer(app);

var io = require('socket.io')(server);
var jwt = require('jsonwebtoken');  
var CarServer = require('./server/car_server.js');

app.get('/*', function (req, res, next) {

    //This is the current file they have requested
    var file = req.params[0];
    
    //Send the requesting client the file.
    res.sendfile(__dirname + '/' + file);

});

//io.use(function (socket, next) {
    
//    if (socket.handshake.query && socket.handshake.query.token) {
//        jwt.verify(socket.handshaker.query.token, 'SECRET_KEY', function (err, decoded) {
//            if (err)
//                return next(new Error('Authentication error'));
//            socket.decoded = decoded;
//            next();
//        });
//    }
//    next(new Error('Authentication error'));
//}).on('connection', CarServer.onClientConnect);
io.sockets.on('connection', CarServer.onClientConnect);

server.listen(serverPort, serverIpAddress, function () {
    console.log('Listening on ' + serverIpAddress + ', serverPort ' + serverPort);
});