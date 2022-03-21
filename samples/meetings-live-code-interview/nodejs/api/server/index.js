const express = require('express');
const PORT = process.env.PORT || 3978;
const app = express();
app.use(express.json());
app.use(express.urlencoded({
    extended: true
}));
const server = require('http').createServer(app);
const io = require('socket.io')(server,{cors:{origin: "*"}});

io.on("connection", (socket) => {
    socket.on("message", (message) => {
      io.emit("message", message)
    })
  });

server.listen(PORT, () => {
    console.log(`Server listening on http://localhost:${PORT}`);
});